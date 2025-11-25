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
        
        // Bullet Hell UI
        private Label _grazeLabel;
        private Label _bulletCountLabel;
        private Label _difficultyLabel;
        private Label _waveTimerLabel; // Timer de oleada
        private Label _livesLabel; // Contador de vidas
        private Label _encryptionBurstLabel; // Encryption Bursts (panic button)
        private Label _firewallLabel; // Firewall Mode (resistencia)
        
        // Pause Menu Buttons
        private Button _resumeButton;
        private Button _menuButton;
        private int _selectedPauseOption = 0;

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
            // CRÍTICO: El HUD debe funcionar durante la pausa
            ProcessMode = ProcessModeEnum.Always;
            
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
            
            // BULLET HELL STATS (abajo izquierda)
            CreateBulletHellStats();
            
            // WAVE TIMER (arriba centro, debajo del score)
            CreateWaveTimer();
            
            // LIVES INDICATOR (arriba izquierda, debajo de wave panel)
            CreateLivesIndicator();
            
            // ADAPTIVE DIFFICULTY INDICATORS (abajo derecha)
            CreateAdaptiveIndicators();

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

        private void CreateBulletHellStats()
        {
            // Panel de estadísticas bullet hell (abajo izquierda)
            var bhPanel = CreateTerminalPanel();
            bhPanel.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
            bhPanel.OffsetLeft = 15;
            bhPanel.OffsetTop = -90;
            bhPanel.OffsetRight = 180;
            bhPanel.OffsetBottom = -15;
            AddChild(bhPanel);

            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetLeft = 10;
            content.OffsetTop = 6;
            content.OffsetRight = -10;
            content.OffsetBottom = -6;
            content.AddThemeConstantOverride("separation", 3);
            bhPanel.AddChild(content);

            // Graze counter
            var grazeContainer = new HBoxContainer();
            grazeContainer.AddThemeConstantOverride("separation", 5);
            content.AddChild(grazeContainer);

            var grazeIcon = new Label();
            grazeIcon.Text = "⚡";
            grazeIcon.AddThemeFontSizeOverride("font_size", 14);
            grazeContainer.AddChild(grazeIcon);

            _grazeLabel = new Label();
            _grazeLabel.Text = "GRAZE: 0";
            _grazeLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _grazeLabel.AddThemeFontSizeOverride("font_size", 14);
            grazeContainer.AddChild(_grazeLabel);

            // Bullet count
            _bulletCountLabel = new Label();
            _bulletCountLabel.Text = "BULLETS: 0";
            _bulletCountLabel.AddThemeColorOverride("font_color", ALERT_RED);
            _bulletCountLabel.AddThemeFontSizeOverride("font_size", 12);
            content.AddChild(_bulletCountLabel);

            // Difficulty
            _difficultyLabel = new Label();
            _difficultyLabel.Text = "DIFF: x1.0";
            _difficultyLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _difficultyLabel.AddThemeFontSizeOverride("font_size", 12);
            content.AddChild(_difficultyLabel);
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
        
        private void CreateWaveTimer()
        {
            // Timer de oleada - Centro superior (debajo del score)
            _waveTimerLabel = new Label();
            _waveTimerLabel.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
            _waveTimerLabel.OffsetTop = 130;
            _waveTimerLabel.OffsetLeft = -100;
            _waveTimerLabel.OffsetRight = 100;
            _waveTimerLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _waveTimerLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _waveTimerLabel.AddThemeFontSizeOverride("font_size", 20);
            _waveTimerLabel.Text = "";
            _waveTimerLabel.Visible = false;
            AddChild(_waveTimerLabel);
        }
        
        private void CreateLivesIndicator()
        {
            // Indicador de vidas (debajo del wave panel)
            _livesLabel = new Label();
            _livesLabel.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            _livesLabel.OffsetTop = 105;
            _livesLabel.OffsetLeft = 20;
            _livesLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
            _livesLabel.AddThemeFontSizeOverride("font_size", 18);
            _livesLabel.Text = "♥♥♥";
            AddChild(_livesLabel);
        }
        
        private void CreateAdaptiveIndicators()
        {
            // Panel para indicadores de sistema adaptativo (abajo derecha)
            var adaptPanel = CreateTerminalPanel();
            adaptPanel.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
            adaptPanel.OffsetLeft = -200;
            adaptPanel.OffsetTop = -110;
            adaptPanel.OffsetRight = -15;
            adaptPanel.OffsetBottom = -15;
            AddChild(adaptPanel);

            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetLeft = 10;
            content.OffsetTop = 6;
            content.OffsetRight = -10;
            content.OffsetBottom = -6;
            content.AddThemeConstantOverride("separation", 4);
            adaptPanel.AddChild(content);

            // Header
            var headerLabel = new Label();
            headerLabel.Text = "🛡️ DEFENSES";
            headerLabel.HorizontalAlignment = HorizontalAlignment.Center;
            headerLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
            headerLabel.AddThemeFontSizeOverride("font_size", 12);
            content.AddChild(headerLabel);

            // Encryption Burst (panic button)
            var burstContainer = new HBoxContainer();
            burstContainer.AddThemeConstantOverride("separation", 5);
            content.AddChild(burstContainer);

            var burstIcon = new Label();
            burstIcon.Text = "🔐";
            burstIcon.AddThemeFontSizeOverride("font_size", 14);
            burstContainer.AddChild(burstIcon);

            _encryptionBurstLabel = new Label();
            _encryptionBurstLabel.Text = "BURST: ●●○";
            _encryptionBurstLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
            _encryptionBurstLabel.AddThemeFontSizeOverride("font_size", 14);
            burstContainer.AddChild(_encryptionBurstLabel);

            // Hint de tecla
            var burstHint = new Label();
            burstHint.Text = "[TAB/Q]";
            burstHint.AddThemeColorOverride("font_color", TERMINAL_DIM);
            burstHint.AddThemeFontSizeOverride("font_size", 10);
            burstContainer.AddChild(burstHint);

            // Firewall Mode
            var fwContainer = new HBoxContainer();
            fwContainer.AddThemeConstantOverride("separation", 5);
            content.AddChild(fwContainer);

            var fwIcon = new Label();
            fwIcon.Text = "🔥";
            fwIcon.AddThemeFontSizeOverride("font_size", 14);
            fwContainer.AddChild(fwIcon);

            _firewallLabel = new Label();
            _firewallLabel.Text = "FIREWALL: 0%";
            _firewallLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            _firewallLabel.AddThemeFontSizeOverride("font_size", 14);
            fwContainer.AddChild(_firewallLabel);
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
            // ═══════════════════════════════════════════════════════════════════
            // MENÚ DE PAUSA - Rediseño UX completo
            // 
            // SIMULACIÓN DE USUARIO:
            // 1. Usuario presiona ESC → Aparece menú de pausa
            // 2. Usuario ve botones claros y clickeables
            // 3. Usuario puede usar MOUSE o TECLADO (flechas + Enter)
            // 4. Feedback visual al hover y selección
            // 5. Atajos de teclado visibles pero secundarios
            // ═══════════════════════════════════════════════════════════════════
            
            _pausePanel = new Panel();
            _pausePanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _pausePanel.CustomMinimumSize = new Vector2(420, 340);
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
            // CRÍTICO: El panel de pausa DEBE procesar durante la pausa
            _pausePanel.ProcessMode = ProcessModeEnum.Always;
            AddChild(_pausePanel);

            // Header estilo terminal
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

            // Contenido principal
            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetTop = 55;
            content.OffsetLeft = 40;
            content.OffsetRight = -40;
            content.OffsetBottom = -25;
            content.AddThemeConstantOverride("separation", 15);
            _pausePanel.AddChild(content);

            // Título
            var title = new Label();
            title.Text = "▌▌ PAUSA";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
            title.AddThemeFontSizeOverride("font_size", 28);
            content.AddChild(title);

            // Separador
            var separator = new ColorRect();
            separator.CustomMinimumSize = new Vector2(0, 2);
            separator.Color = new Color(RIPPIER_PURPLE, 0.3f);
            content.AddChild(separator);

            // Contenedor de botones
            var buttonContainer = new VBoxContainer();
            buttonContainer.AddThemeConstantOverride("separation", 12);
            buttonContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            content.AddChild(buttonContainer);

            // ═══════════════════════════════════════════════════════════════════
            // BOTÓN REANUDAR (Opción principal - más destacado)
            // ═══════════════════════════════════════════════════════════════════
            _resumeButton = CreatePauseButton("▶  REANUDAR", TERMINAL_GREEN, true);
            _resumeButton.Pressed += OnResumePressed;
            buttonContainer.AddChild(_resumeButton);

            // ═══════════════════════════════════════════════════════════════════
            // BOTÓN MENÚ PRINCIPAL
            // ═══════════════════════════════════════════════════════════════════
            _menuButton = CreatePauseButton("⌂  MENÚ PRINCIPAL", ALERT_RED, false);
            _menuButton.Pressed += OnMenuPressed;
            buttonContainer.AddChild(_menuButton);

            // Atajos de teclado (información secundaria)
            var shortcutsLabel = new Label();
            shortcutsLabel.Text = "ESC: Reanudar  •  ↑↓: Navegar  •  Enter: Seleccionar";
            shortcutsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            shortcutsLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            shortcutsLabel.AddThemeFontSizeOverride("font_size", 11);
            content.AddChild(shortcutsLabel);
        }

        private Button CreatePauseButton(string text, Color color, bool isPrimary)
        {
            var button = new Button();
            button.Text = text;
            button.CustomMinimumSize = new Vector2(0, isPrimary ? 50 : 42);
            button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            button.FocusMode = Control.FocusModeEnum.All;

            // Estilo normal
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = isPrimary ? new Color(color, 0.15f) : new Color(0.1f, 0.1f, 0.1f, 0.8f);
            normalStyle.BorderColor = color;
            normalStyle.SetBorderWidthAll(isPrimary ? 2 : 1);
            normalStyle.SetCornerRadiusAll(6);
            button.AddThemeStyleboxOverride("normal", normalStyle);

            // Estilo hover
            var hoverStyle = new StyleBoxFlat();
            hoverStyle.BgColor = new Color(color, 0.25f);
            hoverStyle.BorderColor = color;
            hoverStyle.SetBorderWidthAll(2);
            hoverStyle.SetCornerRadiusAll(6);
            button.AddThemeStyleboxOverride("hover", hoverStyle);

            // Estilo presionado
            var pressedStyle = new StyleBoxFlat();
            pressedStyle.BgColor = new Color(color, 0.4f);
            pressedStyle.BorderColor = color;
            pressedStyle.SetBorderWidthAll(2);
            pressedStyle.SetCornerRadiusAll(6);
            button.AddThemeStyleboxOverride("pressed", pressedStyle);

            // Estilo focus (navegación con teclado)
            var focusStyle = new StyleBoxFlat();
            focusStyle.BgColor = new Color(color, 0.2f);
            focusStyle.BorderColor = Colors.White;
            focusStyle.SetBorderWidthAll(2);
            focusStyle.SetCornerRadiusAll(6);
            button.AddThemeStyleboxOverride("focus", focusStyle);

            // Texto
            button.AddThemeColorOverride("font_color", color);
            button.AddThemeColorOverride("font_hover_color", Colors.White);
            button.AddThemeColorOverride("font_pressed_color", Colors.White);
            button.AddThemeColorOverride("font_focus_color", Colors.White);
            button.AddThemeFontSizeOverride("font_size", isPrimary ? 20 : 16);
            
            // CRÍTICO: Los botones deben procesar durante la pausa
            button.ProcessMode = ProcessModeEnum.Always;
            button.MouseFilter = Control.MouseFilterEnum.Stop;

            return button;
        }

        private void OnResumePressed()
        {
            GD.Print("[Pause] Reanudando juego...");
            GetTree().Paused = false;
            _pausePanel.Visible = false;
            GameEventBus.Instance?.EmitGameStateChanged(GameState.Playing);
        }

        private void OnMenuPressed()
        {
            GD.Print("[Pause] Volviendo al menú...");
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }

        private void ShowPauseMenu()
        {
            _pausePanel.Visible = true;
            _selectedPauseOption = 0;
            
            // Dar foco al primer botón para navegación con teclado
            _resumeButton?.GrabFocus();
        }

        private void HidePauseMenu()
        {
            _pausePanel.Visible = false;
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
            // Soporte para oleadas infinitas
            if (InfiniteWaveSystem.Instance != null)
            {
                int wave = InfiniteWaveSystem.Instance.GetCurrentWave();
                _waveLabel.Text = $"WAVE {wave} ∞";
                _waveLabel.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
                
                int remaining = InfiniteWaveSystem.Instance.GetEnemiesRemaining();
                _enemiesRemaining = remaining;
                
                if (remaining > 0)
                {
                    _enemiesLabel.Text = $"THREATS: {remaining}";
                    _enemyProgressBar.Value = 100; // Siempre mostrar algo
                }
                else
                {
                    _enemiesLabel.Text = "NEXT WAVE...";
                }
                
                // Color según dificultad
                float diff = InfiniteWaveSystem.Instance.GetDifficulty();
                if (diff >= 2.0f)
                {
                    _waveLabel.AddThemeColorOverride("font_color", ALERT_RED);
                }
                else if (diff >= 1.5f)
                {
                    _waveLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
                }
            }
            else
            {
                // Sistema de waves legacy
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
            // Tips ahora manejados por NonIntrusiveNotificationSystem
            // GameEventBus.Instance.OnSecurityTipShown += ShowTip;
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
            GD.Print($"[GameHUD] OnHealthChanged recibido: {health}");
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
            if (newState == GameState.Paused)
            {
                ShowPauseMenu();
            }
            else
            {
                HidePauseMenu();
            }
            
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
            
            // Actualizar sistema de oleadas infinitas
            if (InfiniteWaveSystem.Instance != null)
            {
                int remaining = InfiniteWaveSystem.Instance.GetEnemiesRemaining();
                if (remaining != _enemiesRemaining)
                {
                    _enemiesRemaining = remaining;
                    UpdateWaveDisplay();
                }
                
                // Actualizar timer de oleada
                UpdateWaveTimerDisplay();
            }
            else if (WaveSystem.Instance != null)
            {
                int remaining = WaveSystem.Instance.GetEnemiesRemaining();
                if (remaining != _enemiesRemaining)
                {
                    _enemiesRemaining = remaining;
                    UpdateWaveDisplay();
                }
            }
            
            // Actualizar stats de bullet hell
            UpdateBulletHellStats();
            
            // Actualizar vidas
            UpdateLivesDisplay();
            
            // Actualizar sistema adaptativo
            UpdateAdaptiveDisplay();
        }
        
        private void UpdateWaveTimerDisplay()
        {
            if (_waveTimerLabel == null || InfiniteWaveSystem.Instance == null) return;
            
            if (InfiniteWaveSystem.Instance.IsWaveActive())
            {
                float timeRemaining = InfiniteWaveSystem.Instance.GetWaveTimeRemaining();
                float timeLimit = InfiniteWaveSystem.Instance.GetWaveTimeLimit();
                bool timedOut = InfiniteWaveSystem.Instance.IsWaveTimedOut();
                
                _waveTimerLabel.Visible = true;
                
                int secs = (int)timeRemaining;
                
                // BALANCE UX: Solo mostrar timer cuando importa
                // No estresar al jugador con un contador constante
                if (timedOut)
                {
                    _waveTimerLabel.AddThemeColorOverride("font_color", ALERT_RED);
                    _waveTimerLabel.Text = "⚠️ OVERTIME";
                    _waveTimerLabel.Visible = ((int)(_timer * 3)) % 2 == 0;
                }
                else if (timeRemaining < 15)
                {
                    // Últimos 15 segundos: mostrar con urgencia
                    _waveTimerLabel.AddThemeColorOverride("font_color", ALERT_RED);
                    _waveTimerLabel.Text = $"⏱ {secs}s";
                    
                    // Parpadeo más rápido en últimos 5 segundos
                    if (timeRemaining < 5)
                    {
                        _waveTimerLabel.Visible = ((int)(_timer * 6)) % 2 == 0;
                    }
                    else
                    {
                        _waveTimerLabel.Visible = ((int)(_timer * 3)) % 2 == 0;
                    }
                }
                else if (timeRemaining < 25)
                {
                    // 15-25 segundos: aviso amarillo
                    _waveTimerLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
                    _waveTimerLabel.Text = $"⏱ {secs}s";
                    _waveTimerLabel.Visible = true;
                }
                else
                {
                    // Más de 25 segundos: ocultar para no distraer
                    _waveTimerLabel.Visible = false;
                }
            }
            else
            {
                _waveTimerLabel.Visible = false;
            }
        }
        
        private int _lastLivesCount = -1; // Para detectar cambios
        
        private void UpdateLivesDisplay()
        {
            if (_livesLabel == null || GameManager.Instance == null) return;
            
            int lives = GameManager.Instance.Lives;
            
            // Detectar si ganamos o perdimos una vida
            if (_lastLivesCount >= 0 && lives != _lastLivesCount)
            {
                if (lives > _lastLivesCount)
                {
                    // Ganamos vida - efecto positivo
                    AnimateLifeGain();
                }
                else
                {
                    // Perdimos vida - efecto negativo
                    AnimateLifeLoss();
                }
            }
            _lastLivesCount = lives;
            
            // Mostrar corazones según vidas (máximo visual: 6)
            string hearts = "";
            int displayLives = Mathf.Min(lives, 6);
            for (int i = 0; i < displayLives; i++)
            {
                hearts += "♥";
            }
            
            // Si tiene más de 6, mostrar número
            if (lives > 6)
            {
                hearts = $"♥×{lives}";
            }
            
            // Corazones vacíos hasta 4 (vidas base)
            if (lives < 4)
            {
                for (int i = lives; i < 4; i++)
                {
                    hearts += "♡";
                }
            }
            
            _livesLabel.Text = hearts;
            
            // Color según vidas restantes
            if (lives <= 1)
            {
                _livesLabel.AddThemeColorOverride("font_color", ALERT_RED);
            }
            else if (lives <= 2)
            {
                _livesLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            }
            else
            {
                _livesLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
            }
        }
        
        private void AnimateLifeGain()
        {
            if (_livesLabel == null) return;
            
            // Flash verde y escala
            var tween = CreateTween();
            tween.TweenProperty(_livesLabel, "modulate", new Color(0, 2, 0.5f), 0.1f);
            tween.TweenProperty(_livesLabel, "modulate", Colors.White, 0.3f);
            
            var scaleTween = CreateTween();
            scaleTween.TweenProperty(_livesLabel, "scale", new Vector2(1.3f, 1.3f), 0.15f);
            scaleTween.TweenProperty(_livesLabel, "scale", Vector2.One, 0.2f);
        }
        
        private void AnimateLifeLoss()
        {
            if (_livesLabel == null) return;
            
            // Flash rojo y shake
            var tween = CreateTween();
            tween.TweenProperty(_livesLabel, "modulate", new Color(2, 0, 0), 0.1f);
            tween.TweenProperty(_livesLabel, "modulate", Colors.White, 0.2f);
        }
        
        private void UpdateAdaptiveDisplay()
        {
            if (AdaptiveDifficultySystem.Instance == null) return;
            
            // Encryption Bursts
            if (_encryptionBurstLabel != null)
            {
                int bursts = AdaptiveDifficultySystem.Instance.EncryptionBursts;
                string burstDots = "";
                for (int i = 0; i < 3; i++)
                {
                    burstDots += i < bursts ? "●" : "○";
                }
                _encryptionBurstLabel.Text = $"BURST: {burstDots}";
                
                // Color según disponibilidad
                if (bursts == 0)
                {
                    _encryptionBurstLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
                }
                else if (bursts == 1)
                {
                    _encryptionBurstLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
                }
                else
                {
                    _encryptionBurstLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
                }
            }
            
            // Firewall Mode
            if (_firewallLabel != null)
            {
                float resistance = AdaptiveDifficultySystem.Instance.FirewallResistance;
                _firewallLabel.Text = $"FIREWALL: {(int)resistance}%";
                
                // Color según nivel de protección
                if (resistance >= 30)
                {
                    _firewallLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
                }
                else if (resistance >= 10)
                {
                    _firewallLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
                }
                else
                {
                    _firewallLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
                }
            }
        }
        
        private void UpdateBulletHellStats()
        {
            // Graze
            if (_grazeLabel != null && GrazingSystem.Instance != null)
            {
                int graze = GrazingSystem.Instance.GetSessionGrazeCount();
                _grazeLabel.Text = $"GRAZE: {graze}";
                
                // Color según cantidad de graze
                if (graze >= 100)
                    _grazeLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
                else if (graze >= 50)
                    _grazeLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
                else
                    _grazeLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            }
            
            // Bullets on screen
            if (_bulletCountLabel != null && BulletHellSystem.Instance != null)
            {
                int bullets = BulletHellSystem.Instance.GetActiveBulletCount();
                _bulletCountLabel.Text = $"BULLETS: {bullets}";
                
                // Color según peligro
                if (bullets > 50)
                    _bulletCountLabel.AddThemeColorOverride("font_color", ALERT_RED);
                else if (bullets > 20)
                    _bulletCountLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
                else
                    _bulletCountLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            }
            
            // Difficulty
            if (_difficultyLabel != null)
            {
                float diff = BulletHellSystem.Instance?.GetDifficultyMultiplier() ?? 1.0f;
                _difficultyLabel.Text = $"DIFF: x{diff:F1}";
                
                if (diff >= 2.0f)
                    _difficultyLabel.AddThemeColorOverride("font_color", ALERT_RED);
                else if (diff >= 1.5f)
                    _difficultyLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
                else
                    _difficultyLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            }
        }

        public override void _Input(InputEvent @event)
        {
            // ═══════════════════════════════════════════════════════════════════
            // MANEJO DE INPUT DEL MENÚ DE PAUSA
            // 
            // UX: El usuario puede interactuar de múltiples formas:
            // 1. MOUSE: Click en botones (más intuitivo)
            // 2. TECLADO: Flechas + Enter (para gamers)
            // 3. ATAJOS: ESC=reanudar, R=reiniciar, M=menú (para expertos)
            // ═══════════════════════════════════════════════════════════════════
            
            if (!_pausePanel.Visible) return;
            
            if (@event is InputEventKey key && key.Pressed)
            {
                switch (key.Keycode)
                {
                    // ESC siempre reanuda (más intuitivo)
                    case Key.Escape:
                        OnResumePressed();
                        GetViewport().SetInputAsHandled();
                        break;
                        
                    // Atajos de teclado (mantener compatibilidad)
                    case Key.M:
                        OnMenuPressed();
                        GetViewport().SetInputAsHandled();
                        break;
                        
                    // Navegación con flechas
                    case Key.Up:
                    case Key.W:
                        NavigatePauseMenu(-1);
                        GetViewport().SetInputAsHandled();
                        break;
                        
                    case Key.Down:
                    case Key.S:
                        NavigatePauseMenu(1);
                        GetViewport().SetInputAsHandled();
                        break;
                        
                    // Enter/Space selecciona la opción actual
                    case Key.Enter:
                    case Key.KpEnter:
                        ActivateSelectedPauseOption();
                        GetViewport().SetInputAsHandled();
                        break;
                }
            }
        }

        private void NavigatePauseMenu(int direction)
        {
            _selectedPauseOption = (_selectedPauseOption + direction + 2) % 2;
            
            // Dar foco al botón correspondiente
            switch (_selectedPauseOption)
            {
                case 0:
                    _resumeButton?.GrabFocus();
                    break;
                case 1:
                    _menuButton?.GrabFocus();
                    break;
            }
        }

        private void ActivateSelectedPauseOption()
        {
            switch (_selectedPauseOption)
            {
                case 0:
                    OnResumePressed();
                    break;
                case 1:
                    OnMenuPressed();
                    break;
            }
        }

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnScoreChanged -= OnScoreChanged;
            GameEventBus.Instance.OnPlayerHealthChanged -= OnHealthChanged;
            // Tips ahora manejados por NonIntrusiveNotificationSystem
            // GameEventBus.Instance.OnSecurityTipShown -= ShowTip;
            GameEventBus.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameEventBus.Instance.OnEnemyDefeated -= OnEnemyDefeated;
            GameEventBus.Instance.OnWaveAnnounced -= OnWaveAnnounced;
        }
    }
}
