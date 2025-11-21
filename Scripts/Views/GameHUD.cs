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
		private ProgressBar _cpuBar;
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
			// Crear estructura UI programáticamente con mejor diseño y accesibilidad
			// Usamos Anchors y Offsets explícitos para garantizar respuesta a cambios de resolución

			// 1. TOP BAR CONTAINER (Unified Info)
			var topBar = new Panel();
			topBar.Name = "TopBar";
			// Anchor Top Wide
			topBar.AnchorLeft = 0;
			topBar.AnchorTop = 0;
			topBar.AnchorRight = 1;
			topBar.AnchorBottom = 0;
			topBar.OffsetLeft = 0;
			topBar.OffsetTop = 0;
			topBar.OffsetRight = 0;
			topBar.OffsetBottom = 60;
			
			var topBarStyle = new StyleBoxFlat();
			topBarStyle.BgColor = new Color(0, 0, 0, 0.9f); // Darker black
			topBarStyle.BorderColor = new Color("bf00ff"); // Rippier Purple border
			topBarStyle.BorderWidthBottom = 2;
			topBar.AddThemeStyleboxOverride("panel", topBarStyle);
			AddChild(topBar);

			var topHBox = new HBoxContainer();
			topHBox.SetAnchorsPreset(Control.LayoutPreset.FullRect, true);
			topHBox.AddThemeConstantOverride("separation", 50);
			topHBox.Alignment = BoxContainer.AlignmentMode.Center;
			topBar.AddChild(topHBox);

			// Score Label
			_scoreLabel = new Label();
			_scoreLabel.Text = "DATA_STREAM: 0MB";
			_scoreLabel.AddThemeColorOverride("font_color", new Color("00ff41")); // Terminal Green
			_scoreLabel.AddThemeFontSizeOverride("font_size", 24); // Larger font
			_scoreLabel.VerticalAlignment = VerticalAlignment.Center;
			topHBox.AddChild(_scoreLabel);

			// Level Label
			_levelLabel = new Label();
			_levelLabel.Text = "SEC_LAYER: 1";
			_levelLabel.AddThemeColorOverride("font_color", new Color("bf00ff")); // Rippier Purple
			_levelLabel.AddThemeFontSizeOverride("font_size", 24);
			_levelLabel.VerticalAlignment = VerticalAlignment.Center;
			topHBox.AddChild(_levelLabel);

			// Lives Label
			_livesLabel = new Label();
			_livesLabel.Text = "REDUNDANCY: 3";
			_livesLabel.AddThemeColorOverride("font_color", new Color("ff0000")); // Alert Red
			_livesLabel.AddThemeFontSizeOverride("font_size", 24);
			_livesLabel.VerticalAlignment = VerticalAlignment.Center;
			topHBox.AddChild(_livesLabel);

			// 2. BOTTOM STATUS PANEL (Health & Shield)
			var statusPanel = new Panel();
			// Anchor Bottom Left
			statusPanel.AnchorLeft = 0;
			statusPanel.AnchorTop = 1;
			statusPanel.AnchorRight = 0;
			statusPanel.AnchorBottom = 1;
			statusPanel.OffsetLeft = 20;
			statusPanel.OffsetTop = -180; // Above Dialogue
			statusPanel.OffsetRight = 320; // Width 300
			statusPanel.OffsetBottom = -100; // Height 80
			
			var statusStyle = new StyleBoxFlat();
			statusStyle.BgColor = new Color(0, 0, 0, 0.6f);
			statusStyle.CornerRadiusTopRight = 20;
			statusStyle.CornerRadiusBottomRight = 20;
			statusPanel.AddThemeStyleboxOverride("panel", statusStyle);
			AddChild(statusPanel);

			var statusVBox = new VBoxContainer();
			statusVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect, true);
			statusVBox.Alignment = BoxContainer.AlignmentMode.Center;
			statusVBox.AddThemeConstantOverride("separation", 10);
			statusPanel.AddChild(statusVBox);

			// Health Bar
			_healthBar = new ProgressBar();
			_healthBar.CustomMinimumSize = new Vector2(260, 20);
			_healthBar.ShowPercentage = false;
			
			var healthBg = new StyleBoxFlat { BgColor = new Color(0.2f, 0, 0), CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomRight = 5, CornerRadiusBottomLeft = 5 };
			var healthFill = new StyleBoxFlat { BgColor = new Color(0, 1, 0), CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomRight = 5, CornerRadiusBottomLeft = 5 };
			_healthBar.AddThemeStyleboxOverride("background", healthBg);
			_healthBar.AddThemeStyleboxOverride("fill", healthFill);
			statusVBox.AddChild(_healthBar);

			var healthLabel = new Label();
			healthLabel.Text = "SYS_INTEGRITY";
			healthLabel.HorizontalAlignment = HorizontalAlignment.Center;
			healthLabel.AddThemeFontSizeOverride("font_size", 12);
			statusVBox.AddChild(healthLabel);

			// CPU Load Bar (Flux)
			_cpuBar = new ProgressBar();
			_cpuBar.CustomMinimumSize = new Vector2(260, 15);
			_cpuBar.ShowPercentage = false;
			
			var cpuBg = new StyleBoxFlat { BgColor = new Color(0.1f, 0.1f, 0.1f), CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomRight = 5, CornerRadiusBottomLeft = 5 };
			var cpuFill = new StyleBoxFlat { BgColor = new Color("ffaa00"), CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomRight = 5, CornerRadiusBottomLeft = 5 }; // Flux Orange
			_cpuBar.AddThemeStyleboxOverride("background", cpuBg);
			_cpuBar.AddThemeStyleboxOverride("fill", cpuFill);
			statusVBox.AddChild(_cpuBar);

			var cpuLabel = new Label();
			cpuLabel.Text = "CPU_FLUX";
			cpuLabel.HorizontalAlignment = HorizontalAlignment.Center;
			cpuLabel.AddThemeFontSizeOverride("font_size", 10);
			statusVBox.AddChild(cpuLabel);

			// Shield Bar (Optional)
			_shieldBar = new ProgressBar();
			_shieldBar.CustomMinimumSize = new Vector2(260, 10);
			_shieldBar.ShowPercentage = false;
			_shieldBar.Visible = false;
			
			var shieldFill = new StyleBoxFlat { BgColor = new Color(0, 0.8f, 1), CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomRight = 5, CornerRadiusBottomLeft = 5 };
			_shieldBar.AddThemeStyleboxOverride("background", healthBg);
			_shieldBar.AddThemeStyleboxOverride("fill", shieldFill);
			statusVBox.AddChild(_shieldBar);

			// 3. WEAPON INDICATOR (Bottom Right)
			var weaponPanel = new Panel();
			// Anchor Bottom Right
			weaponPanel.AnchorLeft = 1;
			weaponPanel.AnchorTop = 1;
			weaponPanel.AnchorRight = 1;
			weaponPanel.AnchorBottom = 1;
			weaponPanel.OffsetLeft = -270; // Width 250
			weaponPanel.OffsetTop = -180;
			weaponPanel.OffsetRight = -20;
			weaponPanel.OffsetBottom = -120; // Height 60

			weaponPanel.AddThemeStyleboxOverride("panel", statusStyle);
			AddChild(weaponPanel);
			
			_weaponLabel = new Label();
			_weaponLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
			_weaponLabel.Text = "EXEC_PROTOCOL: FIREWALL";
			_weaponLabel.AddThemeColorOverride("font_color", new Color("bf00ff")); // Rippier Purple
			_weaponLabel.AddThemeFontSizeOverride("font_size", 20);
			weaponPanel.AddChild(_weaponLabel);

			// 4. NOTIFICATION SYSTEM (Top Right)
			_notificationContainer = new VBoxContainer();
			_notificationContainer.Name = "NotificationContainer";
			// Anchor Top Right
			_notificationContainer.AnchorLeft = 1;
			_notificationContainer.AnchorTop = 0;
			_notificationContainer.AnchorRight = 1;
			_notificationContainer.AnchorBottom = 0;
			_notificationContainer.OffsetLeft = -370; // Width 350
			_notificationContainer.OffsetTop = 80;
			_notificationContainer.OffsetRight = -20;
			_notificationContainer.OffsetBottom = 80; // Grows down

			_notificationContainer.AddThemeConstantOverride("separation", 15);
			AddChild(_notificationContainer);

			UpdateUI(0, 1, 100, 0, 3, "Firewall");
		}

		private void InitializeStatePanels()
		{
			// Estilo común de terminal
			var terminalStyle = new StyleBoxFlat();
			terminalStyle.BgColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
			terminalStyle.BorderColor = new Color("bf00ff"); // Rippier Purple
			terminalStyle.SetBorderWidthAll(2);
			terminalStyle.SetCornerRadiusAll(4);
			terminalStyle.ShadowColor = new Color("bf00ff");
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
			GameEventBus.Instance.OnCpuLoadChanged += UpdateCpuLoad;
			GameEventBus.Instance.OnCpuOverloadChanged += OnCpuOverload;
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

		private void UpdateCpuLoad(float current, float max)
		{
			if (_cpuBar != null)
			{
				_cpuBar.MaxValue = max;
				_cpuBar.Value = current;
				
				// Cambiar color según intensidad
				var fillStyle = _cpuBar.GetThemeStylebox("fill") as StyleBoxFlat;
				if (fillStyle != null)
				{
					float percentage = current / max;
					if (percentage > 0.8f)
						fillStyle.BgColor = new Color(1, 0, 0); // Rojo crítico
					else if (percentage > 0.5f)
						fillStyle.BgColor = new Color(1, 0.5f, 0); // Naranja
					else
						fillStyle.BgColor = new Color(0, 0.8f, 1); // Azul normal
				}
			}
		}

		private void OnCpuOverload(bool isOverloaded)
		{
			if (_cpuBar != null)
			{
				var fillStyle = _cpuBar.GetThemeStylebox("fill") as StyleBoxFlat;
				if (fillStyle != null)
				{
					if (isOverloaded)
					{
						fillStyle.BgColor = new Color(1, 0, 1); // Magenta glitch
						// Podríamos añadir una animación de parpadeo aquí
					}
					else
					{
						fillStyle.BgColor = new Color(0, 0.8f, 1); // Reset a azul
					}
				}
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
			GameEventBus.Instance.OnCpuLoadChanged -= UpdateCpuLoad;
			GameEventBus.Instance.OnCpuOverloadChanged -= OnCpuOverload;
		}
	}
}
