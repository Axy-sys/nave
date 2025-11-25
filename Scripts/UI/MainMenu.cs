using Godot;
using System;
using CyberSecurityGame.Core;
using CyberSecurityGame.UI;

/// <summary>
/// Menú principal MODERNO y ATRACTIVO
/// - Diseño limpio con animaciones suaves
/// - Colores vibrantes pero elegantes
/// - Feedback visual inmediato
/// - Transiciones fluidas
/// </summary>
public partial class MainMenu : Control
{
	// Paleta de colores moderna
	private static readonly Color BG_DARK = new Color("#0a0a0f");
	private static readonly Color ACCENT_CYAN = new Color("#00d4ff");
	private static readonly Color ACCENT_MAGENTA = new Color("#ff00aa");
	private static readonly Color ACCENT_GOLD = new Color("#ffd700");
	private static readonly Color TEXT_WHITE = new Color("#ffffff");
	private static readonly Color TEXT_DIM = new Color("#666680");
	
	// UI Elements
	private ColorRect _background;
	private Control _logoContainer;
	private Label _titleLabel;
	private Label _subtitleLabel;
	private VBoxContainer _menuContainer;
	private Button _playButton;
	private Button _tutorialButton;
	private Button _quitButton;
	private Panel _scoresPanel;
	private VBoxContainer _scoresContainer;
	
	// Animation
	private float _timer = 0f;
	private bool _animationComplete = false;

	public override void _Ready()
	{
		InitializeSystems();
		CreateModernUI();
		PlayEntranceAnimation();
	}
	
	private void InitializeSystems()
	{
		if (ArcadeScoreSystem.Instance == null)
		{
			var arcadeSystem = new ArcadeScoreSystem();
			arcadeSystem.Name = "ArcadeScoreSystem";
			GetTree().Root.AddChild(arcadeSystem);
		}
		
		if (HighScoreSystem.Instance == null)
		{
			var highScoreSystem = new HighScoreSystem();
			highScoreSystem.Name = "HighScoreSystem";
			GetTree().Root.AddChild(highScoreSystem);
		}
	}
	
	private void CreateModernUI()
	{
		// ═══════════════════════════════════════════════════════════
		// FONDO
		// ═══════════════════════════════════════════════════════════
		_background = new ColorRect();
		_background.SetAnchorsPreset(LayoutPreset.FullRect);
		_background.Color = BG_DARK;
		AddChild(_background);
		
		// Partículas de fondo
		CreateBackgroundParticles();
		
		// ═══════════════════════════════════════════════════════════
		// CONTENEDOR PRINCIPAL
		// ═══════════════════════════════════════════════════════════
		var mainContainer = new VBoxContainer();
		mainContainer.Name = "MainContainer";
		mainContainer.SetAnchorsPreset(LayoutPreset.Center);
		mainContainer.GrowHorizontal = GrowDirection.Both;
		mainContainer.GrowVertical = GrowDirection.Both;
		mainContainer.CustomMinimumSize = new Vector2(600, 500);
		mainContainer.AddThemeConstantOverride("separation", 25);
		AddChild(mainContainer);
		
		// ═══════════════════════════════════════════════════════════
		// LOGO / TÍTULO
		// ═══════════════════════════════════════════════════════════
		_logoContainer = new Control();
		_logoContainer.CustomMinimumSize = new Vector2(600, 120);
		mainContainer.AddChild(_logoContainer);
		
		_titleLabel = new Label();
		_titleLabel.Text = "CODE RIPPIER";
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_titleLabel.SetAnchorsPreset(LayoutPreset.TopWide);
		_titleLabel.AddThemeColorOverride("font_color", TEXT_WHITE);
		_titleLabel.AddThemeFontSizeOverride("font_size", 58);
		_titleLabel.Modulate = new Color(1, 1, 1, 0);
		_logoContainer.AddChild(_titleLabel);
		
		_subtitleLabel = new Label();
		_subtitleLabel.Text = "⚡ Real-time Intrusion Prevention ⚡";
		_subtitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_subtitleLabel.SetAnchorsPreset(LayoutPreset.TopWide);
		_subtitleLabel.OffsetTop = 65;
		_subtitleLabel.AddThemeColorOverride("font_color", ACCENT_CYAN);
		_subtitleLabel.AddThemeFontSizeOverride("font_size", 16);
		_subtitleLabel.Modulate = new Color(1, 1, 1, 0);
		_logoContainer.AddChild(_subtitleLabel);
		
		// Línea decorativa
		var decorLine = new ColorRect();
		decorLine.Name = "DecorLine";
		decorLine.SetAnchorsPreset(LayoutPreset.TopWide);
		decorLine.OffsetTop = 95;
		decorLine.OffsetBottom = 98;
		decorLine.OffsetLeft = 180;
		decorLine.OffsetRight = -180;
		decorLine.Color = ACCENT_CYAN;
		decorLine.Modulate = new Color(1, 1, 1, 0);
		_logoContainer.AddChild(decorLine);
		
		// ═══════════════════════════════════════════════════════════
		// BOTONES
		// ═══════════════════════════════════════════════════════════
		_menuContainer = new VBoxContainer();
		_menuContainer.Name = "MenuContainer";
		_menuContainer.AddThemeConstantOverride("separation", 12);
		_menuContainer.Modulate = new Color(1, 1, 1, 0);
		mainContainer.AddChild(_menuContainer);
		
		var buttonCenter = new HBoxContainer();
		buttonCenter.Alignment = BoxContainer.AlignmentMode.Center;
		_menuContainer.AddChild(buttonCenter);
		
		var buttonVBox = new VBoxContainer();
		buttonVBox.AddThemeConstantOverride("separation", 10);
		buttonCenter.AddChild(buttonVBox);
		
		_playButton = CreateModernButton("▶  JUGAR", ACCENT_CYAN, true);
		_playButton.Pressed += OnPlayPressed;
		buttonVBox.AddChild(_playButton);
		
		_tutorialButton = CreateModernButton("?  TUTORIAL", ACCENT_GOLD, false);
		_tutorialButton.Pressed += OnTutorialPressed;
		buttonVBox.AddChild(_tutorialButton);
		
		_quitButton = CreateModernButton("✕  SALIR", ACCENT_MAGENTA, false);
		_quitButton.Pressed += OnQuitPressed;
		buttonVBox.AddChild(_quitButton);
		
		// ═══════════════════════════════════════════════════════════
		// HIGH SCORES
		// ═══════════════════════════════════════════════════════════
		_scoresPanel = new Panel();
		_scoresPanel.Name = "ScoresPanel";
		_scoresPanel.CustomMinimumSize = new Vector2(350, 130);
		_scoresPanel.Modulate = new Color(1, 1, 1, 0);
		
		var scoresPanelStyle = new StyleBoxFlat();
		scoresPanelStyle.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.8f);
		scoresPanelStyle.BorderColor = new Color(ACCENT_CYAN, 0.3f);
		scoresPanelStyle.SetBorderWidthAll(1);
		scoresPanelStyle.SetCornerRadiusAll(8);
		_scoresPanel.AddThemeStyleboxOverride("panel", scoresPanelStyle);
		mainContainer.AddChild(_scoresPanel);
		
		_scoresContainer = new VBoxContainer();
		_scoresContainer.SetAnchorsPreset(LayoutPreset.FullRect);
		_scoresContainer.OffsetLeft = 15;
		_scoresContainer.OffsetRight = -15;
		_scoresContainer.OffsetTop = 12;
		_scoresContainer.OffsetBottom = -12;
		_scoresContainer.AddThemeConstantOverride("separation", 4);
		_scoresPanel.AddChild(_scoresContainer);
		
		var scoresHeader = new Label();
		scoresHeader.Text = "═══ TOP SCORES ═══";
		scoresHeader.HorizontalAlignment = HorizontalAlignment.Center;
		scoresHeader.AddThemeColorOverride("font_color", ACCENT_GOLD);
		scoresHeader.AddThemeFontSizeOverride("font_size", 14);
		_scoresContainer.AddChild(scoresHeader);
		
		UpdateTopScores();
		
		// ═══════════════════════════════════════════════════════════
		// CONTROLES HINT
		// ═══════════════════════════════════════════════════════════
		var controlsHint = new Label();
		controlsHint.Name = "ControlsHint";
		controlsHint.Text = "WASD Mover  •  Mouse Apuntar  •  Click Disparar  •  Space Evadir";
		controlsHint.HorizontalAlignment = HorizontalAlignment.Center;
		controlsHint.AddThemeColorOverride("font_color", TEXT_DIM);
		controlsHint.AddThemeFontSizeOverride("font_size", 11);
		controlsHint.Modulate = new Color(1, 1, 1, 0);
		mainContainer.AddChild(controlsHint);
		
		// ═══════════════════════════════════════════════════════════
		// VERSION
		// ═══════════════════════════════════════════════════════════
		var versionLabel = new Label();
		versionLabel.Text = "v1.0 | Godot 4 + C#";
		versionLabel.SetAnchorsPreset(LayoutPreset.BottomRight);
		versionLabel.OffsetLeft = -150;
		versionLabel.OffsetTop = -25;
		versionLabel.OffsetRight = -15;
		versionLabel.OffsetBottom = -8;
		versionLabel.HorizontalAlignment = HorizontalAlignment.Right;
		versionLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.3f, 0.4f));
		versionLabel.AddThemeFontSizeOverride("font_size", 10);
		AddChild(versionLabel);
	}
	
	private void CreateBackgroundParticles()
	{
		var particles = new CpuParticles2D();
		particles.Position = new Vector2(600, 450);
		particles.Amount = 40;
		particles.Lifetime = 6f;
		particles.Preprocess = 4f;
		particles.Explosiveness = 0f;
		particles.EmissionShape = CpuParticles2D.EmissionShapeEnum.Rectangle;
		particles.EmissionRectExtents = new Vector2(650, 480);
		particles.Direction = new Vector2(0, -1);
		particles.Spread = 15f;
		particles.InitialVelocityMin = 8f;
		particles.InitialVelocityMax = 25f;
		particles.ScaleAmountMin = 1f;
		particles.ScaleAmountMax = 2.5f;
		particles.Color = new Color(ACCENT_CYAN, 0.25f);
		AddChild(particles);
	}
	
	private Button CreateModernButton(string text, Color accentColor, bool isPrimary)
	{
		var button = new Button();
		button.Text = text;
		button.CustomMinimumSize = new Vector2(260, isPrimary ? 55 : 45);
		button.FocusMode = FocusModeEnum.All;
		
		button.AddThemeColorOverride("font_color", isPrimary ? BG_DARK : TEXT_WHITE);
		button.AddThemeColorOverride("font_hover_color", BG_DARK);
		button.AddThemeColorOverride("font_focus_color", BG_DARK);
		button.AddThemeColorOverride("font_pressed_color", BG_DARK);
		button.AddThemeFontSizeOverride("font_size", isPrimary ? 22 : 18);
		
		var normalStyle = new StyleBoxFlat();
		normalStyle.BgColor = isPrimary ? accentColor : new Color(0.1f, 0.1f, 0.15f, 0.9f);
		normalStyle.BorderColor = accentColor;
		normalStyle.SetBorderWidthAll(isPrimary ? 0 : 2);
		normalStyle.SetCornerRadiusAll(6);
		button.AddThemeStyleboxOverride("normal", normalStyle);
		
		var hoverStyle = new StyleBoxFlat();
		hoverStyle.BgColor = accentColor;
		hoverStyle.BorderColor = accentColor;
		hoverStyle.SetBorderWidthAll(0);
		hoverStyle.SetCornerRadiusAll(6);
		hoverStyle.ShadowColor = new Color(accentColor, 0.4f);
		hoverStyle.ShadowSize = 8;
		button.AddThemeStyleboxOverride("hover", hoverStyle);
		button.AddThemeStyleboxOverride("focus", hoverStyle);
		button.AddThemeStyleboxOverride("pressed", hoverStyle);
		
		button.MouseEntered += () => {
			var tween = CreateTween();
			tween.TweenProperty(button, "scale", new Vector2(1.03f, 1.03f), 0.08f);
		};
		button.MouseExited += () => {
			var tween = CreateTween();
			tween.TweenProperty(button, "scale", Vector2.One, 0.08f);
		};
		
		button.PivotOffset = button.CustomMinimumSize / 2;
		
		return button;
	}
	
	private void UpdateTopScores()
	{
		var leaderboard = ArcadeScoreSystem.Instance?.GetTopScores(3);
		string[] medals = { "🥇", "🥈", "🥉" };
		Color[] colors = { ACCENT_GOLD, new Color("#c0c0c0"), new Color("#cd7f32") };
		
		for (int i = 0; i < 3; i++)
		{
			var scoreLine = new Label();
			scoreLine.Name = $"ScoreLine{i}";
			scoreLine.HorizontalAlignment = HorizontalAlignment.Center;
			scoreLine.AddThemeFontSizeOverride("font_size", 16);
			
			if (leaderboard != null && i < leaderboard.Count)
			{
				var entry = leaderboard[i];
				scoreLine.Text = $"{medals[i]}  {entry.Initials}  {entry.Score,9:N0}";
				scoreLine.AddThemeColorOverride("font_color", colors[i]);
			}
			else
			{
				scoreLine.Text = $"{medals[i]}  ---  ---------";
				scoreLine.AddThemeColorOverride("font_color", TEXT_DIM);
			}
			_scoresContainer.AddChild(scoreLine);
		}
	}
	
	private void PlayEntranceAnimation()
	{
		var tween = CreateTween();
		tween.SetParallel(false);
		
		_titleLabel.Position = new Vector2(0, -40);
		tween.TweenProperty(_titleLabel, "modulate:a", 1f, 0.4f);
		tween.Parallel().TweenProperty(_titleLabel, "position:y", 0f, 0.4f)
			.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		
		tween.TweenProperty(_subtitleLabel, "modulate:a", 1f, 0.25f);
		
		var decorLine = _logoContainer.GetNodeOrNull<ColorRect>("DecorLine");
		if (decorLine != null)
			tween.TweenProperty(decorLine, "modulate:a", 1f, 0.2f);
		
		tween.TweenProperty(_menuContainer, "modulate:a", 1f, 0.3f);
		tween.TweenProperty(_scoresPanel, "modulate:a", 1f, 0.25f);
		
		var mainContainer = GetNodeOrNull<VBoxContainer>("MainContainer");
		var controlsHint = mainContainer?.GetNodeOrNull<Label>("ControlsHint");
		if (controlsHint != null)
			tween.TweenProperty(controlsHint, "modulate:a", 1f, 0.2f);
		
		tween.TweenCallback(Callable.From(() => {
			_animationComplete = true;
			_playButton.GrabFocus();
		}));
	}
	
	public override void _Process(double delta)
	{
		_timer += (float)delta;
		
		if (_animationComplete)
		{
			float glow = 0.85f + 0.15f * Mathf.Sin(_timer * 2f);
			_titleLabel.AddThemeColorOverride("font_color", new Color(glow, glow, glow));
			
			float pulse = 0.75f + 0.25f * Mathf.Sin(_timer * 1.5f);
			_subtitleLabel.AddThemeColorOverride("font_color", new Color(ACCENT_CYAN, pulse));
		}
	}
	
	private void OnPlayPressed()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0f, 0.25f);
		tween.TweenCallback(Callable.From(() => {
			GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
		}));
	}
	
	private void OnTutorialPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Tutorial.tscn");
	}
	
	private void OnQuitPressed()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0f, 0.15f);
		tween.TweenCallback(Callable.From(() => GetTree().Quit()));
	}
	
	public override void _Input(InputEvent @event)
	{
		if (!_animationComplete) return;
		
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.Escape)
				OnQuitPressed();
		}
	}
}
