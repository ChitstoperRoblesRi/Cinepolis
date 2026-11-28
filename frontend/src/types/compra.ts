import type { LineaDulceria } from './cinepolis'
import type { Sesion } from '../services/sesiones'
import type { BoletoCompra, TarifaFuncion } from './cinepolis'
export interface Reserva extends BoletoCompra { vence: number; tarifa: TarifaFuncion }

export interface FichaCompra { horarios: Sesion[]; titulo: string; formato: string; posterUrl: string | null; fecha: string; hora: string }

export type AccionProductos =
  | { tipo: 'cambiarFuncion'; complejoId?: number }
  | { tipo: 'actualizar'; linea: LineaDulceria; anterior?: string }
