// Règles du pseudo arcade : lettres et chiffres uniquement, 20 caractères max.
// Seule source de vérité — utilisée à la saisie ET à la relecture du localStorage
// (défense en profondeur : le storage est librement éditable par l'utilisateur).

export const PSEUDO_MAX_LENGTH = 20

const PSEUDO_PATTERN = /^[A-Za-z0-9]{1,20}$/

export function isValidPseudo(value: string): boolean {
  return PSEUDO_PATTERN.test(value)
}

/** Filtre une saisie libre vers un pseudo valide : caractères hors [A-Za-z0-9] retirés, tronqué à 20. */
export function sanitizePseudo(raw: string): string {
  return raw.replace(/[^A-Za-z0-9]/g, '').slice(0, PSEUDO_MAX_LENGTH)
}
