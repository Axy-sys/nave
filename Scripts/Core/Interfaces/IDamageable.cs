using Godot;

namespace CyberSecurityGame.Core.Interfaces
{
    /// <summary>
    /// Interface para entidades que pueden recibir daño
    /// Principio de Single Responsibility
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount, DamageType damageType);
        float GetCurrentHealth();
        float GetMaxHealth();
        bool IsAlive();
    }
    
    /// <summary>
    /// Tipos de daño relacionados con ciberseguridad
    /// </summary>
    public enum DamageType
    {
        Physical,      // Daño físico normal
        Malware,       // Daño de tipo malware
        DDoS,          // Ataque de denegación de servicio
        Phishing,      // Ataque de phishing
        BruteForce,    // Ataque de fuerza bruta
        SQLInjection   // Inyección SQL
    }
}
