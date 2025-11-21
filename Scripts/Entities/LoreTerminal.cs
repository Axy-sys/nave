using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Entities
{
    /// <summary>
    /// Objeto interactuable que revela fragmentos de historia o conceptos educativos profundos.
    /// Fomenta la exploración.
    /// </summary>
    public partial class LoreTerminal : Area2D
    {
        [Export] public string Title = "Archivo Encriptado";
        [Export(PropertyHint.MultilineText)] public string Content = "Contenido del archivo...";
        [Export] public int SecurityLevel = 1; // 1-3

        private bool _isPlayerNearby = false;
        private bool _isDecrypted = false;
        private Label _label;
        private Sprite2D _icon;

        public override void _Ready()
        {
            // Collision
            var shape = new CollisionShape2D();
            var rect = new RectangleShape2D();
            rect.Size = new Vector2(40, 60);
            shape.Shape = rect;
            AddChild(shape);

            // Visuals
            _icon = new Sprite2D();
            // Placeholder texture: A simple folder icon
            var image = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
            image.Fill(new Color(1, 1, 0)); // Yellow folder
            _icon.Texture = ImageTexture.CreateFromImage(image);
            AddChild(_icon);

            _label = new Label();
            _label.Text = "ENCRYPTED";
            _label.Position = new Vector2(-40, -50);
            _label.AddThemeFontSizeOverride("font_size", 10);
            _label.AddThemeColorOverride("font_color", new Color(1, 0, 0));
            AddChild(_label);

            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        public override void _Process(double delta)
        {
            if (_isDecrypted) return;

            if (_isPlayerNearby && Input.IsKeyPressed(Key.E))
            {
                Decrypt();
            }
        }

        private void Decrypt()
        {
            _isDecrypted = true;
            _label.Text = "DECRYPTED";
            _label.AddThemeColorOverride("font_color", new Color(0, 1, 0));
            
            // Visual effect
            var tween = CreateTween();
            tween.TweenProperty(_icon, "scale", new Vector2(1.5f, 1.5f), 0.2f);
            tween.TweenProperty(_icon, "scale", new Vector2(1f, 1f), 0.2f);

            // Emit event to show the lore content
            // We can use the TipSystem or DialogueSystem to show this
            GameEventBus.Instance.EmitSecurityTipShown($"ARCHIVO DESENCRIPTADO: {Title}\n\n{Content}");
            
            GD.Print($"📂 Lore Terminal Decrypted: {Title}");
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.Name == "Player")
            {
                _isPlayerNearby = true;
                if (!_isDecrypted) _label.Text = "[E] DECRYPT";
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.Name == "Player")
            {
                _isPlayerNearby = false;
                if (!_isDecrypted) _label.Text = "ENCRYPTED";
            }
        }
    }
}
