import { Popcorn, Plus } from 'lucide-react'
import type { ProductoDto } from '../../types/cinepolis'
import { moneda } from '../../services/moneda'
import styles from './ProductGrid.module.css'

interface Props {
  productos: ProductoDto[]
  elegir: (producto: ProductoDto) => void
}

export default function ProductGrid({ productos, elegir }: Props) {
  return <div className={styles.grid}>{productos.map(p =>
    <article key={p.productoId} className={styles.product}>
      <div className={styles.art}>{p.imagenUrl
        ? <img alt="" loading="lazy" src={p.imagenUrl}/>
        : <Popcorn aria-hidden="true" size={38}/>}</div>
      <div className={styles.copy}><h3>{p.nombreProducto}</h3><p>{p.descripcion}</p>
        <strong>{moneda(p.precio ?? 0)}</strong></div>
      <button className="cta" onClick={() => elegir(p)}><Plus size={16}/>
        {p.reglas.length ? 'Personalizar' : 'Añadir'}</button>
    </article>)}</div>
}
