using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Tutorial interactivo guiado por ELLIOT (AI Assistant)
/// </summary>
public partial class Tutorial : Control
{
    // ═══════════════════════════════════════════════════════════
    // COLORES & ESTILO
    // ═══════════════════════════════════════════════════════════
    private static readonly Color ELLIOT_COLOR = new Color("#00d4ff"); // Cyan AI
    private static readonly Color USER_COLOR = new Color("#ffaa00");   // Orange User
    private static readonly Color SUCCESS_COLOR = new Color("#50fa7b");
    
    private enum Step { Intro, Movement, Aiming, Shooting, Dash, Encyclopedia, Complete }
    private Step _currentStep = Step.Intro;

    // UI
    private RichTextLabel _dialogueText;
    private Label _stepTitle;
    private ProgressBar _progressBar;
    private HBoxContainer _keysContainer;
    
    // Tracking
    private HashSet<string> _movedDirs = new HashSet<string>();
    private int _shotsFired = 0;
    private float _mouseMovedDist = 0f;
    private Vector2 _lastMousePos;

    public override void _Ready()
    {
        CreateUI();
        StartStep(Step.Intro);
    }

    private void CreateUI()
    {
        // Fondo oscuro semitransparente
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.85f);
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Contenedor principal
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(LayoutPreset.Center);
        mainContainer.GrowHorizontal = GrowDirection.Both;
        mainContainer.GrowVertical = GrowDirection.Both;
        mainContainer.CustomMinimumSize = new Vector2(800, 500);
        mainContainer.AddThemeConstantOverride("separation", 20);
        AddChild(mainContainer);

        // Header: ELLIOT
        var header = new HBoxContainer();
        header.Alignment = BoxContainer.AlignmentMode.Center;
        mainContainer.AddChild(header);

        var elliotLabel = new Label();
        elliotLabel.Text = "ELLIOT_SYSTEM_v2.4 :: TRAINING_PROTOCOL";
        elliotLabel.AddThemeColorOverride("font_color", ELLIOT_COLOR);
        elliotLabel.AddThemeFontSizeOverride("font_size", 14);
        header.AddChild(elliotLabel);

        // Título del paso
        _stepTitle = new Label();
        _stepTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _stepTitle.AddThemeFontSizeOverride("font_size", 32);
        _stepTitle.AddThemeColorOverride("font_color", Colors.White);
        mainContainer.AddChild(_stepTitle);

        // Caja de diálogo
        var dialogPanel = new Panel();
        dialogPanel.CustomMinimumSize = new Vector2(0, 150);
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f);
        style.BorderColor = ELLIOT_COLOR;
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        dialogPanel.AddThemeStyleboxOverride("panel", style);
        mainContainer.AddChild(dialogPanel);

        _dialogueText = new RichTextLabel();
        _dialogueText.BbcodeEnabled = true;
        _dialogueText.SetAnchorsPreset(LayoutPreset.FullRect);
        _dialogueText.OffsetLeft = 20;
        _dialogueText.OffsetTop = 20;
        _dialogueText.OffsetRight = -20;
        _dialogueText.OffsetBottom = -20;
        _dialogueText.AddThemeFontSizeOverride("normal_font_size", 20);
        dialogPanel.AddChild(_dialogueText);

        // Visualización de teclas
        _keysContainer = new HBoxContainer();
        _keysContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _keysContainer.CustomMinimumSize = new Vector2(0, 80);
        mainContainer.AddChild(_keysContainer);

        // Barra de progreso
        _progressBar = new ProgressBar();
        _progressBar.CustomMinimumSize = new Vector2(0, 10);
        _progressBar.ShowPercentage = false;
        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = ELLIOT_COLOR;
        _progressBar.AddThemeStyleboxOverride("fill", fillStyle);
        mainContainer.AddChild(_progressBar);

        // Botón Skip
        var skipBtn = new Button();
        skipBtn.Text = "[ESC] SALTAR ENTRENAMIENTO";
        skipBtn.SetAnchorsPreset(LayoutPreset.TopRight);
        skipBtn.Position = new Vector2(-220, 20);
        skipBtn.Pressed += () => GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
        AddChild(skipBtn);
    }

    private void StartStep(Step step)
    {
        _currentStep = step;
        _keysContainer.QueueFreeChildren(); // Limpiar teclas anteriores

        switch (step)
        {
            case Step.Intro:
                _stepTitle.Text = "INICIALIZANDO...";
                SetDialogue("Hola, Operador. Soy [color=#00d4ff]ELLIOT[/color], tu asistente de ciberseguridad.\nVamos a calibrar los sistemas de tu nave antes de entrar a la red.\n\nPresiona [color=#ffaa00]SPACE[/color] para comenzar.");
                ShowKey("SPACE");
                break;

            case Step.Movement:
                _stepTitle.Text = "NAVEGACIÓN";
                SetDialogue("Usa [color=#ffaa00]W A S D[/color] para moverte en 8 direcciones.\nNecesito verificar que los propulsores responden correctamente.");
                ShowWASD();
                break;

            case Step.Aiming:
                _stepTitle.Text = "SISTEMA DE APUNTADO";
                SetDialogue("Tu torreta sigue el cursor del mouse.\n[color=#ffaa00]Mueve el mouse[/color] para calibrar los sensores de puntería.");
                _lastMousePos = GetViewport().GetMousePosition();
                ShowKey("MOUSE", "MOVER");
                break;

            case Step.Shooting:
                _stepTitle.Text = "ARMAMENTO";
                SetDialogue("Haz [color=#ffaa00]CLICK IZQUIERDO[/color] para disparar hacia donde apuntas.\nDestruye los objetivos virtuales (5 disparos).");
                ShowKey("LMB", "DISPARAR");
                break;

            case Step.Dash:
                _stepTitle.Text = "EVASIÓN DE EMERGENCIA";
                SetDialogue("Si te ves rodeado, presiona [color=#ffaa00]ESPACIO[/color] para activar el [b]Panic Burst[/b].\nEsto empujará a los enemigos y limpiará proyectiles cercanos.");
                ShowKey("SPACE", "PANIC BURST");
                break;

            case Step.Encyclopedia:
                _stepTitle.Text = "BASE DE CONOCIMIENTO";
                SetDialogue("He cargado la [color=#00d4ff]ENCICLOPEDIA DE AMENAZAS[/color] en tu sistema.\nSi encuentras un malware desconocido, consúltala en el menú principal para saber cómo combatirlo.");
                ShowKey("ENTER", "ENTENDIDO");
                break;

            case Step.Complete:
                _stepTitle.Text = "CALIBRACIÓN COMPLETADA";
                SetDialogue("[color=#50fa7b]Sistemas al 100%.[/color]\nEstás listo para defender la red, Operador. Buena suerte.\n\nPresiona cualquier tecla para iniciar misión.");
                break;
        }
        
        UpdateProgress();
    }

    private void SetDialogue(string text)
    {
        _dialogueText.Text = text;
        // Efecto de tipeo simple
        _dialogueText.VisibleCharacters = 0;
        var tween = CreateTween();
        tween.TweenProperty(_dialogueText, "visible_characters", text.Length, 1.0f);
    }

    private void ShowKey(string key, string label = "")
    {
        var panel = new PanelContainer();
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.2f, 0.2f, 0.2f);
        style.BorderColor = Colors.White;
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(4);
        style.ContentMarginLeft = 15;
        style.ContentMarginRight = 15;
        style.ContentMarginTop = 10;
        style.ContentMarginBottom = 10;
        panel.AddThemeStyleboxOverride("panel", style);
        
        var l = new Label();
        l.Text = string.IsNullOrEmpty(label) ? key : $"{key}\n{label}";
        l.HorizontalAlignment = HorizontalAlignment.Center;
        panel.AddChild(l);
        
        _keysContainer.AddChild(panel);
    }

    private void ShowWASD()
    {
        ShowKey("W");
        ShowKey("A");
        ShowKey("S");
        ShowKey("D");
    }

    public override void _Process(double delta)
    {
        if (_currentStep == Step.Movement)
        {
            if (Input.IsActionPressed("move_up")) _movedDirs.Add("up");
            if (Input.IsActionPressed("move_down")) _movedDirs.Add("down");
            if (Input.IsActionPressed("move_left")) _movedDirs.Add("left");
            if (Input.IsActionPressed("move_right")) _movedDirs.Add("right");

            if (_movedDirs.Count >= 4)
            {
                StartStep(Step.Aiming);
            }
        }
        else if (_currentStep == Step.Aiming)
        {
            Vector2 currentMouse = GetViewport().GetMousePosition();
            _mouseMovedDist += currentMouse.DistanceTo(_lastMousePos);
            _lastMousePos = currentMouse;

            if (_mouseMovedDist > 500f) // Mover mouse un poco
            {
                StartStep(Step.Shooting);
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
            return;
        }

        if (_currentStep == Step.Intro && @event.IsActionPressed("dash")) // Space
        {
            StartStep(Step.Movement);
        }
        else if (_currentStep == Step.Shooting && @event.IsActionPressed("fire")) // Click
        {
            _shotsFired++;
            if (_shotsFired >= 5)
            {
                StartStep(Step.Dash);
            }
        }
        else if (_currentStep == Step.Dash && @event.IsActionPressed("dash")) // Space
        {
            StartStep(Step.Encyclopedia);
        }
        else if (_currentStep == Step.Encyclopedia && @event.IsActionPressed("ui_accept")) // Enter
        {
            StartStep(Step.Complete);
        }
        else if (_currentStep == Step.Complete && @event is InputEventKey k && k.Pressed)
        {
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
        }
    }

    private void UpdateProgress()
    {
        float progress = (float)_currentStep / (float)Step.Complete;
        _progressBar.Value = progress * 100f;
    }
}

// Extension method helper
public static class NodeExtensions
{
    public static void QueueFreeChildren(this Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            child.QueueFree();
        }
    }
}