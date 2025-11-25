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
    /// </summary>
    public partial class MainScene : Node2D
    {
        private GameManager _gameManager;
        private WaveSystem _waveSystem;
        private PowerUpSystem _powerUpSystem;
        private VulnerabilitySystem _vulnerabilitySystem;
        private QuizSystem _quizSystem;
        private SecurityTipsSystem _tipsSystem;
        private MissionIntroSystem _missionIntro;
        private GameJuiceSystem _juiceSystem;
        private GameHUD _hud;
        private QuizView _quizView;
        private ScreenEffects _screenEffects;
        private Entities.Player _player;
        
        private bool _introCompleted = false;
        private int _currentLevel = 1;

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

            // Wave System (no inicia automáticamente ahora)
            _waveSystem = new WaveSystem();
            _waveSystem.Name = "WaveSystem";
            _waveSystem.ProcessMode = ProcessModeEnum.Pausable;
            AddChild(_waveSystem);

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

            // Dialogue System
            var dialogueSystem = new DialogueSystem();
            dialogueSystem.Name = "DialogueSystem";
            AddChild(dialogueSystem);

            // Game Juice System (feedback visual satisfactorio)
            _juiceSystem = new GameJuiceSystem();
            _juiceSystem.Name = "GameJuiceSystem";
            AddChild(_juiceSystem);

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

            // NOTA: DialogueView está DESACTIVADO
            // Los diálogos ahora se manejan exclusivamente por MissionIntroSystem
            // para evitar superposición de UI y slow-motion no deseado

            GD.Print("✓ UI y efectos cargados");
        }

        private void StartMissionIntro()
        {
            // ═══════════════════════════════════════════════════════════
            // FASE 4: INTRO CINEMATOGRÁFICA
            // ═══════════════════════════════════════════════════════════
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
            
            // Ahora sí iniciamos el juego
            _gameManager.StartGame();
            
            GD.Print(">>> MISIÓN INICIADA <<<");
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
            // No procesar input durante la intro
            if (!_introCompleted) return;

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

            // Debug: Mostrar pregunta de prueba
            if (@event.IsActionPressed("ui_accept") && Input.IsKeyPressed(Key.Shift))
            {
                var question = _quizSystem.GetNextQuestion();
                _quizView.ShowQuestion(question);
            }
        }
    }
}
