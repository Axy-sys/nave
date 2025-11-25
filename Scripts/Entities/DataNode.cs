using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Entities
{
    /// <summary>
    /// Nodo de datos hackeable con estética terminal/web
    /// </summary>
    public partial class DataNode : Area2D
    {
        [Export] public float HackDuration = 2.0f;
        [Export] public int PointsReward = 500;
        
        // Colores web
        private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
        private static readonly Color CYBER_BLUE = new Color("#00d4ff");
        private static readonly Color BG_DARK = new Color("#0a0a0a");
        
        private bool _isPlayerNearby = false;
        private float _currentHackTime = 0f;
        private bool _isHacked = false;
        private Panel _visual;
        private Label _label;
        private ProgressBar _progressBar;
        private CpuParticles2D _particles;

        public override void _Ready()
        {
            // Setup Collision
            var shape = new CollisionShape2D();
            var circle = new CircleShape2D();
            circle.Radius = 35f;
            shape.Shape = circle;
            AddChild(shape);

            // ═══ VISUAL: Panel estilo terminal ═══
            _visual = new Panel();
            _visual.Size = new Vector2(60, 50);
            _visual.Position = new Vector2(-30, -25);
            
            var style = new StyleBoxFlat();
            style.BgColor = BG_DARK;
            style.BorderColor = CYBER_BLUE;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(4);
            _visual.AddThemeStyleboxOverride("panel", style);
            AddChild(_visual);
            
            // Icono de datos
            var icon = new Label();
            icon.Text = "[■■]";
            icon.Position = new Vector2(12, 8);
            icon.AddThemeColorOverride("font_color", CYBER_BLUE);
            icon.AddThemeFontSizeOverride("font_size", 16);
            _visual.AddChild(icon);

            // Progress bar
            _progressBar = new ProgressBar();
            _progressBar.Size = new Vector2(50, 6);
            _progressBar.Position = new Vector2(5, 35);
            _progressBar.ShowPercentage = false;
            _progressBar.Value = 0;
            
            var barBg = new StyleBoxFlat();
            barBg.BgColor = new Color(0.1f, 0.1f, 0.1f);
            _progressBar.AddThemeStyleboxOverride("background", barBg);
            
            var barFill = new StyleBoxFlat();
            barFill.BgColor = TERMINAL_GREEN;
            _progressBar.AddThemeStyleboxOverride("fill", barFill);
            _visual.AddChild(_progressBar);

            // Label
            _label = new Label();
            _label.Text = "DATA";
            _label.Position = new Vector2(-15, -45);
            _label.AddThemeColorOverride("font_color", CYBER_BLUE);
            _label.AddThemeFontSizeOverride("font_size", 11);
            AddChild(_label);
            
            // Partículas
            _particles = new CpuParticles2D();
            _particles.Emitting = true;
            _particles.Amount = 6;
            _particles.Lifetime = 1.0f;
            _particles.EmissionShape = CpuParticles2D.EmissionShapeEnum.Sphere;
            _particles.EmissionSphereRadius = 25;
            _particles.Direction = new Vector2(0, -1);
            _particles.Spread = 45;
            _particles.InitialVelocityMin = 15;
            _particles.InitialVelocityMax = 30;
            _particles.ScaleAmountMin = 1;
            _particles.ScaleAmountMax = 2;
            _particles.Color = new Color(CYBER_BLUE, 0.6f);
            AddChild(_particles);

            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        public override void _Process(double delta)
        {
            if (_isHacked) return;

            if (_isPlayerNearby && Input.IsActionPressed("interact"))
            {
                _currentHackTime += (float)delta;
                float progress = _currentHackTime / HackDuration;
                
                // Visual Feedback
                _progressBar.Value = progress * 100;
                _label.Text = $"HACK {(int)(progress * 100)}%";
                _label.AddThemeColorOverride("font_color", TERMINAL_GREEN);
                _particles.Color = new Color(TERMINAL_GREEN, 0.8f);

                if (_currentHackTime >= HackDuration)
                {
                    CompleteHack();
                }
            }
            else if (_currentHackTime > 0)
            {
                // Decay progress if key released
                _currentHackTime -= (float)delta * 2;
                _currentHackTime = Mathf.Max(0, _currentHackTime);
                _progressBar.Value = (_currentHackTime / HackDuration) * 100;
                _label.Text = "DATA";
                _label.AddThemeColorOverride("font_color", CYBER_BLUE);
                _particles.Color = new Color(CYBER_BLUE, 0.6f);
            }
        }

        private void CompleteHack()
        {
            _isHacked = true;
            _label.Text = "✓ OK";
            _label.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _progressBar.Value = 100;
            
            // Cambiar borde a verde
            var style = new StyleBoxFlat();
            style.BgColor = BG_DARK;
            style.BorderColor = TERMINAL_GREEN;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(4);
            _visual.AddThemeStyleboxOverride("panel", style);
            
            _particles.Color = new Color(TERMINAL_GREEN, 0.8f);
            
            GameEventBus.Instance.EmitEnemyDefeated("Data Secured", PointsReward);
            GD.Print("🔓 Data Node Hacked!");
            
            // Heal player
            var player = GetTree().Root.GetNodeOrNull<Player>("Main/Player");
            if (player != null)
            {
                player.Heal(20);
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is Player)
            {
                _isPlayerNearby = true;
                if (!_isHacked)
                {
                    _label.Text = "[E] HACK";
                    _label.AddThemeColorOverride("font_color", new Color("#ffaa00"));
                }
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body is Player)
            {
                _isPlayerNearby = false;
                if (!_isHacked)
                {
                    _label.Text = "DATA";
                    _label.AddThemeColorOverride("font_color", CYBER_BLUE);
                }
            }
        }
    }
}
