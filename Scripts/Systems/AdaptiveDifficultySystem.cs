using Godot;
using System;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de Dificultad Adaptativa - Inspirado en Touhou Rank + Hades God Mode
    /// 
    /// MECÁNICAS:
    /// 1. THREAT LEVEL (Rank invisible): El juego se adapta a tu habilidad
    /// 2. FIREWALL MODE: +3% resistencia por muerte (estilo Hades God Mode)
    /// 3. ENCRYPTION BURST: Panic button que limpia balas (estilo Gungeon Blanks)
    /// 
    /// FILOSOFÍA: "El jugador siempre debe sentir que está a punto de perder,
    ///            pero siempre tiene una oportunidad de ganar"
    /// </summary>
    public partial class AdaptiveDifficultySystem : Node
    {
        private static AdaptiveDifficultySystem _instance;
        public static AdaptiveDifficultySystem Instance => _instance;

        // ═══════════════════════════════════════════════════════════════════
        // THREAT LEVEL (Rank System - Inspirado en Touhou)
        // ═══════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Nivel de amenaza (1-100). Afecta densidad de balas y velocidad
        /// Valor invisible para el jugador - el juego se adapta automáticamente
        /// </summary>
        [Export] public float ThreatLevel { get; private set; } = 50f;
        private const float THREAT_MIN = 10f;
        private const float THREAT_MAX = 100f;
        
        // Modificadores de Threat Level
        private const float THREAT_ON_DEATH = -15f;        // Morir reduce amenaza
        private const float THREAT_ON_HIT = -3f;           // Recibir daño reduce un poco
        private const float THREAT_ON_KILL = 1f;           // Matar enemigo sube un poco
        private const float THREAT_ON_PERFECT_WAVE = 8f;   // Oleada sin daño sube mucho
        private const float THREAT_ON_GRAZE = 0.5f;        // Rozar bala sin morir (futuro)
        private const float THREAT_DECAY_RATE = 0.5f;      // Decaimiento natural por segundo

        // ═══════════════════════════════════════════════════════════════════
        // FIREWALL MODE (God Mode - Inspirado en Hades)
        // ═══════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Reducción de daño permanente que aumenta con cada muerte
        /// "Tu firewall aprende de cada ataque"
        /// </summary>
        [Export] public float FirewallResistance { get; private set; } = 0f;
        private const float FIREWALL_PER_DEATH = 3f;       // +3% por muerte
        private const float FIREWALL_MAX = 50f;            // Máximo 50% reducción
        
        // ═══════════════════════════════════════════════════════════════════
        // ENCRYPTION BURST (Blank - Inspirado en Enter the Gungeon)
        // ═══════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Número de "Encryption Bursts" disponibles
        /// Borra todas las balas en pantalla cuando se activa
        /// </summary>
        [Export] public int EncryptionBursts { get; private set; } = 2;
        private const int BURST_MAX = 3;
        private const int BURST_PER_WAVE = 1;              // +1 por oleada completada
        
        // Cooldown para evitar spam
        private float _burstCooldown = 0f;
        private const float BURST_COOLDOWN_TIME = 1.5f;
        
        // ═══════════════════════════════════════════════════════════════════
        // DEATHBOMB WINDOW (Inspirado en Touhou)
        // ═══════════════════════════════════════════════════════════════════
        
        /// <summary>
        /// Ventana de tiempo después de ser golpeado donde puedes activar Burst
        /// automáticamente para salvarte
        /// </summary>
        private bool _deathbombWindowActive = false;
        private float _deathbombTimer = 0f;
        private const float DEATHBOMB_WINDOW = 0.25f;      // 250ms para reaccionar

        // Estadísticas de sesión
        public int TotalDeaths { get; private set; } = 0;
        public int BurstsUsed { get; private set; } = 0;
        public int PerfectWaves { get; private set; } = 0;
        private bool _tookDamageThisWave = false;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            SubscribeToEvents();
            GD.Print("[AdaptiveDifficulty] Sistema inicializado");
            GD.Print($"  → Threat Level: {ThreatLevel}");
            GD.Print($"  → Firewall Resistance: {FirewallResistance}%");
            GD.Print($"  → Encryption Bursts: {EncryptionBursts}");
        }

        private void SubscribeToEvents()
        {
            GameEventBus.Instance.OnPlayerDied += OnPlayerDeath;
            GameEventBus.Instance.OnPlayerHealthChanged += OnPlayerDamaged;
            GameEventBus.Instance.OnEnemyDefeated += OnEnemyDefeated;
            GameEventBus.Instance.OnWaveCompleted += OnWaveCompleted;
        }

        public override void _Process(double delta)
        {
            // Actualizar cooldown del burst
            if (_burstCooldown > 0)
            {
                _burstCooldown -= (float)delta;
            }
            
            // Actualizar ventana de deathbomb
            if (_deathbombWindowActive)
            {
                _deathbombTimer -= (float)delta;
                if (_deathbombTimer <= 0)
                {
                    _deathbombWindowActive = false;
                }
            }
            
            // Decaimiento natural del threat level (tendencia al equilibrio)
            float targetThreat = 50f;
            if (ThreatLevel > targetThreat)
            {
                ThreatLevel -= THREAT_DECAY_RATE * (float)delta;
            }
            else if (ThreatLevel < targetThreat)
            {
                ThreatLevel += THREAT_DECAY_RATE * 0.5f * (float)delta; // Sube más lento
            }
            
            // Clamp
            ThreatLevel = Mathf.Clamp(ThreatLevel, THREAT_MIN, THREAT_MAX);
            
            // Input para Encryption Burst (Tab o Q)
            if (Input.IsActionJustPressed("encryption_burst") || Input.IsKeyPressed(Key.Tab) || Input.IsKeyPressed(Key.Q))
            {
                TryUseEncryptionBurst();
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // ENCRYPTION BURST
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Intenta usar Encryption Burst - limpia todas las balas
        /// </summary>
        public bool TryUseEncryptionBurst()
        {
            if (EncryptionBursts <= 0)
            {
                GD.Print("[AdaptiveDifficulty] ⚠️ Sin Encryption Bursts disponibles");
                return false;
            }
            
            if (_burstCooldown > 0)
            {
                GD.Print($"[AdaptiveDifficulty] ⚠️ Burst en cooldown: {_burstCooldown:F1}s");
                return false;
            }
            
            // Usar burst
            EncryptionBursts--;
            BurstsUsed++;
            _burstCooldown = BURST_COOLDOWN_TIME;
            
            // Limpiar todas las balas
            BulletHellSystem.Instance?.ClearAllBullets();
            
            // Efecto visual
            CreateBurstEffect();
            
            GD.Print($"[AdaptiveDifficulty] 🔐 ENCRYPTION BURST! Balas eliminadas. Restantes: {EncryptionBursts}");
            
            // Emitir evento para UI
            GameEventBus.Instance.EmitSecurityTipShown($"🔐 ENCRYPTION BURST! ({EncryptionBursts} restantes)");
            
            return true;
        }

        /// <summary>
        /// Usa burst automáticamente durante deathbomb window
        /// </summary>
        public bool TryDeathbomb()
        {
            if (!_deathbombWindowActive) return false;
            return TryUseEncryptionBurst();
        }

        private void CreateBurstEffect()
        {
            // Buscar el jugador para crear efecto en su posición
            var player = GetTree().Root.GetNodeOrNull<Node2D>("Main/Player");
            if (player == null) return;
            
            // Crear onda expansiva visual
            var effect = new CpuParticles2D();
            effect.GlobalPosition = player.GlobalPosition;
            effect.Emitting = true;
            effect.Amount = 100;
            effect.Lifetime = 0.6f;
            effect.OneShot = true;
            effect.Explosiveness = 1.0f;
            effect.Spread = 180f;
            effect.InitialVelocityMin = 400f;
            effect.InitialVelocityMax = 600f;
            effect.ScaleAmountMin = 4f;
            effect.ScaleAmountMax = 10f;
            effect.Color = new Color("#00ffff"); // Cyan = Encryption
            effect.Finished += () => effect.QueueFree();
            
            GetTree().Root.AddChild(effect);
            
            // Flash de pantalla
            var canvas = new CanvasLayer();
            var flash = new ColorRect();
            flash.Color = new Color(0, 1, 1, 0.3f);
            flash.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            canvas.AddChild(flash);
            GetTree().Root.AddChild(canvas);
            
            var tween = GetTree().CreateTween();
            tween.TweenProperty(flash, "color:a", 0f, 0.3f);
            tween.TweenCallback(Callable.From(() => canvas.QueueFree()));
        }

        // ═══════════════════════════════════════════════════════════════════
        // EVENT HANDLERS
        // ═══════════════════════════════════════════════════════════════════

        private void OnPlayerDeath()
        {
            TotalDeaths++;
            
            // Reducir Threat Level
            ThreatLevel += THREAT_ON_DEATH; // Es negativo
            GD.Print($"[AdaptiveDifficulty] 💀 Muerte #{TotalDeaths} - Threat: {ThreatLevel:F0}");
            
            // Aumentar Firewall Resistance (God Mode progresivo)
            if (FirewallResistance < FIREWALL_MAX)
            {
                FirewallResistance = Mathf.Min(FirewallResistance + FIREWALL_PER_DEATH, FIREWALL_MAX);
                GD.Print($"[AdaptiveDifficulty] 🛡️ Firewall mejorado: {FirewallResistance}% resistencia");
                GameEventBus.Instance.EmitSecurityTipShown($"🛡️ Firewall: +{FIREWALL_PER_DEATH}% resistencia ({FirewallResistance}% total)");
            }
        }

        private float _lastKnownHealth = 100f;
        private void OnPlayerDamaged(float newHealth)
        {
            if (newHealth < _lastKnownHealth)
            {
                // Tomamos daño
                ThreatLevel += THREAT_ON_HIT;
                _tookDamageThisWave = true;
                
                // Activar ventana de deathbomb
                _deathbombWindowActive = true;
                _deathbombTimer = DEATHBOMB_WINDOW;
            }
            _lastKnownHealth = newHealth;
        }

        private void OnEnemyDefeated(string enemyType, int points)
        {
            ThreatLevel += THREAT_ON_KILL;
        }

        private void OnWaveCompleted(int wave)
        {
            // Dar burst por completar oleada
            if (EncryptionBursts < BURST_MAX)
            {
                EncryptionBursts = Mathf.Min(EncryptionBursts + BURST_PER_WAVE, BURST_MAX);
                GD.Print($"[AdaptiveDifficulty] 🔐 +1 Encryption Burst (Total: {EncryptionBursts})");
            }
            
            // Bonus por oleada perfecta
            if (!_tookDamageThisWave)
            {
                PerfectWaves++;
                ThreatLevel += THREAT_ON_PERFECT_WAVE;
                GD.Print($"[AdaptiveDifficulty] ⭐ Oleada perfecta #{PerfectWaves}! Threat: {ThreatLevel:F0}");
                GameEventBus.Instance.EmitSecurityTipShown("⭐ ¡OLEADA PERFECTA! +Dificultad");
            }
            
            // Reset para siguiente oleada
            _tookDamageThisWave = false;
        }

        // ═══════════════════════════════════════════════════════════════════
        // GETTERS PARA OTROS SISTEMAS
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Multiplicador de dificultad basado en Threat Level (0.5 - 1.5)
        /// </summary>
        public float GetThreatMultiplier()
        {
            // Threat 10 = 0.5x, Threat 50 = 1.0x, Threat 100 = 1.5x
            return 0.5f + (ThreatLevel / THREAT_MAX);
        }

        /// <summary>
        /// Multiplicador de daño recibido (considerando Firewall)
        /// </summary>
        public float GetDamageMultiplier()
        {
            // 0% firewall = 1.0x daño, 50% firewall = 0.5x daño
            return 1f - (FirewallResistance / 100f);
        }

        /// <summary>
        /// Aplica reducción de daño por Firewall
        /// </summary>
        public float ApplyFirewallReduction(float damage)
        {
            return damage * GetDamageMultiplier();
        }

        /// <summary>
        /// Reinicia el sistema para nueva partida (mantiene Firewall Mode)
        /// </summary>
        public void ResetForNewGame()
        {
            ThreatLevel = 50f;
            EncryptionBursts = 2;
            _tookDamageThisWave = false;
            _lastKnownHealth = 100f;
            // Nota: FirewallResistance NO se resetea (es progresión permanente estilo Hades)
            
            GD.Print("[AdaptiveDifficulty] Reset para nueva partida (Firewall preservado)");
        }

        /// <summary>
        /// Reset completo (incluyendo Firewall) - Solo al cerrar el juego
        /// </summary>
        public void FullReset()
        {
            ThreatLevel = 50f;
            FirewallResistance = 0f;
            EncryptionBursts = 2;
            TotalDeaths = 0;
            BurstsUsed = 0;
            PerfectWaves = 0;
            
            GD.Print("[AdaptiveDifficulty] Reset completo");
        }

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnPlayerDied -= OnPlayerDeath;
            GameEventBus.Instance.OnPlayerHealthChanged -= OnPlayerDamaged;
            GameEventBus.Instance.OnEnemyDefeated -= OnEnemyDefeated;
            GameEventBus.Instance.OnWaveCompleted -= OnWaveCompleted;
        }
    }
}
