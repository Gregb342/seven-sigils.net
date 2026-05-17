import { useEffect, useState } from 'react'
import type { Difficulty, GameMode } from '../../domain/models/types'
import { APP_VERSION } from '../../version'

interface StartScreenProps {
  bestScore: number
  loading: boolean
  isAuthenticated: boolean
  onStart: (mode: GameMode, difficulty: Difficulty, fixedRounds: number) => Promise<void>
  onOpenEncyclopedia: () => void
  onLogin: () => void
  onLogout: () => void
}

export function StartScreen({
  bestScore,
  loading,
  isAuthenticated,
  onStart,
  onOpenEncyclopedia,
  onLogin,
  onLogout,
}: StartScreenProps) {
  const [mode, setMode] = useState<GameMode>('fixed')
  const [difficulty, setDifficulty] = useState<Difficulty>('easy')
  const [fixedRounds, setFixedRounds] = useState(10)
  const maxFixedRounds = difficulty === 'easy' ? 30 : 40

  useEffect(() => {
    setFixedRounds((current) => Math.min(current, maxFixedRounds))
  }, [maxFixedRounds])

  return (
    <section className="card intro-card" aria-labelledby="title">
      <p className="eyebrow">Seven Sigils — {APP_VERSION}</p>
      <h1 id="title">Quiz des blasons de Westeros et d'Essos</h1>
      <p className="intro-text">
        À chaque manche, identifie le blason correct parmi 4 maisons.
      </p>

      <div className="settings-grid">
        <label>
          Mode
          <select
            value={mode}
            onChange={(event) => setMode(event.target.value as GameMode)}
            aria-label="Mode de jeu"
          >
            <option value="fixed">Partie fixe</option>
            <option value="infinite">Partie infinie</option>
          </select>
        </label>

        <label>
          Difficulté
          <select
            value={difficulty}
            onChange={(event) => setDifficulty(event.target.value as Difficulty)}
            aria-label="Difficulté"
          >
            <option value="easy">Facile</option>
            <option value="hard">Difficile</option>
          </select>
        </label>

        <label>
          Manches (mode fixe)
          <input
            type="number"
            min={5}
            max={maxFixedRounds}
            value={fixedRounds}
            onChange={(event) => {
              const parsed = Number.parseInt(event.target.value, 10) || 10
              setFixedRounds(Math.max(5, Math.min(maxFixedRounds, parsed)))
            }}
            aria-label="Nombre de manches"
            disabled={mode !== 'fixed'}
          />
        </label>
      </div>

      <button
        type="button"
        className="primary-btn"
        onClick={() => void onStart(mode, difficulty, fixedRounds)}
        disabled={loading}
      >
        {loading ? 'Chargement...' : 'Lancer la partie'}
      </button>

      <div className="start-secondary-actions">
        <button type="button" className="ghost-btn" onClick={onOpenEncyclopedia}>
          {isAuthenticated ? 'Encyclopédie des blasons' : 'Encyclopédie (connexion requise)'}
        </button>

        {isAuthenticated ? (
          <button type="button" className="ghost-btn" onClick={onLogout}>
            Se déconnecter
          </button>
        ) : (
          <button type="button" className="ghost-btn" onClick={onLogin}>
            Se connecter
          </button>
        )}
      </div>

      <p className="best-score">Meilleur score local : {bestScore}</p>
    </section>
  )
}
