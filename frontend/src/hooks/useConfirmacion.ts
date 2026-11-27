import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { TicketDigital } from '../types/cinepolis'
import { useSelectorCinepolis } from '../context/CinepolisContext'
import type { IOrdenApiService } from '../types/servicios'
/** Estado y acciones de ConfirmacionPage. */
export function useConfirmacion(folio: string, servicio: IOrdenApiService = api) {
  const sesion = useSelectorCinepolis(e => e.sesion)
  const ultimaCompra = useSelectorCinepolis(e => e.ultimaCompra)
  const recibo = ultimaCompra?.folio === folio ? ultimaCompra : undefined
  const [consultado,setDigital] = useState<TicketDigital>()
  const [error,setError] = useState('')
  const [intento,setIntento] = useState(0)
  useEffect(() => {
    let activo = true
    setError(''); setDigital(undefined)
    if (recibo) return
    servicio.ticket(folio).then(d => { if (activo) setDigital(d) }).catch(e => { if (activo) setError(e.message) })
    return () => { activo = false }
  },[folio,intento,sesion,recibo])
  const digital = recibo ?? consultado
  const t = digital?.ticket
  const funcion = t?.boletos[0]
  return { error, setIntento, digital, t, funcion }
}
