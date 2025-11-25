using Godot;
using System;
using CyberSecurityGame.Core;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.UI
{
    /// <summary>
    /// Game Over con estética web: Terminal window, CRT scanlines, glitch effects
    /// - Instantáneo para arcade feel
    /// - Colores coordinados con la web
    /// </summary>
    public partial class GameOverScreen : CanvasLayer
    {
        // Colores exactos de la web
        private static readonly Color BG_COLOR = new Color("#050505");
        private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
        private static readonly Color TERMINAL_DIM = new Color("#008F11");
        private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
        private static readonly Color ALERT_RED = new Color("#ff0000");
        private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");

        private Panel _terminalWindow;
        private Label _titleLabel;
        private Label _statsLabel;
        private Label _highScoreNotice;
        private HBoxContainer _inputContainer;
        private LineEdit _nameInput;
        private VBoxContainer _buttonsContainer;
        private Button _retryButton;
        private Button _leaderboardButton;
        private Button _menuButton;
        private ColorRect _scanlines;

        private int _finalScore;
        private int _finalWave;
        private int _finalLevel;
        private bool _scoreSubmitted = false;
        private bool _isHighScore = false;
        private float _timer = 0f;
        private Random _rng = new Random();

        public override void _Ready()
        {
            Layer = 100;
            Visible = false;
            CreateWebStyleUI();
            GameEventBus.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver) Show();
        }

        private void CreateWebStyleUI()
        {
            // Fondo oscuro
            var bg = new ColorRect();
            bg.Color = new Color(BG_COLOR, 0.95f);
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(bg);

            // Terminal Window
            _terminalWindow = new Panel();
            _terminalWindow.SetAnchorsPreset(Control.LayoutPreset.Center);
            _terminalWindow.CustomMinimumSize = new Vector2(550, 480);
            _terminalWindow.GrowHorizontal = Control.GrowDirection.Both;
            _terminalWindow.GrowVertical = Control.GrowDirection.Both;
            
            var terminalStyle = new StyleBoxFlat();
            terminalStyle.BgColor = new Color(0, 0, 0, 0.95f);
            terminalStyle.BorderColor = ALERT_RED;
            terminalStyle.SetBorderWidthAll(2);
            terminalStyle.SetCornerRadiusAll(5);
            terminalStyle.ShadowColor = new Color(ALERT_RED, 0.3f);
            terminalStyle.ShadowSize = 20;
            _terminalWindow.AddThemeStyleboxOverride("panel", terminalStyle);
            AddChild(_terminalWindow);

            // Terminal Header
            var headerPanel = new Panel();
            headerPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
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
            headerTitle.Text = "system_breach.log";
            headerTitle.Position = new Vector2(80, 8);
            headerTitle.AddThemeColorOverride("font_color", new Color("#666666"));
            headerTitle.AddThemeFontSizeOverride("font_size", 14);
            headerPanel.AddChild(headerTitle);

            // Content
            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetTop = 45;
            content.OffsetLeft = 30;
            content.OffsetRight = -30;
            content.OffsetBottom = -20;
            content.AddThemeConstantOverride("separation", 15);
            _terminalWindow.AddChild(content);

            // Título SYSTEM BREACH
            _titleLabel = new Label();
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeColorOverride("font_color", ALERT_RED);
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _titleLabel.Text = ">>> SYSTEM BREACH <<<";
            content.AddChild(_titleLabel);

            // Stats
            _statsLabel = new Label();
            _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _statsLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _statsLabel.AddThemeFontSizeOverride("font_size", 16);
            content.AddChild(_statsLabel);

            // High score notice
            _highScoreNotice = new Label();
            _highScoreNotice.HorizontalAlignment = HorizontalAlignment.Center;
            _highScoreNotice.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _highScoreNotice.AddThemeFontSizeOverride("font_size", 22);
            _highScoreNotice.Text = "★ NEW HIGH SCORE ★";
            _highScoreNotice.Visible = false;
            content.AddChild(_highScoreNotice);

            // Input para nombre
            _inputContainer = new HBoxContainer();
            _inputContainer.AddThemeConstantOverride("separation", 15);
            _inputContainer.Alignment = BoxContainer.AlignmentMode.Center;
            _inputContainer.Visible = false;
            content.AddChild(_inputContainer);

            var inputLabel = new Label();
            inputLabel.Text = "CALLSIGN:";
            inputLabel.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
            inputLabel.AddThemeFontSizeOverride("font_size", 18);
            _inputContainer.AddChild(inputLabel);

            _nameInput = new LineEdit();
            _nameInput.CustomMinimumSize = new Vector2(180, 45);
            _nameInput.MaxLength = 10;
            _nameInput.PlaceholderText = "ANON";
            _nameInput.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _nameInput.AddThemeFontSizeOverride("font_size", 18);
            
            var inputStyle = new StyleBoxFlat();
            inputStyle.BgColor = new Color(0, 0.05f, 0.02f);
            inputStyle.BorderColor = TERMINAL_GREEN;
            inputStyle.SetBorderWidthAll(2);
            _nameInput.AddThemeStyleboxOverride("normal", inputStyle);
            _nameInput.AddThemeStyleboxOverride("focus", inputStyle);
            _nameInput.TextSubmitted += OnNameSubmitted;
            _inputContainer.AddChild(_nameInput);

            var submitBtn = new Button();
            submitBtn.Text = "SAVE";
            submitBtn.CustomMinimumSize = new Vector2(70, 45);
            submitBtn.AddThemeColorOverride("font_color", BG_COLOR);
            submitBtn.AddThemeFontSizeOverride("font_size", 16);
            var btnStyle = new StyleBoxFlat();
            btnStyle.BgColor = TERMINAL_GREEN;
            submitBtn.AddThemeStyleboxOverride("normal", btnStyle);
            submitBtn.AddThemeStyleboxOverride("hover", btnStyle);
            submitBtn.Pressed += () => OnNameSubmitted(_nameInput.Text);
            _inputContainer.AddChild(submitBtn);

            // Botones
            _buttonsContainer = new VBoxContainer();
            _buttonsContainer.AddThemeConstantOverride("separation", 8);
            _buttonsContainer.Alignment = BoxContainer.AlignmentMode.Center;
            content.AddChild(_buttonsContainer);

            _retryButton = CreateTerminalButton("[R] RETRY_MISSION", "Reintentar");
            _retryButton.Pressed += OnRetryPressed;
            _buttonsContainer.AddChild(_retryButton);

            _leaderboardButton = CreateTerminalButton("[L] VIEW_RANKINGS", "Ver records");
            _leaderboardButton.Pressed += OnLeaderboardPressed;
            _buttonsContainer.AddChild(_leaderboardButton);

            _menuButton = CreateTerminalButton("[ESC] ABORT_TO_MENU", "Volver al menú");
            _menuButton.Pressed += OnMenuPressed;
            _buttonsContainer.AddChild(_menuButton);

            // Scanlines
            _scanlines = new ColorRect();
            _scanlines.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _scanlines.MouseFilter = Control.MouseFilterEnum.Ignore;
            _scanlines.Color = new Color(1, 0, 0, 0.02f);
            AddChild(_scanlines);
        }

        private Button CreateTerminalButton(string text, string tooltip)
        {
            var button = new Button();
            button.Text = "  " + text;
            button.TooltipText = tooltip;
            button.CustomMinimumSize = new Vector2(300, 40);
            button.FocusMode = Control.FocusModeEnum.All;
            button.Alignment = HorizontalAlignment.Left;
            
            button.AddThemeColorOverride("font_color", new Color("#cccccc"));
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

        public void Show(int score = 0, int wave = 0, int level = 0)
        {
            if (score == 0 && GameManager.Instance != null)
            {
                _finalScore = GameManager.Instance.Score;
                _finalLevel = GameManager.Instance.CurrentLevel;
            }
            else
            {
                _finalScore = score;
                _finalLevel = level;
            }
            
            _finalWave = wave > 0 ? wave : (Systems.WaveSystem.Instance?.GetCurrentWave() ?? 1);
            _scoreSubmitted = false;

            Visible = true;
            GetTree().Paused = true;

            int topScore = HighScoreSystem.Instance?.GetTopScore() ?? 0;
            _statsLabel.Text = $"─────────────────────────────────\n" +
                              $"  FINAL_SCORE:    {_finalScore:N0}\n" +
                              $"  WAVE_REACHED:   {_finalWave}\n" +
                              $"  SYSTEM_RECORD:  {topScore:N0}\n" +
                              $"─────────────────────────────────";

            _isHighScore = HighScoreSystem.Instance?.IsHighScore(_finalScore) ?? false;
            
            if (_isHighScore)
            {
                _highScoreNotice.Visible = true;
                _inputContainer.Visible = true;
                _nameInput.GrabFocus();
            }
            else
            {
                _highScoreNotice.Visible = false;
                _inputContainer.Visible = false;
                _retryButton.GrabFocus();
            }
        }

        public override void _Process(double delta)
        {
            if (!Visible) return;
            _timer += (float)delta;
            
            // Glitch en el título
            if (_rng.NextDouble() < 0.03)
            {
                _titleLabel.Position = new Vector2((float)(_rng.NextDouble() - 0.5) * 4, 0);
            }
            else
            {
                _titleLabel.Position = Vector2.Zero;
            }
            
            // Scanlines rojos
            float alpha = 0.02f + 0.01f * Mathf.Sin(_timer * 6);
            _scanlines.Color = new Color(1, 0, 0, alpha);
        }

        private void OnNameSubmitted(string name)
        {
            if (_scoreSubmitted) return;
            _scoreSubmitted = true;

            string playerName = string.IsNullOrWhiteSpace(name) ? "ANON" : name.ToUpper();
            HighScoreSystem.Instance?.TryAddScore(playerName, _finalScore, _finalWave, _finalLevel);

            _inputContainer.Visible = false;
            _highScoreNotice.Text = $"★ Saved: {playerName} ★";
            _retryButton.GrabFocus();
        }

        private void OnRetryPressed()
        {
            if (_isHighScore && !_scoreSubmitted) OnNameSubmitted(_nameInput.Text);
            GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        }

        private void OnLeaderboardPressed()
        {
            if (_isHighScore && !_scoreSubmitted) OnNameSubmitted(_nameInput.Text);
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/Leaderboard.tscn");
        }

        private void OnMenuPressed()
        {
            if (_isHighScore && !_scoreSubmitted) OnNameSubmitted(_nameInput.Text);
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;

            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (_inputContainer.Visible && _nameInput.HasFocus())
                {
                    if (keyEvent.Keycode == Key.Escape) OnMenuPressed();
                    return;
                }

                switch (keyEvent.Keycode)
                {
                    case Key.R: OnRetryPressed(); break;
                    case Key.L: OnLeaderboardPressed(); break;
                    case Key.Escape: OnMenuPressed(); break;
                }
            }
        }
    }
}
