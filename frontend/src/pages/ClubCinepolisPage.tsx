import { useHistorial } from '../hooks/useHistorial'
import { useState } from 'react'
import { Crown, RotateCw, Ticket } from 'lucide-react'
import { useCinepolis } from '../context/CinepolisContext'
import type { PerfilClub } from '../types/cinepolis'
import { AuthPage } from './AuthPage'
import { moneda } from './compra/PasoBoletos'

function Tarjeta({ perfil: p }: { perfil: PerfilClub }) {
  const [reverso,setReverso] = useState(false)
  const nivel = p.nombreNivel.normalize('NFD').replace(/[\u0300-\u036f]/g,'').toLowerCase()
  const tema = nivel.includes('super') ? 'super' : nivel.includes('fanatico') ? 'fanatico' : 'fan'
  return <div><button className="tarjeta-club" data-nivel={tema} onClick={() => setReverso(v => !v)} aria-pressed={reverso}
    aria-label={reverso ? 'Mostrar frente de la tarjeta' : 'Mostrar reverso de la tarjeta'}>
    <span className="marca">Club Cinépolis</span><span className="nivel-club"><Crown size={19}/>{p.nombreNivel}</span>
    {reverso ? <><small>Número de tarjeta</small><strong>{p.numeroTarjeta}</strong><span>{p.usuarioCc}</span></> :
      <><strong>{p.nombre} {p.apellidos}</strong><span>Socio {p.socioId}</span></>}
    <span className="girar"><RotateCw size={15}/> Girar tarjeta</span></button>
    <div className="saldo-club"><h2>{p.puntosAcumulados.toLocaleString('es-MX')} puntos</h2>
    <p>Vencimiento · {p.puntosVencenFecha}</p>
    {p.siguienteNivel && p.metaVisitas !== null ? <><label htmlFor="progreso-club">Hacia {p.siguienteNivel}</label>
      <progress id="progreso-club" max={p.metaVisitas} value={Math.min(p.visitasSemestreActual,p.metaVisitas)}/>
      <p>{p.visitasSemestreActual} de {p.metaVisitas} visitas este semestre</p></> :
      <p>Nivel máximo · {p.visitasSemestreActual} visitas este semestre</p>}</div></div>
}

function Historial() {
  const { datos, activas, setActivas, error, setIntento, filas } = useHistorial()
  return <section className="historial-club"><div className="pills"><button aria-pressed={!activas} onClick={() => setActivas(false)}>Mis compras</button>
    <button aria-pressed={activas} onClick={() => setActivas(true)}>Boletos activos</button></div>
    {error ? <p role="alert">{error} <button onClick={() => setIntento(i => i+1)}>Reintentar</button></p> : !datos && <p role="status">Cargando historial…</p>}
    {filas?.length === 0 && <p>{activas ? 'No tienes boletos activos.' : 'Aún no tienes compras con esta cuenta.'}</p>}
    {filas?.map(c => <article key={c.folio}><Ticket aria-hidden="true"/><div><h3>{c.titulo}</h3><p>{c.complejo} · {new Date(c.horario).toLocaleString('es-MX',{dateStyle:'medium',timeStyle:'short'})}</p>
    <p>{moneda(c.total)} · {c.activa ? 'Boleto activo' : c.estado === 'activo' ? 'Función finalizada o inactiva' : c.estado}</p>
    <a href={'/confirmacion/'+c.folio}>Abrir boleto QR</a></div></article>)}</section>
}

export function ClubCinepolisPage() {
  const { sesion, perfil, cerrarSesion, errorPerfil, recargarPerfil } = useCinepolis()
  if (!sesion) return <AuthPage/>
  return <section className="wrap zona-club"><header><h1>Mi Club Cinépolis</h1><button className="back" onClick={cerrarSesion}>Cerrar sesión</button></header>
    {perfil ? <div className="panel-club"><Tarjeta perfil={perfil}/><Historial key={perfil.socioId}/></div> :
      errorPerfil ? <p role="alert">{errorPerfil} <button onClick={recargarPerfil}>Reintentar</button></p> : <p role="status">Cargando tu tarjeta…</p>}</section>
}
