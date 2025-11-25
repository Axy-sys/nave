using Godot;
using System;
using System.Collections.Generic;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de Bullet Hell - Patrones de balas estilo Touhou/Ikaruga
    /// 
    /// MECÁNICAS BULLET HELL:
    /// - Patrones de balas hermosos y memorables
    /// - Escalado de dificultad infinito
    /// - Grazing (rozar balas sin morir = puntos)
    /// - Densidad de balas progresiva
    /// 
    /// PATRONES DISPONIBLES:
    /// - Radial: Disparos en círculo
    /// - Spiral: Espirales de balas
    /// - Aimed: Dirigido al jugador
    /// - Wave: Ondas sinusoidales
    /// - Burst: Ráfagas rápidas
    /// - Ring: Anillos expandibles
    /// </summary>
    public partial class BulletHellSystem : Node
    {
        private static BulletHellSystem _instance;
        public static BulletHellSystem Instance => _instance;

        // Pool de balas para rendimiento
        private List<EnemyBullet> _bulletPool = new List<EnemyBullet>();
        private const int INITIAL_POOL_SIZE = 500;
        private const int MAX_BULLETS = 2000;
        
        // Contenedor de balas activas
        private Node2D _bulletContainer;
        private int _activeBullets = 0;

        // Colores de balas por tipo de enemigo
        private static readonly Color MALWARE_BULLET = new Color("#ff5555");
        private static readonly Color PHISHING_BULLET = new Color("#ffaa00");
        private static readonly Color DDOS_BULLET = new Color("#bf00ff");
        private static readonly Color SQL_BULLET = new Color("#00d4ff");
        private static readonly Color RANSOMWARE_BULLET = new Color("#ff0000");
        private static readonly Color DEFAULT_BULLET = new Color("#00ff41");

        // Dificultad escalante
        private float _difficultyMultiplier = 1.0f;
        private int _currentWave = 0;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;

            InitializeBulletPool();
        }

        private void InitializeBulletPool()
        {
            _bulletContainer = new Node2D();
            _bulletContainer.Name = "EnemyBullets";
            _bulletContainer.ZIndex = 50;
            
            CallDeferred(nameof(AddBulletContainer));

            // Pre-crear balas para el pool
            for (int i = 0; i < INITIAL_POOL_SIZE; i++)
            {
                var bullet = CreateBullet();
                bullet.Deactivate();
                _bulletPool.Add(bullet);
            }

            GD.Print($"[BulletHell] Pool inicializado con {INITIAL_POOL_SIZE} balas");
        }

        private void AddBulletContainer()
        {
            var main = GetTree().Root.GetNodeOrNull("Main");
            if (main != null)
            {
                main.AddChild(_bulletContainer);
            }
        }

        public override void _Process(double delta)
        {
            // Actualizar todas las balas activas
            var viewport = GetViewport().GetVisibleRect();
            var toDeactivate = new List<EnemyBullet>();

            foreach (var bullet in _bulletPool)
            {
                if (bullet.IsActive)
                {
                    bullet.UpdateBullet((float)delta);
                    
                    // Desactivar si sale de pantalla
                    if (!viewport.HasPoint(bullet.GlobalPosition))
                    {
                        toDeactivate.Add(bullet);
                    }
                }
            }

            foreach (var bullet in toDeactivate)
            {
                bullet.Deactivate();
                _activeBullets--;
            }
        }

        /// <summary>
        /// Actualiza el multiplicador de dificultad según la oleada
        /// </summary>
        public void SetWaveDifficulty(int wave)
        {
            _currentWave = wave;
            // Dificultad escala: 1.0 → 1.5 → 2.0 → 2.5... (cada 5 oleadas +0.5)
            _difficultyMultiplier = 1.0f + (wave / 5) * 0.5f;
            
            // ADAPTIVE DIFFICULTY: Modificar según Threat Level
            if (AdaptiveDifficultySystem.Instance != null)
            {
                float threatMod = AdaptiveDifficultySystem.Instance.GetThreatMultiplier();
                _difficultyMultiplier *= threatMod;
            }
            
            // Velocidad de balas también escala
            GD.Print($"[BulletHell] Wave {wave}, Dificultad: x{_difficultyMultiplier:F1}");
        }

        // ═══════════════════════════════════════════════════════════════════
        // PATRONES DE DISPARO
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Patrón RADIAL: Dispara en todas direcciones desde un punto
        /// </summary>
        public void FireRadialPattern(Vector2 origin, int bulletCount, float speed, Color color, float angleOffset = 0f)
        {
            float angleStep = Mathf.Tau / bulletCount;
            
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = angleStep * i + angleOffset;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnBullet(origin, direction, speed * _difficultyMultiplier, color);
            }
        }

        /// <summary>
        /// Patrón SPIRAL: Espiral de balas en rotación
        /// </summary>
        public void FireSpiralPattern(Vector2 origin, int arms, float speed, Color color, float rotation)
        {
            float angleStep = Mathf.Tau / arms;
            
            for (int i = 0; i < arms; i++)
            {
                float angle = angleStep * i + rotation;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnBullet(origin, direction, speed * _difficultyMultiplier, color);
            }
        }

        /// <summary>
        /// Patrón AIMED: Disparo dirigido al jugador
        /// </summary>
        public void FireAimedPattern(Vector2 origin, Vector2 targetPos, int bulletCount, float spread, float speed, Color color)
        {
            Vector2 baseDirection = (targetPos - origin).Normalized();
            float baseAngle = Mathf.Atan2(baseDirection.Y, baseDirection.X);
            
            float spreadRad = Mathf.DegToRad(spread);
            float angleStep = bulletCount > 1 ? spreadRad / (bulletCount - 1) : 0;
            float startAngle = baseAngle - spreadRad / 2;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnBullet(origin, direction, speed * _difficultyMultiplier, color);
            }
        }

        /// <summary>
        /// Patrón WAVE: Onda sinusoidal de balas
        /// </summary>
        public void FireWavePattern(Vector2 origin, int bulletCount, float speed, Color color, float waveAmplitude, float waveFrequency)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                float t = (float)i / bulletCount;
                float x = -1 + t * 2; // -1 a 1
                float y = Mathf.Sin(x * waveFrequency) * waveAmplitude;
                Vector2 direction = new Vector2(x, 0.5f + y * 0.3f).Normalized();
                SpawnBullet(origin, direction, speed * _difficultyMultiplier, color);
            }
        }

        /// <summary>
        /// Patrón BURST: Ráfaga rápida en una dirección
        /// </summary>
        public void FireBurstPattern(Vector2 origin, Vector2 direction, int bulletCount, float speed, Color color, float delayBetween = 0.05f)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                float delay = i * delayBetween;
                var timer = GetTree().CreateTimer(delay);
                int index = i;
                timer.Timeout += () => {
                    float speedVariation = speed + index * 20; // Cada bala más rápida
                    SpawnBullet(origin, direction, speedVariation * _difficultyMultiplier, color);
                };
            }
        }

        /// <summary>
        /// Patrón RING: Anillo que se expande
        /// </summary>
        public void FireRingPattern(Vector2 origin, int bulletCount, float speed, Color color)
        {
            FireRadialPattern(origin, bulletCount, speed, color);
        }

        /// <summary>
        /// Patrón CROSS: Cruz de balas
        /// </summary>
        public void FireCrossPattern(Vector2 origin, float speed, Color color, float rotation = 0f)
        {
            Vector2[] directions = new Vector2[]
            {
                new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation)),
                new Vector2(Mathf.Cos(rotation + Mathf.Pi/2), Mathf.Sin(rotation + Mathf.Pi/2)),
                new Vector2(Mathf.Cos(rotation + Mathf.Pi), Mathf.Sin(rotation + Mathf.Pi)),
                new Vector2(Mathf.Cos(rotation + 3*Mathf.Pi/2), Mathf.Sin(rotation + 3*Mathf.Pi/2))
            };

            foreach (var dir in directions)
            {
                SpawnBullet(origin, dir, speed * _difficultyMultiplier, color);
            }
        }

        /// <summary>
        /// Patrón RANDOM: Dispersión aleatoria
        /// </summary>
        public void FireRandomPattern(Vector2 origin, int bulletCount, float speed, Color color)
        {
            var rng = new Random();
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = (float)rng.NextDouble() * Mathf.Tau;
                float speedVar = speed * (0.8f + (float)rng.NextDouble() * 0.4f);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnBullet(origin, direction, speedVar * _difficultyMultiplier, color);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // GESTIÓN DE BALAS
        // ═══════════════════════════════════════════════════════════════════

        private EnemyBullet GetBulletFromPool()
        {
            // Buscar bala inactiva
            foreach (var bullet in _bulletPool)
            {
                if (!bullet.IsActive)
                {
                    return bullet;
                }
            }

            // Si no hay disponibles y no excedemos el máximo, crear nueva
            if (_bulletPool.Count < MAX_BULLETS)
            {
                var newBullet = CreateBullet();
                _bulletPool.Add(newBullet);
                return newBullet;
            }

            // Pool lleno, no crear más
            return null;
        }

        private EnemyBullet CreateBullet()
        {
            var bullet = new EnemyBullet();
            if (_bulletContainer != null && IsInstanceValid(_bulletContainer))
            {
                _bulletContainer.CallDeferred("add_child", bullet);
            }
            return bullet;
        }

        private void SpawnBullet(Vector2 position, Vector2 direction, float speed, Color color)
        {
            var bullet = GetBulletFromPool();
            if (bullet != null)
            {
                bullet.Activate(position, direction, speed, color);
                _activeBullets++;
            }
        }

        /// <summary>
        /// Obtiene el color de bala según el tipo de enemigo
        /// </summary>
        public Color GetBulletColorForEnemy(string enemyType)
        {
            return enemyType switch
            {
                "Malware" => MALWARE_BULLET,
                "Phishing" => PHISHING_BULLET,
                "DDoS" => DDOS_BULLET,
                "SQLInjection" => SQL_BULLET,
                "Ransomware" => RANSOMWARE_BULLET,
                "BruteForce" => new Color("#ff8800"),
                "Worm" => new Color("#88ff00"),
                "Trojan" => new Color("#ff00ff"),
                "MiniBoss" => new Color("#ff4444"),
                "Boss" => new Color("#ffffff"),
                _ => DEFAULT_BULLET
            };
        }

        /// <summary>
        /// Limpia todas las balas (para transiciones o power-ups)
        /// </summary>
        public void ClearAllBullets()
        {
            foreach (var bullet in _bulletPool)
            {
                if (bullet.IsActive)
                {
                    bullet.Deactivate();
                }
            }
            _activeBullets = 0;
            GD.Print("[BulletHell] Todas las balas limpiadas");
        }

        public int GetActiveBulletCount() => _activeBullets;
        public float GetDifficultyMultiplier() => _difficultyMultiplier;
    }

    /// <summary>
    /// Bala enemiga individual - Optimizada para pool
    /// </summary>
    public partial class EnemyBullet : Area2D
    {
        public bool IsActive { get; private set; } = false;
        
        private Vector2 _direction;
        private float _speed;
        private Sprite2D _sprite;
        private CollisionShape2D _collision;

        public override void _Ready()
        {
            // Configurar colisión
            CollisionLayer = 8;  // Capa de balas enemigas
            CollisionMask = 1;   // Solo detecta jugador
            
            // Sprite simple (círculo)
            _sprite = new Sprite2D();
            _sprite.Texture = CreateBulletTexture();
            AddChild(_sprite);

            // Colisión pequeña (hitbox precisa)
            _collision = new CollisionShape2D();
            var shape = new CircleShape2D();
            shape.Radius = 4f; // Hitbox pequeña = bullet hell justo
            _collision.Shape = shape;
            AddChild(_collision);

            // Conectar señal de colisión
            BodyEntered += OnBodyEntered;

            Visible = false;
            SetProcess(false);
        }

        private Texture2D CreateBulletTexture()
        {
            // Crear textura de círculo simple
            var image = Image.CreateEmpty(16, 16, false, Image.Format.Rgba8);
            image.Fill(Colors.White);
            
            // Dibujar círculo
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    float dist = new Vector2(x - 8, y - 8).Length();
                    if (dist > 6) image.SetPixel(x, y, Colors.Transparent);
                    else if (dist > 4) image.SetPixel(x, y, new Color(1, 1, 1, 0.5f));
                }
            }
            
            return ImageTexture.CreateFromImage(image);
        }

        public void Activate(Vector2 position, Vector2 direction, float speed, Color color)
        {
            GlobalPosition = position;
            _direction = direction.Normalized();
            _speed = speed;
            _sprite.Modulate = color;
            IsActive = true;
            Visible = true;
            SetProcess(true);
            Monitoring = true;
        }

        public void Deactivate()
        {
            IsActive = false;
            Visible = false;
            SetProcess(false);
            Monitoring = false;
            GlobalPosition = new Vector2(-1000, -1000); // Fuera de pantalla
        }

        public void UpdateBullet(float delta)
        {
            if (!IsActive) return;
            GlobalPosition += _direction * _speed * delta;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (!IsActive) return;
            
            // Si golpea al jugador, causar daño y desactivar
            if (body.Name == "Player" || body.IsInGroup("Player"))
            {
                var healthComp = body.GetNodeOrNull<CyberSecurityGame.Components.HealthComponent>("HealthComponent");
                if (healthComp != null)
                {
                    // BALANCE: Daño base reducido de 10 a 5
                    // Multiplicador de dificultad con cap de 2.0x para evitar one-shots
                    float diffMult = BulletHellSystem.Instance?.GetDifficultyMultiplier() ?? 1f;
                    diffMult = Mathf.Min(diffMult, 2.0f); // Cap en 2x
                    
                    float damage = 5f * diffMult;
                    healthComp.TakeDamage(damage, CyberSecurityGame.Core.Interfaces.DamageType.Physical);
                }
                Deactivate();
            }
        }
    }
}
