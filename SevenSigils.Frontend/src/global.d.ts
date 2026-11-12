declare const __APP_VERSION__: string

interface ImportMetaEnv {
  /** 'false' au build de production : masque l'écran citadel (back-office absent côté API). */
  readonly VITE_CITADEL_ENABLED?: string
}
