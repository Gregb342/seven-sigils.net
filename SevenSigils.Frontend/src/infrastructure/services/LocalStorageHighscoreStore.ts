import type { HighscoreStore } from '../../domain/ports'
import type { Difficulty, HighscoreEntry } from '../../domain/models/types'
import { isValidPseudo } from '../../domain/pseudo'
import { readLastPseudo, saveLastPseudo } from './lastPseudo'

const HIGHSCORES_KEY = 'seven_sigils_highscores'
const MAX_ENTRIES_PER_DIFFICULTY = 10

const DIFFICULTIES: readonly Difficulty[] = ['easy', 'hard']

// Le localStorage est librement éditable (F12) : tout ce qui en sort est re-validé.
// Une entrée corrompue est silencieusement écartée plutôt que de casser l'affichage.
function isValidEntry(value: unknown): value is HighscoreEntry {
  if (typeof value !== 'object' || value === null) return false
  const entry = value as Record<string, unknown>
  return (
    typeof entry.pseudo === 'string' &&
    isValidPseudo(entry.pseudo) &&
    typeof entry.score === 'number' &&
    Number.isFinite(entry.score) &&
    entry.score >= 0 &&
    typeof entry.difficulty === 'string' &&
    DIFFICULTIES.includes(entry.difficulty as Difficulty) &&
    typeof entry.dateIso === 'string'
  )
}

export class LocalStorageHighscoreStore implements HighscoreStore {
  getTop(difficulty: Difficulty): HighscoreEntry[] {
    return this.readAll().filter((entry) => entry.difficulty === difficulty)
  }

  add(entry: HighscoreEntry): HighscoreEntry[] {
    // Validation runtime malgré le typage : l'appelant peut construire l'entrée
    // depuis une saisie utilisateur. `difficulty` est lue avant le garde de type,
    // car TS rétrécit `entry` à `never` dans la branche négative.
    const difficulty = entry.difficulty
    if (!isValidEntry(entry)) {
      return this.getTop(difficulty)
    }

    const others = this.readAll().filter((e) => e.difficulty !== entry.difficulty)
    const top = [...this.getTop(entry.difficulty), entry]
      .sort((a, b) => b.score - a.score)
      .slice(0, MAX_ENTRIES_PER_DIFFICULTY)

    this.write([...others, ...top])
    saveLastPseudo(entry.pseudo)
    return top
  }

  getLastPseudo(): string {
    return readLastPseudo()
  }

  private readAll(): HighscoreEntry[] {
    try {
      const raw = window.localStorage.getItem(HIGHSCORES_KEY)
      if (!raw) return []
      const parsed: unknown = JSON.parse(raw)
      if (!Array.isArray(parsed)) return []
      return parsed.filter(isValidEntry).sort((a, b) => b.score - a.score)
    } catch {
      return []
    }
  }

  private write(entries: HighscoreEntry[]): void {
    try {
      window.localStorage.setItem(HIGHSCORES_KEY, JSON.stringify(entries))
    } catch {
      // Écriture impossible (storage plein, mode privé…) : le jeu continue sans persistance.
    }
  }
}
