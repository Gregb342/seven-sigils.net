import { useCallback, useMemo, useState } from 'react'
import { ApiError, type ApiClient } from '../../infrastructure/api/apiClient'
import {
  CitadelApi,
  type AdminBlazon,
  type BlazonWritePayload,
} from '../../infrastructure/api/citadelApi'
import { BlazonForm } from './BlazonForm'
import { QuotesPanel } from './QuotesPanel'

interface CitadelScreenProps {
  apiClient: ApiClient
}

type View = { kind: 'list' } | { kind: 'create' } | { kind: 'edit'; blazon: AdminBlazon }

// Back-office « citadel ». Le token JWT vit uniquement en mémoire :
// F5 = re-login, et rien n'est exposé dans le localStorage.
export function CitadelScreen({ apiClient }: CitadelScreenProps) {
  const api = useMemo(() => new CitadelApi(apiClient), [apiClient])

  const [authenticated, setAuthenticated] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [blazons, setBlazons] = useState<AdminBlazon[]>([])
  const [view, setView] = useState<View>({ kind: 'list' })
  const [section, setSection] = useState<'blazons' | 'quotes'>('blazons')
  const [search, setSearch] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const logout = useCallback(() => {
    api.logout()
    setAuthenticated(false)
    setPassword('')
    setBlazons([])
    setView({ kind: 'list' })
  }, [api])

  // Toute erreur passe ici : un 401 (token expiré) renvoie au login.
  const handleError = useCallback(
    (e: unknown, fallback: string) => {
      if (e instanceof ApiError && e.status === 401) {
        logout()
        setError('Session expirée — reconnecte-toi.')
        return
      }
      if (e instanceof ApiError && e.status === 403) {
        setError('Accès refusé : ce compte n’a pas le rôle Admin.')
        return
      }
      setError(e instanceof ApiError ? `${fallback} (HTTP ${e.status}) : ${e.message}` : fallback)
    },
    [logout],
  )

  const reload = useCallback(async () => {
    const all = await api.fetchAllBlazons()
    all.sort((a, b) => a.familySlug.localeCompare(b.familySlug))
    setBlazons(all)
  }, [api])

  const login = async () => {
    setBusy(true)
    setError(null)
    try {
      const result = await api.login(email.trim(), password)
      if (!result.roles.includes('Admin')) {
        api.logout()
        setError('Ce compte n’a pas le rôle Admin.')
        return
      }
      setAuthenticated(true)
      setPassword('')
      await reload()
    } catch (e) {
      if (e instanceof ApiError && e.status === 401) {
        setError('Email ou mot de passe incorrect.')
      } else {
        handleError(e, 'Connexion impossible.')
      }
    } finally {
      setBusy(false)
    }
  }

  const save = async (payload: BlazonWritePayload) => {
    setBusy(true)
    setError(null)
    try {
      if (view.kind === 'edit') {
        // Le slug n'est pas modifiable en édition (input disabled) : il identifie la ressource.
        const { familySlug, ...updatePayload } = payload
        await api.updateBlazon(familySlug, updatePayload)
      } else {
        await api.createBlazon(payload)
      }
      await reload()
      setView({ kind: 'list' })
    } catch (e) {
      handleError(e, 'Enregistrement impossible.')
    } finally {
      setBusy(false)
    }
  }

  const remove = async (blazon: AdminBlazon) => {
    if (!window.confirm(`Supprimer définitivement « ${blazon.familyLabel} » (${blazon.familySlug}) ?`)) {
      return
    }
    setBusy(true)
    setError(null)
    try {
      await api.deleteBlazon(blazon.familySlug)
      await reload()
    } catch (e) {
      handleError(e, 'Suppression impossible.')
    } finally {
      setBusy(false)
    }
  }

  if (!authenticated) {
    return (
      <section className="card citadel-login">
        <p className="eyebrow">Seven Sigils — Citadel</p>
        <h1>Connexion administrateur</h1>
        <div className="settings-grid">
          <label>
            Email
            <input type="email" value={email} autoComplete="username" onChange={(e) => setEmail(e.target.value)} />
          </label>
          <label>
            Mot de passe
            <input
              type="password"
              value={password}
              autoComplete="current-password"
              onChange={(e) => setPassword(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter') void login()
              }}
            />
          </label>
        </div>
        {error && (
          <div className="error-banner" role="alert">
            <p>{error}</p>
          </div>
        )}
        <button type="button" className="primary-btn" onClick={() => void login()} disabled={busy || email.trim() === '' || password === ''}>
          {busy ? 'Connexion…' : 'Se connecter'}
        </button>
      </section>
    )
  }

  if (view.kind !== 'list') {
    return (
      <BlazonForm
        initial={view.kind === 'edit' ? view.blazon : null}
        submitting={busy}
        error={error}
        onSubmit={(payload) => void save(payload)}
        onCancel={() => {
          setError(null)
          setView({ kind: 'list' })
        }}
      />
    )
  }

  const visible = blazons.filter(
    (b) =>
      b.familySlug.toLowerCase().includes(search.toLowerCase()) ||
      b.familyLabel.toLowerCase().includes(search.toLowerCase()),
  )

  return (
    <section className="card citadel-list">
      <p className="eyebrow">Seven Sigils — Citadel</p>

      <div className="mode-switch" role="tablist" aria-label="Section d'administration">
        <button
          type="button"
          role="tab"
          aria-selected={section === 'blazons'}
          className={section === 'blazons' ? 'mode-tab mode-tab--active' : 'mode-tab'}
          onClick={() => setSection('blazons')}
        >
          Blasons
        </button>
        <button
          type="button"
          role="tab"
          aria-selected={section === 'quotes'}
          className={section === 'quotes' ? 'mode-tab mode-tab--active' : 'mode-tab'}
          onClick={() => setSection('quotes')}
        >
          Citations
        </button>
      </div>

      {error && (
        <div className="error-banner" role="alert">
          <p>{error}</p>
          <button type="button" className="ghost-btn" onClick={() => setError(null)}>
            Fermer
          </button>
        </div>
      )}

      {section === 'quotes' ? (
        <>
          <QuotesPanel api={api} busy={busy} setBusy={setBusy} onError={handleError} />
          <div className="citadel-toolbar">
            <button type="button" className="ghost-btn" onClick={logout}>
              Se déconnecter
            </button>
          </div>
        </>
      ) : (
        <>
          <h1>Blasons ({blazons.length})</h1>

          <div className="citadel-toolbar">
            <input
              type="search"
              value={search}
              placeholder="Filtrer par slug ou label…"
              onChange={(e) => setSearch(e.target.value)}
            />
            <button type="button" className="primary-btn" onClick={() => setView({ kind: 'create' })} disabled={busy}>
              Nouveau blason
            </button>
            <button type="button" className="ghost-btn" onClick={logout}>
              Se déconnecter
            </button>
          </div>

          <ul className="citadel-rows">
            {visible.map((blazon) => (
              <li key={blazon.id} className="citadel-row">
                <span className="citadel-row-slug">{blazon.familySlug}</span>
                <span className="citadel-row-label">{blazon.familyLabel}</span>
                <span className="citadel-row-flags">
                  {blazon.includeInEasy ? 'facile' : ''} {blazon.includeInHard ? 'difficile' : ''}
                </span>
                <span className="citadel-row-actions">
                  <button type="button" className="ghost-btn" onClick={() => setView({ kind: 'edit', blazon })} disabled={busy}>
                    Modifier
                  </button>
                  <button type="button" className="ghost-btn citadel-danger" onClick={() => void remove(blazon)} disabled={busy}>
                    Supprimer
                  </button>
                </span>
              </li>
            ))}
          </ul>
          {visible.length === 0 && <p>Aucun blason ne correspond au filtre.</p>}
        </>
      )}
    </section>
  )
}
