export const moneda = (valor: number) => valor.toLocaleString('es-MX', { style: 'currency', currency: 'MXN' })

/** Importe de una línea con varias unidades del mismo artículo. */
export const importeLinea = (cantidad: number, precio: number) => cantidad * precio
