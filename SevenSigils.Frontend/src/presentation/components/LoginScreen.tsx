import { useState } from 'react'

interface LoginScreenProps {
  onLogin: (email: string, password: string) => Promise<void>
  onBack: () => void
  loading: boolean
  error: string | null
}

export function LoginScreen({ onLogin, onBack, loading, error }: LoginScreenProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    void onLogin(email, password)
  }

  return (
    <section className="card login-card" aria-labelledby="login-title">
      <p className="eyebrow">Compte</p>
      <h1 id="login-title">Connexion</h1>
      <p className="intro-text">
        Connectez-vous pour accéder à l'encyclopédie des blasons.
      </p>

      <form className="login-form" onSubmit={handleSubmit} noValidate>
        <label>
          Adresse email
          <input
            type="email"
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="votre@email.com"
            required
          />
        </label>

        <label>
          Mot de passe
          <input
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </label>

        {error && (
          <p role="alert" className="login-error">
            {error}
          </p>
        )}

        <button type="submit" className="primary-btn" disabled={loading || !email || !password}>
          {loading ? 'Connexion...' : 'Se connecter'}
        </button>
      </form>

      <button type="button" className="ghost-btn login-back-btn" onClick={onBack}>
        Retour au menu
      </button>
    </section>
  )
}
