-- Correcciones de datos del catálogo. Los IDs de opciones de ventas anteriores se conservan.
BEGIN;

UPDATE public.modificador_opcion o SET nombre_opcion = 'M&M''s®'
FROM public.modificador_regla r JOIN public.producto_dulceria p USING (producto_id)
WHERE r.regla_id = o.regla_id AND p.api_id = '16454'
  AND lower(trim(o.nombre_opcion)) = 'helado micha no usar';

-- Completar cada unidad desde el componente sin número sólo donde falta su disponibilidad.
WITH unidades AS (
    SELECT d.catalogo_id, destino.opcion_id, m.activo, m.precio_extra
    FROM public.producto_dulceria p JOIN public.detalle_catalogo d USING (producto_id)
    JOIN public.modificador_regla padre USING (producto_id)
    JOIN public.modificador_opcion o ON o.regla_id = padre.regla_id
    JOIN public.disponibilidad_modificador m ON m.opcion_id = o.opcion_id AND m.catalogo_id = d.catalogo_id
    JOIN public.modificador_regla unidad ON unidad.producto_id = p.producto_id
      AND unidad.grupo_orden = padre.grupo_orden AND unidad.tipo_modificador = padre.tipo_modificador
      AND lower(unidad.titulo_regla) = lower(padre.titulo_regla)
      AND unidad.grupo_titulo IN (padre.grupo_titulo || ' 1', padre.grupo_titulo || ' 2')
    JOIN public.modificador_opcion destino ON destino.regla_id = unidad.regla_id AND destino.api_id_opcion = o.api_id_opcion
    WHERE p.api_id IN ('17497','17509','14179','17500','16454','17504')
      AND lower(padre.grupo_titulo) SIMILAR TO '%(refresco|icee®)'
)
INSERT INTO public.disponibilidad_modificador (catalogo_id, opcion_id, activo, precio_extra)
SELECT catalogo_id, opcion_id, activo, precio_extra FROM unidades
ON CONFLICT (opcion_id, catalogo_id) DO NOTHING;

UPDATE public.disponibilidad_modificador m SET activo = false, actualizado_en = now()
FROM public.modificador_opcion o JOIN public.modificador_regla r USING (regla_id)
JOIN public.producto_dulceria p USING (producto_id)
WHERE m.opcion_id = o.opcion_id AND m.activo AND p.api_id IN ('17497','17509','14179','17500','16454','17504')
  AND lower(r.grupo_titulo) SIMILAR TO '%(refresco|icee®)'
  AND 2 = (SELECT count(DISTINCT u.grupo_titulo) FROM public.modificador_regla u
      JOIN public.modificador_opcion v USING (regla_id) JOIN public.disponibilidad_modificador n USING (opcion_id)
      WHERE u.producto_id = p.producto_id AND u.grupo_orden = r.grupo_orden
        AND u.grupo_titulo IN (r.grupo_titulo || ' 1', r.grupo_titulo || ' 2')
        AND u.tipo_modificador = r.tipo_modificador AND lower(u.titulo_regla) = lower(r.titulo_regla)
        AND v.api_id_opcion = o.api_id_opcion AND n.catalogo_id = m.catalogo_id);

-- Un ID de opción por regla permite elegir agua en ambas bebidas y persistir ambas elecciones.
INSERT INTO public.modificador_opcion (regla_id, api_id_opcion, nombre_opcion)
SELECT DISTINCT r.regla_id, '275', 'Agua Ciel® 1 lt'
FROM public.modificador_regla r JOIN public.producto_dulceria p USING (producto_id)
JOIN public.modificador_opcion soda USING (regla_id)
WHERE p.nombre_producto ILIKE '%combo%' AND r.tipo_modificador = 'flavor'
  AND (r.grupo_titulo ILIKE '%refresco%' OR r.grupo_titulo ILIKE '%bebida%')
  AND soda.nombre_opcion ILIKE '%coca%cola%'
ON CONFLICT (regla_id, api_id_opcion) DO UPDATE SET nombre_opcion = excluded.nombre_opcion
WHERE public.modificador_opcion.nombre_opcion <> excluded.nombre_opcion;

WITH bebidas AS (
    SELECT DISTINCT m.catalogo_id, agua.opcion_id
    FROM public.modificador_regla r JOIN public.producto_dulceria p USING (producto_id)
    JOIN public.modificador_opcion agua USING (regla_id)
    JOIN public.modificador_opcion soda USING (regla_id)
    JOIN public.disponibilidad_modificador m ON m.opcion_id = soda.opcion_id
    JOIN public.detalle_catalogo d ON d.producto_id = p.producto_id AND d.catalogo_id = m.catalogo_id
    WHERE p.nombre_producto ILIKE '%combo%' AND r.tipo_modificador = 'flavor'
      AND (r.grupo_titulo ILIKE '%refresco%' OR r.grupo_titulo ILIKE '%bebida%')
      AND agua.api_id_opcion = '275' AND soda.opcion_id <> agua.opcion_id AND m.activo
)
INSERT INTO public.disponibilidad_modificador (catalogo_id, opcion_id, activo, precio_extra)
SELECT b.catalogo_id, b.opcion_id, true, coalesce((
    SELECT min(d.precio_actual) FROM public.detalle_catalogo d JOIN public.producto_dulceria p USING (producto_id)
    WHERE d.catalogo_id = b.catalogo_id AND p.nombre_producto = 'Agua Embotellada'
), 0) FROM bebidas b
ON CONFLICT (opcion_id, catalogo_id) DO UPDATE SET activo = true, actualizado_en = now()
WHERE NOT public.disponibilidad_modificador.activo;

COMMIT;
