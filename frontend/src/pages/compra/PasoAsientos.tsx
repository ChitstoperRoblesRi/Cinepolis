import { AsientoCellFactory } from '../../components/AsientoCellFactory'
import { useSala } from '../../hooks/useSala'
import { type CSSProperties } from 'react'
import { Accessibility, Crown } from 'lucide-react'
import { navegar } from '../../services/navigation'

export function PasoAsientos() {
  const { c, mapa, mensaje, ocupado, filas, columnas, elegido, completo, pulsar } = useSala()
  if (!mapa) return null
  return <section className="paso"><h1>Elige tus asientos</h1>
    <p>{c.asientos.length} de {c.totalBoletos} seleccionados</p>
    <div className="auditorio" role="region" aria-label="Mapa de butacas" style={{'--columnas': columnas} as CSSProperties}>
      <div className="pantalla">Pantalla</div>
      {[...filas].map(([fila, butacas]) => <div key={fila} className="fila-sala">
        <strong className="letra-fila" style={{gridColumn: 1}}>{fila}</strong>{butacas.map((a, posicion) =>
          <AsientoCellFactory key={a?.asientoId ?? "pasillo-" + posicion} a={a} mia={!!a && elegido(a.asientoId)} ocupado={ocupado}
            conectada={c.salaConectada} completo={completo} columnas={columnas} pulsar={pulsar}/>
        )}<strong className="letra-fila" aria-hidden="true" style={{gridColumn: columnas + 2}}>{fila}</strong></div>)}</div>
    <div className="leyenda">{[['disponible','Disponible'],['seleccionado','Tu selección'],['reservado','Bloqueado'],['vendido','Vendido']].map(([estado,label]) => <span key={estado}><i className="butaca" data-estado={estado}/>{label}</span>)}<span><Crown size={16}/> VIP</span><span><Accessibility size={16}/> Accesible</span></div>
    <p role="status">{mensaje}</p><div className="acciones-compra"><a href="/compra/boletos">Volver a boletos</a>
    <button className="cta" disabled={!completo || ocupado || !c.salaConectada} onClick={() => navegar('/compra/dulceria')}>Continuar a dulcería</button></div></section>
}
