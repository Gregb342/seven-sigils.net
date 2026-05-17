import type { CatalogRepository, QuizRepository } from '../../domain/ports'
import type { Attribution, Blazon, Difficulty, HouseHint, Question } from '../../domain/models/types'
import type { ApiClient } from '../api/apiClient'

// ── Raw shapes returned by the API ───────────────────────────────────────────

interface ApiBlazon {
  id: string
  familySlug: string
  familyLabel: string
  displayName?: string
  housePageUrl: string
  kind?: string
  variantOf?: string
  hints: HouseHint[]
  attribution: Attribution
}

interface ApiQuestion {
  blazon: ApiBlazon
  options: string[]
  correctOption: string
}

interface PagedApiResponse {
  items: ApiBlazon[]
  totalCount: number
}

// ── Mapping helper ────────────────────────────────────────────────────────────

function toBlazon(raw: ApiBlazon): Blazon {
  return {
    id: raw.id,
    familySlug: raw.familySlug,
    familyLabel: raw.familyLabel,
    displayName: raw.displayName,
    imageUrl: `/blazons/Blason-${raw.familySlug}-2014-v01-256px.png`,
    housePageUrl: raw.housePageUrl,
    hints: raw.hints,
    kind: raw.kind,
    variantOf: raw.variantOf,
    attribution: raw.attribution,
  }
}

// ── Repository ────────────────────────────────────────────────────────────────

export class ApiBlazonRepository implements QuizRepository, CatalogRepository {
  private readonly client: ApiClient

  constructor(client: ApiClient) {
    this.client = client
  }

  async fetchQuestion(difficulty: Difficulty, excludedIds: string[]): Promise<Question> {
    const data = await this.client.post<ApiQuestion>('/api/v1/quiz/question', {
      difficulty,
      excludedIds,
    })
    return {
      blazon: toBlazon(data.blazon),
      options: data.options,
      correctOption: data.correctOption,
    }
  }

  async fetchPage(page: number, pageSize: number): Promise<{ items: Blazon[]; totalCount: number }> {
    const data = await this.client.get<PagedApiResponse>(
      `/api/v1/catalog?page=${page}&pageSize=${pageSize}`,
    )
    return {
      items: data.items.map(toBlazon),
      totalCount: data.totalCount,
    }
  }
}
