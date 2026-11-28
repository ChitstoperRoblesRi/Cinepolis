import { useState } from 'react'
import { useCinepolis } from '../context/CinepolisContext'
import { useResumenCompra } from '../hooks/useResumenCompra'
import { useLineaDulceria } from '../hooks/useLineaDulceria'
import { moneda } from '../services/moneda'
import type { LineaDulceria } from '../types/cinepolis'
import { Opciones } from './Opciones'
import { Poster } from './Pelicula'

function componentes(linea: LineaDulceria) {
  const grupos = new Map<string, string[]>()
  for (const regla of linea.producto.reglas) {
    const nombres = regla.opciones.filter(o => linea.opciones.includes(o.opcionId)).map(o => o.nombre)
    if (!nombres.length) continue
    const grupo = regla.grupoTitulo?.trim() || linea.nombre
    grupos.set(grupo, [...(grupos.get(grupo) ?? []), ...nombres])
  }
  return [...grupos]
}

export function ResumenPedidoSidebar({ ruta, modoSoloLectura = false }: { ruta: string; modoSoloLectura?: boolean }) {
  const { complejo } = useCinepolis()
  const { c, paso, lineas, etiquetas, subtotalCompra } = useResumenCompra(ruta)
  const [editando, editar] = useState<LineaDulceria | null>(null)
  const [aviso, setAviso] = useState('')
  const guardar = useLineaDulceria(c.funcion?.complejoId ?? complejo?.complejoId)
  const formatoFecha: Intl.DateTimeFormatOptions = { day:'numeric', month:'long' }
  const fecha = c.ficha
    ? new Date(c.ficha.fecha + 'T' + c.ficha.hora).toLocaleString('es-MX', {...formatoFecha, hour:'2-digit', minute:'2-digit'})
    : new Date().toLocaleDateString('es-MX', formatoFecha)
  const total = moneda(subtotalCompra + c.cargo)
  return <aside className="resumen pedido" aria-label="Tu pedido">
    <header><h2>Tu pedido</h2><strong>{total}</strong></header>
    <div className="pedido-cuerpo" tabIndex={0}>
      <strong>Cine</strong><p>{c.funcion?.nombreComercial ?? complejo?.nombreComercial ?? 'Por elegir'}</p>
      <strong>Fecha y hora</strong><p>{fecha}</p>
      {c.funcion && c.ficha && <><div className="resumen-ficha"><Poster url={c.ficha.posterUrl}/><div><h3>{c.ficha.titulo}</h3>
        <p>{c.ficha.formato} · {c.funcion.idioma}</p><p>{c.funcion.nombreSala}</p></div></div>
        <strong>Boletos ({c.totalBoletos})</strong>{lineas.map(l => <p className="importe" key={l.clave}><span>{l.cantidad} x {l.nombre}</span>{moneda(l.importe)}</p>)}
        <p>Butacas · {etiquetas.join(', ') || 'Por elegir'}</p></>}
      <h3>Alimentos ({c.productos.reduce((n,p) => n + p.cantidad, 0)})</h3>
      {c.productos.map(p => <article className="pedido-linea" key={p.clave}><div className="importe"><strong>{p.cantidad} x {p.nombre}</strong><span>{moneda(p.cantidad * p.precio)}</span></div>
        <ul>{componentes(p).map(([grupo, opciones]) => <li key={grupo}>{grupo}: {opciones.join(', ')}</li>)}</ul>
        {!modoSoloLectura && <><button disabled={c.procesandoPago} onClick={() => {setAviso(''); editar(p)}}>Editar</button>
        <button disabled={c.procesandoPago} onClick={() => c.actualizarProducto({...p, cantidad:0})}>Eliminar</button></>}</article>)}
    </div>
    <footer><div className="importe"><span>Subtotal</span>{moneda(subtotalCompra)}</div>
      {c.cargo > 0 && <div className="importe"><span>Cargo por servicio</span>{moneda(c.cargo)}</div>}
      <div className="importe"><strong>Total</strong><strong className="pedido-total">{total}</strong></div>
      <button className="cta" disabled={!c.puedeContinuar(paso)} form={paso === 3 ? 'pago-compra' : undefined}
        type={paso === 3 ? 'submit' : 'button'} onClick={paso === 3 ? undefined : () => c.continuar(paso)}>{c.procesandoPago ? 'Confirmando…' : 'Continuar'}</button></footer>
    {!modoSoloLectura && editando && <Opciones p={editando.producto} combo={editando.combo} inicial={editando} aviso={aviso} cerrar={() => editar(null)}
      agregar={(ids, unidades) => {
        const error = guardar(editando.producto, ids, unidades, editando.combo, editando.clave)
        if (error) setAviso(error)
        else editar(null)
      }}/>}
  </aside>
}
