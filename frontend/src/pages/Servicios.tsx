import { useState, type FormEvent } from 'react'
import { api } from '../services/api'
import { useCinepolis } from '../context/CinepolisContext'

export function Club() {
  const { sesion, perfil, iniciarSesion, cerrarSesion, errorPerfil } = useCinepolis()
  const [correo, setCorreo] = useState('')
  const [codigo, setCodigo] = useState('')
  const [enviado, setEnviado] = useState(false)
  const [error, setError] = useState('')
  const [ocupado, setOcupado] = useState(false)
  async function entrar(e: FormEvent) {
    e.preventDefault(); setError(''); setOcupado(true)
    try {
      if (enviado) iniciarSesion(await api.login({ correo, codigo }))
      else { await api.codigo({correo}); setEnviado(true) }
    } catch (e) { setError(e instanceof Error ? e.message : 'No se pudo iniciar sesión.') }
    finally { setOcupado(false) }
  }
  return <section className="wrap cartel"><h1>Club Cinépolis</h1>{sesion ? <><h2>{perfil?.nombre}</h2><p>{perfil ? perfil.nombreNivel + ' · ' + perfil.puntosAcumulados + ' puntos' : errorPerfil || 'Cargando perfil…'}</p><button className="cta" onClick={cerrarSesion}>Cerrar sesión</button></> :
    <form className="club" onSubmit={entrar}><label>Correo electrónico<input type="email" required value={correo} readOnly={enviado} onChange={e => setCorreo(e.target.value)}/></label>
    {enviado && <label>Código recibido por correo<input required inputMode="numeric" pattern="[0-9]{6}" maxLength={6} value={codigo} onChange={e => setCodigo(e.target.value)}/></label>}
    <button disabled={ocupado} className="cta">{enviado ? 'Iniciar sesión' : 'Recibir código'}</button>{error && <p role="alert">{error}</p>}</form>}</section>
}
