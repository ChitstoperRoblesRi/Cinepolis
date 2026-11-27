import { claveTarifa, type useCheckoutFacade } from '../hooks/useCheckoutFacade'

/** Agrupa tarifas equivalentes preservando las zonas aplicables al boleto. */
export function presentarTarifas(c: ReturnType<typeof useCheckoutFacade>) {
  const tarifas = [...new Map(c.mapa?.tarifas.map(t => [claveTarifa(t), t])).values()]
  return tarifas.map(t => {
    const clave = claveTarifa(t)
    const cantidad = c.cantidades[clave] ?? 0
    const zonas = c.mapa!.tarifas.filter(a => claveTarifa(a) === clave).map(a => a.areaCategoryCode)
    const mostrarZona = tarifas.some(a => a.tipoBoletoId === t.tipoBoletoId && a.precio !== t.precio)
    return { t, clave, cantidad, zonas, mostrarZona }
  })
}
