import type { ProductoDto, Regla } from '../types/cinepolis'
import type { ICalculoItemDulceriaStrategy, ICotizadorDulceria, IVisualizacionPrecioStrategy } from '../types/servicios'
import { moneda } from './moneda'

/** Cada bloque de palomitas cobra una vez su sabor más caro. */
const maximo: ICalculoItemDulceriaStrategy = { acumular: (actual, extra) => Math.max(actual ?? extra, extra) }
const suma: ICalculoItemDulceriaStrategy = { acumular: (actual, extra) => (actual ?? 0) + extra }

/** Compara presentaciones completas contra el precio de portada. */
export const EstrategiaVisualPrimerGrupo: IVisualizacionPrecioStrategy = {
  etiquetas(opciones, base, cotizar) {
    const precios = opciones.map(cotizar)
    const mostrar = precios.some(p => Math.round(p * 100) !== Math.round(base * 100))
    return precios.map(p => mostrar ? moneda(p) : '')
  }
}
/** Los grupos posteriores muestran únicamente suplementos positivos. */
export const EstrategiaVisualGrupoSecundario: IVisualizacionPrecioStrategy = {
  etiquetas: opciones => opciones.map(o => o.extra > 0 ? '+' + moneda(o.extra) : '')
}
export function saboresPalomitas(regla: Regla) {
  return regla.saboresPalomitas ?? (regla.bloquePalomitas != null && regla.tipoModificador === 'flavor')
}

export function precioUnitario(p: ProductoDto, ids: number[]) {
  let base = p.precio ?? 0
  let extras = 0
  for (const regla of p.reglas) {
    const estrategia = saboresPalomitas(regla) ? maximo : suma
    let cargo: number | undefined
    for (const o of regla.opciones) {
      if (!ids.includes(o.opcionId)) continue
      if (o.precioVariante != null) base = o.precioVariante
      else {
        const centavos = Math.round(o.extra * 100)
        cargo = estrategia.acumular(cargo, centavos)
      }
    }
    extras += cargo ?? 0
  }
  return (Math.round(base * 100) + extras) / 100
}

// El catálogo publica el diferencial neto de cada componente incluido y reserva
// precioVariante para las presentaciones que sustituyen la portada del producto.
export const cotizadorDulceria: ICotizadorDulceria = { calcular: precioUnitario }

export function normalizarProducto(p: ProductoDto): ProductoDto {
  const ids = new Set<number>()
  const reglas = p.reglas.filter(r => {
    if (ids.has(r.reglaId)) return false
    ids.add(r.reglaId)
    return true
  })
  return { ...p, reglas }
}

export function disponible(p: ProductoDto) {
  return p.catalogoId != null && p.precio != null && p.reglas.every(r => r.opciones.length >= r.min)
}

export function esCombo(p: ProductoDto, categoria: string) {
  if (['combos', 'para compartir'].includes(categoria.trim().toLowerCase()) || /\b(?:maxi)?combo\b/i.test(p.nombreProducto)) return true
  const componentes = p.reglas.filter(r => r.tipoModificador === 'size' && r.grupoTitulo)
    .map(r => r.bloquePalomitas != null ? 'palomitas' : r.grupoTitulo!.trim().toLowerCase())
  return new Set(componentes).size > 1
}

