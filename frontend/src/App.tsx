import { useCarteleraLocal } from './hooks/useCarteleraLocal'
import { type MouseEvent } from 'react'
import { Layout } from './components/Layout'
import { CarteleraPage } from './pages/CarteleraPage'
import { PeliculaDetallePage } from './pages/PeliculaDetallePage'
import { CompraLayout } from './pages/compra/CompraLayout'
import { ConfirmacionPage } from './pages/ConfirmacionPage'
import { MisBoletosPage } from './pages/MisBoletosPage'
import { DulceriaPage } from './pages/DulceriaPage'
import { AuthPage } from './pages/AuthPage'
import { ClubCinepolisPage } from './pages/ClubCinepolisPage'
import { navegar } from './services/navigation'

function enlaces(e: MouseEvent) {
  const a = (e.target as Element).closest('a')
  if (!a || e.defaultPrevented || e.button || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey ||
    a.target || a.origin !== location.origin || a.protocol !== location.protocol) return
  e.preventDefault()
  navegar(a.pathname + a.search)
}
export function App() {
  const { complejo, url, ciudades, cines, catalogo, error, reintentar, path, detalle, peliculas } = useCarteleraLocal()
  return <div onClick={enlaces}><Layout key={url} ruta={path} peliculas={peliculas} ciudades={ciudades} complejos={cines}>
    {error && <div role="alert" className="wrap aviso">{error} <button onClick={() => reintentar(n => n+1)}>Reintentar</button></div>}
    {path === '/mis-boletos' ? <MisBoletosPage/> : /^\/confirmacion\/WEB-[0-9A-F]{40}$/.test(path) ? <ConfirmacionPage folio={path.split('/')[2]}/> : detalle ? <PeliculaDetallePage id={Number(detalle[1])} banner={peliculas.find(p => p.peliculaId === Number(detalle[1]))?.bannerUrl} complejos={cines}/> :
    ['/','/cartelera','/preventas'].includes(path) ? <CarteleraPage peliculas={peliculas} preventas={path === '/preventas'} pendiente={!error && (!catalogo || catalogo.id !== complejo?.complejoId)}/> :
    ['/compra/boletos','/compra/asientos','/compra/dulceria','/compra/pago'].includes(path) ? <CompraLayout ruta={path}/> : path === '/auth' ? <AuthPage/> : path === '/club' ? <ClubCinepolisPage/> : path === '/dulceria' ? <DulceriaPage/> :
    <section className="wrap cartel"><h1>Página no encontrada</h1><a href="/cartelera">Ver cartelera</a></section>}
  </Layout></div>
}
