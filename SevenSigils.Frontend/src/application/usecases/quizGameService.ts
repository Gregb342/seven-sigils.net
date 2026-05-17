import type { BestScoreStore, QuizRepository } from '../../domain/ports'
import type { GameSettings, SessionSnapshot } from '../../domain/models/types'

const EASY_MAX_ROUNDS = 30
const HARD_MAX_ROUNDS = 40

const defaultSettings: GameSettings = {
  mode: 'fixed',
  difficulty: 'easy',
  fixedRounds: 10,
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
    this.snapshot = {
      status: 'idle',
      settings: defaultSettings,
      score: 0,
      bestScore: this.bestScoreStore.getBestScore(),
      roundIndex: 0,
      question: null,
      answerLocked: false,
      selectedAnswer: null,
      lastResult: null,
    }
  }

  getSnapshot(): SessionSnapshot {
    return this.snapshot
  }

  async start(settings: GameSettings): Promise<SessionSnapshot> {
    this.seenIds = new Set<string>()

    const maxRounds = settings.difficulty === 'easy' ? EASY_MAX_ROUNDS : HARD_MAX_ROUNDS
    this.sessionMaxRounds = maxRounds

    const effectiveSettings: GameSettings = {
      ...settings,
      fixedRounds:
        settings.mode === 'fixed'
          ? Math.min(settings.fixedRounds, maxRounds)
          : settings.fixedRounds,
    }

    const firstQuestion = await this.quizRepo.fetchQuestion(effectiveSettings.difficulty, [])
    this.seenIds.add(firstQuestion.blazon.id)

    this.snapshot = {
      status: 'running',
      settings: effectiveSettings,
      score: 0,
      bestScore: this.bestScoreStore.getBestScore(),
      roundIndex: 1,
      question: firstQuestion,
      answerLocked: false,
      selectedAnswer: null,
      lastResult: null,
    }

    return this.snapshot
  }

  answer(option: string): SessionSnapshot {
    if (this.snapshot.status !== 'running' || this.snapshot.answerLocked || !this.snapshot.question) {
      return this.snapshot
    }

    const isCorrect = option === this.snapshot.question.correctOption
    const nextScore = isCorrect ? this.snapshot.score + 1 : this.snapshot.score

    this.snapshot = {
      ...this.snapshot,
      score: nextScore,
      selectedAnswer: option,
      answerLocked: true,
      lastResult: {
        isCorrect,
        correctOption: this.snapshot.question.correctOption,
      },
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
    this.snapshot = {
      status: 'idle',
      settings: this.snapshot.settings,
      score: 0,
      bestScore: this.bestScoreStore.getBestScore(),
      roundIndex: 0,
      question: null,
      answerLocked: false,
      selectedAnswer: null,
      lastResult: null,
    }
    return this.snapshot
  }

  private finish(): void {
    const best = Math.max(this.snapshot.score, this.snapshot.bestScore)
    if (best > this.snapshot.bestScore) {
      this.bestScoreStore.saveBestScore(best)
    }
    this.snapshot = {
      ...this.snapshot,
      status: 'finished',
      bestScore: best,
    }
  }
}
