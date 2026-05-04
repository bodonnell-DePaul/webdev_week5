import type { Session } from '../types/park'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5015'
const clientId = 'react-demo-client'
const scope = 'national-parks.read'
const sessionKey = 'webdev-week5-oauth-session'
const verifierKey = 'webdev-week5-pkce-verifier'
const stateKey = 'webdev-week5-oauth-state'

interface TokenResponse {
  accessToken: string
  tokenType: string
  expiresIn: number
  scope: string
  subject: string
  displayName: string
}

export function getApiBaseUrl() {
  return apiBaseUrl
}

export function getSession(): Session | null {
  const saved = sessionStorage.getItem(sessionKey)
  if (!saved) {
    return null
  }

  const session = JSON.parse(saved) as Session
  if (session.expiresAt <= Date.now()) {
    clearSession()
    return null
  }

  return session
}

export function clearSession() {
  sessionStorage.removeItem(sessionKey)
}

export async function beginLogin() {
  const verifier = randomString(64)
  const challenge = await pkceChallenge(verifier)
  const state = randomString(32)
  const redirectUri = `${window.location.origin}/callback`

  sessionStorage.setItem(verifierKey, verifier)
  sessionStorage.setItem(stateKey, state)

  const parameters = new URLSearchParams({
    response_type: 'code',
    client_id: clientId,
    redirect_uri: redirectUri,
    scope,
    state,
    code_challenge: challenge,
    code_challenge_method: 'S256',
  })

  window.location.assign(`${apiBaseUrl}/oauth/authorize?${parameters.toString()}`)
}

export async function completeLogin(code: string, returnedState: string | null): Promise<Session> {
  const verifier = sessionStorage.getItem(verifierKey)
  const expectedState = sessionStorage.getItem(stateKey)
  if (!verifier || !expectedState || returnedState !== expectedState) {
    throw new Error('OAuth state or PKCE verifier is missing. Start the sign-in again.')
  }

  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    code,
    redirect_uri: `${window.location.origin}/callback`,
    client_id: clientId,
    code_verifier: verifier,
  })

  const response = await fetch(`${apiBaseUrl}/oauth/token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  })

  if (!response.ok) {
    throw new Error('The OAuth token exchange failed.')
  }

  const token = (await response.json()) as TokenResponse
  const session: Session = {
    accessToken: token.accessToken,
    displayName: token.displayName,
    scope: token.scope,
    expiresAt: Date.now() + token.expiresIn * 1000,
  }

  sessionStorage.setItem(sessionKey, JSON.stringify(session))
  sessionStorage.removeItem(verifierKey)
  sessionStorage.removeItem(stateKey)
  return session
}

function randomString(length: number) {
  const bytes = new Uint8Array(length)
  crypto.getRandomValues(bytes)
  return base64Url(bytes).slice(0, length)
}

async function pkceChallenge(verifier: string) {
  const data = new TextEncoder().encode(verifier)
  const digest = await crypto.subtle.digest('SHA-256', data)
  return base64Url(new Uint8Array(digest))
}

function base64Url(bytes: Uint8Array) {
  return btoa(String.fromCharCode(...bytes))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '')
}
