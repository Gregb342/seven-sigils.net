import type { BestScoreStore, QuizRepository } from '../../domain/ports'
import type { GameSettings, SessionSnapshot } from '../../domain/models/types'
import {
  HINT_COST,
  MAX_ROUNDS,
  MAX_TIMER_SECONDS,
  MIN_ROUNDS,
  MIN_TIMER_SECONDS,
  computePoints,
} from '../../domain/competitiveScoring'

const EASY_MAX_ROUNDS = 30
const HARD_MAX_ROUNDS = 40

const defaultSettings: GameSettings = {
  gameType: 'classic',
  mode: 'fixed',
  difficulty: 'easy',
  fixedRounds: 10,
  timerSeconds: 8,
}

export class QuizGameService {
  private readonly quizRepo: QuizRepository
  private readonly bestScoreStore: BestScoreStore
  private snapshot: SessionSnapshot
  private seenIds: Set<string>
  private sessionMaxRounds: number

  constructor(quizRepo: QuizRepository, bestScoreStore: BestScoreStore) {
    this.quizRepo = quizRepo
    this.bestScoreStore = bestScoreStore
    this.seenIds = new Set<string>()
    this.sessionMaxRounds = 0
    this.snapshot = this.idleSnapshot(defaultSettings)
  }

  getSnapshot(): SessionSnapshot {
    return this.snapshot
  }

  async start(settings: GameSettings): Promise<SessionSnapshot> {
    this.seenIds = new Set<string>()

    const maxRounds = settings.difficulty === 'easy' ? EASY_MAX_ROUNDS : HARD_MAX_ROUNDS
    this.sessionMaxRounds = maxRounds

    const isCompetitive = settings.gameType === 'competitive'

    const effectiveSettings: GameSettings = {
      ...settings,
      // Une compétition exige des runs comparables : pas de mode infini en compétitif.
      mode: isCompetitive ? 'fixed' : settings.mode,
      fixedRounds: isCompetitive
        ? Math.min(clamp(settings.fixedRounds, MIN_ROUNDS, MAX_ROUNDS), maxRounds)
        : settings.mode === 'fixed'
          ? Math.min(settings.fixedRounds, maxRounds)
          : settings.fixedRounds,
      timerSeconds: clamp(settings.timerSeconds, MIN_TIMER_SECONDS, MAX_TIMER_SECONDS),
    }

    const firstQuestion = await this.quizRepo.fetchQuestion(effectiveSettings.difficulty, [])
    this.seenIds.add(firstQuestion.blazon.id)

    this.snapshot = {
      ...this.idleSnapshot(effectiveSettings),
      status: 'running',
      roundIndex: 1,
      question: firstQuestion,
    }

    return this.snapshot
  }

  answer(option: string, elapsedMs = 0): SessionSnapshot {
    if (this.snapshot.status !== 'running' || this.snapshot.answerLocked || !this.snapshot.question) {
      return this.snapshot
    }

    const isCorrect = option === this.snapshot.question.correctOption
    const isCompetitive = this.snapshot.settings.gameType === 'competitive'
    const timerMs = this.snapshot.settings.timerSeconds * 1000

    let pointsEarned: number
    let rawPoints: number
    if (isCompetitive) {
      rawPoints = computePoints(elapsedMs, timerMs, isCorrect)
      // Malus indices : le score global ne descend jamais sous 0.
      const delta = rawPoints - this.snapshot.hintsUsed * HINT_COST
      pointsEarned = Math.max(delta, -this.snapshot.score)
    } else {
      rawPoints = isCorrect ? 1 : 0
      pointsEarned = rawPoints
    }

    this.snapshot = {
      ...this.snapshot,
      score: this.snapshot.score + pointsEarned,
      totalTimeMs: this.snapshot.totalTimeMs + (isCompetitive ? Math.min(elapsedMs, timerMs) : 0),
      streak: isCorrect ? this.snapshot.streak + 1 : 0,
      roundOutcomes: [...this.snapshot.roundOutcomes, { isCorrect, timedOut: false, rawPoints }],
      selectedAnswer: option,
      answerLocked: true,
      lastResult: {
        isCorrect,
        correctOption: this.snapshot.question.correctOption,
        pointsEarned,
        timedOut: false,
      },
    }

    return this.snapshot
  }

  /** Compétitif : le timer a expiré sans réponse — manche perdue, réponse révélée. */
  timeout(): SessionSnapshot {
    if (
      this.snapshot.status !== 'running' ||
      this.snapshot.answerLocked ||
      !this.snapshot.question ||
      this.snapshot.settings.gameType !== 'competitive'
    ) {
      return this.snapshot
    }

    const timerMs = this.snapshot.settings.timerSeconds * 1000
    // Les indices révélés coûtent même sur un timeout (sinon indice gratuit en laissant filer).
    const pointsEarned = Math.max(-this.snapshot.hintsUsed * HINT_COST, -this.snapshot.score)

    this.snapshot = {
      ...this.snapshot,
      score: this.snapshot.score + pointsEarned,
      totalTimeMs: this.snapshot.totalTimeMs + timerMs,
      streak: 0,
      roundOutcomes: [
        ...this.snapshot.roundOutcomes,
        { isCorrect: false, timedOut: true, rawPoints: 0 },
      ],
      selectedAnswer: null,
      answerLocked: true,
      lastResult: {
        isCorrect: false,
        correctOption: this.snapshot.question.correctOption,
        pointsEarned,
        timedOut: true,
      },
    }

    return this.snapshot
  }

  /** Compétitif : un indice révélé sur la manche courante (−1 pt à la résolution). */
  useHint(): SessionSnapshot {
    if (
      this.snapshot.status !== 'running' ||
      this.snapshot.answerLocked ||
      this.snapshot.settings.gameType !== 'competitive'
    ) {
      return this.snapshot
    }

    this.snapshot = {
      ...this.snapshot,
      hintsUsed: this.snapshot.hintsUsed + 1,
      totalHintsUsed: this.snapshot.totalHintsUsed + 1,
    }
    return this.snapshot
  }

  async nextRound(): Promise<SessionSnapshot> {
    if (this.snapshot.status !== 'running' || !this.snapshot.answerLocked) {
      return this.snapshot
    }

    const isFixedMode = this.snapshot.settings.mode === 'fixed'
    const hasReachedLimit = isFixedMode
      ? this.snapshot.roundIndex >= this.snapshot.settings.fixedRounds
      : this.snapshot.roundIndex >= this.sessionMaxRounds

    if (hasReachedLimit) {
      this.finish()
      return this.snapshot
    }

    let nextQuestion
    try {
      nextQuestion = await this.quizRepo.fetchQuestion(
        this.snapshot.settings.difficulty,
        [...this.seenIds],
      )
    } catch {
      this.finish()
      return this.snapshot
    }

    this.seenIds.add(nextQuestion.blazon.id)

    this.snapshot = {
      ...this.snapshot,
      roundIndex: this.snapshot.roundIndex + 1,
      question: nextQuestion,
      answerLocked: false,
      selectedAnswer: null,
      lastResult: null,
      hintsUsed: 0,
    }

    return this.snapshot
  }

  stop(): SessionSnapshot {
    if (this.snapshot.status === 'running') {
      this.finish()
    }
    return this.snapshot
  }

  goToMenu(): SessionSnapshot {
    this.seenIds.clear()
    this.sessionMaxRounds = 0
    this.snapshot = this.idleSnapshot(this.snapshot.settings)
    return this.snapshot
  }

  private finish(): void {
    // Le meilleur score local historique ne concerne que le mode classique :
    // les scores compétitifs vivent dans leur propre classement.
    let best = this.snapshot.bestScore
    if (this.snapshot.settings.gameType === 'classic') {
      best = Math.max(this.snapshot.score, this.snapshot.bestScore)
      if (best > this.snapshot.bestScore) {
        this.bestScoreStore.saveBestScore(best)
      }
    }
    this.snapshot = {
      ...this.snapshot,
      status: 'finished',
      bestScore: best,
    }
  }

  private idleSnapshot(settings: GameSettings): SessionSnapshot {
    return {
      status: 'idle',
      settings,
      score: 0,
      bestScore: this.bestScoreStore.getBestScore(),
      roundIndex: 0,
      question: null,
      answerLocked: false,
      selectedAnswer: null,
      lastResult: null,
      totalTimeMs: 0,
      hintsUsed: 0,
      totalHintsUsed: 0,
      streak: 0,
      roundOutcomes: [],
    }
  }
}

function clamp(value: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, Number.isFinite(value) ? value : min))
}
