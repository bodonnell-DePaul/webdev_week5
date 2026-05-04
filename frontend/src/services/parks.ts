import { getApiBaseUrl } from './auth'
import type { NationalPark, Session } from '../types/park'

export async function fetchParks(session: Session, state: string, query: string): Promise<NationalPark[]> {
  const parameters = new URLSearchParams()
  if (state.trim()) {
    parameters.set('state', state.trim().toUpperCase())
  }
  if (query.trim()) {
    parameters.set('q', query.trim())
  }

  const path = parameters.size > 0 ? `/api/parks/search?${parameters.toString()}` : '/api/parks'
  const response = await fetch(`${getApiBaseUrl()}${path}`, {
    headers: { Authorization: `Bearer ${session.accessToken}` },
  })

  if (response.status === 401 || response.status === 403) {
    throw new Error('Your session cannot access this guarded dataset route. Please sign in again.')
  }

  if (!response.ok) {
    throw new Error('The parks API request failed.')
  }

  return (await response.json()) as NationalPark[]
}
