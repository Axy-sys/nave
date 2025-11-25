using Godot;
using CyberSecurityGame.Components;

namespace CyberSecurityGame.Entities
{
    /// <summary>
    /// Obstáculo del entorno - Datos corruptos estilo glitch
    /// </summary>
    public partial class Obstacle : Area2D
    {
        [Export] public float Speed = 150f;
        [Export] public int Damage = 20;
        [Export] public string ObstacleName = "Corrupted Data";
        
        // Colores web
        private static readonly Color GLITCH_PURPLE = new Color("#bf00ff");
        private static readonly Color GLITCH_GREEN = new Color("#00ff41");
        private static readonly Color DARK_BG = new Color("#0a0a0a");
        
        private Panel _visual;
        private Label _glitchText;
        private float _glitchTimer = 0f;
        private string[] _glitchChars = { "█", "▓", "▒", "░", "╳", "◊", "●", "■" };

        public override void _Ready()
        {
            AddToGroup("Obstacles");
            BodyEntered += OnBodyEntered;
            
            // ═══ VISUAL: Panel con efecto glitch ═══
            _visual = new Panel();
            _visual.Size = new Vector2(40, 40);
            _visual.Position = new Vector2(-20, -20);
            
            var style = new StyleBoxFlat();
            style.BgColor = DARK_BG;
            style.BorderColor = GLITCH_PURPLE;
            style.SetBorderWidthAll(1);
            _visual.AddThemeStyleboxOverride("panel", style);
            AddChild(_visual);
            
            // Texto glitch dentro
            _glitchText = new Label();
            _glitchText.Text = "ERR";
            _glitchText.Position = new Vector2(6, 8);
            _glitchText.AddThemeColorOverride("font_color", GLITCH_PURPLE);
            _glitchText.AddThemeFontSizeOverride("font_size", 14);
            _visual.AddChild(_glitchText);
            
            // Partículas de corrupción
            var particles = new CpuParticles2D();
            particles.Emitting = true;
            particles.Amount = 4;
            particles.Lifetime = 0.3f;
            particles.SpeedScale = 3;
            particles.EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle;
            particles.EmissionRectExtents = new Vector2(20, 20);
            particles.Direction = new Vector2(0, 0);
            particles.Spread = 180;
            particles.InitialVelocityMin = 20;
            particles.InitialVelocityMax = 50;
            particles.ScaleAmountMin = 1;
            particles.ScaleAmountMax = 3;
            particles.Color = new Color(GLITCH_PURPLE, 0.7f);
            AddChild(particles);

            // Collision
            var collision = new CollisionShape2D();
            var shape = new RectangleShape2D();
            shape.Size = new Vector2(36, 36);
            collision.Shape = shape;
            AddChild(collision);
        }

        public override void _Process(double delta)
        {
            // Mover hacia abajo
            Position += new Vector2(0, Speed * (float)delta);
            
            // Efecto glitch en el texto
            _glitchTimer += (float)delta;
            if (_glitchTimer > 0.15f)
            {
                _glitchTimer = 0;
                var rand = new System.Random();
                string glitch = "";
                for (int i = 0; i < 3; i++)
                {
                    glitch += _glitchChars[rand.Next(_glitchChars.Length)];
                }
                _glitchText.Text = glitch;
                
                // Alternar color
                _glitchText.AddThemeColorOverride("font_color", 
                    rand.NextDouble() > 0.7 ? GLITCH_GREEN : GLITCH_PURPLE);
            }

            // Destruir si sale de la pantalla
            if (Position.Y > 1000)
            {
                QueueFree();
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.Name == "Player" || body is Player)
            {
                if (body.HasMethod("TakeDamage"))
                {
                    // Usar reflection o interfaz si es posible, por ahora llamada directa
                    body.Call("TakeDamage", Damage, 0); // 0 = Physical/Collision
                }
                QueueFree(); // El obstáculo se destruye al impactar
            }
        }
    }
}
