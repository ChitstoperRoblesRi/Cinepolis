import { Accessibility, Crown } from 'lucide-react'
import type { AsientoLectura } from '../types/cinepolis'
import type { ReactNode } from 'react'
interface RenderButaca { admite(a: AsientoLectura): boolean; contenido(a: AsientoLectura): ReactNode }
const ButacaAccesible: RenderButaca = { admite: a => a.esAccesible, contenido: () => <Accessibility aria-hidden="true" size={16}/> }
const ButacaVip: RenderButaca = { admite: a => a.tipoAsiento.trim().toLowerCase() === 'vip',
  contenido: a => <><Crown aria-hidden="true" size={12}/><span>{a.numero}</span></> }
const ButacaNormal: RenderButaca = { admite: () => true, contenido: a => <span>{a.numero}</span> }
const tipos = [ButacaAccesible, ButacaVip, ButacaNormal]
const PasilloVacio = null
/** Asigna a cada posición física su estado, icono y etiqueta accesible. */
export function AsientoCellFactory({ a, mia, ocupado, conectada, completo, columnas, pulsar }: {
  a?: AsientoLectura; mia: boolean; ocupado: boolean; conectada: boolean; completo: boolean;
  columnas: number; pulsar: (a: AsientoLectura) => Promise<void>
}) {
  if (!a) return PasilloVacio
  const estado = mia ? 'seleccionado' : a.estado
  const codigo = a.numero ? a.fila + a.numero : `Fila ${a.fila}, sin etiqueta`
  const etiqueta = codigo + ' · ' + (a.esAccesible ? 'Silla de ruedas' : a.tipoAsiento) + ' · ' + estado
  return <button key={a.asientoId} className="butaca" data-estado={estado} data-tipo={a.tipoAsiento}
    style={{gridColumn: columnas - a.columna + 2}} aria-pressed={mia} aria-label={etiqueta} title={etiqueta}
    disabled={ocupado || !conectada || (!mia && (completo || a.estado !== 'disponible'))}
    onClick={() => void pulsar(a)}>
    {tipos.find(tipo => tipo.admite(a))!.contenido(a)}
  </button>
}
