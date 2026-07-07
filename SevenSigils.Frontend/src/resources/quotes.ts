import type { CompetitiveTier } from '../domain/competitiveScoring'

// Titres de rang et citations affichés en fin de partie compétitive, par palier.
// V1 en dur — une gestion via la citadel (Mongo + CRUD admin) est prévue en suivi.

interface TierContent {
  title: string
  quotes: string[]
}

export const TIER_CONTENT: Record<CompetitiveTier, TierContent> = {
  legendary: {
    title: 'Mestre de la Citadelle',
    quotes: [
      'Le savoir est une arme. Tu es redoutablement armé.',
      'Un esprit a besoin de livres comme une épée a besoin d’une pierre à aiguiser.',
      'Les archives de la Citadelle se souviendront de ton nom.',
    ],
  },
  strong: {
    title: 'Main du Roi',
    quotes: [
      'Un Lannister paie toujours ses dettes — et toi, tu honores tes blasons.',
      'Le Nord se souvient. De toi aussi, désormais.',
      'Quand on joue au jeu des blasons, on gagne ou on révise.',
    ],
  },
  average: {
    title: 'Frère juré de la Garde de Nuit',
    quotes: [
      'Ton tour de garde ne fait que commencer.',
      'La Garde a besoin d’hommes qui persévèrent. Rejoue.',
      'Ni gloire, ni honte : le Mur tient, et toi aussi.',
    ],
  },
  grim: {
    title: 'Marcheur d’hiver',
    quotes: [
      'Vous ne savez rien… mais ça se soigne, à l’encyclopédie.',
      'La nuit est sombre et pleine d’erreurs.',
      'L’hiver vient. Il est même déjà là, visiblement.',
    ],
  },
}

export function pickQuote(tier: CompetitiveTier): string {
  const pool = TIER_CONTENT[tier].quotes
  return pool[Math.floor(Math.random() * pool.length)]
}
