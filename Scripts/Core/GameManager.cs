using Godot;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Models;
using CyberSecurityGame.Entities;

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

		[Export] public int StartingLives = 4; // BALANCE: 4 vidas para dar más oportunidades
		[Export] public float DifficultyIncreaseRate = 0.1f;

		// Referencias a modelos (Model en MVC)
		private GameStateModel _gameState;
		private PlayerModel _playerModel;
		
		// Referencia al jugador
		private Player _player;
		
		// Estado del juego
		public GameState CurrentState { get; private set; }
		public int CurrentLevel { get; private set; }
		public int Score { get; private set; }
		public float DifficultyMultiplier { get; private set; }
		public int Lives { get; private set; }
		public int Deaths { get; private set; } = 0;  // Contador de muertes para estadísticas

		public override void _Process(double delta)
		{
			// Reducir cooldown de tips
			if (_tipCooldown > 0)
			{
				_tipCooldown -= (float)delta;
			}
		}
		
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
			Lives = StartingLives;
			DifficultyMultiplier = 1.0f;
			
			GD.Print($"GameManager inicializado - {StartingLives} vidas");
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
			Lives = StartingLives;
			DifficultyMultiplier = 1.0f;
			
			_playerModel.Reset(100f, StartingLives);
			
			// Buscar jugador
			_player = GetTree().Root.GetNodeOrNull<Player>("Main/Player");
			
			GameEventBus.Instance.EmitLevelStarted(CurrentLevel);
			GameEventBus.Instance.EmitGameStateChanged(CurrentState);
			
			GD.Print($"Juego iniciado - {Lives} vidas");
		}

		public void PauseGame()
		{
			if (CurrentState == GameState.Playing)
			{
				CurrentState = GameState.Paused;
				GetTree().Paused = true;
				GameEventBus.Instance.EmitGameStateChanged(CurrentState);
			}
		}

		public void ResumeGame()
		{
			if (CurrentState == GameState.Paused)
			{
				CurrentState = GameState.Playing;
				GetTree().Paused = false;
				GameEventBus.Instance.EmitGameStateChanged(CurrentState);
			}
		}

		public void GameOver()
		{
			CurrentState = GameState.GameOver;
			GameEventBus.Instance.EmitGameStateChanged(CurrentState);
			GD.Print("Game Over - Puntuación final: ", Score);
		}

		// Control de frecuencia de tips para no saturar
		private float _tipCooldown = 0f;
		private const float TIP_COOLDOWN_TIME = 8.0f; // Mínimo 8 segundos entre tips
		private string _lastTipType = "";
		
		private void HandleEnemyDefeated(string enemyType, int points)
		{
			AddScore(points);
			
			// Solo mostrar tip si ha pasado suficiente tiempo y es un tipo diferente
			if (_tipCooldown <= 0 && enemyType != _lastTipType)
			{
				string tip = GetSecurityTipForEnemy(enemyType);
				if (!string.IsNullOrEmpty(tip))
				{
					GameEventBus.Instance.EmitSecurityTipShown(tip);
					_tipCooldown = TIP_COOLDOWN_TIME;
					_lastTipType = enemyType;
				}
			}
		}

		private void HandlePlayerDeath()
		{
			Lives--;
			Deaths++;  // Incrementar contador de muertes para estadísticas
			_playerModel.LoseLife();
			
			GD.Print($"💀 Jugador murió - Vidas restantes: {Lives} (Muertes totales: {Deaths})");
			
			if (Lives <= 0)
			{
				GD.Print("☠️ GAME OVER - Sin vidas");
				GameOver();
			}
			else
			{
				// Respawn del jugador
				GD.Print($"🔄 Respawning... ({Lives} vidas)");
				
				// Buscar jugador y hacer respawn
				if (_player == null || !IsInstanceValid(_player))
				{
					_player = GetTree().Root.GetNodeOrNull<Player>("Main/Player");
				}
				
				if (_player != null)
				{
					_player.Respawn();
				}
				
				// Limpiar balas para dar respiro
				Systems.BulletHellSystem.Instance?.ClearAllBullets();
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
		
		/// <summary>
		/// Añade una vida extra al jugador
		/// BALANCE: Se gana cada 10 oleadas sobrevividas
		/// </summary>
		public void AddLife()
		{
			Lives++;
			_playerModel.AddLife();
			GD.Print($"💚 +1 Vida! Total: {Lives}");
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
