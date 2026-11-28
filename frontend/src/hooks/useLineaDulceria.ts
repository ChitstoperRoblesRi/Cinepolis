import { useCinepolis } from '../context/CinepolisContext'
import type { ProductoDto } from '../types/cinepolis'
import { cotizadorDulceria } from '../services/dulceria'
import { validadorSeleccion } from '../services/seleccion'

export function useLineaDulceria(cine?: number, cotizador = cotizadorDulceria, validador = validadorSeleccion) {
  const { carrito: c } = useCinepolis()
  return (p: ProductoDto, opciones: number[], unidades: number, combo: boolean, anterior?: string) => {
    if (p.precio == null || !cine || p.reglas.some(r => !validador.evaluar(r, opciones).valido))
      return 'Revisa las opciones de ' + p.nombreProducto + '.'
    const clave = [p.productoId, p.catalogoId, ...opciones.slice().sort((a,b) => a-b)].join('/')
    const otra = c.productos.find(x => x.clave === clave && x.clave !== anterior)
    const cantidad = unidades + (otra?.cantidad ?? 0)
    if (cantidad > 999) return 'Alcanzaste el máximo por artículo.'
    c.actualizarProducto({ clave, productoId:p.productoId, catalogoId:p.catalogoId, complejoId:cine,
      cantidad, opciones, nombre:p.nombreProducto, producto:p, combo, precio:cotizador.calcular(p, opciones) }, anterior)
  }
}
