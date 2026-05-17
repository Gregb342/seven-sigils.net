export type Difficulty = 'easy' | 'hard'
export type GameMode = 'fixed' | 'infinite'

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
  mode: GameMode
  difficulty: Difficulty
  fixedRounds: number
}

export interface RoundResult {
  isCorrect: boolean
  correctOption: string
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
}
