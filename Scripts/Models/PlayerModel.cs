using Godot;

namespace CyberSecurityGame.Models
{
    /// <summary>
    /// Model que representa los datos del jugador (MVC Pattern)
    /// Principio de Single Responsibility: solo maneja datos del jugador
    /// </summary>
    public class PlayerModel
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public int Lives { get; private set; }
        public int MaxLives { get; private set; }
        public float ShieldStrength { get; set; }
        public bool HasShield { get; set; }
        
        // Estadísticas de progreso
        public int TotalDamageDealt { get; set; }
        public int ThreatsNeutralized { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }

        public PlayerModel(float maxHealth, int maxLives)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MaxLives = maxLives;
            Lives = maxLives;
            ShieldStrength = 0f;
            HasShield = false;
            
            TotalDamageDealt = 0;
            ThreatsNeutralized = 0;
            CorrectAnswers = 0;
            IncorrectAnswers = 0;
        }

        public void TakeDamage(float damage)
        {
            // Si hay escudo, absorbe primero
            if (HasShield && ShieldStrength > 0)
            {
                float shieldDamage = Mathf.Min(damage, ShieldStrength);
                ShieldStrength -= shieldDamage;
                damage -= shieldDamage;
                
                if (ShieldStrength <= 0)
                {
                    HasShield = false;
                    ShieldStrength = 0;
                }
            }
            
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Max(0, CurrentHealth);
        }

        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }

        public void ActivateShield(float strength)
        {
            HasShield = true;
            ShieldStrength = strength;
        }

        public void LoseLife()
        {
            Lives--;
            if (Lives > 0)
            {
                CurrentHealth = MaxHealth; // Restaura salud al perder vida
            }
        }

        public void Reset(float maxHealth, int maxLives)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            MaxLives = maxLives;
            Lives = maxLives;
            ShieldStrength = 0f;
            HasShield = false;
        }

        public float GetHealthPercentage()
        {
            return CurrentHealth / MaxHealth;
        }

        public bool IsAlive()
        {
            return CurrentHealth > 0;
        }
    }
}
