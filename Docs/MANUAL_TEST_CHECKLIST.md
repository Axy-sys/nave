# 🎮 CHECKLIST DE PRUEBAS MANUALES - CODE RIPPIER

## 📋 Instrucciones
1. Abre Godot 4
2. Carga el proyecto `nave`
3. Ejecuta (F5) o Play
4. Marca cada prueba con ✅ o ❌

---

## 🚀 TEST 1: INICIO DEL JUEGO
| # | Prueba | Resultado |
|---|--------|-----------|
| 1.1 | El juego inicia sin errores | ⬜ |
| 1.2 | Aparece la intro cinematográfica | ⬜ |
| 1.3 | ESC o SPACE skipea la intro | ⬜ |
| 1.4 | El jugador aparece en pantalla | ⬜ |
| 1.5 | El HUD muestra Score, Vidas, Health | ⬜ |

---

## 🎯 TEST 2: CONTROLES BÁSICOS
| # | Prueba | Resultado |
|---|--------|-----------|
| 2.1 | WASD mueve al jugador | ⬜ |
| 2.2 | El jugador rota hacia el mouse | ⬜ |
| 2.3 | Click izquierdo dispara | ⬜ |
| 2.4 | SPACE hace dash (destello cyan) | ⬜ |
| 2.5 | SHIFT reduce velocidad (Focus Mode) | ⬜ |
| 2.6 | ESC pausa el juego | ⬜ |
| 2.7 | ESC de nuevo resume | ⬜ |

---

## 👾 TEST 3: OLEADAS Y ENEMIGOS
| # | Prueba | Resultado |
|---|--------|-----------|
| 3.1 | Wave 1 aparece tras ~2 segundos | ⬜ |
| 3.2 | Enemigos spawn desde arriba | ⬜ |
| 3.3 | Enemigos disparan balas | ⬜ |
| 3.4 | Matar enemigo da puntos | ⬜ |
| 3.5 | Al matar todos → Wave 2 | ⬜ |
| 3.6 | Wave 5 tiene mini-boss | ⬜ |

---

## 💀 TEST 4: MUERTE Y RESPAWN
| # | Prueba | Resultado |
|---|--------|-----------|
| 4.1 | Recibir daño reduce la barra de vida | ⬜ |
| 4.2 | Al morir, jugador explota (partículas) | ⬜ |
| 4.3 | Respawn en centro-abajo de pantalla | ⬜ |
| 4.4 | Jugador parpadea (invincibilidad) | ⬜ |
| 4.5 | Invincibilidad dura ~3.5 segundos | ⬜ |
| 4.6 | Vidas disminuyen en HUD | ⬜ |
| 4.7 | Balas se limpian al respawnear | ⬜ |

---

## ☠️ TEST 5: GAME OVER
| # | Prueba | Resultado |
|---|--------|-----------|
| 5.1 | Al perder todas las vidas → Game Over | ⬜ |
| 5.2 | Pantalla de Game Over aparece | ⬜ |
| 5.3 | Muestra puntuación final | ⬜ |
| 5.4 | R reinicia el juego | ⬜ |
| 5.5 | ESC vuelve al menú | ⬜ |

---

## 📚 TEST 6: SISTEMA EDUCATIVO
| # | Prueba | Resultado |
|---|--------|-----------|
| 6.1 | **E** abre la Enciclopedia | ⬜ |
| 6.2 | Enciclopedia pausa el juego | ⬜ |
| 6.3 | Lista de amenazas visible | ⬜ |
| 6.4 | Click en amenaza muestra detalles | ⬜ |
| 6.5 | **ESC** o **X** cierra enciclopedia | ⬜ |
| 6.6 | Al matar enemigo → "Nueva amenaza descubierta!" | ⬜ |

---

## ❓ TEST 7: QUIZZES CONTEXTUALES
| # | Prueba | Resultado |
|---|--------|-----------|
| 7.1 | Morir 1 vez por Malware → Info aparece | ⬜ |
| 7.2 | Morir 2 veces por mismo enemigo → Quiz | ⬜ |
| 7.3 | Quiz muestra contexto personalizado | ⬜ |
| 7.4 | Teclas 1-4 seleccionan respuesta | ⬜ |
| 7.5 | Respuesta correcta → "+500 puntos" | ⬜ |
| 7.6 | Explicación aparece tras responder | ⬜ |

---

## 🛡️ TEST 8: DIFICULTAD ADAPTATIVA
| # | Prueba | Resultado |
|---|--------|-----------|
| 8.1 | **TAB** activa Encryption Burst | ⬜ |
| 8.2 | Burst limpia todas las balas | ⬜ |
| 8.3 | Contador de bursts en HUD (3 max) | ⬜ |
| 8.4 | Tras morir mucho, Firewall Mode se activa | ⬜ |
| 8.5 | Firewall reduce daño recibido | ⬜ |

---

## 🎨 TEST 9: EFECTOS VISUALES
| # | Prueba | Resultado |
|---|--------|-----------|
| 9.1 | Screen shake al recibir daño | ⬜ |
| 9.2 | Flash rojo al recibir daño | ⬜ |
| 9.3 | Explosiones al matar enemigos | ⬜ |
| 9.4 | Partículas de grazing (rozar balas) | ⬜ |
| 9.5 | Scanlines de terminal en pantalla | ⬜ |

---

## 📊 TEST 10: PUNTUACIÓN
| # | Prueba | Resultado |
|---|--------|-----------|
| 10.1 | Score aumenta al matar | ⬜ |
| 10.2 | Combo multiplier visible | ⬜ |
| 10.3 | Grazing da puntos | ⬜ |
| 10.4 | High Score se guarda | ⬜ |
| 10.5 | "NEW RECORD" si superas high score | ⬜ |

---

## 🔧 BUGS ENCONTRADOS

| # | Descripción | Severidad | Pasos para reproducir |
|---|-------------|-----------|----------------------|
| | | | |
| | | | |
| | | | |

---

## 📝 NOTAS ADICIONALES

```
Escribe aquí cualquier observación:



```

---

## ✅ RESUMEN

| Categoría | Pasaron | Fallaron |
|-----------|---------|----------|
| Inicio | /5 | |
| Controles | /7 | |
| Oleadas | /6 | |
| Muerte/Respawn | /7 | |
| Game Over | /5 | |
| Educativo | /6 | |
| Quizzes | /6 | |
| Dificultad | /5 | |
| Efectos | /5 | |
| Puntuación | /5 | |
| **TOTAL** | **/57** | |

---

**Tester:** _________________  
**Fecha:** _________________  
**Versión:** Build 25-Nov-2025
