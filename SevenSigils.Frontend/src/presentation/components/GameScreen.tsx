import { useCallback, useEffect, useState } from 'react'
import type { SessionSnapshot } from '../../domain/models/types'
import { HINT_COST } from '../../domain/competitiveScoring'
import { useCountdown } from '../hooks/useCountdown'

interface GameScreenProps {
  snapshot: SessionSnapshot
  onAnswer: (option: string, elapsedMs?: number) => void
  onTimeout: () => void
  onHint: () => void
  onNext: () => Promise<void>
  onStop: () => void
  onMainMenu: () => void
}

const PRE_STEPS = ['3', '2', '1', 'GO !'] as const
const PRE_STEP_MS = 800
const AUTO_ADVANCE_MS = 2000

export function GameScreen({
  snapshot,
  onAnswer,
  onTimeout,
  onHint,
  onNext,
  onStop,
  onMainMenu,
}: GameScreenProps) {
  const question = snapshot.question
  const isCompetitive = snapshot.settings.gameType === 'competitive'
  const timerMs = snapshot.settings.timerSeconds * 1000

  const [hintIndex, setHintIndex] = useState(0)
  const [imageReady, setImageReady] = useState(false)
  // Compte à rebours 3-2-1-GO avant la première manche compétitive uniquement
  // (le composant est remonté à chaque question via sa prop key).
  const [preStep, setPreStep] = useState(isCompetitive && snapshot.roundIndex === 1 ? 0 : -1)
  const preGame = preStep >= 0

  const countdown = useCountdown(timerMs, onTimeout)
  const { start: startTimer, stop: stopTimer, running: timerRunning } = countdown

  // Déroulé du 3-2-1-GO (setTimeout : asynchrone, pas de setState direct dans l'effet).
  useEffect(() => {
    if (preStep < 0) return
    const timer = setTimeout(
      () => setPreStep((current) => (current + 1 >= PRE_STEPS.length ? -1 : current + 1)),
      PRE_STEP_MS,
    )
    return () => clearTimeout(timer)
  }, [preStep])

  // Équité : le chrono ne démarre qu'une fois l'image du blason affichée
  // (la latence réseau ne doit pas coûter de points), et après le GO.
  useEffect(() => {
    if (!isCompetitive || preGame || !imageReady || snapshot.answerLocked || timerRunning) return
    startTimer()
  }, [isCompetitive, preGame, imageReady, snapshot.answerLocked, timerRunning, startTimer])

  const playable = !snapshot.answerLocked && (!isCompetitive || (timerRunning && !preGame))

  const handleAnswer = useCallback(
    (option: string) => {
      if (isCompetitive) {
        const elapsed = countdown.elapsedMs
        stopTimer()
        onAnswer(option, elapsed)
      } else {
        onAnswer(option)
      }
    },
    [isCompetitive, countdown.elapsedMs, stopTimer, onAnswer],
  )

  // Raccourcis clavier 1-4 (compétitif : la vitesse est le jeu).
  useEffect(() => {
    if (!isCompetitive || !playable || !question) return
    const handler = (event: KeyboardEvent) => {
      const index = ['1', '2', '3', '4'].indexOf(event.key)
      if (index === -1) return
      const option = question.options[index]
      if (option) handleAnswer(option)
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [isCompetitive, playable, question, handleAnswer])

  // Rythme arcade : enchaînement automatique vers la manche suivante.
  useEffect(() => {
    if (!isCompetitive || !snapshot.answerLocked) return
    const timer = setTimeout(() => {
      void onNext()
    }, AUTO_ADVANCE_MS)
    return () => clearTimeout(timer)
  }, [isCompetitive, snapshot.answerLocked, onNext])

  if (!question) {
    return null
  }

  const hasHints = question.blazon.hints.length > 0
  const shownHints = question.blazon.hints.slice(0, hintIndex)
  const showSources = snapshot.answerLocked

  const revealHint = () => {
    setHintIndex((current) => Math.min(current + 1, question.blazon.hints.length))
    if (isCompetitive) onHint()
  }

  const getButtonClassName = (option: string): string => {
    if (!snapshot.answerLocked) return 'option-btn'
    if (option === question.correctOption) return 'option-btn correct'
    if (option === snapshot.selectedAnswer) return 'option-btn wrong'
    return 'option-btn muted'
  }

  const timerStress = countdown.ratio < 0.25 ? 'danger' : countdown.ratio < 0.5 ? 'warn' : ''

  return (
    <section
      className={`card game-card${isCompetitive && timerRunning && timerStress === 'danger' ? ' game-card--danger' : ''}`}
      aria-live="polite"
    >
      {preGame && (
        <div className="pre-countdown" role="status" aria-label="La partie commence">
          <span key={preStep} className="pre-countdown-step">
            {PRE_STEPS[preStep]}
          </span>
        </div>
      )}

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
        {isCompetitive ? (
          <div>
            <p className="score-label">Série</p>
            <strong className={snapshot.streak >= 2 ? 'streak-on' : 'streak-off'}>
              {snapshot.streak >= 2 ? `🔥 ×${snapshot.streak}` : '—'}
            </strong>
          </div>
        ) : (
          <div>
            <p className="score-label">Meilleur</p>
            <strong>{snapshot.bestScore}</strong>
          </div>
        )}
      </header>

      {isCompetitive && (
        <div
          className={`timer-track ${timerStress}`}
          role="timer"
          aria-label="Temps restant pour répondre"
        >
          <div
            className={`timer-fill ${timerStress}`}
            style={{ width: `${(timerRunning ? countdown.ratio : snapshot.answerLocked ? 0 : 1) * 100}%` }}
          />
          <span className="timer-seconds">
            {timerRunning ? `${(countdown.remainingMs / 1000).toFixed(1)} s` : ''}
          </span>
        </div>
      )}

      <figure className="blazon-figure">
        <img
          src={question.blazon.imageUrl}
          alt={`Blason de la maison ${question.blazon.familyLabel}`}
          ref={(node) => {
            // Image déjà en cache : onLoad peut ne jamais tirer, on vérifie ici.
            if (node?.complete) setImageReady(true)
          }}
          onLoad={() => setImageReady(true)}
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
        {question.options.map((option, index) => (
          <button
            key={option}
            type="button"
            className={getButtonClassName(option)}
            onClick={() => handleAnswer(option)}
            disabled={!playable}
          >
            {isCompetitive && <span className="key-badge" aria-hidden="true">{index + 1}</span>}
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
            onClick={revealHint}
            disabled={!hasHints || hintIndex >= question.blazon.hints.length || (isCompetitive && !playable)}
          >
            {hasHints
              ? isCompetitive
                ? `Afficher un indice (−${HINT_COST} pt)`
                : 'Afficher un indice'
              : 'Aucun indice disponible'}
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

        {!isCompetitive && (
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
        )}
      </div>

      {snapshot.lastResult && (
        <p className={snapshot.lastResult.isCorrect ? 'feedback good' : 'feedback bad'}>
          {snapshot.lastResult.timedOut
            ? `Trop tard ! Bonne réponse : ${snapshot.lastResult.correctOption}`
            : snapshot.lastResult.isCorrect
              ? isCompetitive
                ? `Bonne réponse ! +${snapshot.lastResult.pointsEarned} pt${snapshot.lastResult.pointsEarned > 1 ? 's' : ''}`
                : 'Bonne réponse.'
              : `Incorrect. Bonne réponse : ${snapshot.lastResult.correctOption}`}
        </p>
      )}
    </section>
  )
}
