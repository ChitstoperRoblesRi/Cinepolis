-- Nombres de presentación verificados por SKU, regla y etiqueta anterior.
BEGIN;
WITH etiquetas(api_id, anterior, nuevo) AS (
    VALUES
        ('14089', 'Refresco', 'Jumbo'),
        ('18446', 'Palomitas', 'Jumbo'),
        ('26052', 'Frappé Agua.', 'Jumbo'),
        ('28748', 'Frappé Futbolista', 'Jumbo'),
        ('26055', 'Frappé Leche', 'Jumbo'),
        ('26058', 'Frappé Premium', 'Jumbo'),
        ('26051', 'Frappé de Pelón®  Pelonada', 'Jumbo'),
        ('21408', 'Tequesitos', 'Jumbo'),
        ('21455', 'Tequesitos®', 'Jumbo'),
        ('18972', 'ICEE®', 'Grande'),
        ('15743', 'Vaso Granel', 'Grande'),
        ('14734', 'Americano', 'Grande'),
        ('14739', 'Americano Frío', 'Grande'),
        ('15176', 'Capuccino', 'Grande'),
        ('15060', 'Chocolate', 'Grande'),
        ('15143', 'Latte', 'Grande'),
        ('15147', 'Latte Frío', 'Grande'),
        ('15117', 'Moka Obscuro', 'Grande'),
        ('15118', 'Moka Obscuro Frío', 'Grande'),
        ('14733', 'Té Chai latte', 'Grande'),
        ('14709', 'Té Frío', 'Grande'),
        ('14700', 'Té caliente', 'Grande'),
        ('28578', 'Boba Dragon Ice', 'Grande'),
        ('28588', 'Boba Dragon Ice Milk', 'Grande'),
        ('28583', 'Boba Pink Lemonade', 'Grande'),
        ('28586', 'Boba Tropical Ice', 'Grande'),
        ('25671', 'Dragon Ice', 'Grande'),
        ('25677', 'Dragon Ice Milk', 'Grande'),
        ('25679', 'Pink Lemonade', 'Grande'),
        ('25681', 'Tropical Ice', 'Grande'),
        ('14175', 'Nachos', 'Grandes'),
        ('15342', 'Hot Dog', 'Chico'),
        ('14846', 'Hot Dog Guacamole', 'Chico'),
        ('14212', 'Hot Dog Individual', 'Chico'),
        ('25148', 'Texas Dog', 'Chico'),
        ('21042', 'Hot Dog Doritos', 'Chico'),
        ('28537', 'Mac & Cheese Dog', 'Chico'),
        ('25929', 'Cheesy Boneless Dog', 'Chico'),
        ('14662', 'Papas Twister', 'Tradicional (170 g)'),
        ('275', 'Agua Embotellada', '1 L'),
        ('14509', 'Sangre de Toro Cosecha', 'Copa')
)
UPDATE public.modificador_opcion AS o
SET nombre_opcion = e.nuevo
FROM etiquetas AS e, public.modificador_regla AS r
WHERE r.regla_id = o.regla_id
  AND o.api_id_opcion = e.api_id
  AND lower(trim(o.nombre_opcion)) = lower(e.anterior)
  AND lower(r.titulo_regla) ~ '\mtama[ñn]os?\M'
  AND current_database() = 'cinepolis_vision'
RETURNING o.opcion_id, e.anterior, o.nombre_opcion;
COMMIT;
