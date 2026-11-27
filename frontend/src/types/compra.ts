import type { LineaDulceria } from './cinepolis'
import type { BoletoCompra } from './cinepolis'
export interface Reserva extends BoletoCompra { vence: number }

export interface FichaCompra { titulo: string; posterUrl: string | null; fecha: string; hora: string }

export type AccionProductos =
  | { tipo: 'cambiarFuncion'; complejoId?: number }
  | { tipo: 'actualizar'; linea: LineaDulceria }
