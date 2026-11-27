import { useState, type FormEvent } from 'react'
import { useCinepolis } from '../context/CinepolisContext'
import { navegar } from '../services/navigation'
import { recordarInvitado } from '../services/folios'

/** Estado y acciones de PasoPago. */
export function usePago(boletos: number) {
  const { carrito: c, perfil, sesion, guardarConfirmacion } = useCinepolis()
  const total = Number((boletos + c.dulceria + c.cargo).toFixed(2))
  const [puntos,setPuntos] = useState(false)
  const [error,setError] = useState('')
  const [ocupado,setOcupado] = useState(false)
  const puedeCanjear = perfil && perfil.puntosAcumulados >= total && perfil.puntosVencenFecha >= new Date().toLocaleDateString('sv-SE')
  const canje = puntos && !!puedeCanjear
  async function confirmar(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    if (ocupado) return
    const form = new FormData(e.currentTarget)
    const campo = (key:string) => String(form.get(key) ?? '').trim()
    const tarjetaClub = campo('club')
    if (!campo('nombre')) { setError('Escribe el nombre del comprador.'); return }
    if (tarjetaClub && tarjetaClub !== perfil?.numeroTarjeta) { setError('La tarjeta Club debe corresponder a tu sesión.'); return }
    if (!canje) {
      const [mes,anio] = campo('fecha').split('/').map(Number)
      if (new Date(2000+anio,mes,1) <= new Date()) { setError('La tarjeta de prueba está vencida.'); return }
    }
    setError(''); setOcupado(true)
    try {
      const digital = await c.comprar({formaPago:canje ? 'Puntos' : 'Tarjeta', nombre:campo('nombre'), correo:campo('correo'),
        tarjetaClub:tarjetaClub || undefined,totalEsperado:total})
      guardarConfirmacion(digital)
      const folio = digital.folio
      if (!sesion) recordarInvitado(folio)
      navegar('/confirmacion/'+folio)
    } catch(e) { setError(e instanceof Error ? e.message : 'No se pudo confirmar la compra.') }
    finally { setOcupado(false) }
  }
  return { c, perfil, total, puntos, setPuntos, error, ocupado, puedeCanjear, canje, confirmar }
}
