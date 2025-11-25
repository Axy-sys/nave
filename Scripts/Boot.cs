using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boot Screen - CYBERPUNK INDIE EXPERIENCE
/// 
/// REFERENCIAS DE DISEÑO:
/// - Transistor: Logo elegante con partículas
/// - Celeste: Feedback inmediato, transiciones fluidas
/// - Hotline Miami: Neón agresivo, glitch effects
/// - Enter the Gungeon: Loading con personalidad
/// - Furi: Colores vibrantes, minimalismo impactante
/// 
/// ═══════════════════════════════════════════════════════════════════════════
/// UX FLOW SIMULADO:
/// ═══════════════════════════════════════════════════════════════════════════
/// 
/// [0.0s] Usuario inicia el juego
///        → Pantalla negra total (build tension)
/// 
/// [0.3s] Glitch inicial - líneas de código aparecen
///        → Establece estética "hacking"
/// 
/// [0.8s] Logo "CODE RIPPIER" aparece con efecto de scan
///        → Character reveal estilo terminal
/// 
/// [1.2s] Barra de carga comienza (fake loading con frases)
///        → "Initializing firewall..."
///        → "Loading threat database..."
///        → "Bypassing security..."
///        → "System ready..."
/// 
/// [3.0s] "PRESS ANY KEY" aparece con glow pulsante
///        → Input habilitado
/// 
/// [5.5s] Auto-skip si usuario no presiona
///        → No frustrar al usuario que ya vio esto
/// 
/// [SKIP] Flash + glitch de transición + fade
///        → Transición memorable
/// 
/// ═══════════════════════════════════════════════════════════════════════════
/// MANEJO DE ERRORES:
/// ═══════════════════════════════════════════════════════════════════════════
/// - Input deshabilitado primeros 0.8s (evitar skip accidental)
/// - Cualquier input válido funciona (teclado/mouse/gamepad)
/// - Si falla la transición → ir directo a MainMenu
/// - Partículas con límite (evitar lag en PCs bajas)
/// </summary>
public partial class Boot : Control
{
    // ═══════════════════════════════════════════════════════════════════════
    // PALETA CYBERPUNK - Colores consistentes con el juego
    // ═══════════════════════════════════════════════════════════════════════
    private static readonly Color BG_VOID = new Color("#000000");
    private static readonly Color BG_DARK = new Color("#030303");
    private static readonly Color NEON_CYAN = new Color("#00fff2");
    private static readonly Color NEON_PINK = new Color("#ff0080");
    private static readonly Color NEON_GREEN = new Color("#00ff41");
    private static readonly Color NEON_PURPLE = new Color("#bf00ff");
    private static readonly Color NEON_ORANGE = new Color("#ff6600");
    private static readonly Color WHITE_GLOW = new Color("#ffffff");
    private static readonly Color TERMINAL_DIM = new Color("#008F11");
    
    // ═══════════════════════════════════════════════════════════════════════
    // UI ELEMENTS
    // ═══════════════════════════════════════════════════════════════════════
    private ColorRect _background;
    private Control _glitchLayer;
    private Control _particleContainer;
    private Control _logoContainer;
    private Label _titleLabel;
    private Label _subtitleLabel;
    private Label _pressKeyLabel;
    private Panel _loadingBarBg;
    private ColorRect _loadingBarFill;
    private Label _loadingTextLabel;
    private Label _versionLabel;
    
    // Glitch elements
    private List<ColorRect> _glitchLines = new List<ColorRect>();
    private List<Label> _codeLines = new List<Label>();
    
    // Partículas
    private List<CyberParticle> _particles = new List<CyberParticle>();
    private const int MAX_PARTICLES = 60;
    
    // ═══════════════════════════════════════════════════════════════════════
    // STATE MACHINE
    // ═══════════════════════════════════════════════════════════════════════
    private enum BootPhase { Init, GlitchIn, LogoReveal, Loading, Ready, Transition }
    private BootPhase _phase = BootPhase.Init;
    
    private float _timer = 0f;
    private float _phaseTimer = 0f;
    private float _loadingProgress = 0f;
    private int _loadingStep = 0;
    private bool _canSkip = false;
    private bool _isTransitioning = false;
    private Random _rng = new Random();
    
    // Loading messages - Temática cybersecurity
    private string[] _loadingMessages = new string[]
    {
        "> Initializing neural firewall...",
        "> Loading threat database...",
        "> Calibrating defense matrices...",
        "> Bypassing legacy protocols...",
        "> Compiling counter-measures...",
        "> Establishing secure connection...",
        "> System armed. Ready for breach."
    };
    
    // Code lines for glitch effect
    private string[] _codeSnippets = new string[]
    {
        "0x7F45 :: FIREWALL_INIT",
        "LOAD NEURAL_NET v3.2.1",
        ">>> SCANNING PORTS...",
        "THREAT_LEVEL = CRITICAL",
        "for(int i=0; i<∞; i++)",
        "ENCRYPT(0xDEADBEEF)",
        "CONNECT >> 192.168.1.1",
        "AUTH_TOKEN: ************",
        "BYPASS_SECURITY(true);",
        "DEFENSE_MODE: ACTIVE"
    };

    // Timing constants
    private const float PHASE_INIT_DURATION = 0.3f;
    private const float PHASE_GLITCH_DURATION = 0.5f;
    private const float PHASE_LOGO_DURATION = 0.8f;
    private const float PHASE_LOADING_DURATION = 2.0f;
    private const float AUTO_SKIP_TIME = 5.5f;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        CreateUI();
        StartBootSequence();
    }

    private void CreateUI()
    {
        // ═══════════════════════════════════════════════════════════════════
        // LAYER 1: FONDO NEGRO ABSOLUTO
        // ═══════════════════════════════════════════════════════════════════
        _background = new ColorRect();
        _background.Color = BG_VOID;
        _background.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_background);

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 2: GLITCH LAYER (líneas y código)
        // ═══════════════════════════════════════════════════════════════════
        _glitchLayer = new Control();
        _glitchLayer.SetAnchorsPreset(LayoutPreset.FullRect);
        _glitchLayer.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_glitchLayer);
        
        // Pre-crear líneas de glitch
        for (int i = 0; i < 8; i++)
        {
            var line = new ColorRect();
            line.SetAnchorsPreset(LayoutPreset.TopWide);
            line.CustomMinimumSize = new Vector2(0, 2 + _rng.Next(4));
            line.Color = new Color(NEON_CYAN, 0);
            line.MouseFilter = MouseFilterEnum.Ignore;
            _glitchLayer.AddChild(line);
            _glitchLines.Add(line);
        }
        
        // Pre-crear líneas de código
        for (int i = 0; i < 6; i++)
        {
            var codeLine = new Label();
            codeLine.Position = new Vector2(20 + _rng.Next(100), 50 + i * 35);
            codeLine.Text = _codeSnippets[_rng.Next(_codeSnippets.Length)];
            codeLine.AddThemeColorOverride("font_color", new Color(NEON_GREEN, 0));
            codeLine.AddThemeFontSizeOverride("font_size", 11);
            codeLine.MouseFilter = MouseFilterEnum.Ignore;
            _glitchLayer.AddChild(codeLine);
            _codeLines.Add(codeLine);
        }

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 3: PARTÍCULAS FLOTANTES
        // ═══════════════════════════════════════════════════════════════════
        _particleContainer = new Control();
        _particleContainer.SetAnchorsPreset(LayoutPreset.FullRect);
        _particleContainer.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(_particleContainer);

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 4: LOGO CONTAINER
        // ═══════════════════════════════════════════════════════════════════
        _logoContainer = new Control();
        _logoContainer.SetAnchorsPreset(LayoutPreset.Center);
        _logoContainer.Modulate = new Color(1, 1, 1, 0);
        AddChild(_logoContainer);

        // Título con efecto especial
        _titleLabel = new Label();
        _titleLabel.Text = "";
        _titleLabel.Position = new Vector2(-220, -50);
        _titleLabel.CustomMinimumSize = new Vector2(440, 80);
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.VerticalAlignment = VerticalAlignment.Center;
        _titleLabel.AddThemeColorOverride("font_color", WHITE_GLOW);
        _titleLabel.AddThemeFontSizeOverride("font_size", 72);
        _logoContainer.AddChild(_titleLabel);

        // Línea decorativa animada
        var titleLine = new ColorRect();
        titleLine.Name = "TitleLine";
        titleLine.Position = new Vector2(-180, 40);
        titleLine.CustomMinimumSize = new Vector2(0, 3);
        titleLine.Color = NEON_CYAN;
        _logoContainer.AddChild(titleLine);

        // Subtítulo
        _subtitleLabel = new Label();
        _subtitleLabel.Text = "";
        _subtitleLabel.Position = new Vector2(-220, 55);
        _subtitleLabel.CustomMinimumSize = new Vector2(440, 30);
        _subtitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _subtitleLabel.AddThemeColorOverride("font_color", new Color(NEON_CYAN, 0.8f));
        _subtitleLabel.AddThemeFontSizeOverride("font_size", 16);
        _logoContainer.AddChild(_subtitleLabel);

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 5: LOADING BAR
        // ═══════════════════════════════════════════════════════════════════
        _loadingBarBg = new Panel();
        _loadingBarBg.SetAnchorsPreset(LayoutPreset.CenterBottom);
        _loadingBarBg.Position = new Vector2(-200, -140);
        _loadingBarBg.CustomMinimumSize = new Vector2(400, 8);
        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        bgStyle.BorderColor = NEON_CYAN;
        bgStyle.SetBorderWidthAll(1);
        bgStyle.SetCornerRadiusAll(2);
        _loadingBarBg.AddThemeStyleboxOverride("panel", bgStyle);
        _loadingBarBg.Modulate = new Color(1, 1, 1, 0);
        AddChild(_loadingBarBg);

        _loadingBarFill = new ColorRect();
        _loadingBarFill.Position = new Vector2(2, 2);
        _loadingBarFill.CustomMinimumSize = new Vector2(0, 4);
        _loadingBarFill.Color = NEON_CYAN;
        _loadingBarBg.AddChild(_loadingBarFill);

        // Loading text
        _loadingTextLabel = new Label();
        _loadingTextLabel.SetAnchorsPreset(LayoutPreset.CenterBottom);
        _loadingTextLabel.Position = new Vector2(-200, -165);
        _loadingTextLabel.CustomMinimumSize = new Vector2(400, 20);
        _loadingTextLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _loadingTextLabel.AddThemeColorOverride("font_color", NEON_GREEN);
        _loadingTextLabel.AddThemeFontSizeOverride("font_size", 12);
        _loadingTextLabel.Text = "";
        _loadingTextLabel.Modulate = new Color(1, 1, 1, 0);
        AddChild(_loadingTextLabel);

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 6: PRESS ANY KEY
        // ═══════════════════════════════════════════════════════════════════
        _pressKeyLabel = new Label();
        _pressKeyLabel.SetAnchorsPreset(LayoutPreset.CenterBottom);
        _pressKeyLabel.Position = new Vector2(-200, -80);
        _pressKeyLabel.CustomMinimumSize = new Vector2(400, 40);
        _pressKeyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _pressKeyLabel.AddThemeColorOverride("font_color", NEON_GREEN);
        _pressKeyLabel.AddThemeFontSizeOverride("font_size", 20);
        _pressKeyLabel.Text = "[ PRESS ANY KEY ]";
        _pressKeyLabel.Modulate = new Color(1, 1, 1, 0);
        AddChild(_pressKeyLabel);

        // ═══════════════════════════════════════════════════════════════════
        // LAYER 7: VERSION INFO
        // ═══════════════════════════════════════════════════════════════════
        _versionLabel = new Label();
        _versionLabel.SetAnchorsPreset(LayoutPreset.BottomRight);
        _versionLabel.Position = new Vector2(-160, -35);
        _versionLabel.CustomMinimumSize = new Vector2(150, 25);
        _versionLabel.HorizontalAlignment = HorizontalAlignment.Right;
        _versionLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 0.25f));
        _versionLabel.AddThemeFontSizeOverride("font_size", 11);
        _versionLabel.Text = "BUILD 2025.11 // ALPHA";
        AddChild(_versionLabel);
    }

    private void StartBootSequence()
    {
        _phase = BootPhase.Init;
        _phaseTimer = 0f;
        
        // Spawn partículas iniciales
        var viewport = GetViewportRect().Size;
        if (viewport.X == 0) viewport = new Vector2(1920, 1080);
        
        for (int i = 0; i < 30; i++)
        {
            SpawnParticle(viewport, true);
        }
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _timer += dt;
        _phaseTimer += dt;
        
        // Actualizar según fase
        switch (_phase)
        {
            case BootPhase.Init:
                ProcessInitPhase();
                break;
            case BootPhase.GlitchIn:
                ProcessGlitchPhase(dt);
                break;
            case BootPhase.LogoReveal:
                ProcessLogoRevealPhase(dt);
                break;
            case BootPhase.Loading:
                ProcessLoadingPhase(dt);
                break;
            case BootPhase.Ready:
                ProcessReadyPhase(dt);
                break;
            case BootPhase.Transition:
                // Esperar a que termine la transición
                break;
        }
        
        // Actualizar partículas siempre
        UpdateParticles(dt);
        
        // Actualizar glitch random
        if (_phase >= BootPhase.LogoReveal)
        {
            UpdateRandomGlitch(dt);
        }
        
        // Auto-skip
        if (_canSkip && _timer >= AUTO_SKIP_TIME && !_isTransitioning)
        {
            StartTransition();
        }
    }

    private void ProcessInitPhase()
    {
        if (_phaseTimer >= PHASE_INIT_DURATION)
        {
            _phase = BootPhase.GlitchIn;
            _phaseTimer = 0f;
            TriggerGlitchEffect();
        }
    }

    private void ProcessGlitchPhase(float dt)
    {
        // Mostrar código lines gradualmente
        float progress = _phaseTimer / PHASE_GLITCH_DURATION;
        for (int i = 0; i < _codeLines.Count; i++)
        {
            float lineProgress = Mathf.Clamp((progress - i * 0.15f) * 5f, 0, 1);
            _codeLines[i].AddThemeColorOverride("font_color", new Color(NEON_GREEN, lineProgress * 0.6f));
        }
        
        if (_phaseTimer >= PHASE_GLITCH_DURATION)
        {
            _phase = BootPhase.LogoReveal;
            _phaseTimer = 0f;
            StartLogoReveal();
        }
    }

    private void ProcessLogoRevealPhase(float dt)
    {
        // Typewriter effect para el título
        string fullTitle = "CODE RIPPIER";
        int charsToShow = (int)((_phaseTimer / PHASE_LOGO_DURATION) * (fullTitle.Length + 2));
        charsToShow = Mathf.Min(charsToShow, fullTitle.Length);
        
        string displayText = fullTitle.Substring(0, charsToShow);
        if (_phaseTimer % 0.1f < 0.05f && charsToShow < fullTitle.Length)
        {
            displayText += "█"; // Cursor parpadeante
        }
        _titleLabel.Text = displayText;
        
        // Fade in del container
        float alpha = Mathf.Min(_phaseTimer / 0.3f, 1);
        _logoContainer.Modulate = new Color(1, 1, 1, alpha);
        
        // Animar línea bajo el título
        var titleLine = _logoContainer.GetNodeOrNull<ColorRect>("TitleLine");
        if (titleLine != null)
        {
            float lineProgress = Mathf.Clamp((_phaseTimer - 0.3f) / 0.4f, 0, 1);
            titleLine.CustomMinimumSize = new Vector2(360 * lineProgress, 3);
            titleLine.Position = new Vector2(-180, 40);
        }
        
        if (_phaseTimer >= PHASE_LOGO_DURATION)
        {
            _titleLabel.Text = fullTitle;
            _subtitleLabel.Text = "CYBER DEFENSE PROTOCOL";
            _phase = BootPhase.Loading;
            _phaseTimer = 0f;
            StartLoadingPhase();
        }
    }

    private void ProcessLoadingPhase(float dt)
    {
        // Calcular progreso del loading
        _loadingProgress = Mathf.Min(_phaseTimer / PHASE_LOADING_DURATION, 1f);
        
        // Actualizar barra
        float barWidth = 396 * _loadingProgress;
        _loadingBarFill.CustomMinimumSize = new Vector2(barWidth, 4);
        
        // Actualizar mensaje según progreso
        int step = (int)(_loadingProgress * (_loadingMessages.Length - 1));
        if (step != _loadingStep)
        {
            _loadingStep = step;
            _loadingTextLabel.Text = _loadingMessages[Mathf.Min(step, _loadingMessages.Length - 1)];
            
            // Pequeño glitch al cambiar mensaje
            TriggerMiniGlitch();
        }
        
        // Color de la barra cambia según progreso
        if (_loadingProgress > 0.8f)
        {
            _loadingBarFill.Color = NEON_GREEN;
            _loadingTextLabel.AddThemeColorOverride("font_color", NEON_GREEN);
        }
        else if (_loadingProgress > 0.5f)
        {
            _loadingBarFill.Color = NEON_CYAN;
        }
        
        if (_phaseTimer >= PHASE_LOADING_DURATION)
        {
            _phase = BootPhase.Ready;
            _phaseTimer = 0f;
            _canSkip = true;
            ShowReadyState();
        }
    }

    private void ProcessReadyPhase(float dt)
    {
        // Pulso del "PRESS ANY KEY"
        float pulse = 0.6f + 0.4f * Mathf.Sin(_timer * 4f);
        _pressKeyLabel.Modulate = new Color(1, 1, 1, pulse);
        
        // Glow pulsante en el título
        float titleGlow = 0.9f + 0.1f * Mathf.Sin(_timer * 2f);
        _titleLabel.Modulate = new Color(titleGlow, titleGlow, titleGlow, 1);
        
        // Color alternante sutil del subtítulo
        float hue = (_timer * 0.1f) % 1f;
        _subtitleLabel.AddThemeColorOverride("font_color", 
            Color.FromHsv(0.5f + hue * 0.1f, 0.8f, 1f, 0.9f));
    }

    private void StartLogoReveal()
    {
        // Fade in de la barra de loading
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_loadingBarBg, "modulate:a", 1.0f, 0.3f);
        tween.TweenProperty(_loadingTextLabel, "modulate:a", 1.0f, 0.3f);
    }

    private void StartLoadingPhase()
    {
        _loadingTextLabel.Text = _loadingMessages[0];
    }

    private void ShowReadyState()
    {
        // Ocultar loading bar, mostrar PRESS ANY KEY
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_loadingBarBg, "modulate:a", 0.0f, 0.3f);
        tween.TweenProperty(_loadingTextLabel, "modulate:a", 0.0f, 0.3f);
        tween.TweenProperty(_pressKeyLabel, "modulate:a", 1.0f, 0.4f);
        
        // Cambiar loading text a COMPLETE
        _loadingTextLabel.Text = _loadingMessages[_loadingMessages.Length - 1];
        _loadingTextLabel.AddThemeColorOverride("font_color", NEON_GREEN);
        
        // Flash verde de confirmación
        tween.TweenCallback(Callable.From(() => {
            var flash = new ColorRect();
            flash.SetAnchorsPreset(LayoutPreset.FullRect);
            flash.Color = new Color(NEON_GREEN, 0.15f);
            flash.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(flash);
            
            var flashTween = CreateTween();
            flashTween.TweenProperty(flash, "color:a", 0f, 0.3f);
            flashTween.TweenCallback(Callable.From(flash.QueueFree));
        }));
    }

    private void TriggerGlitchEffect()
    {
        // Activar varias líneas de glitch
        foreach (var line in _glitchLines)
        {
            float y = (float)_rng.NextDouble() * GetViewportRect().Size.Y;
            line.Position = new Vector2(0, y);
            line.CustomMinimumSize = new Vector2(GetViewportRect().Size.X, 2 + _rng.Next(6));
            
            Color glitchColor = _rng.NextDouble() > 0.5 ? NEON_CYAN : NEON_PINK;
            line.Color = new Color(glitchColor, 0.7f);
            
            var tween = CreateTween();
            tween.TweenProperty(line, "color:a", 0f, 0.1f + (float)_rng.NextDouble() * 0.2f);
        }
    }

    private void TriggerMiniGlitch()
    {
        // Glitch pequeño durante el loading
        var line = _glitchLines[_rng.Next(_glitchLines.Count)];
        float y = (float)_rng.NextDouble() * GetViewportRect().Size.Y;
        line.Position = new Vector2(0, y);
        line.CustomMinimumSize = new Vector2(GetViewportRect().Size.X, 1 + _rng.Next(3));
        line.Color = new Color(NEON_CYAN, 0.5f);
        
        var tween = CreateTween();
        tween.TweenProperty(line, "color:a", 0f, 0.15f);
    }

    private void UpdateRandomGlitch(float dt)
    {
        // Glitch aleatorio ocasional
        if (_rng.NextDouble() < dt * 0.5f) // ~50% chance per second
        {
            TriggerMiniGlitch();
        }
    }

    private void SpawnParticle(Vector2 viewport, bool randomY)
    {
        if (_particles.Count >= MAX_PARTICLES) return;
        
        var particle = new CyberParticle();
        particle.Position = new Vector2(
            (float)_rng.NextDouble() * viewport.X,
            randomY ? (float)_rng.NextDouble() * viewport.Y : viewport.Y + 20
        );
        
        // Variedad de comportamientos
        int type = _rng.Next(3);
        switch (type)
        {
            case 0: // Subiendo lento
                particle.Velocity = new Vector2(((float)_rng.NextDouble() - 0.5f) * 10, -20 - (float)_rng.NextDouble() * 30);
                particle.Size = 1.5f + (float)_rng.NextDouble() * 2f;
                particle.ParticleColor = NEON_CYAN;
                break;
            case 1: // Data bits rápidos
                particle.Velocity = new Vector2(((float)_rng.NextDouble() - 0.5f) * 40, -50 - (float)_rng.NextDouble() * 40);
                particle.Size = 1f + (float)_rng.NextDouble() * 1.5f;
                particle.ParticleColor = NEON_GREEN;
                particle.IsSquare = true;
                break;
            case 2: // Sparkles
                particle.Velocity = new Vector2(((float)_rng.NextDouble() - 0.5f) * 20, -30 - (float)_rng.NextDouble() * 20);
                particle.Size = 2f + (float)_rng.NextDouble() * 3f;
                particle.ParticleColor = NEON_PINK;
                particle.IsSpark = true;
                break;
        }
        
        particle.Alpha = 0.3f + (float)_rng.NextDouble() * 0.5f;
        
        _particleContainer.AddChild(particle);
        _particles.Add(particle);
    }

    private void UpdateParticles(float dt)
    {
        var viewport = GetViewportRect().Size;
        if (viewport.X == 0) viewport = new Vector2(1920, 1080);
        
        // Limpiar partículas muertas
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            if (!IsInstanceValid(p) || p.Position.Y < -50 || p.IsDead)
            {
                if (IsInstanceValid(p)) p.QueueFree();
                _particles.RemoveAt(i);
            }
        }
        
        // Spawn nuevas
        if (_rng.NextDouble() < dt * 2f)
        {
            SpawnParticle(viewport, false);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_isTransitioning || !_canSkip) return;
        
        bool shouldSkip = @event switch
        {
            InputEventKey key => key.Pressed && !key.Echo,
            InputEventMouseButton mouse => mouse.Pressed,
            InputEventJoypadButton joy => joy.Pressed,
            _ => false
        };
        
        if (shouldSkip)
        {
            StartTransition();
            GetViewport().SetInputAsHandled();
        }
    }

    private void StartTransition()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        _phase = BootPhase.Transition;
        
        // ═══════════════════════════════════════════════════════════════════
        // TRANSICIÓN ÉPICA
        // ═══════════════════════════════════════════════════════════════════
        
        // Cambiar texto
        _pressKeyLabel.Text = "[ INITIALIZING... ]";
        _pressKeyLabel.AddThemeColorOverride("font_color", NEON_CYAN);
        _pressKeyLabel.Modulate = Colors.White;
        
        // Glitch intenso
        for (int i = 0; i < 5; i++)
        {
            float delay = i * 0.05f;
            var timer = GetTree().CreateTimer(delay);
            timer.Timeout += TriggerGlitchEffect;
        }
        
        // Flash blanco
        var flash = new ColorRect();
        flash.SetAnchorsPreset(LayoutPreset.FullRect);
        flash.Color = new Color(1, 1, 1, 0);
        flash.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(flash);
        
        // Secuencia de transición
        var tween = CreateTween();
        tween.TweenProperty(flash, "color:a", 0.6f, 0.15f);
        tween.TweenProperty(flash, "color:a", 0f, 0.2f);
        tween.TweenInterval(0.1f);
        tween.TweenProperty(this, "modulate:a", 0f, 0.3f);
        tween.TweenCallback(Callable.From(() => {
            try
            {
                GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
            }
            catch (Exception e)
            {
                GD.PrintErr($"[Boot] Error en transición: {e.Message}");
                // Fallback: intentar de nuevo
                GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
            }
        }));
    }
}

/// <summary>
/// Partícula cyberpunk para el boot screen
/// </summary>
public partial class CyberParticle : Node2D
{
    public Vector2 Velocity;
    public float Size = 2f;
    public float Alpha = 0.5f;
    public Color ParticleColor = Colors.Cyan;
    public bool IsSquare = false;
    public bool IsSpark = false;
    public bool IsDead = false;
    
    private float _life = 0f;
    private float _maxLife;
    private float _wobbleOffset;
    
    public override void _Ready()
    {
        _maxLife = 3f + (float)GD.Randf() * 2f;
        _wobbleOffset = (float)GD.Randf() * Mathf.Pi * 2;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _life += dt;
        
        if (_life >= _maxLife)
        {
            IsDead = true;
            return;
        }
        
        // Movimiento con wobble
        Position += Velocity * dt;
        
        if (!IsSquare)
        {
            Position += new Vector2(Mathf.Sin(_life * 2f + _wobbleOffset) * 0.5f, 0);
        }
        
        // Fade out al final de vida
        float lifeRatio = _life / _maxLife;
        float currentAlpha = Alpha;
        if (lifeRatio > 0.7f)
        {
            currentAlpha *= 1f - (lifeRatio - 0.7f) / 0.3f;
        }
        
        Modulate = new Color(1, 1, 1, currentAlpha);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (IsSpark)
        {
            // Cruz brillante
            float s = Size;
            DrawLine(new Vector2(-s, 0), new Vector2(s, 0), new Color(ParticleColor, 0.8f), 1.5f);
            DrawLine(new Vector2(0, -s), new Vector2(0, s), new Color(ParticleColor, 0.8f), 1.5f);
            DrawCircle(Vector2.Zero, Size * 0.3f, ParticleColor);
        }
        else if (IsSquare)
        {
            // Cuadrado (data bit)
            DrawRect(new Rect2(-Size/2, -Size/2, Size, Size), ParticleColor);
        }
        else
        {
            // Círculo con glow
            DrawCircle(Vector2.Zero, Size * 1.5f, new Color(ParticleColor, Alpha * 0.2f));
            DrawCircle(Vector2.Zero, Size, new Color(ParticleColor, Alpha * 0.5f));
            DrawCircle(Vector2.Zero, Size * 0.5f, ParticleColor);
        }
    }
}
