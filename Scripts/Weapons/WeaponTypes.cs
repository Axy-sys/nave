using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Weapons
{
    /// <summary>
    /// Arma básica tipo Firewall
    /// Dispara proyectiles simples de protección
    /// </summary>
    public partial class FirewallWeapon : BaseWeapon
    {
        public FirewallWeapon()
        {
            Damage = 10f;
            ProjectileSpeed = 600f;
            _maxAmmo = -1; // Munición infinita
            _currentAmmo = -1;
        }

        public override void Fire(Vector2 position, Vector2 direction)
        {
            SpawnProjectile(position, direction.Normalized(), DamageType.Physical);
        }

        public override bool CanFire()
        {
            return true; // Siempre puede disparar
        }

        public override void Reload()
        {
            // No necesita recarga
        }

        public override string GetWeaponName() => "Firewall Básico";
        public override WeaponType GetWeaponType() => WeaponType.Firewall;
    }

    /// <summary>
    /// Arma Antivirus - Dispara ráfagas que neutralizan malware
    /// </summary>
    public partial class AntivirusWeapon : BaseWeapon
    {
        private int _burstCount = 3;
        private float _burstDelay = 0.1f;
        private int _currentBurst = 0;

        public AntivirusWeapon()
        {
            Damage = 15f;
            ProjectileSpeed = 700f;
            _maxAmmo = 30;
            _currentAmmo = 30;
        }

        public override void Fire(Vector2 position, Vector2 direction)
        {
            if (_currentAmmo <= 0) return;

            // Dispara ráfaga de 3 proyectiles
            for (int i = 0; i < _burstCount; i++)
            {
                Vector2 spreadDirection = direction.Rotated(Mathf.DegToRad((i - 1) * 10));
                SpawnProjectile(position, spreadDirection.Normalized(), DamageType.Malware);
            }

            _currentAmmo--;
            if (_currentAmmo <= 0)
            {
                _needsReload = true;
            }
        }

        public override bool CanFire()
        {
            return _currentAmmo > 0;
        }

        public override void Reload()
        {
            _currentAmmo = _maxAmmo;
            _needsReload = false;
            GD.Print("🔄 Antivirus recargado");
        }

        public override string GetWeaponName() => "Antivirus Scanner";
        public override WeaponType GetWeaponType() => WeaponType.Antivirus;
    }

    /// <summary>
    /// Arma de Encriptación - Proyectiles lentos pero poderosos
    /// </summary>
    public partial class EncryptionWeapon : BaseWeapon
    {
        public EncryptionWeapon()
        {
            Damage = 30f;
            ProjectileSpeed = 400f;
            _maxAmmo = 10;
            _currentAmmo = 10;
        }

        public override void Fire(Vector2 position, Vector2 direction)
        {
            if (_currentAmmo <= 0) return;

            SpawnProjectile(position, direction.Normalized(), DamageType.Physical);
            _currentAmmo--;
            
            if (_currentAmmo <= 0)
            {
                _needsReload = true;
            }
        }

        public override bool CanFire()
        {
            return _currentAmmo > 0;
        }

        public override void Reload()
        {
            _currentAmmo = _maxAmmo;
            _needsReload = false;
            GD.Print("🔄 Encriptación recargada");
        }

        public override string GetWeaponName() => "Cannon de Encriptación";
        public override WeaponType GetWeaponType() => WeaponType.Encryption;
    }

    /// <summary>
    /// Honeypot - Coloca trampas que atraen y dañan enemigos
    /// </summary>
    public partial class HoneypotWeapon : BaseWeapon
    {
        public HoneypotWeapon()
        {
            Damage = 5f; // Daño por tick
            ProjectileSpeed = 0f; // Estático
            _maxAmmo = 5;
            _currentAmmo = 5;
        }

        public override void Fire(Vector2 position, Vector2 direction)
        {
            if (_currentAmmo <= 0) return;

            // Coloca un honeypot estático
            SpawnProjectile(position, Vector2.Zero, DamageType.Physical);
            _currentAmmo--;
            
            if (_currentAmmo <= 0)
            {
                _needsReload = true;
            }
            
            GD.Print("🍯 Honeypot desplegado");
        }

        public override bool CanFire()
        {
            return _currentAmmo > 0;
        }

        public override void Reload()
        {
            _currentAmmo = _maxAmmo;
            _needsReload = false;
            GD.Print("🔄 Honeypots recargados");
        }

        public override string GetWeaponName() => "Honeypot Trap";
        public override WeaponType GetWeaponType() => WeaponType.Honeypot;
    }
}
