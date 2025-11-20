using Godot;
using CyberSecurityGame.Systems;

namespace CyberSecurityGame.Views
{
    public partial class DialogueView : CanvasLayer
    {
        private Control _container;
        private Label _speakerLabel;
        private Label _textLabel;
        private TextureRect _portrait;
        private AnimationPlayer _animPlayer;

        public override void _Ready()
        {
            // Setup UI programmatically if not using a .tscn (for robustness in this environment)
            SetupUI();

            DialogueSystem.Instance.DialogueStarted += OnDialogueStarted;
            DialogueSystem.Instance.DialogueEnded += OnDialogueEnded;
            
            _container.Visible = false;
        }

        private void SetupUI()
        {
            _container = new Control();
            _container.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
            _container.Position = new Vector2(0, -20); // Slight offset
            AddChild(_container);

            // Panel Background (Bubble style)
            var panel = new Panel();
            panel.CustomMinimumSize = new Vector2(600, 150);
            panel.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
            panel.GrowHorizontal = Control.GrowDirection.Both;
            panel.GrowVertical = Control.GrowDirection.Begin;
            // Simple stylebox
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0, 0, 0, 0.8f);
            style.CornerRadiusTopLeft = 20;
            style.CornerRadiusTopRight = 20;
            style.CornerRadiusBottomLeft = 20;
            style.CornerRadiusBottomRight = 20;
            style.BorderWidthBottom = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderColor = new Color(0, 1, 0); // Hacker Green
            panel.AddThemeStyleboxOverride("panel", style);
            _container.AddChild(panel);

            // Portrait Placeholder
            _portrait = new TextureRect();
            _portrait.Position = new Vector2(20, -80); // Above the box
            _portrait.Size = new Vector2(80, 80);
            _portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            // Create a simple colored texture for the portrait
            var image = Image.CreateEmpty(80, 80, false, Image.Format.Rgba8);
            image.Fill(new Color(0, 1, 0)); // Green for friendly
            var texture = ImageTexture.CreateFromImage(image);
            _portrait.Texture = texture;
            panel.AddChild(_portrait);

            // Speaker Label
            _speakerLabel = new Label();
            _speakerLabel.Position = new Vector2(110, 10); // Shifted right
            _speakerLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
            _speakerLabel.AddThemeFontSizeOverride("font_size", 24);
            _speakerLabel.Text = "SPEAKER";
            panel.AddChild(_speakerLabel);

            // Text Label
            _textLabel = new Label();
            _textLabel.Position = new Vector2(110, 50); // Shifted right
            _textLabel.Size = new Vector2(470, 90);
            _textLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _textLabel.AddThemeFontSizeOverride("font_size", 18);
            _textLabel.Text = "Dialogue text goes here...";
            panel.AddChild(_textLabel);
        }

        private void OnDialogueStarted(string speaker, string text)
        {
            _container.Visible = true;
            _speakerLabel.Text = speaker;
            _textLabel.Text = text;
            
            // Change portrait color based on speaker
            if (_portrait.Texture is ImageTexture imgTex)
            {
                var img = imgTex.GetImage();
                if (speaker == "ELLIOT") img.Fill(new Color(0.1f, 0.1f, 0.1f)); // Black/Dark Grey (Hoodie)
                else if (speaker == "MR. ROBOT") img.Fill(new Color(0.6f, 0.4f, 0.2f)); // Brown/Tan (Jacket)
                else if (speaker == "FSOCIETY") img.Fill(new Color(1, 1, 1)); // White (Mask)
                else img.Fill(new Color(1, 0, 0)); // Red/Unknown
                _portrait.Texture = ImageTexture.CreateFromImage(img);
            }
        }

        private void OnDialogueEnded()
        {
            _container.Visible = false;
        }

        public override void _ExitTree()
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.DialogueStarted -= OnDialogueStarted;
                DialogueSystem.Instance.DialogueEnded -= OnDialogueEnded;
            }
        }
    }
}
