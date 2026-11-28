import { useEffect, useRef } from 'react'
import { X } from 'lucide-react'
import { useConfigurador } from '../hooks/useConfigurador'
import type { LineaDulceria, ProductoDto, Regla } from '../types/cinepolis'
import { moneda } from '../services/moneda'
import './opciones.css'

function ordenComponente(regla: Regla) {
  const nombre = regla.grupoTitulo ?? ''
  if (regla.bloquePalomitas != null || /palomitas/i.test(nombre)) return 0
  return /refresco|bebida/i.test(nombre) ? 1 : 2
}

export function Opciones({ p, combo, agregar, cerrar, aviso, inicial }: {
  inicial?: LineaDulceria; p: ProductoDto; combo: boolean; agregar: (ids: number[], unidades: number) => void; cerrar: () => void; aviso: string
}) {
  const modal = useRef<HTMLDialogElement>(null)
  const { ids, unidades, setUnidades, grupos, valido, total } = useConfigurador(p, undefined, undefined, inicial)
  const ordenados = combo ? [...grupos].sort((a,b) => ordenComponente(a.regla) - ordenComponente(b.regla)) : grupos
  useEffect(() => { const d = modal.current!; d.showModal(); return () => d.close() }, [])
  return <dialog className="configurador" ref={modal} aria-labelledby="combo-titulo" onCancel={cerrar}>
    <div className="configurador-cuerpo">
      <header className="configurador-titulo">
        <h2 id="combo-titulo">{p.nombreProducto}</h2>
        <button type="button" onClick={cerrar}>Cerrar <X size={16} aria-hidden="true"/></button>
      </header>
      <div className="configurador-banner">
        {p.imagenUrl && <img src={p.imagenUrl} alt=""/>}
      </div>
      {p.descripcion && <p className="configurador-descripcion">{p.descripcion}</p>}
      {ordenados.map(({regla, opcional, indicador, admite, alternar, etiquetas}) => <details open key={regla.reglaId}>
        <summary><strong>{regla.grupoTitulo || p.nombreProducto}</strong><span>{regla.titulo} {opcional && <small className="configurador-opcional">Opcional</small>}</span></summary>
        <p>{indicador}</p>
        <div className="opciones-combo">{regla.opciones.map((o,i) =>
          <button type="button" key={o.opcionId} aria-pressed={ids.includes(o.opcionId)}
            disabled={!admite(o.opcionId)} onClick={() => alternar(o)}>
            <strong>{o.nombre}</strong>{etiquetas[i] && <small>{etiquetas[i]}</small>}
          </button>)}</div>
      </details>)}
      <div className="unidades-combo"><strong>Cantidad</strong><div className="contador">
        <button aria-label="Quitar una unidad" disabled={unidades <= 1} onClick={() => setUnidades(unidades - 1)}>−</button>
        <output>{unidades}</output><button aria-label="Añadir una unidad" disabled={unidades >= 999} onClick={() => setUnidades(unidades + 1)}>+</button>
      </div></div>
    </div>
    <div className="configurador-pie">{aviso && <p role="alert">{aviso}</p>}
      <button disabled={!valido} onClick={() => agregar(ids, unidades)}>{inicial ? 'Guardar' : 'Agregar'} • {moneda(total)}</button>
    </div>
  </dialog>
}
