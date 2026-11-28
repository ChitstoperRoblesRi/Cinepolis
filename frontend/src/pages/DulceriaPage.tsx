import ProductGrid from '../components/visual/ProductGrid'
import { useCinepolis } from '../context/CinepolisContext'
import { esCombo } from '../services/dulceria'
import { useCatalogo } from '../hooks/useCatalogo'
import { Opciones } from '../components/Opciones'
import { ResumenPedidoSidebar } from '../components/ResumenPedidoSidebar'

export function CatalogoDulceria({ cine }: { cine?: number }) {
  const { datos, categoria, setCategoria, producto, setProducto, aviso, setAviso, error, setError, setIntento, agregar, grupo } = useCatalogo(cine)
  if (!cine) return <p>Elige un complejo para consultar su dulcería.</p>
  if (error) return <p role="alert">{error} <button onClick={() => { setError(''); setIntento(x => x+1) }}>Reintentar</button></p>
  if (!datos) return <p role="status">Cargando dulcería…</p>
  if (!datos.length) return <p>No hay productos disponibles en este cine.</p>
  return <><nav className="pills categorias" aria-label="Categorías de dulcería">{datos.map(g =>
    <button key={g.categoriaId} aria-pressed={categoria === g.categoriaId} onClick={() => setCategoria(g.categoriaId)}>{g.nombreCategoria}</button>)}</nav>
    {grupo && <ProductGrid productos={[...grupo.combos, ...grupo.productos]} elegir={p => {
      setAviso('')
      if (p.reglas.length) setProducto(p)
      else agregar(p, [])
    }}/> }
    <p role="status">{aviso}</p>{producto && grupo && <Opciones p={producto} combo={esCombo(producto, grupo.nombreCategoria)}
      aviso={aviso} cerrar={() => setProducto(null)} agregar={(ids, unidades) => agregar(producto, ids, unidades)}/>}</>
}

export function DulceriaPage() {
  const { complejo, carrito: c } = useCinepolis()
  const cine = c.funcion?.complejoId ?? complejo?.complejoId
  return <section className="wrap compra"><div className="compra-grid"><div className="paso"><h1>Dulcería</h1><CatalogoDulceria key={cine} cine={cine}/></div>
    <ResumenPedidoSidebar ruta="/dulceria"/></div></section>
}
