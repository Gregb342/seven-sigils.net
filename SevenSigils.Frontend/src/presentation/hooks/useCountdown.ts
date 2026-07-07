import { useCallback, useEffect, useRef, useState } from 'react'

interface Countdown {
  /** Arme le compte à rebours (réarme s'il tournait déjà). */
  start: () => void
  stop: () => void
  running: boolean
  remainingMs: number
  /** 1 → 0 : fraction restante, pilote la barre et les états de stress. */
  ratio: number
  elapsedMs: number
}

/**
 * Compte à rebours basé sur une deadline `Date.now()` (pas de décrément par tick :
 * aucune dérive, et un onglet mis en arrière-plan ne « gèle » pas le temps).
 * Rafraîchi par requestAnimationFrame ; `onExpire` est appelé une seule fois.
 */
export function useCountdown(durationMs: number, onExpire: () => void): Countdown {
  const [deadline, setDeadline] = useState<number | null>(null)
  const [now, setNow] = useState(() => Date.now())
  const onExpireRef = useRef(onExpire)
  useEffect(() => {
    onExpireRef.current = onExpire
  })

  useEffect(() => {
    if (deadline === null) return
    let frame: number
    const tick = () => {
      const current = Date.now()
      setNow(current)
      if (current >= deadline) {
        setDeadline(null)
        onExpireRef.current()
        return
      }
      frame = requestAnimationFrame(tick)
    }
    frame = requestAnimationFrame(tick)
    return () => cancelAnimationFrame(frame)
  }, [deadline])

  const start = useCallback(() => {
    setNow(Date.now())
    setDeadline(Date.now() + durationMs)
  }, [durationMs])

  const stop = useCallback(() => setDeadline(null), [])

  const remainingMs = deadline === null ? 0 : Math.max(0, deadline - now)
  const running = deadline !== null

  return {
    start,
    stop,
    running,
    remainingMs,
    ratio: running && durationMs > 0 ? remainingMs / durationMs : 0,
    elapsedMs: running ? durationMs - remainingMs : 0,
  }
}
