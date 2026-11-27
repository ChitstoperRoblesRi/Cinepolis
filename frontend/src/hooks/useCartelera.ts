import { useState } from 'react'
import type { CarteleraDto } from '../types/cinepolis'
import { sesiones } from '../services/sesiones'
export const formatos = ['VIP', 'IMAX', '4DX', 'Macro XE', 'Tradicional', 'Junior']
export const idiomas: Record<string,string> = { DOB: 'Doblada', SUB: 'Subtitulada', Español: 'Español' }
export function fechaISO(d: Date) { return [d.getFullYear(), String(d.getMonth()+1).padStart(2,'0'), String(d.getDate()).padStart(2,'0')].join('-') }


/** Estado y acciones de CarteleraPage. */
export function useCartelera(peliculas: CarteleraDto[], preventas: boolean) {
  const dias = Array.from({length:7}, (_,i) => { const d = new Date(); d.setDate(d.getDate()+i); return d })
  const [fecha, setFecha] = useState(preventas ? '' : fechaISO(dias[0]))
  const inicial = new URLSearchParams(location.search).get('formato') ?? ''
  const [formatosActivos, setFormatos] = useState<string[]>(formatos.includes(inicial) ? [inicial] : [])
  const [lenguas, setLenguas] = useState<string[]>([])
  const [ratings, setRatings] = useState<string[]>([])
  const [slide, setSlide] = useState(0)
  const [trailer, setTrailer] = useState<CarteleraDto | null>(null)
  const films = peliculas.map(p => ({ p, items: sesiones(p.formatos).filter(s => !preventas || s.funcion.esPreventa) })).filter(x => x.items.length)
  const novedades = films.filter(({p,items}) => ['Estreno','Preventa'].includes(p.categoria) || items.some(s => s.funcion.esPreventa))
  const slides = novedades.length ? novedades : films
  const hero = slides[slide % slides.length]
  const lista = films.map(({p,items}) => ({p,items: items.filter(s => (!fecha || fecha === s.fecha) &&
    (!formatosActivos.length || formatosActivos.some(x => [s.tipo,s.formato].some(n => n.toUpperCase() === x.toUpperCase()))) &&
    (!lenguas.length || lenguas.some(l => idiomas[l] === s.funcion.idioma))) }))
    .filter(({p,items}) => items.length && (!ratings.length || ratings.includes(p.clasificacion)))
  return { dias, fecha, setFecha, formatosActivos, setFormatos, lenguas, setLenguas, ratings, setRatings, slide, setSlide, trailer, setTrailer, slides, hero, lista }
}
