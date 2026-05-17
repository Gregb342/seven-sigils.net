import { useCallback, useState } from 'react'
import { ApiClient, ApiError } from '../../infrastructure/api/apiClient'

interface LoginResponse {
  token: string
}

export function useAuth(apiClient: ApiClient) {
  const [token, setToken] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const login = useCallback(
    async (email: string, password: string): Promise<boolean> => {
      setLoading(true)
      setError(null)
      try {
        const data = await apiClient.post<LoginResponse>('/api/v1/auth/login', { email, password })
        apiClient.setToken(data.token)
        setToken(data.token)
        return true
      } catch (e) {
        if (e instanceof ApiError && e.status === 401) {
          setError('Email ou mot de passe incorrect.')
        } else {
          setError('Erreur de connexion. Veuillez réessayer.')
        }
        return false
      } finally {
        setLoading(false)
      }
    },
    [apiClient],
  )

  const logout = useCallback(() => {
    apiClient.setToken(null)
    setToken(null)
  }, [apiClient])

  const clearError = useCallback(() => setError(null), [])

  return {
    token,
    isAuthenticated: !!token,
    loading,
    error,
    login,
    logout,
    clearError,
  }
}
