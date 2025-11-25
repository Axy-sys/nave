using Godot;
using CyberSecurityGame.Core.Interfaces;
using CyberSecurityGame.Entities;

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

		protected void SpawnProjectile(Vector2 position, Vector2 direction, DamageType damageType, bool isPlayerProjectile = true)
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

			// Validar dirección
			if (direction == Vector2.Zero || !direction.IsFinite())
			{
				GD.PrintErr($"BaseWeapon: dirección inválida {direction}");
				direction = Vector2.Up;
			}

			var projectile = ProjectileScene.Instantiate() as Node2D;
			if (projectile == null) return;

			// Configurar posición
			projectile.GlobalPosition = position;
			
			GD.Print($"🔫 Spawning projectile at {position} dir {direction}");
			
			// PRIMERO añadir al árbol de escena
			_sceneRoot.AddChild(projectile);
			
			// DESPUÉS inicializar (ahora _PhysicsProcess funcionará)
			if (projectile is Projectile proj)
			{
				proj.Initialize(direction.Normalized(), ProjectileSpeed, Damage, (int)damageType, isPlayerProjectile);
			}
			else if (projectile.HasMethod("Initialize"))
			{
				projectile.Call("Initialize", direction.Normalized(), ProjectileSpeed, Damage, (int)damageType, isPlayerProjectile);
			}
		}
	}
}
