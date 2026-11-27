import { useAcceso } from '../hooks/useAcceso'
import './club.css'

export function AuthPage() {
  const { registro, setRegistro, correo, setCorreo, enviado, setEnviado, codigo, setCodigo, error, setError, mensaje, setMensaje, ocupado, pedirCodigo, enviar } = useAcceso()
  return <section className="zona-club wrap"><div className="acceso-club"><h1>Club Cinépolis</h1>
    <div className="pills">{['Iniciar sesión','Crear cuenta'].map((label,i) => <button key={label} disabled={ocupado}
      aria-pressed={registro === Boolean(i)} onClick={() => { setRegistro(Boolean(i)); setError('') }}>{label}</button>)}</div>
    <form onSubmit={enviar}><fieldset disabled={ocupado}><label>Correo electrónico<input required type="email" maxLength={150}
      autoComplete="email" value={correo} readOnly={enviado} onChange={e => setCorreo(e.target.value)}/></label>
    {enviado && <><button className="back" type="button" onClick={() => { setEnviado(false); setCodigo(''); setError(''); setMensaje('') }}>Cambiar correo</button>
      {registro && <><label>Usuario Club<input name="usuario" required maxLength={100} autoComplete="username"/></label>
      <label>Nombre<input name="nombre" required maxLength={100} autoComplete="given-name"/></label>
      <label>Apellidos<input name="apellidos" required maxLength={150} autoComplete="family-name"/></label>
      <label>Fecha de nacimiento<input name="nacimiento" type="date" required max={new Date().toLocaleDateString('sv-SE')} autoComplete="bday"/></label></>}
      <label>Código de seis dígitos<input autoComplete="one-time-code" required inputMode="numeric" pattern="[0-9]{6}"
        maxLength={6} value={codigo} onChange={e => setCodigo(e.target.value)}/></label>
      <button className="back" type="button" onClick={() => void pedirCodigo()}>Solicitar otro código</button></>}
    <p role="status">{mensaje}</p>{error && <p role="alert">{error}</p>}
    <button className="cta">{ocupado ? 'Procesando…' : !enviado ? 'Enviar código' : registro ? 'Crear cuenta' : 'Iniciar sesión'}</button>
    </fieldset></form></div></section>
}
