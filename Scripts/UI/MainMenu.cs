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
