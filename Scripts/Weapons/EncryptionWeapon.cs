using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Weapons
{
	public partial class EncryptionWeapon : BaseWeapon
	{
		public EncryptionWeapon()
		{
			Damage = 40f;
			ProjectileSpeed = 800f;
			_maxAmmo = 10;
			_currentAmmo = _maxAmmo;
			ProjectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		}

		public override void Fire(Vector2 position, Vector2 direction)
		{
			// Disparo rápido y potente
			SpawnProjectile(position, direction, DamageType.SQLInjection);
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
			return "Encryption Key";
		}

		public override WeaponType GetWeaponType()
		{
			return WeaponType.Encryption;
		}
	}
}
