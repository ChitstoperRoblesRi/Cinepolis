import type { ProductoDto, Regla } from '../types/cinepolis'

import { esGrupoOpcional } from './reglasSeleccion'

const preferencias = [
  'mantequilla', 'cocacola', 'hotdogindividual', 'hotdogchico', 'hotdogtradicional',
]

function normalizar(nombre: string) {
  return nombre.toLowerCase().replace(/[^a-z0-9]/g, '')
}

export function opcionesPredeterminadas(producto: ProductoDto, regla: Regla): number[] {
  if (esGrupoOpcional(regla) || regla.max < 1) return []
  if (regla.opciones.length === 1) return [regla.opciones[0].opcionId]

  const incluidas = regla.opciones.filter(opcion => opcion.precioVariante == null
    ? opcion.extra <= 0
    : opcion.precioVariante <= (producto.precio ?? 0))
  const cantidad = Math.min(regla.max, Math.max(1, regla.min))
  const preferida = preferencias.map(nombre => incluidas.find(o => normalizar(o.nombre) === nombre))
    .find(opcion => opcion != null)
  const ordenadas = preferida ? [preferida, ...incluidas.filter(o => o !== preferida)] : incluidas
  return ordenadas.slice(0, cantidad).map(o => o.opcionId)
}

export function seleccionInicial(producto: ProductoDto) {
  return producto.reglas.flatMap(regla => opcionesPredeterminadas(producto, regla))
}
