import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { CompraClub } from '../types/cinepolis'

/** Estado y acciones de Historial. */
export function useHistorial() {
  const [datos,setDatos] = useState<CompraClub[]>()
  const [activas,setActivas] = useState(false)
  const [error,setError] = useState('')
  const [intento,setIntento] = useState(0)
  useEffect(() => {
    const abort = new AbortController()
    setError('')
    api.comprasClub(abort.signal).then(result => { if (!abort.signal.aborted) setDatos(result) })
      .catch(e => { if (!abort.signal.aborted) setError(e.message) })
    return () => abort.abort()
  },[intento])
  const filas = datos?.filter(c => !activas || c.activa)
  return { datos, activas, setActivas, error, setIntento, filas }
}
