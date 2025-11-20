using Godot;

public partial class MainMenu : Control
{
    private AnimationPlayer _animationPlayer;
    
    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animationPlayer.Play("title_glow");
        
        // Animar estrellas
        var starsAnimation = _animationPlayer.GetAnimation("stars");
        if (starsAnimation != null)
        {
            _animationPlayer.Play("stars");
        }

        ApplyTerminalStyle();
    }

    private void ApplyTerminalStyle()
    {
        // Estilo base para botones
        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0, 0, 0, 0.6f);
        normalStyle.BorderColor = new Color(0, 1, 0); // Verde terminal
        normalStyle.SetBorderWidthAll(2);
        normalStyle.SetCornerRadiusAll(4);

        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(0, 1, 0, 0.2f);
        hoverStyle.BorderColor = new Color(0, 1, 0);
        hoverStyle.SetBorderWidthAll(2);
        hoverStyle.SetCornerRadiusAll(4);
        hoverStyle.ShadowColor = new Color(0, 1, 0, 0.4f);
        hoverStyle.ShadowSize = 8;

        var pressedStyle = new StyleBoxFlat();
        pressedStyle.BgColor = new Color(0, 0.8f, 0, 0.4f);
        pressedStyle.BorderColor = new Color(0, 1, 0);
        pressedStyle.SetBorderWidthAll(2);
        pressedStyle.SetCornerRadiusAll(4);

        // Aplicar a todos los botones
        string[] buttonNames = { "PlayButton", "TutorialButton", "OptionsButton", "QuitButton" };
        var buttonsContainer = GetNode<VBoxContainer>("UI/MenuButtons");

        foreach (string btnName in buttonNames)
        {
            var btn = buttonsContainer.GetNode<Button>(btnName);
            btn.AddThemeStyleboxOverride("normal", normalStyle);
            btn.AddThemeStyleboxOverride("hover", hoverStyle);
            btn.AddThemeStyleboxOverride("pressed", pressedStyle);
            
            // Texto estilo terminal
            btn.AddThemeColorOverride("font_color", new Color(0, 1, 0));
            btn.AddThemeColorOverride("font_hover_color", Colors.White);
            btn.AddThemeColorOverride("font_pressed_color", Colors.Black);
            
            // Añadir prefijo > al texto si no lo tiene
            if (!btn.Text.StartsWith("> "))
            {
                btn.Text = "> " + btn.Text.Replace("▶ ", "").Replace("📖 ", "").Replace("⚙️ ", "").Replace("❌ ", "");
            }
        }

        // Estilo del Título
        var titleLabel = GetNode<Label>("UI/TitleContainer/Title");
        titleLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
        titleLabel.Text = "SYSTEM_DEFENDER_V1.0";
        
        var subtitleLabel = GetNode<Label>("UI/TitleContainer/Subtitle");
        subtitleLabel.AddThemeColorOverride("font_color", new Color(0, 0.8f, 0));
        subtitleLabel.Text = "[ CYBERSECURITY_TRAINING_MODULE ]";
    }
    
    private void _on_play_button_pressed()
    {
        // Transición al juego principal
        GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
    }
    
    private void _on_tutorial_button_pressed()
    {
        // Ir al tutorial
        GetTree().ChangeSceneToFile("res://Scenes/Tutorial.tscn");
    }
    
    private void _on_options_button_pressed()
    {
        // TODO: Abrir menú de opciones
        GD.Print("Options menu - Coming soon!");
    }
    
    private void _on_quit_button_pressed()
    {
        GetTree().Quit();
    }
    
    public override void _Input(InputEvent @event)
    {
        // Presionar ESC para salir
        if (@event.IsActionPressed("ui_cancel"))
        {
            GetTree().Quit();
        }
    }
}
