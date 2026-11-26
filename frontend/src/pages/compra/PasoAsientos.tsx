import { useRef, useState, type CSSProperties } from 'react'
import { Accessibility, Crown } from 'lucide-react'
import { claveTarifa, useCinepolis } from '../../context/CinepolisContext'
import { navegar } from '../../services/navigation'
import type { AsientoLectura } from '../../types/cinepolis'

export function PasoAsientos() {
  const { carrito: c } = useCinepolis()
  const [mensaje, setMensaje] = useState('')
  const [ocupado, setOcupado] = useState(false)
  const procesando = useRef(false)
  const mapa = c.mapa
  if (!mapa) return null
  const filas = new Map<string, AsientoLectura[]>()
  let columnas = 1
  for (const a of mapa.asientos) {
    if (!filas.has(a.fila)) filas.set(a.fila, [])
    filas.get(a.fila)!.push(a)
    columnas = Math.max(columnas, a.columna)
  }
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
  return <section className="paso"><h1>Elige tus asientos</h1>
    <p>{c.asientos.length} de {c.totalBoletos} seleccionados</p>
    <div className="auditorio" role="region" aria-label="Mapa de butacas" style={{'--columnas': columnas} as CSSProperties}>
      <div className="pantalla">Pantalla</div>
      {[...filas].map(([fila, butacas]) => <div key={fila} className="fila-sala">
        <strong className="letra-fila" style={{gridColumn: 1}}>{fila}</strong>{butacas.map(a => {
          const mia = elegido(a.asientoId)
          const estado = mia ? 'seleccionado' : a.estado
          const tipo = a.tipoAsiento.trim().toLowerCase()
          const codigo = a.numero ? fila + a.numero : `Fila ${fila}, sin etiqueta`
          const etiqueta = codigo + ' · ' + (a.esAccesible ? 'Silla de ruedas' : a.tipoAsiento) + ' · ' + estado
          return <button key={a.asientoId} className="butaca" data-estado={estado} data-tipo={a.tipoAsiento}
            style={{gridColumn: columnas - a.columna + 2}} aria-pressed={mia} aria-label={etiqueta} title={etiqueta}
            disabled={ocupado || !c.salaConectada || (!mia && (completo || a.estado !== 'disponible'))}
            onClick={() => void pulsar(a)}>
            {a.esAccesible ? <Accessibility aria-hidden="true" size={16}/> : <>
              {tipo === 'vip' && <Crown aria-hidden="true" size={12}/>}
              <span>{a.numero}</span>
            </>}
          </button>
        })}<strong className="letra-fila" aria-hidden="true" style={{gridColumn: columnas + 2}}>{fila}</strong></div>)}</div>
    <div className="leyenda">{[['disponible','Disponible'],['seleccionado','Tu selección'],['reservado','Bloqueado'],['vendido','Vendido']].map(([estado,label]) => <span key={estado}><i className="butaca" data-estado={estado}/>{label}</span>)}<span><Crown size={16}/> VIP</span><span><Accessibility size={16}/> Accesible</span></div>
    <p role="status">{mensaje}</p><div className="acciones-compra"><a href="/compra/boletos">Volver a boletos</a>
    <button className="cta" disabled={!completo || ocupado || !c.salaConectada} onClick={() => navegar('/compra/dulceria')}>Continuar a dulcería</button></div></section>
}
