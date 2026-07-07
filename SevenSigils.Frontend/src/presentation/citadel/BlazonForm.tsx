import { useState } from 'react'
import type { AdminBlazon, AdminHint, BlazonWritePayload } from '../../infrastructure/api/citadelApi'

interface BlazonFormProps {
  /** null = création, sinon édition (le slug devient non modifiable). */
  initial: AdminBlazon | null
  submitting: boolean
  error: string | null
  onSubmit: (payload: BlazonWritePayload) => void
  onCancel: () => void
}

const DEFAULT_LICENSE_LABEL = 'CC BY-SA 4.0'
const DEFAULT_LICENSE_URL = 'https://creativecommons.org/licenses/by-sa/4.0/'

export function BlazonForm({ initial, submitting, error, onSubmit, onCancel }: BlazonFormProps) {
  const isEdit = initial !== null

  const [familySlug, setFamilySlug] = useState(initial?.familySlug ?? '')
  const [familyLabel, setFamilyLabel] = useState(initial?.familyLabel ?? '')
  const [displayName, setDisplayName] = useState(initial?.displayName ?? '')
  const [housePageUrl, setHousePageUrl] = useState(initial?.housePageUrl ?? '')
  const [kind, setKind] = useState(initial?.kind ?? '')
  const [variantOf, setVariantOf] = useState(initial?.variantOf ?? '')
  const [includeInEasy, setIncludeInEasy] = useState(initial?.includeInEasy ?? false)
  const [includeInHard, setIncludeInHard] = useState(initial?.includeInHard ?? true)
  const [hints, setHints] = useState<AdminHint[]>(initial?.hints ?? [])
  const [author, setAuthor] = useState(initial?.attribution.author ?? 'Evrach')
  const [sourcePageUrl, setSourcePageUrl] = useState(initial?.attribution.sourcePageUrl ?? '')
  const [licenseLabel, setLicenseLabel] = useState(initial?.attribution.licenseLabel ?? DEFAULT_LICENSE_LABEL)
  const [licenseUrl, setLicenseUrl] = useState(initial?.attribution.licenseUrl ?? DEFAULT_LICENSE_URL)
  const [notes, setNotes] = useState(initial?.attribution.notes ?? '')

  const updateHint = (index: number, patch: Partial<AdminHint>) => {
    setHints((current) => current.map((h, i) => (i === index ? { ...h, ...patch } : h)))
  }

  const canSubmit =
    familySlug.trim() !== '' &&
    familyLabel.trim() !== '' &&
    housePageUrl.trim() !== '' &&
    !submitting

  const submit = () => {
    onSubmit({
      familySlug: familySlug.trim(),
      familyLabel: familyLabel.trim(),
      displayName: displayName.trim() === '' ? null : displayName.trim(),
      housePageUrl: housePageUrl.trim(),
      kind: kind.trim() === '' ? null : kind.trim(),
      variantOf: variantOf.trim() === '' ? null : variantOf.trim(),
      includeInEasy,
      includeInHard,
      hints: hints.filter((h) => h.title.trim() !== '' || h.value.trim() !== ''),
      attribution: {
        author: author.trim() === '' ? null : author.trim(),
        // Par défaut, la page source est la page de la maison.
        sourcePageUrl: sourcePageUrl.trim() === '' ? housePageUrl.trim() : sourcePageUrl.trim(),
        licenseLabel: licenseLabel.trim(),
        licenseUrl: licenseUrl.trim(),
        notes: notes.trim() === '' ? null : notes.trim(),
      },
    })
  }

  return (
    <section className="card citadel-form">
      <h2>{isEdit ? `Modifier « ${initial.familyLabel} »` : 'Nouveau blason'}</h2>

      <div className="settings-grid">
        <label>
          Slug (minuscules, chiffres, tirets)
          <input
            type="text"
            value={familySlug}
            disabled={isEdit}
            onChange={(e) => setFamilySlug(e.target.value)}
            placeholder="stark"
          />
        </label>
        <label>
          Label de la famille *
          <input type="text" value={familyLabel} onChange={(e) => setFamilyLabel(e.target.value)} placeholder="Stark" />
        </label>
        <label>
          Nom affiché (optionnel)
          <input type="text" value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
        </label>
        <label>
          Page de la maison (URL) *
          <input type="text" value={housePageUrl} onChange={(e) => setHousePageUrl(e.target.value)} placeholder="https://lagardedenuit.com/..." />
        </label>
        <label>
          Kind (optionnel)
          <input type="text" value={kind} onChange={(e) => setKind(e.target.value)} />
        </label>
        <label>
          Variante de (slug, optionnel)
          <input type="text" value={variantOf} onChange={(e) => setVariantOf(e.target.value)} />
        </label>
      </div>

      <div className="citadel-checks">
        <label>
          <input type="checkbox" checked={includeInEasy} onChange={(e) => setIncludeInEasy(e.target.checked)} />
          Mode facile
        </label>
        <label>
          <input type="checkbox" checked={includeInHard} onChange={(e) => setIncludeInHard(e.target.checked)} />
          Mode difficile
        </label>
      </div>

      <h3>Indices</h3>
      {hints.map((hint, index) => (
        <div className="citadel-hint-row" key={index}>
          <input
            type="text"
            value={hint.title}
            placeholder="Titre"
            onChange={(e) => updateHint(index, { title: e.target.value })}
          />
          <input
            type="text"
            value={hint.value}
            placeholder="Valeur"
            onChange={(e) => updateHint(index, { value: e.target.value })}
          />
          <button
            type="button"
            className="ghost-btn"
            onClick={() => setHints((current) => current.filter((_, i) => i !== index))}
          >
            Retirer
          </button>
        </div>
      ))}
      <button type="button" className="ghost-btn" onClick={() => setHints((current) => [...current, { title: '', value: '' }])}>
        Ajouter un indice
      </button>

      <h3>Attribution</h3>
      <div className="settings-grid">
        <label>
          Auteur
          <input type="text" value={author} onChange={(e) => setAuthor(e.target.value)} />
        </label>
        <label>
          Page source (URL — vide = page de la maison)
          <input type="text" value={sourcePageUrl} onChange={(e) => setSourcePageUrl(e.target.value)} />
        </label>
        <label>
          Licence *
          <input type="text" value={licenseLabel} onChange={(e) => setLicenseLabel(e.target.value)} />
        </label>
        <label>
          URL de licence *
          <input type="text" value={licenseUrl} onChange={(e) => setLicenseUrl(e.target.value)} />
        </label>
        <label>
          Notes
          <input type="text" value={notes} onChange={(e) => setNotes(e.target.value)} />
        </label>
      </div>

      {error && (
        <div className="error-banner" role="alert">
          <p>{error}</p>
        </div>
      )}

      <div className="citadel-form-actions">
        <button type="button" className="primary-btn" onClick={submit} disabled={!canSubmit}>
          {submitting ? 'Enregistrement…' : isEdit ? 'Enregistrer' : 'Créer'}
        </button>
        <button type="button" className="ghost-btn" onClick={onCancel} disabled={submitting}>
          Annuler
        </button>
      </div>
    </section>
  )
}
