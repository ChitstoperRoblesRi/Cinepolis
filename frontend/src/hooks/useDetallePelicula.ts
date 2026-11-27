import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { DetallePeliculaDto } from '../types/cinepolis'
import { sesiones } from '../services/sesiones'
import { useCinepolis } from '../context/CinepolisContext'
import type { IPeliculaApiService } from '../types/servicios'
/** Estado y acciones de PeliculaDetallePage. */
export function useDetallePelicula(id: number, banner: string | null | undefined, servicio: IPeliculaApiService = api) {
  const [p, setPelicula] = useState<DetallePeliculaDto | null>(null)
  const [error, setError] = useState('')
  const [trailer, setTrailer] = useState(false)
  const { complejo, elegirComplejo } = useCinepolis()
  useEffect(() => {
    const abort = new AbortController()
    servicio.pelicula(id, abort.signal).then(setPelicula).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    return () => abort.abort()
  }, [id, servicio])
  const funciones = sesiones(p?.formatos ?? []).filter(s => s.funcion.complejoId === complejo?.complejoId)
  const fondo = banner || p?.posterUrl
  return { p, error, trailer, setTrailer, complejo, elegirComplejo, funciones, fondo }
}
