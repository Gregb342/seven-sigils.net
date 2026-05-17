export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export class ApiClient {
  private token: string | null = null

  setToken(token: string | null): void {
    this.token = token
  }

  private buildHeaders(): Record<string, string> {
    const headers: Record<string, string> = { 'Content-Type': 'application/json' }
    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`
    }
    return headers
  }

  async post<T>(path: string, body: unknown): Promise<T> {
    const response = await fetch(path, {
      method: 'POST',
      headers: this.buildHeaders(),
      body: JSON.stringify(body),
    })

    if (!response.ok) {
      const text = await response.text().catch(() => response.statusText)
      throw new ApiError(response.status, text)
    }

    return response.json() as Promise<T>
  }

  async get<T>(path: string): Promise<T> {
    const response = await fetch(path, {
      method: 'GET',
      headers: this.buildHeaders(),
    })

    if (!response.ok) {
      const text = await response.text().catch(() => response.statusText)
      throw new ApiError(response.status, text)
    }

    return response.json() as Promise<T>
  }
}
