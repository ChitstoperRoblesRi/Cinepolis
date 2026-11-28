import styles from '../../components/visual/PaymentForm.module.css'
import { usePago } from '../../hooks/usePago'
import { moneda } from '../../services/moneda'

export function PasoPago({ boletos }: { boletos: number }) {
  const { c, perfil, total, puntos, setPuntos, error, ocupado, puedeCanjear, canje, confirmar } = usePago(boletos)
  return <section className="paso pago"><h1>Pago</h1><form className={styles.payment} id="pago-compra" onSubmit={confirmar}><fieldset className={styles.group} disabled={ocupado}>
    <legend>Datos del comprador</legend>
    <label>Nombre completo<input required maxLength={100} name="nombre" autoComplete="name" defaultValue={perfil?.nombre}/></label>
    <label>Correo electrónico<input required maxLength={150} type="email" name="correo" autoComplete="email" defaultValue={perfil?.correo}/></label>
    <label>Tarjeta Club Cinépolis (opcional)<input name="club" maxLength={20} defaultValue={perfil?.numeroTarjeta}/></label>
    {perfil ? <p>{perfil.puntosAcumulados} puntos disponibles. Tu compra con tarjeta acumula puntos.</p> :
      <p><a href="/auth?volver=pago">Inicia sesión en Club</a> para acumular o canjear puntos.</p>}
    {perfil && <label className="check"><input type="checkbox" disabled={!puedeCanjear} checked={canje} onChange={e => setPuntos(e.target.checked)}/> Pagar el total con {total} puntos</label>}
    </fieldset>{!canje && <fieldset className={styles.group} disabled={ocupado}><legend>Tarjeta de prueba</legend>
      <p>Simulación de pago. Usa 4242 4242 4242 4242, una fecha futura y tres dígitos de prueba.</p>
      <label>Número de tarjeta<input name="numero" inputMode="numeric" placeholder="4242 4242 4242 4242" autoComplete="off" required pattern="4242 ?4242 ?4242 ?4242" title="Usa el número de prueba indicado"/></label>
      <div className="par-pago"><label>Vencimiento<input name="fecha" placeholder="MM/AA" autoComplete="off" maxLength={5} pattern="(0[1-9]|1[0-2])/[0-9]{2}" required/></label>
      <label>Código de seguridad<input name="cvc" type="password" autoComplete="off" inputMode="numeric" maxLength={3} pattern="[0-9]{3}" required/></label></div>
    </fieldset>}
    <dl>{[['Boletos',boletos],['Dulcería',c.dulceria],['Cargo por servicio',c.cargo],['Total',total]].map(([label,n]) => <div className="importe" key={label}><dt>{label}</dt><dd>{moneda(Number(n))}</dd></div>)}</dl>
    {error && <p role="alert">{error}</p>}
    </form></section>
}
