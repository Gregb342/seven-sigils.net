import type { QuotesRepository } from '../../domain/ports'
import type { CompetitiveTier } from '../../domain/competitiveScoring'
import type { ApiClient } from '../api/apiClient'

interface ApiQuote {
  id: string
  tier: string
  text: string
}

const TIERS: readonly CompetitiveTier[] = ['legendary', 'strong', 'average', 'grim']

export class ApiQuotesRepository implements QuotesRepository {
  private readonly client: ApiClient

  constructor(client: ApiClient) {
    this.client = client
  }

  async fetchQuotesByTier(): Promise<Partial<Record<CompetitiveTier, string[]>>> {
    const quotes = await this.client.get<ApiQuote[]>('/api/v1/quotes')
    const grouped: Partial<Record<CompetitiveTier, string[]>> = {}
    for (const quote of quotes) {
      if (!TIERS.includes(quote.tier as CompetitiveTier) || quote.text.trim() === '') continue
      const tier = quote.tier as CompetitiveTier
      grouped[tier] = [...(grouped[tier] ?? []), quote.text]
    }
    return grouped
  }
}
