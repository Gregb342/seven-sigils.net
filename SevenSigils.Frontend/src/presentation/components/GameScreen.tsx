import { useState } from 'react'
import type { SessionSnapshot } from '../../domain/models/types'

interface GameScreenProps {
  snapshot: SessionSnapshot
  onAnswer: (option: string) => void
  onNext: () => Promise<void>
  onStop: () => void
  onMainMenu: () => void
}

export function GameScreen({ snapshot, onAnswer, onNext, onStop, onMainMenu }: GameScreenProps) {
  const [hintIndex, setHintIndex] = useState(0)
  const question = snapshot.question

  if (!question) {
    return null
  }

  const hasHints = question.blazon.hints.length > 0
  const shownHints = question.blazon.hints.slice(0, hintIndex)
  const showSources = snapshot.answerLocked

  const getButtonClassName = (option: string): string => {
    if (!snapshot.answerLocked) return 'option-btn'
    if (option === question.correctOption) return 'option-btn correct'
    if (option === snapshot.selectedAnswer) return 'option-btn wrong'
    return 'option-btn muted'
  }

  return (
    <section className="card game-card" aria-live="polite">
      <header className="score-row">
        <div>
          <p className="score-label">Manche</p>
          <strong>
            {snapshot.roundIndex}
            {snapshot.settings.mode === 'fixed' ? ` / ${snapshot.settings.fixedRounds}` : ''}
          </strong>
        </div>
        <div>
          <p className="score-label">Score</p>
          <strong>{snapshot.score}</strong>
        </div>
        <div>
          <p className="score-label">Meilleur</p>
          <strong>{snapshot.bestScore}</strong>
        </div>
      </header>

      <figure className="blazon-figure">
        <img
          src={question.blazon.imageUrl}
          alt={`Blason de la maison ${question.blazon.familyLabel}`}
        />
        {showSources && (
          <figcaption className="source-fade-in">
            Auteur : {question.blazon.attribution.author ?? 'auteur non renseigné'}
            {' · '}
            <a
              href={question.blazon.attribution.sourcePageUrl}
              target="_blank"
              rel="noopener noreferrer"
            >
              source fichier
            </a>
          </figcaption>
        )}
      </figure>

      <div className="options-grid" role="group" aria-label="Choix de familles">
        {question.options.map((option) => (
          <button
            key={option}
            type="button"
            className={getButtonClassName(option)}
            onClick={() => onAnswer(option)}
            disabled={snapshot.answerLocked}
          >
            {option}
          </button>
        ))}
      </div>

      <section className="hint-panel" aria-live="polite">
        <div className="hint-header">
          <h3>Indice</h3>
          <button
            type="button"
            className="ghost-btn"
            onClick={() =>
              setHintIndex((current) => Math.min(current + 1, question.blazon.hints.length))
            }
            disabled={!hasHints || hintIndex >= question.blazon.hints.length}
          >
            {hasHints ? 'Afficher un indice' : 'Aucun indice disponible'}
          </button>
        </div>

        {shownHints.length > 0 && (
          <ul>
            {shownHints.map((hint) => (
              <li key={`${hint.title}-${hint.value}`}>
                <strong>{hint.title} :</strong> {hint.value}
              </li>
            ))}
          </ul>
        )}

        {showSources && (
          <p className="source-fade-in">
            Source maison :{' '}
            <a href={question.blazon.housePageUrl} target="_blank" rel="noopener noreferrer">
              article La Garde de Nuit
            </a>
          </p>
        )}
      </section>

      <div className="actions-row">
        <button type="button" className="ghost-btn" onClick={onMainMenu}>
          Menu principal
        </button>

        <button type="button" className="ghost-btn" onClick={onStop}>
          Stop
        </button>

        <button
          type="button"
          className="primary-btn"
          onClick={() => {
            void onNext()
          }}
          disabled={!snapshot.answerLocked}
        >
          Manche suivante
        </button>
      </div>

      {snapshot.lastResult && (
        <p className={snapshot.lastResult.isCorrect ? 'feedback good' : 'feedback bad'}>
          {snapshot.lastResult.isCorrect
            ? 'Bonne réponse.'
            : `Incorrect. Bonne réponse : ${snapshot.lastResult.correctOption}`}
        </p>
      )}
    </section>
  )
}
