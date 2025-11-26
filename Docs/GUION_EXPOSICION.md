# 🎤 GUION DE EXPOSICIÓN - CODE RIPPIER

## 📋 Información General
- **Duración recomendada:** 10-15 minutos
- **Formato:** Exposición técnica + Demo del juego
- **Materiales necesarios:** Diagramas UML, Demo del juego, Presentación web

---

## 🚀 ESTRUCTURA DE LA EXPOSICIÓN

### PARTE 1: INTRODUCCIÓN (2 minutos)

#### 🎯 Slide de Apertura
> "Buenos días/tardes. Soy [nombre] del equipo CodeRippier, y hoy les presentaré **Code Rippier**, un juego educativo de ciberseguridad que combina entretenimiento con aprendizaje real."

#### 🔑 Puntos Clave a Mencionar:
- **Nombre del proyecto:** Code Rippier - Cybersecurity Defense Game
- **Objetivo:** Enseñar ciberseguridad de forma interactiva
- **Tecnología:** Godot Engine 4.x + C# / .NET 8.0
- **Equipo:** 5 miembros con roles especializados

#### 💡 Hook Inicial (gancho para captar atención):
> "¿Sabían que el 95% de los ataques de ciberseguridad ocurren por error humano? Code Rippier busca cambiar eso, educando mientras divierte."

---

### PARTE 2: CONCEPTO DEL JUEGO (2 minutos)

#### 🎮 Descripción del Gameplay:
> "Code Rippier es un simulador táctico donde el jugador defiende sistemas informáticos contra amenazas reales: Phishing, Ransomware, DDoS, Malware..."

#### ⚡ Mecánica Innovadora - Sistema CPU (Flux):
> "Lo que hace único a nuestro juego es el sistema de **CPU o Flux**. No hay cooldowns tradicionales. Cada acción consume ciclos de procesamiento:
> - Disparar = 5-25% CPU
> - Escudo = 15% CPU continuo
> - Sobrecarga = Sistema vulnerable
> 
> Esto simula cómo funcionan los recursos reales de un sistema informático."

#### 🛡️ Mecánica de Parry:
> "El escudo tiene una mecánica de **Parry**: si lo activas justo cuando un proyectil te alcanza, lo reflejas Y ventas calor instantáneamente. Alto riesgo, alta recompensa."

---

### PARTE 3: ARQUITECTURA TÉCNICA - UML (5 minutos)

#### 📊 Diagrama 1: MVC Pattern
**Archivo:** `1-mvc-pattern.puml`

> "Implementamos el patrón **Model-View-Controller** para separar responsabilidades:"

| Capa | Clase | Función |
|------|-------|---------|
| **Model** | `GameStateModel`, `PlayerModel` | Solo datos, sin lógica de UI |
| **View** | `GameHUD`, `QuizView` | Solo visualización |
| **Controller** | `GameManager` | Coordina Model y View |

**Frase clave:**
> "El GameManager actúa como controlador central, manipulando los modelos y actualizando las vistas. Esto nos permite cambiar la UI sin tocar la lógica del juego."

---

#### 📊 Diagrama 2: Component Pattern
**Archivo:** `2-component-pattern.puml`

> "Usamos **Composición sobre Herencia**. En vez de una clase Player monolítica, el jugador está compuesto de componentes independientes:"

```
Player
  ├── HealthComponent (salud)
  ├── MovementComponent (movimiento)
  ├── WeaponComponent (armas)
  └── ShieldComponent (escudo)
```

**Por qué es importante:**
> "Cada componente es reutilizable. Si queremos que un enemigo también tenga escudo, simplemente le agregamos `ShieldComponent`. No hay código duplicado."

---

#### 📊 Diagrama 3: Strategy Pattern
**Archivo:** `3-strategy-pattern.puml`

> "Las armas y la IA de enemigos usan el patrón **Strategy**, que permite intercambiar algoritmos en tiempo de ejecución."

**Armas (IWeapon):**
| Arma | Comportamiento |
|------|---------------|
| `FirewallWeapon` | Disparo básico, bajo costo |
| `AntivirusWeapon` | Ráfaga de 3 disparos |
| `EncryptionWeapon` | Alto daño, alto costo |
| `HoneypotWeapon` | Trampa que atrae enemigos |

**IA de Enemigos:**
| IA | Comportamiento |
|----|---------------|
| `ChaseAI` | Persigue directo |
| `ZigZagAI` | Movimiento evasivo |
| `CircleAI` | Rodea al jugador |

**Frase clave:**
> "Agregar una nueva arma es tan simple como crear una clase que implemente `IWeapon`. No tocamos código existente. Esto es el **Open/Closed Principle** en acción."

---

#### 📊 Diagrama 4: Observer + Factory
**Archivo:** `4-observer-factory.puml`

> "Usamos **Observer Pattern** a través de un EventBus para comunicación desacoplada:"

**Eventos del sistema:**
- `EnemyDefeated` → HUD actualiza score
- `PlayerDamaged` → HUD actualiza vida
- `QuizQuestionShown` → QuizView muestra pregunta
- `SecurityTipShown` → Tip educativo aparece

> "Y el **Factory Pattern** para crear enemigos:"

```csharp
EnemyFactory.CreateEnemy(EnemyType.Phishing, position);
EnemyFactory.CreateEnemy(EnemyType.Ransomware, position);
```

**Frase clave:**
> "El EventBus permite que el HUD no conozca al Player directamente. Solo escucha eventos. Si mañana cambiamos cómo funciona el daño, el HUD no se entera."

---

#### 📊 Diagrama 5: Arquitectura Completa
**Archivo:** `5-complete-architecture.puml`

> "Este diagrama muestra cómo se integra todo:"

**Capas del sistema:**
1. 🎮 **Core Layer** - GameManager, EventBus, Interfaces
2. 📊 **Data Layer** - Models (GameState, Player)
3. 🎨 **Presentation Layer** - Views (HUD, Quiz)
4. 🔧 **Component Layer** - Health, Movement, Weapon, Shield
5. ⚔️ **Weapons** - Strategy implementations
6. 👾 **Entities** - Player, Enemies, Projectiles
7. 📚 **Education** - QuizSystem, SecurityTipsSystem

---

### PARTE 4: IMPACTO EDUCATIVO (2 minutos)

#### 🎓 Sistema de Aprendizaje:

> "Code Rippier integra educación en el gameplay:"

1. **Quiz System:** Preguntas contextuales al derrotar enemigos
2. **Security Tips:** Consejos que aparecen según la amenaza
3. **Lore Terminals:** Historia y contexto de ciberseguridad

**Ejemplo práctico:**
> "Si derrotas un enemigo tipo Phishing, aparece una pregunta sobre cómo identificar correos fraudulentos. Si la respondes bien, obtienes bonus de puntos."

#### 📈 Objetivos de Aprendizaje:
- Identificar tipos de malware
- Comprender cómo funcionan los ataques
- Aplicar mejores prácticas de seguridad
- Tomar decisiones bajo presión (gestión de recursos)

---

### PARTE 5: DEMO EN VIVO (2-3 minutos)

#### 🎮 Secuencia de Demo:
1. **Menú Principal** → Mostrar UI profesional
2. **Tutorial** → Explicar controles
3. **Gameplay** → Mostrar:
   - Sistema de CPU/Flux en acción
   - Diferentes armas
   - Tipos de enemigos
   - Quiz educativo
   - Mecánica de Parry (si es posible)

#### ⚠️ Puntos a destacar durante la demo:
- "Observen cómo el medidor de CPU sube con cada disparo"
- "Este enemigo es Phishing, se mueve rápido y en grupo"
- "Aquí aparece la pregunta educativa"

---

### PARTE 6: CONCLUSIÓN (1 minuto)

#### 🏆 Resumen de Puntos Fuertes:

| Aspecto | Implementación |
|---------|---------------|
| **Innovación** | Sistema CPU único, educación gamificada |
| **Arquitectura** | 6 patrones de diseño, principios SOLID |
| **Educación** | Quiz, tips, terminales informativas |
| **Código** | Open source, documentado, limpio |

#### 🎯 Frase de Cierre:
> "Code Rippier demuestra que los videojuegos pueden ser herramientas educativas poderosas. Gracias a una arquitectura sólida y un diseño centrado en el aprendizaje, creamos una experiencia que entretiene mientras enseña conceptos críticos de ciberseguridad."

#### 🙋 Apertura a Preguntas:
> "¿Tienen alguna pregunta sobre la arquitectura, el gameplay o el proceso de desarrollo?"

---

## 📎 RECURSOS ADICIONALES

### 🔗 Links Útiles:
- **Website:** https://axy-sys.github.io/nave/
- **GitHub:** https://github.com/Axy-sys/nave
- **Documentación:** `Docs/` en el repositorio

### 📊 Herramientas para Visualizar UML:

1. **PlantUML Online:** https://www.plantuml.com/plantuml/uml/
   - Copia el contenido de los archivos `.puml`
   - Genera imágenes PNG/SVG al instante

2. **VS Code Extension:** "PlantUML" de jebbs
   - Preview en tiempo real: `Alt + D`
   - Exportar a PNG/SVG

3. **Kroki.io:** https://kroki.io/
   - Soporta múltiples formatos de diagramas

---

## 🎨 CHEAT SHEET - PATRONES DE DISEÑO

### Para explicar rápidamente:

| Patrón | Analogía Simple | En Code Rippier |
|--------|-----------------|-----------------|
| **MVC** | "Chef (Controller), Receta (Model), Plato servido (View)" | GameManager controla todo |
| **Component** | "LEGO blocks que se combinan" | Player = Health + Movement + Weapon + Shield |
| **Strategy** | "Diferentes herramientas para el mismo trabajo" | Armas intercambiables |
| **Observer** | "Suscripción a notificaciones" | EventBus notifica cambios |
| **Factory** | "Fábrica que produce productos" | EnemyFactory crea enemigos |
| **Singleton** | "Solo puede haber uno" | GameManager.Instance |

---

## ⏱️ TIMING SUGERIDO

| Sección | Tiempo | Acumulado |
|---------|--------|-----------|
| Introducción | 2 min | 2 min |
| Concepto del Juego | 2 min | 4 min |
| Arquitectura UML | 5 min | 9 min |
| Impacto Educativo | 2 min | 11 min |
| Demo en Vivo | 3 min | 14 min |
| Conclusión + Q&A | 1 min | 15 min |

---

## 💡 TIPS PARA LA EXPOSICIÓN

1. **Practicar la demo** antes para evitar bugs en vivo
2. **Tener backup** de capturas de pantalla por si falla algo
3. **Conocer los diagramas** para poder explicar sin leer
4. **Hacer contacto visual** con el público
5. **Mostrar entusiasmo** - si tú no estás emocionado, el público tampoco

---

*Documento creado para el equipo CodeRippier - Code Rippier Cybersecurity Defense Game*
