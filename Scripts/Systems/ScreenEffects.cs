using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de efectos visuales de pantalla
    /// Proporciona screen shake, flash, y otros efectos de feedback
    /// </summary>
    public partial class ScreenEffects : CanvasLayer
    {
        private static ScreenEffects _instance;
        public static ScreenEffects Instance => _instance;

        // Componentes visuales
        private ColorRect _flashRect;
        private ColorRect _vignetteRect;
        private Camera2D _camera;
        
        // Screen shake
        private float _shakeIntensity = 0f;
        private float _shakeDuration = 0f;
        private float _shakeTimer = 0f;
        private Vector2 _originalCameraOffset;

        // Flash
        private Tween _flashTween;

        // Colores del tema
        private static readonly Color DAMAGE_COLOR = new Color(1, 0, 0, 0.4f);
        private static readonly Color HEAL_COLOR = new Color(0, 1, 0.3f, 0.3f);
        private static readonly Color POWERUP_COLOR = new Color(0, 0.8f, 1, 0.3f);
        private static readonly Color COMBO_COLOR = new Color(1, 1, 0, 0.3f);

        public override void _Ready()
        {
            _instance = this;
            Layer = 100; // Siempre encima
            
            CreateEffectLayers();
            SubscribeToEvents();
        }

        private void CreateEffectLayers()
        {
            // Flash overlay (para daño, curación, etc.)
            _flashRect = new ColorRect();
            _flashRect.Name = "FlashOverlay";
            _flashRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _flashRect.Color = new Color(0, 0, 0, 0);
            _flashRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(_flashRect);

            // Viñeta (para tensión)
            _vignetteRect = new ColorRect();
            _vignetteRect.Name = "Vignette";
            _vignetteRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _vignetteRect.Color = new Color(0, 0, 0, 0);
            _vignetteRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(_vignetteRect);
        }

        private void SubscribeToEvents()
        {
            GameEventBus.Instance.OnPlayerHealthChanged += OnHealthChanged;
            GameEventBus.Instance.OnPlayerDied += OnPlayerDied;
            GameEventBus.Instance.OnPowerUpCollected += OnPowerUpCollected;
            GameEventBus.Instance.OnEnemyDefeated += OnEnemyDefeated;
        }

        public override void _Process(double delta)
        {
            ProcessScreenShake(delta);
        }

        #region Screen Shake

        private void ProcessScreenShake(double delta)
        {
            if (_shakeTimer > 0)
            {
                _shakeTimer -= (float)delta;
                
                // Calcular intensidad decreciente
                float currentIntensity = _shakeIntensity * (_shakeTimer / _shakeDuration);
                
                // Aplicar offset aleatorio a la cámara o viewport
                var offset = new Vector2(
                    (float)GD.RandRange(-currentIntensity, currentIntensity),
                    (float)GD.RandRange(-currentIntensity, currentIntensity)
                );

                if (_camera != null)
                {
                    _camera.Offset = _originalCameraOffset + offset;
                }
                else
                {
                    // Fallback: mover el layer entero
                    Offset = offset;
                }
            }
            else if (_camera != null && _camera.Offset != _originalCameraOffset)
            {
                _camera.Offset = _originalCameraOffset;
                Offset = Vector2.Zero;
            }
        }

        /// <summary>
        /// Inicia un screen shake
        /// </summary>
        /// <param name="intensity">Intensidad del shake (píxeles)</param>
        /// <param name="duration">Duración en segundos</param>
        public void Shake(float intensity = 5f, float duration = 0.2f)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeTimer = duration;

            // Intentar encontrar cámara
            if (_camera == null)
            {
                _camera = GetViewport().GetCamera2D();
                if (_camera != null)
                {
                    _originalCameraOffset = _camera.Offset;
                }
            }
        }

        /// <summary>
        /// Shake pequeño (hit menor)
        /// </summary>
        public void ShakeSmall() => Shake(3f, 0.1f);

        /// <summary>
        /// Shake medio (enemigo destruido)
        /// </summary>
        public void ShakeMedium() => Shake(6f, 0.15f);

        /// <summary>
        /// Shake fuerte (daño recibido, explosión grande)
        /// </summary>
        public void ShakeStrong() => Shake(12f, 0.25f);

        /// <summary>
        /// Shake épico (game over, boss)
        /// </summary>
        public void ShakeEpic() => Shake(20f, 0.4f);

        #endregion

        #region Flash Effects

        /// <summary>
        /// Flash de pantalla con color y duración
        /// </summary>
        public void Flash(Color color, float duration = 0.15f)
        {
            _flashTween?.Kill();
            _flashTween = CreateTween();
            
            _flashRect.Color = color;
            _flashTween.TweenProperty(_flashRect, "color:a", 0f, duration)
                .SetEase(Tween.EaseType.Out);
        }

        /// <summary>
        /// Flash rojo de daño
        /// </summary>
        public void FlashDamage() => Flash(DAMAGE_COLOR, 0.2f);

        /// <summary>
        /// Flash verde de curación
        /// </summary>
        public void FlashHeal() => Flash(HEAL_COLOR, 0.25f);

        /// <summary>
        /// Flash cyan de power-up
        /// </summary>
        public void FlashPowerUp() => Flash(POWERUP_COLOR, 0.3f);

        /// <summary>
        /// Flash amarillo de combo
        /// </summary>
        public void FlashCombo() => Flash(COMBO_COLOR, 0.15f);

        #endregion

        #region Vignette Effects

        /// <summary>
        /// Mostrar viñeta de peligro (vida baja)
        /// </summary>
        public void ShowDangerVignette(float healthPercent)
        {
            if (healthPercent < 30)
            {
                float intensity = (30 - healthPercent) / 30f * 0.4f;
                _vignetteRect.Color = new Color(0.5f, 0, 0, intensity);
            }
            else
            {
                _vignetteRect.Color = new Color(0, 0, 0, 0);
            }
        }

        #endregion

        #region Event Handlers

        private float _lastHealth = 100f;
        
        private void OnHealthChanged(float health)
        {
            // Flash de daño si perdimos vida
            if (health < _lastHealth)
            {
                float damage = _lastHealth - health;
                if (damage > 20)
                {
                    FlashDamage();
                    ShakeStrong();
                }
                else if (damage > 10)
                {
                    Flash(DAMAGE_COLOR * 0.7f, 0.15f);
                    ShakeMedium();
                }
                else
                {
                    Flash(DAMAGE_COLOR * 0.4f, 0.1f);
                    ShakeSmall();
                }
            }
            // Flash de curación si ganamos vida
            else if (health > _lastHealth)
            {
                FlashHeal();
            }

            _lastHealth = health;
            ShowDangerVignette(health);
        }

        private void OnPlayerDied()
        {
            ShakeEpic();
            Flash(new Color(1, 0, 0, 0.6f), 0.5f);
        }

        private void OnPowerUpCollected(string powerUpType)
        {
            FlashPowerUp();
            ShakeSmall();
        }

        private static int _comboCount = 0;
        private static float _comboTimer = 0f;

        private void OnEnemyDefeated(string enemyType, int points)
        {
            // Pequeño shake por cada enemigo
            ShakeSmall();
            
            // Combo tracking simple
            _comboCount++;
            _comboTimer = 2f;
            
            if (_comboCount >= 5)
            {
                FlashCombo();
                ShakeMedium();
            }
        }

        #endregion

        #region Utility Effects

        /// <summary>
        /// Efecto de freeze frame (pausa corta para impacto)
        /// </summary>
        public async void FreezeFrame(float duration = 0.05f)
        {
            Engine.TimeScale = 0.0;
            await ToSignal(GetTree().CreateTimer(duration, true, false, true), "timeout");
            Engine.TimeScale = 1.0;
        }

        /// <summary>
        /// Slow motion temporal
        /// </summary>
        public async void SlowMotion(float scale = 0.3f, float duration = 0.5f)
        {
            Engine.TimeScale = scale;
            
            var tween = CreateTween();
            tween.SetProcessMode(Tween.TweenProcessMode.Physics);
            tween.TweenProperty(Engine.Singleton, "time_scale", 1.0f, duration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Quad);
        }

        #endregion

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnPlayerHealthChanged -= OnHealthChanged;
            GameEventBus.Instance.OnPlayerDied -= OnPlayerDied;
            GameEventBus.Instance.OnPowerUpCollected -= OnPowerUpCollected;
            GameEventBus.Instance.OnEnemyDefeated -= OnEnemyDefeated;
            
            _instance = null;
        }
    }
}
