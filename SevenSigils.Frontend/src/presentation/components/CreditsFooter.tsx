import { APP_VERSION } from '../../version'

export function CreditsFooter() {
  const currentYear = new Date().getFullYear()

  return (
    <footer className="credits" aria-label="Crédits et licence">
      <p>
        Images de blasons issues du wiki de{' '}
        <a
          href="https://lagardedenuit.com/wiki/index.php?title=Accueil"
          target="_blank"
          rel="noopener noreferrer"
        >
          La Garde de Nuit
        </a>
        . Contenu sous licence{' '}
        <a
          href="https://creativecommons.org/licenses/by-sa/4.0/"
          target="_blank"
          rel="noopener noreferrer"
        >
          Creative Commons BY-SA 4.0
        </a>{' '}
        sauf mention contraire.
      </p>
      <p>
        Copyright {currentYear} Seven Sigils — Grégoire Bouteillier —{' '}
        <a href="https://github.com/Gregb342/" target="_blank" rel="noopener noreferrer">
          github.com/Gregb342
        </a>
      </p>
      <p>Version : {APP_VERSION}</p>
    </footer>
  )
}
