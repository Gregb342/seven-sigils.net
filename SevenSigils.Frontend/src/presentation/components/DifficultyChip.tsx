import type { Difficulty } from '../../domain/models/types'

// Pastille de difficulté : la difficulté impacte le jeu, elle doit être
// visible partout où un score est affiché ou classé.
export function DifficultyChip({ difficulty }: { difficulty: Difficulty }) {
  return (
    <span className={`difficulty-chip difficulty-chip--${difficulty}`}>
      {difficulty === 'easy' ? 'Facile' : 'Difficile'}
    </span>
  )
}
