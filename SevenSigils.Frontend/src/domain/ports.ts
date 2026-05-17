import type { Blazon, Difficulty, Question } from './models/types'

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
