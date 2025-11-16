# Guía de Patrones de Diseño Implementados

## 📐 Patrón MVC (Model-View-Controller)

### Model (Modelos de Datos)
```csharp
// GameStateModel.cs - Almacena estado del juego
public class GameStateModel
{
    public int CurrentWave { get; set; }
    public Dictionary<string, int> EnemiesDefeatedByType { get; }
    // Solo datos, sin lógica de presentación ni control
}

// PlayerModel.cs - Almacena datos del jugador
public class PlayerModel
{
    public float CurrentHealth { get; private set; }
    public int Lives { get; private set; }
    // Encapsula datos del jugador
}
```

### View (Vistas UI)
```csharp
// GameHUD.cs - Visualización del HUD
public partial class GameHUD : CanvasLayer
{
    // Solo se encarga de MOSTRAR información
    // Escucha eventos y actualiza UI
    private void UpdateScore(int score)
    {
        _scoreLabel.Text = $"Score: {score}";
    }
}

// QuizView.cs - Visualización de preguntas
public partial class QuizView : CanvasLayer
{
    public void ShowQuestion(QuizQuestion question)
    {
        // Muestra la pregunta, no contiene lógica de negocio
    }
}
```

### Controller (Controladores)
```csharp
// GameManager.cs - Coordina el flujo del juego
public partial class GameManager : Node
{
    // Maneja lógica de negocio
    // Coordina entre Model y View
    public void StartGame()
    {
        _gameState = new GameStateModel();
        GameEventBus.Instance.EmitLevelStarted(CurrentLevel);
    }
}
```

**Beneficios**:
- ✅ Separación clara de responsabilidades
- ✅ Fácil testeo de cada capa
- ✅ Cambios UI no afectan lógica de negocio

---

## 🏭 Factory Pattern

### Implementación: EnemyFactory
```csharp
public static class EnemyFactory
{
    // Método factory que crea enemigos
    public static Node2D CreateEnemy(EnemyType type, Vector2 position)
    {
        var enemy = CreateEnemyInstance(type);
        ConfigureEnemy(enemy, type);
        return enemy;
    }
    
    private static EnemyStats GetEnemyStats(EnemyType type)
    {
        return type switch
        {
            EnemyType.Malware => new EnemyStats(30, 200, 10, ...),
            EnemyType.Phishing => new EnemyStats(20, 250, 15, ...),
            // Cada tipo tiene sus stats
        };
    }
}
```

**Uso**:
```csharp
// Crear enemigo sin saber detalles de construcción
var enemy = EnemyFactory.CreateEnemy(EnemyType.Malware, spawnPos);
```

**Beneficios**:
- ✅ Centraliza creación de objetos complejos
- ✅ Fácil agregar nuevos tipos
- ✅ Encapsula lógica de construcción

---

## 🎯 Strategy Pattern

### Implementación: Sistema de Armas
```csharp
// Interface común
public interface IWeapon
{
    void Fire(Vector2 position, Vector2 direction);
    bool CanFire();
    string GetWeaponName();
}

// Estrategias concretas
public partial class FirewallWeapon : BaseWeapon
{
    public override void Fire(Vector2 position, Vector2 direction)
    {
        // Comportamiento específico de Firewall
        SpawnProjectile(position, direction, DamageType.Physical);
    }
}

public partial class AntivirusWeapon : BaseWeapon
{
    public override void Fire(Vector2 position, Vector2 direction)
    {
        // Comportamiento específico: ráfaga de 3
        for (int i = 0; i < 3; i++)
        {
            SpawnProjectile(position, direction, DamageType.Malware);
        }
    }
}
```

**Uso dinámico**:
```csharp
// WeaponComponent puede cambiar estrategia en runtime
public void SetWeapon(IWeapon weapon)
{
    _currentWeapon = weapon;
}

// Cambio dinámico
weaponComponent.SetWeapon(new AntivirusWeapon());
```

### Implementación: AI de Enemigos
```csharp
// Estrategia base
public abstract partial class BaseEnemyAI : Node
{
    protected abstract void UpdateAI(double delta);
}

// Diferentes estrategias de comportamiento
public partial class MalwareAI : BaseEnemyAI
{
    protected override void UpdateAI(double delta)
    {
        // Movimiento en zigzag
    }
}

public partial class PhishingAI : BaseEnemyAI
{
    protected override void UpdateAI(double delta)
    {
        // Acercamiento sigiloso, luego ataque rápido
    }
}
```

**Beneficios**:
- ✅ Intercambio de comportamientos en runtime
- ✅ Elimina condicionales complejos
- ✅ Fácil agregar nuevas estrategias

---

## 👀 Observer Pattern

### Implementación: EventBus
```csharp
public partial class GameEventBus : Node
{
    // Eventos observables
    public event Action<float> OnPlayerHealthChanged;
    public event Action<int> OnScoreChanged;
    public event Action<string> OnSecurityTipShown;
    
    // Métodos para emitir eventos
    public void EmitPlayerHealthChanged(float health)
    {
        OnPlayerHealthChanged?.Invoke(health);
    }
}
```

**Suscriptores**:
```csharp
// GameHUD suscribe a eventos
public override void _Ready()
{
    GameEventBus.Instance.OnScoreChanged += UpdateScore;
    GameEventBus.Instance.OnPlayerHealthChanged += UpdateHealth;
}

private void UpdateScore(int score)
{
    _scoreLabel.Text = $"Score: {score}";
}
```

**Emisores**:
```csharp
// HealthComponent emite cuando cambia salud
public void TakeDamage(float amount, DamageType damageType)
{
    _currentHealth -= amount;
    
    if (IsPlayer)
    {
        GameEventBus.Instance.EmitPlayerHealthChanged(_currentHealth);
    }
}
```

**Beneficios**:
- ✅ Desacoplamiento total entre componentes
- ✅ Múltiples observadores sin modificar emisor
- ✅ Comunicación sin referencias directas

---

## 🧩 Component Pattern

### Implementación: Sistema de Componentes
```csharp
// Interface base
public interface IComponent
{
    void Initialize(Node owner);
    void UpdateComponent(double delta);
    void Cleanup();
    bool IsActive { get; set; }
}

// Componente base abstracto
public abstract partial class BaseComponent : Node, IComponent
{
    protected Node _owner;
    
    public virtual void Initialize(Node owner)
    {
        _owner = owner;
        OnInitialize();
    }
    
    protected abstract void OnInitialize();
    protected abstract void OnUpdate(double delta);
}

// Componentes específicos
public partial class HealthComponent : BaseComponent, IDamageable
{
    // Solo maneja salud
}

public partial class MovementComponent : BaseComponent, IMovable
{
    // Solo maneja movimiento
}
```

### Composición en Entidades
```csharp
public partial class Player : CharacterBody2D
{
    // Composición: el jugador TIENE componentes
    private HealthComponent _healthComponent;
    private MovementComponent _movementComponent;
    private WeaponComponent _weaponComponent;
    private ShieldComponent _shieldComponent;
    
    private void InitializeComponents()
    {
        _healthComponent = new HealthComponent();
        AddChild(_healthComponent);
        _healthComponent.Initialize(this);
        
        _movementComponent = new MovementComponent();
        AddChild(_movementComponent);
        _movementComponent.Initialize(this);
        
        // Más componentes...
    }
}
```

**Beneficios**:
- ✅ Reutilización de código
- ✅ Flexibilidad: añadir/quitar componentes
- ✅ Evita jerarquías de herencia profundas
- ✅ Single Responsibility por componente

---

## 🔒 Singleton Pattern

### Implementación
```csharp
public partial class GameManager : Node
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    public override void _Ready()
    {
        // Asegurar única instancia
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }
        _instance = this;
    }
}
```

**Uso global**:
```csharp
// Acceso desde cualquier lugar
GameManager.Instance.AddScore(100);
QuizSystem.Instance.GetNextQuestion();
```

**Beneficios**:
- ✅ Acceso global controlado
- ✅ Única instancia garantizada
- ⚠️ Usar con moderación (puede crear acoplamiento)

---

## 📋 Template Method Pattern

### Implementación: BaseComponent
```csharp
public abstract partial class BaseComponent : Node, IComponent
{
    // Método template que define el flujo
    public virtual void Initialize(Node owner)
    {
        _owner = owner;
        OnInitialize();  // Hook method
    }
    
    public virtual void UpdateComponent(double delta)
    {
        if (!IsActive) return;
        OnUpdate(delta);  // Hook method
    }
    
    // Métodos abstractos para subclases
    protected abstract void OnInitialize();
    protected abstract void OnUpdate(double delta);
    protected abstract void OnCleanup();
}
```

**Subclases implementan pasos específicos**:
```csharp
public partial class HealthComponent : BaseComponent
{
    protected override void OnInitialize()
    {
        _currentHealth = MaxHealth;
        _isAlive = true;
    }
    
    protected override void OnUpdate(double delta)
    {
        // Lógica específica de health
    }
}
```

**Beneficios**:
- ✅ Define estructura común
- ✅ Subclases personalizan pasos específicos
- ✅ Reduce duplicación de código

---

## 🎓 Principios SOLID en Acción

### 1. Single Responsibility
```csharp
// ❌ MAL: Una clase hace demasiado
public class Player
{
    void Move() { }
    void TakeDamage() { }
    void Fire() { }
    void DrawHealthBar() { }  // ¡Mezcla lógica con UI!
}

// ✅ BIEN: Cada clase/componente una responsabilidad
public class Player
{
    private MovementComponent _movement;  // Solo movimiento
    private HealthComponent _health;      // Solo salud
    private WeaponComponent _weapon;      // Solo armas
}
```

### 2. Open/Closed
```csharp
// ✅ Abierto a extensión, cerrado a modificación
public abstract class BaseWeapon : IWeapon
{
    // Código base no se modifica
}

// Extender agregando nuevas clases
public class LaserWeapon : BaseWeapon
{
    // Nueva arma sin tocar código existente
}
```

### 3. Liskov Substitution
```csharp
// Cualquier IWeapon es intercambiable
IWeapon weapon = new FirewallWeapon();
weapon.Fire(pos, dir);

weapon = new AntivirusWeapon();
weapon.Fire(pos, dir);  // Mismo interface, diferente comportamiento
```

### 4. Interface Segregation
```csharp
// ❌ MAL: Interface gordo
public interface IEntity
{
    void Move();
    void TakeDamage();
    void Fire();
    void Heal();
}

// ✅ BIEN: Interfaces específicas
public interface IMovable { void Move(); }
public interface IDamageable { void TakeDamage(); }
public interface IWeapon { void Fire(); }
```

### 5. Dependency Inversion
```csharp
// ✅ Depende de abstracción (IWeapon), no implementación concreta
public class WeaponComponent
{
    private IWeapon _currentWeapon;  // ← Interface, no clase concreta
    
    public void SetWeapon(IWeapon weapon)
    {
        _currentWeapon = weapon;
    }
}
```

---

## 💡 Composición vs Herencia

### ❌ Problema con Herencia
```csharp
public class Entity { }
public class MovableEntity : Entity { }
public class DamageableMovableEntity : MovableEntity { }
public class ShootingDamageableMovableEntity : DamageableMovableEntity { }
// Jerarquía rígida y difícil de mantener
```

### ✅ Solución con Composición
```csharp
public class Entity
{
    // Añade los componentes que necesites
    private List<IComponent> _components;
    
    public void AddComponent(IComponent component)
    {
        _components.Add(component);
    }
}

// Flexibilidad total
var player = new Entity();
player.AddComponent(new MovementComponent());
player.AddComponent(new HealthComponent());
player.AddComponent(new WeaponComponent());
```

---

## 📚 Resumen de Patrones

| Patrón | Propósito | Implementación en el Juego |
|--------|-----------|---------------------------|
| **MVC** | Separar responsabilidades | GameManager (C), Models (M), Views (V) |
| **Factory** | Crear objetos complejos | EnemyFactory, PowerUpFactory |
| **Strategy** | Intercambiar algoritmos | IWeapon, AI behaviors |
| **Observer** | Comunicación desacoplada | GameEventBus |
| **Component** | Composición flexible | Health, Movement, Weapon components |
| **Singleton** | Instancia única global | GameManager, Systems |
| **Template Method** | Definir esqueleto de algoritmo | BaseComponent, BaseWeapon |

---

**Este código es una demostración práctica de cómo aplicar patrones de diseño en un juego real.**
