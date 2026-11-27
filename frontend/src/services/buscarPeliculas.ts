import type { CarteleraDto } from '../types/cinepolis'

/** Sugiere hasta siete títulos ignorando acentos y mayúsculas. */
export function buscarPeliculas(peliculas: CarteleraDto[], busqueda: string) {
  const normalizar = (s: string) => s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim()
  return peliculas.filter(p => normalizar(p.titulo).includes(normalizar(busqueda))).slice(0, 7)
}
