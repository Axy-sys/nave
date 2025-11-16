# Assets SVG - CyberSecurity Defender

Todos los assets fueron creados con estilo minimalista de líneas simples y trazos ligeros.

## 🎨 Assets Creados

### Nave del Jugador
- `player_ship.svg` - Nave triangular tipo caza espacial en cyan (#00ffff)

### Enemigos (Amenazas de Ciberseguridad)
- `enemy_malware.svg` - Forma orgánica irregular en rojo/rosa (#ff0066)
- `enemy_phishing.svg` - Forma de anzuelo en naranja (#ffaa00)
- `enemy_ddos.svg` - Flechas convergentes en rojo (#ff3333)
- `enemy_sql_injection.svg` - Jeringa inyectando código en morado (#9933ff)
- `enemy_bruteforce.svg` - Martillo con líneas de impacto en naranja (#ff6600)
- `enemy_ransomware.svg` - Candado con cadenas (BOSS) en rojo oscuro (#cc0000)

### Proyectiles
- `projectile_firewall.svg` - Hexágono con cruz en cyan (#00ffff)
- `projectile_antivirus.svg` - Cruz médica circular en verde (#00ff88)
- `projectile_encryption.svg` - Candado cerrado en naranja (#ffaa00)

### Power-Ups
- `powerup_antivirus.svg` - Escudo con cruz en verde (#00ff88)
- `powerup_firewall.svg` - Muro de protección en azul (#00aaff)
- `powerup_encryption.svg` - Llave en naranja (#ffaa00)
- `powerup_2fa.svg` - Doble verificación (1-2) en magenta (#ff66ff)
- `powerup_patch.svg` - Vendaje/parche en verde (#66ff66)

### Fondo
- `background.svg` - Grid cibernético espacial con gradiente azul oscuro

## 📐 Características de Diseño

- **Estilo**: Líneas vectoriales simples, trazos ligeros
- **Colores**: Paleta cibernética neón sobre fondo oscuro
- **Formato**: SVG escalable sin pérdida de calidad
- **Compatibilidad**: Optimizado para Godot 4.x

## 🔧 Uso en Godot

Los SVG se importan automáticamente como `CompressedTexture2D` y están listos para usar en las escenas.

Para cambiar el color de un sprite en runtime:
```gdscript
$Sprite2D.modulate = Color(1, 0, 0) # Rojo
```

## 🎨 Personalización

Puedes editar los SVG con cualquier editor vectorial:
- Inkscape (gratis)
- Adobe Illustrator
- Figma
- Cualquier editor de texto (son XML)

## 📊 Tamaños

- Player Ship: 64x64px
- Enemigos: 48x48px
- Boss: 64x64px
- Proyectiles: 24-28px
- Power-ups: 32x32px
- Background: 1200x800px

---

Todos los assets están diseñados para ser educativos y representar visualmente conceptos de ciberseguridad.
