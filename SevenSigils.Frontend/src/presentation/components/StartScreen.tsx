import { useEffect, useState } from 'react'
import type { Difficulty, GameMode, GameSettings, GameType } from '../../domain/models/types'
import type { CompetitiveScoreStore, HighscoreStore } from '../../domain/ports'
import {
  MAX_ROUNDS,
  MAX_TIMER_SECONDS,
  MIN_ROUNDS,
  MIN_TIMER_SECONDS,
  OFFICIAL_ROUNDS,
  OFFICIAL_TIMER_SECONDS,
  isOfficialFormat,
} from '../../domain/competitiveScoring'
import { APP_VERSION_LABEL } from '../../version'
import { HighscoreTable } from './HighscoreTable'
import { CompetitiveScoreTable } from './CompetitiveScoreTable'
import { DifficultyChip } from './DifficultyChip'

interface StartScreenProps {
  bestScore: number
  loading: boolean
  highscoreStore: HighscoreStore
  competitiveStore: CompetitiveScoreStore
  onStart: (settings: GameSettings) => Promise<void>
  onOpenEncyclopedia: () => void
}

type BoardView = 'none' | 'classic' | 'competitive'

export function StartScreen({
  bestScore,
  loading,
  highscoreStore,
  competitiveStore,
  onStart,
  onOpenEncyclopedia,
}: StartScreenProps) {
  const [gameType, setGameType] = useState<GameType>('classic')
  const [mode, setMode] = useState<GameMode>('fixed')
  const [difficulty, setDifficulty] = useState<Difficulty>('easy')
  const [fixedRounds, setFixedRounds] = useState(10)
  const [timerSeconds, setTimerSeconds] = useState(OFFICIAL_TIMER_SECONDS)
  const [boardView, setBoardView] = useState<BoardView>('none')
  // La difficulté consultée au classement est indépendante de celle du formulaire de jeu.
  const [boardDifficulty, setBoardDifficulty] = useState<Difficulty>('easy')

  const isCompetitive = gameType === 'competitive'
  const maxFixedRounds = isCompetitive ? MAX_ROUNDS : difficulty === 'easy' ? 30 : 40
  const minFixedRounds = isCompetitive ? MIN_ROUNDS : 5

  useEffect(() => {
    setFixedRounds((current) => Math.min(current, maxFixedRounds))
  }, [maxFixedRounds])

  const settings: GameSettings = {
    gameType,
    mode: isCompetitive ? 'fixed' : mode,
    difficulty,
    fixedRounds,
    timerSeconds,
  }

  const official = isOfficialFormat(settings)

  return (
    <section className="card intro-card" aria-labelledby="title">
      <p className="eyebrow">Seven Sigils — {APP_VERSION_LABEL}</p>
      <h1 id="title">Quiz des blasons de Westeros et d'Essos</h1>
      <p className="intro-text">
        À chaque manche, identifie le blason correct parmi 4 maisons.
      </p>

      <div className="mode-switch" role="tablist" aria-label="Type de partie">
        <button
          type="button"
          role="tab"
          aria-selected={!isCompetitive}
          className={isCompetitive ? 'mode-tab' : 'mode-tab mode-tab--active'}
          onClick={() => setGameType('classic')}
        >
          Entraînement
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={isCompetitive}
          className={isCompetitive ? 'mode-tab mode-tab--active' : 'mode-tab'}
          onClick={() => setGameType('competitive')}
        >
          ⚔️ Compétitif
        </button>
      </div>

      {isCompetitive && (
        <p className={official ? 'format-badge format-badge--official' : 'format-badge'}>
          {official
            ? '★ Format officiel — cette partie compte pour le classement'
            : 'Partie libre — hors classement (officiel : 10 manches, timer 8 s)'}
        </p>
      )}

      <div className="settings-grid">
        {!isCompetitive && (
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
        )}

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
          Manches{!isCompetitive && mode !== 'fixed' ? ' (mode fixe)' : ''}
          <input
            type="number"
            min={minFixedRounds}
            max={maxFixedRounds}
            value={fixedRounds}
            onChange={(event) => {
              const parsed = Number.parseInt(event.target.value, 10) || OFFICIAL_ROUNDS
              setFixedRounds(Math.max(minFixedRounds, Math.min(maxFixedRounds, parsed)))
            }}
            aria-label="Nombre de manches"
            disabled={!isCompetitive && mode !== 'fixed'}
          />
        </label>

        {isCompetitive && (
          <label>
            Timer (secondes)
            <input
              type="number"
              min={MIN_TIMER_SECONDS}
              max={MAX_TIMER_SECONDS}
              value={timerSeconds}
              onChange={(event) => {
                const parsed = Number.parseInt(event.target.value, 10) || OFFICIAL_TIMER_SECONDS
                setTimerSeconds(
                  Math.max(MIN_TIMER_SECONDS, Math.min(MAX_TIMER_SECONDS, parsed)),
                )
              }}
              aria-label="Durée du timer par question"
            />
          </label>
        )}
      </div>

      <button
        type="button"
        className="primary-btn"
        onClick={() => void onStart(settings)}
        disabled={loading}
      >
        {loading ? 'Chargement...' : isCompetitive ? 'Lancer le défi ⚔️' : 'Lancer la partie'}
      </button>

      <div className="start-secondary-actions">
        <button type="button" className="ghost-btn" onClick={onOpenEncyclopedia}>
          Encyclopédie des blasons
        </button>

        <button
          type="button"
          className="ghost-btn"
          onClick={() => setBoardView((current) => (current === 'classic' ? 'none' : 'classic'))}
        >
          Meilleurs scores
        </button>

        <button
          type="button"
          className="ghost-btn"
          onClick={() =>
            setBoardView((current) => (current === 'competitive' ? 'none' : 'competitive'))
          }
        >
          Classement compétitif
        </button>
      </div>

      {boardView !== 'none' && (
        <div className="highscore-board">
          <div className="board-difficulty-switch" role="tablist" aria-label="Difficulté du classement">
            {(['easy', 'hard'] as const).map((level) => (
              <button
                key={level}
                type="button"
                role="tab"
                aria-selected={boardDifficulty === level}
                className={boardDifficulty === level ? 'mode-tab mode-tab--active' : 'mode-tab'}
                onClick={() => setBoardDifficulty(level)}
              >
                {level === 'easy' ? 'Facile' : 'Difficile'}
              </button>
            ))}
          </div>

          {boardView === 'classic' ? (
            <>
              <h3>
                Meilleurs scores <DifficultyChip difficulty={boardDifficulty} />
              </h3>
              <HighscoreTable entries={highscoreStore.getTop(boardDifficulty)} />
            </>
          ) : (
            <>
              <h3>
                Classement officiel ⚔️ <DifficultyChip difficulty={boardDifficulty} /> (10
                manches · 8 s)
              </h3>
              <CompetitiveScoreTable entries={competitiveStore.getTop(boardDifficulty)} />
            </>
          )}
        </div>
      )}

      <p className="best-score">Meilleur score local (entraînement) : {bestScore}</p>
    </section>
  )
}
