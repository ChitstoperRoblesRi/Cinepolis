import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../services/api'
import { useCinepolis } from '../context/CinepolisContext'
import { navegar } from '../services/navigation'
import './club.css'

export function AuthPage() {
  const { sesion, iniciarSesion, carrito } = useCinepolis()
  const [registro, setRegistro] = useState(false)
  const [correo, setCorreo] = useState('')
  const [enviado, setEnviado] = useState(false)
  const [codigo, setCodigo] = useState('')
  const [error, setError] = useState('')
  const [mensaje, setMensaje] = useState('')
  const [ocupado, setOcupado] = useState(false)
  const destino = new URLSearchParams(location.search).get('volver') === 'pago' && carrito.funcion ? '/compra/pago' : '/club'
  useEffect(() => {
    if (sesion && location.pathname === '/auth') navegar(destino)
  }, [sesion,destino])

  async function pedirCodigo() {
    setError(''); setMensaje(''); setOcupado(true)
    try {
      await api.codigo({ correo: correo.trim() })
      setCodigo(''); setEnviado(true); setMensaje('Revisa tu correo. El código vence en cinco minutos.')
    } catch (e) { setError(e instanceof Error ? e.message : 'No se pudo enviar el código.') }
    finally { setOcupado(false) }
  }
  async function enviar(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    if (ocupado) return
    if (!enviado) { await pedirCodigo(); return }
    const data = new FormData(e.currentTarget)
    const campo = (nombre: string) => String(data.get(nombre) ?? '').trim()
    setOcupado(true); setError('')
    try {
      const acceso = { correo: correo.trim(), codigo }
      const resultado = registro ? await api.registro({ ...acceso, usuarioCc: campo('usuario'), nombre: campo('nombre'),
        apellidos: campo('apellidos'), fechaNacimiento: campo('nacimiento') }) : await api.login(acceso)
      iniciarSesion(resultado)
    } catch (e) { setError(e instanceof Error ? e.message : 'No se pudo completar el acceso.') }
    finally { setOcupado(false) }
  }
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
