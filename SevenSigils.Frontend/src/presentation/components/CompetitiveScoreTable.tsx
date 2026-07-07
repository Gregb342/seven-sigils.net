import type { CompetitiveEntry } from '../../domain/models/types'
import { formatDuration } from '../../domain/competitiveScoring'

interface CompetitiveScoreTableProps {
  entries: CompetitiveEntry[]
  /** Index de l'entrée à mettre en évidence (score fraîchement enregistré). */
  highlightIndex?: number
}

// Pseudos rendus en JSX texte uniquement (React échappe) — jamais de dangerouslySetInnerHTML.
export function CompetitiveScoreTable({ entries, highlightIndex }: CompetitiveScoreTableProps) {
  if (entries.length === 0) {
    return (
      <p className="highscore-empty">
        Aucune partie officielle enregistrée. Le trône est à prendre !
      </p>
    )
  }

  return (
    <ol className="highscore-list">
      {entries.map((entry, index) => (
        <li
          key={`${entry.pseudo}-${entry.dateIso}-${index}`}
          className={index === highlightIndex ? 'highscore-row highscore-row--new' : 'highscore-row'}
        >
          <span className="highscore-rank">{index + 1}</span>
          <span className="highscore-pseudo">{entry.pseudo}</span>
          <span className="highscore-time">{formatDuration(entry.totalTimeMs)}</span>
          <span className="highscore-score">{entry.score}</span>
        </li>
      ))}
    </ol>
  )
}
