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

  private async request(path: string, method: string, body?: unknown): Promise<Response> {
    const response = await fetch(path, {
      method,
      headers: this.buildHeaders(),
      body: body === undefined ? undefined : JSON.stringify(body),
    })

    if (!response.ok) {
      const text = await response.text().catch(() => response.statusText)
      throw new ApiError(response.status, text)
    }

    return response
  }

  async get<T>(path: string): Promise<T> {
    const response = await this.request(path, 'GET')
    return response.json() as Promise<T>
  }

  async post<T>(path: string, body: unknown): Promise<T> {
    const response = await this.request(path, 'POST', body)
    return response.json() as Promise<T>
  }

  async put<T>(path: string, body: unknown): Promise<T> {
    const response = await this.request(path, 'PUT', body)
    return response.json() as Promise<T>
  }

  /** DELETE renvoie 204 No Content : pas de corps à parser. */
  async delete(path: string): Promise<void> {
    await this.request(path, 'DELETE')
  }
}
