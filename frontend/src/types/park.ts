export interface NationalPark {
  id: number
  name: string
  state: string
  establishedYear: number
  acres: number
  region: string
  highlight: string
  activities: string[]
}

export interface Session {
  accessToken: string
  displayName: string
  scope: string
  expiresAt: number
}
