import type { ProductoDto, Regla, Opcion } from '../types/cinepolis'
import { saboresPalomitas } from './dulceria'

/** Selecciones incluidas del catálogo al abrir una personalización. */
export function seleccionInicial(p: ProductoDto, combo: boolean) {
  return p.reglas.flatMap(r => {
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
  })
}

/** Aplica el máximo del catálogo y la capacidad del bloque de palomitas seleccionado. */
export function limiteSeleccion(p: ProductoDto, regla: Regla, ids: number[]) {
  const esSabor = saboresPalomitas(regla)
  const paraLlevar = esSabor && p.reglas.some(t => t.bloquePalomitas === regla.bloquePalomitas &&
    t.tipoModificador === 'size' && t.opciones.some(o => ids.includes(o.opcionId) && o.nombre.trim().toLowerCase() === 'para llevar'))
  return esSabor ? Math.min(regla.max, paraLlevar ? 4 : 2) : regla.max
}

/** Cambiar de tamaño reinicia únicamente los sabores del bloque correspondiente. */
export function seleccionarOpcion(p: ProductoDto, combo: boolean, regla: Regla, ids: number[], o: Opcion, maximo: number) {
  const cambiaTamano = regla.bloquePalomitas != null && regla.tipoModificador === 'size'
  if (ids.includes(o.opcionId)) return ids
  let conservar = maximo === 1 ? ids.filter(id => !regla.opciones.some(v => v.opcionId === id)) : ids
  if (cambiaTamano) {
    for (const sabor of p.reglas.filter(r => saboresPalomitas(r) && r.bloquePalomitas === regla.bloquePalomitas)) {
      const opciones = sabor.opciones
      conservar = conservar.filter(id => !opciones.some(v => v.opcionId === id))
      const incluida = opciones.length === 1 ? opciones[0] : combo
        ? opciones.find(v => v.nombre.trim().toLowerCase() === 'mantequilla') : undefined
      if (incluida && sabor.max > 0) conservar.push(incluida.opcionId)
    }
  }
  return [...conservar, o.opcionId]
}
