import { useState } from 'react'
import type { HighscoreEntry, SessionSnapshot } from '../../domain/models/types'
import type { HighscoreStore } from '../../domain/ports'
import { PSEUDO_MAX_LENGTH, isValidPseudo, sanitizePseudo } from '../../domain/pseudo'
import { HighscoreTable } from './HighscoreTable'

interface EndScreenProps {
  snapshot: SessionSnapshot
  highscoreStore: HighscoreStore
  onReplay: () => Promise<void>
  onMainMenu: () => void
}

export function EndScreen({ snapshot, highscoreStore, onReplay, onMainMenu }: EndScreenProps) {
  const difficulty = snapshot.settings.difficulty
  const [pseudo, setPseudo] = useState(() => highscoreStore.getLastPseudo())
  const [savedTop, setSavedTop] = useState<HighscoreEntry[] | null>(null)
  const [savedIndex, setSavedIndex] = useState<number | undefined>(undefined)

  const saveScore = () => {
    if (!isValidPseudo(pseudo)) return
    const entry: HighscoreEntry = {
      pseudo,
      score: snapshot.score,
      difficulty,
      dateIso: new Date().toISOString(),
    }
    const top = highscoreStore.add(entry)
    setSavedTop(top)
    setSavedIndex(top.indexOf(entry) === -1 ? undefined : top.indexOf(entry))
  }

  return (
    <section className="card end-card">
      <p className="eyebrow">Partie terminée</p>
      <h2>Ton score final : {snapshot.score}</h2>
      <p>Meilleur score local : {snapshot.bestScore}</p>

      {savedTop === null ? (
        <div className="highscore-entry-form">
          <label htmlFor="pseudo-input">
            Entre ton pseudo pour la postérité (lettres et chiffres, {PSEUDO_MAX_LENGTH} max)
          </label>
          <div className="highscore-entry-row">
            <input
              id="pseudo-input"
              type="text"
              value={pseudo}
              maxLength={PSEUDO_MAX_LENGTH}
              autoComplete="off"
              spellCheck={false}
              placeholder="AAA"
              onChange={(event) => setPseudo(sanitizePseudo(event.target.value))}
              onKeyDown={(event) => {
                if (event.key === 'Enter') saveScore()
              }}
            />
            <button
              type="button"
              className="primary-btn"
              onClick={saveScore}
              disabled={!isValidPseudo(pseudo)}
            >
              Enregistrer le score
            </button>
          </div>
        </div>
      ) : (
        <div className="highscore-board">
          <h3>Meilleurs scores — {difficulty === 'easy' ? 'Facile' : 'Difficile'}</h3>
          <HighscoreTable entries={savedTop} highlightIndex={savedIndex} />
          {savedIndex === undefined && (
            <p className="highscore-empty">Pas dans le top 10 cette fois… entraîne-toi !</p>
          )}
        </div>
      )}

      <button
        type="button"
        className="primary-btn"
        onClick={() => {
          void onReplay()
        }}
      >
        Rejouer
      </button>
      <button type="button" className="ghost-btn" onClick={onMainMenu}>
        Menu principal
      </button>
    </section>
  )
}
