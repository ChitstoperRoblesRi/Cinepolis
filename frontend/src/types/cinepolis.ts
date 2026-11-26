export interface FiltrosCartelera {
  complejoId?: number | null
  fecha?: string | null
  formato?: string | null
  idioma?: string | null
  clasificacion?: string | null
}

export interface FuncionDto {
  funcionId: number
  complejoId: number
  nombreComercial: string
  salaId: number
  nombreSala: string
  idioma: string
  horaTermino: string
  esPreventa: boolean
}

export interface HorarioDto {
  fecha: string
  horaInicio: string
  funciones: FuncionDto[]
}

export interface FormatoDto {
  tipoSala: string
  nombreFormato: string
  horarios: HorarioDto[]
}

export interface CarteleraDto {
  genero: string
  duracionMin: number
  categoria: string
  bannerUrl: string | null
  trailerUrl: string | null
  peliculaId: number
  titulo: string
  clasificacion: string
  posterUrl: string | null
  formatos: FormatoDto[]
}

export interface DetallePeliculaDto {
  directores: readonly string[]
  reparto: readonly string[]
  peliculaId: number
  titulo: string
  clasificacion: string
  sinopsis: string
  trailerUrl: string | null
  posterUrl: string | null
  genero: string
  duracionMin: number
  formatos: FormatoDto[]
}

export interface Opcion { opcionId: number; nombre: string; extra: number; precioVariante: number | null }
export interface Regla { reglaId: number; titulo: string; grupoTitulo: string | null; tipoModificador: string; bloquePalomitas: number | null; min: number; max: number; opciones: Opcion[] }
export interface ProductoDto {
  catalogoId: number | null
  precio: number | null
  reglas: Regla[]
  productoId: number
  categoriaId: number
  nombreProducto: string
  estructura: string
  descripcion: string | null
  imagenUrl: string | null
}

export interface CategoriaDulceriaDto {
  categoriaId: number
  nombreCategoria: string
  productos: ProductoDto[]
  combos: ProductoDto[]
}

export interface SalaLectura {
  salaId: number
  complejoId: number
  nombreSala: string
  tipoSala: string
  capacidadAsientos: number
}

export interface AsientoLectura {
  asientoId: number
  fila: string
  columna: number
  numero: string | null
  tipoAsiento: string
  esAccesible: boolean
  estado: string
  areaCategoryCode: string
}

export interface TarifaFuncion {
  cargo: number
  tipoBoletoId: number
  nombre: string
  areaCategoryCode: string
  precio: number
}

export interface MapaSala {
  tarifas: TarifaFuncion[]
  funcionId: number
  sala: SalaLectura
  asientos: AsientoLectura[]
}

export interface CambioEstadoAsiento {
  funcionId: number
  asientoId: number
  estado: string
}

export interface SolicitarCodigo {
  correo: string
}

export interface Login {
  correo: string
  codigo: string
}

export interface Registro {
  correo: string
  codigo: string
  usuarioCc: string
  nombre: string
  apellidos: string
  fechaNacimiento: string | null
}

export interface CompraClub {
  folio: string
  titulo: string
  complejo: string
  horario: string
  total: number
  estado: string
  activa: boolean
}

export interface PerfilClub {
  visitasSemestreActual: number
  siguienteNivel: string | null
  metaVisitas: number | null
  socioId: number
  usuarioCc: string
  numeroTarjeta: string
  nombre: string
  apellidos: string
  correo: string
  puntosAcumulados: number
  puntosVencenFecha: string
  nivelId: number
  nombreNivel: string
  porcentajeAcumulacionPuntos: number
}

export interface BoletoCompra {
  asientoId: number
  tipoBoletoId: number
}

export interface ProductoCompra {
  opciones?: number[]
  productoId: number
  cantidad: number
  catalogoId?: number | null
}

export interface LineaDulceria extends ProductoCompra {
  clave: string; nombre: string; precio: number; sabores: string[]; complejoId: number
}

export interface PagoCompra {
  nombre?: string
  correo?: string
  tarjetaClub?: string
  totalEsperado?: number
  formaPago: 'Tarjeta' | 'PayPal' | 'ClickToPay' | 'Aplazo' | 'Puntos'
}

export interface Checkout {
  funcionId: number
  tokenReserva: string
  asientos: BoletoCompra[]
  productos: ProductoCompra[]
  pago: PagoCompra
}

export interface BoletoTicket {
  nombreComercial: string
  titulo: string
  horario: string
  nombreSala: string
  funcionId: number
  asientoId: number
  fila: string
  columna: number
  numero: string | null
  tipoBoleto: string
  precioUnitario: number
}

export interface ProductoTicket {
  opciones: string[]
  productoId: number
  nombreProducto: string
  cantidad: number
  precioUnitario: number
}

export interface CabeceraTicket {
  folio: string
  estado: string
  fechaVenta: string
  formaPago: string
  cargoServicio: number
  importeTotal: number
}

export interface Ticket {
  orden: CabeceraTicket
  subtotalBoletos: number
  subtotalDulceria: number
  puntosAcreditados: number
  boletos: BoletoTicket[]
  productos: ProductoTicket[]
}

export interface CiudadDto {
  ciudadId: number
  nombreCiudad: string
}

export interface ComplejoDto {
  complejoId: number
  ciudadId: number
  nombreComercial: string
  tipoComplejo: string
  direccion: string
  codigoPostal: string
  slug: string
  imagenUrl: string | null
}

export interface TicketDigital {
  folio: string
  qrPayload: string
  ticket: Ticket
  validacionQr: string
}

export interface RespuestaLogin {
  accessToken: string
  expiraUtc: string
}
