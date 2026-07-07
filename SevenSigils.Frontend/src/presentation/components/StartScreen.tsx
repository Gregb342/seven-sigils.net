import { useEffect, useState } from 'react'
import type { Difficulty, GameMode } from '../../domain/models/types'
import type { HighscoreStore } from '../../domain/ports'
import { APP_VERSION_LABEL } from '../../version'
import { HighscoreTable } from './HighscoreTable'

interface StartScreenProps {
  bestScore: number
  loading: boolean
  highscoreStore: HighscoreStore
  onStart: (mode: GameMode, difficulty: Difficulty, fixedRounds: number) => Promise<void>
  onOpenEncyclopedia: () => void
}

export function StartScreen({
  bestScore,
  loading,
  highscoreStore,
  onStart,
  onOpenEncyclopedia,
}: StartScreenProps) {
  const [mode, setMode] = useState<GameMode>('fixed')
  const [difficulty, setDifficulty] = useState<Difficulty>('easy')
  const [fixedRounds, setFixedRounds] = useState(10)
  const [showHighscores, setShowHighscores] = useState(false)
  const maxFixedRounds = difficulty === 'easy' ? 30 : 40

  useEffect(() => {
    setFixedRounds((current) => Math.min(current, maxFixedRounds))
  }, [maxFixedRounds])

  return (
    <section className="card intro-card" aria-labelledby="title">
      <p className="eyebrow">Seven Sigils — {APP_VERSION_LABEL}</p>
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
          Encyclopédie des blasons
        </button>

        <button
          type="button"
          className="ghost-btn"
          onClick={() => setShowHighscores((current) => !current)}
        >
          {showHighscores ? 'Masquer les meilleurs scores' : 'Meilleurs scores'}
        </button>
      </div>

      {showHighscores && (
        <div className="highscore-board">
          <h3>Meilleurs scores — {difficulty === 'easy' ? 'Facile' : 'Difficile'}</h3>
          <HighscoreTable entries={highscoreStore.getTop(difficulty)} />
        </div>
      )}

      <p className="best-score">Meilleur score local : {bestScore}</p>
    </section>
  )
}
