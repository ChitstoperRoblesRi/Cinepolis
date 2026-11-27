import type { AsientoLectura } from '../types/cinepolis'
/** Materializa los pasillos y conserva la orientación derecha-izquierda de las coordenadas físicas. */
export function matrizSala(asientos: AsientoLectura[]) {
  const columnas = asientos.reduce((max, a) => Math.max(max, a.columna), 1)
  const filas = new Map<string, (AsientoLectura | undefined)[]>()
  for (const asiento of asientos) {
    let fila = filas.get(asiento.fila)
    if (!fila) {
      fila = Array.from({ length: columnas })
      filas.set(asiento.fila, fila)
    }
    fila[columnas - asiento.columna] = asiento
  }
  return { filas, columnas }
}
