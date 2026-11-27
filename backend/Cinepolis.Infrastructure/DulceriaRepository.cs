using Cinepolis.Domain;
using System.Text.Json;
using Cinepolis.Application;
using Dapper;
using System.Data.Common;

namespace Cinepolis.Infrastructure;

/// <summary>Recupera el catálogo regional de productos y combos.</summary>
public sealed class DulceriaRepository(IDbConnectionFactory factory, NormalizadorCatalogo normalizador,
    CalculadoraPrecioDulceriaContext calculadora) : IDulceriaCatalogoRepository
{
    private sealed record Fila(int ProductoId, int CategoriaId, string NombreProducto, string Estructura,
        string? Descripcion, string? ImagenUrl, int? CatalogoId, decimal? Precio, string Reglas, string? SkuProducto, bool EsCombo);
    private sealed record OpcionCatalogo(int OpcionId, string Sku, string Nombre, PrecioOpcionCatalogo Importe,
        bool PrecioVariante);
    private sealed record ReglaCatalogo(int ReglaId, string Titulo, int Min, int Max, OpcionCatalogo[] Opciones,
        string? GrupoTitulo, string TipoModificador, int? BloquePalomitas);

    internal async Task<ProductoDto[]> LeerProductos(DbConnection db, int? complejoId, int[]? ids,
        DbTransaction? tx, CancellationToken token, int? catalogoId = null)
    {
        const string sql = """
            WITH bebidas AS (
                SELECT dc.catalogo_id, dc.precio_actual, b.api_id, b.producto_id
                FROM public.detalle_catalogo dc JOIN public.producto_dulceria b USING (producto_id)
                JOIN public.catalogo_complejo cc USING (catalogo_id)
                WHERE b.nombre_producto = ANY(@BebidasIncluidas) AND (@ComplejoId::int IS NULL OR cc.complejo_id = @ComplejoId)
            ), precios_bebida AS (
                SELECT catalogo_id, sku, min(precio_actual) AS base_incluida FROM (
                    SELECT catalogo_id, api_id AS sku, precio_actual FROM bebidas
                    UNION ALL
                    -- El catálogo regional 18795 usa 18791 para el litro incluido como SKU 275 en el combo.
                    SELECT catalogo_id, @SkuAguaLitro, precio_actual FROM bebidas WHERE api_id = @SkuAguaRegional
                    UNION ALL
                    SELECT b.catalogo_id, o.api_id_opcion, b.precio_actual FROM bebidas b
                    JOIN public.modificador_regla r USING (producto_id)
                    JOIN public.modificador_opcion o USING (regla_id)
                    WHERE r.tipo_modificador = ANY(@TiposVariante)
                ) variantes GROUP BY catalogo_id, sku
            )
            SELECT p.producto_id, p.categoria_id, p.nombre_producto, p.estructura, p.descripcion, p.imagen_url,
                cat.catalogo_id, cat.precio_actual AS precio, coalesce(reglas.lista, '[]')::text AS reglas, p.api_id AS sku_producto, tipo.es_combo
            FROM public.producto_dulceria p
            JOIN public.categoria_dulceria categoria USING (categoria_id)
            CROSS JOIN LATERAL (SELECT categoria.nombre_categoria = @CategoriaCombos
                OR p.nombre_producto ILIKE @NombreCombo OR p.nombre_producto LIKE @ProductosJuntos
                OR categoria.nombre_categoria ILIKE @Compartir AS es_combo,
                p.nombre_producto ILIKE @Maxicombo OR categoria.nombre_categoria ILIKE @Compartir AS es_maxicombo) tipo
            LEFT JOIN LATERAL (
                SELECT d.catalogo_id, d.precio_actual FROM public.detalle_catalogo d
                JOIN public.catalogo_complejo c USING (catalogo_id)
                WHERE c.complejo_id = @ComplejoId AND d.producto_id = p.producto_id AND d.disponibilidad
                  AND (@CatalogoId::int IS NULL OR c.catalogo_id = @CatalogoId)
                ORDER BY d.precio_actual, d.catalogo_id LIMIT 1
            ) cat ON true
            LEFT JOIN LATERAL (
                WITH catalogadas AS (
                    SELECT r.*, o.opcion_id, o.api_id_opcion, o.nombre_opcion, o.es_default, m.activo,
                        d.precio_actual, m.precio_extra, bebida.base_incluida
                    FROM public.modificador_regla r
                    JOIN public.modificador_opcion o USING (regla_id)
                    JOIN public.disponibilidad_modificador m ON m.opcion_id = o.opcion_id
                    JOIN public.detalle_catalogo d ON d.catalogo_id = m.catalogo_id AND d.producto_id = r.producto_id
                    LEFT JOIN precios_bebida bebida ON bebida.catalogo_id = m.catalogo_id AND bebida.sku = o.api_id_opcion
                        AND (categoria.nombre_categoria = @CategoriaCombos OR (tipo.es_combo AND o.api_id_opcion = @SkuAguaLitro
                            AND r.tipo_modificador = @TipoSabor AND lower(r.grupo_titulo) LIKE ANY(@GruposRefresco))) AND r.min_items > 0
                        AND r.tipo_modificador = ANY(@TiposVariante)
                    WHERE r.producto_id = p.producto_id AND m.catalogo_id = coalesce(cat.catalogo_id, (
                        SELECT min(d.catalogo_id) FROM public.detalle_catalogo d
                        WHERE d.producto_id = p.producto_id AND d.disponibilidad
                    ))
                ), componentes AS (
                    SELECT grupo_orden, lower(trim(grupo_titulo)) AS nombre, min(trim(grupo_titulo)) AS etiqueta
                    FROM catalogadas GROUP BY grupo_orden, lower(trim(grupo_titulo))
                ), nombres AS (
                    -- Algunas importaciones guardan el mismo componente con y sin sufijo de unidad.
                    SELECT c.grupo_orden, c.nombre, coalesce(unidad.nombre, c.nombre) AS nombre_unico,
                        coalesce(unidad.etiqueta, c.etiqueta) AS etiqueta
                    FROM componentes c LEFT JOIN LATERAL (
                        SELECT min(u.nombre) AS nombre, min(u.etiqueta) AS etiqueta FROM componentes u
                        WHERE u.grupo_orden = c.grupo_orden AND u.nombre <> c.nombre
                            AND regexp_replace(u.nombre, ' [0-9]+$', '') = c.nombre
                            AND (p.api_id <> ALL(@CombosCiel) OR c.nombre NOT LIKE @GrupoAgua OR u.nombre = c.nombre || ' 1')
                            AND (p.api_id <> ALL(@CombosDosBebidas) OR c.nombre NOT LIKE ALL(@GruposDosBebidas)
                                OR u.nombre = c.nombre || ' 1')
                        -- En los maxicombos, el encabezado es un alias de la primera unidad de esa posición.
                        HAVING count(*) = 1 OR (tipo.es_maxicombo AND
                            (c.nombre LIKE @GrupoPalomitas OR c.nombre LIKE ANY(@GruposRefresco)))
                    ) unidad ON true
                ), opciones AS (
                    SELECT c.*, CASE WHEN n.nombre_unico LIKE @GrupoPalomitas OR lower(p.nombre_producto) = @Palomitas
                        THEN min(c.regla_id) OVER (PARTITION BY c.grupo_orden, n.nombre_unico) END AS bloque_palomitas,
                        min(c.regla_id) OVER (PARTITION BY n.nombre_unico,
                        CASE WHEN n.nombre_unico LIKE @GrupoPalomitas THEN c.grupo_orden END,
                        lower(trim(c.titulo_regla)), c.tipo_modificador, c.min_items, c.max_items) AS grupo_id
                    FROM catalogadas c JOIN nombres n ON n.grupo_orden IS NOT DISTINCT FROM c.grupo_orden
                        AND n.nombre IS NOT DISTINCT FROM lower(trim(c.grupo_titulo))
                ), unicas AS (
                    SELECT DISTINCT ON (grupo_id, api_id_opcion) *
                    FROM opciones ORDER BY grupo_id, api_id_opcion, activo DESC, opcion_id
                ), referencias AS (
                    SELECT grupo_id, bool_or(min_items > 0) AS obligatoria,
                        max(precio_extra) FILTER (WHERE api_id_opcion = p.api_id) AS importe_sku_principal,
                        max(precio_extra) FILTER (WHERE lower(trim(nombre_opcion)) = @ParaLlevar) AS importe_llevar,
                        min(precio_extra) FILTER (WHERE es_default) AS importe_incluido,
                        min(precio_extra) FILTER (WHERE activo OR api_id_opcion = p.api_id) AS importe_base
                    FROM unicas GROUP BY grupo_id
                ), clasificadas AS (
                    SELECT u.*, u.tipo_modificador = ANY(@TiposVariante) AND u.min_items = 1 AND u.max_items = 1
                        AS eleccion_unica, ref.importe_base, ref.importe_incluido, ref.importe_llevar, ref.importe_sku_principal,
                        (SELECT count(*) FROM referencias WHERE obligatoria) = 1 AS unica_obligatoria
                    FROM unicas u JOIN referencias ref USING (grupo_id)
                ), grupos AS (
                    SELECT grupo_id, min(grupo_orden) AS orden, min(bloque_palomitas) AS bloque_palomitas,
                        coalesce(jsonb_agg(jsonb_build_object(
                        'OpcionId', opcion_id, 'Sku', api_id_opcion, 'Nombre', CASE
                            WHEN tipo.es_combo AND tipo_modificador = @TipoTamano AND lower(grupo_titulo) LIKE ANY(@GruposRefresco)
                                AND lower(trim(nombre_opcion)) = @Refresco THEN @Jumbo
                            WHEN tipo_modificador = @TipoTamano AND lower(grupo_titulo) LIKE @GrupoNachos
                                AND api_id_opcion = @SkuNachosGrandes THEN @Grandes
                            WHEN tipo.es_combo AND api_id_opcion = @SkuAguaLitro AND tipo_modificador = @TipoSabor
                                AND lower(grupo_titulo) LIKE ANY(@GruposRefresco) THEN @AguaCiel
                            WHEN p.nombre_producto = @Nachos AND tipo_modificador = @TipoTamano
                                AND api_id_opcion = p.api_id THEN @Grandes
                            WHEN bloque_palomitas IS NOT NULL AND p.estructura = @Combo AND lower(p.nombre_producto) <> @Palomitas
                                AND tipo_modificador = @TipoTamano
                                AND lower(trim(nombre_opcion)) = @Palomitas THEN @Jumbo
                            ELSE nombre_opcion END,
                        'Importe', jsonb_build_object(
                            'Extra', precio_extra, 'PrecioCatalogo', precio_actual, 'BaseBebida', base_incluida,
                            'SkuPrincipal', importe_sku_principal, 'ParaLlevar', importe_llevar,
                            'Incluido', importe_incluido, 'Minimo', importe_base, 'EleccionUnica', eleccion_unica,
                            'AguaEnCombo', tipo.es_combo AND api_id_opcion = @SkuAguaLitro AND tipo_modificador = @TipoSabor
                                AND coalesce(lower(grupo_titulo) LIKE ANY(@GruposRefresco), false),
                            'TamanoNachos', p.nombre_producto = @Nachos AND tipo_modificador = @TipoTamano,
                            'TamanoPalomitasCombo', p.estructura = @Combo AND lower(p.nombre_producto) <> @Palomitas
                                AND bloque_palomitas IS NOT NULL AND tipo_modificador = @TipoTamano,
                            'NombreOrigen', nombre_opcion, 'OpcionIncluidaCombo', categoria.nombre_categoria = @CategoriaCombos
                                AND eleccion_unica AND bloque_palomitas IS NULL,
                            'VarianteHelado', eleccion_unica AND unica_obligatoria AND categoria.nombre_categoria = @CategoriaHelados),
                        'PrecioVariante', eleccion_unica AND unica_obligatoria
                    ) ORDER BY opcion_id) FILTER (WHERE activo), '[]') AS opciones
                    FROM clasificadas GROUP BY grupo_id
                )
                SELECT jsonb_agg(jsonb_build_object('ReglaId', r.regla_id, 'Titulo', r.titulo_regla,
                    'GrupoTitulo', n.etiqueta, 'TipoModificador', r.tipo_modificador, 'BloquePalomitas', g.bloque_palomitas,
                    'Min', r.min_items, 'Max', r.max_items, 'Opciones', g.opciones
                ) ORDER BY g.orden,
                    CASE WHEN tipo.es_combo THEN n.nombre_unico END,
                    CASE WHEN tipo.es_combo THEN CASE r.tipo_modificador
                            WHEN @TipoTamano THEN 0 WHEN @TipoSabor THEN 1 ELSE 2 END END,
                    coalesce(g.bloque_palomitas, r.regla_id),
                    CASE WHEN g.bloque_palomitas IS NOT NULL AND r.tipo_modificador = @TipoTamano THEN 0 ELSE 1 END,
                    r.regla_id) AS lista
                FROM grupos g JOIN public.modificador_regla r ON r.regla_id = g.grupo_id
                JOIN nombres n ON n.grupo_orden IS NOT DISTINCT FROM r.grupo_orden
                    AND n.nombre IS NOT DISTINCT FROM lower(trim(r.grupo_titulo))
            ) reglas ON true
            WHERE (@ComplejoId::int IS NULL OR cat.catalogo_id IS NOT NULL)
              AND (@Ids::int[] IS NULL OR p.producto_id = ANY(@Ids))
            ORDER BY p.categoria_id, p.nombre_producto, p.producto_id
            """;
        var parametros = new { complejoId, ids, catalogoId, TiposVariante = new[] { "flavor", "size" },
            Nachos = "Nachos", Grandes = "Grandes", Maxicombo = "%maxicombo%", Compartir = "Para Compartir",
            TipoSabor = "flavor", Refresco = "refresco", AguaCiel = "Agua Ciel® 1 lt",
            GrupoNachos = "%nachos%", SkuNachosGrandes = "14175", NombreCombo = "%combo%", ProductosJuntos = "% + %",
            GruposDosBebidas = new[] { "%refresco", "%icee®" }, GruposRefresco = new[] { "%refresco%", "%bebida%" },
            CombosDosBebidas = new[] { "17497", "17509", "14179", "17500", "16454", "17504" },
            CategoriaHelados = "Helados", TipoTamano = "size", SkuAguaLitro = "275", SkuAguaRegional = "18795",
            GrupoPalomitas = "%palomitas%",
            Palomitas = "palomitas", Combo = "combo", Jumbo = "Jumbo", CategoriaCombos = "Combos",
            ParaLlevar = "para llevar",
            BebidasIncluidas = new[] { "Agua Embotellada", "Refresco", "ICEE®" },
            CombosCiel = new[] { "14511", "26165" }, GrupoAgua = "%agua%" };
        var filas = await db.QueryAsync<Fila>(new CommandDefinition(sql, parametros, tx, cancellationToken: token));
        return filas.Select(NormalizarFila).ToArray();
    }

    private ProductoDto NormalizarFila(Fila fila)
    {
        var origen = JsonSerializer.Deserialize<ReglaCatalogo[]>(fila.Reglas)!;
        var reglas = origen.Select(r => new ReglaDto(r.ReglaId, r.Titulo, r.Min, r.Max,
            r.Opciones.Select(o =>
            {
                var extra = calculadora.Diferencial(o.Importe);
                return new OpcionDto(o.OpcionId, o.Nombre, extra, o.PrecioVariante ? o.Importe.PrecioCatalogo + extra : null);
            }).ToArray(), r.GrupoTitulo, r.TipoModificador, r.BloquePalomitas)).ToArray();
        var producto = new ProductoDto(fila.ProductoId, fila.CategoriaId, fila.NombreProducto, fila.Estructura,
            fila.Descripcion, fila.ImagenUrl, fila.CatalogoId, fila.Precio, reglas);
        var claves = origen.SelectMany(r => r.Opciones).ToDictionary(o => o.OpcionId, o => o.Sku);
        return normalizador.Normalizar(producto, new(fila.SkuProducto, fila.EsCombo, claves));
    }

    /// <summary>Recupera categorías y productos para el complejo indicado.</summary>
    public async Task<(IReadOnlyList<CategoriaDto> Categorias, IReadOnlyList<ProductoDto> Productos)> CatalogoAsync(int? complejoId, CancellationToken token)
    {
        await using var db = await factory.OpenAsync(token);
        var categorias = await db.QueryAsync<CategoriaDto>(new CommandDefinition(
            "SELECT categoria_id, nombre_categoria FROM public.categoria_dulceria ORDER BY nombre_categoria", cancellationToken: token));
        return (categorias.AsList(), await LeerProductos(db, complejoId, null, null, token));
    }
}
