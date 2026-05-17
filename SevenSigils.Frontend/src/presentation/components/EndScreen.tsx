import type { SessionSnapshot } from '../../domain/models/types'

interface EndScreenProps {
  snapshot: SessionSnapshot
  onReplay: () => Promise<void>
  onMainMenu: () => void
}

export function EndScreen({ snapshot, onReplay, onMainMenu }: EndScreenProps) {
  return (
    <section className="card end-card">
      <p className="eyebrow">Partie terminée</p>
      <h2>Ton score final : {snapshot.score}</h2>
      <p>Meilleur score local : {snapshot.bestScore}</p>
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
