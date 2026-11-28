import { AsientoCellFactory } from '../../components/AsientoCellFactory'
import { useSala } from '../../hooks/useSala'
import { type CSSProperties } from 'react'
import { Accessibility } from 'lucide-react'

export function PasoAsientos() {
  const { c, mapa, mensaje, ocupado, filas, columnas, elegido, pulsar } = useSala()
  const ficha = c.ficha
  if (!ficha) return null
  return <section className="paso"><h1>Elige tus asientos</h1>
    <nav className="horarios-sala" aria-label="Horarios de la función">{ficha.horarios.map(s => {
      const activo = s.funcion.funcionId === c.funcion?.funcionId
      return <button key={s.funcion.funcionId} aria-current={activo ? 'time' : undefined}
        onClick={() => { if (!activo) c.elegirFuncion(s.funcion, { ...ficha, formato: s.formato, fecha: s.fecha, hora: s.hora }) }}>
        <strong>{s.hora.slice(0, 5)}</strong><small>{s.formato} · {s.funcion.idioma} · {s.funcion.nombreSala}</small>
      </button>
    })}</nav>
    {mapa && <><p>Asientos seleccionados ({c.asientos.length})</p>
    <div className="auditorio" role="region" aria-label="Mapa de butacas" style={{'--columnas': columnas} as CSSProperties}>
      <div className="pantalla">Pantalla</div>
      {[...filas].map(([fila, butacas]) => <div key={fila} className="fila-sala">
        <strong className="letra-fila" style={{gridColumn: 1}}>{fila}</strong>{butacas.map((a, posicion) =>
          <AsientoCellFactory key={a?.asientoId ?? "pasillo-" + posicion} a={a} mia={!!a && elegido(a.asientoId)} ocupado={ocupado}
            conectada={c.salaConectada} columnas={columnas} pulsar={pulsar}/>
        )}<strong className="letra-fila" aria-hidden="true" style={{gridColumn: columnas + 2}}>{fila}</strong></div>)}</div>
    <div className="leyenda">{[['seleccionado','Seleccionado'],['disponible','Disponible'],['ocupado','Ocupado']].map(([estado,label]) => <span key={estado}><i className="butaca" data-estado={estado}/>{label}</span>)}<span><Accessibility size={16}/> Silla de ruedas</span></div>
    <p role="status">{mensaje}</p></>}</section>
}
