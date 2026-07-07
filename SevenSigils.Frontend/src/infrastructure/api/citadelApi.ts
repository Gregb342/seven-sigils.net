import type { ApiClient } from './apiClient'

// ── Formes brutes de l'API admin (contrats BlazonResponse / CreateBlazonRequest) ──

export interface AdminHint {
  title: string
  value: string
}

export interface AdminAttribution {
  author?: string | null
  sourcePageUrl: string
  licenseLabel: string
  licenseUrl: string
  notes?: string | null
}

export interface AdminBlazon {
  id: string
  familySlug: string
  familyLabel: string
  displayName?: string | null
  housePageUrl: string
  kind?: string | null
  variantOf?: string | null
  includeInEasy: boolean
  includeInHard: boolean
  hints: AdminHint[]
  attribution: AdminAttribution
}

/** Payload d'écriture : create = tout, update = tout sauf familySlug (porté par l'URL). */
export type BlazonWritePayload = Omit<AdminBlazon, 'id'>

export interface AdminQuote {
  id: string
  tier: string
  text: string
}

interface LoginResponse {
  accessToken: string
  email: string
  roles: string[]
}

interface PagedResponse {
  items: AdminBlazon[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export class CitadelApi {
  private readonly client: ApiClient

  constructor(client: ApiClient) {
    this.client = client
  }

  /** Le token JWT reste en mémoire (ApiClient) — jamais persisté côté navigateur. */
  async login(email: string, password: string): Promise<LoginResponse> {
    const result = await this.client.post<LoginResponse>('/api/v1/auth/login', { email, password })
    this.client.setToken(result.accessToken)
    return result
  }

  logout(): void {
    this.client.setToken(null)
  }

  /** Charge tout le catalogue (public) page par page. */
  async fetchAllBlazons(): Promise<AdminBlazon[]> {
    const all: AdminBlazon[] = []
    let page = 1
    for (;;) {
      const result = await this.client.get<PagedResponse>(`/api/v1/catalog?page=${page}&pageSize=100`)
      all.push(...result.items)
      if (page >= result.totalPages) break
      page++
    }
    return all
  }

  createBlazon(payload: BlazonWritePayload): Promise<AdminBlazon> {
    return this.client.post<AdminBlazon>('/api/v1/admin/blazons', payload)
  }

  fetchQuotes(): Promise<AdminQuote[]> {
    return this.client.get<AdminQuote[]>('/api/v1/quotes')
  }

  createQuote(tier: string, text: string): Promise<AdminQuote> {
    return this.client.post<AdminQuote>('/api/v1/admin/quotes', { tier, text })
  }

  updateQuote(id: string, tier: string, text: string): Promise<AdminQuote> {
    return this.client.put<AdminQuote>(`/api/v1/admin/quotes/${encodeURIComponent(id)}`, {
      tier,
      text,
    })
  }

  deleteQuote(id: string): Promise<void> {
    return this.client.delete(`/api/v1/admin/quotes/${encodeURIComponent(id)}`)
  }

  updateBlazon(slug: string, payload: Omit<BlazonWritePayload, 'familySlug'>): Promise<AdminBlazon> {
    return this.client.put<AdminBlazon>(`/api/v1/admin/blazons/${encodeURIComponent(slug)}`, payload)
  }

  deleteBlazon(slug: string): Promise<void> {
    return this.client.delete(`/api/v1/admin/blazons/${encodeURIComponent(slug)}`)
  }
}
