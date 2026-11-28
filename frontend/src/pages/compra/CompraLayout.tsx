import { useResumenCompra } from '../../hooks/useResumenCompra'
import { TiempoReserva } from '../../components/TiempoReserva'
import { ResumenPedidoSidebar } from '../../components/ResumenPedidoSidebar'
import { useEffect } from 'react'
import { navegar } from '../../services/navigation'
import { PasoAsientos } from './PasoAsientos'
import { PasoDulceria } from './PasoDulceria'
import { PasoPago } from './PasoPago'
import './compra.css'
import './pago.css'

const pasos = ['Horario', 'Asientos', 'Alimentos', 'Pago']
export function CompraLayout({ ruta }: { ruta: string }) {
  const { c, paso, subtotal } = useResumenCompra(ruta)
  useEffect(() => {
    if (ruta === '/compra/boletos') navegar('/compra/asientos', true)
  }, [ruta])
  const directo = ruta === '/compra/pago' && !c.funcion && c.productos.length > 0
  if (ruta === '/compra/pago' && !c.funcion && !c.expirada && !c.productos.length)
    return <main className="wrap compra"><div className="paso"><h1>Tu pedido está vacío</h1><a className="cta" href="/dulceria">Elegir alimentos</a></div></main>
  if (!directo && (!c.funcion || !c.ficha)) return <main className="wrap cartel"><h1>{c.expirada ? 'Tu sesión de compra terminó' : 'Selecciona una función'}</h1>
    <p>{c.expirada ? 'Se agotaron los siete minutos de reserva.' : 'Elige un horario para comenzar tu compra.'}</p><a className="cta" href="/cartelera">Ver cartelera</a></main>
  return <main className="wrap compra"><ol className="progreso" aria-label="Progreso de compra">{pasos.map((nombre,i) =>
    (!directo || i >= 2) && <li key={nombre} aria-current={i === paso ? 'step' : undefined} data-hecho={i < paso}><b>{directo ? i-1 : i+1}</b>{nombre}</li>)}</ol>
    <div className="compra-grid"><div>{c.funcion && <TiempoReserva vence={c.vence}/>}
      {c.errorSala ? <p role="alert">{c.errorSala} <button onClick={c.reconectar}>Reintentar</button></p> :
      c.funcion && !c.mapa && <p role="status">Cargando tarifas y sala…</p>}
      {directo && <a href="/dulceria">Volver a dulcería</a>}
      {paso <= 1 ? <PasoAsientos key={c.funcion?.funcionId}/> : paso === 2 ? <PasoDulceria/> : <PasoPago boletos={subtotal}/>}</div>
      <ResumenPedidoSidebar ruta={ruta} modoSoloLectura={paso !== 2 || ruta === '/compra/pago'}/></div></main>
}
