import { useState, type FormEvent } from 'react'
import { useCinepolis } from '../../context/CinepolisContext'
import { navegar } from '../../services/navigation'
import { recordarInvitado } from '../MisBoletosPage'
import { moneda } from './PasoBoletos'

export function PasoPago({ boletos }: { boletos: number }) {
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
  return <section className="paso pago"><h1>Pago</h1><form onSubmit={confirmar}><fieldset disabled={ocupado}>
    <legend>Datos del comprador</legend>
    <label>Nombre completo<input required maxLength={100} name="nombre" autoComplete="name" defaultValue={perfil?.nombre}/></label>
    <label>Correo electrónico<input required maxLength={150} type="email" name="correo" autoComplete="email" defaultValue={perfil?.correo}/></label>
    <label>Tarjeta Club Cinépolis (opcional)<input name="club" maxLength={20} defaultValue={perfil?.numeroTarjeta}/></label>
    {perfil ? <p>{perfil.puntosAcumulados} puntos disponibles. Tu compra con tarjeta acumula puntos.</p> :
      <p><a href="/auth?volver=pago">Inicia sesión en Club</a> para acumular o canjear puntos.</p>}
    {perfil && <label className="check"><input type="checkbox" disabled={!puedeCanjear} checked={canje} onChange={e => setPuntos(e.target.checked)}/> Pagar el total con {total} puntos</label>}
    </fieldset>{!canje && <fieldset disabled={ocupado}><legend>Tarjeta de prueba</legend>
      <p>Simulación de pago. Usa 4242 4242 4242 4242, una fecha futura y tres dígitos de prueba.</p>
      <label>Número de tarjeta<input name="numero" inputMode="numeric" placeholder="4242 4242 4242 4242" autoComplete="off" required pattern="4242 ?4242 ?4242 ?4242" title="Usa el número de prueba indicado"/></label>
      <div className="par-pago"><label>Vencimiento<input name="fecha" placeholder="MM/AA" autoComplete="off" maxLength={5} pattern="(0[1-9]|1[0-2])/[0-9]{2}" required/></label>
      <label>Código de seguridad<input name="cvc" type="password" autoComplete="off" inputMode="numeric" maxLength={3} pattern="[0-9]{3}" required/></label></div>
    </fieldset>}
    <dl>{[['Boletos',boletos],['Dulcería',c.dulceria],['Cargo por servicio',c.cargo],['Total',total]].map(([label,n]) => <div className="importe" key={label}><dt>{label}</dt><dd>{moneda(Number(n))}</dd></div>)}</dl>
    {error && <p role="alert">{error}</p>}<button disabled={ocupado || !c.salaConectada} className="cta">{ocupado ? 'Confirmando…' : 'Confirmar compra'}</button>
    </form></section>
}
