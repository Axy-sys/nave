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
			// Valores base - RÁPIDO Y SATISFACTORIO
			Damage = 8f;
			ProjectileSpeed = 900f; // MUY rápido
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
			// MODO PRECISIÓN - Cyan brillante, muy rápido
			var proj = SpawnProjectileCustom(position, direction, 1000f, 12f, new Color("00ffff")); // Cyan
		}

		private void FireRapid(Vector2 position, Vector2 direction)
		{
			// MODO RÁPIDO - Amarillo/naranja, doble
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(-4)), 850f, 8f, new Color("ffcc00")); // Amarillo
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(4)), 850f, 8f, new Color("ffcc00"));
		}

		private void FireChaos(Vector2 position, Vector2 direction)
		{
			// MODO CAOS - Púrpura/magenta, triple
			SpawnProjectileCustom(position, direction, 700f, 15f, new Color("ff00ff")); // Magenta
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(-12)), 700f, 15f, new Color("ff00ff"));
			SpawnProjectileCustom(position, direction.Rotated(Mathf.DegToRad(12)), 700f, 15f, new Color("ff00ff"));
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
