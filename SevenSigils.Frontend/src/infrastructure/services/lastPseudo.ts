import { isValidPseudo } from '../../domain/pseudo'

// Dernier pseudo utilisé, partagé entre les classements classique et compétitif.
const LAST_PSEUDO_KEY = 'seven_sigils_last_pseudo'

export function readLastPseudo(): string {
  try {
    const raw = window.localStorage.getItem(LAST_PSEUDO_KEY)
    return raw !== null && isValidPseudo(raw) ? raw : ''
  } catch {
    return ''
  }
}

export function saveLastPseudo(pseudo: string): void {
  try {
    window.localStorage.setItem(LAST_PSEUDO_KEY, pseudo)
  } catch {
    // Échec non bloquant (storage plein, mode privé…).
  }
}
