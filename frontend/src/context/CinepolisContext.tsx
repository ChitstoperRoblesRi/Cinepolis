import { createContext, useContext, useEffect, useRef, useState, type ReactNode } from 'react'
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { api, readSession, saveSession } from '../services/api'
import type { TicketDigital, MapaSala, TarifaFuncion, BoletoCompra, CambioEstadoAsiento, CiudadDto, ComplejoDto, FuncionDto, PagoCompra, PerfilClub, LineaDulceria, RespuestaLogin } from '../types/cinepolis'

const minuto = 60_000
interface Reserva extends BoletoCompra { vence: number }

interface FichaCompra { titulo: string; posterUrl: string | null; fecha: string; hora: string }
export function claveTarifa(t: TarifaFuncion) { return t.tipoBoletoId + '/' + t.precio }

function useCarrito() {
  const [funcion, setFuncion] = useState<FuncionDto | null>(null)
  const [ficha, setFicha] = useState<FichaCompra | null>(null)
  const [mapa, setMapa] = useState<MapaSala | null>(null)
  const [cantidades, setCantidades] = useState<Record<string, number>>({})
  const [expirada, setExpirada] = useState(false)
  const [intento, setIntento] = useState(0)
  const totalBoletos = Object.values(cantidades).reduce((a, b) => a + b, 0)
  const [productos, setProductos] = useState<LineaDulceria[]>([])
  const [asientos, setAsientos] = useState<Reserva[]>([])
  const [vence, setVence] = useState(0)
  const [ahora, setAhora] = useState(Date.now)
  const [lista, setLista] = useState(false)
  const [errorSala, setErrorSala] = useState('')
  const sala = useRef<ReturnType<HubConnectionBuilder['build']> | null>(null)
  const reservas = useRef(new Map<number, Reserva>())
  const pendientes = useRef(new Set<number>())
  const [procesandoPago, setProcesandoPago] = useState(false)
  const comprando = useRef(false)

  function sincronizar() { setAsientos([...reservas.current.values()]) }
  function elegirFuncion(nueva: FuncionDto | null, detalle: FichaCompra | null = null) {
    sala.current = null
    setFicha(detalle)
    setMapa(null)
    setCantidades({})
    setExpirada(false)
    pendientes.current = new Set()
    reservas.current.clear()
    sincronizar()
    setProductos(prev => nueva ? prev.filter(p => p.complejoId === nueva.complejoId) : [])
    setLista(false)
    setErrorSala('')
    setAhora(Date.now())
    setVence(nueva ? Date.now() + 7 * minuto : 0)
    setFuncion(nueva ? { ...nueva } : null)
  }

  useEffect(() => {
    if (!funcion) return
    const hub = new HubConnectionBuilder().withUrl(import.meta.env.VITE_HUB_URL).build()
    const cambios = new Map<number, string>()
    const propios = new Set<number>()
    const operaciones = new Set<Promise<unknown>>()
    ciclo.current = { propios, operaciones }
    sala.current = hub
    hub.on('AsientoActualizado', (cambio: CambioEstadoAsiento) => {
      if (sala.current !== hub || cambio.funcionId !== funcion.funcionId) return
      cambios.set(cambio.asientoId, cambio.estado)
      setMapa(prev => prev && ({ ...prev, asientos: prev.asientos.map(a => a.asientoId === cambio.asientoId ? { ...a, estado: cambio.estado } : a) }))
      if (cambio.estado !== 'reservado') {
        propios.delete(cambio.asientoId)
        reservas.current.delete(cambio.asientoId)
        sincronizar()
      }
    })
    hub.onclose(() => {
      if (sala.current !== hub) return
      reservas.current.clear()
      sincronizar()
      setLista(false)
      setErrorSala('Se perdió la conexión con la sala. Vuelve a elegir la función.')
    })
    void (async () => {
      await hub.start()
      if (sala.current !== hub) return
      await hub.invoke('UnirseAFuncion', funcion.funcionId)
      const datos = await api.asientos(funcion.funcionId)
      if (sala.current !== hub) return
      setMapa({ ...datos, asientos: datos.asientos.map(a => ({ ...a, estado: cambios.get(a.asientoId) ?? a.estado })) })
      setLista(true)
    })().catch(() => {
      if (sala.current === hub) setErrorSala('No se pudo conectar con la sala.')
    })
    return () => {
      if (sala.current === hub) sala.current = null
      void (async () => {
        await Promise.allSettled([...operaciones])
        if (hub.state === HubConnectionState.Connected) {
          await Promise.allSettled([...propios].map(id => hub.invoke('LiberarAsiento', funcion.funcionId, id)))
        }
        await hub.stop()
      })()
    }
  }, [funcion, intento])

  useEffect(() => {
    if (!vence) return
    const timer = window.setInterval(() => {
      const t = Date.now()
      setAhora(t)
      if (t >= vence && !comprando.current) { elegirFuncion(null); setExpirada(true); return }
      let cambio = false
      for (const [id, asiento] of reservas.current) {
        if (asiento.vence <= t) { reservas.current.delete(id); cambio = true }
      }
      if (cambio) sincronizar()
    }, 500)
    return () => clearInterval(timer)
  }, [vence, funcion])

  const ciclo = useRef({ propios: new Set<number>(), operaciones: new Set<Promise<unknown>>() })
  function seleccionar(boleto: BoletoCompra) {
    const actual = ciclo.current
    const operacion = bloquear(boleto, actual.propios)
    actual.operaciones.add(operacion)
    return operacion.finally(() => actual.operaciones.delete(operacion))
  }
  async function bloquear(boleto: BoletoCompra, propios: Set<number>) {
    const hub = sala.current
    if (!lista || !funcion || !hub || Date.now() >= vence || comprando.current) return false
    const enCurso = pendientes.current
    const asiento = mapa?.asientos.find(a => a.asientoId === boleto.asientoId)
    const tarifa = mapa?.tarifas.find(t => t.tipoBoletoId === boleto.tipoBoletoId && t.areaCategoryCode === asiento?.areaCategoryCode)
    if (!tarifa || enCurso.size || reservas.current.has(boleto.asientoId) ||
        usados(claveTarifa(tarifa)) >= (cantidades[claveTarifa(tarifa)] ?? 0)) return false
    enCurso.add(boleto.asientoId)
    const inicio = Date.now()
    try {
      if (!await hub.invoke<boolean>('SeleccionarAsiento', funcion.funcionId, boleto.asientoId)) return false
      propios.add(boleto.asientoId)
      if (hub !== sala.current || Date.now() >= vence) {
        if (hub.state === HubConnectionState.Connected) await hub.invoke('LiberarAsiento', funcion.funcionId, boleto.asientoId)
        return false
      }
      reservas.current.set(boleto.asientoId, { ...boleto, vence: inicio + 7 * minuto })
      sincronizar()
      return true
    } finally { enCurso.delete(boleto.asientoId) }
  }

  async function liberar(id: number) {
    const hub = sala.current
    if (!hub || !funcion || comprando.current || !reservas.current.has(id)) return false
    const ok = await hub.invoke<boolean>('LiberarAsiento', funcion.funcionId, id)
    if (hub === sala.current && ok) { reservas.current.delete(id); sincronizar() }
    return ok
  }

  function actualizarProducto(linea: LineaDulceria) {
    if (comprando.current) return
    if (!Number.isInteger(linea.cantidad) || linea.cantidad < 0 || linea.cantidad > 999) throw new Error('Cantidad inválida.')
    setProductos(prev => [...prev.filter(p => p.clave !== linea.clave), ...(linea.cantidad ? [linea] : [])])
  }

  async function comprar(pago: PagoCompra) {
    const hub = sala.current
    const boletos = [...reservas.current.values()]
    if (!hub || !funcion || !lista || comprando.current || pendientes.current.size || !boletos.length || boletos.length !== totalBoletos ||
        Date.now() >= vence || boletos.some(b => b.vence <= Date.now())) throw new Error('Revisa la reserva de tus asientos.')
    comprando.current = true
    setProcesandoPago(true)
    try {
      const tokenReserva = await hub.invoke<string>('ObtenerTokenReserva')
      if (hub !== sala.current) throw new Error('La función seleccionada cambió.')
      const ticket = await api.checkout({ funcionId: funcion.funcionId, tokenReserva,
        asientos: boletos.map(({ asientoId, tipoBoletoId }) => ({ asientoId, tipoBoletoId })), productos: productos.map(({productoId,catalogoId,cantidad,opciones}) => ({productoId,catalogoId,cantidad,opciones})), pago })
      if (hub === sala.current) { reservas.current.clear(); elegirFuncion(null) }
      window.dispatchEvent(new Event('cinepolis:compra'))
      return ticket
    } finally { comprando.current = false; setProcesandoPago(false) }
  }

  function usados(clave: string) {
    return [...reservas.current.values()].filter(b => mapa?.tarifas.some(t =>
      t.tipoBoletoId === b.tipoBoletoId && claveTarifa(t) === clave &&
      mapa.asientos.some(a => a.asientoId === b.asientoId && a.areaCategoryCode === t.areaCategoryCode))).length
  }
  function ajustar(clave: string, delta: 1 | -1) {
    if (pendientes.current.size) return
    setCantidades(prev => {
      const nueva = (prev[clave] ?? 0) + delta
      if (nueva < usados(clave) || Object.values(prev).reduce((a,b) => a+b, 0) + delta > 10) return prev
      return { ...prev, [clave]: nueva }
    })
  }
  function reconectar() {
    setLista(false)
    setErrorSala('')
    setMapa(null)
    setIntento(i => i + 1)
  }
  const dulceria = productos.reduce((s,p) => s + p.precio*p.cantidad, 0)
  const cargo = asientos.reduce((s,b) => {
    const area = mapa?.asientos.find(a => a.asientoId === b.asientoId)?.areaCategoryCode
    return s + (mapa?.tarifas.find(t => t.tipoBoletoId === b.tipoBoletoId && t.areaCategoryCode === area)?.cargo ?? 0)
  }, 0)
  return { procesandoPago, dulceria, cargo, ficha, mapa, cantidades, totalBoletos, ajustar, usados, expirada, reconectar, funcion, elegirFuncion, asientos, productos, actualizarProducto, seleccionar, liberar, comprar,
    segundosRestantes: Math.max(0, Math.ceil((vence - ahora) / 1000)), salaConectada: lista, errorSala }
}

function useEstado() {
  const [ciudad, setCiudad] = useState<CiudadDto | null>(null)
  const [complejo, setComplejo] = useState<ComplejoDto | null>(null)
  const [sesion, setSesion] = useState(readSession)
  const [perfil, setPerfil] = useState<PerfilClub | null>(null)
  const [intentoPerfil,setIntentoPerfil] = useState(0)
  const [errorPerfil, setErrorPerfil] = useState('')
  const carrito = useCarrito()
  const [confirmacion, setConfirmacion] = useState<{ ticket: TicketDigital; sesion: typeof sesion } | null>(null)
  function guardarConfirmacion(ticket: TicketDigital) { setConfirmacion({ ticket, sesion }) }

  useEffect(() => {
    const actualizar = () => { setConfirmacion(null); setPerfil(null); setSesion(readSession()) }
    window.addEventListener('cinepolis:sesion', actualizar)
    return () => window.removeEventListener('cinepolis:sesion', actualizar)
  }, [])
  useEffect(() => {
    setErrorPerfil('')
    if (!sesion) return
    const abort = new AbortController()
    const timeout = window.setTimeout(() => saveSession(null), Math.max(0, Date.parse(sesion.expiraUtc) - Date.now()))
    const cargar = () => api.perfil(abort.signal).then(p => { if (!abort.signal.aborted) setPerfil(p) })
      .catch(() => { if (!abort.signal.aborted) setErrorPerfil('No se pudo cargar el perfil.') })
    void cargar()
    window.addEventListener('cinepolis:compra', cargar)
    return () => { window.removeEventListener('cinepolis:compra', cargar); abort.abort(); clearTimeout(timeout) }
  }, [sesion,intentoPerfil])
  function elegirCiudad(valor: CiudadDto | null) {
    if (valor?.ciudadId === ciudad?.ciudadId) return
    if (valor) localStorage.setItem('cinepolis.ciudad', String(valor.ciudadId))
    setCiudad(valor); setComplejo(null); carrito.elegirFuncion(null)
  }
  function elegirComplejo(valor: ComplejoDto | null) {
    if (valor && valor.ciudadId !== ciudad?.ciudadId) throw new Error('El cine pertenece a otra ciudad.')
    if (valor) localStorage.setItem('cinepolis.complejo', String(valor.complejoId))
    setComplejo(valor); carrito.elegirFuncion(null)
  }
  return { ultimaCompra: confirmacion?.sesion === sesion ? confirmacion.ticket : undefined, guardarConfirmacion, ciudad, complejo, elegirCiudad, elegirComplejo, carrito, sesion, perfil, errorPerfil,
    recargarPerfil: () => setIntentoPerfil(i => i+1),
    iniciarSesion: (resultado: RespuestaLogin) => saveSession(resultado), cerrarSesion: () => saveSession(null) }
}

const Context = createContext<ReturnType<typeof useEstado> | null>(null)
export function CinepolisProvider({ children }: { children: ReactNode }) {
  const estado = useEstado()
  return <Context.Provider value={estado}>{children}</Context.Provider>
}
export function useCinepolis() {
  const estado = useContext(Context)
  if (!estado) throw new Error('Se requiere CinepolisProvider.')
  return estado
}
