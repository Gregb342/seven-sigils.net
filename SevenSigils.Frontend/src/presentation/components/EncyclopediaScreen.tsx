import { useEffect, useMemo, useState } from 'react'
import type { Blazon } from '../../domain/models/types'

interface EncyclopediaScreenProps {
  entries: Blazon[]
  loading: boolean
  error: string | null
  onBack: () => void
}

function toDisplayName(entry: Blazon): string {
  return entry.displayName ?? `Maison ${entry.familyLabel}`
}

function toKindLabel(kind: Blazon['kind']): string {
  if (kind === 'special') return 'Spécial'
  if (kind === 'variant') return 'Variante'
  return 'Maison'
}

function getSectionKey(label: string): string {
  const firstLetter = label.charAt(0).toLocaleUpperCase('fr')
  return /^[A-Z]$/.test(firstLetter) ? firstLetter : '#'
}

export function EncyclopediaScreen({ entries, loading, error, onBack }: EncyclopediaScreenProps) {
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 4

  const filtered = useMemo(() => {
    const normalizedQuery = query.trim().toLocaleLowerCase('fr')
    if (!normalizedQuery) return entries
    return entries.filter((entry) => {
      const searchable = [entry.familySlug, entry.familyLabel, entry.displayName ?? '']
        .join(' ')
        .toLocaleLowerCase('fr')
      return searchable.includes(normalizedQuery)
    })
  }, [entries, query])

  useEffect(() => {
    setPage(1)
  }, [query, entries])

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize))
  const safePage = Math.min(page, totalPages)
  const pageStart = (safePage - 1) * pageSize
  const paginated = filtered.slice(pageStart, pageStart + pageSize)

  const letterToFirstIndex = useMemo(() => {
    const map = new Map<string, number>()
    filtered.forEach((entry, index) => {
      const key = getSectionKey(entry.familyLabel)
      if (!map.has(key)) map.set(key, index)
    })
    return map
  }, [filtered])

  const availableLetters = useMemo(
    () => [...letterToFirstIndex.keys()].sort((a, b) => a.localeCompare(b, 'fr')),
    [letterToFirstIndex],
  )

  const sections = useMemo(() => {
    const grouped = new Map<string, Blazon[]>()
    for (const entry of paginated) {
      const sectionKey = getSectionKey(entry.familyLabel)
      const current = grouped.get(sectionKey) ?? []
      current.push(entry)
      grouped.set(sectionKey, current)
    }
    return [...grouped.entries()].sort(([a], [b]) => a.localeCompare(b, 'fr'))
  }, [paginated])

  return (
    <section className="card encyclopedia-card" aria-labelledby="encyclopedia-title">
      <header className="encyclopedia-header">
        <div>
          <p className="eyebrow">Référence</p>
          <h1 id="encyclopedia-title">Encyclopédie des blasons</h1>
        </div>
        <button type="button" className="ghost-btn" onClick={onBack}>
          Retour à l'accueil
        </button>
      </header>

      <label className="encyclopedia-search">
        Rechercher un blason
        <input
          type="search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Ex : stark, lannister..."
          aria-label="Rechercher un blason"
        />
      </label>

      {loading && <p>Chargement de l'encyclopédie...</p>}
      {error && <p role="alert">{error}</p>}

      {!loading && !error && (
        <>
          <p className="encyclopedia-count">{filtered.length} résultat(s)</p>

          {filtered.length === 0 && <p>Aucun blason ne correspond à cette recherche.</p>}

          {filtered.length > 0 && (
            <>
              <div className="encyclopedia-layout">
                <div>
                  {sections.map(([letter, items]) => (
                    <section key={letter} className="encyclopedia-section" aria-label={`Section ${letter}`}>
                      <h2>{letter}</h2>
                      <div className="encyclopedia-grid">
                        {items.map((entry) => {
                          const regionHint = entry.hints.find((hint) =>
                            hint.title.toLocaleLowerCase('fr').includes('region'),
                          )

                          return (
                            <article key={entry.id} className="encyclopedia-item">
                              <img
                                src={entry.imageUrl}
                                alt={`Blason ${toDisplayName(entry)}`}
                                onError={(e) => {
                                  e.currentTarget.style.display = 'none'
                                }}
                              />
                              <h3>{toDisplayName(entry)}</h3>
                              <p>
                                <strong>Type :</strong> {toKindLabel(entry.kind)}
                              </p>
                              <p>
                                <strong>Région :</strong> {regionHint?.value ?? 'Non renseignée'}
                              </p>

                              {entry.hints.length > 0 && (
                                <div className="encyclopedia-hints">
                                  <strong>Indices :</strong>
                                  <ul>
                                    {entry.hints.map((hint) => (
                                      <li key={`${entry.id}-${hint.title}`}>
                                        {hint.title} : {hint.value}
                                      </li>
                                    ))}
                                  </ul>
                                </div>
                              )}

                              <p>
                                <strong>Source :</strong>{' '}
                                <a
                                  href={entry.housePageUrl}
                                  target="_blank"
                                  rel="noopener noreferrer"
                                >
                                  article La Garde de Nuit
                                </a>
                              </p>

                              <p>
                                <em>
                                  {entry.attribution.author ?? 'Auteur inconnu'} —{' '}
                                  <a
                                    href={entry.attribution.licenseUrl}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                  >
                                    {entry.attribution.licenseLabel}
                                  </a>
                                </em>
                              </p>
                            </article>
                          )
                        })}
                      </div>
                    </section>
                  ))}
                </div>

                {availableLetters.length > 1 && (
                  <nav className="encyclopedia-letter-rail" aria-label="Ascenseur alphabétique">
                    {availableLetters.map((letter) => {
                      const firstIndex = letterToFirstIndex.get(letter) ?? 0
                      const targetPage = Math.floor(firstIndex / pageSize) + 1
                      const isActivePage = targetPage === safePage

                      return (
                        <button
                          key={letter}
                          type="button"
                          className="ghost-btn"
                          aria-label={`Aller à la lettre ${letter}`}
                          aria-current={isActivePage ? 'page' : undefined}
                          onClick={() => setPage(targetPage)}
                        >
                          {letter}
                        </button>
                      )
                    })}
                  </nav>
                )}
              </div>

              {filtered.length > pageSize && (
                <nav className="encyclopedia-pagination" aria-label="Pagination des blasons">
                  <button
                    type="button"
                    className="ghost-btn"
                    onClick={() => setPage((current) => Math.max(1, current - 1))}
                    disabled={safePage <= 1}
                  >
                    Page précédente
                  </button>

                  <span>
                    Page {safePage} / {totalPages}
                  </span>

                  <button
                    type="button"
                    className="ghost-btn"
                    onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
                    disabled={safePage >= totalPages}
                  >
                    Page suivante
                  </button>
                </nav>
              )}
            </>
          )}
        </>
      )}
    </section>
  )
}
