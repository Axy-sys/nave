using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Weapons
{
	public partial class FirewallWeapon : BaseWeapon
	{
		public FirewallWeapon()
		{
			Damage = 15f;
			ProjectileSpeed = 600f;
			_maxAmmo = 100; // Infinite for basic weapon
			_currentAmmo = _maxAmmo;
			ProjectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		}

		public override void Fire(Vector2 position, Vector2 direction)
		{
			SpawnProjectile(position, direction, DamageType.Physical);
		}

		public override bool CanFire()
		{
			return true;
		}

		public override void Reload()
		{
			_currentAmmo = _maxAmmo;
		}

		public override string GetWeaponName()
		{
			return "Firewall Blaster";
		}

		public override WeaponType GetWeaponType()
		{
			return WeaponType.Firewall;
		}
	}
}
