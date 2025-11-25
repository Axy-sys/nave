# 🚀 Guía de Inicio Rápido - CyberSecurity Defender

## ✅ Lo que ya está listo:

### 1. ✔️ Input Map Configurado
El archivo `project.godot` ya tiene todos los controles configurados:
- WASD / Flechas: Movimiento
- Espacio / Click Izquierdo: Disparar
- 1-4: Cambiar armas
- ESC: Pausar

### 2. ✔️ Assets SVG Creados (16 archivos)
Todos en `Assets/`:
- 1 nave de jugador
- 6 tipos de enemigos
- 3 tipos de proyectiles
- 5 power-ups
- 1 fondo espacial

**Estilo**: Líneas simples vectoriales, colores neón, trazos ligeros

### 3. ✔️ Escenas .tscn Creadas (8 archivos)
- `Main.tscn` - Escena principal
- `Player.tscn` - Jugador con componentes
- `Projectile.tscn` - Proyectil base
- `Enemies/` - 4 tipos de enemigos (Malware, Phishing, DDoS, Ransomware)
- `PowerUp.tscn` - Power-up base con animación

## 🎮 Cómo ejecutar el juego:

### Paso 1: Abrir en Godot
1. Abre Godot 4.x
2. Importa el proyecto desde la carpeta `nave`
3. Espera a que Godot importe todos los assets

### Paso 2: Configurar la Escena Principal
El proyecto ya está configurado para usar `res://Scenes/Main.tscn` como escena principal.

### Paso 3: Compilar C#
1. Ve a **Build > Build Project** (o presiona Ctrl+B)
2. Espera a que compile todo el código C#
3. Si hay errores, verifica que tengas .NET SDK instalado

### Paso 4: ¡Jugar!
Presiona F5 o el botón Play en Godot.

## 🔧 Posibles Ajustes Necesarios:

### Si el jugador no aparece:
1. Abre `Scenes/Main.tscn`
2. Instancia `Player.tscn` como hijo de Main
3. Posiciónalo en (600, 600)

### Si los sprites no se ven:
1. Verifica que los archivos .svg.import se hayan creado
2. Reimporta los assets (click derecho > Reimport)
3. Revisa que los UIDs en las escenas coincidan

### Si hay errores de compilación C#:
1. Verifica que tengas .NET 8.0 SDK instalado
2. Ejecuta `dotnet --version` en terminal
3. Reconstruye el proyecto en Godot

## 🎯 Orden de Desarrollo Recomendado:

1. **Probar el jugador básico**
   - Movimiento
   - Disparo
   
2. **Añadir enemigos manualmente**
   - Instancia `EnemyMalware.tscn` en Main
   - Verifica que se mueva hacia el jugador
   
3. **Conectar los sistemas**
   - Asegúrate de que GameManager se inicialice
   - Verifica que el EventBus funcione
   
4. **Añadir el HUD**
   - GameHUD debería mostrarse automáticamente
   
5. **Sistema de oleadas**
   - WaveSystem comenzará a spawner enemigos
   
6. **Power-ups y quiz**
   - PowerUpSystem generará power-ups
   - QuizView mostrará preguntas

## 📁 Estructura Final:

```
nave/
├── Assets/                 ✔️ 16 SVG creados
│   ├── player_ship.svg
│   ├── enemy_*.svg (6)
│   ├── projectile_*.svg (3)
│   ├── powerup_*.svg (5)
│   └── background.svg
├── Scenes/                 ✔️ 8 escenas creadas
│   ├── Main.tscn
│   ├── Player.tscn
│   ├── Projectile.tscn
│   ├── PowerUp.tscn
│   └── Enemies/ (4)
├── Scripts/                ✔️ 30+ archivos C#
│   ├── Core/
│   ├── Components/
│   ├── Entities/
│   ├── Weapons/
│   ├── Systems/
│   ├── Education/
│   ├── Views/
│   └── Models/
└── project.godot          ✔️ Inputs configurados
```

## 🐛 Troubleshooting:

### "No se encuentra el script C#"
- Asegúrate de haber compilado el proyecto (Build > Build Project)
- Verifica que los paths en los .tscn coincidan con los archivos .cs

### "El jugador no se mueve"
- Verifica que los inputs estén configurados (Project Settings > Input Map)
- Revisa que el script Player.cs esté adjunto al nodo

### "No aparecen enemigos"
- El WaveSystem tarda 20 segundos en spawner la primera oleada
- Puedes reducir `TimeBetweenWaves` en el inspector

### "No veo el HUD"
- Verifica que GameHUD sea un CanvasLayer
- Debe ser hijo directo de Main

## 🎓 Próximos Pasos Opcionales:

1. **Mejorar Visuales**
   - Añadir efectos de partículas
   - Agregar trails a los proyectiles
   - Screen shake en explosiones

2. **Audio**
   - Música de fondo
   - Efectos de sonido
   - Feedback auditivo

3. **Más Contenido**
   - Más tipos de enemigos
   - Más armas
   - Más preguntas educativas
   - Jefes únicos por cada 5 niveles

4. **Polish**
   - Menú principal
   - Pantalla de Game Over
   - Sistema de High Scores
   - Tutorial inicial

## 💡 Consejos:

- **Empieza simple**: Primero haz que el jugador se mueva y dispare
- **Itera rápido**: Prueba frecuentemente
- **Usa GD.Print()**: Para debug en C#
- **Consulta la documentación**: Los archivos .md tienen info detallada

## 📖 Documentación Adicional:

- `README.md` - Descripción general del proyecto
- `DESIGN_PATTERNS.md` - Guía de patrones de diseño
- `INPUT_SETUP.md` - Configuración de controles
- `Assets/README_ASSETS.md` - Info sobre los assets

---

**¡El juego está listo para empezar a desarrollar! 🎮🛡️**

Si tienes problemas, revisa primero que:
1. ✅ Godot 4.x está instalado
2. ✅ .NET SDK 8.0+ está instalado
3. ✅ El proyecto se ha compilado (Build Project)
4. ✅ Los assets se han importado correctamente
