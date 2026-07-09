import type { CompetitiveScoreStore } from '../../domain/ports'
import type { CompetitiveEntry, Difficulty } from '../../domain/models/types'
import { isValidPseudo } from '../../domain/pseudo'
import {
  OFFICIAL_ROUNDS,
  OFFICIAL_TIMER_SECONDS,
  POINTS_POOL,
} from '../../domain/competitiveScoring'
import { readLastPseudo, saveLastPseudo } from './lastPseudo'

const STORAGE_KEY = 'seven_sigils_competitive_scores'
const MAX_ENTRIES_PER_DIFFICULTY = 10

const DIFFICULTIES: readonly Difficulty[] = ['easy', 'hard']

// Seules les parties au format officiel (10 manches, 8 s) entrent au classement,
// et le localStorage étant librement éditable, tout ce qui en sort est re-validé :
// une entrée corrompue, forgée ou hors format est silencieusement écartée.
function isValidEntry(value: unknown): value is CompetitiveEntry {
  if (typeof value !== 'object' || value === null) return false
  const entry = value as Record<string, unknown>
  return (
    typeof entry.pseudo === 'string' &&
    isValidPseudo(entry.pseudo) &&
    typeof entry.score === 'number' &&
    Number.isInteger(entry.score) &&
    entry.score >= 0 &&
    entry.score <= OFFICIAL_ROUNDS * POINTS_POOL &&
    typeof entry.totalTimeMs === 'number' &&
    Number.isFinite(entry.totalTimeMs) &&
    entry.totalTimeMs >= 0 &&
    typeof entry.difficulty === 'string' &&
    DIFFICULTIES.includes(entry.difficulty as Difficulty) &&
    entry.rounds === OFFICIAL_ROUNDS &&
    entry.timerSeconds === OFFICIAL_TIMER_SECONDS &&
    typeof entry.dateIso === 'string'
  )
}

/** Tri du classement : score décroissant, puis temps cumulé croissant (départage). */
function compareEntries(a: CompetitiveEntry, b: CompetitiveEntry): number {
  if (b.score !== a.score) return b.score - a.score
  return a.totalTimeMs - b.totalTimeMs
}

export class LocalStorageCompetitiveScoreStore implements CompetitiveScoreStore {
  getTop(difficulty: Difficulty): CompetitiveEntry[] {
    return this.readAll().filter((entry) => entry.difficulty === difficulty)
  }

  add(entry: CompetitiveEntry): CompetitiveEntry[] {
    const difficulty = entry.difficulty
    if (!isValidEntry(entry)) {
      return this.getTop(difficulty)
    }

    const others = this.readAll().filter((e) => e.difficulty !== difficulty)
    const top = [...this.getTop(difficulty), entry]
      .sort(compareEntries)
      .slice(0, MAX_ENTRIES_PER_DIFFICULTY)

    this.write([...others, ...top])
    saveLastPseudo(entry.pseudo)
    return top
  }

  getLastPseudo(): string {
    return readLastPseudo()
  }

  private readAll(): CompetitiveEntry[] {
    try {
      const raw = window.localStorage.getItem(STORAGE_KEY)
      if (!raw) return []
      const parsed: unknown = JSON.parse(raw)
      if (!Array.isArray(parsed)) return []
      return parsed.filter(isValidEntry).sort(compareEntries)
    } catch {
      return []
    }
  }

  private write(entries: CompetitiveEntry[]): void {
    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(entries))
    } catch {
      // Écriture impossible : le jeu continue sans persistance.
    }
  }
}
