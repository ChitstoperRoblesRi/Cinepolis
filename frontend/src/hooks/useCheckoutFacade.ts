import { ErrorSeleccionButaca, tarifaGeneral, limiteBoletos } from '../services/tarifas'
import { navegar } from '../services/navigation'
import { reducirProductos } from '../services/carrito'
import { useMemo,useCallback,useReducer,useEffect,useRef,useState,} from 'react'
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { api,} from '../services/api'
import type { MapaSala, TarifaFuncion, CambioEstadoAsiento, FuncionDto, PagoCompra, LineaDulceria } from '../types/cinepolis'

import type { Reserva, FichaCompra } from '../types/compra'
import type { IFuncionApiService, IOrdenApiService } from '../types/servicios'
const minuto = 60_000
export function claveTarifa(t: TarifaFuncion) { return t.tipoBoletoId + '/' + t.precio }

/** Coordina boletos, reservas SignalR, carrito y confirmación transaccional. */
export function useCheckoutFacade(funciones: IFuncionApiService = api, ordenes: IOrdenApiService = api) {
  const [funcion, setFuncion] = useState<FuncionDto | null>(null)
  const [ficha, setFicha] = useState<FichaCompra | null>(null)
  const [mapa, setMapa] = useState<MapaSala | null>(null)
  const [expirada, setExpirada] = useState(false)
  const [intento, setIntento] = useState(0)

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
  function seleccionar(asientoId: number) {
    const actual = ciclo.current
    const operacion = bloquear(asientoId, actual.propios)
    actual.operaciones.add(operacion)
    return operacion.finally(() => actual.operaciones.delete(operacion))
  }
  async function bloquear(asientoId: number, propios: Set<number>) {
    const hub = sala.current
    if (!lista || !funcion || !hub || Date.now() >= vence || comprando.current) return false
    const enCurso = pendientes.current
    if (reservas.current.size >= limiteBoletos) throw new ErrorSeleccionButaca(`Puedes elegir hasta ${limiteBoletos} boletos por compra`)
    const asiento = mapa?.asientos.find(a => a.asientoId === asientoId)
    if (!asiento || !mapa || enCurso.size || reservas.current.has(asientoId)) return false
    const tarifa = tarifaGeneral(mapa.tarifas, asiento.areaCategoryCode)
    if (!tarifa) throw new ErrorSeleccionButaca('No hay tarifa general disponible para esta zona.')
    enCurso.add(asientoId)
    const inicio = Date.now()
    try {
      if (!await hub.invoke<boolean>('SeleccionarAsiento', funcion.funcionId, asientoId)) return false
      propios.add(asientoId)
      if (hub !== sala.current || Date.now() >= vence) {
        if (hub.state === HubConnectionState.Connected) await hub.invoke('LiberarAsiento', funcion.funcionId, asientoId)
        return false
      }
      reservas.current.set(asientoId, { asientoId, tipoBoletoId: tarifa.tipoBoletoId, tarifa, vence: inicio + 7 * minuto })
      sincronizar()
      return true
    } finally { enCurso.delete(asientoId) }
  }

  async function liberar(id: number) {
    const hub = sala.current
    if (!hub || !funcion || comprando.current || !reservas.current.has(id)) return false
    const ok = await hub.invoke<boolean>('LiberarAsiento', funcion.funcionId, id)
    if (hub === sala.current && ok) { reservas.current.delete(id); sincronizar() }
    return ok
  }

  const actualizarProducto = useCallback((linea: LineaDulceria, anterior?: string) => {
    if (comprando.current) return
    if (!Number.isInteger(linea.cantidad) || linea.cantidad < 0 || linea.cantidad > 999) throw new Error('Cantidad inválida.')
    dispatch({ tipo: 'actualizar', linea, anterior })
  }, [])

  async function comprar(pago: PagoCompra, complejoId?: number) {
    const hub = sala.current
    const boletos = [...reservas.current.values()]
    if (comprando.current) throw new Error('La compra está en proceso.')
    if (!funcion && (!complejoId || !productos.length || productos.some(p => p.complejoId !== complejoId))) throw new Error('Selecciona alimentos del cine elegido.')
    if (funcion && (!hub || !lista || pendientes.current.size || !boletos.length || boletos.length !== totalBoletos ||
        Date.now() >= vence || boletos.some(b => b.vence <= Date.now()))) throw new Error('Revisa la reserva de tus asientos.')
    comprando.current = true
    setProcesandoPago(true)
    try {
      const tokenReserva = funcion && hub ? await hub.invoke<string>('ObtenerTokenReserva') : null
      if (hub !== sala.current) throw new Error('La función seleccionada cambió.')
      const ticket = await ordenes.checkout({ funcionId: funcion?.funcionId ?? null, tokenReserva,
        ...(!funcion && { complejoId }),
        asientos: boletos.map(({ asientoId, tipoBoletoId }) => ({ asientoId, tipoBoletoId })), productos: productos.map(({productoId,catalogoId,cantidad,opciones}) => ({productoId,catalogoId,cantidad,opciones})), pago })
      if (hub === sala.current) { reservas.current.clear(); elegirFuncion(null) }
      window.dispatchEvent(new Event('cinepolis:compra'))
      return ticket
    } finally { comprando.current = false; setProcesandoPago(false) }
  }

  const totalBoletos = asientos.length
  const cantidades = useMemo(() => {
    const resultado: Record<string, number> = {}
    for (const reserva of asientos) {
      const clave = claveTarifa(reserva.tarifa)
      resultado[clave] = (resultado[clave] ?? 0) + 1
    }
    return resultado
  }, [asientos])
  function reconectar() {
    setLista(false)
    setErrorSala('')
    setMapa(null)
    setIntento(i => i + 1)
  }
  const dulceria = useMemo(() => productos.reduce((s,p) => s + p.precio*p.cantidad, 0), [productos])
  const cargo = asientos.reduce((suma, reserva) => suma + reserva.tarifa.cargo, 0)
  function puedeContinuar(paso: number) {
    if (procesandoPago) return false
    if (!funcion) return productos.length > 0
    return lista && totalBoletos > 0 && paso >= 1
  }

  function continuar(paso: number) {
    if (!puedeContinuar(paso)) return
    if (!funcion || paso === 2) navegar('/compra/pago')
    else if (paso === 1) navegar('/compra/dulceria')
  }

  return { continuar, puedeContinuar, procesandoPago, dulceria, cargo, ficha, mapa, cantidades, totalBoletos, expirada, reconectar, funcion, elegirFuncion, asientos, productos, actualizarProducto, seleccionar, liberar, comprar,
    vence, salaConectada: lista, errorSala }
}

