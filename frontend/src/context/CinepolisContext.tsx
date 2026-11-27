import { useLayoutEffect, useSyncExternalStore } from 'react'
import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api, readSession, saveSession } from '../services/api'
import type { TicketDigital, CiudadDto, ComplejoDto, PerfilClub, RespuestaLogin } from '../types/cinepolis'

import { useCheckoutFacade } from '../hooks/useCheckoutFacade'
export { claveTarifa } from '../hooks/useCheckoutFacade'
function useEstado() {
  const [ciudad, setCiudad] = useState<CiudadDto | null>(null)
  const [complejo, setComplejo] = useState<ComplejoDto | null>(null)
  const [sesion, setSesion] = useState(readSession)
  const [perfil, setPerfil] = useState<PerfilClub | null>(null)
  const [intentoPerfil,setIntentoPerfil] = useState(0)
  const [errorPerfil, setErrorPerfil] = useState('')
  const carrito = useCheckoutFacade()
  const [confirmacion, setConfirmacion] = useState<{ ticket: TicketDigital; sesion: typeof sesion } | null>(null)
  function guardarConfirmacion(ticket: TicketDigital) { setConfirmacion({ ticket, sesion }) }

  useEffect(() => {
    const actualizar = () => { setConfirmacion(null); setPerfil(null); setSesion(readSession()) }
    window.addEventListener('cinepolis:sesion', actualizar)
    return () => window.removeEventListener('cinepolis:sesion', actualizar)
  }, [])
  useEffect(() => {
    setErrorPerfil('')
    if (!sesion) return
    const abort = new AbortController()
    const timeout = window.setTimeout(() => saveSession(null), Math.max(0, Date.parse(sesion.expiraUtc) - Date.now()))
    const cargar = () => api.perfil(abort.signal).then(p => { if (!abort.signal.aborted) setPerfil(p) })
      .catch(() => { if (!abort.signal.aborted) setErrorPerfil('No se pudo cargar el perfil.') })
    void cargar()
    window.addEventListener('cinepolis:compra', cargar)
    return () => { window.removeEventListener('cinepolis:compra', cargar); abort.abort(); clearTimeout(timeout) }
  }, [sesion,intentoPerfil])
  function elegirCiudad(valor: CiudadDto | null) {
    if (valor?.ciudadId === ciudad?.ciudadId) return
    if (valor) localStorage.setItem('cinepolis.ciudad', String(valor.ciudadId))
    setCiudad(valor); setComplejo(null); carrito.elegirFuncion(null)
  }
  function elegirComplejo(valor: ComplejoDto | null) {
    if (valor && valor.ciudadId !== ciudad?.ciudadId) throw new Error('El cine pertenece a otra ciudad.')
    if (valor) localStorage.setItem('cinepolis.complejo', String(valor.complejoId))
    setComplejo(valor); carrito.elegirFuncion(null)
  }
  return { ultimaCompra: confirmacion?.sesion === sesion ? confirmacion.ticket : undefined, guardarConfirmacion, ciudad, complejo, elegirCiudad, elegirComplejo, carrito, sesion, perfil, errorPerfil,
    recargarPerfil: () => setIntentoPerfil(i => i+1),
    iniciarSesion: (resultado: RespuestaLogin) => saveSession(resultado), cerrarSesion: () => saveSession(null) }
}

type Estado = ReturnType<typeof useEstado>

/** Publica instantáneas confirmadas y permite observar una proyección del estado. */
function crearObservable(inicial: Estado) {
  let actual = inicial
  const observadores = new Set<() => void>()
  return {
    leer: () => actual,
    suscribir(observador: () => void) {
      observadores.add(observador)
      return () => { observadores.delete(observador) }
    },
    publicar(siguiente: Estado) {
      if (actual === siguiente) return
      actual = siguiente
      observadores.forEach(notificar => notificar())
    }
  }
}

const Context = createContext<ReturnType<typeof crearObservable> | null>(null)
export function CinepolisProvider({ children }: { children: ReactNode }) {
  const estado = useEstado()
  const [observable] = useState(() => crearObservable(estado))
  useLayoutEffect(() => observable.publicar(estado), [observable, estado])
  return <Context.Provider value={observable}>{children}</Context.Provider>
}

/** React compara la selección por identidad antes de volver a renderizar la vista. */
export function useSelectorCinepolis<T>(selector: (estado: Estado) => T) {
  const observable = useContext(Context)
  if (!observable) throw new Error('Se requiere CinepolisProvider.')
  return useSyncExternalStore(observable.suscribir, () => selector(observable.leer()))
}

export function useCinepolis() {
  return useSelectorCinepolis(estado => estado)
}

/** Acceso al estado vigente para acciones diferidas que no requieren suscripción. */
export function useLeerCinepolis() {
  const observable = useContext(Context)
  if (!observable) throw new Error('Se requiere CinepolisProvider.')
  return observable.leer
}
