import { useCallback, useEffect, useRef, useState } from 'react'
import './App.css'
import type { Blazon, GameSettings } from './domain/models/types'
import { OFFICIAL_ROUNDS, OFFICIAL_TIMER_SECONDS } from './domain/competitiveScoring'
import { CreditsFooter } from './presentation/components/CreditsFooter'
import { EncyclopediaScreen } from './presentation/components/EncyclopediaScreen'
import { EndScreen } from './presentation/components/EndScreen'
import { GameScreen } from './presentation/components/GameScreen'
import { StartScreen } from './presentation/components/StartScreen'
import { CitadelScreen } from './presentation/citadel/CitadelScreen'
import { useQuizController } from './presentation/hooks/useQuizController'
import { ApiClient } from './infrastructure/api/apiClient'
import { ApiBlazonRepository } from './infrastructure/repositories/ApiBlazonRepository'
import { ApiQuotesRepository } from './infrastructure/repositories/ApiQuotesRepository'
import { LocalStorageHighscoreStore } from './infrastructure/services/LocalStorageHighscoreStore'
import { LocalStorageCompetitiveScoreStore } from './infrastructure/services/LocalStorageCompetitiveScoreStore'

const apiClient = new ApiClient()
const repository = new ApiBlazonRepository(apiClient)
const quotesRepository = new ApiQuotesRepository(apiClient)
const highscoreStore = new LocalStorageHighscoreStore()
const competitiveStore = new LocalStorageCompetitiveScoreStore()

type HomeView = 'menu' | 'encyclopedia'

// Back-office accessible via /#citadel ou /citadel (le fallback SPA de nginx sert
// index.html pour les deux) — volontairement sans lien dans l'UI joueur.
// Ce n'est pas une protection (l'API reste le vrai garde), juste de la discrétion.
const isCitadelLocation = () =>
  window.location.hash === '#citadel' || window.location.pathname === '/citadel'

function App() {
  const [isCitadel, setIsCitadel] = useState(isCitadelLocation)
  useEffect(() => {
    const onHashChange = () => setIsCitadel(isCitadelLocation())
    window.addEventListener('hashchange', onHashChange)
    return () => window.removeEventListener('hashchange', onHashChange)
  }, [])

  const {
    snapshot,
    loading,
    error,
    start,
    answer,
    timeout,
    useHint,
    nextRound,
    stop,
    goToMenu,
    resetError,
  } = useQuizController(repository)

  const [homeView, setHomeView] = useState<HomeView>('menu')
  const [encyclopediaEntries, setEncyclopediaEntries] = useState<Blazon[]>([])
  const [loadingEncyclopedia, setLoadingEncyclopedia] = useState(false)
  const [encyclopediaError, setEncyclopediaError] = useState<string | null>(null)
  const lastSettingsRef = useRef<GameSettings>({
    gameType: 'classic',
    mode: 'fixed',
    difficulty: 'easy',
    fixedRounds: OFFICIAL_ROUNDS,
    timerSeconds: OFFICIAL_TIMER_SECONDS,
  })

  const loadEncyclopedia = useCallback(async () => {
    if (encyclopediaEntries.length > 0 || loadingEncyclopedia) return
    setEncyclopediaError(null)
    setLoadingEncyclopedia(true)
    try {
      const result = await repository.fetchPage(1, 100)
      setEncyclopediaEntries(result.items)
    } catch (e) {
      setEncyclopediaError(e instanceof Error ? e.message : 'Erreur inconnue')
    } finally {
      setLoadingEncyclopedia(false)
    }
  }, [encyclopediaEntries.length, loadingEncyclopedia])

  // L'encyclopédie est publique : accès direct, sans authentification.
  const openEncyclopedia = () => {
    setHomeView('encyclopedia')
    void loadEncyclopedia()
  }

  const backToMenu = () => {
    setHomeView('menu')
    goToMenu()
  }

  const onStart = async (settings: GameSettings) => {
    // Les bornes fines (manches, timer) sont garanties par QuizGameService.
    lastSettingsRef.current = settings
    await start(settings)
  }

  const onReplay = async () => {
    await start(lastSettingsRef.current)
  }

  if (isCitadel) {
    return (
      <div className="app-shell">
        <main className="main-content">
          <CitadelScreen apiClient={apiClient} />
        </main>
        <CreditsFooter />
      </div>
    )
  }

  return (
    <div className="app-shell">
      <main className="main-content">
        {snapshot.status === 'idle' && (
          <>
            {homeView === 'menu' && (
              <StartScreen
                bestScore={snapshot.bestScore}
                loading={loading}
                highscoreStore={highscoreStore}
                competitiveStore={competitiveStore}
                onStart={onStart}
                onOpenEncyclopedia={openEncyclopedia}
              />
            )}

            {homeView === 'encyclopedia' && (
              <EncyclopediaScreen
                entries={encyclopediaEntries}
                loading={loadingEncyclopedia}
                error={encyclopediaError}
                onBack={() => setHomeView('menu')}
              />
            )}
          </>
        )}

        {snapshot.status === 'running' && (
          <GameScreen
            key={snapshot.question?.blazon.id ?? 'no-question'}
            snapshot={snapshot}
            onAnswer={answer}
            onTimeout={timeout}
            onHint={useHint}
            onNext={nextRound}
            onStop={stop}
            onMainMenu={backToMenu}
          />
        )}

        {snapshot.status === 'finished' && (
          <EndScreen
            snapshot={snapshot}
            highscoreStore={highscoreStore}
            competitiveStore={competitiveStore}
            quotesRepository={quotesRepository}
            onReplay={onReplay}
            onMainMenu={backToMenu}
          />
        )}

        {error && (
          <div className="error-banner" role="alert">
            <p>{error}</p>
            <button type="button" className="ghost-btn" onClick={resetError}>
              Fermer
            </button>
          </div>
        )}
      </main>

      <CreditsFooter />
    </div>
  )
}

export default App
