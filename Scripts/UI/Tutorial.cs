using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Tutorial rediseñado con estética web y UX mejorada
/// - Selección de modo: Quick/Full/Skip
/// - Pasos reducidos y claros
/// - Manejo de errores robusto
/// - Estilo visual consistente con la web
/// </summary>
public partial class Tutorial : Control
{
    // ═══════════════════════════════════════════════════════════
    // COLORES WEB
    // ═══════════════════════════════════════════════════════════
    private static readonly Color BG_COLOR = new Color("#050505");
    private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
    private static readonly Color TERMINAL_DIM = new Color("#008F11");
    private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
    private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
    private static readonly Color ALERT_RED = new Color("#ff5555");
    private static readonly Color SUCCESS_GREEN = new Color("#50fa7b");

    // ═══════════════════════════════════════════════════════════
    // ESTADOS Y MODOS
    // ═══════════════════════════════════════════════════════════
    private enum TutorialMode { Selection, Quick, Full }
    private enum QuickStep { Movement, Shooting, Complete }
    private enum FullStep { Movement, Shooting, Dash, CpuBasics, Parry, Complete }

    private TutorialMode _mode = TutorialMode.Selection;
    private QuickStep _quickStep = QuickStep.Movement;
    private FullStep _fullStep = FullStep.Movement;

    // ═══════════════════════════════════════════════════════════
    // UI ELEMENTS
    // ═══════════════════════════════════════════════════════════
    private Panel _terminalWindow;
    private Panel _selectionPanel;
    private VBoxContainer _selectionButtons;
    private Panel _trainingPanel;
    private Label _stepLabel;
    private Label _titleLabel;
    private RichTextLabel _instructionLabel;
    private Label _progressLabel;
    private HBoxContainer _keysDisplay;
    private ColorRect _scanlines;
    private Button _skipButton;

    // ═══════════════════════════════════════════════════════════
    // TRACKING
    // ═══════════════════════════════════════════════════════════
    private HashSet<string> _movedDirections = new HashSet<string>();
    private int _shotsFired = 0;
    private int _dashesPerformed = 0;
    private int _parriesPerformed = 0;
    private bool _cpuUnderstood = false;
    private float _timer = 0f;
    private float _hintTimer = 0f;
    private const float HINT_DELAY = 8f;

    // ═══════════════════════════════════════════════════════════
    // READY - Con manejo de errores
    // ═══════════════════════════════════════════════════════════
    public override void _Ready()
    {
        try
        {
            CreateUI();
            ShowModeSelection();
        }
        catch (Exception e)
        {
            GD.PrintErr($"[Tutorial] Error en inicialización: {e.Message}");
            // Fallback seguro: ir al menú principal
            GetTree().CreateTimer(1.0f).Timeout += () => 
                GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }
    }

    private void CreateUI()
    {
        // Fondo
        var bg = new ColorRect();
        bg.Color = BG_COLOR;
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Terminal Window principal
        _terminalWindow = CreateTerminalWindow();
        _terminalWindow.SetAnchorsPreset(LayoutPreset.Center);
        _terminalWindow.CustomMinimumSize = new Vector2(700, 450);
        _terminalWindow.GrowHorizontal = GrowDirection.Both;
        _terminalWindow.GrowVertical = GrowDirection.Both;
        AddChild(_terminalWindow);

        // Panel de selección de modo
        _selectionPanel = new Panel();
        _selectionPanel.SetAnchorsPreset(LayoutPreset.FullRect);
        var selStyle = new StyleBoxFlat();
        selStyle.BgColor = new Color(0, 0, 0, 0);
        _selectionPanel.AddThemeStyleboxOverride("panel", selStyle);
        _terminalWindow.AddChild(_selectionPanel);
        CreateSelectionUI();

        // Panel de entrenamiento
        _trainingPanel = new Panel();
        _trainingPanel.SetAnchorsPreset(LayoutPreset.FullRect);
        _trainingPanel.Visible = false;
        var trainStyle = new StyleBoxFlat();
        trainStyle.BgColor = new Color(0, 0, 0, 0);
        _trainingPanel.AddThemeStyleboxOverride("panel", trainStyle);
        _terminalWindow.AddChild(_trainingPanel);
        CreateTrainingUI();

        // Skip button (siempre visible)
        _skipButton = new Button();
        _skipButton.Text = "[ESC] EXIT";
        _skipButton.SetAnchorsPreset(LayoutPreset.TopRight);
        _skipButton.Position = new Vector2(-120, 10);
        _skipButton.CustomMinimumSize = new Vector2(100, 35);
        _skipButton.AddThemeColorOverride("font_color", TERMINAL_DIM);
        _skipButton.AddThemeFontSizeOverride("font_size", 14);
        var skipStyle = new StyleBoxFlat();
        skipStyle.BgColor = new Color(0, 0, 0, 0);
        _skipButton.AddThemeStyleboxOverride("normal", skipStyle);
        _skipButton.AddThemeStyleboxOverride("hover", skipStyle);
        _skipButton.Pressed += OnSkipPressed;
        AddChild(_skipButton);

        // Scanlines
        _scanlines = new ColorRect();
        _scanlines.SetAnchorsPreset(LayoutPreset.FullRect);
        _scanlines.MouseFilter = MouseFilterEnum.Ignore;
        _scanlines.Color = new Color(0, 1, 0.3f, 0.02f);
        AddChild(_scanlines);
    }

    private Panel CreateTerminalWindow()
    {
        var panel = new Panel();
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0, 0, 0, 0.95f);
        style.BorderColor = RIPPIER_PURPLE;
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        style.ShadowColor = new Color(RIPPIER_PURPLE, 0.3f);
        style.ShadowSize = 20;
        panel.AddThemeStyleboxOverride("panel", style);

        // Header con dots
        var header = new Panel();
        header.SetAnchorsPreset(LayoutPreset.TopWide);
        header.OffsetBottom = 40;
        var headerStyle = new StyleBoxFlat();
        headerStyle.BgColor = new Color("#0d0d0d");
        headerStyle.SetCornerRadiusAll(8);
        header.AddThemeStyleboxOverride("panel", headerStyle);
        panel.AddChild(header);

        var dots = new HBoxContainer();
        dots.Position = new Vector2(15, 12);
        dots.AddThemeConstantOverride("separation", 8);
        header.AddChild(dots);

        foreach (var c in new[] { "#ff5f56", "#ffbd2e", "#27c93f" })
        {
            var dot = new ColorRect();
            dot.CustomMinimumSize = new Vector2(14, 14);
            dot.Color = new Color(c);
            dots.AddChild(dot);
        }

        var headerTitle = new Label();
        headerTitle.Text = "training_protocol.exe";
        headerTitle.Position = new Vector2(85, 10);
        headerTitle.AddThemeColorOverride("font_color", new Color("#888888"));
        headerTitle.AddThemeFontSizeOverride("font_size", 14);
        header.AddChild(headerTitle);

        return panel;
    }

    // ═══════════════════════════════════════════════════════════
    // SELECTION UI - Elegir modo de tutorial
    // ═══════════════════════════════════════════════════════════
    private void CreateSelectionUI()
    {
        var container = new VBoxContainer();
        container.SetAnchorsPreset(LayoutPreset.FullRect);
        container.OffsetTop = 60;
        container.OffsetLeft = 40;
        container.OffsetRight = -40;
        container.OffsetBottom = -30;
        container.AddThemeConstantOverride("separation", 20);
        _selectionPanel.AddChild(container);

        // Título
        var title = new Label();
        title.Text = ">>> OPERATOR TRAINING <<<";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeColorOverride("font_color", FLUX_ORANGE);
        title.AddThemeFontSizeOverride("font_size", 26);
        container.AddChild(title);

        // Subtítulo
        var subtitle = new Label();
        subtitle.Text = "Select training intensity:";
        subtitle.HorizontalAlignment = HorizontalAlignment.Center;
        subtitle.AddThemeColorOverride("font_color", TERMINAL_DIM);
        subtitle.AddThemeFontSizeOverride("font_size", 16);
        container.AddChild(subtitle);

        // Spacer
        container.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

        // Botones de selección
        _selectionButtons = new VBoxContainer();
        _selectionButtons.AddThemeConstantOverride("separation", 15);
        _selectionButtons.Alignment = BoxContainer.AlignmentMode.Center;
        container.AddChild(_selectionButtons);

        // [1] QUICK START
        var quickBtn = CreateModeButton(
            "[1] QUICK_START", 
            "Learn basics in 2 steps (recommended)",
            TERMINAL_GREEN
        );
        quickBtn.Pressed += () => StartTraining(TutorialMode.Quick);
        _selectionButtons.AddChild(quickBtn);

        // [2] FULL TRAINING
        var fullBtn = CreateModeButton(
            "[2] FULL_TRAINING", 
            "Complete training: 5 modules",
            FLUX_ORANGE
        );
        fullBtn.Pressed += () => StartTraining(TutorialMode.Full);
        _selectionButtons.AddChild(fullBtn);

        // [3] SKIP
        var skipBtn = CreateModeButton(
            "[3] SKIP_PROTOCOL", 
            "I know how to operate this system",
            TERMINAL_DIM
        );
        skipBtn.Pressed += () => GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
        _selectionButtons.AddChild(skipBtn);

        // Recomendación
        var rec = new Label();
        rec.Text = "\n★ New operators: Select QUICK_START";
        rec.HorizontalAlignment = HorizontalAlignment.Center;
        rec.AddThemeColorOverride("font_color", SUCCESS_GREEN);
        rec.AddThemeFontSizeOverride("font_size", 14);
        container.AddChild(rec);
    }

    private Button CreateModeButton(string title, string desc, Color color)
    {
        var btn = new Button();
        btn.CustomMinimumSize = new Vector2(500, 70);
        btn.FocusMode = FocusModeEnum.All;

        var vbox = new VBoxContainer();
        vbox.MouseFilter = MouseFilterEnum.Ignore;
        btn.AddChild(vbox);

        var titleLbl = new Label();
        titleLbl.Text = title;
        titleLbl.HorizontalAlignment = HorizontalAlignment.Left;
        titleLbl.AddThemeColorOverride("font_color", color);
        titleLbl.AddThemeFontSizeOverride("font_size", 20);
        titleLbl.MouseFilter = MouseFilterEnum.Ignore;
        vbox.AddChild(titleLbl);

        var descLbl = new Label();
        descLbl.Text = "    " + desc;
        descLbl.HorizontalAlignment = HorizontalAlignment.Left;
        descLbl.AddThemeColorOverride("font_color", new Color("#666666"));
        descLbl.AddThemeFontSizeOverride("font_size", 14);
        descLbl.MouseFilter = MouseFilterEnum.Ignore;
        vbox.AddChild(descLbl);

        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0, 0, 0, 0);
        normalStyle.BorderColor = new Color(color, 0.3f);
        normalStyle.SetBorderWidthAll(1);
        normalStyle.SetCornerRadiusAll(5);
        normalStyle.ContentMarginLeft = 20;
        normalStyle.ContentMarginTop = 10;
        btn.AddThemeStyleboxOverride("normal", normalStyle);

        var hoverStyle = new StyleBoxFlat();
        hoverStyle.BgColor = new Color(color, 0.15f);
        hoverStyle.BorderColor = color;
        hoverStyle.SetBorderWidthAll(2);
        hoverStyle.SetCornerRadiusAll(5);
        hoverStyle.ContentMarginLeft = 20;
        hoverStyle.ContentMarginTop = 10;
        btn.AddThemeStyleboxOverride("hover", hoverStyle);
        btn.AddThemeStyleboxOverride("focus", hoverStyle);

        return btn;
    }

    // ═══════════════════════════════════════════════════════════
    // TRAINING UI - Panel de entrenamiento activo
    // ═══════════════════════════════════════════════════════════
    private void CreateTrainingUI()
    {
        var container = new VBoxContainer();
        container.SetAnchorsPreset(LayoutPreset.FullRect);
        container.OffsetTop = 55;
        container.OffsetLeft = 30;
        container.OffsetRight = -30;
        container.OffsetBottom = -25;
        container.AddThemeConstantOverride("separation", 12);
        _trainingPanel.AddChild(container);

        // Step label
        _stepLabel = new Label();
        _stepLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _stepLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
        _stepLabel.AddThemeFontSizeOverride("font_size", 14);
        container.AddChild(_stepLabel);

        // Title
        _titleLabel = new Label();
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        container.AddChild(_titleLabel);

        // Separator
        var sep = new HSeparator();
        sep.AddThemeConstantOverride("separation", 10);
        container.AddChild(sep);

        // Instructions
        _instructionLabel = new RichTextLabel();
        _instructionLabel.BbcodeEnabled = true;
        _instructionLabel.FitContent = true;
        _instructionLabel.CustomMinimumSize = new Vector2(0, 80);
        _instructionLabel.AddThemeFontSizeOverride("normal_font_size", 18);
        _instructionLabel.AddThemeColorOverride("default_color", Colors.White);
        container.AddChild(_instructionLabel);

        // Keys display
        _keysDisplay = new HBoxContainer();
        _keysDisplay.Alignment = BoxContainer.AlignmentMode.Center;
        _keysDisplay.AddThemeConstantOverride("separation", 20);
        container.AddChild(_keysDisplay);

        // Progress
        _progressLabel = new Label();
        _progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _progressLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
        _progressLabel.AddThemeFontSizeOverride("font_size", 16);
        container.AddChild(_progressLabel);

        // Hint
        var hintLabel = new Label();
        hintLabel.Name = "HintLabel";
        hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        hintLabel.AddThemeColorOverride("font_color", new Color("#555555"));
        hintLabel.AddThemeFontSizeOverride("font_size", 12);
        hintLabel.Text = "";
        container.AddChild(hintLabel);
    }

    // ═══════════════════════════════════════════════════════════
    // FLOW CONTROL
    // ═══════════════════════════════════════════════════════════
    private void ShowModeSelection()
    {
        _mode = TutorialMode.Selection;
        _selectionPanel.Visible = true;
        _trainingPanel.Visible = false;

        // Focus primer botón
        GetTree().CreateTimer(0.1f).Timeout += () => {
            var btns = _selectionButtons.GetChildren();
            if (btns.Count > 0 && btns[0] is Button b) b.GrabFocus();
        };
    }

    private void StartTraining(TutorialMode mode)
    {
        _mode = mode;
        _selectionPanel.Visible = false;
        _trainingPanel.Visible = true;

        // Reset tracking
        _movedDirections.Clear();
        _shotsFired = 0;
        _dashesPerformed = 0;
        _parriesPerformed = 0;
        _cpuUnderstood = false;
        _hintTimer = 0;

        if (mode == TutorialMode.Quick)
        {
            _quickStep = QuickStep.Movement;
        }
        else
        {
            _fullStep = FullStep.Movement;
        }

        UpdateTrainingUI();
    }

    private void UpdateTrainingUI()
    {
        ClearKeysDisplay();
        _hintTimer = 0;

        if (_mode == TutorialMode.Quick)
        {
            UpdateQuickUI();
        }
        else
        {
            UpdateFullUI();
        }
    }

    private void UpdateQuickUI()
    {
        int step = (int)_quickStep + 1;
        int total = 2;
        _stepLabel.Text = $"QUICK_START :: Module {step}/{total}";

        switch (_quickStep)
        {
            case QuickStep.Movement:
                _titleLabel.Text = "NAVIGATION";
                _instructionLabel.Text = "Move your ship using [color=#ffaa00][b]W A S D[/b][/color] keys.\nTest all 4 directions to calibrate thrusters.";
                _progressLabel.Text = "Directions: 0/4";
                ShowWASDKeys();
                break;

            case QuickStep.Shooting:
                _titleLabel.Text = "WEAPONS";
                _instructionLabel.Text = "Press [color=#ffaa00][b]LEFT CLICK[/b][/color] or [color=#ffaa00][b]SPACE[/b][/color] to fire.\nExecute 5 attack scripts.";
                _progressLabel.Text = "Shots: 0/5";
                ShowKey("LMB / SPACE", "Fire");
                break;

            case QuickStep.Complete:
                ShowComplete();
                break;
        }
    }

    private void UpdateFullUI()
    {
        int step = (int)_fullStep + 1;
        int total = 5;
        _stepLabel.Text = $"FULL_TRAINING :: Module {step}/{total}";

        switch (_fullStep)
        {
            case FullStep.Movement:
                _titleLabel.Text = "NAVIGATION";
                _instructionLabel.Text = "Move your ship using [color=#ffaa00][b]W A S D[/b][/color] keys.\nTest all 4 directions.";
                _progressLabel.Text = "Directions: 0/4";
                ShowWASDKeys();
                break;

            case FullStep.Shooting:
                _titleLabel.Text = "WEAPONS";
                _instructionLabel.Text = "Press [color=#ffaa00][b]LEFT CLICK[/b][/color] or [color=#ffaa00][b]SPACE[/b][/color] to fire.";
                _progressLabel.Text = "Shots: 0/5";
                ShowKey("LMB / SPACE", "Fire");
                break;

            case FullStep.Dash:
                _titleLabel.Text = "DASH";
                _instructionLabel.Text = "Press [color=#ffaa00][b]SHIFT[/b][/color] while moving to dash.\nQuick evasion maneuver. Uses CPU.";
                _progressLabel.Text = "Dashes: 0/3";
                ShowKey("SHIFT", "Dash");
                break;

            case FullStep.CpuBasics:
                _titleLabel.Text = "CPU MANAGEMENT";
                _instructionLabel.Text = "Actions generate [color=#ff5555]HEAT[/color]. Watch the CPU bar.\n[color=#00ff41]Low heat[/color] = Precision  |  [color=#ff5555]High heat[/color] = Chaos\nPress [color=#ffaa00][b]ENTER[/b][/color] when you understand.";
                _progressLabel.Text = "Press ENTER to confirm";
                ShowKey("ENTER", "Understood");
                break;

            case FullStep.Parry:
                _titleLabel.Text = "PARRY";
                _instructionLabel.Text = "Press [color=#ffaa00][b]RIGHT CLICK[/b][/color] to parry.\nReflects projectiles and cools CPU!";
                _progressLabel.Text = "Parries: 0/2";
                ShowKey("RMB", "Parry");
                break;

            case FullStep.Complete:
                ShowComplete();
                break;
        }
    }

    private void ShowComplete()
    {
        _stepLabel.Text = "STATUS: COMPLETE";
        _titleLabel.Text = "TRAINING COMPLETE";
        _titleLabel.AddThemeColorOverride("font_color", SUCCESS_GREEN);
        _instructionLabel.Text = "[center]System calibrated. You are ready for deployment.\n\n[color=#ffaa00]Press any key to begin mission...[/color][/center]";
        _progressLabel.Text = "";
        ClearKeysDisplay();

        // Flash de éxito
        FlashSuccess();
    }

    // ═══════════════════════════════════════════════════════════
    // KEY DISPLAY HELPERS
    // ═══════════════════════════════════════════════════════════
    private void ClearKeysDisplay()
    {
        foreach (Node child in _keysDisplay.GetChildren())
            child.QueueFree();
    }

    private void ShowWASDKeys()
    {
        var grid = new GridContainer();
        grid.Columns = 3;
        grid.AddThemeConstantOverride("h_separation", 5);
        grid.AddThemeConstantOverride("v_separation", 5);
        _keysDisplay.AddChild(grid);

        // Row 1: _ W _
        grid.AddChild(CreateKeyBox("", false, ""));
        grid.AddChild(CreateKeyBox("W", !_movedDirections.Contains("up"), "up"));
        grid.AddChild(CreateKeyBox("", false, ""));

        // Row 2: A S D
        grid.AddChild(CreateKeyBox("A", !_movedDirections.Contains("left"), "left"));
        grid.AddChild(CreateKeyBox("S", !_movedDirections.Contains("down"), "down"));
        grid.AddChild(CreateKeyBox("D", !_movedDirections.Contains("right"), "right"));
    }

    private Control CreateKeyBox(string key, bool pending, string dir)
    {
        if (string.IsNullOrEmpty(key))
        {
            return new Control { CustomMinimumSize = new Vector2(50, 50) };
        }

        var box = new Panel();
        box.Name = $"Key_{dir}";
        box.CustomMinimumSize = new Vector2(50, 50);

        var style = new StyleBoxFlat();
        style.BgColor = pending ? new Color(0.1f, 0.1f, 0.1f) : new Color(SUCCESS_GREEN, 0.3f);
        style.BorderColor = pending ? TERMINAL_DIM : SUCCESS_GREEN;
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(5);
        box.AddThemeStyleboxOverride("panel", style);

        var lbl = new Label();
        lbl.Text = key;
        lbl.HorizontalAlignment = HorizontalAlignment.Center;
        lbl.VerticalAlignment = VerticalAlignment.Center;
        lbl.SetAnchorsPreset(LayoutPreset.FullRect);
        lbl.AddThemeColorOverride("font_color", pending ? Colors.White : SUCCESS_GREEN);
        lbl.AddThemeFontSizeOverride("font_size", 20);
        box.AddChild(lbl);

        return box;
    }

    private void ShowKey(string key, string action)
    {
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 15);
        _keysDisplay.AddChild(hbox);

        var keyBox = new Panel();
        keyBox.CustomMinimumSize = new Vector2(120, 50);
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.1f);
        style.BorderColor = FLUX_ORANGE;
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(5);
        keyBox.AddThemeStyleboxOverride("panel", style);
        hbox.AddChild(keyBox);

        var keyLbl = new Label();
        keyLbl.Text = key;
        keyLbl.HorizontalAlignment = HorizontalAlignment.Center;
        keyLbl.VerticalAlignment = VerticalAlignment.Center;
        keyLbl.SetAnchorsPreset(LayoutPreset.FullRect);
        keyLbl.AddThemeColorOverride("font_color", FLUX_ORANGE);
        keyLbl.AddThemeFontSizeOverride("font_size", 18);
        keyBox.AddChild(keyLbl);

        var actionLbl = new Label();
        actionLbl.Text = "→ " + action;
        actionLbl.AddThemeColorOverride("font_color", TERMINAL_DIM);
        actionLbl.AddThemeFontSizeOverride("font_size", 16);
        hbox.AddChild(actionLbl);
    }

    private void UpdateKeyVisual(string dir)
    {
        var keyNode = _keysDisplay.FindChild($"Key_{dir}", true, false) as Panel;
        if (keyNode != null)
        {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(SUCCESS_GREEN, 0.3f);
            style.BorderColor = SUCCESS_GREEN;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(5);
            keyNode.AddThemeStyleboxOverride("panel", style);

            var lbl = keyNode.GetChild(0) as Label;
            if (lbl != null) lbl.AddThemeColorOverride("font_color", SUCCESS_GREEN);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PROCESS - Input tracking
    // ═══════════════════════════════════════════════════════════
    public override void _Process(double delta)
    {
        _timer += (float)delta;
        float alpha = 0.015f + 0.008f * Mathf.Sin(_timer * 3);
        _scanlines.Color = new Color(0, 1, 0.3f, alpha);

        if (_mode == TutorialMode.Selection) return;

        // Hint timer
        _hintTimer += (float)delta;
        if (_hintTimer > HINT_DELAY)
        {
            ShowHint();
            _hintTimer = 0;
        }

        // Track movement
        TrackMovement();
    }

    private void TrackMovement()
    {
        bool isMovementStep = (_mode == TutorialMode.Quick && _quickStep == QuickStep.Movement) ||
                              (_mode == TutorialMode.Full && _fullStep == FullStep.Movement);

        if (!isMovementStep) return;

        bool changed = false;
        if (Input.IsActionPressed("move_up") && !_movedDirections.Contains("up"))
        {
            _movedDirections.Add("up");
            UpdateKeyVisual("up");
            changed = true;
        }
        if (Input.IsActionPressed("move_down") && !_movedDirections.Contains("down"))
        {
            _movedDirections.Add("down");
            UpdateKeyVisual("down");
            changed = true;
        }
        if (Input.IsActionPressed("move_left") && !_movedDirections.Contains("left"))
        {
            _movedDirections.Add("left");
            UpdateKeyVisual("left");
            changed = true;
        }
        if (Input.IsActionPressed("move_right") && !_movedDirections.Contains("right"))
        {
            _movedDirections.Add("right");
            UpdateKeyVisual("right");
            changed = true;
        }

        if (changed)
        {
            _hintTimer = 0;
            _progressLabel.Text = $"Directions: {_movedDirections.Count}/4";

            if (_movedDirections.Count >= 4)
            {
                AdvanceStep();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        // ESC siempre sale
        if (@event.IsActionPressed("ui_cancel"))
        {
            OnSkipPressed();
            return;
        }

        // Atajos de teclado en selección
        if (_mode == TutorialMode.Selection && @event is InputEventKey key && key.Pressed && !key.Echo)
        {
            switch (key.Keycode)
            {
                case Key.Key1: StartTraining(TutorialMode.Quick); return;
                case Key.Key2: StartTraining(TutorialMode.Full); return;
                case Key.Key3: GetTree().ChangeSceneToFile("res://Scenes/Main.tscn"); return;
            }
        }

        // Complete - cualquier tecla
        bool isComplete = (_mode == TutorialMode.Quick && _quickStep == QuickStep.Complete) ||
                          (_mode == TutorialMode.Full && _fullStep == FullStep.Complete);
        if (isComplete && @event is InputEventKey k && k.Pressed)
        {
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
            return;
        }

        // Shooting
        bool isShootingStep = (_mode == TutorialMode.Quick && _quickStep == QuickStep.Shooting) ||
                              (_mode == TutorialMode.Full && _fullStep == FullStep.Shooting);
        if (isShootingStep)
        {
            if (@event.IsActionPressed("fire") || (@event is InputEventKey ke && ke.Pressed && ke.Keycode == Key.Space))
            {
                _shotsFired++;
                _hintTimer = 0;
                _progressLabel.Text = $"Shots: {_shotsFired}/5";
                if (_shotsFired >= 5) AdvanceStep();
            }
        }

        // Full training specific
        if (_mode == TutorialMode.Full)
        {
            // Dash
            if (_fullStep == FullStep.Dash && @event is InputEventKey dk && dk.Pressed && !dk.Echo && dk.Keycode == Key.Shift)
            {
                _dashesPerformed++;
                _hintTimer = 0;
                _progressLabel.Text = $"Dashes: {_dashesPerformed}/3";
                if (_dashesPerformed >= 3) AdvanceStep();
            }

            // CPU understood
            if (_fullStep == FullStep.CpuBasics && @event is InputEventKey ck && ck.Pressed && ck.Keycode == Key.Enter)
            {
                _cpuUnderstood = true;
                AdvanceStep();
            }

            // Parry
            if (_fullStep == FullStep.Parry)
            {
                if (@event.IsActionPressed("parry") || (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Right))
                {
                    _parriesPerformed++;
                    _hintTimer = 0;
                    _progressLabel.Text = $"Parries: {_parriesPerformed}/2";
                    if (_parriesPerformed >= 2) AdvanceStep();
                }
            }
        }
    }

    private void AdvanceStep()
    {
        FlashSuccess();

        if (_mode == TutorialMode.Quick)
        {
            _quickStep++;
        }
        else
        {
            _fullStep++;
        }

        UpdateTrainingUI();
    }

    private void ShowHint()
    {
        var hintNode = _trainingPanel.FindChild("HintLabel", true, false) as Label;
        if (hintNode != null)
        {
            hintNode.Text = "💡 Check the highlighted keys above";
            hintNode.AddThemeColorOverride("font_color", FLUX_ORANGE);

            // Fade out
            var tween = CreateTween();
            tween.TweenProperty(hintNode, "modulate:a", 0.5f, 2.0f);
        }
    }

    private void FlashSuccess()
    {
        var flash = new ColorRect();
        flash.Color = new Color(SUCCESS_GREEN, 0);
        flash.SetAnchorsPreset(LayoutPreset.FullRect);
        flash.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(flash);

        var tween = CreateTween();
        tween.TweenProperty(flash, "color:a", 0.2f, 0.1f);
        tween.TweenProperty(flash, "color:a", 0f, 0.3f);
        tween.TweenCallback(Callable.From(flash.QueueFree));
    }

    private void OnSkipPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }
}
