import type { BestScoreStore } from '../../domain/ports'

const BEST_SCORE_KEY = 'seven_sigils_best_score'

export class LocalStorageBestScoreStore implements BestScoreStore {
  getBestScore(): number {
    try {
      const raw = window.localStorage.getItem(BEST_SCORE_KEY)
      if (!raw) return 0
      const parsed = Number.parseInt(raw, 10)
      return Number.isNaN(parsed) ? 0 : Math.max(0, parsed)
    } catch {
      return 0
    }
  }

  saveBestScore(score: number): void {
    try {
      window.localStorage.setItem(BEST_SCORE_KEY, String(Math.max(0, score)))
    } catch {
      // Intentionally ignore write failures so gameplay continues without persistence.
    }
  }
}
