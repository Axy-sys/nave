# Configuración de Input Maps para Godot

Este archivo documenta los controles necesarios para el juego.

## 📝 Configuración en Godot

Para configurar los controles, ve a **Project > Project Settings > Input Map** y añade las siguientes acciones:

### Movimiento
```
move_up
  - Keyboard: W
  - Keyboard: Arrow Up

move_down
  - Keyboard: S
  - Keyboard: Arrow Down

move_left
  - Keyboard: A
  - Keyboard: Arrow Left

move_right
  - Keyboard: D
  - Keyboard: Arrow Right
```

### Combate
```
fire
  - Keyboard: Space
  - Mouse: Left Button
```

### Cambio de Armas
```
weapon_1
  - Keyboard: 1

weapon_2
  - Keyboard: 2

weapon_3
  - Keyboard: 3

weapon_4
  - Keyboard: 4
```

### UI y Sistema
```
ui_cancel (ya existe por defecto)
  - Keyboard: Escape

ui_accept (ya existe por defecto)
  - Keyboard: Enter
```

## 🎮 Controles del Juego

| Acción | Teclas | Descripción |
|--------|--------|-------------|
| Mover Arriba | W / ↑ | Mueve la nave hacia arriba |
| Mover Abajo | S / ↓ | Mueve la nave hacia abajo |
| Mover Izquierda | A / ← | Mueve la nave hacia izquierda |
| Mover Derecha | D / → | Mueve la nave hacia derecha |
| Disparar | Espacio / Click Izquierdo | Dispara el arma actual |
| Arma 1 | 1 | Equipa Firewall (básico) |
| Arma 2 | 2 | Equipa Antivirus (ráfaga) |
| Arma 3 | 3 | Equipa Encryption Cannon |
| Arma 4 | 4 | Equipa Honeypot |
| Pausar | ESC | Pausa/Resume el juego |
| Aceptar | Enter | Confirma en menús |

## 💻 Código de Ejemplo

```csharp
// Verificar input en código C#
if (Input.IsActionPressed("move_up"))
{
    // Mover hacia arriba
}

if (Input.IsActionJustPressed("fire"))
{
    // Disparar (solo al presionar, no mantener)
}

if (Input.IsActionJustPressed("weapon_1"))
{
    // Cambiar a arma 1
}
```

## 🔧 Input Alternativo (Opcional)

Si quieres añadir soporte para gamepad:

### Gamepad
```
move_up
  - Joypad: Left Stick Up
  - Joypad: D-Pad Up

move_down
  - Joypad: Left Stick Down
  - Joypad: D-Pad Down

move_left
  - Joypad: Left Stick Left
  - Joypad: D-Pad Left

move_right
  - Joypad: Left Stick Right
  - Joypad: D-Pad Right

fire
  - Joypad: Button 0 (A/Cross)

weapon_1
  - Joypad: Button 2 (X/Square)

weapon_2
  - Joypad: Button 3 (Y/Triangle)
```

## 📱 Mobile (Futuro)

Para versión móvil, se pueden añadir controles táctiles:
- Joystick virtual para movimiento
- Botón de disparo en pantalla
- Menú de armas táctil

---

**Nota**: Asegúrate de configurar estos inputs antes de ejecutar el juego, de lo contrario los controles no funcionarán correctamente.
