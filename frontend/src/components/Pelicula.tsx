import type { FormatoDto } from '../types/cinepolis'
import { agruparSesiones, sesiones } from '../services/sesiones'
import { useEffect, useRef } from 'react'
import { Film, X } from 'lucide-react'
import { useCinepolis } from '../context/CinepolisContext'
import { navegar } from '../services/navigation'

import type { Sesion } from '../services/sesiones'
export function Horarios({ items, pelicula }: { items: Sesion[]; pelicula: { formatos: FormatoDto[]; titulo: string; posterUrl: string | null } }) {
  const { carrito } = useCinepolis()
  const grupos = agruparSesiones(items)
  return <div className="horarios">{[...grupos].map(([key, grupo]) => <div key={key}>
    <p>{[...new Set([grupo[0].tipo, grupo[0].formato])].join(' / ')} · {grupo[0].funcion.idioma} · {grupo[0].fecha}</p>
    <div className="pills">{grupo.map(s => <button key={s.funcion.funcionId} title={s.funcion.nombreSala}
      aria-label={s.hora.slice(0,5) + ', ' + s.funcion.nombreSala} onClick={() => { carrito.elegirFuncion(s.funcion, { horarios: sesiones(pelicula.formatos).filter(h => h.funcion.complejoId === s.funcion.complejoId && h.fecha === s.fecha).sort((a,b) => a.hora.localeCompare(b.hora)), titulo: pelicula.titulo, formato: s.formato, posterUrl: pelicula.posterUrl, fecha: s.fecha, hora: s.hora }); navegar('/compra/asientos') }}>{s.hora.slice(0,5)}</button>)}</div>
  </div>)}</div>
}

export function Poster({ url }: { url: string | null }) {
  return url ? <img className="poster" src={url} alt="" loading="lazy"/> : <div className="poster placeholder"><Film /></div>
}

export function Trailer({ url }: { url: string }) {
  let video: URL
  try { video = new URL(url) } catch { return <p>Tráiler no disponible.</p> }
  if (!['https:', 'http:'].includes(video.protocol)) return <p>Tráiler no disponible.</p>
  const host = video.hostname.replace(/^www\./, '')
  const id = host === 'youtu.be' ? video.pathname.slice(1) : ['youtube.com','m.youtube.com','youtube-nocookie.com'].includes(host)
    ? video.searchParams.get('v') ?? video.pathname.match(/^\/(?:embed|shorts)\/([^/]+)/)?.[1] : null
  let embed = id && /^[\w-]{11}$/.test(id) ? 'https://www.youtube-nocookie.com/embed/' + id : null
  if (['vimeo.com','player.vimeo.com'].includes(host) && /^\/(?:video\/)?\d+$/.test(video.pathname))
    embed = 'https://player.vimeo.com/video/' + video.pathname.split('/').pop()
  if (embed) return <iframe title="Tráiler de la película" className="video" src={embed} allowFullScreen />
  if (/\.(mp4|webm|ogv)$/i.test(video.pathname)) return <video className="video" controls src={video.href} />
  return <a className="cta" href={video.href} target="_blank" rel="noreferrer">Abrir tráiler</a>
}
export function TrailerModal({ url, titulo, cerrar }: { url: string; titulo: string; cerrar: () => void }) {
  const dialog = useRef<HTMLDialogElement>(null)
  useEffect(() => { const el = dialog.current!; el.showModal(); return () => el.close() }, [])
  return <dialog ref={dialog} onCancel={cerrar} aria-label={'Tráiler de ' + titulo}><div className="dialog-bar"><strong>{titulo}</strong><button aria-label="Cerrar tráiler" onClick={cerrar}><X/></button></div><Trailer url={url}/></dialog>
}
