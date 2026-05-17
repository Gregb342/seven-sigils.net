import { useCallback, useRef, useState } from 'react'
import './App.css'
import type { Blazon, GameSettings } from './domain/models/types'
import { CreditsFooter } from './presentation/components/CreditsFooter'
import { EncyclopediaScreen } from './presentation/components/EncyclopediaScreen'
import { EndScreen } from './presentation/components/EndScreen'
import { GameScreen } from './presentation/components/GameScreen'
import { LoginScreen } from './presentation/components/LoginScreen'
import { StartScreen } from './presentation/components/StartScreen'
import { useAuth } from './presentation/hooks/useAuth'
import { useQuizController } from './presentation/hooks/useQuizController'
import { ApiClient } from './infrastructure/api/apiClient'
import { ApiBlazonRepository } from './infrastructure/repositories/ApiBlazonRepository'

const apiClient = new ApiClient()
const repository = new ApiBlazonRepository(apiClient)

type HomeView = 'menu' | 'login' | 'encyclopedia'

function App() {
  const auth = useAuth(apiClient)
  const { snapshot, loading, error, start, answer, nextRound, stop, goToMenu, resetError } =
    useQuizController(repository)

  const [homeView, setHomeView] = useState<HomeView>('menu')
  const [encyclopediaEntries, setEncyclopediaEntries] = useState<Blazon[]>([])
  const [loadingEncyclopedia, setLoadingEncyclopedia] = useState(false)
  const [encyclopediaError, setEncyclopediaError] = useState<string | null>(null)
  const lastSettingsRef = useRef<GameSettings>({
    mode: 'fixed',
    difficulty: 'easy',
    fixedRounds: 10,
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

  const openEncyclopedia = () => {
    if (!auth.isAuthenticated) {
      setHomeView('login')
      return
    }
    setHomeView('encyclopedia')
    void loadEncyclopedia()
  }

  const handleLoginSubmit = async (email: string, password: string) => {
    const success = await auth.login(email, password)
    if (success) {
      setHomeView('encyclopedia')
      void loadEncyclopedia()
    }
  }

  const backToMenu = () => {
    setHomeView('menu')
    goToMenu()
  }

  const onStart = async (
    mode: GameSettings['mode'],
    difficulty: GameSettings['difficulty'],
    fixedRounds: number,
  ) => {
    const maxFixedRounds = difficulty === 'easy' ? 30 : 40
    const safeFixedRounds = Number.isFinite(fixedRounds)
      ? Math.max(5, Math.min(maxFixedRounds, Math.round(fixedRounds)))
      : 10
    const settings: GameSettings = { mode, difficulty, fixedRounds: safeFixedRounds }
    lastSettingsRef.current = settings
    await start(settings.mode, settings.difficulty, settings.fixedRounds)
  }

  const onReplay = async () => {
    const { mode, difficulty, fixedRounds } = lastSettingsRef.current
    await start(mode, difficulty, fixedRounds)
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
                isAuthenticated={auth.isAuthenticated}
                onStart={onStart}
                onOpenEncyclopedia={openEncyclopedia}
                onLogin={() => setHomeView('login')}
                onLogout={auth.logout}
              />
            )}

            {homeView === 'login' && (
              <LoginScreen
                onLogin={handleLoginSubmit}
                onBack={() => setHomeView('menu')}
                loading={auth.loading}
                error={auth.error}
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
            onNext={nextRound}
            onStop={stop}
            onMainMenu={backToMenu}
          />
        )}

        {snapshot.status === 'finished' && (
          <EndScreen snapshot={snapshot} onReplay={onReplay} onMainMenu={backToMenu} />
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
