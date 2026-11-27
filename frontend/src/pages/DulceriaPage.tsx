import { useConfigurador } from '../hooks/useConfigurador'
import { useCatalogo } from '../hooks/useCatalogo'
import { useRef, useEffect } from 'react'
import { Popcorn, Plus, X } from 'lucide-react'
import { useCinepolis, useSelectorCinepolis } from '../context/CinepolisContext'
import { moneda, importeLinea } from '../services/moneda'
import type { ProductoDto } from '../types/cinepolis'

import { esCombo } from '../services/dulceria'
function Opciones({ p, combo, agregar, cerrar, aviso }: {
  p: ProductoDto; combo: boolean; agregar: (ids: number[], unidades: number) => void; cerrar: () => void; aviso: string
}) {
  const modal = useRef<HTMLDialogElement>(null)
  const { ids, paso, setPaso, unidades, setUnidades, regla, esSabor, maximo, seleccionadas, valido, ultimo, total, alternar, etiquetas } = useConfigurador(p, combo)
  useEffect(() => { const d = modal.current!; d.showModal(); return () => d.close() }, [])
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
          {etiquetas[i] && <small>{etiquetas[i]}</small>}
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
  const productos = useSelectorCinepolis(e => e.carrito.productos)
  const actualizarProducto = useSelectorCinepolis(e => e.carrito.actualizarProducto)
  return <>{productos.map(p => <div className="linea-dulce" key={p.clave}><div><strong>{p.cantidad} × {p.nombre}</strong>
    {p.sabores.length > 0 && <small>{p.sabores.join(' · ')}</small>}<span>{moneda(importeLinea(p.cantidad, p.precio))}</span></div>
    <button aria-label={'Quitar una unidad de ' + p.nombre} onClick={() => actualizarProducto({...p,cantidad:p.cantidad-1})}>−</button></div>)}</>
}

export function CatalogoDulceria({ cine }: { cine?: number }) {
  const { datos, categoria, setCategoria, producto, setProducto, aviso, setAviso, error, setError, setIntento, agregar, grupo } = useCatalogo(cine)
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
