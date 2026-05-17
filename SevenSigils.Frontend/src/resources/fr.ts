import type { AppResource } from './types'

export const fr: AppResource = {
  hero: {
    title: 'Seven Sigils',
    subtitle:
      "Quiz héraldique Game of Thrones en refonte fullstack .NET 10 + React. L'API quiz est en place, MongoDB est branché, l'auth JWT est en cours d'industrialisation.",
    primaryCta: 'Ouvrir Swagger',
    secondaryCta: 'Voir les blasons',
    status: 'Langue active : français. La structure des ressources est prête pour le multilingue.',
  },
  sections: {
    api: {
      title: 'API disponible',
      description:
        'Le backend expose déjà le quiz, les endpoints auth et les health checks. Le frontend n’est pas encore connecté, mais la base contractuelle existe.',
      links: [
        { href: 'http://localhost:5000/swagger', label: 'Swagger' },
        { href: 'http://localhost:5000/health', label: 'Health check' },
      ],
    },
    roadmap: {
      title: 'Prochaines tranches',
      description:
        'Le prochain objectif fonctionnel est de brancher le frontend sur les endpoints auth et quiz, puis d’ouvrir le catalogue et l’administration.',
      items: [
        'Connexion frontend vers l’API et gestion du JWT',
        'Catalogue des blazons avec pagination',
        'Endpoints d’administration sécurisés',
      ],
    },
  },
}