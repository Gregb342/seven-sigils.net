import type { QuotesRepository, TierContent } from '../../domain/ports'
import type { CompetitiveTier } from '../../domain/competitiveScoring'
import type { ApiClient } from '../api/apiClient'

interface ApiQuote {
  id: string
  tier: string
  text: string
}

interface ApiQuotesResponse {
  titles: Record<string, string>
  quotes: ApiQuote[]
}

const TIERS: readonly CompetitiveTier[] = ['legendary', 'strong', 'average', 'grim']

function isTier(value: string): value is CompetitiveTier {
  return TIERS.includes(value as CompetitiveTier)
}

export class ApiQuotesRepository implements QuotesRepository {
  private readonly client: ApiClient

  constructor(client: ApiClient) {
    this.client = client
  }

  async fetchTierContent(): Promise<TierContent> {
    const response = await this.client.get<ApiQuotesResponse>('/api/v1/quotes')

    const titles: TierContent['titles'] = {}
    for (const [tier, title] of Object.entries(response.titles ?? {})) {
      if (isTier(tier) && title.trim() !== '') titles[tier] = title
    }

    const quotesByTier: TierContent['quotesByTier'] = {}
    for (const quote of response.quotes ?? []) {
      if (!isTier(quote.tier) || quote.text.trim() === '') continue
      quotesByTier[quote.tier] = [...(quotesByTier[quote.tier] ?? []), quote.text]
    }

    return { titles, quotesByTier }
  }
}
