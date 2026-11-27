import { usePasoDulceria } from '../../hooks/usePasoDulceria'
import { CatalogoDulceria } from '../DulceriaPage'

export function PasoDulceria() {
  const { c, omitir } = usePasoDulceria()
  return <section className="paso"><h1>Elige tu dulcería</h1><CatalogoDulceria key={c.funcion?.complejoId} cine={c.funcion?.complejoId}/>
    <div className="acciones-compra"><a href="/compra/asientos">Volver a asientos</a>
    <button onClick={omitir}>Continuar sin dulcería</button>
    {c.productos.length > 0 && <a className="cta" href="/compra/pago">Continuar al pago</a>}</div></section>
}
