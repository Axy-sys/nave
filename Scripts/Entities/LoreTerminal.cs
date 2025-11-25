using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Entities
{
    /// <summary>
    /// Terminal de lore - Archivos desencriptables estilo hacker
    /// </summary>
    public partial class LoreTerminal : Area2D
    {
        [Export] public string Title = "Archivo Encriptado";
        [Export(PropertyHint.MultilineText)] public string Content = "Contenido del archivo...";
        [Export] public int SecurityLevel = 1;
        
        // Colores web
        private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
        private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
        private static readonly Color ALERT_RED = new Color("#ff5555");
        private static readonly Color DARK_BG = new Color("#0a0a0a");

        private bool _isPlayerNearby = false;
        private bool _isDecrypted = false;
        private Panel _terminal;
        private Label _label;
        private Label _icon;

        public override void _Ready()
        {
            // Collision
            var shape = new CollisionShape2D();
            var rect = new RectangleShape2D();
            rect.Size = new Vector2(50, 65);
            shape.Shape = rect;
            AddChild(shape);

            // ═══ VISUAL: Mini terminal ═══
            _terminal = new Panel();
            _terminal.Size = new Vector2(55, 60);
            _terminal.Position = new Vector2(-27, -30);
            
            var style = new StyleBoxFlat();
            style.BgColor = DARK_BG;
            style.BorderColor = ALERT_RED;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(3);
            _terminal.AddThemeStyleboxOverride("panel", style);
            AddChild(_terminal);
            
            // Icono de archivo
            _icon = new Label();
            _icon.Text = "📁";
            _icon.Position = new Vector2(15, 5);
            _icon.AddThemeFontSizeOverride("font_size", 22);
            _terminal.AddChild(_icon);
            
            // Texto de estado
            var statusLabel = new Label();
            statusLabel.Text = "LOCKED";
            statusLabel.Position = new Vector2(5, 38);
            statusLabel.AddThemeColorOverride("font_color", ALERT_RED);
            statusLabel.AddThemeFontSizeOverride("font_size", 10);
            _terminal.AddChild(statusLabel);

            // Label externo
            _label = new Label();
            _label.Text = "🔒 FILE";
            _label.Position = new Vector2(-25, -50);
            _label.AddThemeColorOverride("font_color", ALERT_RED);
            _label.AddThemeFontSizeOverride("font_size", 11);
            AddChild(_label);

            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        public override void _Process(double delta)
        {
            if (_isDecrypted) return;

            if (_isPlayerNearby && Input.IsActionPressed("interact"))
            {
                Decrypt();
            }
        }

        private void Decrypt()
        {
            _isDecrypted = true;
            
            // Cambiar visual
            var style = new StyleBoxFlat();
            style.BgColor = DARK_BG;
            style.BorderColor = TERMINAL_GREEN;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(3);
            _terminal.AddThemeStyleboxOverride("panel", style);
            
            _icon.Text = "📂";
            _label.Text = "✓ READ";
            _label.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            
            // Actualizar status interno
            var statusLabel = _terminal.GetChild<Label>(1);
            if (statusLabel != null)
            {
                statusLabel.Text = "OPEN";
                statusLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            }
            
            // Efecto visual
            var tween = CreateTween();
            tween.TweenProperty(_terminal, "scale", new Vector2(1.1f, 1.1f), 0.1f);
            tween.TweenProperty(_terminal, "scale", Vector2.One, 0.1f);

            // Mostrar contenido
            GameEventBus.Instance.EmitSecurityTipShown($"[{Title}] {Content}");
            
            GD.Print($"📂 Lore Terminal Decrypted: {Title}");
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.Name == "Player" || body is Player)
            {
                _isPlayerNearby = true;
                if (!_isDecrypted)
                {
                    _label.Text = "[E] READ";
                    _label.AddThemeColorOverride("font_color", FLUX_ORANGE);
                }
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.Name == "Player" || body is Player)
            {
                _isPlayerNearby = false;
                if (!_isDecrypted)
                {
                    _label.Text = "🔒 FILE";
                    _label.AddThemeColorOverride("font_color", ALERT_RED);
                }
            }
        }
    }
}
