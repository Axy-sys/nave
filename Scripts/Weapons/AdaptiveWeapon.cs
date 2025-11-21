using Godot;
using CyberSecurityGame.Core.Interfaces;
using CyberSecurityGame.Components;

namespace CyberSecurityGame.Weapons
{
	/// <summary>
	/// Arma adaptativa que cambia su comportamiento según la carga de CPU (Heat).
	/// "Less is More": Una sola arma que evoluciona.
	/// </summary>
	public partial class AdaptiveWeapon : BaseWeapon
	{
		private CpuComponent _cpuComponent;

		public AdaptiveWeapon()
		{
			// Valores base
			Damage = 10f;
			ProjectileSpeed = 600f;
			ProjectileScene = GD.Load<PackedScene>("res://Scenes/Projectile.tscn");
		}

		public override void SetOwner(Node2D owner)
		{
			base.SetOwner(owner);
			_cpuComponent = owner.GetNodeOrNull<CpuComponent>("CpuComponent");
		}

		public override void Fire(Vector2 position, Vector2 direction)
		{
			float heat = _cpuComponent != null ? _cpuComponent.GetLoadPercentage() : 0f;

			if (heat < 0.3f)
			{
				// MODO PRECISIÓN (Verde)
				// Disparo rápido y preciso
				FirePrecision(position, direction);
			}
			else if (heat < 0.7f)
			{
				// MODO FUEGO RÁPIDO (Amarillo)
				// Disparo doble con ligera dispersión
				FireRapid(position, direction);
			}
			else
			{
				// MODO CAOS (Rojo/Púrpura)
				// Escopeta de alto daño pero corto alcance
				FireChaos(position, direction);
			}
		}

		private void FirePrecision(Vector2 position, Vector2 direction)
		{
			var proj = SpawnProjectileCustom(position, direction, 800f, 15f, new Color("00ff41")); // Terminal Green
		}

		private void FireRapid(Vector2 position, Vector2 direction)
		{
			// Disparar 2 proyectiles con ligera desviación
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(-5)), 600f, 10f, new Color("ffaa00")); // Flux Orange
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(5)), 600f, 10f, new Color("ffaa00"));
		}

		private void FireChaos(Vector2 position, Vector2 direction)
		{
			// Escopeta: 3 proyectiles
			SpawnProjectileCustom(position, direction, 450f, 20f, new Color("bf00ff")); // Rippier Purple
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(-15)), 450f, 20f, new Color("bf00ff"));
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(15)), 450f, 20f, new Color("bf00ff"));
		}

		private Node2D SpawnProjectileCustom(Vector2 position, Vector2 direction, float speed, float damage, Color color)
		{
			if (ProjectileScene == null || _sceneRoot == null) return null;

			var projectile = ProjectileScene.Instantiate() as Node2D;
			if (projectile == null) return null;

			projectile.GlobalPosition = position;
			projectile.Modulate = color; // Cambiar color según modo

			if (projectile.HasMethod("Initialize"))
			{
				projectile.Call("Initialize", direction, speed, damage, (int)DamageType.Physical);
			}

			_sceneRoot.AddChild(projectile);
			return projectile;
		}

		public override bool CanFire()
		{
			return true;
		}

		public override void Reload()
		{
			// No reload needed
		}

		public override string GetWeaponName()
		{
			return "Adaptive Blaster";
		}

		public override WeaponType GetWeaponType()
		{
			return WeaponType.Firewall; // Usamos un tipo genérico por ahora
		}
	}
}
