import type { Blazon, CompetitiveEntry, Difficulty, HighscoreEntry, Question } from './models/types'

export interface QuizRepository {
  fetchQuestion(difficulty: Difficulty, excludedIds: string[]): Promise<Question>
}

export interface CatalogRepository {
  fetchPage(page: number, pageSize: number): Promise<{ items: Blazon[]; totalCount: number }>
}

export interface BestScoreStore {
  getBestScore(): number
  saveBestScore(score: number): void
}

export interface HighscoreStore {
  /** Top scores (triés décroissants) pour une difficulté donnée. */
  getTop(difficulty: Difficulty): HighscoreEntry[]
  /** Enregistre une entrée et retourne le top mis à jour pour sa difficulté. */
  add(entry: HighscoreEntry): HighscoreEntry[]
  /** Dernier pseudo utilisé, pour préremplir la saisie (chaîne vide si aucun). */
  getLastPseudo(): string
}

export interface CompetitiveScoreStore {
  /** Classement officiel (score desc, temps asc) pour une difficulté donnée. */
  getTop(difficulty: Difficulty): CompetitiveEntry[]
  /** Enregistre une partie officielle et retourne le classement mis à jour. */
  add(entry: CompetitiveEntry): CompetitiveEntry[]
  /** Dernier pseudo utilisé, pour préremplir la saisie (chaîne vide si aucun). */
  getLastPseudo(): string
}
