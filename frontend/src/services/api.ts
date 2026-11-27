import type { ProblemDetails } from '../types/servicios'
import type * as C from '../types/cinepolis'

const base = import.meta.env.VITE_API_URL.replace(/\/$/, '')
const sessionKey = 'cinepolis.club'

export function readSession(): C.RespuestaLogin | null {
  const raw = sessionStorage.getItem(sessionKey)
  if (raw) {
    try {
      const data: unknown = JSON.parse(raw)
      if (data && typeof data === 'object' && 'accessToken' in data && typeof data.accessToken === 'string' &&
          'expiraUtc' in data && typeof data.expiraUtc === 'string' && Date.parse(data.expiraUtc) > Date.now())
        return { accessToken: data.accessToken, expiraUtc: data.expiraUtc }
    } catch { /* El almacenamiento puede contener una sesión anterior. */ }
    saveSession(null)
  }
  return null
}

export function saveSession(session: C.RespuestaLogin | null) {
  if (session) sessionStorage.setItem(sessionKey, JSON.stringify(session))
  else sessionStorage.removeItem(sessionKey)
  window.dispatchEvent(new Event('cinepolis:sesion'))
}

export class ApiError extends Error {
  constructor(public status: number, public problem: ProblemDetails) {
    super(Object.values(problem.errors ?? {}).flat().join(' ') || problem.detail || problem.title || `Error HTTP ${status}`)
  }
}

async function request<T>(path: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  const headers = new Headers({ Accept: 'application/json' })
  const session = readSession()
  if (session && !path.startsWith('/auth/')) headers.set('Authorization', `Bearer ${session.accessToken}`)
  if (body !== undefined) headers.set('Content-Type', 'application/json')
  const response = await fetch(base + path, {
    method: body === undefined ? 'GET' : 'POST', headers, body: JSON.stringify(body), signal,
  })
  if (!response.ok) {
    if (response.status === 401 && headers.has('Authorization') && readSession()?.accessToken === session?.accessToken) saveSession(null)
    const problem = await response.json().catch(() => null)
    throw new ApiError(response.status, problem ?? {})
  }
  const text = await response.text()
  return text ? JSON.parse(text) as T : undefined as T
}

function query(filters: C.FiltrosCartelera | { ciudadId?: number; complejoId?: number }) {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(filters)) if (value != null) params.set(key, String(value))
  return params.size ? '?' + params.toString() : ''
}

export const api = {
  ciudades: (signal?: AbortSignal) => request<C.CiudadDto[]>('/ciudades', undefined, signal),
  complejos: (ciudadId?: number, signal?: AbortSignal) => request<C.ComplejoDto[]>('/complejos' + query({ ciudadId }), undefined, signal),
  cartelera: (filters: C.FiltrosCartelera = {}, signal?: AbortSignal) => request<C.CarteleraDto[]>('/cartelera' + query(filters), undefined, signal),
  pelicula: (id: number, signal?: AbortSignal) => request<C.DetallePeliculaDto>(`/peliculas/${id}`, undefined, signal),
  dulceria: (complejoId?: number, signal?: AbortSignal) => request<C.CategoriaDulceriaDto[]>('/dulceria' + query({ complejoId }), undefined, signal),
  comprasClub: (signal?: AbortSignal) => request<C.CompraClub[]>('/club/compras', undefined, signal),
  misBoletos: () => request<string[]>('/ordenes'),
  asientos: (id: number) => request<C.MapaSala>(`/funciones/${id}/asientos`),
  codigo: (data: C.SolicitarCodigo) => request<void>('/auth/codigo', data),
  login: (data: C.Login) => request<C.RespuestaLogin>('/auth/login', data),
  registro: (data: C.Registro) => request<C.RespuestaLogin>('/auth/registro', data),
  perfil: (signal?: AbortSignal) => request<C.PerfilClub>('/club/perfil', undefined, signal),
  checkout: (data: C.Checkout) => request<C.TicketDigital>('/ordenes/checkout', data),
  ticket: (folio: string) => request<C.TicketDigital>('/ordenes/' + encodeURIComponent(folio)),
}
