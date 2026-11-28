import type { CategoriaDulceriaDto, Checkout, DetallePeliculaDto, MapaSala, TicketDigital, Opcion, ProductoDto, Regla } from './cinepolis'

export interface ICotizadorDulceria {
  calcular(producto: ProductoDto, opciones: number[]): number
}
export interface IValidadorSeleccionDulceria {
  evaluar(regla: Regla, opciones: number[]): {
    opcional: boolean; maximo: number; valido: boolean; indicador: string; admite: (opcionId: number) => boolean
  }
}

/** Puertos HTTP consumidos por cada caso de uso. */
export interface IPeliculaApiService { pelicula(id: number, signal?: AbortSignal): Promise<DetallePeliculaDto> }
export interface IFuncionApiService { asientos(id: number): Promise<MapaSala> }
export interface IDulceriaApiService { dulceria(id?: number, signal?: AbortSignal): Promise<CategoriaDulceriaDto[]> }
export interface IOrdenApiService {
  checkout(compra: Checkout): Promise<TicketDigital>
  ticket(folio: string): Promise<TicketDigital>
  misBoletos(): Promise<string[]>
}
/** Ajusta el acumulador de un componente, expresado en centavos. */
export interface ICalculoItemDulceriaStrategy { acumular(actual: number | undefined, extra: number): number }
/** Devuelve las etiquetas en el mismo orden que las opciones del grupo. */
export interface IVisualizacionPrecioStrategy {
  etiquetas(opciones: Opcion[], base: number, cotizar: (opcion: Opcion) => number): string[]
}

export interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

