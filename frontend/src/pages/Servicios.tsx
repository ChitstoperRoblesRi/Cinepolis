import { useAccesoClub } from '../hooks/useAccesoClub'

export function Club() {
  const { sesion, perfil, cerrarSesion, errorPerfil, correo, setCorreo, codigo, setCodigo, enviado, error, ocupado, entrar } = useAccesoClub()
  return <section className="wrap cartel"><h1>Club Cinépolis</h1>{sesion ? <><h2>{perfil?.nombre}</h2><p>{perfil ? perfil.nombreNivel + ' · ' + perfil.puntosAcumulados + ' puntos' : errorPerfil || 'Cargando perfil…'}</p><button className="cta" onClick={cerrarSesion}>Cerrar sesión</button></> :
    <form className="club" onSubmit={entrar}><label>Correo electrónico<input type="email" required value={correo} readOnly={enviado} onChange={e => setCorreo(e.target.value)}/></label>
    {enviado && <label>Código recibido por correo<input required inputMode="numeric" pattern="[0-9]{6}" maxLength={6} value={codigo} onChange={e => setCodigo(e.target.value)}/></label>}
    <button disabled={ocupado} className="cta">{enviado ? 'Iniciar sesión' : 'Recibir código'}</button>{error && <p role="alert">{error}</p>}</form>}</section>
}
