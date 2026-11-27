import { useEffect, useState } from 'react'
import { Clock3 } from 'lucide-react'

/** Actualiza sólo el reloj mientras la fachada supervisa la vigencia de las reservas. */
export function TiempoReserva({ vence }: { vence: number }) {
  const [ahora, setAhora] = useState(Date.now)
  useEffect(() => {
    const reloj = window.setInterval(() => setAhora(Date.now()), 500)
    return () => window.clearInterval(reloj)
  }, [vence])
  const segundos = Math.max(0, Math.ceil((vence - ahora) / 1000))
  return <span role="timer" aria-label="Tiempo restante"><Clock3 size={17}/> {Math.floor(segundos/60)}:{String(segundos%60).padStart(2,'0')}</span>
}
