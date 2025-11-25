using Godot;
using System;
using System.Collections.Generic;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de Grazing - Rozar balas sin morir = puntos extra
    /// 
    /// MECÁNICA BULLET HELL:
    /// - El jugador tiene un hitbox pequeño (colisión real)
    /// - Alrededor tiene un "graze box" más grande
    /// - Cuando una bala entra al graze box sin tocar el hitbox = GRAZE
    /// - Cada graze da puntos y puede cargar especiales
    /// 
    /// UX:
    /// - Retroalimentación visual: flash blanco al rozar
    /// - Sonido satisfactorio
    /// - Contador visible en pantalla
    /// - Incentiva el juego arriesgado = más diversión
    /// </summary>
    public partial class GrazingSystem : Node
    {
        private static GrazingSystem _instance;
        public static GrazingSystem Instance => _instance;

        // Configuración
        [Export] public int PointsPerGraze = 5;
        [Export] public float GrazeRadius = 30f; // Radio del área de graze
        [Export] public float GrazeCooldown = 0.1f; // Evita múltiples grazes de la misma bala

        // Estado
        private int _totalGrazeCount = 0;
        private int _sessionGrazeCount = 0;
        private float _grazeCooldownTimer = 0f;
        private HashSet<ulong> _grazedBullets = new HashSet<ulong>();

        // Referencias
        private Node2D _player;
        private Area2D _grazeArea;
        private Sprite2D _hitboxIndicator;

        // Efectos
        private float _grazeFlashTimer = 0f;
        private bool _isFlashing = false;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;

            // Esperar a que el jugador esté listo
            CallDeferred(nameof(InitializeGrazeSystem));
        }

        private void InitializeGrazeSystem()
        {
            _player = GetTree().Root.GetNodeOrNull<Node2D>("Main/Player");
            
            if (_player != null)
            {
                SetupGrazeArea();
                SetupHitboxIndicator();
                GD.Print("[Grazing] Sistema de grazing inicializado");
            }
            else
            {
                // Reintentar
                var timer = GetTree().CreateTimer(1.0f);
                timer.Timeout += () => CallDeferred(nameof(InitializeGrazeSystem));
            }
        }

        private void SetupGrazeArea()
        {
            // Crear área de graze alrededor del jugador
            _grazeArea = new Area2D();
            _grazeArea.Name = "GrazeArea";
            _grazeArea.CollisionLayer = 0;
            _grazeArea.CollisionMask = 8; // Detecta balas enemigas (layer 8)

            var shape = new CollisionShape2D();
            var circleShape = new CircleShape2D();
            circleShape.Radius = GrazeRadius;
            shape.Shape = circleShape;
            _grazeArea.AddChild(shape);

            // Conectar señales
            _grazeArea.AreaEntered += OnBulletEnteredGrazeZone;
            _grazeArea.AreaExited += OnBulletExitedGrazeZone;

            _player.AddChild(_grazeArea);
        }

        private void SetupHitboxIndicator()
        {
            // Crear indicador visual del hitbox real (muy pequeño)
            _hitboxIndicator = new Sprite2D();
            _hitboxIndicator.Texture = CreateHitboxTexture();
            _hitboxIndicator.Modulate = new Color(1, 1, 1, 0.4f);
            _hitboxIndicator.ZIndex = 100;
            _hitboxIndicator.Name = "HitboxIndicator";
            
            // Solo visible cuando hay balas cerca o al presionar un botón
            _hitboxIndicator.Visible = false;

            _player.AddChild(_hitboxIndicator);
        }

        private Texture2D CreateHitboxTexture()
        {
            // Crear textura de punto pequeño brillante
            int size = 16;
            var image = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = new Vector2(x - size/2, y - size/2).Length();
                    float maxDist = size / 2f;
                    
                    if (dist < 3) // Centro brillante
                    {
                        image.SetPixel(x, y, new Color("#00ff41")); // Verde terminal
                    }
                    else if (dist < 6) // Borde suave
                    {
                        float alpha = 1f - (dist - 3) / 3f;
                        image.SetPixel(x, y, new Color(0, 1, 0.25f, alpha * 0.6f));
                    }
                    else
                    {
                        image.SetPixel(x, y, Colors.Transparent);
                    }
                }
            }
            
            return ImageTexture.CreateFromImage(image);
        }

        public override void _Process(double delta)
        {
            // Cooldown de graze
            if (_grazeCooldownTimer > 0)
            {
                _grazeCooldownTimer -= (float)delta;
            }

            // Efecto de flash
            if (_isFlashing)
            {
                _grazeFlashTimer -= (float)delta;
                if (_grazeFlashTimer <= 0)
                {
                    _isFlashing = false;
                    if (_player != null)
                    {
                        var sprite = _player.GetNodeOrNull<Sprite2D>("Sprite");
                        if (sprite != null) sprite.Modulate = Colors.White;
                    }
                }
            }

            // Mostrar hitbox cuando hay muchas balas cerca
            UpdateHitboxVisibility();

            // Limpiar balas ya procesadas periódicamente
            if (Engine.GetProcessFrames() % 60 == 0)
            {
                _grazedBullets.Clear();
            }
        }

        private void UpdateHitboxVisibility()
        {
            if (_hitboxIndicator == null || _player == null) return;

            // Mostrar hitbox si:
            // 1. Hay balas activas cerca (bullet hell intenso)
            // 2. El jugador presiona un botón específico (shift por ejemplo)
            bool showHitbox = false;

            // Verificar cantidad de balas activas
            int activeBullets = BulletHellSystem.Instance?.GetActiveBulletCount() ?? 0;
            if (activeBullets > 10)
            {
                showHitbox = true;
            }

            // O si el jugador está en "focus mode" (movimiento lento)
            if (Input.IsActionPressed("focus") || Input.IsKeyPressed(Key.Shift))
            {
                showHitbox = true;
            }

            _hitboxIndicator.Visible = showHitbox;

            // Pulsar el indicador
            if (showHitbox)
            {
                float pulse = 0.3f + Mathf.Sin((float)Time.GetTicksMsec() / 200f) * 0.1f;
                _hitboxIndicator.Modulate = new Color(0, 1, 0.25f, pulse);
            }
        }

        private void OnBulletEnteredGrazeZone(Area2D area)
        {
            // Verificar si es una bala enemiga
            if (area is not EnemyBullet bullet) return;
            if (!bullet.IsActive) return;

            // Evitar múltiples grazes de la misma bala
            if (_grazedBullets.Contains(bullet.GetInstanceId())) return;
            if (_grazeCooldownTimer > 0) return;

            // ¡GRAZE!
            RegisterGraze(bullet);
        }

        private void OnBulletExitedGrazeZone(Area2D area)
        {
            // Bala salió del área sin golpear = graze exitoso confirmado
            if (area is EnemyBullet bullet)
            {
                _grazedBullets.Remove(bullet.GetInstanceId());
            }
        }

        private void RegisterGraze(EnemyBullet bullet)
        {
            _grazedBullets.Add(bullet.GetInstanceId());
            _grazeCooldownTimer = GrazeCooldown;
            
            _totalGrazeCount++;
            _sessionGrazeCount++;

            // Dar puntos
            int points = PointsPerGraze;
            
            // Bonus por graze consecutivo
            if (_sessionGrazeCount > 10) points += 2;
            if (_sessionGrazeCount > 50) points += 5;
            if (_sessionGrazeCount > 100) points += 10;

            GameEventBus.Instance?.EmitScoreChanged(points);

            // Efectos visuales
            TriggerGrazeEffect();

            // Cada 10 grazes, mostrar mensaje
            if (_sessionGrazeCount % 10 == 0)
            {
                string msg = _sessionGrazeCount switch
                {
                    10 => "¡GRAZE x10!",
                    25 => "¡GRAZE MASTER!",
                    50 => "¡GRAZE KING!",
                    100 => "¡GRAZE LEGEND!",
                    _ => $"GRAZE x{_sessionGrazeCount}"
                };
                
                // Mostrar puntos bonus por milestone de graze
                if (_player != null)
                    GameJuiceSystem.Instance?.SpawnFloatingScore(_player.GlobalPosition + Vector2.Up * 30, _sessionGrazeCount, true);
            }
        }

        private void TriggerGrazeEffect()
        {
            // Flash blanco rápido
            _isFlashing = true;
            _grazeFlashTimer = 0.05f;
            
            if (_player != null)
            {
                var sprite = _player.GetNodeOrNull<Sprite2D>("Sprite");
                if (sprite != null)
                {
                    sprite.Modulate = new Color(1.5f, 1.5f, 1.5f, 1f);
                }

                // Partículas de graze
                SpawnGrazeParticles(_player.GlobalPosition);
            }

            // Screen shake mínimo
            GameJuiceSystem.Instance?.TriggerScreenShake(0.5f, 0.02f);
        }

        private void SpawnGrazeParticles(Vector2 position)
        {
            // Crear pequeñas partículas verdes
            var rng = new Random();
            for (int i = 0; i < 3; i++)
            {
                float angle = (float)rng.NextDouble() * Mathf.Tau;
                float speed = 50 + (float)rng.NextDouble() * 50;
                
                var particle = new GrazeParticle();
                particle.GlobalPosition = position;
                particle.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                
                GetTree().Root.GetNodeOrNull("Main")?.AddChild(particle);
            }
        }

        /// <summary>
        /// Resetea el contador de graze de la sesión
        /// </summary>
        public void ResetSessionGraze()
        {
            _sessionGrazeCount = 0;
        }

        // API Pública
        public int GetTotalGrazeCount() => _totalGrazeCount;
        public int GetSessionGrazeCount() => _sessionGrazeCount;
    }

    /// <summary>
    /// Partícula visual de graze
    /// </summary>
    public partial class GrazeParticle : Node2D
    {
        public Vector2 Velocity;
        private float _lifetime = 0.3f;
        private float _age = 0f;

        public override void _Ready()
        {
            ZIndex = 90;
        }

        public override void _Process(double delta)
        {
            _age += (float)delta;
            
            if (_age >= _lifetime)
            {
                QueueFree();
                return;
            }

            GlobalPosition += Velocity * (float)delta;
            Velocity *= 0.9f; // Fricción
            
            Modulate = new Color(0, 1, 0.25f, 1f - _age / _lifetime);
        }

        public override void _Draw()
        {
            DrawCircle(Vector2.Zero, 2f, new Color("#00ff41"));
        }
    }
}
