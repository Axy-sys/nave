using Godot;
using System;
using System.Collections.Generic;
using CyberSecurityGame.Core;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de introducción de misión cinematográfica
    /// Controla la secuencia: Fondo → Nave → HUD → Elliot → Briefing
    /// Diseñado para inmersión y claridad narrativa
    /// </summary>
    public partial class MissionIntroSystem : CanvasLayer
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

        // ═══════════════════════════════════════════════════════════
        // SIGNALS
        // ═══════════════════════════════════════════════════════════
        [Signal] public delegate void IntroCompletedEventHandler();
        [Signal] public delegate void PhaseChangedEventHandler(string phaseName);

        // ═══════════════════════════════════════════════════════════
        // ESTADOS
        // ═══════════════════════════════════════════════════════════
        private enum IntroPhase
        {
            FadeIn,
            ShowBackground,
            ShowPlayer,
            ShowHUD,
            ShowElliot,
            ShowBriefing,
            Ready,
            Complete
        }

        private IntroPhase _currentPhase = IntroPhase.FadeIn;
        private int _currentLevel = 1;
        private int _currentWave = 1;
        private bool _canSkip = false;
        private bool _isActive = false;

        // ═══════════════════════════════════════════════════════════
        // UI ELEMENTS
        // ═══════════════════════════════════════════════════════════
        private ColorRect _fadeOverlay;
        private Panel _elliotPanel;
        private Label _elliotName;
        private Label _elliotText;
        private ColorRect _elliotPortrait;
        private Panel _briefingPanel;
        private Label _briefingTitle;
        private RichTextLabel _briefingContent;
        private Label _briefingHint;
        private ColorRect _scanlines;
        private Label _skipHint;

        // Callbacks
        private Action _onComplete;

        public override void _Ready()
        {
            Layer = 100; // Encima de todo
            ProcessMode = ProcessModeEnum.Always; // Funciona incluso pausado
            CreateUI();
        }

        private void CreateUI()
        {
            // Fade Overlay (cubre toda la pantalla)
            _fadeOverlay = new ColorRect();
            _fadeOverlay.Color = BG_COLOR;
            _fadeOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _fadeOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(_fadeOverlay);

            // Panel de Elliot (arriba)
            CreateElliotPanel();

            // Panel de Briefing (centro)
            CreateBriefingPanel();

            // Scanlines
            _scanlines = new ColorRect();
            _scanlines.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _scanlines.MouseFilter = Control.MouseFilterEnum.Ignore;
            _scanlines.Color = new Color(0, 1, 0.3f, 0.02f);
            _scanlines.Visible = false;
            AddChild(_scanlines);

            // Skip hint
            _skipHint = new Label();
            _skipHint.Text = "[SPACE] Skip intro";
            _skipHint.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
            _skipHint.Position = new Vector2(-180, -40);
            _skipHint.AddThemeColorOverride("font_color", new Color(TERMINAL_DIM, 0.5f));
            _skipHint.AddThemeFontSizeOverride("font_size", 14);
            _skipHint.Visible = false;
            AddChild(_skipHint);

            // Ocultar todo inicialmente
            _elliotPanel.Visible = false;
            _briefingPanel.Visible = false;
        }

        private void CreateElliotPanel()
        {
            _elliotPanel = new Panel();
            _elliotPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _elliotPanel.OffsetLeft = 80;
            _elliotPanel.OffsetTop = 60;
            _elliotPanel.OffsetRight = -80;
            _elliotPanel.OffsetBottom = 200;

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0, 0, 0, 0.95f);
            style.BorderColor = RIPPIER_PURPLE;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            style.ShadowColor = new Color(RIPPIER_PURPLE, 0.3f);
            style.ShadowSize = 15;
            _elliotPanel.AddThemeStyleboxOverride("panel", style);
            AddChild(_elliotPanel);

            // Header con dots
            var header = new HBoxContainer();
            header.Position = new Vector2(15, 10);
            header.AddThemeConstantOverride("separation", 8);
            _elliotPanel.AddChild(header);

            foreach (var c in new[] { "#ff5f56", "#ffbd2e", "#27c93f" })
            {
                var dot = new ColorRect();
                dot.CustomMinimumSize = new Vector2(12, 12);
                dot.Color = new Color(c);
                header.AddChild(dot);
            }

            var headerTitle = new Label();
            headerTitle.Text = "  secure_channel.exe";
            headerTitle.AddThemeColorOverride("font_color", new Color("#666666"));
            headerTitle.AddThemeFontSizeOverride("font_size", 14);
            header.AddChild(headerTitle);

            // Portrait (cuadrado con color)
            _elliotPortrait = new ColorRect();
            _elliotPortrait.Position = new Vector2(25, 45);
            _elliotPortrait.CustomMinimumSize = new Vector2(80, 80);
            _elliotPortrait.Color = new Color("#1a1a1a");
            _elliotPanel.AddChild(_elliotPortrait);

            // ASCII face inside portrait
            var faceLabel = new Label();
            faceLabel.Text = " (•_•)\n  <|>\n  /\\";
            faceLabel.Position = new Vector2(10, 15);
            faceLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            faceLabel.AddThemeFontSizeOverride("font_size", 14);
            _elliotPortrait.AddChild(faceLabel);

            // Nombre
            _elliotName = new Label();
            _elliotName.Text = "ELLIOT";
            _elliotName.Position = new Vector2(130, 45);
            _elliotName.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _elliotName.AddThemeFontSizeOverride("font_size", 22);
            _elliotPanel.AddChild(_elliotName);

            // Texto
            _elliotText = new Label();
            _elliotText.Text = "...";
            _elliotText.Position = new Vector2(130, 80);
            _elliotText.Size = new Vector2(600, 60);
            _elliotText.AutowrapMode = TextServer.AutowrapMode.Word;
            _elliotText.AddThemeColorOverride("font_color", Colors.White);
            _elliotText.AddThemeFontSizeOverride("font_size", 18);
            _elliotPanel.AddChild(_elliotText);
        }

        private void CreateBriefingPanel()
        {
            _briefingPanel = new Panel();
            _briefingPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _briefingPanel.CustomMinimumSize = new Vector2(650, 400);
            _briefingPanel.GrowHorizontal = Control.GrowDirection.Both;
            _briefingPanel.GrowVertical = Control.GrowDirection.Both;

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0, 0, 0, 0.98f);
            style.BorderColor = FLUX_ORANGE;
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            style.ShadowColor = new Color(FLUX_ORANGE, 0.3f);
            style.ShadowSize = 20;
            _briefingPanel.AddThemeStyleboxOverride("panel", style);
            AddChild(_briefingPanel);

            // Header con dots
            var header = new Panel();
            header.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            header.OffsetBottom = 40;
            var headerStyle = new StyleBoxFlat();
            headerStyle.BgColor = new Color("#0d0d0d");
            header.AddThemeStyleboxOverride("panel", headerStyle);
            _briefingPanel.AddChild(header);

            var dots = new HBoxContainer();
            dots.Position = new Vector2(15, 12);
            dots.AddThemeConstantOverride("separation", 8);
            header.AddChild(dots);

            foreach (var c in new[] { "#ff5f56", "#ffbd2e", "#27c93f" })
            {
                var dot = new ColorRect();
                dot.CustomMinimumSize = new Vector2(12, 12);
                dot.Color = new Color(c);
                dots.AddChild(dot);
            }

            var headerTitleLbl = new Label();
            headerTitleLbl.Text = "mission_briefing.dat";
            headerTitleLbl.Position = new Vector2(80, 10);
            headerTitleLbl.AddThemeColorOverride("font_color", new Color("#888888"));
            headerTitleLbl.AddThemeFontSizeOverride("font_size", 14);
            header.AddChild(headerTitleLbl);

            // Content container
            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetTop = 50;
            content.OffsetLeft = 30;
            content.OffsetRight = -30;
            content.OffsetBottom = -25;
            content.AddThemeConstantOverride("separation", 15);
            _briefingPanel.AddChild(content);

            // Título
            _briefingTitle = new Label();
            _briefingTitle.HorizontalAlignment = HorizontalAlignment.Center;
            _briefingTitle.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _briefingTitle.AddThemeFontSizeOverride("font_size", 24);
            content.AddChild(_briefingTitle);

            // Separador
            var sep = new HSeparator();
            sep.AddThemeColorOverride("separator", TERMINAL_DIM);
            content.AddChild(sep);

            // Contenido
            _briefingContent = new RichTextLabel();
            _briefingContent.BbcodeEnabled = true;
            _briefingContent.FitContent = true;
            _briefingContent.CustomMinimumSize = new Vector2(0, 180);
            _briefingContent.AddThemeFontSizeOverride("normal_font_size", 16);
            _briefingContent.AddThemeColorOverride("default_color", Colors.White);
            content.AddChild(_briefingContent);

            // Hint para continuar
            _briefingHint = new Label();
            _briefingHint.Text = ">>> Press [ENTER] to begin mission <<<";
            _briefingHint.HorizontalAlignment = HorizontalAlignment.Center;
            _briefingHint.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _briefingHint.AddThemeFontSizeOverride("font_size", 16);
            content.AddChild(_briefingHint);
        }

        // ═══════════════════════════════════════════════════════════
        // PUBLIC API
        // ═══════════════════════════════════════════════════════════
        public void StartIntro(int level, int wave, Action onComplete = null)
        {
            _currentLevel = level;
            _currentWave = wave;
            _onComplete = onComplete;
            _isActive = true;
            _currentPhase = IntroPhase.FadeIn;

            // Pausar el juego durante la intro
            GetTree().Paused = true;

            // Reset UI
            _fadeOverlay.Color = BG_COLOR;
            _fadeOverlay.Visible = true;
            _elliotPanel.Visible = false;
            _briefingPanel.Visible = false;
            _scanlines.Visible = true;
            _skipHint.Visible = true;
            _canSkip = true;

            // Iniciar secuencia
            RunIntroSequence();
        }

        public void StartWaveBriefing(int level, int wave, Action onComplete = null)
        {
            // Versión simplificada para oleadas posteriores (sin fade completo)
            _currentLevel = level;
            _currentWave = wave;
            _onComplete = onComplete;
            _isActive = true;

            GetTree().Paused = true;
            
            _fadeOverlay.Color = new Color(BG_COLOR, 0.7f);
            _fadeOverlay.Visible = true;
            _scanlines.Visible = true;

            // Solo mostrar briefing
            ShowBriefingOnly();
        }

        // ═══════════════════════════════════════════════════════════
        // INTRO SEQUENCE
        // ═══════════════════════════════════════════════════════════
        private async void RunIntroSequence()
        {
            try
            {
                // Fase 1: Fade desde negro (fondo aparece)
                _currentPhase = IntroPhase.ShowBackground;
                EmitSignal(SignalName.PhaseChanged, "background");
                
                var tween1 = CreateTween();
                tween1.TweenProperty(_fadeOverlay, "color:a", 0.0f, 1.0f);
                await ToSignal(tween1, "finished");

                if (!_isActive) return; // Check if skipped

                await ToSignal(GetTree().CreateTimer(0.3f, true, false, true), "timeout");

                // Fase 2: Elliot aparece
                _currentPhase = IntroPhase.ShowElliot;
                EmitSignal(SignalName.PhaseChanged, "elliot");
                await ShowElliotDialogue();

                if (!_isActive) return;

                // Fase 3: Briefing
                _currentPhase = IntroPhase.ShowBriefing;
                EmitSignal(SignalName.PhaseChanged, "briefing");
                ShowBriefing();
            }
            catch (Exception e)
            {
                GD.PrintErr($"[MissionIntro] Error en secuencia: {e.Message}");
                CompleteIntro();
            }
        }

        private async System.Threading.Tasks.Task ShowElliotDialogue()
        {
            _elliotPanel.Visible = true;
            _elliotPanel.Modulate = new Color(1, 1, 1, 0);

            var fadeIn = CreateTween();
            fadeIn.TweenProperty(_elliotPanel, "modulate:a", 1.0f, 0.3f);
            await ToSignal(fadeIn, "finished");

            // Diálogo según nivel
            var (speaker, lines) = GetElliotDialogue(_currentLevel, _currentWave);
            _elliotName.Text = speaker;

            foreach (var line in lines)
            {
                if (!_isActive) return;
                
                _elliotText.Text = line;
                
                // Typing effect simulado con fade
                _elliotText.Modulate = new Color(1, 1, 1, 0);
                var typeTween = CreateTween();
                typeTween.TweenProperty(_elliotText, "modulate:a", 1.0f, 0.2f);
                
                await ToSignal(GetTree().CreateTimer(2.5f, true, false, true), "timeout");
            }

            // Fade out Elliot
            var fadeOut = CreateTween();
            fadeOut.TweenProperty(_elliotPanel, "modulate:a", 0.0f, 0.3f);
            await ToSignal(fadeOut, "finished");
            _elliotPanel.Visible = false;
        }

        private void ShowBriefing()
        {
            var (title, content) = GetMissionBriefing(_currentLevel, _currentWave);
            
            _briefingTitle.Text = title;
            _briefingContent.Text = content;

            _briefingPanel.Visible = true;
            _briefingPanel.Modulate = new Color(1, 1, 1, 0);
            _briefingPanel.Scale = new Vector2(0.9f, 0.9f);

            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_briefingPanel, "modulate:a", 1.0f, 0.3f);
            tween.TweenProperty(_briefingPanel, "scale", Vector2.One, 0.3f);

            // Blink del hint
            StartHintBlink();
        }

        private void ShowBriefingOnly()
        {
            var (title, content) = GetMissionBriefing(_currentLevel, _currentWave);
            
            _briefingTitle.Text = title;
            _briefingContent.Text = content;

            _briefingPanel.Visible = true;
            _briefingPanel.Modulate = Colors.White;
            _briefingPanel.Scale = Vector2.One;

            StartHintBlink();
        }

        private void StartHintBlink()
        {
            var blinkTween = CreateTween();
            blinkTween.SetLoops();
            blinkTween.TweenProperty(_briefingHint, "modulate:a", 0.3f, 0.5f);
            blinkTween.TweenProperty(_briefingHint, "modulate:a", 1.0f, 0.5f);
        }

        // ═══════════════════════════════════════════════════════════
        // CONTENT DATA
        // ═══════════════════════════════════════════════════════════
        private (string speaker, List<string> lines) GetElliotDialogue(int level, int wave)
        {
            if (level == 1 && wave == 1)
            {
                return ("ELLIOT", new List<string> {
                    "Hey... are you there?",
                    "The system thinks it's secure. It's not.",
                    "They're watching. Let's give them something to see."
                });
            }
            else if (level == 2 && wave == 1)
            {
                return ("MR. ROBOT", new List<string> {
                    "Welcome to the Deep Web, kid.",
                    "No rules here. Only survival.",
                    "Don't trust anything. Not even me."
                });
            }
            else if (level == 3 && wave == 1)
            {
                return ("ELLIOT", new List<string> {
                    "This is it. E-Corp's core.",
                    "All their secrets. All their lies.",
                    "Time to execute Phase 2."
                });
            }
            else
            {
                return ("SYSTEM", new List<string> {
                    $"Initiating wave {wave}...",
                    "Threat level increasing.",
                    "Stay focused, operator."
                });
            }
        }

        private (string title, string content) GetMissionBriefing(int level, int wave)
        {
            // DISEÑO DE OLEADAS COMPLETO
            // Nivel 1: Surface Web (3 oleadas)
            // Nivel 2: Deep Web (3 oleadas)
            // Nivel 3: Dark Web / E-Corp Core (3 oleadas)

            if (level == 1)
            {
                return wave switch
                {
                    1 => ("MISSION 1.1: HELLO FRIEND", 
                        $"[color=#888888]LOCATION:[/color] Surface Web - Entry Node\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nYou've entered the network. Corporate tracking scripts are everywhere. " +
                        $"They think they're watching us, but we're watching them.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#00ff41]MALWARE[/color] - Generic malicious code\n• Tracking cookies\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Neutralize all threats to secure the entry point.\n\n" +
                        $"[color=#888888]TIP: Use LEFT CLICK to fire. WASD to move.[/color]"),

                    2 => ("MISSION 1.2: SOCIAL ENGINEERING",
                        $"[color=#888888]LOCATION:[/color] E-Corp Mail Server\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nEmployees are the weak link. Their inboxes are flooded with fake emails. " +
                        $"Phishing attacks are attempting to harvest credentials.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#00ff41]MALWARE[/color] - Still lurking\n• [color=#bf00ff]PHISHING[/color] - Deceptive messages\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Block the phishing campaign before data leaks."),

                    3 => ("MISSION 1.3: THE ALL-SEEING EYE",
                        $"[color=#888888]LOCATION:[/color] Surveillance Network\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nThey know we're here. Corporate has activated full surveillance protocols. " +
                        $"A botnet is preparing a DDoS attack to flush us out.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#00ff41]MALWARE[/color]\n• [color=#bf00ff]PHISHING[/color]\n• [color=#ffaa00]DDoS BOTNET[/color] - Distributed attack\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Survive the assault. Prove they can't stop us."),

                    _ => ("ALERT", "Unknown mission parameters.")
                };
            }
            else if (level == 2)
            {
                return wave switch
                {
                    1 => ("MISSION 2.1: ONION LAYERS",
                        $"[color=#888888]LOCATION:[/color] Deep Web - Tor Network\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nWe've gone deeper. The onion routing provides anonymity, " +
                        $"but malicious exit nodes are trying to de-anonymize our traffic.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#ffaa00]DDoS ATTACKS[/color] - Network flooding\n• Rogue Tor nodes\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Maintain anonymity. Eliminate hostile nodes."),

                    2 => ("MISSION 2.2: BLACK MARKET",
                        $"[color=#888888]LOCATION:[/color] Silk Road 3.0\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nThe digital black market. Everything has a price here. " +
                        $"The transaction ledgers are being attacked with SQL injections.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#ffaa00]DDoS[/color]\n• [color=#ff0000]SQL INJECTION[/color] - Database attacks\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Protect the ledgers. Don't let them corrupt the data."),

                    3 => ("MISSION 2.3: FBI HONEYPOT",
                        $"[color=#888888]LOCATION:[/color] Federal Trap Server\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nIt's a trap. This node is an FBI honeypot. " +
                        $"They're launching brute force attacks to crack our encryption.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#ffaa00]DDoS[/color]\n• [color=#ff0000]SQL INJECTION[/color]\n• [color=#ff5555]BRUTE FORCE[/color] - Password cracking\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Escape the honeypot. Don't get caught."),

                    _ => ("ALERT", "Unknown mission parameters.")
                };
            }
            else // Level 3
            {
                return wave switch
                {
                    1 => ("MISSION 3.1: DIGITAL HOSTAGE",
                        $"[color=#888888]LOCATION:[/color] Dark Web - Dark Army Server\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nThe Dark Army has deployed military-grade ransomware. " +
                        $"They're encrypting everything. Backups are being targeted.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#ff0000]RANSOMWARE[/color] - File encryption malware\n• Crypto demands\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Stop the ransomware. Protect the backups."),

                    2 => ("MISSION 3.2: DARK ARMY",
                        $"[color=#888888]LOCATION:[/color] Whiterose's Network\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nWhiterose has sent her best operatives. " +
                        $"State-sponsored trojans and spyware are infiltrating every system.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#ff0000]RANSOMWARE[/color]\n• [color=#bf00ff]TROJANS[/color] - Disguised malware\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Eliminate the Dark Army's digital soldiers."),

                    3 => ("MISSION 3.3: PHASE 2",
                        $"[color=#888888]LOCATION:[/color] E-Corp Central Core\n\n" +
                        $"[color=#ffaa00]SITUATION:[/color]\nThis is it. The core is exposed. " +
                        $"Zero-day exploits and self-replicating worms guard the final barrier.\n\n" +
                        $"[color=#ff5555]THREATS:[/color]\n• [color=#ff0000]RANSOMWARE[/color]\n• [color=#bf00ff]TROJANS[/color]\n• [color=#ffaa00]WORMS[/color] - Self-replicating\n\n" +
                        $"[color=#00ff41]OBJECTIVE:[/color] Execute the final hack. END E-CORP.\n\n" +
                        $"[color=#00ff41]\"We are fsociety. We are finally free.\"[/color]"),

                    _ => ("ALERT", "Unknown mission parameters.")
                };
            }
        }

        // ═══════════════════════════════════════════════════════════
        // INPUT & COMPLETION
        // ═══════════════════════════════════════════════════════════
        public override void _Input(InputEvent @event)
        {
            if (!_isActive) return;

            // Skip con SPACE (solo durante fases skippeables)
            if (_canSkip && @event is InputEventKey key && key.Pressed && key.Keycode == Key.Space)
            {
                SkipToEnd();
                return;
            }

            // ENTER para confirmar briefing
            if (_currentPhase == IntroPhase.ShowBriefing && @event is InputEventKey enterKey && 
                enterKey.Pressed && enterKey.Keycode == Key.Enter)
            {
                CompleteIntro();
            }

            // Click también acepta
            if (_currentPhase == IntroPhase.ShowBriefing && @event is InputEventMouseButton mb && 
                mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            {
                CompleteIntro();
            }
        }

        private void SkipToEnd()
        {
            _canSkip = false;
            _currentPhase = IntroPhase.ShowBriefing;
            
            // Ocultar Elliot si está visible
            _elliotPanel.Visible = false;
            
            // Mostrar briefing directamente
            _fadeOverlay.Color = new Color(BG_COLOR, 0.0f);
            ShowBriefing();
        }

        private void CompleteIntro()
        {
            _isActive = false;
            _currentPhase = IntroPhase.Complete;

            // Fade out de todo
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_fadeOverlay, "color:a", 0.0f, 0.3f);
            tween.TweenProperty(_briefingPanel, "modulate:a", 0.0f, 0.3f);
            tween.TweenProperty(_scanlines, "modulate:a", 0.0f, 0.3f);

            tween.Chain().TweenCallback(Callable.From(() => {
                // Despausar el juego
                GetTree().Paused = false;

                // Ocultar todo
                Visible = false;

                // Emitir señal
                EmitSignal(SignalName.IntroCompleted);
                
                // Callback
                _onComplete?.Invoke();
            }));
        }

        public override void _Process(double delta)
        {
            if (!_isActive) return;

            // Animate scanlines
            float time = (float)Time.GetTicksMsec() / 1000f;
            float alpha = 0.015f + 0.01f * Mathf.Sin(time * 4);
            _scanlines.Color = new Color(0, 1, 0.3f, alpha);
        }
    }
}
