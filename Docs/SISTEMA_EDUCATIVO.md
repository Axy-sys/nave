# 📚 Sistema Educativo de CODE RIPPIER

## 🎯 Filosofía de Diseño UX

### El Problema Original
> "Los quizzes aparecían en momentos aleatorios, interrumpiendo el gameplay y frustrando al jugador."

### La Solución: Aprendizaje Contextual
Los quizzes ahora aparecen en **MOMENTOS EDUCATIVOS ÓPTIMOS**:
1. **Al morir** por un enemigo (máximo engagement emocional)
2. **Al completar oleada** (pausa natural)
3. **Al descubrir amenaza** (curiosidad activa)
4. **Después de N muertes** por el mismo enemigo (frustración = necesidad de aprender)

---

## 🎮 Flujo de Interacción del Usuario

```
┌─────────────────────────────────────────────────────────────────────┐
│                     EXPERIENCIA DEL JUGADOR                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│   1. DESCUBRIMIENTO                                                  │
│   ┌─────────────────┐                                               │
│   │ Jugador derrota │──▶ 🔍 "¡Nueva amenaza descubierta!"           │
│   │ nuevo enemigo   │    └── Amenaza añadida a Enciclopedia         │
│   └─────────────────┘        Nivel de conocimiento = 1               │
│                                                                      │
│   2. APRENDIZAJE POR ERROR                                           │
│   ┌─────────────────┐                                               │
│   │ Jugador muere   │──▶ Primera vez: Info básica del enemigo       │
│   │ por enemigo X   │──▶ Segunda vez: ❓ QUIZ CONTEXTUAL             │
│   └─────────────────┘    └── "Has sido derrotado por [Malware]"      │
│                              "¡Es hora de aprender a defenderte!"   │
│                                                                      │
│   3. PROGRESIÓN DE CONOCIMIENTO                                      │
│   ┌─────────────────┐                                               │
│   │ Quiz correcto   │──▶ Nivel de conocimiento ↑                    │
│   └─────────────────┘    ├── Nivel 1: Descripción básica            │
│                          ├── Nivel 2: Cómo defenderse               │
│                          └── Nivel 3: Lore completo                 │
│                                                                      │
│   4. CONSULTA LIBRE (Tecla E)                                        │
│   ┌─────────────────┐                                               │
│   │ Enciclopedia    │──▶ Ver todas las amenazas descubiertas        │
│   │ de Amenazas     │    Ver progreso de conocimiento               │
│   └─────────────────┘    Repasar tips y debilidades                 │
│                                                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📁 Arquitectura del Sistema

### Archivos Creados

| Archivo | Propósito |
|---------|-----------|
| `Scripts/Education/ThreatEncyclopedia.cs` | Base de datos de amenazas estilo Pokédex |
| `Scripts/Education/ContextualLearningSystem.cs` | Timing inteligente de quizzes |
| `Scripts/Views/EncyclopediaView.cs` | UI de la Enciclopedia (acceso con E) |

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `Scripts/Views/QuizView.cs` | Añadido `ShowQuestionWithContext()` |
| `Scripts/MainScene.cs` | Integración de sistemas educativos |

---

## 🦠 Amenazas Disponibles (8 total)

| ID | Nombre | Icono | Categoría |
|----|--------|-------|-----------|
| `Malware` | MALWARE | 🦠 | Malware |
| `Phishing` | PHISHING | 🎣 | Social Engineering |
| `DDoS` | DDoS ATTACK | ⚡ | Network Attack |
| `SQLInjection` | SQL INJECTION | 💉 | Web Attack |
| `Ransomware` | RANSOMWARE | 🔐 | Malware |
| `BruteForce` | BRUTE FORCE | 🔨 | Authentication |
| `Worm` | WORM | 🐛 | Malware |
| `Trojan` | TROJAN | 🐴 | Malware |

### Niveles de Conocimiento

```
Nivel 0: ??? (No descubierto)
    └── Silueta gris en la lista

Nivel 1: Básico (●○○)
    └── Descripción corta
    └── "Responde quizzes para desbloquear más información"

Nivel 2: Intermedio (●●○)
    └── Descripción completa
    └── Cómo defenderse (gameplay + vida real)

Nivel 3: Experto (●●●)
    └── Todo lo anterior
    └── Lore profundo / datos históricos
    └── ⭐ "DOMINADO"
```

---

## ⌨️ Controles del Sistema Educativo

| Tecla | Acción |
|-------|--------|
| `E` | Abrir/Cerrar Enciclopedia de Amenazas |
| `ESC` | Cerrar Enciclopedia (si está abierta) |
| `1-4` | Seleccionar respuesta en Quiz |

---

## 🎨 Diseño Visual

### Enciclopedia UI
- **Fondo:** Negro con borde púrpura (RIPPIER_PURPLE)
- **Lista izquierda:** Amenazas con indicadores de nivel (●●○)
- **Panel derecho:** Detalle de amenaza seleccionada
- **Barra de progreso:** Conocimiento de la amenaza
- **Animación:** Entrada con scale bounce

### Quiz Contextual
- **Header personalizado:** Contexto de por qué apareció el quiz
- **Ejemplos:**
  - `💀 HAS SIDO DERROTADO POR 🦠 MALWARE`
  - `📚 REPASO DE SEGURIDAD: 🎣 PHISHING`

---

## 🔄 Integración con Otros Sistemas

```
                    ┌─────────────────────┐
                    │   GameEventBus      │
                    └──────────┬──────────┘
                               │
           ┌───────────────────┼───────────────────┐
           │                   │                   │
           ▼                   ▼                   ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ThreatEncyclopedia│ │ContextualLearning│ │AdaptiveDifficulty│
├──────────────────┤ ├──────────────────┤ ├──────────────────┤
│OnEnemyDefeated   │ │OnPlayerDied      │ │OnPlayerDied      │
│OnPlayerDamaged   │ │OnPlayerDamaged   │ │OnWaveCompleted   │
│OnQuestionAnswered│ │OnWaveCompleted   │ │OnEnemyDefeated   │
└──────────────────┘ └──────────────────┘ └──────────────────┘
           │                   │                   │
           └───────────────────┼───────────────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │      QuizView       │
                    │  EncyclopediaView   │
                    └─────────────────────┘
```

---

## 📊 Métricas de Aprendizaje

El sistema trackea:
- **Amenazas descubiertas:** X/8
- **Amenazas dominadas:** X/8 (nivel 3)
- **Quizzes correctos:** X
- **Eficiencia de aprendizaje:** (correctos/total) × 100%
- **Muertes por tipo de enemigo:** Para personalizar quizzes

---

## 🎯 Inspiración de Diseño

| Juego | Concepto Adaptado |
|-------|-------------------|
| **Pokémon** | Pokédex - Descubrir y completar colección |
| **Mass Effect** | Codex - Lore profundo y organizado |
| **Witcher 3** | Bestiary - Debilidades de enemigos |
| **Dark Souls** | "You Died" - Reflexión tras fallo |
| **Duolingo** | Timing óptimo - Momentos de máxima retención |

---

## ✅ Checklist de UX

- [x] Quizzes NO interrumpen gameplay activo
- [x] Quizzes aparecen tras pausas naturales (muerte, oleada)
- [x] Primera muerte = Info, segunda = Quiz
- [x] Enciclopedia accesible en cualquier momento (E)
- [x] Progresión visual clara (●●○)
- [x] Feedback positivo por aprendizaje
- [x] Tips contextuales relacionados con la amenaza actual
- [x] No hay spam de quizzes (cooldowns implementados)
