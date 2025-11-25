using Godot;
using System;
using CyberSecurityGame.Core;

namespace CyberSecurityGame.UI
{
    /// <summary>
    /// Leaderboard con estética web: Terminal window, CRT scanlines
    /// - Instantáneo, tabla visible de inmediato
    /// - Colores coordinados con la web
    /// </summary>
    public partial class Leaderboard : Control
    {
        // Colores exactos de la web
        private static readonly Color BG_COLOR = new Color("#050505");
        private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
        private static readonly Color TERMINAL_DIM = new Color("#008F11");
        private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
        private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
        private static readonly Color GOLD = new Color("#ffd700");
        private static readonly Color SILVER = new Color("#c0c0c0");
        private static readonly Color BRONZE = new Color("#cd7f32");

        private Panel _terminalWindow;
        private Label _tableLabel;
        private HBoxContainer _buttonsContainer;
        private Button _backButton;
        private Button _clearButton;
        private ColorRect _scanlines;
        private bool _waitingConfirm = false;
        private float _timer = 0f;

        public override void _Ready()
        {
            if (HighScoreSystem.Instance == null)
            {
                var highScoreSystem = new HighScoreSystem();
                highScoreSystem.Name = "HighScoreSystem";
                GetTree().Root.AddChild(highScoreSystem);
            }

            CreateWebStyleUI();
            _backButton.GrabFocus();
        }

        private void CreateWebStyleUI()
        {
            // Fondo
            var bg = new ColorRect();
            bg.Color = BG_COLOR;
            bg.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(bg);

            // Terminal Window
            _terminalWindow = new Panel();
            _terminalWindow.SetAnchorsPreset(LayoutPreset.Center);
            _terminalWindow.CustomMinimumSize = new Vector2(650, 520);
            _terminalWindow.GrowHorizontal = GrowDirection.Both;
            _terminalWindow.GrowVertical = GrowDirection.Both;
            
            var terminalStyle = new StyleBoxFlat();
            terminalStyle.BgColor = new Color(0, 0, 0, 0.95f);
            terminalStyle.BorderColor = FLUX_ORANGE;
            terminalStyle.SetBorderWidthAll(1);
            terminalStyle.SetCornerRadiusAll(5);
            terminalStyle.ShadowColor = new Color(FLUX_ORANGE, 0.2f);
            terminalStyle.ShadowSize = 15;
            _terminalWindow.AddThemeStyleboxOverride("panel", terminalStyle);
            AddChild(_terminalWindow);

            // Terminal Header
            var headerPanel = new Panel();
            headerPanel.SetAnchorsPreset(LayoutPreset.TopWide);
            headerPanel.OffsetBottom = 35;
            var headerStyle = new StyleBoxFlat();
            headerStyle.BgColor = new Color("#1a1a1a");
            headerPanel.AddThemeStyleboxOverride("panel", headerStyle);
            _terminalWindow.AddChild(headerPanel);

            // Dots
            var dotsContainer = new HBoxContainer();
            dotsContainer.Position = new Vector2(15, 12);
            dotsContainer.AddThemeConstantOverride("separation", 8);
            headerPanel.AddChild(dotsContainer);
            
            foreach (var color in new[] { "#ff5f56", "#ffbd2e", "#27c93f" })
            {
                var dot = new ColorRect();
                dot.CustomMinimumSize = new Vector2(12, 12);
                dot.Color = new Color(color);
                dotsContainer.AddChild(dot);
            }

            var headerTitle = new Label();
            headerTitle.Text = "hall_of_defenders.db";
            headerTitle.Position = new Vector2(80, 8);
            headerTitle.AddThemeColorOverride("font_color", new Color("#666666"));
            headerTitle.AddThemeFontSizeOverride("font_size", 14);
            headerPanel.AddChild(headerTitle);

            // Content
            var content = new VBoxContainer();
            content.SetAnchorsPreset(LayoutPreset.FullRect);
            content.OffsetTop = 45;
            content.OffsetLeft = 25;
            content.OffsetRight = -25;
            content.OffsetBottom = -20;
            content.AddThemeConstantOverride("separation", 15);
            _terminalWindow.AddChild(content);

            // Título
            var titleLabel = new Label();
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            titleLabel.AddThemeFontSizeOverride("font_size", 22);
            titleLabel.Text = ">>> HALL OF DEFENDERS <<<";
            content.AddChild(titleLabel);

            // Tabla
            _tableLabel = new Label();
            _tableLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _tableLabel.AddThemeFontSizeOverride("font_size", 14);
            _tableLabel.Text = GetFormattedLeaderboard();
            content.AddChild(_tableLabel);

            // Stats
            var statsLabel = new Label();
            statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            statsLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            statsLabel.AddThemeFontSizeOverride("font_size", 14);
            int count = HighScoreSystem.Instance?.HighScores.Count ?? 0;
            int topScore = HighScoreSystem.Instance?.GetTopScore() ?? 0;
            statsLabel.Text = count > 0 
                ? $"Total Defenders: {count}  |  Top Score: {topScore:N0}"
                : "No records yet. Be the first defender!";
            content.AddChild(statsLabel);

            // Botones
            _buttonsContainer = new HBoxContainer();
            _buttonsContainer.AddThemeConstantOverride("separation", 30);
            _buttonsContainer.Alignment = BoxContainer.AlignmentMode.Center;
            content.AddChild(_buttonsContainer);

            _backButton = CreateTerminalButton("[ESC] BACK_TO_MENU", "Volver");
            _backButton.Pressed += OnBackPressed;
            _buttonsContainer.AddChild(_backButton);

            _clearButton = CreateTerminalButton("[C] CLEAR_DATA", "Borrar todo");
            _clearButton.AddThemeColorOverride("font_color", new Color("#ff6666"));
            _clearButton.Pressed += OnClearPressed;
            _buttonsContainer.AddChild(_clearButton);

            // Scanlines
            _scanlines = new ColorRect();
            _scanlines.SetAnchorsPreset(LayoutPreset.FullRect);
            _scanlines.MouseFilter = MouseFilterEnum.Ignore;
            _scanlines.Color = new Color(1, 0.7f, 0, 0.02f);
            AddChild(_scanlines);
        }

        private Button CreateTerminalButton(string text, string tooltip)
        {
            var button = new Button();
            button.Text = "  " + text;
            button.TooltipText = tooltip;
            button.CustomMinimumSize = new Vector2(220, 40);
            button.FocusMode = FocusModeEnum.All;
            button.Alignment = HorizontalAlignment.Left;
            
            button.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            button.AddThemeColorOverride("font_hover_color", BG_COLOR);
            button.AddThemeColorOverride("font_focus_color", BG_COLOR);
            button.AddThemeFontSizeOverride("font_size", 16);
            
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = new Color(0, 0, 0, 0);
            button.AddThemeStyleboxOverride("normal", normalStyle);
            
            var focusStyle = new StyleBoxFlat();
            focusStyle.BgColor = TERMINAL_GREEN;
            button.AddThemeStyleboxOverride("hover", focusStyle);
            button.AddThemeStyleboxOverride("focus", focusStyle);
            button.AddThemeStyleboxOverride("pressed", focusStyle);
            
            return button;
        }

        private string GetFormattedLeaderboard()
        {
            var scores = HighScoreSystem.Instance?.HighScores;
            
            if (scores == null || scores.Count == 0)
            {
                return "\n       ─────────────────────────────────────\n" +
                       "       │                                   │\n" +
                       "       │    No records in database yet.    │\n" +
                       "       │    Play to become a defender!     │\n" +
                       "       │                                   │\n" +
                       "       ─────────────────────────────────────\n";
            }

            string output = "\n  ┌─────┬────────────────┬────────────┬────────┐\n";
            output +=         "  │ RNK │    CALLSIGN    │   SCORE    │  WAVE  │\n";
            output +=         "  ├─────┼────────────────┼────────────┼────────┤\n";
            
            for (int i = 0; i < scores.Count && i < 10; i++)
            {
                var score = scores[i];
                
                string medal = i switch
                {
                    0 => "★",
                    1 => "☆",
                    2 => "◆",
                    _ => " "
                };
                
                string pos = $"{medal}{i + 1,2}";
                string name = score.PlayerName.PadRight(14);
                string pts = score.Score.ToString("N0").PadLeft(10);
                string wave = score.Wave.ToString().PadLeft(5);
                
                output += $"  │ {pos} │ {name} │ {pts} │ {wave}  │\n";
            }
            
            output += "  └─────┴────────────────┴────────────┴────────┘\n";
            return output;
        }

        public override void _Process(double delta)
        {
            _timer += (float)delta;
            float alpha = 0.015f + 0.01f * Mathf.Sin(_timer * 4);
            _scanlines.Color = new Color(1, 0.7f, 0, alpha);
        }

        private void OnBackPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }

        private void OnClearPressed()
        {
            if (!_waitingConfirm)
            {
                _clearButton.Text = "  [C] CONFIRM?";
                _waitingConfirm = true;
            }
            else
            {
                HighScoreSystem.Instance?.ClearAllScores();
                GetTree().CreateTimer(0.5f).Timeout += () => GetTree().ReloadCurrentScene();
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Escape: OnBackPressed(); break;
                    case Key.C: OnClearPressed(); break;
                }
            }
        }
    }
}
