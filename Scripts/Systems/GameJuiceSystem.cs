using Godot;
using System;
using System.Collections.Generic;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de "Game Juice" - Feedback visual satisfactorio
    /// Inspirado en Geometry Wars, Vampire Survivors, Space Invaders
    /// 
    /// Hace el juego ADICTIVO:
    /// - Puntos flotantes que vuelan hacia el score
    /// - Screen shake en impactos
    /// - Multiplicadores visibles
    /// - Efectos de partículas satisfactorios
    /// - Hit stop (micro-pausa en impacto)
    /// </summary>
    public partial class GameJuiceSystem : Node
    {
        private static GameJuiceSystem _instance;
        public static GameJuiceSystem Instance => _instance;

        // Colores
        private static readonly Color SCORE_COLOR = new Color("#00ff41");
        private static readonly Color COMBO_COLOR = new Color("#ffaa00");
        private static readonly Color CRIT_COLOR = new Color("#ff5555");
        private static readonly Color HEAL_COLOR = new Color("#00d4ff");

        // Referencias
        private Camera2D _camera;
        private Node2D _effectsContainer;
        private CanvasLayer _uiEffectsLayer;

        // Estado del multiplicador
        private int _currentMultiplier = 1;
        private int _killStreak = 0;
        private float _streakTimer = 0f;
        private const float STREAK_TIMEOUT = 2.5f;
        private const int KILLS_FOR_MULTIPLIER = 5;

        // Screen shake
        private Vector2 _originalCameraOffset;
        private float _shakeIntensity = 0f;
        private float _shakeDuration = 0f;

        public int CurrentMultiplier => _currentMultiplier;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;

            // Container para efectos en el mundo
            _effectsContainer = new Node2D();
            _effectsContainer.Name = "JuiceEffects";
            _effectsContainer.ZIndex = 100;

            // Layer para efectos UI
            _uiEffectsLayer = new CanvasLayer();
            _uiEffectsLayer.Name = "UIEffects";
            _uiEffectsLayer.Layer = 50;

            CallDeferred(MethodName.SetupContainers);

            // Suscribir a eventos
            GameEventBus.Instance.OnEnemyDefeated += OnEnemyDefeated;
            GameEventBus.Instance.OnPlayerHealthChanged += OnPlayerHealthChanged;
            GameEventBus.Instance.OnPlayerDamagedByEnemy += OnPlayerDamagedByEnemy;
        }

        private void SetupContainers()
        {
            var main = GetTree().Root.GetNodeOrNull("Main");
            if (main != null)
            {
                main.AddChild(_effectsContainer);
                main.AddChild(_uiEffectsLayer);
                _camera = main.GetNodeOrNull<Camera2D>("Player/Camera2D");
            }
        }

        public override void _Process(double delta)
        {
            // Actualizar streak timer
            if (_streakTimer > 0)
            {
                _streakTimer -= (float)delta;
                if (_streakTimer <= 0)
                {
                    ResetStreak();
                }
            }

            // Screen shake
            if (_shakeDuration > 0)
            {
                _shakeDuration -= (float)delta;
                if (_camera != null)
                {
                    var rand = new Random();
                    float offsetX = (float)(rand.NextDouble() * 2 - 1) * _shakeIntensity;
                    float offsetY = (float)(rand.NextDouble() * 2 - 1) * _shakeIntensity;
                    _camera.Offset = _originalCameraOffset + new Vector2(offsetX, offsetY);
                }

                if (_shakeDuration <= 0 && _camera != null)
                {
                    _camera.Offset = _originalCameraOffset;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // PUNTOS FLOTANTES
        // ═══════════════════════════════════════════════════════════
        public void SpawnFloatingScore(Vector2 worldPosition, int points, bool isCrit = false)
        {
            if (_effectsContainer == null) return;

            var label = new Label();
            label.Text = isCrit ? $"+{points * _currentMultiplier}!" : $"+{points * _currentMultiplier}";
            label.GlobalPosition = worldPosition + new Vector2(-30, -20);
            label.ZIndex = 200;
            
            Color color = isCrit ? CRIT_COLOR : SCORE_COLOR;
            int fontSize = isCrit ? 28 : 20;
            
            label.AddThemeColorOverride("font_color", color);
            label.AddThemeFontSizeOverride("font_size", fontSize);
            
            // Mostrar multiplicador si > 1
            if (_currentMultiplier > 1)
            {
                label.Text = $"+{points} x{_currentMultiplier}";
            }

            _effectsContainer.AddChild(label);

            // Animación: subir y fade out
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(label, "position:y", label.Position.Y - 60, 0.8f)
                 .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(label, "modulate:a", 0.0f, 0.8f)
                 .SetDelay(0.3f);
            
            if (isCrit)
            {
                tween.TweenProperty(label, "scale", new Vector2(1.3f, 1.3f), 0.1f);
                tween.Chain().TweenProperty(label, "scale", Vector2.One, 0.2f);
            }
            
            tween.Chain().TweenCallback(Callable.From(label.QueueFree));
        }

        // ═══════════════════════════════════════════════════════════
        // COMBO/MULTIPLICADOR
        // ═══════════════════════════════════════════════════════════
        private void OnEnemyDefeated(string enemyType, int points)
        {
            _killStreak++;
            _streakTimer = STREAK_TIMEOUT;

            // Calcular nuevo multiplicador
            int newMultiplier = 1 + (_killStreak / KILLS_FOR_MULTIPLIER);
            newMultiplier = Math.Min(newMultiplier, 8); // Max x8

            if (newMultiplier > _currentMultiplier)
            {
                _currentMultiplier = newMultiplier;
                ShowMultiplierPopup();
            }

            // Buscar posición del enemigo para spawn de score
            // Como no tenemos la posición exacta, usamos un evento mejorado
            // Por ahora spawn en posición aleatoria superior
            var viewportSize = GetViewport().GetVisibleRect().Size;
            var rand = new Random();
            Vector2 pos = new Vector2(
                (float)rand.NextDouble() * viewportSize.X,
                (float)rand.NextDouble() * (viewportSize.Y / 2)
            );
            
            bool isCrit = enemyType.Contains("Boss") || points >= 50;
            SpawnFloatingScore(pos, points, isCrit);

            // Pequeño shake por kill
            TriggerScreenShake(isCrit ? 8f : 3f, 0.1f);
        }

        private void ShowMultiplierPopup()
        {
            if (_uiEffectsLayer == null) return;

            var popup = new Label();
            popup.Text = $"x{_currentMultiplier} MULTIPLIER!";
            popup.SetAnchorsPreset(Control.LayoutPreset.Center);
            popup.HorizontalAlignment = HorizontalAlignment.Center;
            popup.AddThemeColorOverride("font_color", COMBO_COLOR);
            popup.AddThemeFontSizeOverride("font_size", 36);
            popup.Modulate = new Color(1, 1, 1, 0);
            popup.Scale = new Vector2(0.5f, 0.5f);
            
            _uiEffectsLayer.AddChild(popup);

            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(popup, "modulate:a", 1.0f, 0.15f);
            tween.TweenProperty(popup, "scale", new Vector2(1.2f, 1.2f), 0.15f);
            tween.Chain().TweenProperty(popup, "scale", Vector2.One, 0.1f);
            tween.Chain().TweenInterval(0.8f);
            tween.Chain().TweenProperty(popup, "modulate:a", 0.0f, 0.3f);
            tween.Chain().TweenCallback(Callable.From(popup.QueueFree));

            // Big shake para multiplicador
            TriggerScreenShake(12f, 0.2f);
        }

        private void ResetStreak()
        {
            if (_currentMultiplier > 1)
            {
                ShowMultiplierLost();
            }
            _killStreak = 0;
            _currentMultiplier = 1;
        }

        private void ShowMultiplierLost()
        {
            if (_uiEffectsLayer == null) return;

            var popup = new Label();
            popup.Text = "MULTIPLIER LOST";
            popup.SetAnchorsPreset(Control.LayoutPreset.Center);
            popup.Position = new Vector2(0, 50);
            popup.HorizontalAlignment = HorizontalAlignment.Center;
            popup.AddThemeColorOverride("font_color", CRIT_COLOR);
            popup.AddThemeFontSizeOverride("font_size", 20);
            
            _uiEffectsLayer.AddChild(popup);

            var tween = CreateTween();
            tween.TweenProperty(popup, "modulate:a", 0.0f, 1.5f);
            tween.TweenCallback(Callable.From(popup.QueueFree));
        }

        // ═══════════════════════════════════════════════════════════
        // SCREEN SHAKE
        // ═══════════════════════════════════════════════════════════
        public void TriggerScreenShake(float intensity, float duration)
        {
            if (_camera == null)
            {
                _camera = GetTree().Root.GetNodeOrNull<Camera2D>("Main/Player/Camera2D");
                if (_camera != null) _originalCameraOffset = _camera.Offset;
            }

            _shakeIntensity = intensity;
            _shakeDuration = duration;
        }

        // ═══════════════════════════════════════════════════════════
        // HIT STOP (Micro pausa en impacto)
        // ═══════════════════════════════════════════════════════════
        public async void TriggerHitStop(float duration = 0.05f)
        {
            Engine.TimeScale = 0.1f;
            await ToSignal(GetTree().CreateTimer(duration, true, false, true), "timeout");
            Engine.TimeScale = 1.0f;
        }

        // ═══════════════════════════════════════════════════════════
        // EFECTOS DE DAÑO/CURACIÓN
        // ═══════════════════════════════════════════════════════════
        private float _lastHealth = 100f;

        private void OnPlayerHealthChanged(float health)
        {
            if (health < _lastHealth)
            {
                // Tomó daño - flash rojo y shake
                float damage = _lastHealth - health;
                TriggerScreenShake(damage / 3, 0.15f);
                ShowDamageFlash();
                
                // Resetear multiplicador si toma mucho daño
                if (damage >= 20)
                {
                    ResetStreak();
                }
            }
            else if (health > _lastHealth)
            {
                // Se curó - efecto positivo
                ShowHealEffect(health - _lastHealth);
            }
            
            _lastHealth = health;
        }

        private void ShowDamageFlash()
        {
            if (_uiEffectsLayer == null) return;

            var flash = new ColorRect();
            flash.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            flash.Color = new Color(1, 0, 0, 0.3f);
            flash.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            _uiEffectsLayer.AddChild(flash);

            var tween = CreateTween();
            tween.TweenProperty(flash, "color:a", 0.0f, 0.2f);
            tween.TweenCallback(Callable.From(flash.QueueFree));
        }

        private void ShowHealEffect(float amount)
        {
            if (_uiEffectsLayer == null) return;

            var popup = new Label();
            popup.Text = $"+{(int)amount} HP";
            popup.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
            popup.Position = new Vector2(150, -120);
            popup.AddThemeColorOverride("font_color", HEAL_COLOR);
            popup.AddThemeFontSizeOverride("font_size", 24);
            
            _uiEffectsLayer.AddChild(popup);

            var tween = CreateTween();
            tween.TweenProperty(popup, "position:y", popup.Position.Y - 40, 0.6f);
            tween.Parallel().TweenProperty(popup, "modulate:a", 0.0f, 0.6f).SetDelay(0.2f);
            tween.TweenCallback(Callable.From(popup.QueueFree));
        }

        // ═══════════════════════════════════════════════════════════
        // TIPS CONTEXTUALES AL RECIBIR DAÑO
        // ═══════════════════════════════════════════════════════════
        private float _lastTipTime = 0f;
        private const float TIP_COOLDOWN = 10f; // No mostrar tips muy seguido

        private void OnPlayerDamagedByEnemy(string enemyType, float damage)
        {
            // Cooldown para no saturar de tips
            float currentTime = (float)Time.GetTicksMsec() / 1000f;
            if (currentTime - _lastTipTime < TIP_COOLDOWN) return;
            
            // Solo mostrar tip si el daño es significativo
            if (damage < 10) return;

            string tip = GetTipForEnemy(enemyType);
            if (!string.IsNullOrEmpty(tip))
            {
                ShowContextualTip(tip, enemyType);
                _lastTipTime = currentTime;
            }
        }

        private string GetTipForEnemy(string enemyType)
        {
            return enemyType switch
            {
                "Malware" => "MALWARE: Software malicioso que daña tu sistema. Usa ANTIVIRUS para eliminarlo.",
                "Phishing" => "PHISHING: Intento de engaño. Siempre verifica la URL antes de hacer clic.",
                "DDoS" => "DDoS: Ataque de saturacion. Los CDN y rate limiting ayudan a mitigarlo.",
                "SQLInjection" => "SQL INJECTION: Ataque a bases de datos. Usa consultas parametrizadas.",
                "BruteForce" => "FUERZA BRUTA: Miles de intentos de contrasena. Usa 2FA y limita intentos.",
                "Ransomware" => "RANSOMWARE: Cifra tus archivos. Haz backups regulares y nunca pagues.",
                "Trojan" => "TROYANO: Se disfraza de software legitimo. Analiza antes de instalar.",
                "Worm" => "GUSANO: Se replica automaticamente. Usa FIREWALL para aislarlo.",
                _ => ""
            };
        }

        private void ShowContextualTip(string tip, string enemyType)
        {
            if (_uiEffectsLayer == null) return;

            // Panel contenedor con estilo terminal
            var panel = new Panel();
            panel.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
            panel.OffsetTop = -150;
            panel.OffsetBottom = -50;
            panel.OffsetLeft = -300;
            panel.OffsetRight = 300;
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.02f, 0.02f, 0.02f, 0.95f);
            style.BorderColor = new Color("#ffaa00");
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(5);
            panel.AddThemeStyleboxOverride("panel", style);
            
            _uiEffectsLayer.AddChild(panel);

            // Icono y texto
            var label = new Label();
            label.Text = $"[!] {tip}";
            label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            label.OffsetLeft = 15;
            label.OffsetRight = -15;
            label.OffsetTop = 10;
            label.OffsetBottom = -10;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            label.AutowrapMode = TextServer.AutowrapMode.Word;
            label.AddThemeColorOverride("font_color", new Color("#ffaa00"));
            label.AddThemeFontSizeOverride("font_size", 16);
            panel.AddChild(label);

            // Animación de entrada y salida
            panel.Modulate = new Color(1, 1, 1, 0);
            panel.Scale = new Vector2(0.9f, 0.9f);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(panel, "modulate:a", 1.0f, 0.2f);
            tween.TweenProperty(panel, "scale", Vector2.One, 0.2f);
            tween.Chain().TweenInterval(5.0f);
            tween.Chain().TweenProperty(panel, "modulate:a", 0.0f, 0.5f);
            tween.Chain().TweenCallback(Callable.From(panel.QueueFree));
        }

        // ═══════════════════════════════════════════════════════════
        // EXPLOSIÓN DE PARTÍCULAS
        // ═══════════════════════════════════════════════════════════
        public void SpawnExplosion(Vector2 position, Color color, int particleCount = 20)
        {
            if (_effectsContainer == null) return;

            var particles = new CpuParticles2D();
            particles.GlobalPosition = position;
            particles.Emitting = true;
            particles.OneShot = true;
            particles.Explosiveness = 1.0f;
            particles.Amount = particleCount;
            particles.Lifetime = 0.5f;
            particles.SpeedScale = 2;
            particles.Direction = new Vector2(0, 0);
            particles.Spread = 180;
            particles.InitialVelocityMin = 100;
            particles.InitialVelocityMax = 250;
            particles.Gravity = new Vector2(0, 200);
            particles.ScaleAmountMin = 2;
            particles.ScaleAmountMax = 5;
            particles.Color = color;

            _effectsContainer.AddChild(particles);

            // Auto-eliminar después de la animación
            GetTree().CreateTimer(1.0f).Timeout += particles.QueueFree;
        }

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnEnemyDefeated -= OnEnemyDefeated;
            GameEventBus.Instance.OnPlayerHealthChanged -= OnPlayerHealthChanged;
            GameEventBus.Instance.OnPlayerDamagedByEnemy -= OnPlayerDamagedByEnemy;
        }
    }
}
