using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Components
{
    /// <summary>
    /// Componente de escudo de protección
    /// Representa diferentes tipos de protección de ciberseguridad
    /// </summary>
    public partial class ShieldComponent : BaseComponent
    {
        [Export] public float MaxShieldStrength = 50f;
        [Export] public float RechargeRate = 5f; // Puntos por segundo
        [Export] public float RechargeDelay = 3f; // Segundos antes de empezar a recargar
        
        private float _currentStrength;
        private float _rechargeTimer = 0f;
        private bool _isActive = false;
        private ShieldType _shieldType = ShieldType.Firewall;

        protected override void OnInitialize()
        {
            _currentStrength = 0f;
            _isActive = false;
        }

        protected override void OnUpdate(double delta)
        {
            if (!_isActive) return;

            // Sistema de recarga automática
            if (_currentStrength < MaxShieldStrength)
            {
                if (_rechargeTimer > 0)
                {
                    _rechargeTimer -= (float)delta;
                }
                else
                {
                    _currentStrength += RechargeRate * (float)delta;
                    _currentStrength = Mathf.Min(_currentStrength, MaxShieldStrength);
                }
            }
        }

        protected override void OnCleanup()
        {
            _isActive = false;
        }

        public void ActivateShield(ShieldType type, float strength)
        {
            _shieldType = type;
            MaxShieldStrength = strength;
            _currentStrength = strength;
            _isActive = true;
            
            string shieldName = GetShieldName(type);
            GameEventBus.Instance.EmitShieldActivated(shieldName);
            GD.Print($"🛡️ Escudo activado: {shieldName}");
        }

        public float AbsorbDamage(float damage)
        {
            if (!_isActive || _currentStrength <= 0) return damage;

            float absorbed = Mathf.Min(damage, _currentStrength);
            _currentStrength -= absorbed;
            
            // Reiniciar timer de recarga cuando recibe daño
            _rechargeTimer = RechargeDelay;

            if (_currentStrength <= 0)
            {
                _currentStrength = 0;
                _isActive = false;
                GD.Print("🛡️ Escudo agotado");
            }

            return damage - absorbed;
        }

        public new bool IsActive() => _isActive;
        public float GetStrength() => _currentStrength;
        public float GetStrengthPercentage() => MaxShieldStrength > 0 ? _currentStrength / MaxShieldStrength : 0f;

        private string GetShieldName(ShieldType type)
        {
            return type switch
            {
                ShieldType.Firewall => "Firewall",
                ShieldType.Encryption => "Encriptación",
                ShieldType.Antivirus => "Antivirus",
                ShieldType.IDS => "Sistema de Detección de Intrusos",
                _ => "Escudo"
            };
        }
    }

    public enum ShieldType
    {
        Firewall,
        Encryption,
        Antivirus,
        IDS
    }
}
