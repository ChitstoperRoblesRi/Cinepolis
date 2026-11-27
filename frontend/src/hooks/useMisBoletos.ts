import { invitados } from '../services/folios'
import { useEffect, useState } from 'react'
import { useSelectorCinepolis } from '../context/CinepolisContext'
import { api } from '../services/api'
import type { IOrdenApiService } from '../types/servicios'
/** Estado y acciones de MisBoletosPage. */
export function useMisBoletos(servicio: IOrdenApiService = api) {
  const sesion = useSelectorCinepolis(e => e.sesion)
  const [folios,setFolios] = useState<string[]>()
  const [error,setError] = useState('')
  useEffect(() => {
    let activo = true
    setFolios(undefined); setError('')
    const req = sesion ? servicio.misBoletos() : Promise.resolve(invitados())
    req.then(v => { if (activo) setFolios(v) }).catch(e => { if (activo) setError(e.message) })
    return () => { activo = false }
  }, [sesion, servicio])
  return { sesion, folios, error }
}
