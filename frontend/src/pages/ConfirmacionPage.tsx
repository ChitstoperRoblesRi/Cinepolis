import { useEffect, useState } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import { Printer } from 'lucide-react'
import { api } from '../services/api'
import type { TicketDigital } from '../types/cinepolis'
import { moneda } from './compra/PasoBoletos'
import { useCinepolis } from '../context/CinepolisContext'

export function ConfirmacionPage({ folio }: { folio:string }) {
  const { sesion, ultimaCompra } = useCinepolis()
  const recibo = ultimaCompra?.folio === folio ? ultimaCompra : undefined
  const [consultado,setDigital] = useState<TicketDigital>()
  const [error,setError] = useState('')
  const [intento,setIntento] = useState(0)
  useEffect(() => {
    let activo = true
    setError(''); setDigital(undefined)
    if (recibo) return
    api.ticket(folio).then(d => { if (activo) setDigital(d) }).catch(e => { if (activo) setError(e.message) })
    return () => { activo = false }
  },[folio,intento,sesion,recibo])
  const digital = recibo ?? consultado
  const t = digital?.ticket
  const funcion = t?.boletos[0]
  return <section className="wrap compra"><article className="paso ticket"><h1>{t ? 'Compra confirmada' : 'Ticket digital'}</h1>
    {error ? <p role="alert">{error} <button onClick={() => setIntento(n => n+1)}>Reintentar</button></p> : !t && <p>Cargando comprobante…</p>}
    {digital && t && <><p>Folio · <strong>{t.orden.folio}</strong></p><p>Estado · {t.orden.estado}</p>
      <QRCodeSVG value={digital.qrPayload ?? digital.validacionQr} size={224} level="M" marginSize={4} title="Código de validación del ticket"/>
      {funcion && <><h2>{funcion.titulo}</h2><p>{funcion.nombreComercial} · {funcion.nombreSala} · {funcion.horario}</p></>}
      <h3>Boletos</h3>{t.boletos.map(b => <div className="importe" key={b.asientoId}><span>Fila {b.fila}, asiento {b.numero || 'sin etiqueta'} · {b.tipoBoleto}</span>{moneda(b.precioUnitario)}</div>)}
      {t.productos.length > 0 && <h3>Dulcería</h3>}{t.productos.map((p,i) => <div className="importe" key={i}><span>{p.cantidad} × {p.nombreProducto} {p.opciones.join(' · ')}</span>{moneda(p.cantidad*p.precioUnitario)}</div>)}
      <p>Cargo por servicio · {moneda(t.orden.cargoServicio)}</p><h3>Total · {moneda(t.orden.importeTotal)}</h3><p>{t.orden.formaPago === 'Puntos' ? t.orden.importeTotal + ' puntos canjeados' : t.puntosAcreditados + ' puntos acreditados'}</p>
      <button className="cta" onClick={() => window.print()}><Printer size={18}/> Imprimir o guardar PDF</button></>}
    <p><a href="/mis-boletos">Mis boletos</a></p></article></section>
}
