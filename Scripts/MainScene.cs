using Godot;
using CyberSecurityGame.Core;
using CyberSecurityGame.Views;
using CyberSecurityGame.Systems;
using CyberSecurityGame.Education;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Core.Interfaces;
using CyberSecurityGame.UI;

namespace CyberSecurityGame
{
    /// <summary>
    /// Escena principal del juego
    /// Coordina todos los sistemas y la inicialización
    /// Incluye secuencia de intro cinematográfica
    /// MODO: Bullet Hell con oleadas infinitas
    /// </summary>
    public partial class MainScene : Node2D
    {
        private GameManager _gameManager;
        private WaveSystem _waveSystem;
        private InfiniteWaveSystem _infiniteWaveSystem;
        private BulletHellSystem _bulletHellSystem;
        private GrazingSystem _grazingSystem;
        private PowerUpSystem _powerUpSystem;
        private VulnerabilitySystem _vulnerabilitySystem;
        private QuizSystem _quizSystem;
        private SecurityTipsSystem _tipsSystem;
        private MissionIntroSystem _missionIntro;
        private GameJuiceSystem _juiceSystem;
        private GameHUD _hud;
        private QuizView _quizView;
        private EncyclopediaView _encyclopediaView;
        private ScreenEffects _screenEffects;
        private Entities.Player _player;
        
        private bool _introCompleted = false;
        private int _currentLevel = 1;
        
        // Modo de juego: true = Bullet Hell infinito, false = Modo campaña
        private bool _infiniteMode = true;
        
        // Para detectar reinicios y saltear intro
        private static bool _hasPlayedIntro = false;

        public override void _Ready()
        {
            GD.Print("=== CyberSecurity Defender - Iniciando ===");
            
            // Fase 1: Setup básico (sistemas internos)
            InitializeCoreSystems();
            
            // Fase 2: Setup visual (fondo y escena)
            SetupScene();
            
            // Fase 3: Mostrar intro cinematográfica
            StartMissionIntro();
        }

        private void InitializeCoreSystems()
        {
            // High Score System (singleton global)
            if (HighScoreSystem.Instance == null)
            {
                var highScoreSystem = new HighScoreSystem();
                highScoreSystem.Name = "HighScoreSystem";
                GetTree().Root.AddChild(highScoreSystem);
            }



            // Game Manager
            _gameManager = new GameManager();
            _gameManager.Name = "GameManager";
            AddChild(_gameManager);

            // Power-Up System
            _powerUpSystem = new PowerUpSystem();
            _powerUpSystem.Name = "PowerUpSystem";
            _powerUpSystem.ProcessMode = ProcessModeEnum.Pausable;
            AddChild(_powerUpSystem);

            // Vulnerability System
            _vulnerabilitySystem = new VulnerabilitySystem();
            _vulnerabilitySystem.Name = "VulnerabilitySystem";
            AddChild(_vulnerabilitySystem);

            // Quiz System
            _quizSystem = new QuizSystem();
            _quizSystem.Name = "QuizSystem";
            AddChild(_quizSystem);

            // Tips System
            _tipsSystem = new SecurityTipsSystem();
            _tipsSystem.Name = "SecurityTipsSystem";
            AddChild(_tipsSystem);
            
            // Threat Encyclopedia (base de conocimiento educativa)
            var threatEncyclopedia = new ThreatEncyclopedia();
            threatEncyclopedia.Name = "ThreatEncyclopedia";
            AddChild(threatEncyclopedia);
            
            // Contextual Learning System (quizzes en momentos óptimos)
            var contextualLearning = new ContextualLearningSystem();
            contextualLearning.Name = "ContextualLearningSystem";
            AddChild(contextualLearning);

            // Dialogue System
            var dialogueSystem = new DialogueSystem();
            dialogueSystem.Name = "DialogueSystem";
            AddChild(dialogueSystem);

            // Game Juice System (feedback visual satisfactorio)
            _juiceSystem = new GameJuiceSystem();
            _juiceSystem.Name = "GameJuiceSystem";
            AddChild(_juiceSystem);

            // ═══════════════════════════════════════════════════════════
            // BULLET HELL SYSTEMS
            // ═══════════════════════════════════════════════════════════
            
            // Bullet Hell System (patrones de balas)
            _bulletHellSystem = new BulletHellSystem();
            _bulletHellSystem.Name = "BulletHellSystem";
            AddChild(_bulletHellSystem);

            // Grazing System (rozar balas = puntos)
            _grazingSystem = new GrazingSystem();
            _grazingSystem.Name = "GrazingSystem";
            AddChild(_grazingSystem);
            
            // Adaptive Difficulty System (dificultad adaptativa estilo Hades/Touhou)
            var adaptiveSystem = new AdaptiveDifficultySystem();
            adaptiveSystem.Name = "AdaptiveDifficultySystem";
            AddChild(adaptiveSystem);

            if (_infiniteMode)
            {
                // Infinite Wave System (modo bullet hell)
                _infiniteWaveSystem = new InfiniteWaveSystem();
                _infiniteWaveSystem.Name = "InfiniteWaveSystem";
                _infiniteWaveSystem.ProcessMode = ProcessModeEnum.Pausable;
                AddChild(_infiniteWaveSystem);
                GD.Print(">>> MODO BULLET HELL INFINITO ACTIVADO <<<");
            }
            else
            {
                // Wave System tradicional (modo campaña)
                _waveSystem = new WaveSystem();
                _waveSystem.Name = "WaveSystem";
                _waveSystem.ProcessMode = ProcessModeEnum.Pausable;
                AddChild(_waveSystem);
            }

            // Conectar eventos
            GameEventBus.Instance.OnVulnerabilityDetected += OnVulnerabilityDetected;
            GameEventBus.Instance.OnQuestionAnswered += OnQuestionAnswered;
            GameEventBus.Instance.OnNewEnemyEncountered += OnNewEnemyEncountered;
            GameEventBus.Instance.OnWaveAnnounced += OnWaveAnnounced;

            GD.Print("✓ Sistemas core inicializados");
        }

        private void SetupScene()
        {
            // ═══════════════════════════════════════════════════════════
            // FASE 1: FONDO (primera cosa que aparece)
            // ═══════════════════════════════════════════════════════════
            var bgScene = GD.Load<PackedScene>("res://Scenes/AnimatedBackground.tscn");
            if (bgScene != null)
            {
                GetNodeOrNull("Background")?.QueueFree();
                
                var parallax = new Parallax2D();
                parallax.Name = "ParallaxSystem";
                parallax.ScrollScale = new Vector2(0.5f, 0.5f);
                parallax.RepeatSize = new Vector2(2400, 1600);
                parallax.Autoscroll = new Vector2(0, 50);
                
                var background = bgScene.Instantiate() as Node2D;
                if (background != null)
                {
                    background.Name = "AnimatedBackground";
                    background.Scale = new Vector2(2, 2);
                    parallax.AddChild(background);
                }
                
                AddChild(parallax);
                MoveChild(parallax, 0);
                GD.Print("✓ Fondo cargado");
            }

            // ═══════════════════════════════════════════════════════════
            // FASE 2: JUGADOR - Buscar existente O crear nuevo
            // (Main.tscn ya puede tener Player instanciado)
            // ═══════════════════════════════════════════════════════════
            _player = GetNodeOrNull<Entities.Player>("Player");
            
            if (_player == null)
            {
                // Solo crear si no existe en la escena
                var playerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
                if (playerScene != null)
                {
                    _player = playerScene.Instantiate<Entities.Player>();
                    _player.Name = "Player";
                    AddChild(_player);
                    GD.Print("✓ Jugador creado (nuevo)");
                }
                else
                {
                    GD.PrintErr("✗ No se pudo cargar la escena del jugador");
                }
            }
            else
            {
                GD.Print("✓ Jugador encontrado (existente en escena)");
            }
            
            // Configurar ProcessMode en cualquier caso
            if (_player != null)
            {
                _player.ProcessMode = ProcessModeEnum.Pausable;
            }

            // ═══════════════════════════════════════════════════════════
            // FASE 3: EFECTOS Y HUD
            // ═══════════════════════════════════════════════════════════
            _screenEffects = new ScreenEffects();
            _screenEffects.Name = "ScreenEffects";
            AddChild(_screenEffects);

            _hud = new GameHUD();
            _hud.Name = "GameHUD";
            AddChild(_hud);

            _quizView = new QuizView();
            _quizView.Name = "QuizView";
            AddChild(_quizView);

            // Encyclopedia View (Pokédex de amenazas - acceso con E)
            _encyclopediaView = new EncyclopediaView();
            _encyclopediaView.Name = "EncyclopediaView";
            AddChild(_encyclopediaView);

            // Offscreen Indicator System (flechas que muestran enemigos fuera de pantalla)
            var offscreenIndicators = new OffscreenIndicatorSystem();
            offscreenIndicators.Name = "OffscreenIndicators";
            AddChild(offscreenIndicators);

            // Non-Intrusive Notification System (avisos que no pausan el juego)
            var notificationSystem = new NonIntrusiveNotificationSystem();
            notificationSystem.Name = "NotificationSystem";
            AddChild(notificationSystem);

            // NOTA: DialogueView está DESACTIVADO
            // Los diálogos ahora se manejan exclusivamente por MissionIntroSystem
            // para evitar superposición de UI y slow-motion no deseado

            GD.Print("✓ UI y efectos cargados");
        }

        private void StartMissionIntro()
        {
            // ═══════════════════════════════════════════════════════════
            // FASE 4: INTRO CINEMATOGRÁFICA
            // 
            // UX: Si el usuario ya vio la intro (reinició), saltearla
            // para no frustrarlo con repetición innecesaria
            // ═══════════════════════════════════════════════════════════
            
            if (_hasPlayedIntro)
            {
                // Saltar intro en reinicios
                GD.Print(">>> Intro salteada (reinicio detectado) <<<");
                OnIntroCompleted();
                return;
            }
            
            _missionIntro = new MissionIntroSystem();
            _missionIntro.Name = "MissionIntro";
            AddChild(_missionIntro);

            _missionIntro.IntroCompleted += OnIntroCompleted;

            // Iniciar la intro del nivel 1, oleada 1
            _missionIntro.StartIntro(_currentLevel, 1, () => {
                GD.Print("✓ Intro completada - Iniciando juego");
            });
        }

        private void OnIntroCompleted()
        {
            _introCompleted = true;
            _hasPlayedIntro = true; // Marcar para futuros reinicios
            
            // ═══════════════════════════════════════════════════════════════════
            // INICIO DEL JUEGO
            // 
            // UX CRÍTICO: Usamos CallDeferred para garantizar que:
            // 1. Todos los nodos hijos (_infiniteWaveSystem, etc.) estén listos
            // 2. Los event listeners estén conectados
            // 3. El árbol de escena esté completamente inicializado
            // 
            // Sin esto, tras reiniciar misión las oleadas no aparecen
            // porque el evento GameStateChanged se emite antes de que
            // InfiniteWaveSystem esté escuchando
            // ═══════════════════════════════════════════════════════════════════
            CallDeferred(nameof(StartGameDeferred));
            
            GD.Print(">>> MISIÓN INICIADA <<<");
        }
        
        private void StartGameDeferred()
        {
            // Verificar que el sistema de oleadas está listo
            if (_infiniteMode && _infiniteWaveSystem != null)
            {
                GD.Print("[Main] InfiniteWaveSystem verificado - Iniciando juego");
            }
            
            _gameManager.StartGame();
            
            // Forzar inicio si el sistema no recibió el evento
            // (seguridad extra para evitar el bug de oleadas no apareciendo)
            if (_infiniteMode && _infiniteWaveSystem != null && !_infiniteWaveSystem.IsWaveActive())
            {
                GD.Print("[Main] Forzando inicio de modo infinito (failsafe)");
                _infiniteWaveSystem.StartInfiniteMode();
            }
        }

        private void SkipIntro()
        {
            if (_introCompleted) return;
            
            GD.Print(">>> Intro skippeada por el usuario <<<");
            
            // Limpiar la intro si existe
            if (_missionIntro != null && IsInstanceValid(_missionIntro))
            {
                _missionIntro.IntroCompleted -= OnIntroCompleted;
                _missionIntro.QueueFree();
                _missionIntro = null;
            }
            
            // Completar la intro manualmente
            OnIntroCompleted();
        }

        private void OnWaveAnnounced(int wave, string title, string desc)
        {
            // Si es oleada > 1, mostrar briefing rápido
            if (wave > 1 && _missionIntro != null)
            {
                // Para oleadas posteriores, solo mostramos el panel de info
                _quizView.ShowInfo($"WAVE {wave}: {title}", desc, "PREPARE FOR DEFENSE");
            }
        }

        private void OnNewEnemyEncountered(string name, string desc, string weakness)
        {
            _quizView.ShowInfo(name, desc, weakness);
        }

        private void OnVulnerabilityDetected(string vulnerability)
        {
            if (vulnerability.Contains("Assessment"))
            {
                int level = 1;
                if (int.TryParse(vulnerability.Split(' ')[1], out int parsedLevel))
                {
                    level = parsedLevel;
                }

                var question = _quizSystem.GetQuestionForLevel(level);
                _quizView.ShowQuestion(question);
            }
            else if (vulnerability == "Firewall Breach")
            {
                var question = _quizSystem.GetNextQuestion();
                _quizView.ShowQuestion(question);
            }
        }

        private void OnQuestionAnswered(bool correct)
        {
            if (correct && _player != null)
            {
                _player.Heal(25f);
                GameEventBus.Instance.EmitSecurityTipShown("Breach patched! Integrity restored.");
            }
            else
            {
                GameEventBus.Instance.EmitSecurityTipShown("Critical failure! Breach persists.");
                if (_player != null) _player.TakeDamage(10, DamageType.Physical);
            }

            if (_waveSystem.GetCurrentWave() >= 3 && !_waveSystem.IsWaveActive())
            {
                _waveSystem.CompleteLevel();
            }
        }

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnVulnerabilityDetected -= OnVulnerabilityDetected;
            GameEventBus.Instance.OnQuestionAnswered -= OnQuestionAnswered;
            GameEventBus.Instance.OnNewEnemyEncountered -= OnNewEnemyEncountered;
            GameEventBus.Instance.OnWaveAnnounced -= OnWaveAnnounced;
            
            if (_missionIntro != null)
            {
                _missionIntro.IntroCompleted -= OnIntroCompleted;
            }
        }

        public override void _Input(InputEvent @event)
        {
            // ═══════════════════════════════════════════════════════════
            // MANEJO DE INPUT
            // 
            // UX: ESC siempre debe funcionar:
            // - Durante intro: Skipea la intro
            // - Durante juego: Pausa
            // - Durante pausa: Resume (manejado por GameHUD)
            // ═══════════════════════════════════════════════════════════
            
            // Permitir skipear la intro con ESC o SPACE
            if (!_introCompleted)
            {
                if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("ui_accept"))
                {
                    SkipIntro();
                    GetViewport().SetInputAsHandled();
                }
                return;
            }

            // Manejo de Game Over
            if (_gameManager.CurrentState == GameState.GameOver)
            {
                if (@event.IsActionPressed("ui_cancel"))
                {
                    GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
                }
                else if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.R)
                {
                    GetTree().ReloadCurrentScene();
                }
                return;
            }

            // Pausa
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_gameManager.CurrentState == GameState.Playing)
                {
                    _gameManager.PauseGame();
                    GD.Print("⏸ Game Paused");
                }
                else if (_gameManager.CurrentState == GameState.Paused)
                {
                    _gameManager.ResumeGame();
                    GD.Print("▶ Game Resumed");
                }
            }

            // Enciclopedia de Amenazas (E para abrir)
            if (@event is InputEventKey encKey && encKey.Pressed && encKey.Keycode == Key.E)
            {
                if (_gameManager.CurrentState == GameState.Playing || 
                    _gameManager.CurrentState == GameState.Paused)
                {
                    _encyclopediaView.Toggle();
                    GD.Print("📚 Encyclopedia toggled");
                }
            }

            // Debug: Mostrar pregunta de prueba
            if (@event.IsActionPressed("ui_accept") && Input.IsKeyPressed(Key.Shift))
            {
                var question = _quizSystem.GetNextQuestion();
                _quizView.ShowQuestion(question);
            }
        }
    }
}
