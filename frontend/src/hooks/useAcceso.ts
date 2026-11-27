import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../services/api'
import { useCinepolis } from '../context/CinepolisContext'
import { navegar } from '../services/navigation'

/** Estado y acciones de AuthPage. */
export function useAcceso() {
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
  return { registro, setRegistro, correo, setCorreo, enviado, setEnviado, codigo, setCodigo, error, setError, mensaje, setMensaje, ocupado, pedirCodigo, enviar }
}
