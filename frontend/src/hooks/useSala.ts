import { useMemo } from 'react'
import { ErrorSeleccionButaca } from '../services/tarifas'
import { matrizSala } from '../services/mapaSala'
import { useRef, useState } from 'react'
import { useCinepolis } from '../context/CinepolisContext'
import type { AsientoLectura } from '../types/cinepolis'
/** Serializa los clics sobre el mapa y presenta el resultado de cada reserva. */
export function useSala() {
  const { carrito: c } = useCinepolis()
  const [mensaje, setMensaje] = useState('')
  const [ocupado, setOcupado] = useState(false)
  const procesando = useRef(false)
  const mapa = c.mapa

  const { filas, columnas } = useMemo(() => matrizSala(mapa?.asientos ?? []), [mapa?.asientos])
  const elegido = (id: number) => c.asientos.some(a => a.asientoId === id)
  async function pulsar(a: AsientoLectura) {
    if (procesando.current) return
    procesando.current = true
    setOcupado(true)
    setMensaje('')
    try {
      if (elegido(a.asientoId)) {
        if (!await c.liberar(a.asientoId)) setMensaje('No se pudo liberar la butaca.')
      } else {
        if (!await c.seleccionar(a.asientoId))
          setMensaje('La butaca ya no está disponible. Elige otra.')
      }
    } catch (error) { setMensaje(error instanceof ErrorSeleccionButaca ? error.message : 'No se pudo confirmar el cambio en la sala.') }
    finally { procesando.current = false; setOcupado(false) }
  }
  return { c, mapa, mensaje, ocupado, filas, columnas, elegido, pulsar }
}
