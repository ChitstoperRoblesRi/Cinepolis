import { reducirProductos } from '../services/carrito'
import { useMemo,useCallback,useReducer,useEffect,useRef,useState,} from 'react'
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { api,} from '../services/api'
import type { MapaSala, TarifaFuncion, BoletoCompra, CambioEstadoAsiento, FuncionDto, PagoCompra, LineaDulceria } from '../types/cinepolis'

import type { Reserva, FichaCompra } from '../types/compra'
import type { IFuncionApiService, IOrdenApiService } from '../types/servicios'
const minuto = 60_000
export function claveTarifa(t: TarifaFuncion) { return t.tipoBoletoId + '/' + t.precio }

/** Coordina boletos, reservas SignalR, carrito y confirmación transaccional. */
export function useCheckoutFacade(funciones: IFuncionApiService = api, ordenes: IOrdenApiService = api) {
  const [funcion, setFuncion] = useState<FuncionDto | null>(null)
  const [ficha, setFicha] = useState<FichaCompra | null>(null)
  const [mapa, setMapa] = useState<MapaSala | null>(null)
  const [cantidades, setCantidades] = useState<Record<string, number>>({})
  const [expirada, setExpirada] = useState(false)
  const [intento, setIntento] = useState(0)
  const totalBoletos = useMemo(() => Object.values(cantidades).reduce((a, b) => a + b, 0), [cantidades])
  const [productos, dispatch] = useReducer(reducirProductos, [])
  const [asientos, setAsientos] = useState<Reserva[]>([])
  const [vence, setVence] = useState(0)
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
    dispatch({ tipo: 'cambiarFuncion', complejoId: nueva?.complejoId })
    setLista(false)
    setErrorSala('')
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
      const datos = await funciones.asientos(funcion.funcionId)
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
  }, [funcion, intento, funciones])

  useEffect(() => {
    if (!vence) return
    const timer = window.setInterval(() => {
      const t = Date.now()
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

  const actualizarProducto = useCallback((linea: LineaDulceria) => {
    if (comprando.current) return
    if (!Number.isInteger(linea.cantidad) || linea.cantidad < 0 || linea.cantidad > 999) throw new Error('Cantidad inválida.')
    dispatch({ tipo: 'actualizar', linea })
  }, [])

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
      const ticket = await ordenes.checkout({ funcionId: funcion.funcionId, tokenReserva,
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
  const dulceria = useMemo(() => productos.reduce((s,p) => s + p.precio*p.cantidad, 0), [productos])
  const cargo = useMemo(() => asientos.reduce((s,b) => {
    const area = mapa?.asientos.find(a => a.asientoId === b.asientoId)?.areaCategoryCode
    return s + (mapa?.tarifas.find(t => t.tipoBoletoId === b.tipoBoletoId && t.areaCategoryCode === area)?.cargo ?? 0)
  }, 0), [asientos, mapa])
  return { procesandoPago, dulceria, cargo, ficha, mapa, cantidades, totalBoletos, ajustar, usados, expirada, reconectar, funcion, elegirFuncion, asientos, productos, actualizarProducto, seleccionar, liberar, comprar,
    vence, salaConectada: lista, errorSala }
}

