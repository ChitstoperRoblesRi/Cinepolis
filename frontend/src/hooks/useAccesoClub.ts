import { useState, type FormEvent } from 'react'
import { api } from '../services/api'
import { useCinepolis } from '../context/CinepolisContext'

/** Estado y acciones de Club. */
export function useAccesoClub() {
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
  return { sesion, perfil, cerrarSesion, errorPerfil, correo, setCorreo, codigo, setCodigo, enviado, error, ocupado, entrar }
}
