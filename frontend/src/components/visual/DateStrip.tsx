import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useRef } from 'react'
import { fechaISO } from '../../hooks/useCartelera'
import styles from './DateStrip.module.css'

interface Props {
  dias: Date[]
  fecha: string
  elegir: (fecha: string) => void
}

export default function DateStrip({ dias, fecha, elegir }: Props) {
  const strip = useRef<HTMLDivElement>(null)
  function desplazar(direccion: number) {
    const lista = strip.current
    lista?.scrollBy({ left: direccion * lista.clientWidth * 0.8, behavior: 'smooth' })
  }
  return <div className={styles.dates}>
    <button className={styles.arrow} aria-label="Días anteriores" onClick={() => desplazar(-1)}><ChevronLeft/></button>
    <div className={styles.strip} ref={strip} role="group" aria-label="Fecha de la función">
      {dias.map((dia, indice) => {
        const valor = fechaISO(dia)
        return <button key={valor} aria-pressed={valor === fecha} onClick={() => elegir(valor)}>
          <span>{indice === 0 ? 'Hoy' : indice === 1 ? 'Mañana' : dia.toLocaleDateString('es-MX', { weekday: 'short' })}</span>
          <strong>{dia.getDate()}</strong><small>{dia.toLocaleDateString('es-MX', { month: 'short' })}</small>
        </button>
      })}
      <button aria-pressed={!fecha} onClick={() => elegir('')}><span>Todas las fechas</span></button>
    </div>
    <button className={styles.arrow} aria-label="Días siguientes" onClick={() => desplazar(1)}><ChevronRight/></button>
  </div>
}
