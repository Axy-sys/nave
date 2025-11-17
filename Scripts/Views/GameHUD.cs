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

		public override void _Ready()
		{
			InitializeUI();
			SubscribeToEvents();
		}

		private void InitializeUI()
		{
			// Crear estructura UI programáticamente
			// En producción, esto vendría de una escena .tscn
			
			// Score
			_scoreLabel = new Label();
			_scoreLabel.Name = "ScoreLabel";
			_scoreLabel.Position = new Vector2(20, 20);
			_scoreLabel.AddThemeColorOverride("font_color", Colors.White);
			AddChild(_scoreLabel);

			// Level
			_levelLabel = new Label();
			_levelLabel.Name = "LevelLabel";
			_levelLabel.Position = new Vector2(20, 50);
			_levelLabel.AddThemeColorOverride("font_color", Colors.Cyan);
			AddChild(_levelLabel);

			// Lives
			_livesLabel = new Label();
			_livesLabel.Name = "LivesLabel";
			_livesLabel.Position = new Vector2(20, 80);
			_livesLabel.AddThemeColorOverride("font_color", Colors.Red);
			AddChild(_livesLabel);

			// Health Bar
			_healthBar = new ProgressBar();
			_healthBar.Name = "HealthBar";
			_healthBar.Position = new Vector2(20, 120);
			_healthBar.Size = new Vector2(200, 20);
			_healthBar.MaxValue = 100;
			_healthBar.Value = 100;
			AddChild(_healthBar);

			// Shield Bar
			_shieldBar = new ProgressBar();
			_shieldBar.Name = "ShieldBar";
			_shieldBar.Position = new Vector2(20, 150);
			_shieldBar.Size = new Vector2(200, 20);
			_shieldBar.MaxValue = 100;
			_shieldBar.Value = 0;
			_shieldBar.Visible = false;
			AddChild(_shieldBar);

			// Weapon Label
			_weaponLabel = new Label();
			_weaponLabel.Name = "WeaponLabel";
			_weaponLabel.Position = new Vector2(20, 180);
			_weaponLabel.AddThemeColorOverride("font_color", Colors.Yellow);
			AddChild(_weaponLabel);

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
