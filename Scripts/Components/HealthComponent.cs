using Godot;
using CyberSecurityGame.Core.Interfaces;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Components
{
    /// <summary>
    /// Componente de salud usando composición (Component Pattern)
    /// Principio de Single Responsibility: solo maneja la salud
    /// </summary>
    public partial class HealthComponent : BaseComponent, IDamageable
    {
        [Export] public float MaxHealth = 100f;
        [Export] public bool IsPlayer = false;
        
        private float _currentHealth;
        private bool _isAlive = true;

        // Resistencias a diferentes tipos de daño (0-1, donde 1 = inmune)
        private float _malwareResistance = 0f;
        private float _ddosResistance = 0f;
        private float _phishingResistance = 0f;
        private float _bruteForceResistance = 0f;

        protected override void OnInitialize()
        {
            _currentHealth = MaxHealth;
            _isAlive = true;
        }

        protected override void OnUpdate(double delta)
        {
            // Actualización de estado si es necesario
        }

        protected override void OnCleanup()
        {
            // Limpieza de recursos
        }

        public void TakeDamage(float amount, DamageType damageType)
        {
            if (!_isAlive) return;

            // Aplicar resistencia según el tipo de daño
            float resistance = GetResistanceForDamageType(damageType);
            float actualDamage = amount * (1f - resistance);

            _currentHealth -= actualDamage;
            _currentHealth = Mathf.Max(0, _currentHealth);

            if (IsPlayer)
            {
                GameEventBus.Instance.EmitPlayerHealthChanged(_currentHealth);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }

            // Log educativo del tipo de daño recibido
            LogDamageType(damageType, actualDamage);
        }

        public float GetCurrentHealth() => _currentHealth;
        public float GetMaxHealth() => MaxHealth;
        public bool IsAlive() => _isAlive;

        public void Heal(float amount)
        {
            if (!_isAlive) return;
            
            _currentHealth = Mathf.Min(_currentHealth + amount, MaxHealth);
            
            if (IsPlayer)
            {
                GameEventBus.Instance.EmitPlayerHealthChanged(_currentHealth);
            }
        }

        public void SetResistance(DamageType damageType, float resistance)
        {
            resistance = Mathf.Clamp(resistance, 0f, 1f);
            
            switch (damageType)
            {
                case DamageType.Malware:
                    _malwareResistance = resistance;
                    break;
                case DamageType.DDoS:
                    _ddosResistance = resistance;
                    break;
                case DamageType.Phishing:
                    _phishingResistance = resistance;
                    break;
                case DamageType.BruteForce:
                    _bruteForceResistance = resistance;
                    break;
            }
        }

        private float GetResistanceForDamageType(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Malware => _malwareResistance,
                DamageType.DDoS => _ddosResistance,
                DamageType.Phishing => _phishingResistance,
                DamageType.BruteForce => _bruteForceResistance,
                _ => 0f
            };
        }

        private void Die()
        {
            _isAlive = false;
            
            if (IsPlayer)
            {
                GameEventBus.Instance.EmitPlayerDied();
            }
            
            // El owner (nave/enemigo) puede reaccionar a esto
            _owner?.CallDeferred("queue_free");
        }

        private void LogDamageType(DamageType damageType, float damage)
        {
            string damageInfo = damageType switch
            {
                DamageType.Malware => "🦠 Malware detectado",
                DamageType.DDoS => "⚡ Ataque DDoS en curso",
                DamageType.Phishing => "🎣 Intento de Phishing",
                DamageType.BruteForce => "🔨 Ataque de fuerza bruta",
                DamageType.SQLInjection => "💉 SQL Injection detectada",
                _ => "💥 Daño recibido"
            };
            
            GD.Print($"{damageInfo}: {damage:F1} puntos");
        }

        public float GetHealthPercentage()
        {
            return MaxHealth > 0 ? _currentHealth / MaxHealth : 0f;
        }
    }
}
