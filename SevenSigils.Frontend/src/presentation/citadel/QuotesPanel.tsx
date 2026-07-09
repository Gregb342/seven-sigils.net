import { useCallback, useEffect, useState } from 'react'
import type { AdminQuote, CitadelApi } from '../../infrastructure/api/citadelApi'
import type { CompetitiveTier } from '../../domain/competitiveScoring'
import { TIER_CONTENT } from '../../resources/quotes'

interface QuotesPanelProps {
  api: CitadelApi
  busy: boolean
  setBusy: (value: boolean) => void
  onError: (e: unknown, fallback: string) => void
}

const TIERS: readonly CompetitiveTier[] = ['legendary', 'strong', 'average', 'grim']

// Gestion des citations de fin de partie compétitive, groupées par palier.
// Volontairement spartiate, comme le reste de la citadel.
export function QuotesPanel({ api, busy, setBusy, onError }: QuotesPanelProps) {
  const [quotes, setQuotes] = useState<AdminQuote[]>([])
  const [drafts, setDrafts] = useState<Record<string, string>>({})
  const [newTexts, setNewTexts] = useState<Partial<Record<CompetitiveTier, string>>>({})

  const reload = useCallback(async () => {
    const all = await api.fetchQuotes()
    setQuotes(all)
    setDrafts(Object.fromEntries(all.map((q) => [q.id, q.text])))
  }, [api])

  useEffect(() => {
    let cancelled = false
    api
      .fetchQuotes()
      .then((all) => {
        if (cancelled) return
        setQuotes(all)
        setDrafts(Object.fromEntries(all.map((q) => [q.id, q.text])))
      })
      .catch((e: unknown) => onError(e, 'Chargement des citations impossible.'))
    return () => {
      cancelled = true
    }
  }, [api, onError])

  const run = async (action: () => Promise<unknown>, fallback: string) => {
    setBusy(true)
    try {
      await action()
      await reload()
    } catch (e) {
      onError(e, fallback)
    } finally {
      setBusy(false)
    }
  }

  const save = (quote: AdminQuote) => {
    const text = (drafts[quote.id] ?? '').trim()
    if (text === '' || text === quote.text) return
    void run(() => api.updateQuote(quote.id, quote.tier, text), 'Enregistrement impossible.')
  }

  const remove = (quote: AdminQuote) => {
    if (!window.confirm(`Supprimer cette citation ?\n« ${quote.text} »`)) return
    void run(() => api.deleteQuote(quote.id), 'Suppression impossible.')
  }

  const create = (tier: CompetitiveTier) => {
    const text = (newTexts[tier] ?? '').trim()
    if (text === '') return
    void run(async () => {
      await api.createQuote(tier, text)
      setNewTexts((current) => ({ ...current, [tier]: '' }))
    }, 'Création impossible.')
  }

  return (
    <div className="quotes-panel">
      <p className="format-badge">
        Citations affichées en fin de partie compétitive, selon le palier atteint.
      </p>

      {TIERS.map((tier) => (
        <section key={tier} className="quotes-tier">
          <h3>
            {TIER_CONTENT[tier].title}
            <span className="quotes-tier-key"> ({tier})</span>
          </h3>

          {quotes
            .filter((q) => q.tier === tier)
            .map((quote) => (
              <div key={quote.id} className="citadel-hint-row">
                <input
                  type="text"
                  value={drafts[quote.id] ?? ''}
                  maxLength={300}
                  onChange={(e) =>
                    setDrafts((current) => ({ ...current, [quote.id]: e.target.value }))
                  }
                />
                <button
                  type="button"
                  className="ghost-btn"
                  onClick={() => save(quote)}
                  disabled={busy || (drafts[quote.id] ?? '').trim() === quote.text}
                >
                  Enregistrer
                </button>
                <button
                  type="button"
                  className="ghost-btn citadel-danger"
                  onClick={() => remove(quote)}
                  disabled={busy}
                >
                  Supprimer
                </button>
              </div>
            ))}

          <div className="citadel-hint-row">
            <input
              type="text"
              placeholder="Nouvelle citation…"
              value={newTexts[tier] ?? ''}
              maxLength={300}
              onChange={(e) => setNewTexts((current) => ({ ...current, [tier]: e.target.value }))}
              onKeyDown={(e) => {
                if (e.key === 'Enter') create(tier)
              }}
            />
            <button
              type="button"
              className="ghost-btn"
              onClick={() => create(tier)}
              disabled={busy || (newTexts[tier] ?? '').trim() === ''}
            >
              Ajouter
            </button>
          </div>
        </section>
      ))}
    </div>
  )
}
