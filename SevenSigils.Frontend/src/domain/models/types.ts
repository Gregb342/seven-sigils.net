export type Difficulty = 'easy' | 'hard'
export type GameMode = 'fixed' | 'infinite'
// classic = entraînement libre (sans chrono) ; competitive = timer + scoring dégressif.
export type GameType = 'classic' | 'competitive'

export interface Attribution {
  author?: string | null
  sourcePageUrl: string
  licenseLabel: string
  licenseUrl: string
  notes?: string
}

export interface HouseHint {
  title: string
  value: string
}

export interface Blazon {
  id: string
  familySlug: string
  familyLabel: string
  displayName?: string
  imageUrl: string
  housePageUrl: string
  hints: HouseHint[]
  kind?: string
  variantOf?: string
  attribution: Attribution
}

export interface Question {
  blazon: Blazon
  options: string[]
  correctOption: string
}

export interface GameSettings {
  gameType: GameType
  mode: GameMode
  difficulty: Difficulty
  fixedRounds: number
  /** Durée du timer par question (compétitif uniquement, ignoré en classique). */
  timerSeconds: number
}

export interface RoundResult {
  isCorrect: boolean
  correctOption: string
  /** Points effectivement ajoutés au score sur la manche (malus indices inclus). */
  pointsEarned: number
  timedOut: boolean
}

/** Trace d'une manche jouée — alimente le partage façon Wordle. */
export interface RoundOutcome {
  isCorrect: boolean
  timedOut: boolean
  /** Points bruts de la réponse (avant malus indices) : classe la vitesse (🟩 vs 🟨). */
  rawPoints: number
}

export interface HighscoreEntry {
  pseudo: string
  score: number
  difficulty: Difficulty
  dateIso: string
}

/** Score d'une partie compétitive au format officiel (10 manches, timer 8 s). */
export interface CompetitiveEntry {
  pseudo: string
  score: number
  /** Temps de réponse cumulé (ms) — départage les ex æquo au classement. */
  totalTimeMs: number
  difficulty: Difficulty
  rounds: number
  timerSeconds: number
  dateIso: string
}

export interface SessionSnapshot {
  status: 'idle' | 'running' | 'finished'
  settings: GameSettings
  score: number
  bestScore: number
  roundIndex: number
  question: Question | null
  answerLocked: boolean
  selectedAnswer: string | null
  lastResult: RoundResult | null
  /** Compétitif : temps de réponse cumulé (ms) sur la partie. */
  totalTimeMs: number
  /** Compétitif : indices révélés sur la manche courante (−1 pt chacun). */
  hintsUsed: number
  /** Compétitif : indices révélés sur toute la partie (récap de fin). */
  totalHintsUsed: number
  /** Compétitif : série de bonnes réponses en cours (cosmétique). */
  streak: number
  /** Compétitif : résultat de chaque manche jouée. */
  roundOutcomes: RoundOutcome[]
}
