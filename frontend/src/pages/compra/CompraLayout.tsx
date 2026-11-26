import { Clock3 } from 'lucide-react'
import { Poster } from '../../components/Pelicula'
import { claveTarifa, useCinepolis } from '../../context/CinepolisContext'
import { PasoBoletos, moneda } from './PasoBoletos'
import { PasoAsientos } from './PasoAsientos'
import { PasoDulceria } from './PasoDulceria'
import { PasoPago } from './PasoPago'
import { CarritoDulceria } from '../DulceriaPage'
import './compra.css'
import './pago.css'

const pasos = ['Boletos', 'Asientos', 'Dulcería', 'Pago']
export function CompraLayout({ ruta }: { ruta: string }) {
  const { carrito: c } = useCinepolis()
  if (!c.funcion || !c.ficha) return <main className="wrap cartel"><h1>{c.expirada ? 'Tu sesión de compra terminó' : 'Selecciona una función'}</h1>
    <p>{c.expirada ? 'Se agotaron los siete minutos de reserva.' : 'Elige un horario para comenzar tu compra.'}</p><a className="cta" href="/cartelera">Ver cartelera</a></main>
  const solicitado = ['boletos','asientos','dulceria','pago'].indexOf(ruta.split('/').pop()!)
  const completo = c.totalBoletos > 0 && c.asientos.length === c.totalBoletos && c.salaConectada
  const paso = c.procesandoPago ? 3 : Math.min(solicitado, !c.totalBoletos ? 0 : completo ? 3 : 1)
  const tarifas = new Map(c.mapa?.tarifas.map(t => [claveTarifa(t),t]))
  const subtotal = Object.entries(c.cantidades).reduce((sum,[k,n]) => sum + (tarifas.get(k)?.precio ?? 0)*n, 0)
  const etiquetas = c.asientos.map(b => c.mapa?.asientos.find(a => a.asientoId === b.asientoId))
    .map(a => a && (a.numero ? a.fila + a.numero : `Fila ${a.fila}, sin etiqueta`))
  return <main className="wrap compra"><ol className="progreso" aria-label="Progreso de compra">{pasos.map((nombre,i) =>
    <li key={nombre} aria-current={i === paso ? 'step' : undefined} data-hecho={i < paso}><b>{i+1}</b>{nombre}</li>)}</ol>
    <div className="compra-grid"><div>{c.errorSala ? <p role="alert">{c.errorSala} <button onClick={c.reconectar}>Reintentar</button></p> :
      !c.mapa && <p role="status">Cargando tarifas y sala…</p>}
      {paso === 0 ? <PasoBoletos/> : paso === 1 ? <PasoAsientos/> : paso === 2 ? <PasoDulceria/> : <PasoPago boletos={subtotal}/>}</div>
      <aside className="resumen"><header><h2>Tu compra</h2><span role="timer" aria-label="Tiempo restante"><Clock3 size={17}/> {Math.floor(c.segundosRestantes/60)}:{String(c.segundosRestantes%60).padStart(2,'0')}</span></header>
        <div className="resumen-ficha"><Poster url={c.ficha.posterUrl}/><div><h3>{c.ficha.titulo}</h3><p>{c.funcion.nombreComercial}</p><p>{c.funcion.nombreSala} · {c.funcion.idioma}</p>
        <p>{new Date(c.ficha.fecha+'T12:00:00').toLocaleDateString('es-MX',{day:'numeric',month:'short'})} · {c.ficha.hora.slice(0,5)}</p></div></div>
        {Object.entries(c.cantidades).filter(([,n]) => n > 0).map(([k,n]) => <div className="importe" key={k}><span>{n} × {tarifas.get(k)?.nombre}</span><span>{moneda(n*(tarifas.get(k)?.precio ?? 0))}</span></div>)}
        <p><strong>Asientos</strong> · {etiquetas.join(', ') || 'Por elegir'}</p><CarritoDulceria/><div className="importe total"><strong>Subtotal</strong><strong>{moneda(subtotal+c.dulceria)}</strong></div>
      </aside></div></main>
}
