using Godot;
using System;
using System.Collections.Generic;

namespace CyberSecurityGame.Core.Events
{
    /// <summary>
    /// Event Bus global para comunicación desacoplada (Observer Pattern)
    /// Implementa el principio de Dependency Inversion (SOLID)
    /// </summary>
    public partial class GameEventBus : Node
    {
        private static GameEventBus _instance;
        public static GameEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameEventBus();
                }
                return _instance;
            }
        }

        // Eventos del juego
        public event Action<float> OnPlayerHealthChanged;
        public event Action OnPlayerDied;
        public event Action<int> OnScoreChanged;
        public event Action<string, int> OnEnemyDefeated;
        
        // Eventos de ciberseguridad
        public event Action<string> OnQuestionPresented;
        public event Action<bool> OnQuestionAnswered;
        public event Action<string> OnSecurityTipShown;
        public event Action<string> OnVulnerabilityDetected;
        public event Action<string> OnThreatNeutralized;
        
        // Eventos de power-ups
        public event Action<string> OnPowerUpCollected;
        public event Action<string> OnShieldActivated;
        
        // Eventos de nivel
        public event Action<int> OnLevelStarted;
        public event Action<int> OnLevelCompleted;
        public event Action<string> OnBossSpawned;

        public void EmitPlayerHealthChanged(float health)
        {
            OnPlayerHealthChanged?.Invoke(health);
        }

        public void EmitPlayerDied()
        {
            OnPlayerDied?.Invoke();
        }

        public void EmitScoreChanged(int score)
        {
            OnScoreChanged?.Invoke(score);
        }

        public void EmitEnemyDefeated(string enemyType, int points)
        {
            OnEnemyDefeated?.Invoke(enemyType, points);
        }

        public void EmitQuestionPresented(string question)
        {
            OnQuestionPresented?.Invoke(question);
        }

        public void EmitQuestionAnswered(bool correct)
        {
            OnQuestionAnswered?.Invoke(correct);
        }

        public void EmitSecurityTipShown(string tip)
        {
            OnSecurityTipShown?.Invoke(tip);
        }

        public void EmitVulnerabilityDetected(string vulnerability)
        {
            OnVulnerabilityDetected?.Invoke(vulnerability);
        }

        public void EmitThreatNeutralized(string threat)
        {
            OnThreatNeutralized?.Invoke(threat);
        }

        public void EmitPowerUpCollected(string powerUpType)
        {
            OnPowerUpCollected?.Invoke(powerUpType);
        }

        public void EmitShieldActivated(string shieldType)
        {
            OnShieldActivated?.Invoke(shieldType);
        }

        public void EmitLevelStarted(int level)
        {
            OnLevelStarted?.Invoke(level);
        }

        public void EmitLevelCompleted(int level)
        {
            OnLevelCompleted?.Invoke(level);
        }

        public void EmitBossSpawned(string bossName)
        {
            OnBossSpawned?.Invoke(bossName);
        }
    }
}
