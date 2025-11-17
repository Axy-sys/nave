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
		private Panel _tipPanel;
		private Label _tipLabel;
		private Timer _tipTimer;
		private Minimap _minimap;

		public override void _Ready()
		{
			InitializeUI();
			SubscribeToEvents();
		}

		private void InitializeUI()
		{
			// Crear estructura UI programáticamente con mejor diseño
			
			// Panel contenedor principal con fondo semi-transparente
			var hudPanel = new Panel();
			hudPanel.Name = "HUDPanel";
			hudPanel.Position = new Vector2(10, 10);
			hudPanel.Size = new Vector2(280, 220);
			
			var panelStyle = new StyleBoxFlat();
			panelStyle.BgColor = new Color(0, 0.1f, 0.15f, 0.85f);
			panelStyle.BorderColor = new Color(0, 1, 1, 0.8f);
			panelStyle.SetBorderWidthAll(2);
			panelStyle.SetCornerRadiusAll(10);
			panelStyle.ShadowColor = new Color(0, 1, 1, 0.3f);
			panelStyle.ShadowSize = 5;
			hudPanel.AddThemeStyleboxOverride("panel", panelStyle);
			AddChild(hudPanel);
			
			// Score con glow
			_scoreLabel = new Label();
			_scoreLabel.Name = "ScoreLabel";
			_scoreLabel.Position = new Vector2(15, 10);
			_scoreLabel.AddThemeColorOverride("font_color", Colors.White);
			_scoreLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 1, 1, 0.5f));
			_scoreLabel.AddThemeConstantOverride("shadow_outline_size", 2);
			_scoreLabel.AddThemeFontSizeOverride("font_size", 20);
			hudPanel.AddChild(_scoreLabel);

			// Level con efecto cyan
			_levelLabel = new Label();
			_levelLabel.Name = "LevelLabel";
			_levelLabel.Position = new Vector2(15, 35);
			_levelLabel.AddThemeColorOverride("font_color", new Color(0, 1, 1));
			_levelLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 1, 1, 0.8f));
			_levelLabel.AddThemeConstantOverride("shadow_outline_size", 3);
			_levelLabel.AddThemeFontSizeOverride("font_size", 18);
			hudPanel.AddChild(_levelLabel);

			// Lives con icono
			_livesLabel = new Label();
			_livesLabel.Name = "LivesLabel";
			_livesLabel.Position = new Vector2(15, 60);
			_livesLabel.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
			_livesLabel.AddThemeColorOverride("font_shadow_color", new Color(1, 0, 0, 0.5f));
			_livesLabel.AddThemeConstantOverride("shadow_outline_size", 2);
			_livesLabel.AddThemeFontSizeOverride("font_size", 18);
			hudPanel.AddChild(_livesLabel);

			// Label para Health
			var healthLabel = new Label();
			healthLabel.Position = new Vector2(15, 85);
			healthLabel.Text = "💚 SALUD";
			healthLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0.5f));
			healthLabel.AddThemeFontSizeOverride("font_size", 14);
			hudPanel.AddChild(healthLabel);

			// Health Bar mejorada
			_healthBar = new ProgressBar();
			_healthBar.Name = "HealthBar";
			_healthBar.Position = new Vector2(15, 105);
			_healthBar.Size = new Vector2(250, 25);
			_healthBar.MaxValue = 100;
			_healthBar.Value = 100;
			_healthBar.ShowPercentage = true;
			
			var healthStyle = new StyleBoxFlat();
			healthStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
			healthStyle.BorderColor = new Color(0, 1, 0.5f);
			healthStyle.SetBorderWidthAll(2);
			healthStyle.SetCornerRadiusAll(5);
			_healthBar.AddThemeStyleboxOverride("background", healthStyle);
			
			var healthFill = new StyleBoxFlat();
			healthFill.BgColor = new Color(0, 1, 0.5f);
			healthFill.SetCornerRadiusAll(5);
			_healthBar.AddThemeStyleboxOverride("fill", healthFill);
			hudPanel.AddChild(_healthBar);

			// Shield Bar mejorada
			var shieldLabel = new Label();
			shieldLabel.Name = "ShieldLabel";
			shieldLabel.Position = new Vector2(15, 135);
			shieldLabel.Text = "🛡️ ESCUDO";
			shieldLabel.AddThemeColorOverride("font_color", new Color(0, 0.7f, 1));
			shieldLabel.AddThemeFontSizeOverride("font_size", 14);
			shieldLabel.Visible = false;
			hudPanel.AddChild(shieldLabel);
			
			_shieldBar = new ProgressBar();
			_shieldBar.Name = "ShieldBar";
			_shieldBar.Position = new Vector2(15, 155);
			_shieldBar.Size = new Vector2(250, 25);
			_shieldBar.MaxValue = 100;
			_shieldBar.Value = 0;
			_shieldBar.ShowPercentage = true;
			_shieldBar.Visible = false;
			
			var shieldStyle = new StyleBoxFlat();
			shieldStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
			shieldStyle.BorderColor = new Color(0, 0.7f, 1);
			shieldStyle.SetBorderWidthAll(2);
			shieldStyle.SetCornerRadiusAll(5);
			_shieldBar.AddThemeStyleboxOverride("background", shieldStyle);
			
			var shieldFill = new StyleBoxFlat();
			shieldFill.BgColor = new Color(0, 0.7f, 1);
			shieldFill.SetCornerRadiusAll(5);
			_shieldBar.AddThemeStyleboxOverride("fill", shieldFill);
			hudPanel.AddChild(_shieldBar);

			// Weapon Label mejorado (bottom right)
			var weaponPanel = new Panel();
			weaponPanel.Name = "WeaponPanel";
			weaponPanel.Position = new Vector2(920, 650);
			weaponPanel.Size = new Vector2(260, 70);
			
			var weaponStyle = new StyleBoxFlat();
			weaponStyle.BgColor = new Color(0, 0.1f, 0.15f, 0.85f);
			weaponStyle.BorderColor = new Color(1, 0.8f, 0);
			weaponStyle.SetBorderWidthAll(2);
			weaponStyle.SetCornerRadiusAll(10);
			weaponStyle.ShadowColor = new Color(1, 0.8f, 0, 0.3f);
			weaponStyle.ShadowSize = 5;
			weaponPanel.AddThemeStyleboxOverride("panel", weaponStyle);
			AddChild(weaponPanel);
			
			_weaponLabel = new Label();
			_weaponLabel.Name = "WeaponLabel";
			_weaponLabel.Position = new Vector2(15, 20);
			_weaponLabel.AddThemeColorOverride("font_color", new Color(1, 0.9f, 0));
			_weaponLabel.AddThemeColorOverride("font_shadow_color", new Color(1, 0.8f, 0, 0.8f));
			_weaponLabel.AddThemeConstantOverride("shadow_outline_size", 3);
			_weaponLabel.AddThemeFontSizeOverride("font_size", 22);
			weaponPanel.AddChild(_weaponLabel);

			// Tip Panel
			_tipPanel = new Panel();
			_tipPanel.Name = "TipPanel";
			_tipPanel.Position = new Vector2(300, 500);
			_tipPanel.Size = new Vector2(400, 80);
			_tipPanel.Visible = false;
			AddChild(_tipPanel);

			_tipLabel = new Label();
			_tipLabel.Name = "TipLabel";
			_tipLabel.Position = new Vector2(10, 10);
			_tipLabel.Size = new Vector2(380, 60);
			_tipLabel.AutowrapMode = TextServer.AutowrapMode.Word;
			_tipPanel.AddChild(_tipLabel);

			_tipTimer = new Timer();
			_tipTimer.Name = "TipTimer";
			_tipTimer.WaitTime = 5.0;
			_tipTimer.OneShot = true;
			_tipTimer.Timeout += HideTip;
			AddChild(_tipTimer);

			// Minimap integrado en el HUD
			_minimap = new Minimap();
			_minimap.Name = "Minimap";
			_minimap.DetectionRadius = 600f;
			_minimap.MinimapSize = 180f;
			AddChild(_minimap);

			UpdateUI(0, 1, 100, 0, 3, "Firewall");
		}

		private void SubscribeToEvents()
		{
			GameEventBus.Instance.OnScoreChanged += UpdateScore;
			GameEventBus.Instance.OnPlayerHealthChanged += UpdateHealth;
			GameEventBus.Instance.OnSecurityTipShown += ShowTip;
			GameEventBus.Instance.OnLevelStarted += UpdateLevel;
			GameEventBus.Instance.OnShieldActivated += OnShieldActivated;
		}

		private void UpdateScore(int score)
		{
			if (_scoreLabel != null)
			{
				_scoreLabel.Text = $"Score: {score}";
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
				_levelLabel.Text = $"Nivel: {level}";
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
			if (_tipPanel != null && _tipLabel != null)
			{
				_tipLabel.Text = tip;
				_tipPanel.Visible = true;
				_tipTimer.Start();
			}
		}

		private void HideTip()
		{
			if (_tipPanel != null)
			{
				_tipPanel.Visible = false;
			}
		}

		public void UpdateUI(int score, int level, float health, float shield, int lives, string weapon)
		{
			if (_scoreLabel != null)
				_scoreLabel.Text = $"Score: {score}";
			
			if (_levelLabel != null)
				_levelLabel.Text = $"Nivel: {level}";
			
			if (_healthBar != null)
				_healthBar.Value = health;
			
			if (_shieldBar != null)
			{
				_shieldBar.Value = shield;
				_shieldBar.Visible = shield > 0;
			}
			
			if (_livesLabel != null)
				_livesLabel.Text = $"Vidas: {lives}";
			
			if (_weaponLabel != null)
				_weaponLabel.Text = $"Arma: {weapon}";
		}

		public override void _ExitTree()
		{
			GameEventBus.Instance.OnScoreChanged -= UpdateScore;
			GameEventBus.Instance.OnPlayerHealthChanged -= UpdateHealth;
			GameEventBus.Instance.OnSecurityTipShown -= ShowTip;
			GameEventBus.Instance.OnLevelStarted -= UpdateLevel;
			GameEventBus.Instance.OnShieldActivated -= OnShieldActivated;
		}
	}
}
