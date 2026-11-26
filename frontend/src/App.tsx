import { useEffect, useState, type MouseEvent } from 'react'
import { Layout } from './components/Layout'
import { useCinepolis } from './context/CinepolisContext'
import { CarteleraPage } from './pages/CarteleraPage'
import { PeliculaDetallePage } from './pages/PeliculaDetallePage'
import { CompraLayout } from './pages/compra/CompraLayout'
import { ConfirmacionPage } from './pages/ConfirmacionPage'
import { MisBoletosPage } from './pages/MisBoletosPage'
import { DulceriaPage } from './pages/DulceriaPage'
import { AuthPage } from './pages/AuthPage'
import { ClubCinepolisPage } from './pages/ClubCinepolisPage'
import { api } from './services/api'
import { navegar } from './services/navigation'
import type { CarteleraDto, CiudadDto, ComplejoDto } from './types/cinepolis'

function enlaces(e: MouseEvent) {
  const a = (e.target as Element).closest('a')
  if (!a || e.defaultPrevented || e.button || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey ||
    a.target || a.origin !== location.origin || a.protocol !== location.protocol) return
  e.preventDefault()
  navegar(a.pathname + a.search)
}
export function App() {
  const { ciudad, complejo, elegirCiudad, elegirComplejo } = useCinepolis()
  const [url, setUrl] = useState(location.pathname + location.search)
  const [ciudades, setCiudades] = useState<CiudadDto[]>([])
  const [cines, setCines] = useState<ComplejoDto[]>([])
  const [catalogo, setCatalogo] = useState<{id: number; peliculas: CarteleraDto[]} | null>(null)
  const [error,setError] = useState('')
  const [intento, reintentar] = useState(0)
  useEffect(() => {
    const cambio = () => setUrl(location.pathname + location.search)
    window.addEventListener('popstate', cambio)
    return () => window.removeEventListener('popstate', cambio)
  }, [])
  useEffect(() => {
    const abort = new AbortController()
    setError('')
    api.ciudades(abort.signal).then(datos => {
      setCiudades(datos)
      if (!ciudad) elegirCiudad(datos.find(c => c.ciudadId === Number(localStorage.getItem('cinepolis.ciudad'))) ?? datos[0] ?? null)
      if (!datos.length) setError('No hay ciudades disponibles.')
    }).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    return () => abort.abort()
  }, [intento])
  useEffect(() => {
    const abort = new AbortController()
    setCines([])
    if (ciudad) {
      setError('')
      api.complejos(ciudad.ciudadId, abort.signal).then(datos => {
        setCines(datos)
        const restaurado = datos.find(c => c.complejoId === Number(localStorage.getItem('cinepolis.complejo'))) ?? datos[0]
        if (restaurado && restaurado.complejoId !== complejo?.complejoId) elegirComplejo(restaurado)
        if (!datos.length) setError('No hay complejos en esta ciudad.')
      }).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    }
    return () => abort.abort()
  }, [ciudad, intento])
  useEffect(() => {
    const abort = new AbortController()
    if (complejo) {
      setError('')
      api.cartelera({ complejoId: complejo.complejoId }, abort.signal).then(peliculas => setCatalogo({id:complejo.complejoId,peliculas}))
        .catch(e => { if (!abort.signal.aborted) setError(e.message) })
    }
    return () => abort.abort()
  }, [complejo, intento])
  const path = url.split('?')[0]
  const detalle = /^\/peliculas?\/(\d+)$/.exec(path)
  const peliculas = catalogo && catalogo.id === complejo?.complejoId ? catalogo.peliculas : []
  return <div onClick={enlaces}><Layout key={url} ruta={path} peliculas={peliculas} ciudades={ciudades} complejos={cines}>
    {error && <div role="alert" className="wrap aviso">{error} <button onClick={() => reintentar(n => n+1)}>Reintentar</button></div>}
    {path === '/mis-boletos' ? <MisBoletosPage/> : /^\/confirmacion\/WEB-[0-9A-F]{40}$/.test(path) ? <ConfirmacionPage folio={path.split('/')[2]}/> : detalle ? <PeliculaDetallePage id={Number(detalle[1])} banner={peliculas.find(p => p.peliculaId === Number(detalle[1]))?.bannerUrl} complejos={cines}/> :
    ['/','/cartelera','/preventas'].includes(path) ? <CarteleraPage peliculas={peliculas} preventas={path === '/preventas'} pendiente={!error && (!catalogo || catalogo.id !== complejo?.complejoId)}/> :
    ['/compra/boletos','/compra/asientos','/compra/dulceria','/compra/pago'].includes(path) ? <CompraLayout ruta={path}/> : path === '/auth' ? <AuthPage/> : path === '/club' ? <ClubCinepolisPage/> : path === '/dulceria' ? <DulceriaPage/> :
    <section className="wrap cartel"><h1>Página no encontrada</h1><a href="/cartelera">Ver cartelera</a></section>}
  </Layout></div>
}
