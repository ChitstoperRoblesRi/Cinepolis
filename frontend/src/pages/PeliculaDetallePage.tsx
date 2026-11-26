import { useEffect, useState } from 'react'
import { Clock3, Play } from 'lucide-react'
import { api } from '../services/api'
import type { ComplejoDto, DetallePeliculaDto } from '../types/cinepolis'
import { Horarios, Poster, sesiones, TrailerModal } from '../components/Pelicula'
import { useCinepolis } from '../context/CinepolisContext'

interface Props { id: number; complejos: ComplejoDto[]; banner?: string | null }

export function PeliculaDetallePage({ id, complejos, banner }: Props) {
  const [p, setPelicula] = useState<DetallePeliculaDto | null>(null)
  const [error, setError] = useState('')
  const [trailer, setTrailer] = useState(false)
  const { complejo, elegirComplejo } = useCinepolis()
  useEffect(() => {
    const abort = new AbortController()
    api.pelicula(id, abort.signal).then(setPelicula).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    return () => abort.abort()
  }, [id])
  if (!p) return <section className="wrap cartel"><p role="status">{error || 'Cargando película…'}</p><a href="/cartelera">Volver a cartelera</a></section>
  const funciones = sesiones(p.formatos).filter(s => s.funcion.complejoId === complejo?.complejoId)
  const fondo = banner || p.posterUrl
  return <>
    <section className="hero ficha-hero">
      {fondo && <img src={fondo} alt=""/>}
      <div className="wrap ficha-cabecera">
        <a href="/cartelera">← Cartelera</a>
        <div className="detalle"><Poster url={p.posterUrl}/><div>
          <h1>{p.titulo}</h1>
          <div className="ficha-meta"><span className="badge" aria-label={'Clasificación ' + p.clasificacion}>{p.clasificacion}</span>
            <span><Clock3 size={16}/> {p.duracionMin} min</span><span>{p.genero}</span></div>
          {p.trailerUrl && <button className="cta" onClick={() => setTrailer(true)}><Play size={17}/> Ver tráiler</button>}
        </div></div>
      </div>
    </section>
    <div className="wrap ficha-cuerpo">
      <section><h2>Sinopsis</h2><p>{p.sinopsis}</p>
        <h2>Ficha técnica</h2>
        <dl><dt>{p.directores.length > 1 ? 'Directores' : 'Director'}</dt>
          <dd>{p.directores.join(', ') || 'Sin información'}</dd>
          <dt>Reparto</dt><dd>{p.reparto.join(', ') || 'Sin información'}</dd></dl>
      </section>
      <section className="funciones-ficha"><h2>Horarios</h2>
        <label>Cine<select value={complejo?.complejoId ?? ''} onChange={e => elegirComplejo(complejos.find(c => c.complejoId === Number(e.target.value)) ?? null)}>
          <option value="" disabled>Elige tu cine</option>{complejos.map(c => <option key={c.complejoId} value={c.complejoId}>{c.nombreComercial}</option>)}</select></label>
        {funciones.length ? <Horarios pelicula={p} items={funciones}/> : <p>No hay funciones disponibles en este cine.</p>}
      </section>
    </div>
    {trailer && p.trailerUrl && <TrailerModal url={p.trailerUrl} titulo={p.titulo} cerrar={() => setTrailer(false)}/>}
  </>
}
