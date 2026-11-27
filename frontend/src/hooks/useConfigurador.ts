import { seleccionInicial, seleccionarOpcion, limiteSeleccion } from '../services/seleccion'
import { useState } from 'react'
import type { ProductoDto, Opcion } from '../types/cinepolis'
import { saboresPalomitas, precioUnitario, EstrategiaVisualPrimerGrupo, EstrategiaVisualGrupoSecundario } from '../services/dulceria'

/** Controla las selecciones por bloque y la cotización de cada paso. */
export function useConfigurador(p: ProductoDto, combo: boolean) {
  const [ids, setIds] = useState<number[]>(() => seleccionInicial(p, combo))
  const [paso, setPaso] = useState(0)
  const [unidades, setUnidades] = useState(1)
  const regla = p.reglas[paso]
  const esSabor = saboresPalomitas(regla)
  const maximo = limiteSeleccion(p, regla, ids)
  const seleccionadas = regla.opciones.filter(o => ids.includes(o.opcionId)).length
  const valido = seleccionadas >= regla.min && seleccionadas <= maximo
  const ultimo = paso === p.reglas.length - 1
  const total = unidades * precioUnitario(p, ids)
  const conOpcion = (o: Opcion) => seleccionarOpcion(p, combo, regla, ids, o, maximo)
  const estrategia = paso === 0 ? EstrategiaVisualPrimerGrupo : EstrategiaVisualGrupoSecundario
  const etiquetas = estrategia.etiquetas(regla.opciones, p.precio ?? 0, o => precioUnitario(p, conOpcion(o)))
  function alternar(o: Opcion) {
    if (ids.includes(o.opcionId) && (maximo !== 1 || regla.min === 0)) setIds(ids.filter(x => x !== o.opcionId))
    else setIds(conOpcion(o))
  }
  return { ids, paso, setPaso, unidades, setUnidades, regla, esSabor, maximo, seleccionadas, valido, ultimo, total, alternar, etiquetas }
}
