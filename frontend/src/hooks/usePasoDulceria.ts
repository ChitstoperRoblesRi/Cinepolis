import { useCinepolis } from '../context/CinepolisContext'
import { navegar } from '../services/navigation'

/** Estado y acciones de PasoDulceria. */
export function usePasoDulceria() {
  const { carrito: c } = useCinepolis()
  function omitir() {
    c.productos.forEach(p => c.actualizarProducto({...p, cantidad:0}))
    navegar('/compra/pago')
  }
  return { c, omitir }
}
