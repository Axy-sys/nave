using Godot;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Models;

namespace CyberSecurityGame.Core
{
    /// <summary>
    /// GameManager principal - Controller en patrón MVC
    /// Implementa Singleton Pattern y coordina todos los sistemas
    /// Principio de Single Responsibility: gestiona el flujo del juego
    /// </summary>
    public partial class GameManager : Node
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        [Export] public int StartingLives = 3;
        [Export] public float DifficultyIncreaseRate = 0.1f;

        // Referencias a modelos (Model en MVC)
        private GameStateModel _gameState;
        private PlayerModel _playerModel;
        
        // Estado del juego
        public GameState CurrentState { get; private set; }
        public int CurrentLevel { get; private set; }
        public int Score { get; private set; }
        public float DifficultyMultiplier { get; private set; }

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            InitializeGame();
            SubscribeToEvents();
        }

        private void InitializeGame()
        {
            _gameState = new GameStateModel();
            _playerModel = new PlayerModel(100f, StartingLives);
            
            CurrentState = GameState.Menu;
            CurrentLevel = 1;
            Score = 0;
            DifficultyMultiplier = 1.0f;
            
            GD.Print("GameManager inicializado - Juego de Ciberseguridad");
        }

        private void SubscribeToEvents()
        {
            GameEventBus.Instance.OnEnemyDefeated += HandleEnemyDefeated;
            GameEventBus.Instance.OnPlayerDied += HandlePlayerDeath;
            GameEventBus.Instance.OnQuestionAnswered += HandleQuestionAnswered;
            GameEventBus.Instance.OnLevelCompleted += HandleLevelCompleted;
        }

        public void StartGame()
        {
            CurrentState = GameState.Playing;
            CurrentLevel = 1;
            Score = 0;
            DifficultyMultiplier = 1.0f;
            
            _playerModel.Reset(100f, StartingLives);
            GameEventBus.Instance.EmitLevelStarted(CurrentLevel);
            
            GD.Print("Juego iniciado - Nivel ", CurrentLevel);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                GetTree().Paused = true;
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                GetTree().Paused = false;
            }
        }

        public void GameOver()
        {
            CurrentState = GameState.GameOver;
            GD.Print("Game Over - Puntuación final: ", Score);
            // Aquí se podría guardar high scores, etc.
        }

        private void HandleEnemyDefeated(string enemyType, int points)
        {
            AddScore(points);
            
            // Mensaje educativo basado en el tipo de enemigo
            string tip = GetSecurityTipForEnemy(enemyType);
            if (!string.IsNullOrEmpty(tip))
            {
                GameEventBus.Instance.EmitSecurityTipShown(tip);
            }
        }

        private void HandlePlayerDeath()
        {
            _playerModel.LoseLife();
            
            if (_playerModel.Lives <= 0)
            {
                GameOver();
            }
            else
            {
                // Respawn del jugador
                GD.Print("Vidas restantes: ", _playerModel.Lives);
            }
        }

        private void HandleQuestionAnswered(bool correct)
        {
            if (correct)
            {
                // Recompensa por respuesta correcta
                AddScore(500);
                GD.Print("¡Respuesta correcta! +500 puntos");
            }
            else
            {
                GD.Print("Respuesta incorrecta. Sigue intentando!");
            }
        }

        private void HandleLevelCompleted(int level)
        {
            CurrentLevel++;
            DifficultyMultiplier += DifficultyIncreaseRate;
            
            GD.Print($"¡Nivel {level} completado! Siguiente nivel: {CurrentLevel}");
            GameEventBus.Instance.EmitLevelStarted(CurrentLevel);
        }

        public void AddScore(int points)
        {
            Score += points;
            GameEventBus.Instance.EmitScoreChanged(Score);
        }

        private string GetSecurityTipForEnemy(string enemyType)
        {
            // Factory de tips educativos según el tipo de enemigo
            return enemyType switch
            {
                "Malware" => "💡 Siempre mantén tu antivirus actualizado",
                "Phishing" => "💡 Verifica siempre la URL antes de hacer clic",
                "DDoS" => "💡 Los ataques DDoS saturan servidores con tráfico falso",
                "SQLInjection" => "💡 Usa consultas parametrizadas para prevenir SQL Injection",
                "BruteForce" => "💡 Usa contraseñas largas y autenticación de dos factores",
                _ => ""
            };
        }

        public override void _ExitTree()
        {
            // Cleanup
            GameEventBus.Instance.OnEnemyDefeated -= HandleEnemyDefeated;
            GameEventBus.Instance.OnPlayerDied -= HandlePlayerDeath;
            GameEventBus.Instance.OnQuestionAnswered -= HandleQuestionAnswered;
            GameEventBus.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        LevelTransition
    }
}
