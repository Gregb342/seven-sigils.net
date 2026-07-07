import { describe, expect, it } from 'vitest'
import { QuizGameService } from './quizGameService'
import type { BestScoreStore, QuizRepository } from '../../domain/ports'
import type { GameSettings, Question } from '../../domain/models/types'

function makeQuestion(id: string): Question {
  return {
    blazon: {
      id,
      familySlug: id,
      familyLabel: id.toUpperCase(),
      imageUrl: `/blazons/${id}.png`,
      housePageUrl: 'https://example.test',
      hints: [],
      attribution: {
        sourcePageUrl: 'https://example.test',
        licenseLabel: 'CC BY-SA 4.0',
        licenseUrl: 'https://example.test/license',
      },
    },
    options: ['STARK', 'LANNISTER', 'TYRELL', 'MARTELL'],
    correctOption: id.toUpperCase(),
  }
}

class FakeQuizRepository implements QuizRepository {
  private counter = 0

  // Typage structurel : une implémentation sans paramètres satisfait l'interface.
  fetchQuestion(): Promise<Question> {
    this.counter += 1
    return Promise.resolve(makeQuestion(`stark${this.counter === 1 ? '' : this.counter}`))
  }
}

class FakeBestScoreStore implements BestScoreStore {
  saved: number | null = null

  getBestScore(): number {
    return 5
  }

  saveBestScore(score: number): void {
    this.saved = score
  }
}

const competitiveSettings: GameSettings = {
  gameType: 'competitive',
  mode: 'fixed',
  difficulty: 'easy',
  fixedRounds: 5,
  timerSeconds: 8,
}

async function startCompetitive(store = new FakeBestScoreStore()) {
  const service = new QuizGameService(new FakeQuizRepository(), store)
  const snapshot = await service.start(competitiveSettings)
  return { service, snapshot }
}

describe('QuizGameService — compétitif', () => {
  it('marque une réponse rapide et correcte au barème dégressif', async () => {
    const { service, snapshot } = await startCompetitive()

    const next = service.answer(snapshot.question!.correctOption, 800) // 10 % du timer

    expect(next.score).toBe(9) // ceil(10 × 0,9)
    expect(next.lastResult?.pointsEarned).toBe(9)
    expect(next.streak).toBe(1)
    expect(next.totalTimeMs).toBe(800)
    expect(next.roundOutcomes).toHaveLength(1)
  })

  it('compte 0 point sur mauvaise réponse et brise la série', async () => {
    const { service, snapshot } = await startCompetitive()
    service.answer(snapshot.question!.correctOption, 0)
    await service.nextRound()

    const next = service.answer('MAUVAISE', 1000)

    expect(next.score).toBe(10) // le score de la manche 1 reste
    expect(next.lastResult?.isCorrect).toBe(false)
    expect(next.streak).toBe(0)
  })

  it('résout un timeout : manche perdue, temps plein consommé', async () => {
    const { service } = await startCompetitive()

    const next = service.timeout()

    expect(next.answerLocked).toBe(true)
    expect(next.lastResult?.timedOut).toBe(true)
    expect(next.lastResult?.isCorrect).toBe(false)
    expect(next.score).toBe(0)
    expect(next.totalTimeMs).toBe(8000)
    expect(next.roundOutcomes[0]).toEqual({ isCorrect: false, timedOut: true, rawPoints: 0 })
  })

  it('déduit le malus indices des points de la manche', async () => {
    const { service, snapshot } = await startCompetitive()
    service.useHint()
    service.useHint()

    const next = service.answer(snapshot.question!.correctOption, 0) // brut 10, −2 d'indices

    expect(next.score).toBe(8)
    expect(next.totalHintsUsed).toBe(2)
  })

  it('ne descend jamais sous 0 (plancher global)', async () => {
    const { service } = await startCompetitive()
    service.useHint() // score 0, malus −1 sur un timeout

    const next = service.timeout()

    expect(next.score).toBe(0)
  })

  it('les indices coûtent aussi sur un timeout (pas d’indice gratuit)', async () => {
    const { service, snapshot } = await startCompetitive()
    service.answer(snapshot.question!.correctOption, 0) // score 10
    await service.nextRound()
    service.useHint()

    const next = service.timeout()

    expect(next.score).toBe(9)
  })

  it('remet le compteur d’indices à zéro à chaque manche', async () => {
    const { service, snapshot } = await startCompetitive()
    service.useHint()
    service.answer(snapshot.question!.correctOption, 0)
    const next = await service.nextRound()

    expect(next.hintsUsed).toBe(0)
    expect(next.totalHintsUsed).toBe(1)
  })

  it('ne touche jamais au meilleur score classique', async () => {
    const store = new FakeBestScoreStore()
    const { service, snapshot } = await startCompetitive(store)
    service.answer(snapshot.question!.correctOption, 0)
    for (let i = 0; i < 4; i++) {
      await service.nextRound()
      service.timeout()
    }
    const finished = await service.nextRound()

    expect(finished.status).toBe('finished')
    expect(store.saved).toBeNull()
    expect(finished.bestScore).toBe(5) // valeur du store, inchangée
  })

  it('force le mode fixe et borne manches/timer en compétitif', async () => {
    const service = new QuizGameService(new FakeQuizRepository(), new FakeBestScoreStore())

    const snapshot = await service.start({
      ...competitiveSettings,
      mode: 'infinite',
      fixedRounds: 99,
      timerSeconds: 999,
    })

    expect(snapshot.settings.mode).toBe('fixed')
    expect(snapshot.settings.fixedRounds).toBe(20)
    expect(snapshot.settings.timerSeconds).toBe(15)
  })

  it('ignore timeout() et useHint() en mode classique', async () => {
    const service = new QuizGameService(new FakeQuizRepository(), new FakeBestScoreStore())
    const snapshot = await service.start({ ...competitiveSettings, gameType: 'classic' })

    service.timeout()
    service.useHint()
    const next = service.answer(snapshot.question!.correctOption)

    // Barème classique intact : +1 par bonne réponse, pas de malus.
    expect(next.score).toBe(1)
    expect(next.answerLocked).toBe(true)
    expect(next.totalHintsUsed).toBe(0)
  })
})
