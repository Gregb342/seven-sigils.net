import { useEffect, useState } from 'react'
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

const TIER_LABELS: Record<CompetitiveTier, string> = {
  legendary: 'palier légendaire (≥ 85 %)',
  strong: 'palier fort (≥ 60 %)',
  average: 'palier moyen (≥ 35 %)',
  grim: 'palier faible',
}

// Gestion des titres de rang et citations de fin de partie compétitive.
// Volontairement spartiate, comme le reste de la citadel.
export function QuotesPanel({ api, busy, setBusy, onError }: QuotesPanelProps) {
  const [quotes, setQuotes] = useState<AdminQuote[]>([])
  const [titles, setTitles] = useState<Record<string, string>>({})
  const [titleDrafts, setTitleDrafts] = useState<Record<string, string>>({})
  const [drafts, setDrafts] = useState<Record<string, string>>({})
  const [newTexts, setNewTexts] = useState<Partial<Record<CompetitiveTier, string>>>({})

  const applyContent = (content: { titles: Record<string, string>; quotes: AdminQuote[] }) => {
    setQuotes(content.quotes)
    setDrafts(Object.fromEntries(content.quotes.map((q) => [q.id, q.text])))
    setTitles(content.titles)
    setTitleDrafts(
      Object.fromEntries(
        TIERS.map((tier) => [tier, content.titles[tier] ?? TIER_CONTENT[tier].title]),
      ),
    )
  }

  useEffect(() => {
    let cancelled = false
    api
      .fetchQuotes()
      .then((content) => {
        if (!cancelled) applyContent(content)
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
      applyContent(await api.fetchQuotes())
    } catch (e) {
      onError(e, fallback)
    } finally {
      setBusy(false)
    }
  }

  const saveTitle = (tier: CompetitiveTier) => {
    const title = (titleDrafts[tier] ?? '').trim()
    if (title === '' || title === (titles[tier] ?? '')) return
    void run(() => api.updateTierTitle(tier, title), 'Enregistrement du titre impossible.')
  }

  const saveQuote = (quote: AdminQuote) => {
    const text = (drafts[quote.id] ?? '').trim()
    if (text === '' || text === quote.text) return
    void run(() => api.updateQuote(quote.id, quote.tier, text), 'Enregistrement impossible.')
  }

  const removeQuote = (quote: AdminQuote) => {
    if (!window.confirm(`Supprimer cette citation ?\n« ${quote.text} »`)) return
    void run(() => api.deleteQuote(quote.id), 'Suppression impossible.')
  }

  const createQuote = (tier: CompetitiveTier) => {
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
        Titres de rang et citations affichés en fin de partie compétitive, selon le palier atteint.
      </p>

      {TIERS.map((tier) => (
        <section key={tier} className="quotes-tier">
          <h3>
            {titles[tier] ?? TIER_CONTENT[tier].title}
            <span className="quotes-tier-key"> — {TIER_LABELS[tier]}</span>
          </h3>

          <div className="citadel-hint-row">
            <input
              type="text"
              value={titleDrafts[tier] ?? ''}
              maxLength={100}
              aria-label={`Titre du rang ${tier}`}
              onChange={(e) =>
                setTitleDrafts((current) => ({ ...current, [tier]: e.target.value }))
              }
              onKeyDown={(e) => {
                if (e.key === 'Enter') saveTitle(tier)
              }}
            />
            <button
              type="button"
              className="ghost-btn"
              onClick={() => saveTitle(tier)}
              disabled={busy || (titleDrafts[tier] ?? '').trim() === (titles[tier] ?? '')}
            >
              Enregistrer le titre
            </button>
          </div>

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
                  onClick={() => saveQuote(quote)}
                  disabled={busy || (drafts[quote.id] ?? '').trim() === quote.text}
                >
                  Enregistrer
                </button>
                <button
                  type="button"
                  className="ghost-btn citadel-danger"
                  onClick={() => removeQuote(quote)}
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
                if (e.key === 'Enter') createQuote(tier)
              }}
            />
            <button
              type="button"
              className="ghost-btn"
              onClick={() => createQuote(tier)}
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
