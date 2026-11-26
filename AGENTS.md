# Instrucciones del proyecto Cinépolis

## Stack obligatorio

- Backend en C# con .NET 8 y ASP.NET Core Web API. Usa Arquitectura Limpia con los proyectos `Domain`, `Application`, `Infrastructure` y `API`.
- `Domain` contiene entidades, objetos de valor y reglas de negocio. `Application` contiene casos de uso, contratos y puertos. `Infrastructure` implementa persistencia y adaptadores con Dapper y Npgsql. `API` expone HTTP y SignalR y configura la inyección de dependencias.
- `Domain` es independiente. `Application` depende de `Domain`. `Infrastructure` depende de `Application` y, cuando corresponda, de `Domain`. `API` usa `Application` y referencia `Infrastructure` para componer los servicios.
- Usa Dapper y Npgsql para acceder a PostgreSQL. Configura `Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true` al arrancar, antes de ejecutar consultas. Entity Framework y Entity Framework Core están prohibidos.
- Usa SignalR nativo de ASP.NET Core para publicar cambios del mapa de asientos de las salas en tiempo real. Publica cambios persistidos después de confirmar la transacción.
- Configura servicios y rutas de Kong Gateway en modo declarativo mediante `kong.yml`, incluida la ruta del hub de SignalR cuando se exponga a través del gateway.

## Convenciones obligatorias

- Usa código fuertemente tipado con referencias anulables habilitadas. Define DTO, modelos de persistencia y resultados explícitos. Respeta tipos, precisión y nulabilidad de PostgreSQL.
- Devuelve errores HTTP mediante `ProblemDetails` de ASP.NET Core y usa `ValidationProblemDetails` para errores de validación. Centraliza el manejo de excepciones y usa el código HTTP correspondiente.
- Parametriza el 100 % de los valores de las consultas SQL con Dapper y Npgsql. Usa los identificadores reales de este diccionario y califica las tablas con `public`. Los identificadores dinámicos deben provenir de una lista permitida de nombres reales.
- La base de datos PostgreSQL existente es la fuente de verdad. No crees ni modifiques tablas, ni ejecutes migraciones o DDL, hasta que el usuario lo autorice.

## Diccionario de PostgreSQL

El esquema `public` de `cinepolis_vision` contiene 50 tablas, 335 columnas y 74 llaves foráneas. Los metadatos se verificaron mediante MCP desde `pg_catalog`.

Los tipos conservan la longitud y precisión reportadas por PostgreSQL. En la columna de valor por defecto, «—» indica que el catálogo no registra una expresión. Las restricciones muestran su nombre y definición en PostgreSQL. Las llaves foráneas de este esquema apuntan a tablas de `public`.

### `public.asiento`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `asiento_id` | `integer` | No | `nextval('asiento_asiento_id_seq'::regclass)` |
| `asiento_api_id` | `character varying(100)` | Sí | — |
| `fila` | `character varying(5)` | No | — |
| `columna` | `integer` | No | — |
| `tipo_asiento` | `character varying(20)` | No | — |
| `estado` | `character varying(15)` | No | `'disponible'::character varying` |
| `funcion_id` | `integer` | No | — |
| `area_category_code` | `character varying(20)` | No | `'0000000001'::character varying` |

- `asiento_pkey` — `PRIMARY KEY (asiento_id)`
- `uq_asiento_funcion` — `UNIQUE (funcion_id, fila, columna)`
- `asiento_funcion_id_fkey` — `FOREIGN KEY (funcion_id) REFERENCES funcion(funcion_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `asiento_columna_check` — `CHECK (columna > 0)`
- `asiento_estado_check` — `CHECK (estado::text = ANY (ARRAY['disponible'::character varying::text, 'reservado'::character varying::text, 'vendido'::character varying::text, 'bloqueado'::character varying::text]))`
- `asiento_tipo_asiento_check` — `CHECK (tipo_asiento::text = ANY (ARRAY['Tradicional'::character varying::text, 'Especial'::character varying::text, 'MACRO XE'::character varying::text, 'VIP'::character varying::text, 'IMAX'::character varying::text, 'Silla de ruedas'::character varying::text]))`

### `public.asiento_plantilla`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `plantilla_id` | `integer` | No | `nextval('asiento_plantilla_plantilla_id_seq'::regclass)` |
| `asiento_api_id` | `character varying(100)` | Sí | — |
| `sala_id` | `integer` | No | — |
| `fila` | `character varying(5)` | No | — |
| `columna` | `integer` | No | — |
| `tipo_asiento` | `character varying(20)` | No | — |
| `area_category_code` | `character varying(20)` | Sí | — |

- `asiento_plantilla_pkey` — `PRIMARY KEY (plantilla_id)`
- `uq_asiento_plantilla` — `UNIQUE (sala_id, fila, columna)`
- `asiento_plantilla_sala_id_fkey` — `FOREIGN KEY (sala_id) REFERENCES sala(sala_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `asiento_plantilla_columna_check` — `CHECK (columna > 0)`
- `asiento_plantilla_tipo_asiento_check` — `CHECK (tipo_asiento::text = ANY (ARRAY['Tradicional'::character varying::text, 'Especial'::character varying::text, 'MACRO XE'::character varying::text, 'VIP'::character varying::text, 'IMAX'::character varying::text, 'Silla de ruedas'::character varying::text]))`

### `public.auditoria_cambio`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `auditoria_id` | `bigint` | No | Identidad por defecto |
| `usuario_admin_id` | `integer` | Sí | — |
| `tabla_afectada` | `character varying(100)` | No | — |
| `registro_id` | `character varying(100)` | No | — |
| `accion` | `character varying(20)` | No | — |
| `datos_anteriores` | `jsonb` | Sí | — |
| `datos_nuevos` | `jsonb` | Sí | — |
| `ip_origen` | `inet` | Sí | — |
| `fecha_evento` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |

- `auditoria_cambio_pkey` — `PRIMARY KEY (auditoria_id)`
- `auditoria_cambio_usuario_admin_id_fkey` — `FOREIGN KEY (usuario_admin_id) REFERENCES usuario_admin(usuario_admin_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `ck_auditoria_accion` — `CHECK (((accion)::text = ANY ((ARRAY['INSERT'::character varying, 'UPDATE'::character varying, 'DELETE'::character varying, 'LOGIN'::character varying, 'CANCELACION'::character varying, 'REEMBOLSO'::character varying])::text[])))`

### `public.banner_promo`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `banner_id` | `integer` | No | `nextval('banner_promo_banner_id_seq'::regclass)` |
| `titulo` | `character varying(150)` | No | — |
| `imagen_url` | `character varying(500)` | No | — |
| `url_destino` | `character varying(500)` | No | — |
| `fecha_inicio` | `date` | No | — |
| `fecha_fin` | `date` | Sí | — |
| `activo` | `boolean` | No | `true` |
| `aplica_todos` | `boolean` | No | `true` |

- `banner_promo_pkey` — `PRIMARY KEY (banner_id)`

### `public.banner_promo_complejo`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `banner_id` | `integer` | No | — |
| `complejo_id` | `integer` | No | — |

- `banner_promo_complejo_pkey` — `PRIMARY KEY (banner_id, complejo_id)`
- `banner_promo_complejo_banner_id_fkey` — `FOREIGN KEY (banner_id) REFERENCES banner_promo(banner_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `banner_promo_complejo_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE CASCADE`

### `public.carrito`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `carrito_id` | `integer` | No | `nextval('carrito_carrito_id_seq'::regclass)` |
| `uuid_sesion_web` | `character varying(100)` | Sí | — |
| `fecha_creacion` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `fecha_expiracion` | `timestamp without time zone` | No | — |
| `estado` | `character varying(15)` | No | `'activo'::character varying` |
| `sesion_id` | `integer` | Sí | — |
| `socio_id` | `integer` | Sí | — |

- `carrito_pkey` — `PRIMARY KEY (carrito_id)`
- `carrito_sesion_id_fkey` — `FOREIGN KEY (sesion_id) REFERENCES sesion(sesion_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `carrito_socio_id_fkey` — `FOREIGN KEY (socio_id) REFERENCES cuenta_club(socio_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `carrito_estado_check` — `CHECK (estado::text = ANY (ARRAY['activo'::character varying::text, 'abandonado'::character varying::text, 'convertido'::character varying::text]))`

### `public.catalogo_complejo`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `catalogo_id` | `integer` | No | `nextval('catalogo_complejo_catalogo_id_seq'::regclass)` |
| `catalogo_nombre` | `character varying(150)` | No | — |
| `complejo_id` | `integer` | No | — |

- `catalogo_complejo_pkey` — `PRIMARY KEY (catalogo_id)`
- `uq_catalogo_complejo_nombre` — `UNIQUE (complejo_id, catalogo_nombre)`
- `catalogo_complejo_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`

### `public.categoria_dulceria`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `categoria_id` | `integer` | No | `nextval('categoria_dulceria_categoria_id_seq'::regclass)` |
| `nombre_categoria` | `character varying(100)` | No | — |

- `categoria_dulceria_pkey` — `PRIMARY KEY (categoria_id)`
- `uq_cat_nombre` — `UNIQUE (nombre_categoria)`

### `public.ciudad`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `ciudad_id` | `integer` | No | `nextval('ciudad_ciudad_id_seq'::regclass)` |
| `nombre_ciudad` | `character varying(100)` | No | — |

- `ciudad_pkey` — `PRIMARY KEY (ciudad_id)`
- `uq_ciudad_nombre` — `UNIQUE (nombre_ciudad)`

### `public.complejo`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `complejo_id` | `integer` | No | `nextval('complejo_complejo_id_seq'::regclass)` |
| `vista_id` | `character varying(50)` | Sí | — |
| `nombre_comercial` | `character varying(150)` | No | — |
| `tipo_complejo` | `character varying(15)` | No | — |
| `direccion` | `character varying(255)` | No | — |
| `codigo_postal` | `character varying(10)` | No | — |
| `fecha_apertura` | `date` | No | — |
| `slug` | `character varying(100)` | No | — |
| `imagen_url` | `character varying(500)` | Sí | — |
| `ciudad_id` | `integer` | No | — |

- `complejo_pkey` — `PRIMARY KEY (complejo_id)`
- `complejo_slug_key` — `UNIQUE (slug)`
- `complejo_vista_id_key` — `UNIQUE (vista_id)`
- `complejo_ciudad_id_fkey` — `FOREIGN KEY (ciudad_id) REFERENCES ciudad(ciudad_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `complejo_tipo_complejo_check` — `CHECK (tipo_complejo::text = ANY (ARRAY['Tradicional'::character varying::text, 'VIP'::character varying::text]))`

### `public.corte_caja`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `corte_id` | `integer` | No | `nextval('corte_caja_corte_id_seq'::regclass)` |
| `monto_efectivo_contado` | `numeric(10,2)` | No | — |
| `monto_reportado_sistema` | `numeric(10,2)` | No | — |
| `diferencia` | `numeric(10,2)` | No | Generada `(monto_efectivo_contado - monto_reportado_sistema)` |
| `fecha_hora_cierre` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `sesion_id` | `integer` | No | — |

- `corte_caja_pkey` — `PRIMARY KEY (corte_id)`
- `corte_caja_sesion_id_key` — `UNIQUE (sesion_id)`
- `corte_caja_sesion_id_fkey` — `FOREIGN KEY (sesion_id) REFERENCES sesion(sesion_id) ON UPDATE CASCADE ON DELETE RESTRICT`

### `public.cuenta_club`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `socio_id` | `integer` | No | Identidad siempre |
| `usuario_cc` | `character varying(100)` | No | — |
| `numero_tarjeta` | `character varying(20)` | No | — |
| `nombre` | `character varying(100)` | No | — |
| `apellidos` | `character varying(150)` | No | — |
| `correo` | `character varying(150)` | No | — |
| `telefono` | `character varying(20)` | Sí | — |
| `fecha_nacimiento` | `date` | No | — |
| `genero` | `character varying(30)` | Sí | — |
| `codigo_postal` | `character varying(10)` | Sí | — |
| `fecha_registro` | `date` | No | `CURRENT_DATE` |
| `puntos_acumulados` | `integer` | No | `0` |
| `puntos_vencen_fecha` | `date` | No | — |
| `visitas_semestre_actual` | `integer` | No | `0` |
| `semestre_actual` | `character varying(20)` | No | — |
| `nivel_id` | `integer` | No | — |

- `cuenta_club_pkey` — `PRIMARY KEY (socio_id)`
- `cuenta_club_correo_key` — `UNIQUE (correo)`
- `cuenta_club_numero_tarjeta_key` — `UNIQUE (numero_tarjeta)`
- `cuenta_club_usuario_cc_key` — `UNIQUE (usuario_cc)`
- `cuenta_club_nivel_id_fkey` — `FOREIGN KEY (nivel_id) REFERENCES nivel_club(nivel_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `cuenta_club_genero_check` — `CHECK (genero::text = ANY (ARRAY['Hombre'::character varying::text, 'Mujer'::character varying::text, 'Prefiero no responder'::character varying::text]))`
- `cuenta_club_puntos_acumulados_check` — `CHECK (puntos_acumulados >= 0)`
- `cuenta_club_visitas_semestre_actual_check` — `CHECK (visitas_semestre_actual >= 0)`

### `public.detalle_carrito`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `detalle_carrito_id` | `integer` | No | `nextval('detalle_carrito_detalle_carrito_id_seq'::regclass)` |
| `carrito_id` | `integer` | No | — |
| `tipo_item` | `character varying(10)` | No | — |
| `cantidad` | `integer` | No | — |
| `precio_unitario` | `numeric(8,2)` | No | — |
| `asiento_id` | `integer` | Sí | — |
| `producto_id` | `integer` | Sí | — |
| `promocion_id` | `integer` | Sí | — |
| `tipo_boleto_id` | `integer` | Sí | — |

- `detalle_carrito_pkey` — `PRIMARY KEY (detalle_carrito_id)`
- `detalle_carrito_asiento_id_fkey` — `FOREIGN KEY (asiento_id) REFERENCES asiento(asiento_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_carrito_carrito_id_fkey` — `FOREIGN KEY (carrito_id) REFERENCES carrito(carrito_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `detalle_carrito_producto_id_fkey` — `FOREIGN KEY (producto_id) REFERENCES producto_dulceria(producto_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_carrito_promocion_id_fkey` — `FOREIGN KEY (promocion_id) REFERENCES promocion(promocion_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `detalle_carrito_tipo_boleto_id_fkey` — `FOREIGN KEY (tipo_boleto_id) REFERENCES tipo_boleto(tipo_boleto_id)`
- `ck_detalle_carrito_arco_exclusivo` — `CHECK (tipo_item::text = 'boleto'::text AND asiento_id IS NOT NULL AND producto_id IS NULL OR tipo_item::text = 'producto'::text AND producto_id IS NOT NULL AND asiento_id IS NULL)`
- `detalle_carrito_cantidad_check` — `CHECK (cantidad > 0)`
- `detalle_carrito_precio_unitario_check` — `CHECK (precio_unitario >= 0::numeric)`
- `detalle_carrito_tipo_item_check` — `CHECK (tipo_item::text = ANY (ARRAY['boleto'::character varying::text, 'producto'::character varying::text]))`

### `public.detalle_catalogo`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `detalle_id` | `integer` | No | `nextval('detalle_catalogo_detalle_id_seq'::regclass)` |
| `precio_actual` | `numeric(8,2)` | No | — |
| `disponibilidad` | `boolean` | No | `true` |
| `catalogo_id` | `integer` | No | — |
| `producto_id` | `integer` | No | — |

- `detalle_catalogo_pkey` — `PRIMARY KEY (detalle_id)`
- `uq_detalle_catalogo` — `UNIQUE (catalogo_id, producto_id)`
- `detalle_catalogo_catalogo_id_fkey` — `FOREIGN KEY (catalogo_id) REFERENCES catalogo_complejo(catalogo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_catalogo_producto_id_fkey` — `FOREIGN KEY (producto_id) REFERENCES producto_dulceria(producto_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_catalogo_precio_actual_check` — `CHECK (precio_actual >= 0::numeric)`

### `public.detalle_personalizacion`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `personalizacion_id` | `integer` | No | `nextval('detalle_personalizacion_personalizacion_id_seq'::regclass)` |
| `detalle_carrito_id` | `integer` | No | — |
| `opcion_id` | `integer` | No | — |
| `cantidad` | `integer` | No | `1` |
| `porcentaje` | `numeric(5,2)` | Sí | — |

- `detalle_personalizacion_pkey` — `PRIMARY KEY (personalizacion_id)`
- `detalle_personalizacion_detalle_carrito_id_fkey` — `FOREIGN KEY (detalle_carrito_id) REFERENCES detalle_carrito(detalle_carrito_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `detalle_personalizacion_opcion_id_fkey` — `FOREIGN KEY (opcion_id) REFERENCES modificador_opcion(opcion_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_personalizacion_cantidad_check` — `CHECK (cantidad > 0)`
- `detalle_personalizacion_porcentaje_check` — `CHECK (porcentaje > 0::numeric AND porcentaje <= 100::numeric)`

### `public.detalle_puntos`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `detalle_puntos_id` | `integer` | No | `nextval('detalle_puntos_detalle_puntos_id_seq'::regclass)` |
| `fecha_movimiento` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `tipo_movimiento` | `character varying(15)` | No | — |
| `cantidad_puntos` | `numeric(10,2)` | No | — |
| `descripcion_movimiento` | `character varying(200)` | Sí | — |
| `socio_id` | `integer` | No | — |
| `venta_id` | `integer` | Sí | — |

- `detalle_puntos_pkey` — `PRIMARY KEY (detalle_puntos_id)`
- `detalle_puntos_socio_id_fkey` — `FOREIGN KEY (socio_id) REFERENCES cuenta_club(socio_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_puntos_venta_id_fkey` — `FOREIGN KEY (venta_id) REFERENCES venta(venta_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `detalle_puntos_tipo_movimiento_check` — `CHECK (tipo_movimiento::text = ANY (ARRAY['acumulacion'::character varying::text, 'canje'::character varying::text, 'vencimiento'::character varying::text]))`

### `public.detalle_venta_boleto`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `detalle_venta_boleto_id` | `integer` | No | `nextval('detalle_venta_boleto_detalle_venta_boleto_id_seq'::regclass)` |
| `tipo_boleto_id` | `integer` | No | — |
| `precio_unitario` | `numeric(8,2)` | No | — |
| `venta_id` | `integer` | No | — |
| `asiento_id` | `integer` | No | — |
| `estado` | `character varying(20)` | No | `'activo'::character varying` |

- `detalle_venta_boleto_pkey` — `PRIMARY KEY (detalle_venta_boleto_id)`
- `detalle_venta_boleto_asiento_id_fkey` — `FOREIGN KEY (asiento_id) REFERENCES asiento(asiento_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_venta_boleto_tipo_boleto_id_fkey` — `FOREIGN KEY (tipo_boleto_id) REFERENCES tipo_boleto(tipo_boleto_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_venta_boleto_venta_id_fkey` — `FOREIGN KEY (venta_id) REFERENCES venta(venta_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `ck_detalle_venta_boleto_estado` — `CHECK (((estado)::text = ANY ((ARRAY['activo'::character varying, 'cancelado'::character varying])::text[])))`
- `detalle_venta_boleto_precio_unitario_check` — `CHECK ((precio_unitario >= (0)::numeric))`

### `public.detalle_venta_producto`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `detalle_producto_id` | `integer` | No | `nextval('detalle_venta_producto_detalle_producto_id_seq'::regclass)` |
| `cantidad` | `integer` | No | — |
| `precio_unitario` | `numeric(8,2)` | No | — |
| `venta_id` | `integer` | No | — |
| `producto_id` | `integer` | No | — |
| `detalle_catalogo_id` | `integer` | Sí | — |

- `detalle_venta_producto_pkey` — `PRIMARY KEY (detalle_producto_id)`
- `detalle_venta_producto_detalle_catalogo_id_fkey` — `FOREIGN KEY (detalle_catalogo_id) REFERENCES detalle_catalogo(detalle_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_venta_producto_producto_id_fkey` — `FOREIGN KEY (producto_id) REFERENCES producto_dulceria(producto_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_venta_producto_venta_id_fkey` — `FOREIGN KEY (venta_id) REFERENCES venta(venta_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_venta_producto_cantidad_check` — `CHECK ((cantidad > 0))`
- `detalle_venta_producto_precio_unitario_check` — `CHECK ((precio_unitario >= (0)::numeric))`

### `public.detalle_venta_promocion`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `detalle_venta_promocion_id` | `integer` | No | `nextval('detalle_venta_promocion_detalle_venta_promocion_id_seq'::regclass)` |
| `cantidad` | `integer` | No | — |
| `promocion_id` | `integer` | No | — |
| `venta_id` | `integer` | No | — |

- `detalle_venta_promocion_pkey` — `PRIMARY KEY (detalle_venta_promocion_id)`
- `uq_detalle_venta_promo` — `UNIQUE (venta_id, promocion_id)`
- `detalle_venta_promocion_promocion_id_fkey` — `FOREIGN KEY (promocion_id) REFERENCES promocion(promocion_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_venta_promocion_venta_id_fkey` — `FOREIGN KEY (venta_id) REFERENCES venta(venta_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `detalle_venta_promocion_cantidad_check` — `CHECK (cantidad > 0)`

### `public.disponibilidad_modificador`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `disp_id` | `integer` | No | `nextval('disponibilidad_modificador_disp_id_seq'::regclass)` |
| `opcion_id` | `integer` | No | — |
| `catalogo_id` | `integer` | No | — |
| `activo` | `boolean` | No | `true` |
| `precio_extra` | `numeric(10,2)` | No | `0.00` |
| `actualizado_en` | `timestamp with time zone` | No | `now()` |

- `disponibilidad_modificador_pkey` — `PRIMARY KEY (disp_id)`
- `disponibilidad_modificador_opcion_id_catalogo_id_key` — `UNIQUE (opcion_id, catalogo_id)`
- `disponibilidad_modificador_catalogo_id_fkey` — `FOREIGN KEY (catalogo_id) REFERENCES catalogo_complejo(catalogo_id) ON DELETE CASCADE`
- `disponibilidad_modificador_opcion_id_fkey` — `FOREIGN KEY (opcion_id) REFERENCES modificador_opcion(opcion_id) ON DELETE CASCADE`

### `public.empleado`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `empleado_id` | `integer` | No | `nextval('empleado_empleado_id_seq'::regclass)` |
| `nombre` | `character varying(100)` | No | — |
| `apellido_paterno` | `character varying(100)` | No | — |
| `apellido_materno` | `character varying(100)` | Sí | — |
| `telefono` | `character varying(20)` | Sí | — |
| `salario` | `numeric(10,2)` | No | — |
| `fecha_nacimiento` | `date` | No | — |
| `estatus` | `character varying(10)` | No | — |
| `fecha_contratacion` | `date` | No | — |
| `correo` | `character varying(150)` | Sí | — |
| `complejo_id` | `integer` | No | — |
| `rol_id` | `integer` | No | — |

- `empleado_pkey` — `PRIMARY KEY (empleado_id)`
- `empleado_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `empleado_rol_id_fkey` — `FOREIGN KEY (rol_id) REFERENCES rol(rol_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `empleado_estatus_check` — `CHECK (estatus::text = ANY (ARRAY['activo'::character varying::text, 'inactivo'::character varying::text, 'baja'::character varying::text]))`
- `empleado_salario_check` — `CHECK (salario >= 0::numeric)`

### `public.folio`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `folio_id` | `integer` | No | `nextval('folio_folio_id_seq'::regclass)` |
| `codigo` | `character varying(100)` | No | — |
| `tipo_folio` | `character varying(20)` | No | — |
| `tipo_uso` | `character varying(10)` | No | — |
| `estado` | `character varying(10)` | No | `'activo'::character varying` |
| `venta_id` | `integer` | Sí | — |
| `socio_id` | `integer` | Sí | — |

- `folio_pkey` — `PRIMARY KEY (folio_id)`
- `folio_codigo_key` — `UNIQUE (codigo)`
- `folio_socio_id_fkey` — `FOREIGN KEY (socio_id) REFERENCES cuenta_club(socio_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `folio_venta_id_fkey` — `FOREIGN KEY (venta_id) REFERENCES venta(venta_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `folio_estado_check` — `CHECK (estado::text = ANY (ARRAY['activo'::character varying::text, 'usado'::character varying::text, 'vencido'::character varying::text]))`
- `folio_tipo_folio_check` — `CHECK (tipo_folio::text = ANY (ARRAY['Corporativo'::character varying::text, 'Mastercard2x1'::character varying::text, 'Cinepass'::character varying::text, 'Promocional'::character varying::text]))`
- `folio_tipo_uso_check` — `CHECK (tipo_uso::text = ANY (ARRAY['boletos'::character varying::text, 'alimentos'::character varying::text]))`

### `public.formato_sala`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `formato_id` | `integer` | No | `nextval('formato_sala_formato_id_seq'::regclass)` |
| `nombre_formato` | `character varying(50)` | No | — |
| `descripcion` | `text` | Sí | — |
| `es_proyeccion` | `boolean` | No | — |

- `formato_sala_pkey` — `PRIMARY KEY (formato_id)`

### `public.funcion`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `funcion_id` | `integer` | No | `nextval('funcion_funcion_id_seq'::regclass)` |
| `fecha_funcion` | `date` | No | — |
| `hora_inicio` | `time without time zone` | No | — |
| `hora_termino` | `time without time zone` | No | — |
| `idioma` | `character varying(15)` | No | — |
| `es_preventa` | `boolean` | No | `false` |
| `formato_id` | `integer` | No | — |
| `sala_id` | `integer` | No | — |
| `pelicula_id` | `integer` | No | — |
| `activa` | `boolean` | No | `true` |

- `funcion_pkey` — `PRIMARY KEY (funcion_id)`
- `uq_funcion_logica` — `UNIQUE (sala_id, fecha_funcion, hora_inicio)`
- `funcion_formato_id_fkey` — `FOREIGN KEY (formato_id) REFERENCES formato_sala(formato_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `funcion_pelicula_id_fkey` — `FOREIGN KEY (pelicula_id) REFERENCES pelicula(pelicula_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `funcion_sala_id_fkey` — `FOREIGN KEY (sala_id) REFERENCES sala(sala_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `funcion_idioma_check` — `CHECK (idioma::text = ANY (ARRAY['Español'::character varying::text, 'Subtitulada'::character varying::text, 'Doblada'::character varying::text]))`

### `public.garantia_reclamo`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `reclamo_id` | `integer` | No | `nextval('garantia_reclamo_reclamo_id_seq'::regclass)` |
| `nombre_reclamante` | `character varying(150)` | No | — |
| `correo` | `character varying(150)` | No | — |
| `motivo` | `text` | No | — |
| `fecha_reclamo` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `estado_reclamo` | `character varying(15)` | No | `'pendiente'::character varying` |
| `venta_id` | `integer` | Sí | — |
| `pelicula_id` | `integer` | No | — |
| `complejo_id` | `integer` | No | — |

- `garantia_reclamo_pkey` — `PRIMARY KEY (reclamo_id)`
- `garantia_reclamo_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `garantia_reclamo_pelicula_id_fkey` — `FOREIGN KEY (pelicula_id) REFERENCES pelicula(pelicula_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `garantia_reclamo_venta_id_fkey` — `FOREIGN KEY (venta_id) REFERENCES venta(venta_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `garantia_reclamo_estado_reclamo_check` — `CHECK (estado_reclamo::text = ANY (ARRAY['pendiente'::character varying::text, 'aprobado'::character varying::text, 'rechazado'::character varying::text]))`

### `public.historial_producto`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `historial_id` | `integer` | No | `nextval('historial_producto_historial_id_seq'::regclass)` |
| `fecha_cambio` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `precio_nuevo` | `numeric(8,2)` | No | — |
| `precio_anterior` | `numeric(8,2)` | No | — |
| `detalle_id` | `integer` | No | — |

- `historial_producto_pkey` — `PRIMARY KEY (historial_id)`
- `historial_producto_detalle_id_fkey` — `FOREIGN KEY (detalle_id) REFERENCES detalle_catalogo(detalle_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `historial_producto_precio_anterior_check` — `CHECK (precio_anterior >= 0::numeric)`
- `historial_producto_precio_nuevo_check` — `CHECK (precio_nuevo >= 0::numeric)`

### `public.modificador_opcion`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `opcion_id` | `integer` | No | `nextval('modificador_opcion_opcion_id_seq'::regclass)` |
| `regla_id` | `integer` | No | — |
| `api_id_opcion` | `character varying(50)` | No | — |
| `nombre_opcion` | `character varying(100)` | No | — |
| `precio_adicional` | `numeric(8,2)` | No | `0` |
| `imagen_url` | `text` | Sí | — |
| `es_default` | `boolean` | Sí | `false` |

- `modificador_opcion_pkey` — `PRIMARY KEY (opcion_id)`
- `uq_opcion_regla_api` — `UNIQUE (regla_id, api_id_opcion)`
- `modificador_opcion_regla_id_fkey` — `FOREIGN KEY (regla_id) REFERENCES modificador_regla(regla_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `modificador_opcion_precio_adicional_check` — `CHECK (precio_adicional >= 0::numeric)`

### `public.modificador_regla`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `regla_id` | `integer` | No | `nextval('modificador_regla_regla_id_seq'::regclass)` |
| `producto_id` | `integer` | No | — |
| `titulo_regla` | `character varying(150)` | No | — |
| `tipo_modificador` | `character varying(50)` | No | — |
| `min_items` | `integer` | No | `0` |
| `max_items` | `integer` | No | `1` |
| `grupo_titulo` | `text` | Sí | — |
| `grupo_orden` | `integer` | Sí | `0` |

- `modificador_regla_pkey` — `PRIMARY KEY (regla_id)`
- `uq_regla_prod_grupo_titulo_tipo_orden` — `UNIQUE NULLS NOT DISTINCT (producto_id, grupo_titulo, grupo_orden, titulo_regla, tipo_modificador)`
- `modificador_regla_producto_id_fkey` — `FOREIGN KEY (producto_id) REFERENCES producto_dulceria(producto_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `ck_modificador_rango` — `CHECK (min_items <= max_items)`
- `ck_tipo_modificador` — `CHECK (tipo_modificador::text = ANY (ARRAY['flavor'::character varying::text, 'size'::character varying::text, 'extra'::character varying::text, 'ingredient'::character varying::text, 'popcorn'::character varying::text, 'compound'::character varying::text]))`

### `public.nivel_club`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `nivel_id` | `integer` | No | `nextval('nivel_club_nivel_id_seq'::regclass)` |
| `nombre_nivel` | `character varying(50)` | No | — |
| `visitas_min_semestre` | `integer` | No | — |
| `visitas_max_semestre` | `integer` | Sí | — |
| `porcentaje_acumulacion_puntos` | `numeric(5,2)` | No | — |
| `descripcion_beneficios` | `text` | Sí | — |

- `nivel_club_pkey` — `PRIMARY KEY (nivel_id)`
- `ck_nivel_visitas` — `CHECK (visitas_max_semestre IS NULL OR visitas_max_semestre >= visitas_min_semestre)`
- `nivel_club_porcentaje_acumulacion_puntos_check` — `CHECK (porcentaje_acumulacion_puntos > 0::numeric)`
- `nivel_club_visitas_min_semestre_check` — `CHECK (visitas_min_semestre >= 0)`

### `public.otp_verificacion`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `otp_id` | `integer` | No | `nextval('otp_verificacion_otp_id_seq'::regclass)` |
| `correo` | `character varying(150)` | No | — |
| `codigo` | `character varying(6)` | No | — |
| `intentos` | `integer` | No | `0` |
| `expiracion` | `timestamp without time zone` | No | — |
| `usado` | `boolean` | No | `false` |

- `otp_verificacion_pkey` — `PRIMARY KEY (otp_id)`
- `otp_verificacion_intentos_check` — `CHECK (intentos >= 0 AND intentos <= 3)`

### `public.pago`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `pago_id` | `integer` | No | Identidad por defecto |
| `venta_id` | `integer` | No | — |
| `proveedor` | `character varying(50)` | No | — |
| `referencia_proveedor` | `character varying(150)` | Sí | — |
| `idempotency_key` | `character varying(100)` | Sí | — |
| `monto` | `numeric(10,2)` | No | — |
| `moneda` | `character varying(3)` | No | `'MXN'::character varying` |
| `estado` | `character varying(25)` | No | `'pendiente'::character varying` |
| `fecha_pago` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `fecha_conciliacion` | `timestamp without time zone` | Sí | — |
| `respuesta_proveedor` | `jsonb` | Sí | — |

- `pago_pkey` — `PRIMARY KEY (pago_id)`
- `pago_idempotency_key_key` — `UNIQUE (idempotency_key)`
- `pago_venta_id_fkey` — `FOREIGN KEY (venta_id) REFERENCES venta(venta_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `ck_pago_estado` — `CHECK (((estado)::text = ANY ((ARRAY['pendiente'::character varying, 'autorizado'::character varying, 'aprobado'::character varying, 'rechazado'::character varying, 'cancelado'::character varying, 'reembolsado'::character varying])::text[])))`
- `pago_monto_check` — `CHECK ((monto >= (0)::numeric))`

### `public.pelicula`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `pelicula_id` | `integer` | No | `nextval('pelicula_pelicula_id_seq'::regclass)` |
| `vista_id` | `character varying(50)` | Sí | — |
| `titulo` | `character varying(200)` | No | — |
| `slug` | `character varying(200)` | No | — |
| `clasificacion` | `character varying(5)` | No | — |
| `genero` | `character varying(100)` | No | — |
| `duracion_min` | `integer` | No | — |
| `sinopsis` | `text` | No | — |
| `categoria` | `character varying(25)` | No | — |
| `es_especial` | `boolean` | No | `false` |
| `poster_url` | `character varying(500)` | Sí | — |
| `banner_url` | `character varying(500)` | Sí | — |
| `trailer_url` | `character varying(500)` | Sí | — |

- `pelicula_pkey` — `PRIMARY KEY (pelicula_id)`
- `pelicula_slug_key` — `UNIQUE (slug)`
- `pelicula_categoria_check` — `CHECK (categoria::text = ANY (ARRAY['Estreno'::character varying::text, 'Preventa'::character varying::text, 'Cartelera'::character varying::text, 'En Cartelera'::character varying::text, 'Garantia Cinepolis'::character varying::text, 'Evento'::character varying::text, 'Contenido Alternativo'::character varying::text, 'Festival'::character varying::text]))`
- `pelicula_clasificacion_check` — `CHECK (clasificacion::text = ANY (ARRAY['A'::character varying::text, 'AA'::character varying::text, 'B'::character varying::text, 'B15'::character varying::text, 'C'::character varying::text, 'D'::character varying::text, 'TBC'::character varying::text, 'S/C'::character varying::text]))`
- `pelicula_duracion_min_check` — `CHECK (duracion_min > 0)`

### `public.pelicula_persona`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `pelicula_id` | `integer` | No | — |
| `persona_id` | `integer` | No | — |
| `rol` | `character varying(10)` | No | — |

- `pelicula_persona_pkey` — `PRIMARY KEY (pelicula_id, persona_id, rol)`
- `pelicula_persona_pelicula_id_fkey` — `FOREIGN KEY (pelicula_id) REFERENCES pelicula(pelicula_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `pelicula_persona_persona_id_fkey` — `FOREIGN KEY (persona_id) REFERENCES persona(persona_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `pelicula_persona_rol_check` — `CHECK (rol::text = ANY (ARRAY['director'::character varying::text, 'actor'::character varying::text]))`

### `public.permiso`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `permiso_id` | `integer` | No | Identidad por defecto |
| `codigo` | `character varying(80)` | No | — |
| `modulo` | `character varying(50)` | No | — |
| `descripcion` | `text` | Sí | — |

- `permiso_pkey` — `PRIMARY KEY (permiso_id)`
- `permiso_codigo_key` — `UNIQUE (codigo)`

### `public.persona`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `persona_id` | `integer` | No | `nextval('persona_persona_id_seq'::regclass)` |
| `nombre` | `character varying(150)` | No | — |

- `persona_pkey` — `PRIMARY KEY (persona_id)`
- `uq_persona_nombre` — `UNIQUE (nombre)`

### `public.personalizacion_venta`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `personalizacion_venta_id` | `integer` | No | `nextval('personalizacion_venta_personalizacion_venta_id_seq'::regclass)` |
| `detalle_venta_producto_id` | `integer` | No | — |
| `opcion_id` | `integer` | No | — |
| `cantidad` | `integer` | No | `1` |
| `porcentaje` | `numeric(5,2)` | Sí | — |

- `personalizacion_venta_pkey` — `PRIMARY KEY (personalizacion_venta_id)`
- `personalizacion_venta_detalle_venta_producto_id_fkey` — `FOREIGN KEY (detalle_venta_producto_id) REFERENCES detalle_venta_producto(detalle_producto_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `personalizacion_venta_opcion_id_fkey` — `FOREIGN KEY (opcion_id) REFERENCES modificador_opcion(opcion_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `personalizacion_venta_cantidad_check` — `CHECK (cantidad > 0)`
- `personalizacion_venta_porcentaje_check` — `CHECK (porcentaje > 0::numeric AND porcentaje <= 100::numeric)`

### `public.precio_boleto`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `precio_boleto_id` | `integer` | No | `nextval('precio_boleto_precio_boleto_id_seq'::regclass)` |
| `complejo_id` | `integer` | No | — |
| `formato_id` | `integer` | No | — |
| `tipo_boleto_id` | `integer` | No | — |
| `tipo_sala` | `character varying(15)` | No | — |
| `precio` | `numeric(8,2)` | No | — |
| `cargo_servicio_online` | `numeric(8,2)` | No | `0` |
| `area_category_code` | `character varying(20)` | Sí | — |

- `precio_boleto_pkey` — `PRIMARY KEY (precio_boleto_id)`
- `uq_precio_boleto` — `UNIQUE NULLS NOT DISTINCT (complejo_id, formato_id, tipo_boleto_id, tipo_sala, area_category_code)`
- `precio_boleto_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `precio_boleto_formato_id_fkey` — `FOREIGN KEY (formato_id) REFERENCES formato_sala(formato_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `precio_boleto_tipo_boleto_id_fkey` — `FOREIGN KEY (tipo_boleto_id) REFERENCES tipo_boleto(tipo_boleto_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `precio_boleto_cargo_servicio_online_check` — `CHECK (cargo_servicio_online >= 0::numeric)`
- `precio_boleto_precio_check` — `CHECK (precio > 0::numeric)`
- `precio_boleto_tipo_sala_check` — `CHECK (tipo_sala::text = ANY (ARRAY['Tradicional'::character varying::text, 'MACRO XE'::character varying::text, 'IMAX'::character varying::text, 'VIP'::character varying::text, 'Junior'::character varying::text, '4DX'::character varying::text]))`

### `public.producto_dulceria`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `producto_id` | `integer` | No | `nextval('producto_dulceria_producto_id_seq'::regclass)` |
| `api_id` | `character varying(50)` | No | — |
| `nombre_producto` | `character varying(150)` | No | — |
| `estructura` | `character varying(20)` | No | — |
| `descripcion` | `text` | Sí | — |
| `imagen_url` | `character varying(500)` | Sí | — |
| `categoria_id` | `integer` | No | — |

- `producto_dulceria_pkey` — `PRIMARY KEY (producto_id)`
- `producto_dulceria_api_id_key` — `UNIQUE (api_id)`
- `producto_dulceria_categoria_id_fkey` — `FOREIGN KEY (categoria_id) REFERENCES categoria_dulceria(categoria_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `producto_dulceria_estructura_check` — `CHECK (estructura::text = ANY (ARRAY['simple'::character varying::text, 'compound'::character varying::text, 'combo'::character varying::text]))`

### `public.promocion`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `promocion_id` | `integer` | No | `nextval('promocion_promocion_id_seq'::regclass)` |
| `nombre_promocion` | `character varying(150)` | No | — |
| `tipo_promocion` | `character varying(20)` | No | — |
| `aplica_todos` | `boolean` | No | `true` |
| `restricciones` | `text` | Sí | — |
| `fecha_inicio_vigencia` | `date` | No | — |
| `fecha_fin_vigencia` | `date` | Sí | — |
| `descripcion` | `text` | Sí | — |
| `nivel_id` | `integer` | Sí | — |

- `promocion_pkey` — `PRIMARY KEY (promocion_id)`
- `promocion_nivel_id_fkey` — `FOREIGN KEY (nivel_id) REFERENCES nivel_club(nivel_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `promocion_tipo_promocion_check` — `CHECK (tipo_promocion::text = ANY (ARRAY['descuento'::character varying::text, '2x1'::character varying::text, 'combo'::character varying::text, 'beneficio_nivel'::character varying::text, 'precio_especial'::character varying::text]))`

### `public.promocion_complejo`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `promocion_id` | `integer` | No | — |
| `complejo_id` | `integer` | No | — |

- `promocion_complejo_pkey` — `PRIMARY KEY (promocion_id, complejo_id)`
- `promocion_complejo_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `promocion_complejo_promocion_id_fkey` — `FOREIGN KEY (promocion_id) REFERENCES promocion(promocion_id) ON UPDATE CASCADE ON DELETE CASCADE`

### `public.punto_venta`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `punto_venta_id` | `integer` | No | `nextval('punto_venta_punto_venta_id_seq'::regclass)` |
| `tipo_punto` | `character varying(10)` | No | — |
| `descripcion` | `character varying(200)` | Sí | — |
| `complejo_id` | `integer` | No | — |

- `punto_venta_pkey` — `PRIMARY KEY (punto_venta_id)`
- `punto_venta_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `punto_venta_tipo_punto_check` — `CHECK (tipo_punto::text = ANY (ARRAY['Taquilla'::character varying::text, 'Kiosco'::character varying::text, 'Dulcería'::character varying::text, 'Online'::character varying::text]))`

### `public.rol`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `rol_id` | `integer` | No | `nextval('rol_rol_id_seq'::regclass)` |
| `nombre_rol` | `character varying(100)` | No | — |
| `es_mando` | `boolean` | No | `false` |
| `descripcion_responsabilidades` | `text` | Sí | — |
| `area_trabajo` | `character varying(100)` | Sí | — |

- `rol_pkey` — `PRIMARY KEY (rol_id)`

### `public.rol_permiso`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `rol_id` | `integer` | No | — |
| `permiso_id` | `integer` | No | — |

- `rol_permiso_pkey` — `PRIMARY KEY (rol_id, permiso_id)`
- `rol_permiso_permiso_id_fkey` — `FOREIGN KEY (permiso_id) REFERENCES permiso(permiso_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `rol_permiso_rol_id_fkey` — `FOREIGN KEY (rol_id) REFERENCES rol(rol_id) ON UPDATE CASCADE ON DELETE CASCADE`

### `public.sala`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `sala_id` | `integer` | No | `nextval('sala_sala_id_seq'::regclass)` |
| `numero_sala` | `integer` | No | — |
| `nombre_sala` | `character varying(50)` | No | — |
| `capacidad_asientos` | `integer` | No | — |
| `tipo_sala` | `character varying(15)` | No | — |
| `complejo_id` | `integer` | No | — |

- `sala_pkey` — `PRIMARY KEY (sala_id)`
- `uq_sala_complejo` — `UNIQUE (complejo_id, numero_sala)`
- `sala_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `sala_capacidad_asientos_check` — `CHECK (capacidad_asientos > 0)`
- `sala_tipo_sala_check` — `CHECK (tipo_sala::text = ANY (ARRAY['Tradicional'::character varying::text, 'MACRO XE'::character varying::text, 'IMAX'::character varying::text, 'VIP'::character varying::text, 'Junior'::character varying::text, '4DX'::character varying::text]))`

### `public.sala_formato_sala`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `sala_id` | `integer` | No | — |
| `formato_id` | `integer` | No | — |

- `sala_formato_sala_pkey` — `PRIMARY KEY (sala_id, formato_id)`
- `sala_formato_sala_formato_id_fkey` — `FOREIGN KEY (formato_id) REFERENCES formato_sala(formato_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `sala_formato_sala_sala_id_fkey` — `FOREIGN KEY (sala_id) REFERENCES sala(sala_id) ON UPDATE CASCADE ON DELETE CASCADE`

### `public.sesion`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `sesion_id` | `integer` | No | `nextval('sesion_sesion_id_seq'::regclass)` |
| `usuario_empleado` | `character varying(100)` | No | — |
| `estado_sesion` | `character varying(10)` | No | `'abierta'::character varying` |
| `monto_inicial` | `numeric(10,2)` | No | — |
| `monto_final_sistema` | `numeric(10,2)` | Sí | — |
| `fecha_hora_inicio` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `fecha_hora_fin` | `timestamp without time zone` | Sí | — |
| `empleado_id` | `integer` | No | — |
| `punto_venta_id` | `integer` | No | — |

- `sesion_pkey` — `PRIMARY KEY (sesion_id)`
- `sesion_empleado_id_fkey` — `FOREIGN KEY (empleado_id) REFERENCES empleado(empleado_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `sesion_punto_venta_id_fkey` — `FOREIGN KEY (punto_venta_id) REFERENCES punto_venta(punto_venta_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `sesion_estado_sesion_check` — `CHECK (estado_sesion::text = ANY (ARRAY['abierta'::character varying::text, 'cerrada'::character varying::text]))`
- `sesion_monto_inicial_check` — `CHECK (monto_inicial >= 0::numeric)`

### `public.sesion_admin`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `sesion_admin_id` | `integer` | No | Identidad por defecto |
| `usuario_admin_id` | `integer` | No | — |
| `token_hash` | `character varying(255)` | No | — |
| `ip_origen` | `inet` | Sí | — |
| `user_agent` | `text` | Sí | — |
| `fecha_inicio` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `fecha_expiracion` | `timestamp without time zone` | No | — |
| `revocada` | `boolean` | No | `false` |

- `sesion_admin_pkey` — `PRIMARY KEY (sesion_admin_id)`
- `sesion_admin_token_hash_key` — `UNIQUE (token_hash)`
- `sesion_admin_usuario_admin_id_fkey` — `FOREIGN KEY (usuario_admin_id) REFERENCES usuario_admin(usuario_admin_id) ON UPDATE CASCADE ON DELETE CASCADE`
- `ck_sesion_admin_fechas` — `CHECK ((fecha_expiracion > fecha_inicio))`

### `public.tipo_boleto`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `tipo_boleto_id` | `integer` | No | `nextval('tipo_boleto_tipo_boleto_id_seq'::regclass)` |
| `nombre` | `character varying(100)` | No | — |
| `descripcion` | `text` | Sí | — |
| `es_promocional` | `boolean` | No | `false` |
| `precio_fijo` | `numeric(8,2)` | Sí | — |

- `tipo_boleto_pkey` — `PRIMARY KEY (tipo_boleto_id)`
- `uq_tipo_boleto_nombre` — `UNIQUE (nombre)`
- `tipo_boleto_precio_fijo_check` — `CHECK (precio_fijo > 0::numeric)`

### `public.usuario_admin`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `usuario_admin_id` | `integer` | No | Identidad por defecto |
| `empleado_id` | `integer` | Sí | — |
| `username` | `character varying(80)` | No | — |
| `correo` | `character varying(150)` | No | — |
| `password_hash` | `character varying(255)` | No | — |
| `rol_id` | `integer` | No | — |
| `complejo_id` | `integer` | Sí | — |
| `activo` | `boolean` | No | `true` |
| `ultimo_acceso` | `timestamp without time zone` | Sí | — |
| `fecha_creacion` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |

- `usuario_admin_pkey` — `PRIMARY KEY (usuario_admin_id)`
- `usuario_admin_correo_key` — `UNIQUE (correo)`
- `usuario_admin_empleado_id_key` — `UNIQUE (empleado_id)`
- `usuario_admin_username_key` — `UNIQUE (username)`
- `usuario_admin_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `usuario_admin_empleado_id_fkey` — `FOREIGN KEY (empleado_id) REFERENCES empleado(empleado_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `usuario_admin_rol_id_fkey` — `FOREIGN KEY (rol_id) REFERENCES rol(rol_id) ON UPDATE CASCADE ON DELETE RESTRICT`

### `public.venta`

| Columna | Tipo PostgreSQL | Admite NULL | Valor por defecto |
| --- | --- | --- | --- |
| `venta_id` | `integer` | No | `nextval('venta_venta_id_seq'::regclass)` |
| `importe_total` | `numeric(10,2)` | No | — |
| `cargo_servicio` | `numeric(8,2)` | No | `0` |
| `forma_pago` | `character varying(15)` | No | — |
| `tipo_venta` | `character varying(10)` | No | — |
| `fecha_venta` | `timestamp without time zone` | No | `CURRENT_TIMESTAMP` |
| `sesion_id` | `integer` | Sí | — |
| `socio_id` | `integer` | Sí | — |
| `nombre_comprador` | `character varying(100)` | Sí | — |
| `apellido_comprador` | `character varying(100)` | Sí | — |
| `correo_comprador` | `character varying(150)` | Sí | — |
| `estado` | `character varying(20)` | No | `'pagada'::character varying` |
| `fecha_cancelacion` | `timestamp without time zone` | Sí | — |
| `motivo_cancelacion` | `text` | Sí | — |
| `monto_reembolsado` | `numeric(10,2)` | No | `0` |
| `cancelado_por_admin_id` | `integer` | Sí | — |
| `complejo_id` | `integer` | Sí | — |
| `carrito_id` | `integer` | Sí | — |

- `venta_pkey` — `PRIMARY KEY (venta_id)`
- `venta_cancelado_por_admin_id_fkey` — `FOREIGN KEY (cancelado_por_admin_id) REFERENCES usuario_admin(usuario_admin_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `venta_carrito_id_fkey` — `FOREIGN KEY (carrito_id) REFERENCES carrito(carrito_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `venta_complejo_id_fkey` — `FOREIGN KEY (complejo_id) REFERENCES complejo(complejo_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `venta_sesion_id_fkey` — `FOREIGN KEY (sesion_id) REFERENCES sesion(sesion_id) ON UPDATE CASCADE ON DELETE RESTRICT`
- `venta_socio_id_fkey` — `FOREIGN KEY (socio_id) REFERENCES cuenta_club(socio_id) ON UPDATE CASCADE ON DELETE SET NULL`
- `ck_venta_cancelacion_fecha` — `CHECK (((((estado)::text = ANY ((ARRAY['cancelada'::character varying, 'reembolsada'::character varying])::text[])) AND (fecha_cancelacion IS NOT NULL)) OR ((estado)::text <> ALL ((ARRAY['cancelada'::character varying, 'reembolsada'::character varying])::text[]))))`
- `ck_venta_estado` — `CHECK (((estado)::text = ANY ((ARRAY['pendiente'::character varying, 'pagada'::character varying, 'cancelada'::character varying, 'reembolsada'::character varying])::text[])))`
- `ck_venta_monto_reembolsado` — `CHECK (((monto_reembolsado >= (0)::numeric) AND (monto_reembolsado <= importe_total)))`
- `ck_venta_sesion_online` — `CHECK (((sesion_id IS NOT NULL) OR ((tipo_venta)::text = 'Online'::text)))`
- `venta_cargo_servicio_check` — `CHECK ((cargo_servicio >= (0)::numeric))`
- `venta_forma_pago_check` — `CHECK (((forma_pago)::text = ANY (ARRAY[('Tarjeta'::character varying)::text, ('PayPal'::character varying)::text, ('ClickToPay'::character varying)::text, ('Aplazo'::character varying)::text, ('Efectivo'::character varying)::text, ('Puntos'::character varying)::text])))`
- `venta_importe_total_check` — `CHECK ((importe_total >= (0)::numeric))`
- `venta_tipo_venta_check` — `CHECK (((tipo_venta)::text = ANY (ARRAY[('Online'::character varying)::text, ('Taquilla'::character varying)::text, ('Kiosco'::character varying)::text, ('Dulcería'::character varying)::text])))`
