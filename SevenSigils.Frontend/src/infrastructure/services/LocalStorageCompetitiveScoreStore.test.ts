import { beforeEach, describe, expect, it } from 'vitest'
import { LocalStorageCompetitiveScoreStore } from './LocalStorageCompetitiveScoreStore'
import type { CompetitiveEntry } from '../../domain/models/types'

// Stub localStorage : le store lit `window.localStorage` à l'exécution.
const memory = new Map<string, string>()
;(globalThis as Record<string, unknown>).window = {
  localStorage: {
    getItem: (key: string) => (memory.has(key) ? memory.get(key) : null),
    setItem: (key: string, value: string) => void memory.set(key, String(value)),
    removeItem: (key: string) => void memory.delete(key),
  },
}

const STORAGE_KEY = 'seven_sigils_competitive_scores'

function officialEntry(overrides: Partial<CompetitiveEntry> = {}): CompetitiveEntry {
  return {
    pseudo: 'Joueur1',
    score: 50,
    totalTimeMs: 40_000,
    difficulty: 'easy',
    rounds: 10,
    timerSeconds: 8,
    dateIso: '2026-07-07T00:00:00.000Z',
    ...overrides,
  }
}

describe('LocalStorageCompetitiveScoreStore', () => {
  beforeEach(() => memory.clear())

  it('trie par score décroissant puis temps croissant (départage)', () => {
    const store = new LocalStorageCompetitiveScoreStore()
    store.add(officialEntry({ pseudo: 'Lent', score: 80, totalTimeMs: 60_000 }))
    store.add(officialEntry({ pseudo: 'Rapide', score: 80, totalTimeMs: 30_000 }))
    store.add(officialEntry({ pseudo: 'Meilleur', score: 90, totalTimeMs: 70_000 }))

    const top = store.getTop('easy')

    expect(top.map((e) => e.pseudo)).toEqual(['Meilleur', 'Rapide', 'Lent'])
  })

  it('limite le classement à 10 entrées par difficulté', () => {
    const store = new LocalStorageCompetitiveScoreStore()
    for (let i = 1; i <= 12; i++) {
      store.add(officialEntry({ pseudo: `Joueur${i}`, score: i }))
    }

    const top = store.getTop('easy')

    expect(top).toHaveLength(10)
    expect(top[0].score).toBe(12)
    expect(top.some((e) => e.score <= 2)).toBe(false)
  })

  it('sépare les difficultés', () => {
    const store = new LocalStorageCompetitiveScoreStore()
    store.add(officialEntry({ difficulty: 'easy' }))

    expect(store.getTop('hard')).toHaveLength(0)
  })

  it('refuse les parties hors format officiel', () => {
    const store = new LocalStorageCompetitiveScoreStore()
    store.add(officialEntry({ rounds: 20 }))
    store.add(officialEntry({ timerSeconds: 15 }))

    expect(store.getTop('easy')).toHaveLength(0)
  })

  it('écarte les entrées corrompues ou forgées relues du storage', () => {
    memory.set(
      STORAGE_KEY,
      JSON.stringify([
        officialEntry({ pseudo: 'Legit' }),
        officialEntry({ pseudo: '<script>alert(1)</script>' }),
        officialEntry({ pseudo: 'ScoreImpossible', score: 999 }),
        officialEntry({ pseudo: 'ScoreNegatif', score: -1 }),
        officialEntry({ pseudo: 'ScoreDecimal', score: 55.5 }),
        officialEntry({ pseudo: 'HorsFormat', rounds: 5 }),
        'garbage',
      ]),
    )
    const store = new LocalStorageCompetitiveScoreStore()

    const top = store.getTop('easy')

    expect(top).toHaveLength(1)
    expect(top[0].pseudo).toBe('Legit')
  })

  it('tolère un JSON invalide', () => {
    memory.set(STORAGE_KEY, '{pas du json')

    expect(new LocalStorageCompetitiveScoreStore().getTop('easy')).toEqual([])
  })

  it('mémorise le dernier pseudo utilisé', () => {
    const store = new LocalStorageCompetitiveScoreStore()
    store.add(officialEntry({ pseudo: 'Gregb342' }))

    expect(store.getLastPseudo()).toBe('Gregb342')
  })
})
