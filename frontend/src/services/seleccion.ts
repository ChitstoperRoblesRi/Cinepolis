import type { ProductoDto, Regla, Opcion } from '../types/cinepolis'
import { saboresPalomitas } from './dulceria'
import type { IValidadorSeleccionDulceria } from '../types/servicios'

import { esGrupoOpcional, esGrupoHielo } from './reglasSeleccion'
import { opcionesPredeterminadas } from './preseleccion'

/** Interpreta los límites publicados por la misma cadena que usa el checkout. */
export const validadorSeleccion: IValidadorSeleccionDulceria = {
  evaluar(regla, ids) {
    const limites = ids.flatMap(id => regla.maximosPorTamano?.[id] ?? [])
    const opcional = esGrupoOpcional(regla)
    const minimo = opcional ? 0 : regla.min
    const limiteGrupo = esGrupoHielo(regla) ? Math.min(1, regla.max) : regla.max
    const maximo = Math.min(limiteGrupo, ...limites)
    const cantidad = regla.opciones.filter(o => ids.includes(o.opcionId)).length
    const rango = minimo === maximo ? String(maximo) : `de ${minimo} a ${maximo}`
    return {
      opcional, maximo, valido: cantidad >= minimo && cantidad <= maximo,
      indicador: 'Elige ' + rango + (saboresPalomitas(regla) ? ' sabores' : ''),
      admite: id => ids.includes(id) || maximo === 1 || cantidad < maximo,
    }
  },
}

/** Cambiar de tamaño reinicia únicamente los sabores del bloque correspondiente. */
export function seleccionarOpcion(p: ProductoDto, regla: Regla, ids: number[], o: Opcion, maximo: number) {
  const cambiaTamano = regla.bloquePalomitas != null && regla.tipoModificador === 'size'
  if (ids.includes(o.opcionId)) return ids
  let conservar = maximo === 1 ? ids.filter(id => !regla.opciones.some(v => v.opcionId === id)) : ids
  if (cambiaTamano) {
    for (const sabor of p.reglas.filter(r => saboresPalomitas(r) && r.bloquePalomitas === regla.bloquePalomitas)) {
      const opciones = sabor.opciones
      conservar = conservar.filter(id => !opciones.some(v => v.opcionId === id))
      conservar.push(...opcionesPredeterminadas(p, sabor))
    }
  }
  return [...conservar, o.opcionId]
}
