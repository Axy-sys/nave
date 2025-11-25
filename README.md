<div align="center">

# Code Rippier

### Cybersecurity Defense Game

[![Godot 4](https://img.shields.io/badge/Godot-4.x-478cbf?style=for-the-badge&logo=godot-engine&logoColor=white)](https://godotengine.org/)
[![C#](https://img.shields.io/badge/C%23-.NET-239120?style=for-the-badge&logo=dotnet&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Status](https://img.shields.io/badge/Status-v1.0.0-success?style=for-the-badge)]()

**Un simulador táctico de defensa donde gestionas recursos del sistema mientras aprendes sobre amenazas reales de ciberseguridad.**

[🌐 Website](https://axy-sys.github.io/nave) · [📥 Descargar](https://github.com/Axy-sys/nave/releases) · [🐛 Reportar Bug](https://github.com/Axy-sys/nave/issues)

</div>

---

## 📖 Acerca del Proyecto

**Code Rippier** es un juego educativo de acción táctica que te pone en el rol de un operador de seguridad defendiendo sistemas críticos contra amenazas cibernéticas como Phishing, Ransomware y ataques DDoS.

### ¿Qué lo hace diferente?

- **Sistema de CPU (Flux):** No hay cooldowns tradicionales. Cada acción consume ciclos de procesamiento. Sobrecarga el sistema y quedarás expuesto.
- **Parry Táctico:** Activa el escudo en el momento preciso para reflejar proyectiles y ventilar calor instantáneamente.
- **Aprendizaje Integrado:** Quiz system contextual y tips de seguridad mientras juegas.

---

## ✨ Características

| Característica | Descripción |
|:---------------|:------------|
| ⚡ **Gestión de CPU** | Sistema de recursos único que reemplaza cooldowns tradicionales |
| 🛡️ **Parry Táctico** | Mecánica de alto riesgo/recompensa para defensa activa |
| 🎓 **Sistema Educativo** | Aprende ciberseguridad mientras juegas |
| 🔧 **Arquitectura SOLID** | Patrones MVC, Strategy, Observer, Factory |
| ♿ **Accesibilidad** | Tutorial interactivo y feedback visual redundante |
| 🌐 **Open Source** | Código abierto bajo licencia MIT |

---

## 🎮 Controles

| Acción | Tecla |
|:-------|:------|
| Movimiento | `WASD` / Flechas |
| Disparo | `Espacio` / Click Izquierdo |
| Escudo / Parry | `Shift` |
| Cambiar Arma | `1-4` / Rueda del Mouse |
| Pausa | `ESC` |

---

## 🛠️ Instalación

### Requisitos
- [Godot Engine 4.x](https://godotengine.org/) (Versión .NET)
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)

### Pasos

```bash
# Clonar el repositorio
git clone https://github.com/Axy-sys/nave.git

# Entrar al directorio
cd nave

# Restaurar dependencias (opcional)
dotnet restore

# Abrir con Godot Engine 4.x (.NET)
```

---

## 📂 Estructura del Proyecto

```
nave/
├── Assets/          # Sprites, texturas y recursos visuales
├── Docs/            # Documentación técnica
├── Scenes/          # Escenas de Godot (.tscn)
├── Scripts/         # Código fuente C#
│   ├── Components/  # Sistema de componentes
│   ├── Core/        # GameManager, EventBus
│   ├── Education/   # Quiz y tips de seguridad
│   ├── Entities/    # Player, Enemies, Projectiles
│   └── UI/          # Interfaces de usuario
└── website/         # Página web del proyecto
```

---

## 👥 Equipo CodeRippier

<table>
  <tr>
    <td align="center"><b>👑 Ricardo Orozco</b><br><sub>Líder del Proyecto<br>Diseñador de Juego</sub></td>
    <td align="center"><b>💻 David Gutierrez</b><br><sub>Main Developer<br>Diseñador de Juego</sub></td>
    <td align="center"><b>🎨 Juan Carlos Duran</b><br><sub>Diseñador Gráfico<br>Artista de Sprites</sub></td>
  </tr>
  <tr>
    <td align="center"><b>🗺️ Mateo Barrios</b><br><sub>Diseñador de Niveles<br>Tester</sub></td>
    <td align="center"><b>📝 Juan Morales</b><br><sub>Documentación<br>Analista de Requerimientos</sub></td>
    <td align="center"></td>
  </tr>
</table>

---

## 📚 Documentación

- [📖 Guía de Inicio Rápido](Docs/QUICKSTART.md)
- [🏗️ Patrones de Diseño](Docs/DESIGN_PATTERNS.md)
- [🎓 Sistema Educativo](Docs/SISTEMA_EDUCATIVO.md)
- [🎨 Mejoras UX/UI](Docs/MEJORAS_UX_UI.md)
- [⌨️ Configuración de Inputs](Docs/INPUT_SETUP.md)

---

## 📄 Licencia

Este proyecto está bajo la licencia **MIT**. Ver [LICENSE](LICENSE) para más detalles.

---

<div align="center">

**CodeRippier Team** © 2025

*"We rip the code to build the future."*

</div>

