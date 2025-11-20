using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Weapons
{
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
            ProjectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
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
