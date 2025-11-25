using Godot;
using System;
using CyberSecurityGame.Core;

/// <summary>
/// Menú principal con estética web: Terminal window, CRT scanlines, glitch effects
/// - Instantáneo pero con estilo visual sofisticado
/// - Colores coordinados con la web
/// </summary>
public partial class MainMenu : Control
{
	// Colores exactos de la web
	private static readonly Color BG_COLOR = new Color("#050505");
	private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
	private static readonly Color TERMINAL_DIM = new Color("#008F11");
	private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
	private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
	private static readonly Color GLASS_BG = new Color(0, 0.08f, 0, 0.9f);
	
	private Panel _terminalWindow;
	private Label _titleLabel;
	private Label _subtitleLabel;
	private Label _recordLabel;
	private VBoxContainer _menuContainer;
	private Button _playButton;
	private Button _recordsButton;
	private Button _tutorialButton;
	private Button _quitButton;
	private ColorRect _scanlines;
	private float _timer = 0f;
	private Random _rng = new Random();

	public override void _Ready()
	{
		if (HighScoreSystem.Instance == null)
		{
			var highScoreSystem = new HighScoreSystem();
			highScoreSystem.Name = "HighScoreSystem";
			GetTree().Root.AddChild(highScoreSystem);
		}
		
		CreateWebStyleUI();
		_playButton.GrabFocus();
	}
	
	private void CreateWebStyleUI()
	{
		// Fondo con gradiente radial (simulado)
		var bg = new ColorRect();
		bg.Color = BG_COLOR;
		bg.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(bg);
		
		// Terminal Window (como la web)
		_terminalWindow = new Panel();
		_terminalWindow.SetAnchorsPreset(LayoutPreset.Center);
		_terminalWindow.CustomMinimumSize = new Vector2(700, 550);
		_terminalWindow.GrowHorizontal = GrowDirection.Both;
		_terminalWindow.GrowVertical = GrowDirection.Both;
		
		var terminalStyle = new StyleBoxFlat();
		terminalStyle.BgColor = new Color(0, 0, 0, 0.9f);
		terminalStyle.BorderColor = TERMINAL_DIM;
		terminalStyle.SetBorderWidthAll(1);
		terminalStyle.SetCornerRadiusAll(5);
		terminalStyle.ShadowColor = new Color(TERMINAL_GREEN, 0.2f);
		terminalStyle.ShadowSize = 15;
		_terminalWindow.AddThemeStyleboxOverride("panel", terminalStyle);
		AddChild(_terminalWindow);
		
		// Terminal Header (dots como la web)
		var header = new HBoxContainer();
		header.SetAnchorsPreset(LayoutPreset.TopWide);
		header.OffsetBottom = 35;
		header.AddThemeConstantOverride("separation", 8);
		
		var headerBg = new StyleBoxFlat();
		headerBg.BgColor = new Color("#1a1a1a");
		headerBg.ContentMarginLeft = 15;
		headerBg.ContentMarginTop = 10;
		
		var headerPanel = new Panel();
		headerPanel.SetAnchorsPreset(LayoutPreset.TopWide);
		headerPanel.OffsetBottom = 35;
		headerPanel.AddThemeStyleboxOverride("panel", headerBg);
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
		
		// Título en header
		var headerTitle = new Label();
		headerTitle.Text = "main_menu.sys";
		headerTitle.Position = new Vector2(80, 8);
		headerTitle.AddThemeColorOverride("font_color", new Color("#666666"));
		headerTitle.AddThemeFontSizeOverride("font_size", 14);
		headerPanel.AddChild(headerTitle);
		
		// Contenido del terminal
		var content = new VBoxContainer();
		content.SetAnchorsPreset(LayoutPreset.FullRect);
		content.OffsetTop = 45;
		content.OffsetLeft = 25;
		content.OffsetRight = -25;
		content.OffsetBottom = -20;
		content.AddThemeConstantOverride("separation", 12);
		_terminalWindow.AddChild(content);
		
		// Título con efecto glitch
		_titleLabel = new Label();
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
		_titleLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
		_titleLabel.AddThemeFontSizeOverride("font_size", 16);
		_titleLabel.Text = "> PROJECT: R.I.P.";
		content.AddChild(_titleLabel);
		
		_subtitleLabel = new Label();
		_subtitleLabel.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
		_subtitleLabel.AddThemeFontSizeOverride("font_size", 14);
		_subtitleLabel.Text = "  [Real-time Intrusion Prevention] :: CodeRippier";
		content.AddChild(_subtitleLabel);
		
		// Separador
		var sep = new Label();
		sep.AddThemeColorOverride("font_color", TERMINAL_DIM);
		sep.AddThemeFontSizeOverride("font_size", 12);
		sep.Text = "  ─────────────────────────────────────────────";
		content.AddChild(sep);
		
		// Record
		_recordLabel = new Label();
		_recordLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
		_recordLabel.AddThemeFontSizeOverride("font_size", 18);
		int topScore = HighScoreSystem.Instance?.GetTopScore() ?? 0;
		_recordLabel.Text = topScore > 0 ? $"  >>> HIGH SCORE: {topScore:N0} <<<" : "  >>> NO RECORDS YET <<<";
		content.AddChild(_recordLabel);
		
		// Menú de opciones
		_menuContainer = new VBoxContainer();
		_menuContainer.AddThemeConstantOverride("separation", 8);
		content.AddChild(_menuContainer);
		
		// Prompt
		var prompt = new Label();
		prompt.AddThemeColorOverride("font_color", TERMINAL_DIM);
		prompt.AddThemeFontSizeOverride("font_size", 14);
		prompt.Text = "\n  user@defense:~$ select_option";
		_menuContainer.AddChild(prompt);
		
		// Botones estilo terminal
		_playButton = CreateTerminalButton("[1] INICIAR_SISTEMA", "Comenzar partida");
		_playButton.Pressed += OnPlayPressed;
		_menuContainer.AddChild(_playButton);
		
		_recordsButton = CreateTerminalButton("[2] VER_LEADERBOARD", "Tabla de récords");
		_recordsButton.Pressed += OnRecordsPressed;
		_menuContainer.AddChild(_recordsButton);
		
		_tutorialButton = CreateTerminalButton("[3] TUTORIAL_MODE", "Aprende a jugar");
		_tutorialButton.Pressed += OnTutorialPressed;
		_menuContainer.AddChild(_tutorialButton);
		
		_quitButton = CreateTerminalButton("[4] EXIT_SYSTEM", "Cerrar aplicación");
		_quitButton.Pressed += OnQuitPressed;
		_menuContainer.AddChild(_quitButton);
		
		// Footer con controles
		var footer = new Label();
		footer.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
		footer.AddThemeFontSizeOverride("font_size", 14);
		footer.Text = "\n  [↑↓] Navigate   [ENTER] Select   [1-4] Quick Access   [ESC] Exit";
		content.AddChild(footer);
		
		// CRT Scanlines
		_scanlines = new ColorRect();
		_scanlines.SetAnchorsPreset(LayoutPreset.FullRect);
		_scanlines.MouseFilter = MouseFilterEnum.Ignore;
		_scanlines.Color = new Color(0, 1, 0.25f, 0.03f);
		AddChild(_scanlines);
	}
	
	private Button CreateTerminalButton(string text, string tooltip)
	{
		var button = new Button();
		button.Text = "      " + text;
		button.TooltipText = tooltip;
		button.CustomMinimumSize = new Vector2(400, 45);
		button.FocusMode = FocusModeEnum.All;
		button.Alignment = HorizontalAlignment.Left;
		
		button.AddThemeColorOverride("font_color", new Color("#ffffff"));
		button.AddThemeColorOverride("font_hover_color", BG_COLOR);
		button.AddThemeColorOverride("font_focus_color", BG_COLOR);
		button.AddThemeColorOverride("font_pressed_color", BG_COLOR);
		button.AddThemeFontSizeOverride("font_size", 18);
		
		// Estilo normal - transparente
		var normalStyle = new StyleBoxFlat();
		normalStyle.BgColor = new Color(0, 0, 0, 0);
		normalStyle.BorderColor = TERMINAL_DIM;
		normalStyle.BorderWidthLeft = 0;
		normalStyle.BorderWidthRight = 0;
		normalStyle.BorderWidthTop = 0;
		normalStyle.BorderWidthBottom = 0;
		button.AddThemeStyleboxOverride("normal", normalStyle);
		
		// Estilo hover/focus - verde terminal con glow
		var focusStyle = new StyleBoxFlat();
		focusStyle.BgColor = TERMINAL_GREEN;
		focusStyle.BorderColor = TERMINAL_GREEN;
		focusStyle.SetBorderWidthAll(0);
		focusStyle.ContentMarginLeft = 10;
		button.AddThemeStyleboxOverride("hover", focusStyle);
		button.AddThemeStyleboxOverride("focus", focusStyle);
		button.AddThemeStyleboxOverride("pressed", focusStyle);
		
		return button;
	}
	
	public override void _Process(double delta)
	{
		_timer += (float)delta;
		
		// Glitch sutil en el título
		if (_rng.NextDouble() < 0.01)
		{
			_titleLabel.AddThemeColorOverride("font_color", 
				_rng.NextDouble() > 0.5 ? RIPPIER_PURPLE : TERMINAL_GREEN);
		}
		else
		{
			_titleLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
		}
		
		// Scanline animation
		float alpha = 0.02f + 0.015f * Mathf.Sin(_timer * 4);
		_scanlines.Color = new Color(0, 1, 0.25f, alpha);
	}
	
	private void OnPlayPressed() => GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
	private void OnRecordsPressed() => GetTree().ChangeSceneToFile("res://Scenes/Leaderboard.tscn");
	private void OnTutorialPressed() => GetTree().ChangeSceneToFile("res://Scenes/Tutorial.tscn");
	private void OnQuitPressed() => GetTree().Quit();
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Key1: OnPlayPressed(); break;
				case Key.Key2: OnRecordsPressed(); break;
				case Key.Key3: OnTutorialPressed(); break;
				case Key.Key4:
				case Key.Escape: OnQuitPressed(); break;
			}
		}
	}
}
