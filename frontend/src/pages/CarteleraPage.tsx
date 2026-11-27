import { useCartelera, formatos, idiomas, fechaISO } from '../hooks/useCartelera'
import { ArrowUpRight, ChevronLeft, ChevronRight, Play } from 'lucide-react'
import type { CarteleraDto } from '../types/cinepolis'
import { Horarios, Poster, TrailerModal } from '../components/Pelicula'

function filtro(label: string, valores: string[], activo: string[], set: (v: string[]) => void) {
  return <fieldset><legend>{label}</legend><div className="pills">{valores.map(v => <button key={v} aria-pressed={activo.includes(v)} onClick={() => set(activo.includes(v) ? activo.filter(x => x !== v) : [...activo,v])}>{v}</button>)}</div></fieldset>
}
export function CarteleraPage({ peliculas, preventas, pendiente }: { peliculas: CarteleraDto[]; preventas: boolean; pendiente: boolean }) {
  const { dias, fecha, setFecha, formatosActivos, setFormatos, lenguas, setLenguas, ratings, setRatings, slide, setSlide, trailer, setTrailer, slides, hero, lista } = useCartelera(peliculas, preventas)
  return <>{hero && <section className="hero" aria-label="Películas destacadas">
    {(hero.p.bannerUrl || hero.p.posterUrl) && <img src={hero.p.bannerUrl || hero.p.posterUrl!} alt=""/>}
    <div className="wrap hero-inner"><span className="eyebrow">{hero.items.some(s => s.funcion.esPreventa) ? 'Preventa' : hero.p.categoria}</span><h2><a href={'/pelicula/' + hero.p.peliculaId}>{hero.p.titulo}</a></h2>
    <p>{hero.p.genero} · {hero.p.duracionMin} min</p><a className="cta" href={'/pelicula/' + hero.p.peliculaId}>Ver película <ArrowUpRight size={17}/></a>
    {slides.length > 1 && <div className="slides"><button aria-label="Película anterior" onClick={() => setSlide((slide+slides.length-1)%slides.length)}><ChevronLeft/></button>
      <span>{slide%slides.length+1} / {slides.length}</span><button aria-label="Película siguiente" onClick={() => setSlide((slide+1)%slides.length)}><ChevronRight/></button></div>}</div></section>}
    <section className="wrap cartel"><h1>{preventas ? 'Preventas' : 'Cartelera'}</h1>
    <div className="dias">{dias.map((d,i) => <button key={fechaISO(d)} aria-pressed={fecha === fechaISO(d)} onClick={() => setFecha(fechaISO(d))}>{i === 0 ? 'Hoy' : i === 1 ? 'Mañana' : d.toLocaleDateString('es-MX',{weekday:'short'})}<small>{d.toLocaleDateString('es-MX',{day:'numeric',month:'short'})}</small></button>)}
    <button aria-pressed={!fecha} onClick={() => setFecha('')}>Todas las fechas</button></div>
    <div className="filtros">{filtro('Formato',formatos,formatosActivos,setFormatos)}{filtro('Idioma',Object.keys(idiomas),lenguas,setLenguas)}
    {filtro('Clasificación',['AA','A','B','B15','C','D','TBC','S/C'],ratings,setRatings)}</div>
    {pendiente ? <p role="status">Cargando cartelera…</p> : lista.length === 0 && <p>No hay funciones para esta selección.</p>}
    <div className="cards">{lista.map(({p,items}) => <article key={p.peliculaId}><a className="cover" aria-label={'Ver ' + p.titulo} href={'/pelicula/' + p.peliculaId}><Poster url={p.posterUrl}/></a>
    <div className="card-info"><span className="badge" data-rating={p.clasificacion}>{p.clasificacion}</span><h2><a href={'/pelicula/' + p.peliculaId}>{p.titulo}</a></h2><p>{p.genero} · {p.duracionMin} min</p>
    {p.trailerUrl && <button className="trailer-link" onClick={() => setTrailer(p)}><Play size={15}/> Ver tráiler</button>}
    <Horarios pelicula={p} items={items}/></div></article>)}</div></section>
    {trailer?.trailerUrl && <TrailerModal url={trailer.trailerUrl} titulo={trailer.titulo} cerrar={() => setTrailer(null)}/>}</>
}
