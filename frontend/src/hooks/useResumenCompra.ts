import { claveTarifa, useCinepolis } from '../context/CinepolisContext'

/** Estado y acciones de CompraLayout. */
export function useResumenCompra(ruta: string) {
  const { carrito: c } = useCinepolis()
  const solicitado = ['boletos','asientos','dulceria','pago'].indexOf(ruta.split('/').pop()!)
  const completo = c.totalBoletos > 0 && c.asientos.length === c.totalBoletos && c.salaConectada
  const paso = c.procesandoPago ? 3 : Math.min(solicitado, !c.totalBoletos ? 0 : completo ? 3 : 1)
  const tarifas = new Map(c.mapa?.tarifas.map(t => [claveTarifa(t),t]))
  const lineas = Object.entries(c.cantidades).filter(([, cantidad]) => cantidad > 0).map(([clave, cantidad]) => {
    const tarifa = tarifas.get(clave)
    return { clave, cantidad, nombre: tarifa?.nombre, importe: cantidad * (tarifa?.precio ?? 0) }
  })
  const subtotal = lineas.reduce((suma, linea) => suma + linea.importe, 0)
  const etiquetas = c.asientos.map(b => c.mapa?.asientos.find(a => a.asientoId === b.asientoId))
    .map(a => a && (a.numero ? a.fila + a.numero : `Fila ${a.fila}, sin etiqueta`))
  return { c, paso, lineas, subtotal, etiquetas, subtotalCompra: subtotal + c.dulceria }
}
