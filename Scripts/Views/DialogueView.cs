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
            // Main Container Panel
            var panel = new Panel();
            _container = panel; // Use panel as the main container reference
            
            // Anchor Top Wide (Below Top Bar)
            // This ensures it doesn't cover the player at the bottom
            panel.AnchorLeft = 0;
            panel.AnchorTop = 0;
            panel.AnchorRight = 1;
            panel.AnchorBottom = 0;
            panel.OffsetLeft = 100; // Margin from left
            panel.OffsetTop = 70; // Below Top Bar (60px)
            panel.OffsetRight = -100; // Margin from right
            panel.OffsetBottom = 230; // Height 160px

            // Style
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.02f, 0.02f, 0.02f, 0.95f); // Deep Black
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            style.BorderWidthBottom = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderColor = new Color("bf00ff"); // Rippier Purple
            style.ShadowColor = new Color("bf00ff");
            style.ShadowSize = 10;
            panel.AddThemeStyleboxOverride("panel", style);
            AddChild(panel);

            // Portrait
            _portrait = new TextureRect();
            // Anchor Left Center inside the panel
            _portrait.AnchorLeft = 0;
            _portrait.AnchorTop = 0.5f;
            _portrait.AnchorRight = 0;
            _portrait.AnchorBottom = 0.5f;
            _portrait.OffsetLeft = 30;
            _portrait.OffsetTop = -50; // Half height (100px total)
            _portrait.OffsetRight = 130; // Width 100
            _portrait.OffsetBottom = 50;
            
            _portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            var image = Image.CreateEmpty(100, 100, false, Image.Format.Rgba8);
            image.Fill(new Color("bf00ff")); // Default Purple
            var texture = ImageTexture.CreateFromImage(image);
            _portrait.Texture = texture;
            panel.AddChild(_portrait);

            // Speaker Label
            _speakerLabel = new Label();
            _speakerLabel.Position = new Vector2(150, 15); // Right of portrait
            _speakerLabel.AddThemeColorOverride("font_color", new Color("00ff41")); // Terminal Green
            _speakerLabel.AddThemeFontSizeOverride("font_size", 24);
            _speakerLabel.Text = "UNKNOWN_SOURCE";
            panel.AddChild(_speakerLabel);

            // Text Label
            _textLabel = new Label();
            // Anchor to fill the rest of the panel
            _textLabel.AnchorLeft = 0;
            _textLabel.AnchorTop = 0;
            _textLabel.AnchorRight = 1;
            _textLabel.AnchorBottom = 1;
            _textLabel.OffsetLeft = 150; // Right of portrait
            _textLabel.OffsetTop = 50; // Below speaker name
            _textLabel.OffsetRight = -30;
            _textLabel.OffsetBottom = -15;
            
            _textLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _textLabel.AddThemeFontSizeOverride("font_size", 20);
            _textLabel.AddThemeColorOverride("font_color", Colors.White);
            _textLabel.Text = "Establishing secure connection...";
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
