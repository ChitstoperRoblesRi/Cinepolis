import { useMemo } from 'react'
import { matrizSala } from '../services/mapaSala'
import { useRef, useState } from 'react'
import { claveTarifa, useCinepolis } from '../context/CinepolisContext'
import type { AsientoLectura } from '../types/cinepolis'
/** Serializa cambios de reserva y resuelve la tarifa por zona física. */
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
        const tarifa = mapa?.tarifas.find(t => t.areaCategoryCode === a.areaCategoryCode &&
          (c.cantidades[claveTarifa(t)] ?? 0) > c.usados(claveTarifa(t)))
        if (!tarifa) { setMensaje('Elige una butaca de la zona de tus boletos pendientes.'); return }
        if (!await c.seleccionar({ asientoId: a.asientoId, tipoBoletoId: tarifa.tipoBoletoId }))
          setMensaje('La butaca ya no está disponible. Elige otra.')
      }
    } catch { setMensaje('No se pudo confirmar el cambio en la sala.') }
    finally { procesando.current = false; setOcupado(false) }
  }
  const completo = c.asientos.length === c.totalBoletos && c.totalBoletos > 0
  return { c, mapa, mensaje, ocupado, filas, columnas, elegido, completo, pulsar }
}
