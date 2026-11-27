import type { FormatoDto } from '../types/cinepolis'
/** Aplana horarios sin alterar el orden por formato y fecha. */
export function sesiones(formatos: FormatoDto[]) {
  return formatos.flatMap(formato => formato.horarios.flatMap(h => h.funciones.map(funcion => ({
    formato: formato.nombreFormato, tipo: formato.tipoSala, fecha: h.fecha, hora: h.horaInicio, funcion
  }))))
}
export type Sesion = ReturnType<typeof sesiones>[number]

/** Agrupa sesiones por cine, formato, idioma y fecha. */
export function agruparSesiones(items: Sesion[]) {
  const grupos = new Map<string, Sesion[]>()
  for (const s of items) {
    const key = [s.funcion.nombreComercial, s.tipo, s.formato, s.funcion.idioma, s.fecha].join(' · ')
    if (!grupos.has(key)) grupos.set(key, [])
    grupos.get(key)!.push(s)
  }
  return grupos
}
