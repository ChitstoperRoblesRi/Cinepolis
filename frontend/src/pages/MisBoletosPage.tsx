import { useEffect, useState } from 'react'
import { useCinepolis } from '../context/CinepolisContext'
import { api } from '../services/api'
import { navegar } from '../services/navigation'

const clave = 'cinepolis.foliosInvitado'
function invitados(): string[] {
  try {
    const datos: unknown = JSON.parse(localStorage.getItem(clave) ?? '[]')
    return Array.isArray(datos) ? datos.filter((f): f is string => typeof f === 'string' && /^WEB-[A-F0-9]{40}$/.test(f)) : []
  } catch { return [] }
}
export function recordarInvitado(folio: string) {
  try { localStorage.setItem(clave,JSON.stringify([folio,...invitados().filter(f => f !== folio)])) } catch { /* El folio sigue disponible en la confirmación. */ }
}
export function MisBoletosPage() {
  const { sesion } = useCinepolis()
  const [folios,setFolios] = useState<string[]>()
  const [error,setError] = useState('')
  useEffect(() => {
    let activo = true
    setFolios(undefined); setError('')
    const req = sesion ? api.misBoletos() : Promise.resolve(invitados())
    req.then(v => { if (activo) setFolios(v) }).catch(e => { if (activo) setError(e.message) })
    return () => { activo = false }
  }, [sesion])
  return <section className="wrap compra"><div className="paso tickets"><h1>Mis boletos</h1>
    <form onSubmit={e => { e.preventDefault(); navegar('/confirmacion/'+String(new FormData(e.currentTarget).get('folio')).trim().toUpperCase()) }}>
      <label>Consultar por folio<input required name="folio" pattern="[Ww][Ee][Bb]-[a-fA-F0-9]{40}" placeholder="WEB-…" /></label><button className="cta">Buscar ticket</button></form>
    {error && <p role="alert">{error}</p>}{!folios && !error && <p>Cargando boletos…</p>}
    {folios?.length === 0 && <p>{sesion ? 'Aún no tienes compras en esta cuenta.' : 'Aún no hay compras de invitado guardadas en este navegador.'}</p>}
    {folios?.map(f => <p key={f}><a href={'/confirmacion/'+f}>{f}</a></p>)}</div></section>
}
