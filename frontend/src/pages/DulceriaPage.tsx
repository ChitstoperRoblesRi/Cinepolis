import { useState, useRef, useEffect } from 'react'
import { Popcorn, Plus, X } from 'lucide-react'
import { api } from '../services/api'
import { useCinepolis } from '../context/CinepolisContext'
import { moneda } from './compra/PasoBoletos'
import type { CategoriaDulceriaDto, Opcion, ProductoDto, Regla } from '../types/cinepolis'

function saboresPalomitas(regla: Regla) {
  return regla.bloquePalomitas != null && regla.tipoModificador === 'flavor'
}

function precioUnitario(p: ProductoDto, ids: number[]) {
  let base = p.precio ?? 0
  let extras = 0
  for (const regla of p.reglas) {
    let cargo: number | undefined
    for (const o of regla.opciones) {
      if (!ids.includes(o.opcionId)) continue
      if (o.precioVariante != null) base = o.precioVariante
      else {
        const centavos = Math.round(o.extra * 100)
        cargo = saboresPalomitas(regla) ? Math.max(cargo ?? centavos, centavos) : (cargo ?? 0) + centavos
      }
    }
    extras += cargo ?? 0
  }
  return (Math.round(base * 100) + extras) / 100
}

function normalizarProducto(p: ProductoDto): ProductoDto {
  const ids = new Set<number>()
  const reglas = p.reglas.filter(r => {
    if (ids.has(r.reglaId)) return false
    ids.add(r.reglaId)
    return true
  })
  return { ...p, reglas }
}

function disponible(p: ProductoDto) {
  return p.catalogoId != null && p.precio != null && p.reglas.every(r => r.opciones.length >= r.min)
}

function esCombo(p: ProductoDto, categoria: string) {
  if (['combos', 'para compartir'].includes(categoria.trim().toLowerCase()) || /\b(?:maxi)?combo\b/i.test(p.nombreProducto)) return true
  const componentes = p.reglas.filter(r => r.tipoModificador === 'size' && r.grupoTitulo)
    .map(r => r.bloquePalomitas != null ? 'palomitas' : r.grupoTitulo!.trim().toLowerCase())
  return new Set(componentes).size > 1
}

function Opciones({ p, combo, agregar, cerrar, aviso }: {
  p: ProductoDto; combo: boolean; agregar: (ids: number[], unidades: number) => void; cerrar: () => void; aviso: string
}) {
  const modal = useRef<HTMLDialogElement>(null)
  const [ids, setIds] = useState<number[]>(() => p.reglas.flatMap(r => {
    if (r.max < 1) return []
    if (r.opciones.length === 1) return [r.opciones[0].opcionId]
    let nombre: string
    if (p.nombreProducto === 'Nachos' && r.tipoModificador === 'size') nombre = 'grandes'
    else if (combo && r.bloquePalomitas != null && r.tipoModificador === 'size') nombre = 'parallevar'
    else if (combo && saboresPalomitas(r)) nombre = 'mantequilla'
    else if (combo && r.tipoModificador === 'flavor' && /refresco|bebida/i.test(r.grupoTitulo ?? '')) nombre = 'cocacola'
    else return []
    const incluida = r.opciones.find(o => o.nombre.toLowerCase().replace(/[^a-z0-9]/g, '') === nombre)
    return incluida ? [incluida.opcionId] : []
  }))
  const [paso, setPaso] = useState(0)
  const [unidades, setUnidades] = useState(1)
  const regla = p.reglas[paso]
  const esSabor = saboresPalomitas(regla)
  const paraLlevar = esSabor && p.reglas.some(t => t.bloquePalomitas === regla.bloquePalomitas &&
    t.tipoModificador === 'size' && t.opciones.some(o => ids.includes(o.opcionId) && o.nombre.trim().toLowerCase() === 'para llevar'))
  const maximo = esSabor ? Math.min(regla.max, paraLlevar ? 4 : 2) : regla.max
  const seleccionadas = regla.opciones.filter(o => ids.includes(o.opcionId)).length
  const valido = seleccionadas >= regla.min && seleccionadas <= maximo
  const ultimo = paso === p.reglas.length - 1
  const total = unidades * precioUnitario(p, ids)
  const cambiaTamano = regla.bloquePalomitas != null && regla.tipoModificador === 'size'
  function conOpcion(o: Opcion) {
    if (ids.includes(o.opcionId)) return ids
    let conservar = maximo === 1 ? ids.filter(id => !regla.opciones.some(v => v.opcionId === id)) : ids
    if (cambiaTamano && o.nombre.trim().toLowerCase() !== 'para llevar') {
      for (const sabor of p.reglas.filter(r => saboresPalomitas(r) && r.bloquePalomitas === regla.bloquePalomitas)) {
        const excedentes = conservar.filter(id => sabor.opciones.some(v => v.opcionId === id)).slice(2)
        conservar = conservar.filter(id => !excedentes.includes(id))
      }
    }
    return [...conservar, o.opcionId]
  }
  const preciosPrimerGrupo = paso === 0 ? regla.opciones.map(o =>
    precioUnitario(p, conOpcion(o))) : []
  const baseCentavos = Math.round((p.precio ?? 0) * 100)
  const mostrarPrecios = preciosPrimerGrupo.some(precio => Math.round(precio * 100) !== baseCentavos)
  useEffect(() => { const d = modal.current!; d.showModal(); return () => d.close() }, [])
  function alternar(o: Opcion) {
    if (ids.includes(o.opcionId) && (maximo !== 1 || regla.min === 0)) setIds(ids.filter(x => x !== o.opcionId))
    else setIds(conOpcion(o))
  }
  return <dialog className="sabores" ref={modal} aria-labelledby="combo-titulo" onCancel={cerrar}>
    <header>{p.imagenUrl && <img src={p.imagenUrl} alt=""/>}<h2 id="combo-titulo">{p.nombreProducto}</h2>
      <button aria-label="Cerrar personalización" onClick={cerrar}><X/></button></header>
    <div className="paso-config" aria-live="polite">Paso {paso + 1} de {p.reglas.length}</div>
    <progress value={paso + 1} max={p.reglas.length} aria-label="Progreso de personalización"/>
    <fieldset><legend>{regla.grupoTitulo && <>{regla.grupoTitulo} · </>}{esSabor ? 'Sabores de palomitas' : regla.titulo}</legend>
      <p>{esSabor ? 'Elige hasta ' + maximo + ' sabores' :
        regla.min === maximo ? 'Elige ' + regla.min : 'Elige de ' + regla.min + ' a ' + maximo}</p>
      <div className="opciones-combo">{regla.opciones.map((o, i) => {
        const activa = ids.includes(o.opcionId)
        return <button key={o.opcionId} aria-pressed={activa}
          disabled={!activa && maximo !== 1 && seleccionadas >= maximo} onClick={() => alternar(o)}>
          <strong>{combo && regla.tipoModificador === 'size' && /^refresco$/i.test(o.nombre.trim()) ? 'Jumbo' : o.nombre}</strong>
          {mostrarPrecios && <small>{moneda(preciosPrimerGrupo[i])}</small>}
          {paso > 0 && o.extra > 0 && <small>+{moneda(o.extra)}</small>}
        </button>
      })}</div>
    </fieldset>
    {ultimo && <div className="unidades-combo"><strong>Cantidad</strong><div className="contador">
      <button aria-label="Quitar una unidad" disabled={unidades <= 1} onClick={() => setUnidades(unidades - 1)}>−</button>
      <output>{unidades}</output><button aria-label="Añadir una unidad" disabled={unidades >= 999} onClick={() => setUnidades(unidades + 1)}>+</button>
    </div></div>}
    {aviso && <p role="alert">{aviso}</p>}
    <div className="acciones-config">{paso > 0 && <button onClick={() => setPaso(paso - 1)}>Anterior</button>}
      <button className="cta" disabled={!valido} onClick={() => ultimo ? agregar(ids, unidades) : setPaso(paso + 1)}>
        {ultimo ? 'Añadir' : 'Siguiente'} · {moneda(total)}</button></div>
  </dialog>
}

export function CarritoDulceria() {
  const { carrito: c } = useCinepolis()
  return <>{c.productos.map(p => <div className="linea-dulce" key={p.clave}><div><strong>{p.cantidad} × {p.nombre}</strong>
    {p.sabores.length > 0 && <small>{p.sabores.join(' · ')}</small>}<span>{moneda(p.cantidad*p.precio)}</span></div>
    <button aria-label={'Quitar una unidad de ' + p.nombre} onClick={() => c.actualizarProducto({...p,cantidad:p.cantidad-1})}>−</button></div>)}</>
}

export function CatalogoDulceria({ cine }: { cine?: number }) {
  const { carrito: c } = useCinepolis()
  const [datos, setDatos] = useState<CategoriaDulceriaDto[]>()
  const [categoria, setCategoria] = useState(0)
  const [producto, setProducto] = useState<ProductoDto | null>(null)
  const [aviso, setAviso] = useState('')
  const [error, setError] = useState('')
  const [intento, setIntento] = useState(0)
  useEffect(() => {
    const abort = new AbortController()
    if (cine) api.dulceria(cine, abort.signal).then(result => {
      if (abort.signal.aborted) return
      const categorias = result.map(g => ({ ...g, combos: g.combos.map(normalizarProducto).filter(disponible),
        productos: g.productos.map(normalizarProducto).filter(disponible) }))
        .filter(g => g.combos.length + g.productos.length > 0)
      setDatos(categorias)
      setCategoria(categorias[0]?.categoriaId ?? 0)
    }).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    return () => abort.abort()
  }, [cine,intento])
  function agregar(p: ProductoDto, opciones: number[], unidades = 1) {
    if (p.precio == null || !cine) return
    const seleccion = p.reglas.flatMap(r => r.opciones).filter(o => opciones.includes(o.opcionId))
    const clave = [p.productoId,p.catalogoId,...opciones.slice().sort((a,b) => a-b)].join('/')
    const cantidad = unidades + (c.productos.find(x => x.clave === clave)?.cantidad ?? 0)
    if (cantidad > 999) { setAviso('Alcanzaste el máximo por artículo.'); return }
    c.actualizarProducto({clave,productoId:p.productoId,catalogoId:p.catalogoId,complejoId:cine,cantidad,opciones,
      nombre:p.nombreProducto,sabores:seleccion.map(o => o.nombre),precio:precioUnitario(p, opciones)})
    setProducto(null); setAviso(p.nombreProducto + ' añadido.')
  }
  const grupo = datos?.find(g => g.categoriaId === categoria)
  if (!cine) return <p>Elige un complejo para consultar su dulcería.</p>
  if (error) return <p role="alert">{error} <button onClick={() => { setError(''); setIntento(x => x+1) }}>Reintentar</button></p>
  if (!datos) return <p role="status">Cargando dulcería…</p>
  if (!datos.length) return <p>No hay productos disponibles en este cine.</p>
  return <><nav className="pills categorias" aria-label="Categorías de dulcería">{datos.map(g =>
    <button key={g.categoriaId} aria-pressed={categoria === g.categoriaId} onClick={() => setCategoria(g.categoriaId)}>{g.nombreCategoria}</button>)}</nav>
    <div className="dulces-grid">{grupo && [...grupo.combos,...grupo.productos].map(p => <article key={p.productoId}>
      {p.imagenUrl ? <img alt="" loading="lazy" src={p.imagenUrl}/> : <Popcorn size={36}/>}
      <h3>{p.nombreProducto}</h3><p>{p.descripcion}</p><strong>{moneda(p.precio ?? 0)}</strong>
      <button className="back" onClick={() => { setAviso(''); if (p.reglas.length) setProducto(p); else agregar(p, []) }}><Plus size={16}/> {p.reglas.length ? 'Personalizar' : 'Añadir'}</button>
    </article>)}</div>
    <p role="status">{aviso}</p>{producto && grupo && <Opciones p={producto} combo={esCombo(producto, grupo.nombreCategoria)}
      aviso={aviso} cerrar={() => setProducto(null)} agregar={(ids, unidades) => agregar(producto, ids, unidades)}/>}</>
}

export function DulceriaPage() {
  const { complejo, carrito: c } = useCinepolis()
  const cine = c.funcion?.complejoId ?? complejo?.complejoId
  return <section className="wrap compra"><div className="compra-grid"><div className="paso"><h1>Dulcería</h1><CatalogoDulceria key={cine} cine={cine}/></div>
    <aside className="resumen"><h2>Tu dulcería</h2><CarritoDulceria/><p>Subtotal · {moneda(c.dulceria)}</p>
      <a href={c.funcion ? '/compra/dulceria' : '/cartelera'}>{c.funcion ? 'Volver a la compra' : 'Elegir una función'}</a></aside></div></section>
}
