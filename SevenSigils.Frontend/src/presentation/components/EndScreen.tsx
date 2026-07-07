import { useState } from 'react'
import type { CompetitiveEntry, HighscoreEntry, SessionSnapshot } from '../../domain/models/types'
import type { CompetitiveScoreStore, HighscoreStore } from '../../domain/ports'
import { PSEUDO_MAX_LENGTH, isValidPseudo, sanitizePseudo } from '../../domain/pseudo'
import {
  POINTS_POOL,
  buildShareText,
  computeTier,
  formatDuration,
  isOfficialFormat,
} from '../../domain/competitiveScoring'
import { TIER_CONTENT, pickQuote } from '../../resources/quotes'
import { HighscoreTable } from './HighscoreTable'
import { CompetitiveScoreTable } from './CompetitiveScoreTable'
import { DifficultyChip } from './DifficultyChip'

interface EndScreenProps {
  snapshot: SessionSnapshot
  highscoreStore: HighscoreStore
  competitiveStore: CompetitiveScoreStore
  onReplay: () => Promise<void>
  onMainMenu: () => void
}

export function EndScreen({
  snapshot,
  highscoreStore,
  competitiveStore,
  onReplay,
  onMainMenu,
}: EndScreenProps) {
  const isCompetitive = snapshot.settings.gameType === 'competitive'

  return isCompetitive ? (
    <CompetitiveEnd
      snapshot={snapshot}
      competitiveStore={competitiveStore}
      onReplay={onReplay}
      onMainMenu={onMainMenu}
    />
  ) : (
    <ClassicEnd
      snapshot={snapshot}
      highscoreStore={highscoreStore}
      onReplay={onReplay}
      onMainMenu={onMainMenu}
    />
  )
}

// ── Fin de partie compétitive ─────────────────────────────────────────────────

function CompetitiveEnd({
  snapshot,
  competitiveStore,
  onReplay,
  onMainMenu,
}: {
  snapshot: SessionSnapshot
  competitiveStore: CompetitiveScoreStore
  onReplay: () => Promise<void>
  onMainMenu: () => void
}) {
  const rounds = snapshot.settings.fixedRounds
  const difficulty = snapshot.settings.difficulty
  const maxScore = rounds * POINTS_POOL
  const correctCount = snapshot.roundOutcomes.filter((o) => o.isCorrect).length
  const tier = computeTier(snapshot.score, rounds)
  const official = isOfficialFormat(snapshot.settings)

  // useState avec initialiseur : la citation ne change pas à chaque re-render.
  const [quote] = useState(() => pickQuote(tier))
  const [shareFeedback, setShareFeedback] = useState<string | null>(null)
  const [pseudo, setPseudo] = useState(() => competitiveStore.getLastPseudo())
  const [savedTop, setSavedTop] = useState<CompetitiveEntry[] | null>(null)
  const [savedIndex, setSavedIndex] = useState<number | undefined>(undefined)

  const share = async () => {
    const text = buildShareText(snapshot.score, rounds, snapshot.totalTimeMs, snapshot.roundOutcomes)
    try {
      await navigator.clipboard.writeText(text)
      setShareFeedback('Copié ! Colle-le où tu veux.')
    } catch {
      setShareFeedback('Copie impossible dans ce navigateur.')
    }
  }

  const saveScore = () => {
    if (!isValidPseudo(pseudo)) return
    const entry: CompetitiveEntry = {
      pseudo,
      score: snapshot.score,
      totalTimeMs: Math.round(snapshot.totalTimeMs),
      difficulty,
      rounds,
      timerSeconds: snapshot.settings.timerSeconds,
      dateIso: new Date().toISOString(),
    }
    const top = competitiveStore.add(entry)
    const index = top.indexOf(entry)
    setSavedTop(top)
    setSavedIndex(index === -1 ? undefined : index)
  }

  const isNewRecord = savedIndex === 0
  const madeTheBoard = savedIndex !== undefined

  return (
    <section className={`card end-card tier-${tier}${isNewRecord ? ' celebrate' : ''}`}>
      <p className="eyebrow">
        Partie compétitive terminée <DifficultyChip difficulty={difficulty} />
      </p>
      <p className="tier-title">{TIER_CONTENT[tier].title}</p>
      <h2>
        {snapshot.score} / {maxScore}
      </h2>
      <blockquote className="tier-quote">« {quote} »</blockquote>

      <dl className="end-stats">
        <div>
          <dt>Bonnes réponses</dt>
          <dd>
            {correctCount} / {rounds}
          </dd>
        </div>
        <div>
          <dt>Temps total</dt>
          <dd>{formatDuration(snapshot.totalTimeMs)}</dd>
        </div>
        <div>
          <dt>Indices utilisés</dt>
          <dd>{snapshot.totalHintsUsed}</dd>
        </div>
      </dl>

      <div className="share-row">
        <button type="button" className="ghost-btn" onClick={() => void share()}>
          Partager le résultat
        </button>
        {shareFeedback && <span className="share-feedback">{shareFeedback}</span>}
      </div>

      {official ? (
        savedTop === null ? (
          <div className="highscore-entry-form">
            <label htmlFor="pseudo-input">
              Partie officielle ! Entre ton pseudo pour le classement (lettres et chiffres,{' '}
              {PSEUDO_MAX_LENGTH} max)
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
                Enregistrer
              </button>
            </div>
          </div>
        ) : (
          <div className="highscore-board">
            {isNewRecord && <p className="record-banner">👑 Nouveau record !</p>}
            {!isNewRecord && madeTheBoard && (
              <p className="record-banner record-banner--soft">⚔️ Tu entres au classement !</p>
            )}
            <h3>
              Classement officiel <DifficultyChip difficulty={difficulty} />
            </h3>
            <CompetitiveScoreTable entries={savedTop} highlightIndex={savedIndex} />
            {!madeTheBoard && (
              <p className="highscore-empty">Pas dans le top 10 cette fois… entraîne-toi !</p>
            )}
          </div>
        )
      ) : (
        <p className="format-badge">
          Partie libre ({rounds} manches · {snapshot.settings.timerSeconds} s) — hors
          classement. Le format officiel est 10 manches · 8 s.
        </p>
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

// ── Fin de partie classique (comportement historique inchangé) ────────────────

function ClassicEnd({
  snapshot,
  highscoreStore,
  onReplay,
  onMainMenu,
}: {
  snapshot: SessionSnapshot
  highscoreStore: HighscoreStore
  onReplay: () => Promise<void>
  onMainMenu: () => void
}) {
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
          <h3>
            Meilleurs scores <DifficultyChip difficulty={difficulty} />
          </h3>
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
