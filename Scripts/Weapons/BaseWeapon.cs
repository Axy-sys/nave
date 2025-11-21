using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Weapons
{
	/// <summary>
	/// Clase base para armas usando Strategy Pattern
	/// Principio de Open/Closed: fácil de extender con nuevas armas
	/// </summary>
	public abstract partial class BaseWeapon : IWeapon
	{
		public float Damage = 10f;
		public float ProjectileSpeed = 500f;
		public PackedScene ProjectileScene;
		
		protected int _currentAmmo;
		protected int _maxAmmo;
		protected bool _needsReload = false;
		protected Node _sceneRoot; // Referencia al nodo raíz para instanciar proyectiles
		protected Node2D _owner; // Referencia al dueño del arma

		public virtual void SetOwner(Node2D owner)
		{
			_owner = owner;
		}

		public void SetSceneRoot(Node root)
		{
			_sceneRoot = root;
		}

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

			if (_sceneRoot == null)
			{
				GD.PrintErr("SceneRoot no está asignado en el arma");
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

			// Agregar al árbol de escena usando el nodo raíz
			_sceneRoot.AddChild(projectile);
		}
	}
}
