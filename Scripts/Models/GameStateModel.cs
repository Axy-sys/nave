using Godot;
using System.Collections.Generic;

namespace CyberSecurityGame.Models
{
	/// <summary>
	/// Model que representa el estado del juego (MVC Pattern)
	/// Principio de Single Responsibility: solo maneja datos del estado
	/// </summary>
	public class GameStateModel
	{
		public int CurrentWave { get; set; }
		public float TimeElapsed { get; set; }
		public Dictionary<string, int> EnemiesDefeatedByType { get; private set; }
		public List<string> UnlockedWeapons { get; private set; }
		public Dictionary<string, bool> CompletedQuizzes { get; private set; }
		
		public GameStateModel()
		{
			EnemiesDefeatedByType = new Dictionary<string, int>();
			UnlockedWeapons = new List<string> { "Firewall" }; // Arma inicial
			CompletedQuizzes = new Dictionary<string, bool>();
			CurrentWave = 0;
			TimeElapsed = 0f;
		}

		public void RecordEnemyDefeated(string enemyType)
		{
			if (!EnemiesDefeatedByType.ContainsKey(enemyType))
			{
				EnemiesDefeatedByType[enemyType] = 0;
			}
			EnemiesDefeatedByType[enemyType]++;
		}

		public void UnlockWeapon(string weaponName)
		{
			if (!UnlockedWeapons.Contains(weaponName))
			{
				UnlockedWeapons.Add(weaponName);
				GD.Print($"¡Arma desbloqueada: {weaponName}!");
			}
		}

		public void CompleteQuiz(string quizId)
		{
			CompletedQuizzes[quizId] = true;
		}

		public int GetTotalEnemiesDefeated()
		{
			int total = 0;
			foreach (var count in EnemiesDefeatedByType.Values)
			{
				total += count;
			}
			return total;
		}
	}
}
