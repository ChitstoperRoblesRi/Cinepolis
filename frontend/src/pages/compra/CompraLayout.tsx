import { useResumenCompra } from '../../hooks/useResumenCompra'
import { TiempoReserva } from '../../components/TiempoReserva'
import { Poster } from '../../components/Pelicula'
import { PasoBoletos, moneda } from './PasoBoletos'
import { PasoAsientos } from './PasoAsientos'
import { PasoDulceria } from './PasoDulceria'
import { PasoPago } from './PasoPago'
import { CarritoDulceria } from '../DulceriaPage'
import './compra.css'
import './pago.css'

const pasos = ['Boletos', 'Asientos', 'Dulcería', 'Pago']
export function CompraLayout({ ruta }: { ruta: string }) {
  const { c, paso, lineas, subtotal, etiquetas, subtotalCompra } = useResumenCompra(ruta)
  if (!c.funcion || !c.ficha) return <main className="wrap cartel"><h1>{c.expirada ? 'Tu sesión de compra terminó' : 'Selecciona una función'}</h1>
    <p>{c.expirada ? 'Se agotaron los siete minutos de reserva.' : 'Elige un horario para comenzar tu compra.'}</p><a className="cta" href="/cartelera">Ver cartelera</a></main>
  return <main className="wrap compra"><ol className="progreso" aria-label="Progreso de compra">{pasos.map((nombre,i) =>
    <li key={nombre} aria-current={i === paso ? 'step' : undefined} data-hecho={i < paso}><b>{i+1}</b>{nombre}</li>)}</ol>
    <div className="compra-grid"><div>{c.errorSala ? <p role="alert">{c.errorSala} <button onClick={c.reconectar}>Reintentar</button></p> :
      !c.mapa && <p role="status">Cargando tarifas y sala…</p>}
      {paso === 0 ? <PasoBoletos/> : paso === 1 ? <PasoAsientos/> : paso === 2 ? <PasoDulceria/> : <PasoPago boletos={subtotal}/>}</div>
      <aside className="resumen"><header><h2>Tu compra</h2><TiempoReserva vence={c.vence}/></header>
        <div className="resumen-ficha"><Poster url={c.ficha.posterUrl}/><div><h3>{c.ficha.titulo}</h3><p>{c.funcion.nombreComercial}</p><p>{c.funcion.nombreSala} · {c.funcion.idioma}</p>
        <p>{new Date(c.ficha.fecha+'T12:00:00').toLocaleDateString('es-MX',{day:'numeric',month:'short'})} · {c.ficha.hora.slice(0,5)}</p></div></div>
        {lineas.map(linea => <div className="importe" key={linea.clave}><span>{linea.cantidad} × {linea.nombre}</span><span>{moneda(linea.importe)}</span></div>)}
        <p><strong>Asientos</strong> · {etiquetas.join(', ') || 'Por elegir'}</p><CarritoDulceria/><div className="importe total"><strong>Subtotal</strong><strong>{moneda(subtotalCompra)}</strong></div>
      </aside></div></main>
}
