using Godot;
using CyberSecurityGame.Core;
using CyberSecurityGame.Views;
using CyberSecurityGame.Systems;
using CyberSecurityGame.Education;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame
{
    /// <summary>
    /// Escena principal del juego
    /// Coordina todos los sistemas y la inicialización
    /// </summary>
    public partial class MainScene : Node2D
    {
        private GameManager _gameManager;
        private WaveSystem _waveSystem;
        private PowerUpSystem _powerUpSystem;
        private VulnerabilitySystem _vulnerabilitySystem;
        private QuizSystem _quizSystem;
        private SecurityTipsSystem _tipsSystem;
        private GameHUD _hud;
        private QuizView _quizView;
        private Entities.Player _player;

        public override void _Ready()
        {
            GD.Print("=== CyberSecurity Defender - Iniciando ===");
            InitializeSystems();
            SetupScene();
        }

        private void InitializeSystems()
        {
            // Game Manager
            _gameManager = new GameManager();
            _gameManager.Name = "GameManager";
            AddChild(_gameManager);

            // Wave System
            _waveSystem = new WaveSystem();
            _waveSystem.Name = "WaveSystem";
            AddChild(_waveSystem);

            // Power-Up System
            _powerUpSystem = new PowerUpSystem();
            _powerUpSystem.Name = "PowerUpSystem";
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

            // UI
            _hud = new GameHUD();
            _hud.Name = "GameHUD";
            AddChild(_hud);

            _quizView = new QuizView();
            _quizView.Name = "QuizView";
            AddChild(_quizView);

            // Dialogue View
            var dialogueView = new DialogueView();
            dialogueView.Name = "DialogueView";
            AddChild(dialogueView);

            // Conectar eventos para la mecánica de Quiz
            GameEventBus.Instance.OnVulnerabilityDetected += OnVulnerabilityDetected;
            GameEventBus.Instance.OnQuestionAnswered += OnQuestionAnswered;
            GameEventBus.Instance.OnNewEnemyEncountered += OnNewEnemyEncountered;
            GameEventBus.Instance.OnWaveAnnounced += OnWaveAnnounced;

            GD.Print("✓ Sistemas inicializados");
        }

        private void OnWaveAnnounced(int wave, string title, string desc)
        {
            // Mostrar la narrativa al inicio de la oleada
            _quizView.ShowInfo($"OLEADA {wave}: {title}", desc, "PREPÁRATE PARA LA DEFENSA");
        }

        private void OnNewEnemyEncountered(string name, string desc, string weakness)
        {
            _quizView.ShowInfo(name, desc, weakness);
        }

        private void OnVulnerabilityDetected(string vulnerability)
        {
            if (vulnerability.Contains("Assessment"))
            {
                // Extraer el nivel del string "Level X Assessment"
                int level = 1;
                if (int.TryParse(vulnerability.Split(' ')[1], out int parsedLevel))
                {
                    level = parsedLevel;
                }

                // Pausar y mostrar pregunta de evaluación del nivel correspondiente
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
                // Recompensa: Restaurar integridad
                _player.Heal(25f);
                GameEventBus.Instance.EmitSecurityTipShown("¡Brecha parcheada! Integridad restaurada.");
            }
            else
            {
                // Penalización: Daño o mensaje
                GameEventBus.Instance.EmitSecurityTipShown("¡Fallo crítico! La brecha persiste.");
                if (_player != null) _player.TakeDamage(10, DamageType.Physical);
            }

            // Si estamos en la fase final del nivel (WaveSystem ya emitió Level 1 Assessment)
            // Verificamos si debemos completar el nivel
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
        }

        private void SetupScene()
        {
            // Configurar fondo animado con Parallax2D (Godot 4.3+)
            var bgScene = GD.Load<PackedScene>("res://Scenes/AnimatedBackground.tscn");
            if (bgScene != null)
            {
                // Eliminar fondo estático si existe
                GetNodeOrNull("Background")?.QueueFree();
                
                var parallax = new Parallax2D();
                parallax.Name = "ParallaxSystem";
                
                // Configurar movimiento relativo
                parallax.ScrollScale = new Vector2(0.5f, 0.5f);
                
                // Configurar repetición infinita con tamaño aumentado
                // Escalamos el fondo x2 para cubrir el viewport del zoom 0.5
                parallax.RepeatSize = new Vector2(2400, 1600);
                parallax.Autoscroll = new Vector2(0, 50); 
                
                var background = bgScene.Instantiate() as Node2D;
                if (background != null)
                {
                    background.Name = "AnimatedBackground";
                    background.Scale = new Vector2(2, 2); // Escalar x2
                    parallax.AddChild(background);
                }
                
                // Añadir como primer hijo
                AddChild(parallax);
                MoveChild(parallax, 0);
                
                GD.Print("✓ Fondo animado con Parallax2D (Escalado) cargado");
            }

            // Instanciar el jugador
            var playerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
            if (playerScene != null)
            {
                _player = playerScene.Instantiate<Entities.Player>();
                _player.Name = "Player";
                AddChild(_player);
                GD.Print("✓ Jugador creado");
            }
            else
            {
                GD.PrintErr("✗ No se pudo cargar la escena del jugador");
            }

            // Iniciar el juego
            _gameManager.StartGame();
        }

        public override void _Input(InputEvent @event)
        {
            // Manejo de Game Over
            if (_gameManager.CurrentState == GameState.GameOver)
            {
                if (@event.IsActionPressed("ui_cancel")) // ESC -> Menu
                {
                    GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
                }
                else if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.R) // R -> Restart
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
                    GD.Print("⏸ Juego pausado");
                }
                else if (_gameManager.CurrentState == GameState.Paused)
                {
                    _gameManager.ResumeGame();
                    GD.Print("▶ Juego reanudado");
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
