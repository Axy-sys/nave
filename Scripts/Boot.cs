using Godot;
using System;

/// <summary>
/// Boot con estética web: CRT scanlines, glitch effect, terminal style
/// - Skip instantáneo con cualquier input
/// - Efecto glitch sutil en el logo
/// - Colores de la web: verde terminal + púrpura CodeRippier
/// </summary>
public partial class Boot : Control
{
    // Colores exactos de la web
    private static readonly Color BG_COLOR = new Color("#050505");
    private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
    private static readonly Color TERMINAL_DIM = new Color("#008F11");
    private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
    private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
    
    private Label _logoLabel;
    private Label _subtitleLabel;
    private Label _skipHint;
    private ColorRect _scanlines;
    private float _timer = 0f;
    private bool _skipped = false;
    private Random _rng = new Random();
    
    private const float BOOT_DURATION = 2.5f;

    public override void _Ready()
    {
        // Fondo negro profundo
        var bg = new ColorRect();
        bg.Color = BG_COLOR;
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);
        
        // Efecto radial sutil (como la web)
        var vignette = new ColorRect();
        vignette.SetAnchorsPreset(LayoutPreset.FullRect);
        vignette.Color = new Color(0, 0, 0, 0);
        AddChild(vignette);
        
        // Logo principal con efecto glow
        _logoLabel = new Label();
        _logoLabel.SetAnchorsPreset(LayoutPreset.Center);
        _logoLabel.GrowHorizontal = GrowDirection.Both;
        _logoLabel.GrowVertical = GrowDirection.Both;
        _logoLabel.Position = new Vector2(0, -50);
        _logoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _logoLabel.VerticalAlignment = VerticalAlignment.Center;
        _logoLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
        _logoLabel.AddThemeFontSizeOverride("font_size", 14);
        _logoLabel.Text = @"
    ██████╗ ██████╗ ██████╗ ███████╗
   ██╔════╝██╔═══██╗██╔══██╗██╔════╝
   ██║     ██║   ██║██║  ██║█████╗  
   ██║     ██║   ██║██║  ██║██╔══╝  
   ╚██████╗╚██████╔╝██████╔╝███████╗
    ╚═════╝ ╚═════╝ ╚═════╝ ╚══════╝
   ██████╗ ██╗██████╗ ██████╗ ██╗███████╗██████╗ 
   ██╔══██╗██║██╔══██╗██╔══██╗██║██╔════╝██╔══██╗
   ██████╔╝██║██████╔╝██████╔╝██║█████╗  ██████╔╝
   ██╔══██╗██║██╔═══╝ ██╔═══╝ ██║██╔══╝  ██╔══██╗
   ██║  ██║██║██║     ██║     ██║███████╗██║  ██║
   ╚═╝  ╚═╝╚═╝╚═╝     ╚═╝     ╚═╝╚══════╝╚═╝  ╚═╝";
        AddChild(_logoLabel);
        
        // Subtítulo estilo web
        _subtitleLabel = new Label();
        _subtitleLabel.SetAnchorsPreset(LayoutPreset.Center);
        _subtitleLabel.GrowHorizontal = GrowDirection.Both;
        _subtitleLabel.Position = new Vector2(0, 120);
        _subtitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _subtitleLabel.AddThemeColorOverride("font_color", RIPPIER_PURPLE);
        _subtitleLabel.AddThemeFontSizeOverride("font_size", 18);
        _subtitleLabel.Text = "[ REAL-TIME INTRUSION PREVENTION ]";
        AddChild(_subtitleLabel);
        
        // Skip hint con estilo terminal
        _skipHint = new Label();
        _skipHint.SetAnchorsPreset(LayoutPreset.CenterBottom);
        _skipHint.Position = new Vector2(0, -60);
        _skipHint.GrowHorizontal = GrowDirection.Both;
        _skipHint.HorizontalAlignment = HorizontalAlignment.Center;
        _skipHint.AddThemeColorOverride("font_color", TERMINAL_DIM);
        _skipHint.AddThemeFontSizeOverride("font_size", 16);
        _skipHint.Text = "> Press any key to initialize system_";
        AddChild(_skipHint);
        
        // CRT Scanlines overlay (como la web)
        _scanlines = new ColorRect();
        _scanlines.SetAnchorsPreset(LayoutPreset.FullRect);
        _scanlines.MouseFilter = MouseFilterEnum.Ignore;
        _scanlines.Color = new Color(0, 1, 0.25f, 0.03f);
        AddChild(_scanlines);
    }

    public override void _Process(double delta)
    {
        if (_skipped) return;
        _timer += (float)delta;
        
        // Cursor blink en el hint
        string cursor = ((int)(_timer * 2) % 2 == 0) ? "_" : " ";
        _skipHint.Text = $"> Press any key to initialize system{cursor}";
        
        // Glitch effect sutil (como la web)
        if (_rng.NextDouble() < 0.02)
        {
            float offsetX = (float)(_rng.NextDouble() - 0.5) * 4;
            _logoLabel.Position = new Vector2(offsetX, -50);
            
            // Color glitch momentáneo
            _logoLabel.AddThemeColorOverride("font_color", 
                _rng.NextDouble() > 0.5 ? RIPPIER_PURPLE : TERMINAL_GREEN);
        }
        else
        {
            _logoLabel.Position = new Vector2(0, -50);
            _logoLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
        }
        
        // Scanline animation
        float scanAlpha = 0.02f + 0.01f * Mathf.Sin(_timer * 5);
        _scanlines.Color = new Color(0, 1, 0.25f, scanAlpha);
        
        if (_timer >= BOOT_DURATION)
            GoToMainMenu();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (_skipped) return;
        
        bool shouldSkip = @event switch
        {
            InputEventKey key => key.Pressed,
            InputEventMouseButton mouse => mouse.Pressed,
            InputEventJoypadButton joy => joy.Pressed,
            _ => false
        };
        
        if (shouldSkip) GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        if (_skipped) return;
        _skipped = true;
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }
    
    public void OnAnimationFinished(StringName animName) => GoToMainMenu();
    public void OnBootSequenceFinished() => GoToMainMenu();
}
