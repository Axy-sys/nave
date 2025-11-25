using Godot;
using System;
using CyberSecurityGame.Core;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Systems;

namespace CyberSecurityGame.UI
{
    /// <summary>
    /// Game Over ESTILO ARCADE RETRO
    /// - Visual tipo CRT/terminal
    /// - Si es high score → muestra entrada de iniciales (3 letras)
    /// - Simple y directo como los 80s
    /// </summary>
    public partial class GameOverScreen : CanvasLayer
    {
        // Colores arcade
        private static readonly Color BG_COLOR = new Color("#050505");
        private static readonly Color ARCADE_GREEN = new Color("#00ff41");
        private static readonly Color ARCADE_AMBER = new Color("#ffaa00");
        private static readonly Color ARCADE_RED = new Color("#ff0044");
        private static readonly Color DIM_GREEN = new Color("#008F11");

        private Panel _terminalWindow;
        private Label _gameOverLabel;
        private Label _statsLabel;
        private Label _highScoreLabel;
        private VBoxContainer _buttonsContainer;
        private Button _retryButton;
        private Button _menuButton;
        private ColorRect _scanlines;

        // Arcade Initials UI
        private ArcadeInitialsUI _arcadeInitialsUI;

        private int _finalScore;
        private int _finalWave;
        private int _finalKills;
        private float _survivalTime;
        private bool _scoreSubmitted = false;
        private float _timer = 0f;
        private Random _rng = new Random();

        public override void _Ready()
        {
            Layer = 100;
            Visible = false;
            
            // Inicializar sistema arcade
            if (ArcadeScoreSystem.Instance == null)
            {
                var arcadeSystem = new ArcadeScoreSystem();
                arcadeSystem.Name = "ArcadeScoreSystem";
                GetTree().Root.AddChild(arcadeSystem);
            }
            
            CreateArcadeUI();
            CreateArcadeInitialsUI();
            
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

        private void CreateArcadeUI()
        {
            // Fondo oscuro
            var bg = new ColorRect();
            bg.Color = new Color(BG_COLOR, 0.95f);
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(bg);

            // Terminal Window
            _terminalWindow = new Panel();
            _terminalWindow.SetAnchorsPreset(Control.LayoutPreset.Center);
            _terminalWindow.CustomMinimumSize = new Vector2(500, 420);
            _terminalWindow.GrowHorizontal = Control.GrowDirection.Both;
            _terminalWindow.GrowVertical = Control.GrowDirection.Both;
            
            var terminalStyle = new StyleBoxFlat();
            terminalStyle.BgColor = new Color(0, 0, 0, 0.98f);
            terminalStyle.BorderColor = ARCADE_RED;
            terminalStyle.SetBorderWidthAll(3);
            terminalStyle.SetCornerRadiusAll(0); // Bordes rectos = retro
            _terminalWindow.AddThemeStyleboxOverride("panel", terminalStyle);
            AddChild(_terminalWindow);

            // Content
            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetTop = 30;
            content.OffsetLeft = 30;
            content.OffsetRight = -30;
            content.OffsetBottom = -30;
            content.AddThemeConstantOverride("separation", 20);
            _terminalWindow.AddChild(content);

            // GAME OVER título
            _gameOverLabel = new Label();
            _gameOverLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _gameOverLabel.AddThemeColorOverride("font_color", ARCADE_RED);
            _gameOverLabel.AddThemeFontSizeOverride("font_size", 36);
            _gameOverLabel.Text = "GAME OVER";
            content.AddChild(_gameOverLabel);

            // Stats
            _statsLabel = new Label();
            _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _statsLabel.AddThemeColorOverride("font_color", ARCADE_GREEN);
            _statsLabel.AddThemeFontSizeOverride("font_size", 18);
            content.AddChild(_statsLabel);

            // High score notice
            _highScoreLabel = new Label();
            _highScoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _highScoreLabel.AddThemeColorOverride("font_color", ARCADE_AMBER);
            _highScoreLabel.AddThemeFontSizeOverride("font_size", 24);
            _highScoreLabel.Visible = false;
            content.AddChild(_highScoreLabel);

            // Botones
            _buttonsContainer = new VBoxContainer();
            _buttonsContainer.AddThemeConstantOverride("separation", 10);
            _buttonsContainer.Alignment = BoxContainer.AlignmentMode.Center;
            content.AddChild(_buttonsContainer);

            _retryButton = CreateArcadeButton("► RETRY [R]");
            _retryButton.Pressed += OnRetryPressed;
            _buttonsContainer.AddChild(_retryButton);

            _menuButton = CreateArcadeButton("► MENU [ESC]");
            _menuButton.Pressed += OnMenuPressed;
            _buttonsContainer.AddChild(_menuButton);

            // Scanlines
            _scanlines = new ColorRect();
            _scanlines.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _scanlines.MouseFilter = Control.MouseFilterEnum.Ignore;
            _scanlines.Color = new Color(1, 0, 0, 0.02f);
            AddChild(_scanlines);
        }

        private void CreateArcadeInitialsUI()
        {
            _arcadeInitialsUI = new ArcadeInitialsUI();
            _arcadeInitialsUI.Name = "ArcadeInitialsUI";
            _arcadeInitialsUI.InitialsEntered += OnInitialsEntered;
            _arcadeInitialsUI.Cancelled += OnInitialsCancelled;
            AddChild(_arcadeInitialsUI);
        }

        private Button CreateArcadeButton(string text)
        {
            var button = new Button();
            button.Text = text;
            button.CustomMinimumSize = new Vector2(250, 45);
            button.FocusMode = Control.FocusModeEnum.All;
            button.Alignment = HorizontalAlignment.Center;
            
            button.AddThemeColorOverride("font_color", ARCADE_GREEN);
            button.AddThemeColorOverride("font_hover_color", BG_COLOR);
            button.AddThemeColorOverride("font_focus_color", BG_COLOR);
            button.AddThemeFontSizeOverride("font_size", 20);
            
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = new Color(0, 0, 0, 0);
            normalStyle.BorderColor = DIM_GREEN;
            normalStyle.SetBorderWidthAll(2);
            button.AddThemeStyleboxOverride("normal", normalStyle);
            
            var focusStyle = new StyleBoxFlat();
            focusStyle.BgColor = ARCADE_GREEN;
            focusStyle.SetBorderWidthAll(0);
            button.AddThemeStyleboxOverride("hover", focusStyle);
            button.AddThemeStyleboxOverride("focus", focusStyle);
            button.AddThemeStyleboxOverride("pressed", focusStyle);
            
            return button;
        }

        public void Show(int score = 0, int wave = 0, int level = 0)
        {
            var infiniteWave = InfiniteWaveSystem.Instance;
            
            // Obtener score
            if (score == 0 && GameManager.Instance != null)
            {
                _finalScore = GameManager.Instance.Score;
            }
            else
            {
                _finalScore = score;
            }
            
            // Obtener wave
            if (infiniteWave != null)
            {
                _finalWave = infiniteWave.GetCurrentWave();
            }
            else if (wave > 0)
            {
                _finalWave = wave;
            }
            else
            {
                _finalWave = WaveSystem.Instance?.GetCurrentWave() ?? 1;
            }
            
            _finalKills = infiniteWave?.GetTotalKills() ?? 0;
            _survivalTime = infiniteWave?.GetSurvivalTime() ?? 0f;
            _scoreSubmitted = false;

            Visible = true;
            GetTree().Paused = true;

            // Mostrar stats
            string timeStr = FormatTime(_survivalTime);
            int topScore = ArcadeScoreSystem.Instance?.GetTopScore() ?? 0;
            
            _statsLabel.Text = $"══════════════════════════\n" +
                              $"   SCORE:  {_finalScore,10:N0}\n" +
                              $"   WAVE:   {_finalWave,10}\n" +
                              $"   KILLS:  {_finalKills,10}\n" +
                              $"   TIME:   {timeStr,10}\n" +
                              $"══════════════════════════\n" +
                              $"   BEST:   {topScore,10:N0}";

            // Verificar si es high score
            bool isHighScore = ArcadeScoreSystem.Instance?.IsHighScore(_finalScore) ?? false;
            int position = ArcadeScoreSystem.Instance?.GetScorePosition(_finalScore) ?? 0;
            
            if (isHighScore && position > 0)
            {
                _highScoreLabel.Text = $"★ NEW HIGH SCORE! RANK #{position} ★";
                _highScoreLabel.Visible = true;
                _buttonsContainer.Visible = false;
                
                // Mostrar entrada de iniciales arcade
                _arcadeInitialsUI.ShowForScore(_finalScore, _finalWave, position);
            }
            else
            {
                _highScoreLabel.Visible = false;
                _buttonsContainer.Visible = true;
                _retryButton.GrabFocus();
            }
        }
        
        private string FormatTime(float seconds)
        {
            int mins = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{mins:D2}:{secs:D2}";
        }

        public override void _Process(double delta)
        {
            if (!Visible) return;
            _timer += (float)delta;
            
            // Glitch en el título
            if (_rng.NextDouble() < 0.03)
            {
                _gameOverLabel.Position = new Vector2((float)(_rng.NextDouble() - 0.5) * 4, 0);
            }
            else
            {
                _gameOverLabel.Position = Vector2.Zero;
            }
            
            // Scanlines
            float alpha = 0.02f + 0.01f * Mathf.Sin(_timer * 6);
            _scanlines.Color = new Color(1, 0, 0, alpha);
        }

        private void OnInitialsEntered(string initials)
        {
            _scoreSubmitted = true;
            _highScoreLabel.Text = $"★ {initials} SAVED! ★";
            _buttonsContainer.Visible = true;
            _retryButton.GrabFocus();
        }

        private void OnInitialsCancelled()
        {
            _buttonsContainer.Visible = true;
            _retryButton.GrabFocus();
        }

        private void OnRetryPressed()
        {
            GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        }

        private void OnMenuPressed()
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            if (_arcadeInitialsUI.Visible) return; // Dejar que arcade UI maneje input

            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.R: OnRetryPressed(); break;
                    case Key.Escape: OnMenuPressed(); break;
                }
            }
        }
    }
}
