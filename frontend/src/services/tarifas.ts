import type { TarifaFuncion } from '../types/cinepolis'

export class ErrorSeleccionButaca extends Error {}

export const limiteBoletos = 10

export function tarifaGeneral(tarifas: TarifaFuncion[], area: string) {
  const zona = tarifas.filter(t => t.areaCategoryCode === area)
  const prioridades = [/^admisión general$/i, /^adulto$/i, /^(admisión general|general)\b/i, /^evento especial$/i]
  for (const patron of prioridades) {
    const tarifa = zona.find(t => patron.test(t.nombre.trim()))
    if (tarifa) return tarifa
  }
}
