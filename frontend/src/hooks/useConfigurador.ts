import { seleccionInicial } from '../services/preseleccion'
import { seleccionarOpcion, validadorSeleccion } from '../services/seleccion'
import type { ICotizadorDulceria, IValidadorSeleccionDulceria } from '../types/servicios'
import { useState } from 'react'
import type { ProductoDto, Opcion } from '../types/cinepolis'
import { cotizadorDulceria, EstrategiaVisualPrimerGrupo, EstrategiaVisualGrupoSecundario } from '../services/dulceria'

/** Controla las selecciones por bloque y la cotización de cada paso. */
export function useConfigurador(p: ProductoDto, cotizador: ICotizadorDulceria = cotizadorDulceria,
  validador: IValidadorSeleccionDulceria = validadorSeleccion, inicial?: { opciones: number[]; cantidad: number }) {
  const [ids, setIds] = useState<number[]>(() => inicial?.opciones ?? seleccionInicial(p))
  const [unidades, setUnidades] = useState(inicial?.cantidad ?? 1)
  const total = unidades * cotizador.calcular(p, ids)
  const grupos = p.reglas.map((regla, indice) => {
    const { opcional, maximo, valido, indicador, admite } = validador.evaluar(regla, ids)
    const conOpcion = (o: Opcion) => seleccionarOpcion(p, regla, ids, o, maximo)
    const estrategia = indice === 0 ? EstrategiaVisualPrimerGrupo : EstrategiaVisualGrupoSecundario
    const etiquetas = estrategia.etiquetas(regla.opciones, p.precio ?? 0, o => cotizador.calcular(p, conOpcion(o)))
    function alternar(o: Opcion) {
      if (!admite(o.opcionId)) return
      if (ids.includes(o.opcionId) && (maximo !== 1 || opcional)) setIds(ids.filter(x => x !== o.opcionId))
      else setIds(conOpcion(o))
    }
    return { regla, opcional, indicador, admite, valido, alternar, etiquetas }
  })
  return { ids, unidades, setUnidades, grupos, valido: grupos.every(g => g.valido), total }
}
