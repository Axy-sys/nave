using Godot;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Core;
using CyberSecurityGame.UI;
using CyberSecurityGame.Systems;

namespace CyberSecurityGame.Views
{
    public partial class GameHUD : CanvasLayer
    {
        // Colores
        private static readonly Color BG_COLOR = new Color("#050505");
        private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
        private static readonly Color TERMINAL_DIM = new Color("#008F11");
        private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
        private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
        private static readonly Color ALERT_RED = new Color("#ff5555");
        private static readonly Color CYAN_GLOW = new Color("#00d4ff");
        private static readonly Color HEALTH_HIGH = new Color("#00ff41");
        private static readonly Color HEALTH_MED = new Color("#ffaa00");
        private static readonly Color HEALTH_LOW = new Color("#ff5555");

        // UI Elements
        private Label _scoreLabel;
        private Label _scoreValueLabel;
        private Label _highScoreLabel;
        private Label _newRecordLabel;
        private Control _multiplierContainer;
        private Label _multiplierLabel;
        private Panel _wavePanel;
        private Label _waveLabel;
        private Label _waveDescLabel;
        private ProgressBar _enemyProgressBar;
        private Label _enemiesLabel;
        private Panel _statusPanel;
        private Label _healthLabel;
        private ProgressBar _healthBar;
        private Label _weaponLabel;
        private Label _tipLabel;
        private Panel _pausePanel;
        private GameOverScreen _gameOverScreen;
        private ColorRect _scanlines;
        
        // Leaderboard durante juego
        private Panel _leaderboardMini;
        private Label[] _topScoreLabels = new Label[3];

        // State
        private int _currentScore = 0;
        private int _displayedScore = 0;
        private int _highScore = 0;
        private int _currentWave = 1;
        private int _totalWaves = 3;
        private float _currentHealth = 100f;
        private int _currentMultiplier = 1;
        private int _enemiesRemaining = 0;
        private int _enemiesTotal = 0;
        private bool _isNewRecord = false;
        private float _timer = 0f;

        public override void _Ready()
        {
            CreateArcadeHUD();
            CreatePausePanel();
            SubscribeToEvents();
            
            if (HighScoreSystem.Instance == null)
            {
                var highScoreSystem = new HighScoreSystem();
                highScoreSystem.Name = "HighScoreSystem";
                GetTree().Root.AddChild(highScoreSystem);
            }
            
            _highScore = HighScoreSystem.Instance?.GetTopScore() ?? 0;
            UpdateAllDisplays();
        }

        private void CreateArcadeHUD()
        {
            // SCORE GIGANTE CENTRAL
            var scoreContainer = new VBoxContainer();
            scoreContainer.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
            scoreContainer.OffsetTop = 10;
            scoreContainer.OffsetLeft = -200;
            scoreContainer.OffsetRight = 200;
            scoreContainer.AddThemeConstantOverride("separation", 0);
            AddChild(scoreContainer);

            _scoreLabel = new Label();
            _scoreLabel.Text = "SCORE";
            _scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _scoreLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            _scoreLabel.AddThemeFontSizeOverride("font_size", 14);
            scoreContainer.AddChild(_scoreLabel);

            _scoreValueLabel = new Label();
            _scoreValueLabel.Text = "0";
            _scoreValueLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _scoreValueLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _scoreValueLabel.AddThemeFontSizeOverride("font_size", 48);
            scoreContainer.AddChild(_scoreValueLabel);

            _highScoreLabel = new Label();
            _highScoreLabel.Text = "BEST: 0";
            _highScoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _highScoreLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _highScoreLabel.AddThemeFontSizeOverride("font_size", 16);
            scoreContainer.AddChild(_highScoreLabel);

            _newRecordLabel = new Label();
            _newRecordLabel.Text = "* NEW RECORD! *";
            _newRecordLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _newRecordLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _newRecordLabel.AddThemeFontSizeOverride("font_size", 20);
            _newRecordLabel.Visible = false;
            scoreContainer.AddChild(_newRecordLabel);

            // MULTIPLICADOR
            _multiplierContainer = new Control();
            _multiplierContainer.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
            _multiplierContainer.OffsetTop = 110;
            _multiplierContainer.OffsetLeft = -100;
            _multiplierContainer.OffsetRight = 100;
            _multiplierContainer.OffsetBottom = 160;
            _multiplierContainer.Visible = false;
            AddChild(_multiplierContainer);

            _multiplierLabel = new Label();
            _multiplierLabel.Text = "x2";
            _multiplierLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _multiplierLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _multiplierLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _multiplierLabel.AddThemeFontSizeOverride("font_size", 36);
            _multiplierContainer.AddChild(_multiplierLabel);

            // WAVE PANEL (izquierda)
            _wavePanel = CreateTerminalPanel();
            _wavePanel.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            _wavePanel.OffsetLeft = 15;
            _wavePanel.OffsetTop = 15;
            _wavePanel.OffsetRight = 220;
            _wavePanel.OffsetBottom = 100;
            AddChild(_wavePanel);

            var waveContent = new VBoxContainer();
            waveContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            waveContent.OffsetLeft = 12;
            waveContent.OffsetTop = 8;
            waveContent.OffsetRight = -12;
            waveContent.OffsetBottom = -8;
            waveContent.AddThemeConstantOverride("separation", 4);
            _wavePanel.AddChild(waveContent);

            var waveHeader = new HBoxContainer();
            waveHeader.AddThemeConstantOverride("separation", 10);
            waveContent.AddChild(waveHeader);

            _waveLabel = new Label();
            _waveLabel.Text = "WAVE 1/3";
            _waveLabel.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
            _waveLabel.AddThemeFontSizeOverride("font_size", 18);
            waveHeader.AddChild(_waveLabel);

            _waveDescLabel = new Label();
            _waveDescLabel.Text = "SURFACE WEB";
            _waveDescLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            _waveDescLabel.AddThemeFontSizeOverride("font_size", 12);
            waveHeader.AddChild(_waveDescLabel);

            _enemiesLabel = new Label();
            _enemiesLabel.Text = "THREATS: 0/0";
            _enemiesLabel.AddThemeColorOverride("font_color", ALERT_RED);
            _enemiesLabel.AddThemeFontSizeOverride("font_size", 16);
            waveContent.AddChild(_enemiesLabel);

            _enemyProgressBar = new ProgressBar();
            _enemyProgressBar.CustomMinimumSize = new Vector2(0, 12);
            _enemyProgressBar.ShowPercentage = false;
            _enemyProgressBar.MaxValue = 100;
            _enemyProgressBar.Value = 100;

            var enemyBgStyle = new StyleBoxFlat();
            enemyBgStyle.BgColor = new Color(0.15f, 0.05f, 0.05f);
            enemyBgStyle.SetCornerRadiusAll(3);
            _enemyProgressBar.AddThemeStyleboxOverride("background", enemyBgStyle);

            var enemyFillStyle = new StyleBoxFlat();
            enemyFillStyle.BgColor = ALERT_RED;
            enemyFillStyle.SetCornerRadiusAll(3);
            _enemyProgressBar.AddThemeStyleboxOverride("fill", enemyFillStyle);
            waveContent.AddChild(_enemyProgressBar);

            // STATUS PANEL (derecha)
            _statusPanel = CreateTerminalPanel();
            _statusPanel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _statusPanel.OffsetLeft = -220;
            _statusPanel.OffsetTop = 15;
            _statusPanel.OffsetRight = -15;
            _statusPanel.OffsetBottom = 100;
            AddChild(_statusPanel);

            var statusContent = new VBoxContainer();
            statusContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            statusContent.OffsetLeft = 12;
            statusContent.OffsetTop = 8;
            statusContent.OffsetRight = -12;
            statusContent.OffsetBottom = -8;
            statusContent.AddThemeConstantOverride("separation", 4);
            _statusPanel.AddChild(statusContent);

            _healthLabel = new Label();
            _healthLabel.Text = "INTEGRITY: 100%";
            _healthLabel.AddThemeColorOverride("font_color", HEALTH_HIGH);
            _healthLabel.AddThemeFontSizeOverride("font_size", 16);
            statusContent.AddChild(_healthLabel);

            _healthBar = new ProgressBar();
            _healthBar.CustomMinimumSize = new Vector2(0, 16);
            _healthBar.ShowPercentage = false;
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;

            var healthBgStyle = new StyleBoxFlat();
            healthBgStyle.BgColor = new Color(0.1f, 0.1f, 0.1f);
            healthBgStyle.SetCornerRadiusAll(3);
            _healthBar.AddThemeStyleboxOverride("background", healthBgStyle);

            var healthFillStyle = new StyleBoxFlat();
            healthFillStyle.BgColor = HEALTH_HIGH;
            healthFillStyle.SetCornerRadiusAll(3);
            _healthBar.AddThemeStyleboxOverride("fill", healthFillStyle);
            statusContent.AddChild(_healthBar);

            _weaponLabel = new Label();
            _weaponLabel.Text = "[1] FIREWALL    [2] ANTIVIRUS";
            _weaponLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
            _weaponLabel.AddThemeFontSizeOverride("font_size", 11);
            statusContent.AddChild(_weaponLabel);

            // TIPS
            _tipLabel = new Label();
            _tipLabel.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
            _tipLabel.OffsetLeft = -400;
            _tipLabel.OffsetTop = -80;
            _tipLabel.OffsetRight = 400;
            _tipLabel.OffsetBottom = -20;
            _tipLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _tipLabel.VerticalAlignment = VerticalAlignment.Center;
            _tipLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _tipLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            _tipLabel.AddThemeFontSizeOverride("font_size", 14);
            _tipLabel.Visible = false;
            AddChild(_tipLabel);

            // GAME OVER
            _gameOverScreen = new GameOverScreen();
            _gameOverScreen.Name = "GameOverScreen";
            AddChild(_gameOverScreen);

            // MINI LEADERBOARD (Top 3 - esquina derecha)
            CreateMiniLeaderboard();

            // SCANLINES
            _scanlines = new ColorRect();
            _scanlines.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _scanlines.MouseFilter = Control.MouseFilterEnum.Ignore;
            _scanlines.Color = new Color(0, 1, 0.3f, 0.01f);
            AddChild(_scanlines);
        }

        private Panel CreateTerminalPanel()
        {
            var panel = new Panel();
            var style = new StyleBoxFlat();
            style.BgColor = new Color(BG_COLOR, 0.85f);
            style.BorderColor = TERMINAL_DIM;
            style.SetBorderWidthAll(1);
            style.SetCornerRadiusAll(4);
            panel.AddThemeStyleboxOverride("panel", style);
            return panel;
        }

        private void CreateMiniLeaderboard()
        {
            // Panel pequeño con top 3 scores visible durante el juego
            _leaderboardMini = CreateTerminalPanel();
            _leaderboardMini.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _leaderboardMini.OffsetLeft = -160;
            _leaderboardMini.OffsetTop = 110; // Debajo del panel de status
            _leaderboardMini.OffsetRight = -15;
            _leaderboardMini.OffsetBottom = 200;
            AddChild(_leaderboardMini);

            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetLeft = 10;
            content.OffsetTop = 6;
            content.OffsetRight = -10;
            content.OffsetBottom = -6;
            content.AddThemeConstantOverride("separation", 2);
            _leaderboardMini.AddChild(content);

            // Header
            var headerLabel = new Label();
            headerLabel.Text = "🏆 TOP 3";
            headerLabel.HorizontalAlignment = HorizontalAlignment.Center;
            headerLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            headerLabel.AddThemeFontSizeOverride("font_size", 12);
            content.AddChild(headerLabel);

            // Separador
            var separator = new ColorRect();
            separator.CustomMinimumSize = new Vector2(0, 1);
            separator.Color = new Color(FLUX_ORANGE, 0.3f);
            content.AddChild(separator);

            // Top 3 scores
            for (int i = 0; i < 3; i++)
            {
                var scoreRow = new HBoxContainer();
                scoreRow.AddThemeConstantOverride("separation", 8);
                content.AddChild(scoreRow);

                var rankLabel = new Label();
                rankLabel.Text = (i + 1).ToString() + ".";
                rankLabel.CustomMinimumSize = new Vector2(20, 0);
                rankLabel.AddThemeColorOverride("font_color", GetRankColor(i));
                rankLabel.AddThemeFontSizeOverride("font_size", 14);
                scoreRow.AddChild(rankLabel);

                _topScoreLabels[i] = new Label();
                _topScoreLabels[i].Text = "---";
                _topScoreLabels[i].SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                _topScoreLabels[i].HorizontalAlignment = HorizontalAlignment.Right;
                _topScoreLabels[i].AddThemeColorOverride("font_color", GetRankColor(i));
                _topScoreLabels[i].AddThemeFontSizeOverride("font_size", 14);
                scoreRow.AddChild(_topScoreLabels[i]);
            }

            // Cargar scores iniciales
            UpdateMiniLeaderboard();
        }

        private Color GetRankColor(int rank)
        {
            return rank switch
            {
                0 => FLUX_ORANGE,      // Oro
                1 => new Color("#c0c0c0"),  // Plata
                2 => new Color("#cd7f32"),  // Bronce
                _ => TERMINAL_DIM
            };
        }

        private void UpdateMiniLeaderboard()
        {
            if (HighScoreSystem.Instance == null) return;

            var highScores = HighScoreSystem.Instance.HighScores;
            for (int i = 0; i < 3; i++)
            {
                if (i < highScores.Count && highScores[i].Score > 0)
                {
                    _topScoreLabels[i].Text = FormatScoreCompact(highScores[i].Score);
                    
                    // Highlight si el score actual está cerca o supera este rank
                    if (_currentScore > 0 && _currentScore >= highScores[i].Score)
                    {
                        _topScoreLabels[i].AddThemeColorOverride("font_color", TERMINAL_GREEN);
                    }
                    else
                    {
                        _topScoreLabels[i].AddThemeColorOverride("font_color", GetRankColor(i));
                    }
                }
                else
                {
                    _topScoreLabels[i].Text = "---";
                    _topScoreLabels[i].AddThemeColorOverride("font_color", TERMINAL_DIM);
                }
            }
        }

        private string FormatScoreCompact(int score)
        {
            if (score >= 1000000) return (score / 1000000f).ToString("0.#") + "M";
            if (score >= 1000) return (score / 1000f).ToString("0.#") + "K";
            return score.ToString();
        }

        private void CreatePausePanel()
        {
            _pausePanel = new Panel();
            _pausePanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _pausePanel.CustomMinimumSize = new Vector2(400, 300);
            _pausePanel.GrowHorizontal = Control.GrowDirection.Both;
            _pausePanel.GrowVertical = Control.GrowDirection.Both;
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(BG_COLOR, 0.98f);
            style.BorderColor = RIPPIER_PURPLE;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            style.ShadowColor = new Color(RIPPIER_PURPLE, 0.3f);
            style.ShadowSize = 15;
            _pausePanel.AddThemeStyleboxOverride("panel", style);
            _pausePanel.Visible = false;
            AddChild(_pausePanel);

            var header = new Panel();
            header.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            header.OffsetBottom = 40;
            var headerStyle = new StyleBoxFlat();
            headerStyle.BgColor = new Color("#0d0d0d");
            header.AddThemeStyleboxOverride("panel", headerStyle);
            _pausePanel.AddChild(header);

            var dots = new HBoxContainer();
            dots.Position = new Vector2(15, 12);
            dots.AddThemeConstantOverride("separation", 8);
            header.AddChild(dots);
            foreach (var c in new[] { "#ff5f56", "#ffbd2e", "#27c93f" })
            {
                var dot = new ColorRect();
                dot.CustomMinimumSize = new Vector2(12, 12);
                dot.Color = new Color(c);
                dots.AddChild(dot);
            }

            var headerTitle = new Label();
            headerTitle.Text = "  system_paused.exe";
            headerTitle.AddThemeColorOverride("font_color", new Color("#666666"));
            headerTitle.AddThemeFontSizeOverride("font_size", 14);
            dots.AddChild(headerTitle);

            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetTop = 50;
            content.OffsetLeft = 30;
            content.OffsetRight = -30;
            content.OffsetBottom = -20;
            content.AddThemeConstantOverride("separation", 20);
            _pausePanel.AddChild(content);

            var title = new Label();
            title.Text = ">>> SYSTEM PAUSED <<<";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
            title.AddThemeFontSizeOverride("font_size", 24);
            content.AddChild(title);

            var options = new Label();
            options.Text = "[ESC]  Resume mission\n[R]    Restart mission\n[M]    Return to menu";
            options.HorizontalAlignment = HorizontalAlignment.Center;
            options.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            options.AddThemeFontSizeOverride("font_size", 18);
            content.AddChild(options);
        }

        private void UpdateAllDisplays()
        {
            UpdateScoreDisplay();
            UpdateHealthDisplay();
            UpdateWaveDisplay();
            UpdateMultiplierDisplay();
        }

        private void UpdateScoreDisplay()
        {
            _highScoreLabel.Text = $"BEST: {_highScore:N0}";
            
            if (_currentScore > _highScore && _currentScore > 0 && !_isNewRecord)
            {
                _isNewRecord = true;
                ShowNewRecordEffect();
            }
        }

        private void AnimateScoreChange()
        {
            var tween = CreateTween();
            tween.TweenMethod(
                Callable.From<int>((value) => {
                    _displayedScore = value;
                    _scoreValueLabel.Text = value.ToString("N0");
                }),
                _displayedScore,
                _currentScore,
                0.3f
            ).SetEase(Tween.EaseType.Out);

            var scaleTween = CreateTween();
            scaleTween.TweenProperty(_scoreValueLabel, "scale", new Vector2(1.15f, 1.15f), 0.1f);
            scaleTween.TweenProperty(_scoreValueLabel, "scale", Vector2.One, 0.15f);
            
            if (_currentScore - _displayedScore >= 50)
            {
                var colorTween = CreateTween();
                colorTween.TweenProperty(_scoreValueLabel, "modulate", new Color(1.5f, 1.5f, 1.5f), 0.1f);
                colorTween.TweenProperty(_scoreValueLabel, "modulate", Colors.White, 0.2f);
            }
        }

        private void ShowNewRecordEffect()
        {
            _newRecordLabel.Visible = true;
            _scoreValueLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            
            var tween = CreateTween();
            tween.SetLoops();
            tween.TweenProperty(_newRecordLabel, "modulate:a", 0.5f, 0.5f);
            tween.TweenProperty(_newRecordLabel, "modulate:a", 1.0f, 0.5f);
        }

        private void UpdateHealthDisplay()
        {
            int healthPercent = (int)_currentHealth;
            _healthLabel.Text = $"INTEGRITY: {healthPercent}%";
            _healthBar.Value = _currentHealth;

            Color healthColor;
            if (_currentHealth > 60) healthColor = HEALTH_HIGH;
            else if (_currentHealth > 30) healthColor = HEALTH_MED;
            else healthColor = HEALTH_LOW;

            _healthLabel.AddThemeColorOverride("font_color", healthColor);
            
            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = healthColor;
            fillStyle.SetCornerRadiusAll(3);
            _healthBar.AddThemeStyleboxOverride("fill", fillStyle);
        }

        private void UpdateWaveDisplay()
        {
            _waveLabel.Text = $"WAVE {_currentWave}/{_totalWaves}";
            
            if (_enemiesTotal > 0)
            {
                _enemiesLabel.Text = $"THREATS: {_enemiesRemaining} remaining";
                
                float progress = (float)_enemiesRemaining / _enemiesTotal * 100f;
                _enemyProgressBar.Value = progress;
                
                Color barColor = ALERT_RED;
                if (_enemiesRemaining <= 2 && _enemiesRemaining > 0)
                {
                    barColor = FLUX_ORANGE;
                    _enemiesLabel.Text = $"THREATS: {_enemiesRemaining} - ALMOST CLEAR!";
                }
                else if (_enemiesRemaining == 0)
                {
                    barColor = TERMINAL_GREEN;
                    _enemiesLabel.Text = "THREATS: CLEARED";
                }

                var fillStyle = new StyleBoxFlat();
                fillStyle.BgColor = barColor;
                fillStyle.SetCornerRadiusAll(3);
                _enemyProgressBar.AddThemeStyleboxOverride("fill", fillStyle);
            }
            else
            {
                _enemiesLabel.Text = "THREATS: Scanning...";
                _enemyProgressBar.Value = 0;
            }
        }

        private void UpdateMultiplierDisplay()
        {
            if (_currentMultiplier > 1)
            {
                _multiplierContainer.Visible = true;
                _multiplierLabel.Text = $"x{_currentMultiplier}";
                
                Color multColor = _currentMultiplier switch
                {
                    2 => FLUX_ORANGE,
                    3 => new Color("#ff6600"),
                    4 => ALERT_RED,
                    _ => RIPPIER_PURPLE
                };
                _multiplierLabel.AddThemeColorOverride("font_color", multColor);
            }
            else
            {
                _multiplierContainer.Visible = false;
            }
        }

        private void SubscribeToEvents()
        {
            GameEventBus.Instance.OnScoreChanged += OnScoreChanged;
            GameEventBus.Instance.OnPlayerHealthChanged += OnHealthChanged;
            GameEventBus.Instance.OnSecurityTipShown += ShowTip;
            GameEventBus.Instance.OnGameStateChanged += OnGameStateChanged;
            GameEventBus.Instance.OnEnemyDefeated += OnEnemyDefeated;
            GameEventBus.Instance.OnWaveAnnounced += OnWaveAnnounced;
        }

        private void OnScoreChanged(int score)
        {
            _currentScore = score;
            if (score > _highScore) _highScore = score;
            AnimateScoreChange();
            UpdateScoreDisplay();
            UpdateMiniLeaderboard(); // Actualizar posición en leaderboard
        }

        private void OnHealthChanged(float health)
        {
            _currentHealth = health;
            UpdateHealthDisplay();
            
            if (health <= 25 && health > 0)
            {
                FlashHealthWarning();
            }
        }

        private void FlashHealthWarning()
        {
            var tween = CreateTween();
            tween.TweenProperty(_healthBar, "modulate", new Color(2, 1, 1), 0.1f);
            tween.TweenProperty(_healthBar, "modulate", Colors.White, 0.1f);
        }

        private void OnWaveAnnounced(int wave, string title, string desc)
        {
            _currentWave = wave;
            _waveDescLabel.Text = title.ToUpper();
            
            _enemiesTotal = 3 + (wave - 1) * 2;
            _enemiesRemaining = _enemiesTotal;
            
            UpdateWaveDisplay();
            ShowWaveAnnouncement(wave, title);
        }

        private void ShowWaveAnnouncement(int wave, string title)
        {
            var announcement = new Label();
            announcement.Text = $"WAVE {wave}\n{title}";
            announcement.SetAnchorsPreset(Control.LayoutPreset.Center);
            announcement.HorizontalAlignment = HorizontalAlignment.Center;
            announcement.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
            announcement.AddThemeFontSizeOverride("font_size", 32);
            announcement.Modulate = new Color(1, 1, 1, 0);
            announcement.Scale = new Vector2(0.8f, 0.8f);
            AddChild(announcement);

            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(announcement, "modulate:a", 1.0f, 0.2f);
            tween.TweenProperty(announcement, "scale", new Vector2(1.1f, 1.1f), 0.2f);
            tween.Chain().TweenProperty(announcement, "scale", Vector2.One, 0.1f);
            tween.Chain().TweenInterval(1.5f);
            tween.Chain().TweenProperty(announcement, "modulate:a", 0.0f, 0.3f);
            tween.Chain().TweenCallback(Callable.From(announcement.QueueFree));
        }

        private void OnEnemyDefeated(string enemyType, int points)
        {
            _enemiesRemaining = Mathf.Max(0, _enemiesRemaining - 1);
            
            if (GameJuiceSystem.Instance != null)
            {
                _currentMultiplier = GameJuiceSystem.Instance.CurrentMultiplier;
            }
            
            UpdateWaveDisplay();
            UpdateMultiplierDisplay();
        }

        private void OnGameStateChanged(GameState newState)
        {
            _pausePanel.Visible = newState == GameState.Paused;
            
            if (newState == GameState.GameOver)
            {
                _gameOverScreen.Show(_currentScore, _currentWave, 1);
            }
        }

        private void ShowTip(string tip)
        {
            _tipLabel.Text = tip;
            _tipLabel.Visible = true;
            _tipLabel.Modulate = new Color(1, 1, 1, 0);
            
            var tween = CreateTween();
            tween.TweenProperty(_tipLabel, "modulate:a", 1.0f, 0.3f);
            tween.TweenInterval(4.0f);
            tween.TweenProperty(_tipLabel, "modulate:a", 0.0f, 0.5f);
            tween.TweenCallback(Callable.From(() => _tipLabel.Visible = false));
        }

        public override void _Process(double delta)
        {
            _timer += (float)delta;
            float alpha = 0.008f + 0.004f * Mathf.Sin(_timer * 2);
            _scanlines.Color = new Color(0, 1, 0.3f, alpha);
            
            if (WaveSystem.Instance != null)
            {
                int remaining = WaveSystem.Instance.GetEnemiesRemaining();
                if (remaining != _enemiesRemaining)
                {
                    _enemiesRemaining = remaining;
                    UpdateWaveDisplay();
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (_pausePanel.Visible && @event is InputEventKey key && key.Pressed)
            {
                switch (key.Keycode)
                {
                    case Key.M:
                        GetTree().Paused = false;
                        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
                        break;
                    case Key.R:
                        GetTree().Paused = false;
                        GetTree().ReloadCurrentScene();
                        break;
                }
            }
        }

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnScoreChanged -= OnScoreChanged;
            GameEventBus.Instance.OnPlayerHealthChanged -= OnHealthChanged;
            GameEventBus.Instance.OnSecurityTipShown -= ShowTip;
            GameEventBus.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEventBus.Instance.OnEnemyDefeated -= OnEnemyDefeated;
            GameEventBus.Instance.OnWaveAnnounced -= OnWaveAnnounced;
        }
    }
}
