using Godot;
using CyberSecurityGame.Core;
using CyberSecurityGame.Views;
using CyberSecurityGame.Systems;
using CyberSecurityGame.Education;

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

            // UI
            _hud = new GameHUD();
            _hud.Name = "GameHUD";
            AddChild(_hud);

            _quizView = new QuizView();
            _quizView.Name = "QuizView";
            AddChild(_quizView);

            GD.Print("✓ Sistemas inicializados");
        }

        private void SetupScene()
        {
            // Instanciar el jugador
            var playerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
            if (playerScene != null)
            {
                var player = playerScene.Instantiate<Node2D>();
                player.Name = "Player";
                AddChild(player);
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
