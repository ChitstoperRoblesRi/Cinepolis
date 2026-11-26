import { useState, type ReactNode } from 'react'
import { ChevronDown, MapPin, Search, UserRound } from 'lucide-react'
import type { CarteleraDto, CiudadDto, ComplejoDto } from '../types/cinepolis'
import { useCinepolis } from '../context/CinepolisContext'

interface Props {
  children: ReactNode
  ciudades: CiudadDto[]
  complejos: ComplejoDto[]
  peliculas: CarteleraDto[]
  ruta: string
}
export function Layout({ children, ciudades, complejos, peliculas, ruta }: Props) {
  const { ciudad, complejo, elegirCiudad, elegirComplejo, sesion } = useCinepolis()
  const [panel, setPanel] = useState('')
  const [busqueda, setBusqueda] = useState('')
  const normalizar = (s: string) => s.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim()
  const encontrados = peliculas.filter(p => normalizar(p.titulo).includes(normalizar(busqueda))).slice(0, 7)
  return <div onKeyDown={e => { if (e.key === 'Escape') setPanel('') }}><header className="cabecera">
    <div className="wrap top"><a className="marca" href="/cartelera">cinépolis<span>®</span></a>
    <nav aria-label="Principal">{[['Cartelera','/cartelera'],['Preventas','/preventas'],['Dulcería','/dulceria'],['Club Cinépolis','/club'],['Mis boletos','/mis-boletos']].map(([s,href]) =>
      <a key={href} aria-current={ruta === href || href === '/cartelera' && ruta === '/' ? 'page' : undefined} href={href}>{s}</a>)}</nav>
    <div className="tools"><button aria-expanded={panel === 'cine'} onClick={() => setPanel(panel === 'cine' ? '' : 'cine')}><MapPin size={17}/>{complejo?.nombreComercial ?? 'Elige tu cine'}<ChevronDown size={14}/></button>
    <button aria-label="Buscar películas" aria-expanded={panel === 'buscar'} onClick={() => setPanel(panel === 'buscar' ? '' : 'buscar')}><Search size={19}/></button>
    <a href={sesion ? '/club' : '/auth'}><UserRound size={17}/>{sesion ? 'Perfil' : 'Iniciar sesión'}</a></div></div>
    {panel === 'cine' && <div className="pop wrap"><label>Ciudad<select value={ciudad?.ciudadId ?? ''} onChange={e => elegirCiudad(ciudades.find(c => c.ciudadId === +e.target.value) ?? null)}>
      <option value="" disabled>Elige ciudad</option>{ciudades.map(c => <option key={c.ciudadId} value={c.ciudadId}>{c.nombreCiudad}</option>)}</select></label>
      <label>Complejo<select value={complejo?.complejoId ?? ''} onChange={e => { elegirComplejo(complejos.find(c => c.complejoId === +e.target.value) ?? null); setPanel('') }}>
      <option value="" disabled>Elige cine</option>{complejos.map(c => <option key={c.complejoId} value={c.complejoId}>{c.nombreComercial}</option>)}</select></label></div>}
    {panel === 'buscar' && <div className="pop wrap"><label>Buscar en este cine<input autoFocus type="search" value={busqueda} onChange={e => setBusqueda(e.target.value)} placeholder="Título de la película"/></label>
      {busqueda.trim() && <ul>{encontrados.map(p => <li key={p.peliculaId}><a onClick={() => setPanel('')} href={'/pelicula/' + p.peliculaId}>{p.titulo}</a></li>)}{!encontrados.length && <li>Sin coincidencias.</li>}</ul>}</div>}
    </header><main>{children}</main>
    <footer><div className="wrap pie"><a className="marca" href="/cartelera">cinépolis<span>®</span></a><div><strong>Experiencias</strong>{['VIP','IMAX','4DX','Junior'].map(f => <a key={f} href={'/cartelera?formato=' + f}>{f}</a>)}</div>
    <div><strong>Atención a clientes</strong><a href="tel:+525521226060">55 2122 6060</a><a href="mailto:aac@cinepolis.com">Contacto</a></div>
    <div><strong>Información legal</strong><a href="https://static.cinepolis.com/resources/mx/documents/terminos-condiciones-cinepolis-mx.pdf">Términos y condiciones</a><a href="https://static.cinepolis.com/resources/mx/notice_of_privacy/index.html">Aviso de privacidad</a></div></div></footer></div>
}
