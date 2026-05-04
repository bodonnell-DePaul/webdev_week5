import { useCallback, useEffect, useMemo, useState } from 'react'
import './App.css'
import { beginLogin, clearSession, completeLogin, getApiBaseUrl, getSession } from './services/auth'
import { fetchParks } from './services/parks'
import type { NationalPark, Session } from './types/park'

function App() {
  const [path, setPath] = useState(window.location.pathname)
  const [session, setSession] = useState<Session | null>(() => getSession())

  useEffect(() => {
    const onPopState = () => setPath(window.location.pathname)
    window.addEventListener('popstate', onPopState)
    return () => window.removeEventListener('popstate', onPopState)
  }, [])

  const navigate = useCallback((nextPath: string) => {
    window.history.pushState(null, '', nextPath)
    setPath(nextPath)
  }, [])

  const signOut = useCallback(() => {
    clearSession()
    setSession(null)
    navigate('/')
  }, [navigate])

  return (
    <div className="app-shell">
      <Header session={session} onNavigate={navigate} onLogin={beginLogin} onLogout={signOut} />
      <main>
        {path === '/callback' && <CallbackPage onSession={setSession} onNavigate={navigate} />}
        {path === '/parks' && (
          <GuardedPage session={session} onLogin={beginLogin}>
            <ParksPage session={session} />
          </GuardedPage>
        )}
        {path === '/dashboard' && (
          <GuardedPage session={session} onLogin={beginLogin}>
            <DashboardPage session={session} />
          </GuardedPage>
        )}
        {path === '/about' && <AboutPage />}
        {!['/callback', '/parks', '/dashboard', '/about'].includes(path) && (
          <HomePage onLogin={beginLogin} onNavigate={navigate} />
        )}
      </main>
    </div>
  )
}

interface HeaderProps {
  session: Session | null
  onNavigate: (path: string) => void
  onLogin: () => void
  onLogout: () => void
}

function Header({ session, onNavigate, onLogin, onLogout }: HeaderProps) {
  return (
    <header className="site-header">
      <button className="brand" onClick={() => onNavigate('/')}>
        OAuth Parks Explorer
      </button>
      <nav>
        <button onClick={() => onNavigate('/about')}>About</button>
        <button onClick={() => onNavigate('/dashboard')}>Dashboard</button>
        <button onClick={() => onNavigate('/parks')}>Guarded Parks</button>
      </nav>
      {session ? (
        <div className="user-chip">
          <span>{session.displayName}</span>
          <button onClick={onLogout}>Sign out</button>
        </div>
      ) : (
        <button className="primary" onClick={onLogin}>Sign in with OAuth</button>
      )}
    </header>
  )
}

function HomePage({ onLogin, onNavigate }: { onLogin: () => void; onNavigate: (path: string) => void }) {
  return (
    <section className="hero-card">
      <div>
        <p className="eyebrow">Week 5 OAuth + React + .NET 10</p>
        <h1>Explore a guarded open dataset with OAuth PKCE.</h1>
        <p>
          This example combines React components and state, RESTful .NET APIs, CORS, service-layer data access,
          input validation, and OAuth-style authorization code flow with PKCE.
        </p>
        <div className="actions">
          <button className="primary" onClick={onLogin}>Start OAuth flow</button>
          <button onClick={() => onNavigate('/about')}>Review architecture</button>
        </div>
      </div>
      <aside className="flow-card">
        <h2>OAuth flow</h2>
        <ol>
          <li>React creates a PKCE verifier and challenge.</li>
          <li>The browser visits the backend authorization endpoint.</li>
          <li>The backend returns an authorization code to React.</li>
          <li>React exchanges the code for a bearer access token.</li>
          <li>Guarded pages call protected dataset endpoints.</li>
        </ol>
      </aside>
    </section>
  )
}

function AboutPage() {
  return (
    <section className="content-card">
      <h1>About this application</h1>
      <p>
        The dataset is a small educational subset derived from public National Park Service facts about U.S.
        national parks. The protected routes require the <code>national-parks.read</code> scope.
      </p>
      <div className="topic-grid">
        <Topic title="React topics" text="Components, props, state, effects, forms, conditional rendering, and guarded client pages." />
        <Topic title="Backend topics" text="Minimal APIs, endpoint groups, CORS, OpenAPI, validation, and service dependency injection." />
        <Topic title="Security topics" text="OAuth roles, authorization code flow, PKCE, bearer tokens, scopes, and guarded resource routes." />
        <Topic title="Data topics" text="A service layer exposes open dataset records and search filters without coupling UI code to storage." />
      </div>
      <p className="note">
        This is a classroom OAuth demonstration with a local authorization server. Production apps should use a
        trusted OAuth/OIDC provider and secure browser token storage appropriate for the application.
      </p>
    </section>
  )
}

function Topic({ title, text }: { title: string; text: string }) {
  return (
    <article>
      <h2>{title}</h2>
      <p>{text}</p>
    </article>
  )
}

function GuardedPage({ session, onLogin, children }: { session: Session | null; onLogin: () => void; children: React.ReactNode }) {
  if (!session) {
    return (
      <section className="content-card locked-card">
        <h1>Guarded route</h1>
        <p>You need an OAuth access token before viewing this page.</p>
        <button className="primary" onClick={onLogin}>Sign in with OAuth</button>
      </section>
    )
  }

  return children
}

function DashboardPage({ session }: { session: Session | null }) {
  const expiresAt = session ? new Date(session.expiresAt).toLocaleTimeString() : ''
  return (
    <section className="content-card">
      <h1>Authenticated dashboard</h1>
      <div className="metric-grid">
        <Metric label="Signed in user" value={session?.displayName ?? 'Unknown'} />
        <Metric label="Granted scope" value={session?.scope ?? 'None'} />
        <Metric label="Token expires" value={expiresAt} />
        <Metric label="Backend API" value={getApiBaseUrl()} />
      </div>
    </section>
  )
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <article className="metric-card">
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  )
}

function ParksPage({ session }: { session: Session | null }) {
  const [parks, setParks] = useState<NationalPark[]>([])
  const [stateFilter, setStateFilter] = useState('')
  const [query, setQuery] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    if (!session) {
      return
    }

    fetchParks(session, stateFilter, query)
      .then(setParks)
      .catch((requestError: Error) => setError(requestError.message))
      .finally(() => setIsLoading(false))
  }, [session, stateFilter, query])

  const totalAcres = useMemo(() => parks.reduce((sum, park) => sum + park.acres, 0), [parks])

  return (
    <section className="content-card">
      <div className="section-heading">
        <div>
          <h1>Guarded National Parks dataset</h1>
          <p>Protected API route: <code>/api/parks</code></p>
        </div>
        <span className="scope-badge">national-parks.read</span>
      </div>

      <form className="filter-form" onSubmit={(event) => event.preventDefault()}>
        <label>
          State code
          <input
            maxLength={2}
            placeholder="UT"
            value={stateFilter}
            onChange={(event) => {
              setIsLoading(true)
              setError('')
              setStateFilter(event.target.value.toUpperCase())
            }}
          />
        </label>
        <label>
          Search text
          <input
            placeholder="hiking, canyon, geyser..."
            value={query}
            onChange={(event) => {
              setIsLoading(true)
              setError('')
              setQuery(event.target.value)
            }}
          />
        </label>
      </form>

      <div className="summary-line">
        {isLoading ? 'Loading parks...' : `${parks.length} parks, ${totalAcres.toLocaleString()} protected acres`}
      </div>
      {error && <p className="error-message">{error}</p>}

      <div className="park-grid">
        {parks.map((park) => <ParkCard key={park.id} park={park} />)}
      </div>
    </section>
  )
}

function ParkCard({ park }: { park: NationalPark }) {
  return (
    <article className="park-card">
      <div className="park-card-header">
        <h2>{park.name}</h2>
        <span>{park.state}</span>
      </div>
      <p>{park.highlight}</p>
      <dl>
        <div>
          <dt>Established</dt>
          <dd>{park.establishedYear}</dd>
        </div>
        <div>
          <dt>Acres</dt>
          <dd>{park.acres.toLocaleString()}</dd>
        </div>
        <div>
          <dt>Region</dt>
          <dd>{park.region}</dd>
        </div>
      </dl>
      <div className="tag-list">
        {park.activities.map((activity) => <span key={activity}>{activity}</span>)}
      </div>
    </article>
  )
}

function CallbackPage({ onSession, onNavigate }: { onSession: (session: Session) => void; onNavigate: (path: string) => void }) {
  const parameters = useMemo(() => new URLSearchParams(window.location.search), [])
  const code = parameters.get('code')
  const [message, setMessage] = useState(
    code ? 'Completing OAuth sign-in...' : 'The authorization server did not return a code.',
  )

  useEffect(() => {
    if (!code) {
      return
    }

    completeLogin(code, parameters.get('state'))
      .then((session) => {
        onSession(session)
        window.history.replaceState(null, '', '/dashboard')
        onNavigate('/dashboard')
      })
      .catch((error: Error) => setMessage(error.message))
  }, [code, onNavigate, onSession, parameters])

  return (
    <section className="content-card">
      <h1>OAuth callback</h1>
      <p>{message}</p>
    </section>
  )
}

export default App
