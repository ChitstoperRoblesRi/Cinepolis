import { useState, useEffect } from 'react'
import { api } from '../services/api'
import { useLineaDulceria } from './useLineaDulceria'
import type { CategoriaDulceriaDto, ProductoDto } from '../types/cinepolis'

import { esCombo, disponible, normalizarProducto, cotizadorDulceria } from '../services/dulceria'
import { validadorSeleccion } from '../services/seleccion'
import type { ICotizadorDulceria, IDulceriaApiService, IValidadorSeleccionDulceria } from '../types/servicios'
/** Carga el catálogo del cine y añade líneas con la cotización del configurador. */
export function useCatalogo(cine?: number, servicio: IDulceriaApiService = api,
  cotizador: ICotizadorDulceria = cotizadorDulceria, validador: IValidadorSeleccionDulceria = validadorSeleccion) {
  const guardar = useLineaDulceria(cine, cotizador, validador)
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
    const error = guardar(p, opciones, unidades, esCombo(p, grupo?.nombreCategoria ?? ''))
    if (error) { setAviso(error); return }
    setProducto(null); setAviso(p.nombreProducto + ' añadido.')
  }
  const grupo = datos?.find(g => g.categoriaId === categoria)
  return { datos, categoria, setCategoria, producto, setProducto, aviso, setAviso, error, setError, setIntento, agregar, grupo }
}
