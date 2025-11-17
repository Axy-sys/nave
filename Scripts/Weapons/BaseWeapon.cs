using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Weapons
{
	/// <summary>
	/// Clase base para armas usando Strategy Pattern
	/// Principio de Open/Closed: fácil de extender con nuevas armas
	/// </summary>
	public abstract partial class BaseWeapon : Node, IWeapon
	{
		[Export] public float Damage = 10f;
		[Export] public float ProjectileSpeed = 500f;
		[Export] public PackedScene ProjectileScene;
		
		protected int _currentAmmo;
		protected int _maxAmmo;
		protected bool _needsReload = false;

		public abstract void Fire(Vector2 position, Vector2 direction);
		public abstract bool CanFire();
		public abstract void Reload();
		public abstract string GetWeaponName();
		public abstract WeaponType GetWeaponType();

		protected void SpawnProjectile(Vector2 position, Vector2 direction, DamageType damageType)
		{
			if (ProjectileScene == null)
			{
				GD.PrintErr("ProjectileScene no está asignado en el arma");
				return;
			}

			var projectile = ProjectileScene.Instantiate() as Node2D;
			if (projectile == null) return;

			// Configurar projectile
			projectile.GlobalPosition = position;
			
			// Si tiene script de proyectil, configurar velocidad y daño
			if (projectile.HasMethod("Initialize"))
			{
				projectile.Call("Initialize", direction, ProjectileSpeed, Damage, (int)damageType);
			}

			// Agregar al árbol de escena
			GetTree().Root.AddChild(projectile);
		}
	}
}
