import type { GameSettings, RoundOutcome } from './models/types'

// ── Règles du mode compétitif — source de vérité unique, fonctions pures ─────

export const POINTS_POOL = 10
export const HINT_COST = 1

export const OFFICIAL_ROUNDS = 10
export const OFFICIAL_TIMER_SECONDS = 8

export const MIN_ROUNDS = 5
export const MAX_ROUNDS = 20
export const MIN_TIMER_SECONDS = 5
export const MAX_TIMER_SECONDS = 15

/**
 * Points bruts d'une réponse : décroissance linéaire sur la durée du timer,
 * arrondi supérieur. Une bonne réponse rapporte toujours au moins 1 point ;
 * une mauvaise réponse ou un timeout rapporte 0.
 */
export function computePoints(elapsedMs: number, timerMs: number, isCorrect: boolean): number {
  if (!isCorrect || timerMs <= 0) return 0
  const remainingRatio = Math.max(0, 1 - Math.max(0, elapsedMs) / timerMs)
  return Math.max(1, Math.ceil(POINTS_POOL * remainingRatio))
}

/**
 * Format officiel = la mesure de référence du classement : 10 manches, timer 8 s.
 * Toute variable influençant le score doit être au réglage de référence.
 */
export function isOfficialFormat(
  settings: Pick<GameSettings, 'gameType' | 'mode' | 'fixedRounds' | 'timerSeconds'>,
): boolean {
  return (
    settings.gameType === 'competitive' &&
    settings.mode === 'fixed' &&
    settings.fixedRounds === OFFICIAL_ROUNDS &&
    settings.timerSeconds === OFFICIAL_TIMER_SECONDS
  )
}

export type CompetitiveTier = 'legendary' | 'strong' | 'average' | 'grim'

/** Palier de fin de partie, en pourcentage du score maximal possible. */
export function computeTier(score: number, rounds: number): CompetitiveTier {
  const max = rounds * POINTS_POOL
  const ratio = max > 0 ? score / max : 0
  if (ratio >= 0.85) return 'legendary'
  if (ratio >= 0.6) return 'strong'
  if (ratio >= 0.35) return 'average'
  return 'grim'
}

/** mm:ss à partir de millisecondes (ex. 83 200 ms → "1:23"). */
export function formatDuration(totalTimeMs: number): string {
  const totalSeconds = Math.round(Math.max(0, totalTimeMs) / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${minutes}:${String(seconds).padStart(2, '0')}`
}

/**
 * Résumé partageable façon Wordle :
 *   Seven Sigils ⚔️ 87/100 · 10 manches · 1:23
 *   🟩🟩🟨🟥⬛🟩🟩🟨🟩🟩
 * 🟩 réponse rapide (≥ 50 % du pool), 🟨 réponse lente, 🟥 raté, ⬛ timeout.
 */
export function buildShareText(
  score: number,
  rounds: number,
  totalTimeMs: number,
  outcomes: RoundOutcome[],
): string {
  const line = outcomes
    .map((o) => {
      if (o.timedOut) return '⬛'
      if (!o.isCorrect) return '🟥'
      return o.rawPoints >= POINTS_POOL / 2 ? '🟩' : '🟨'
    })
    .join('')
  const header = `Seven Sigils ⚔️ ${score}/${rounds * POINTS_POOL} · ${rounds} manches · ${formatDuration(totalTimeMs)}`
  return `${header}\n${line}`
}
