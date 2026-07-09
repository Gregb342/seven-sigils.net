import { describe, expect, it } from 'vitest'
import {
  OFFICIAL_ROUNDS,
  OFFICIAL_TIMER_SECONDS,
  POINTS_POOL,
  buildShareText,
  computePoints,
  computeTier,
  formatDuration,
  isOfficialFormat,
} from './competitiveScoring'
import type { GameSettings, RoundOutcome } from './models/types'

const TIMER_MS = 8000

describe('computePoints', () => {
  it('donne le pool complet pour une réponse instantanée', () => {
    expect(computePoints(0, TIMER_MS, true)).toBe(POINTS_POOL)
  })

  it('donne au moins 1 point pour une bonne réponse au dernier instant', () => {
    expect(computePoints(TIMER_MS - 1, TIMER_MS, true)).toBe(1)
    expect(computePoints(TIMER_MS, TIMER_MS, true)).toBe(1)
  })

  it('décroît linéairement avec le temps écoulé', () => {
    // À mi-timer : 50 % du pool.
    expect(computePoints(TIMER_MS / 2, TIMER_MS, true)).toBe(POINTS_POOL / 2)
    // À 30 % du timer écoulé : ceil(10 × 0,7) = 7.
    expect(computePoints(TIMER_MS * 0.3, TIMER_MS, true)).toBe(7)
  })

  it('donne 0 pour une mauvaise réponse, quelle que soit la vitesse', () => {
    expect(computePoints(0, TIMER_MS, false)).toBe(0)
  })

  it('tolère les entrées absurdes sans casser', () => {
    expect(computePoints(-500, TIMER_MS, true)).toBe(POINTS_POOL)
    expect(computePoints(TIMER_MS * 5, TIMER_MS, true)).toBe(1)
    expect(computePoints(1000, 0, true)).toBe(0)
  })
})

describe('isOfficialFormat', () => {
  const official: Pick<GameSettings, 'gameType' | 'mode' | 'fixedRounds' | 'timerSeconds'> = {
    gameType: 'competitive',
    mode: 'fixed',
    fixedRounds: OFFICIAL_ROUNDS,
    timerSeconds: OFFICIAL_TIMER_SECONDS,
  }

  it('reconnaît le format officiel (10 manches, 8 s)', () => {
    expect(isOfficialFormat(official)).toBe(true)
  })

  it('rejette toute variation qui influence le score', () => {
    expect(isOfficialFormat({ ...official, fixedRounds: 20 })).toBe(false)
    expect(isOfficialFormat({ ...official, timerSeconds: 15 })).toBe(false)
    expect(isOfficialFormat({ ...official, gameType: 'classic' })).toBe(false)
    expect(isOfficialFormat({ ...official, mode: 'infinite' })).toBe(false)
  })
})

describe('computeTier', () => {
  it('attribue les paliers selon le pourcentage du score maximal', () => {
    expect(computeTier(85, 10)).toBe('legendary') // 85 %
    expect(computeTier(84, 10)).toBe('strong')
    expect(computeTier(60, 10)).toBe('strong') // 60 %
    expect(computeTier(59, 10)).toBe('average')
    expect(computeTier(35, 10)).toBe('average') // 35 %
    expect(computeTier(34, 10)).toBe('grim')
    expect(computeTier(0, 10)).toBe('grim')
  })
})

describe('formatDuration', () => {
  it('formate en m:ss', () => {
    expect(formatDuration(83_200)).toBe('1:23')
    expect(formatDuration(5_000)).toBe('0:05')
    expect(formatDuration(0)).toBe('0:00')
  })
})

describe('buildShareText', () => {
  it('construit l’en-tête et la ligne d’emojis façon Wordle', () => {
    const outcomes: RoundOutcome[] = [
      { isCorrect: true, timedOut: false, rawPoints: 9 }, // 🟩 rapide
      { isCorrect: true, timedOut: false, rawPoints: 3 }, // 🟨 lent
      { isCorrect: false, timedOut: false, rawPoints: 0 }, // 🟥 raté
      { isCorrect: false, timedOut: true, rawPoints: 0 }, // ⬛ timeout
    ]

    const text = buildShareText(12, 4, 83_200, outcomes)

    expect(text).toBe('Seven Sigils ⚔️ 12/40 · 4 manches · 1:23\n🟩🟨🟥⬛')
  })
})
