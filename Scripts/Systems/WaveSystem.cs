using Godot;
using CyberSecurityGame.Entities;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
	/// <summary>
	/// Sistema de spawning de enemigos con oleadas progresivas
	/// Implementa patrón de dificultad creciente
	/// </summary>
	public partial class WaveSystem : Node
	{
		private static WaveSystem _instance;
		public static WaveSystem Instance => _instance;

		[Export] public float TimeBetweenWaves = 20f;
		[Export] public int InitialEnemyCount = 3;
		[Export] public float DifficultyScale = 1.2f;

		private int _currentWave = 0;
		private float _waveTimer = 0f;
		private int _enemiesRemaining = 0;
		private bool _waveActive = false;

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}
			_instance = this;
			
			EnemyFactory.Initialize();
		}

		public override void _Process(double delta)
		{
			if (!_waveActive)
			{
				_waveTimer += (float)delta;
				
				if (_waveTimer >= TimeBetweenWaves)
				{
					StartNextWave();
					_waveTimer = 0f;
				}
			}
			else
			{
				// Verificar si quedan enemigos
				if (_enemiesRemaining <= 0)
				{
					EndWave();
				}
			}
		}

		private void StartNextWave()
		{
			_currentWave++;
			_waveActive = true;
			
			int enemyCount = CalculateEnemyCount();
			_enemiesRemaining = enemyCount;

			GD.Print($"🌊 Iniciando Oleada {_currentWave} - {enemyCount} amenazas");
			GameEventBus.Instance.EmitLevelStarted(_currentWave);

			SpawnWaveEnemies(enemyCount);
		}

		private void SpawnWaveEnemies(int count)
		{
			var random = new System.Random();
			var enemyTypes = System.Enum.GetValues(typeof(EnemyType));

			for (int i = 0; i < count; i++)
			{
				// Retraso escalonado para spawning
				var timer = GetTree().CreateTimer(i * 0.5f);
				int index = i; // Captura para lambda
				timer.Timeout += () => SpawnSingleEnemy(random, enemyTypes);
			}
		}

		private void SpawnSingleEnemy(System.Random random, System.Array enemyTypes)
		{
			var type = (EnemyType)enemyTypes.GetValue(random.Next(enemyTypes.Length));
			Vector2 spawnPos = GetRandomSpawnPosition();
			
			var enemy = EnemyFactory.CreateEnemy(type, spawnPos);
			if (enemy != null)
			{
				GetTree().Root.GetNode("Main").AddChild(enemy);
				
				// Suscribirse a la muerte del enemigo
				enemy.TreeExiting += () => OnEnemyDefeated(type);
			}
		}

		private void OnEnemyDefeated(EnemyType type)
		{
			_enemiesRemaining--;
			
			// Calcular puntos basados en oleada y tipo
			int points = CalculatePoints(type);
			GameEventBus.Instance.EmitEnemyDefeated(type.ToString(), points);
		}

		private void EndWave()
		{
			_waveActive = false;
			GD.Print($"✅ Oleada {_currentWave} completada!");
			GameEventBus.Instance.EmitLevelCompleted(_currentWave);
			
			// Boss cada 5 oleadas
			if (_currentWave % 5 == 0)
			{
				SpawnBoss();
			}
		}

		private void SpawnBoss()
		{
			GD.Print("👹 ¡Boss apareciendo!");
			GameEventBus.Instance.EmitBossSpawned("Ransomware Boss");
			
			Vector2 spawnPos = new Vector2(500, 100); // Centro arriba
			var boss = EnemyFactory.CreateEnemy(EnemyType.Ransomware, spawnPos);
			
			if (boss != null)
			{
				GetTree().Root.GetNode("Main").AddChild(boss);
				boss.TreeExiting += () => OnBossDefeated();
			}
		}

		private void OnBossDefeated()
		{
			GD.Print("🏆 ¡Boss derrotado!");
			GameEventBus.Instance.EmitEnemyDefeated("Boss", 5000);
		}

		private int CalculateEnemyCount()
		{
			// Aumenta enemigos por oleada con límite
			int count = (int)(InitialEnemyCount * Mathf.Pow(DifficultyScale, _currentWave - 1));
			return Mathf.Min(count, 20); // Máximo 20 enemigos por oleada
		}

		private int CalculatePoints(EnemyType type)
		{
			int basePoints = type switch
			{
				EnemyType.Malware => 10,
				EnemyType.Phishing => 15,
				EnemyType.DDoS => 20,
				EnemyType.SQLInjection => 25,
				EnemyType.BruteForce => 15,
				EnemyType.Ransomware => 50,
				EnemyType.Trojan => 20,
				EnemyType.Worm => 12,
				_ => 10
			};

			// Multiplicador por oleada
			return (int)(basePoints * (1 + _currentWave * 0.1f));
		}

		private Vector2 GetRandomSpawnPosition()
		{
			var random = new System.Random();
			
			// Spawns desde arriba
			float x = (float)random.NextDouble() * 1000 + 50;
			float y = -50; // Fuera de pantalla arriba
			
			return new Vector2(x, y);
		}

		public int GetCurrentWave() => _currentWave;
		public int GetEnemiesRemaining() => _enemiesRemaining;
		public bool IsWaveActive() => _waveActive;
	}
}
