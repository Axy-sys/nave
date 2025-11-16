# CyberSecurity Defender 🛡️

Un juego educativo sobre ciberseguridad inspirado en Endless Sky, desarrollado con Godot y C#.

## 🎮 Características

### Arquitectura del Código
- **Patrón MVC**: Separación clara entre Model, View y Controller
- **Principios SOLID**: Cada clase tiene una responsabilidad única
- **Composición sobre Herencia**: Sistema de componentes reutilizables
- **Patrones de Diseño Implementados**:
  - **Factory Pattern**: Creación de enemigos y power-ups
  - **Strategy Pattern**: Diferentes comportamientos AI y armas
  - **Observer Pattern**: Sistema de eventos global (EventBus)
  - **Component Pattern**: Entidades modulares
  - **Singleton Pattern**: Managers globales
  - **Template Method Pattern**: Clases base abstractas

### Sistemas del Juego

#### 1. Sistema de Componentes
- `HealthComponent`: Gestión de salud y resistencias
- `MovementComponent`: Control de movimiento con física
- `WeaponComponent`: Sistema de armas intercambiables
- `ShieldComponent`: Escudos con recarga automática

#### 2. Enemigos (Factory Pattern)
Cada enemigo representa una amenaza de ciberseguridad:
- **Malware**: Movimiento en zigzag
- **Phishing**: Acercamiento sigiloso y ataque rápido
- **DDoS**: Ataques coordinados en formación
- **SQL Injection**: Movimiento impredecible
- **Brute Force**: Ataques directos y persistentes
- **Ransomware**: Boss con múltiples fases

#### 3. Armas Temáticas
- **Firewall**: Arma básica, munición infinita
- **Antivirus**: Ráfagas de 3 proyectiles
- **Encryption Cannon**: Proyectiles poderosos
- **Honeypot**: Trampas estáticas

#### 4. Sistema Educativo
- **QuizSystem**: 13+ preguntas sobre ciberseguridad
- **SecurityTipsSystem**: Tips contextuales según enemigos
- **VulnerabilitySystem**: Vulnerabilidades que aparecen y deben parchearse

#### 5. Power-Ups Educativos
- Actualización de Antivirus
- Mejora de Firewall
- Escudo de Encriptación
- Parche de Seguridad
- Autenticación 2FA
- Restauración de Backup
- Sistema IDS Mejorado
- VPN Segura
- Pregunta Bonus

#### 6. Sistema de Oleadas
- Dificultad progresiva
- Boss cada 5 oleadas
- Spawning escalonado de enemigos

## 📁 Estructura del Proyecto

```
Scripts/
├── Core/
│   ├── Interfaces/          # IComponent, IDamageable, IMovable, IWeapon
│   ├── Events/              # GameEventBus (Observer Pattern)
│   └── GameManager.cs       # Controller principal (MVC)
├── Models/                  # Models del patrón MVC
│   ├── GameStateModel.cs
│   └── PlayerModel.cs
├── Views/                   # Views del patrón MVC
│   ├── GameHUD.cs
│   └── QuizView.cs
├── Components/              # Component Pattern
│   ├── BaseComponent.cs
│   ├── HealthComponent.cs
│   ├── MovementComponent.cs
│   ├── WeaponComponent.cs
│   └── ShieldComponent.cs
├── Entities/
│   ├── Player.cs            # Composición de componentes
│   ├── EnemyFactory.cs      # Factory Pattern
│   ├── EnemyAI.cs           # Strategy Pattern
│   └── Projectile.cs
├── Weapons/
│   ├── BaseWeapon.cs
│   └── WeaponTypes.cs       # Implementaciones específicas
├── Systems/
│   ├── WaveSystem.cs
│   ├── PowerUpSystem.cs
│   └── VulnerabilitySystem.cs
└── Education/
    ├── QuizSystem.cs
    └── SecurityTipsSystem.cs
```

## 🎯 Principios SOLID Aplicados

1. **Single Responsibility**: Cada clase tiene una única razón para cambiar
2. **Open/Closed**: Fácil extensión sin modificar código existente
3. **Liskov Substitution**: Las interfaces son intercambiables
4. **Interface Segregation**: Interfaces pequeñas y específicas
5. **Dependency Inversion**: Dependencia de abstracciones, no implementaciones

## 🚀 Controles

- **WASD / Flechas**: Movimiento
- **Clic / Espacio**: Disparar
- **1-4**: Cambiar armas
- **ESC**: Pausar
- **Shift + Enter**: Mostrar pregunta (debug)

## 🎓 Temas Educativos Cubiertos

- Tipos de malware y su prevención
- Ataques de phishing y cómo identificarlos
- Seguridad de contraseñas y autenticación
- Encriptación y protección de datos
- Firewalls y seguridad de red
- SQL Injection y seguridad web
- Ataques DDoS y mitigación
- Vulnerabilidades comunes
- Mejores prácticas de ciberseguridad

## 🔧 Próximos Pasos

Para completar el juego:

1. **Crear Assets Visuales**:
   - Sprites para nave del jugador
   - Sprites para enemigos (cada tipo diferente)
   - Sprites para proyectiles
   - Sprites para power-ups
   - Background espacial con parallax

2. **Crear Escenas Godot**:
   - `Main.tscn`: Escena principal
   - `Player.tscn`: Nave del jugador
   - `BaseEnemy.tscn`: Template de enemigo
   - `Projectile.tscn`: Proyectil base
   - `PowerUp.tscn`: Power-up base

3. **Configurar Input Map** en project.godot:
   ```
   move_up, move_down, move_left, move_right
   fire
   weapon_1, weapon_2, weapon_3, weapon_4
   ```

4. **Agregar Audio**:
   - Música de fondo
   - Efectos de sonido para disparos
   - Sonidos de impacto
   - Música de boss

5. **Pulir Gameplay**:
   - Balance de dificultad
   - Efectos de partículas
   - Screen shake en explosiones
   - Transiciones suaves

## 💡 Extensibilidad

Gracias a los patrones de diseño, es fácil añadir:
- Nuevos tipos de enemigos (EnemyFactory)
- Nuevas armas (IWeapon interface)
- Nuevos power-ups (PowerUpType enum)
- Nuevas preguntas educativas (QuizSystem)
- Nuevos componentes (IComponent interface)

## 📚 Aprendizajes del Código

El código está diseñado para enseñar:
1. Cómo estructurar proyectos grandes
2. Patrones de diseño en práctica
3. Principios SOLID aplicados
4. Arquitectura MVC en juegos
5. Composición vs Herencia
6. Event-driven architecture

---

**Desarrollado con ❤️ usando Godot 4.x y C#**
