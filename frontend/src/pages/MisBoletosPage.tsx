import { useMisBoletos } from '../hooks/useMisBoletos'
import { navegar } from '../services/navigation'

export function MisBoletosPage() {
  const { sesion, folios, error } = useMisBoletos()
  return <section className="wrap compra"><div className="paso tickets"><h1>Mis boletos</h1>
    <form onSubmit={e => { e.preventDefault(); navegar('/confirmacion/'+String(new FormData(e.currentTarget).get('folio')).trim().toUpperCase()) }}>
      <label>Consultar por folio<input required name="folio" pattern="[Ww][Ee][Bb]-[a-fA-F0-9]{40}" placeholder="WEB-…" /></label><button className="cta">Buscar ticket</button></form>
    {error && <p role="alert">{error}</p>}{!folios && !error && <p>Cargando boletos…</p>}
    {folios?.length === 0 && <p>{sesion ? 'Aún no tienes compras en esta cuenta.' : 'Aún no hay compras de invitado guardadas en este navegador.'}</p>}
    {folios?.map(f => <p key={f}><a href={'/confirmacion/'+f}>{f}</a></p>)}</div></section>
}
