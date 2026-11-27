import type { AccionProductos } from '../types/compra'
import type { LineaDulceria } from '../types/cinepolis'

/** Conserva productos compatibles al cambiar de función y reemplaza líneas por su clave. */
export function reducirProductos(actual: LineaDulceria[], accion: AccionProductos): LineaDulceria[] {
  if (accion.tipo === 'cambiarFuncion') return actual.filter(p => p.complejoId === accion.complejoId)
  const otras = actual.filter(p => p.clave !== accion.linea.clave)
  return accion.linea.cantidad ? [...otras, accion.linea] : otras
}
