import { useEffect, useState } from 'react'
import { useLeerCinepolis, useSelectorCinepolis } from '../context/CinepolisContext'
import { api } from '../services/api'
import type { CarteleraDto, CiudadDto, ComplejoDto } from '../types/cinepolis'

/** Estado y acciones de App. */
export function useCarteleraLocal() {
  const ciudad = useSelectorCinepolis(e => e.ciudad)
  const complejo = useSelectorCinepolis(e => e.complejo)
  const leer = useLeerCinepolis()
  const [url, setUrl] = useState(location.pathname + location.search)
  const [ciudades, setCiudades] = useState<CiudadDto[]>([])
  const [cines, setCines] = useState<ComplejoDto[]>([])
  const [catalogo, setCatalogo] = useState<{id: number; peliculas: CarteleraDto[]} | null>(null)
  const [error,setError] = useState('')
  const [intento, reintentar] = useState(0)
  useEffect(() => {
    const cambio = () => setUrl(location.pathname + location.search)
    window.addEventListener('popstate', cambio)
    return () => window.removeEventListener('popstate', cambio)
  }, [])
  useEffect(() => {
    const abort = new AbortController()
    setError('')
    api.ciudades(abort.signal).then(datos => {
      setCiudades(datos)
      if (!ciudad) leer().elegirCiudad(datos.find(c => c.ciudadId === Number(localStorage.getItem('cinepolis.ciudad'))) ?? datos[0] ?? null)
      if (!datos.length) setError('No hay ciudades disponibles.')
    }).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    return () => abort.abort()
  }, [intento])
  useEffect(() => {
    const abort = new AbortController()
    setCines([])
    if (ciudad) {
      setError('')
      api.complejos(ciudad.ciudadId, abort.signal).then(datos => {
        setCines(datos)
        const restaurado = datos.find(c => c.complejoId === Number(localStorage.getItem('cinepolis.complejo'))) ?? datos[0]
        if (restaurado && restaurado.complejoId !== complejo?.complejoId) leer().elegirComplejo(restaurado)
        if (!datos.length) setError('No hay complejos en esta ciudad.')
      }).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    }
    return () => abort.abort()
  }, [ciudad, intento])
  useEffect(() => {
    const abort = new AbortController()
    if (complejo) {
      setError('')
      api.cartelera({ complejoId: complejo.complejoId }, abort.signal).then(peliculas => setCatalogo({id:complejo.complejoId,peliculas}))
        .catch(e => { if (!abort.signal.aborted) setError(e.message) })
    }
    return () => abort.abort()
  }, [complejo, intento])
  const path = url.split('?')[0]
  const detalle = /^\/peliculas?\/(\d+)$/.exec(path)
  const peliculas = catalogo && catalogo.id === complejo?.complejoId ? catalogo.peliculas : []
  return { complejo, url, ciudades, cines, catalogo, error, reintentar, path, detalle, peliculas }
}
