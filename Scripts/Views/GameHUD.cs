using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Views
{
	/// <summary>
	/// View del HUD principal (MVC Pattern)
	/// Principio de Single Responsibility: solo maneja la visualización del HUD
	/// </summary>
	public partial class GameHUD : CanvasLayer
	{
		// Referencias a elementos UI
		private Label _scoreLabel;
		private Label _levelLabel;
		private ProgressBar _healthBar;
		private ProgressBar _shieldBar;
		private Label _livesLabel;
		private Label _weaponLabel;
		private VBoxContainer _notificationContainer;
		private Minimap _minimap;
		
		// Paneles de estado
		private Panel _pausePanel;
		private Panel _gameOverPanel;

		public override void _Ready()
		{
			InitializeUI();
			InitializeStatePanels();
			SubscribeToEvents();
		}

		private void InitializeUI()
		{
			// Crear estructura UI programáticamente con mejor diseño
			var viewportSize = GetViewport().GetVisibleRect().Size;

			// 1. TOP LEFT: Score & Level (Data & Layer)
			var infoPanel = new Panel();
			infoPanel.Name = "InfoPanel";
			infoPanel.Position = new Vector2(20, 20);
			infoPanel.Size = new Vector2(200, 80);
			
			var terminalStyle = new StyleBoxFlat();
			terminalStyle.BgColor = new Color(0, 0.05f, 0.1f, 0.9f);
			terminalStyle.BorderColor = new Color(0, 1, 1, 0.5f);
			terminalStyle.SetBorderWidthAll(1);
			terminalStyle.SetCornerRadiusAll(5);
			infoPanel.AddThemeStyleboxOverride("panel", terminalStyle);
			AddChild(infoPanel);
			
			_scoreLabel = new Label();
			_scoreLabel.Position = new Vector2(10, 10);
			_scoreLabel.AddThemeColorOverride("font_color", Colors.White);
			_scoreLabel.AddThemeFontSizeOverride("font_size", 18);
			infoPanel.AddChild(_scoreLabel);

			_levelLabel = new Label();
			_levelLabel.Position = new Vector2(10, 40);
			_levelLabel.AddThemeColorOverride("font_color", new Color(0, 1, 1));
			_levelLabel.AddThemeFontSizeOverride("font_size", 16);
			infoPanel.AddChild(_levelLabel);

			// 2. TOP RIGHT: Lives (Backups)
			var livesPanel = new Panel();
			livesPanel.Position = new Vector2(viewportSize.X - 180, 20);
			livesPanel.Size = new Vector2(160, 50);
			livesPanel.AddThemeStyleboxOverride("panel", terminalStyle);
			AddChild(livesPanel);

			_livesLabel = new Label();
			_livesLabel.Position = new Vector2(10, 12);
			_livesLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			_livesLabel.AddThemeFontSizeOverride("font_size", 18);
			livesPanel.AddChild(_livesLabel);

			// 3. BOTTOM CENTER: Status (Health & Shield)
			var statusPanel = new Panel();
			statusPanel.Position = new Vector2((viewportSize.X - 400) / 2, viewportSize.Y - 80);
			statusPanel.Size = new Vector2(400, 70);
			statusPanel.AddThemeStyleboxOverride("panel", terminalStyle);
			AddChild(statusPanel);

			// Health Bar
			var healthLabel = new Label();
			healthLabel.Text = "INTEGRIDAD";
			healthLabel.Position = new Vector2(10, 5);
			healthLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0.5f));
			healthLabel.AddThemeFontSizeOverride("font_size", 12);
			statusPanel.AddChild(healthLabel);

			_healthBar = new ProgressBar();
			_healthBar.Position = new Vector2(10, 25);
			_healthBar.Size = new Vector2(380, 15);
			_healthBar.ShowPercentage = false;
			
			var healthBg = new StyleBoxFlat { BgColor = new Color(0.1f, 0.1f, 0.1f), CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2 };
			var healthFill = new StyleBoxFlat { BgColor = new Color(0, 1, 0.5f), CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2 };
			_healthBar.AddThemeStyleboxOverride("background", healthBg);
			_healthBar.AddThemeStyleboxOverride("fill", healthFill);
			statusPanel.AddChild(_healthBar);

			// Shield Bar
			_shieldBar = new ProgressBar();
			_shieldBar.Position = new Vector2(10, 45);
			_shieldBar.Size = new Vector2(380, 10);
			_shieldBar.ShowPercentage = false;
			_shieldBar.Visible = false; // Hidden by default
			
			var shieldFill = new StyleBoxFlat { BgColor = new Color(0, 0.7f, 1), CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2, CornerRadiusBottomLeft = 2 };
			_shieldBar.AddThemeStyleboxOverride("background", healthBg);
			_shieldBar.AddThemeStyleboxOverride("fill", shieldFill);
			statusPanel.AddChild(_shieldBar);

			// 4. BOTTOM RIGHT: Weapon
			var weaponPanel = new Panel();
			weaponPanel.Position = new Vector2(viewportSize.X - 220, viewportSize.Y - 70);
			weaponPanel.Size = new Vector2(200, 50);
			weaponPanel.AddThemeStyleboxOverride("panel", terminalStyle);
			AddChild(weaponPanel);
			
			_weaponLabel = new Label();
			_weaponLabel.Position = new Vector2(10, 12);
			_weaponLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0));
			_weaponLabel.AddThemeFontSizeOverride("font_size", 16);
			weaponPanel.AddChild(_weaponLabel);

			// 5. BOTTOM LEFT: Minimap
			_minimap = new Minimap();
			_minimap.Name = "Minimap";
			_minimap.Position = new Vector2(100, viewportSize.Y - 100); // Bottom Left
			_minimap.DetectionRadius = 600f;
			_minimap.MinimapSize = 150f;
			AddChild(_minimap);

			// 6. NOTIFICATION SYSTEM (Toast Style - Top Right, below Lives)
			_notificationContainer = new VBoxContainer();
			_notificationContainer.Name = "NotificationContainer";
			_notificationContainer.Position = new Vector2(viewportSize.X - 320, 80);
			_notificationContainer.Size = new Vector2(300, 0); // Height grows automatically
			_notificationContainer.AddThemeConstantOverride("separation", 10);
			AddChild(_notificationContainer);

			UpdateUI(0, 1, 100, 0, 3, "Firewall");
		}

		private void InitializeStatePanels()
		{
			// Estilo común de terminal
			var terminalStyle = new StyleBoxFlat();
			terminalStyle.BgColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
			terminalStyle.BorderColor = new Color(0, 1, 0);
			terminalStyle.SetBorderWidthAll(2);
			terminalStyle.SetCornerRadiusAll(4);
			terminalStyle.ShadowColor = new Color(0, 1, 0, 0.2f);
			terminalStyle.ShadowSize = 10;

			// Panel de Pausa
			_pausePanel = new Panel();
			_pausePanel.Name = "PausePanel";
			_pausePanel.Size = new Vector2(400, 200);
			_pausePanel.Position = new Vector2(
				(GetViewport().GetVisibleRect().Size.X - 400) / 2,
				(GetViewport().GetVisibleRect().Size.Y - 200) / 2
			);
			_pausePanel.AddThemeStyleboxOverride("panel", terminalStyle);
			_pausePanel.Visible = false;
			AddChild(_pausePanel);

			var pauseLabel = new Label();
			pauseLabel.Text = "> SYSTEM_PAUSED";
			pauseLabel.HorizontalAlignment = HorizontalAlignment.Center;
			pauseLabel.VerticalAlignment = VerticalAlignment.Center;
			pauseLabel.Size = new Vector2(400, 200);
			pauseLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
			pauseLabel.AddThemeFontSizeOverride("font_size", 32);
			_pausePanel.AddChild(pauseLabel);

			// Panel de Game Over
			_gameOverPanel = new Panel();
			_gameOverPanel.Name = "GameOverPanel";
			_gameOverPanel.Size = new Vector2(500, 300);
			_gameOverPanel.Position = new Vector2(
				(GetViewport().GetVisibleRect().Size.X - 500) / 2,
				(GetViewport().GetVisibleRect().Size.Y - 300) / 2
			);
			_gameOverPanel.AddThemeStyleboxOverride("panel", terminalStyle);
			_gameOverPanel.Visible = false;
			AddChild(_gameOverPanel);

			var gameOverVBox = new VBoxContainer();
			gameOverVBox.Size = new Vector2(500, 300);
			gameOverVBox.Alignment = BoxContainer.AlignmentMode.Center;
			_gameOverPanel.AddChild(gameOverVBox);

			var gameOverLabel = new Label();
			gameOverLabel.Text = "CRITICAL_SYSTEM_FAILURE";
			gameOverLabel.HorizontalAlignment = HorizontalAlignment.Center;
			gameOverLabel.AddThemeColorOverride("font_color", new Color(1, 0, 0));
			gameOverLabel.AddThemeFontSizeOverride("font_size", 36);
			gameOverVBox.AddChild(gameOverLabel);

			var restartLabel = new Label();
			restartLabel.Text = "Press R to Reboot System\nPress ESC to Abort";
			restartLabel.HorizontalAlignment = HorizontalAlignment.Center;
			restartLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
			restartLabel.AddThemeFontSizeOverride("font_size", 18);
			gameOverVBox.AddChild(restartLabel);
		}

		private void OnGameStateChanged(Core.GameState newState)
		{
			if (_pausePanel != null) _pausePanel.Visible = newState == Core.GameState.Paused;
			if (_gameOverPanel != null) _gameOverPanel.Visible = newState == Core.GameState.GameOver;
		}

		private void SubscribeToEvents()
		{
			GameEventBus.Instance.OnScoreChanged += UpdateScore;
			GameEventBus.Instance.OnPlayerHealthChanged += UpdateHealth;
			GameEventBus.Instance.OnSecurityTipShown += ShowTip;
			GameEventBus.Instance.OnLevelStarted += UpdateLevel;
			GameEventBus.Instance.OnShieldActivated += OnShieldActivated;
			GameEventBus.Instance.OnGameStateChanged += OnGameStateChanged;
		}

		private void UpdateScore(int score)
		{
			if (_scoreLabel != null)
			{
				_scoreLabel.Text = $"Datos: {score}MB";
			}
		}

		private void UpdateHealth(float health)
		{
			if (_healthBar != null)
			{
				_healthBar.Value = health;
			}
		}

		private void UpdateLevel(int level)
		{
			if (_levelLabel != null)
			{
				_levelLabel.Text = $"Capa: {level}";
			}
		}

		private void OnShieldActivated(string shieldType)
		{
			if (_shieldBar != null)
			{
				_shieldBar.Visible = true;
				_shieldBar.Value = 100;
			}
		}

		private void ShowTip(string tip)
		{
			if (_notificationContainer == null) return;

			// Crear notificación tipo Toast
			var notifPanel = new PanelContainer();
			var style = new StyleBoxFlat();
			style.BgColor = new Color(0, 0, 0, 0.8f);
			style.BorderColor = new Color(0, 1, 0);
			style.SetBorderWidthAll(1);
			style.SetCornerRadiusAll(4);
			notifPanel.AddThemeStyleboxOverride("panel", style);
			
			var vbox = new VBoxContainer();
			notifPanel.AddChild(vbox);
			
			var title = new Label();
			title.Text = "> INCOMING_MSG";
			title.AddThemeColorOverride("font_color", new Color(0, 1, 0));
			title.AddThemeFontSizeOverride("font_size", 10);
			vbox.AddChild(title);
			
			var msg = new Label();
			msg.Text = tip;
			msg.AutowrapMode = TextServer.AutowrapMode.Word;
			msg.CustomMinimumSize = new Vector2(280, 0);
			msg.AddThemeColorOverride("font_color", Colors.White);
			msg.AddThemeFontSizeOverride("font_size", 14);
			vbox.AddChild(msg);
			
			_notificationContainer.AddChild(notifPanel);
			
			// Animación de entrada
			notifPanel.Modulate = new Color(1, 1, 1, 0);
			var tween = CreateTween();
			tween.TweenProperty(notifPanel, "modulate:a", 1.0f, 0.3f);
			tween.TweenInterval(4.0f); // Esperar
			tween.TweenProperty(notifPanel, "modulate:a", 0.0f, 0.5f); // Desvanecer
			tween.TweenCallback(Callable.From(notifPanel.QueueFree)); // Destruir
		}

		private void HideTip()
		{
			// Deprecated by Toast system
		}

		public void UpdateUI(int score, int level, float health, float shield, int lives, string weapon)
		{
			if (_scoreLabel != null)
				_scoreLabel.Text = $"Datos: {score}MB";
			
			if (_levelLabel != null)
				_levelLabel.Text = $"Capa: {level}";
			
			if (_healthBar != null)
				_healthBar.Value = health;
			
			if (_shieldBar != null)
			{
				_shieldBar.Value = shield;
				_shieldBar.Visible = shield > 0;
			}
			
			if (_livesLabel != null)
				_livesLabel.Text = $"Backups: {lives}";
			
			if (_weaponLabel != null)
				_weaponLabel.Text = $"Protocolo: {weapon}";
		}

		public override void _ExitTree()
		{
			GameEventBus.Instance.OnScoreChanged -= UpdateScore;
			GameEventBus.Instance.OnPlayerHealthChanged -= UpdateHealth;
			GameEventBus.Instance.OnSecurityTipShown -= ShowTip;
			GameEventBus.Instance.OnLevelStarted -= UpdateLevel;
			GameEventBus.Instance.OnShieldActivated -= OnShieldActivated;
			GameEventBus.Instance.OnGameStateChanged -= OnGameStateChanged;
		}
	}
}
