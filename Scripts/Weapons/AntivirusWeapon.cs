using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Weapons
{
	public partial class AntivirusWeapon : BaseWeapon
	{
		public AntivirusWeapon()
		{
			Damage = 25f;
			ProjectileSpeed = 500f;
			_maxAmmo = 20;
			_currentAmmo = _maxAmmo;
			ProjectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		}

		public override void Fire(Vector2 position, Vector2 direction)
		{
			// Dispara 3 proyectiles en abanico
			SpawnProjectile(position, direction, DamageType.Malware);
			SpawnProjectile(position, direction.Rotated(Mathf.DegToRad(15)), DamageType.Malware);
			SpawnProjectile(position, direction.Rotated(Mathf.DegToRad(-15)), DamageType.Malware);
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
			return "Antivirus Scanner";
		}

		public override WeaponType GetWeaponType()
		{
			return WeaponType.Antivirus;
		}
	}
}
