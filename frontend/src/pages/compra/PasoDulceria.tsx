import { CatalogoDulceria } from '../DulceriaPage'
import { useCinepolis } from '../../context/CinepolisContext'
import { navegar } from '../../services/navigation'

export function PasoDulceria() {
  const { carrito: c } = useCinepolis()
  function omitir() {
    c.productos.forEach(p => c.actualizarProducto({...p, cantidad:0}))
    navegar('/compra/pago')
  }
  return <section className="paso"><h1>Elige tu dulcería</h1><CatalogoDulceria key={c.funcion?.complejoId} cine={c.funcion?.complejoId}/>
    <div className="acciones-compra"><a href="/compra/asientos">Volver a asientos</a>
    <button onClick={omitir}>Continuar sin dulcería</button>
    {c.productos.length > 0 && <a className="cta" href="/compra/pago">Continuar al pago</a>}</div></section>
}
