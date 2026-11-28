import type { Regla } from '../types/cinepolis'

export function esGrupoHielo(regla: Regla) {
  const hielo = /\bhielos?\b/i
  return hielo.test(regla.titulo) || hielo.test(regla.grupoTitulo ?? '') ||
    (regla.opciones.length > 0 && regla.opciones.every(o => hielo.test(o.nombre)))
}

export function esGrupoOpcional(regla: Regla) {
  return regla.min === 0 || esGrupoHielo(regla)
}
