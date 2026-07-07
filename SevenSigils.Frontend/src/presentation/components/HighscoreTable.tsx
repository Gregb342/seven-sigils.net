import type { HighscoreEntry } from '../../domain/models/types'

interface HighscoreTableProps {
  entries: HighscoreEntry[]
  /** Index de l'entrée à mettre en évidence (score fraîchement enregistré). */
  highlightIndex?: number
}

// Les pseudos sont rendus via du JSX texte : React les échappe nativement.
// Ne jamais passer par dangerouslySetInnerHTML ici.
export function HighscoreTable({ entries, highlightIndex }: HighscoreTableProps) {
  if (entries.length === 0) {
    return <p className="highscore-empty">Aucun score enregistré pour l'instant. À toi de jouer !</p>
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
          <span className="highscore-score">{entry.score}</span>
        </li>
      ))}
    </ol>
  )
}
