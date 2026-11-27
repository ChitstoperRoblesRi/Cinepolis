import { presentarTarifas } from '../../services/tarifas'
import { Minus, Plus } from 'lucide-react'
import { useCinepolis } from '../../context/CinepolisContext'
import { navegar } from '../../services/navigation'

import { moneda } from '../../services/moneda'
export { moneda } from '../../services/moneda'

export function PasoBoletos() {
  const { carrito: c } = useCinepolis()
  const tarifas = presentarTarifas(c)
  return <section className="paso"><h1>Elige tus boletos</h1><p>Puedes elegir hasta 10 boletos por compra.</p>
    {tarifas.map(({ t, clave, cantidad, zonas, mostrarZona }) => {
      return <div className="linea-tarifa" key={clave}><div><h3>{t.nombre}</h3><span>{moneda(t.precio)}{mostrarZona && ' · Zona ' + zonas.join(', ')}</span></div>
        <div className="contador"><button aria-label={'Quitar boleto ' + t.nombre} disabled={cantidad <= c.usados(clave)} onClick={() => c.ajustar(clave,-1)}><Minus size={16}/></button>
        <output>{cantidad}</output><button aria-label={'Agregar boleto ' + t.nombre} disabled={c.totalBoletos >= 10} onClick={() => c.ajustar(clave,1)}><Plus size={16}/></button></div></div>
    })}
    {!tarifas.length && c.mapa && <p>No hay tarifas disponibles para esta función.</p>}
    {c.asientos.length > 0 && <p>Para quitar un boleto con asiento, libera primero la butaca.</p>}
    <button className="cta" disabled={!c.salaConectada || c.totalBoletos < 1 || c.totalBoletos > 10} onClick={() => navegar('/compra/asientos')}>Elegir asientos</button>
  </section>
}
