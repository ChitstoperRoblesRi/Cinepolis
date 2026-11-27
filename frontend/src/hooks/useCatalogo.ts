import { useState, useEffect } from 'react'
import { api } from '../services/api'
import { useCinepolis } from '../context/CinepolisContext'
import type { CategoriaDulceriaDto, ProductoDto } from '../types/cinepolis'

import { disponible, normalizarProducto, precioUnitario } from '../services/dulceria'
import type { IDulceriaApiService } from '../types/servicios'
/** Carga el catálogo del cine y añade líneas con la cotización del configurador. */
export function useCatalogo(cine?: number, servicio: IDulceriaApiService = api) {
  const { carrito: c } = useCinepolis()
  const [datos, setDatos] = useState<CategoriaDulceriaDto[]>()
  const [categoria, setCategoria] = useState(0)
  const [producto, setProducto] = useState<ProductoDto | null>(null)
  const [aviso, setAviso] = useState('')
  const [error, setError] = useState('')
  const [intento, setIntento] = useState(0)
  useEffect(() => {
    const abort = new AbortController()
    if (cine) servicio.dulceria(cine, abort.signal).then(result => {
      if (abort.signal.aborted) return
      const categorias = result.map(g => ({ ...g, combos: g.combos.map(normalizarProducto).filter(disponible),
        productos: g.productos.map(normalizarProducto).filter(disponible) }))
        .filter(g => g.combos.length + g.productos.length > 0)
      setDatos(categorias)
      setCategoria(categorias[0]?.categoriaId ?? 0)
    }).catch(e => { if (!abort.signal.aborted) setError(e.message) })
    return () => abort.abort()
  }, [cine,intento,servicio])
  function agregar(p: ProductoDto, opciones: number[], unidades = 1) {
    if (p.precio == null || !cine) return
    const seleccion = p.reglas.flatMap(r => r.opciones).filter(o => opciones.includes(o.opcionId))
    const clave = [p.productoId,p.catalogoId,...opciones.slice().sort((a,b) => a-b)].join('/')
    const cantidad = unidades + (c.productos.find(x => x.clave === clave)?.cantidad ?? 0)
    if (cantidad > 999) { setAviso('Alcanzaste el máximo por artículo.'); return }
    c.actualizarProducto({clave,productoId:p.productoId,catalogoId:p.catalogoId,complejoId:cine,cantidad,opciones,
      nombre:p.nombreProducto,sabores:seleccion.map(o => o.nombre),precio:precioUnitario(p, opciones)})
    setProducto(null); setAviso(p.nombreProducto + ' añadido.')
  }
  const grupo = datos?.find(g => g.categoriaId === categoria)
  return { datos, categoria, setCategoria, producto, setProducto, aviso, setAviso, error, setError, setIntento, agregar, grupo }
}
