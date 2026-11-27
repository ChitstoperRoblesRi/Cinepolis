const clave = 'cinepolis.foliosInvitado'
export function invitados(): string[] {
  try {
    const datos: unknown = JSON.parse(localStorage.getItem(clave) ?? '[]')
    return Array.isArray(datos) ? datos.filter((f): f is string => typeof f === 'string' && /^WEB-[A-F0-9]{40}$/.test(f)) : []
  } catch { return [] }
}
export function recordarInvitado(folio: string) {
  try { localStorage.setItem(clave,JSON.stringify([folio,...invitados().filter(f => f !== folio)])) } catch { /* El folio sigue disponible en la confirmación. */ }
}
