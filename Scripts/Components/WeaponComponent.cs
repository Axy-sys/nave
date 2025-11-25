using Godot;
using CyberSecurityGame.Core.Interfaces;
using CyberSecurityGame.Weapons;

namespace CyberSecurityGame.Components
{
	/// <summary>
	/// Componente de arma usando Strategy Pattern
	/// Principio de Open/Closed y Dependency Inversion
	/// </summary>
	public partial class WeaponComponent : BaseComponent
	{
		[Export] public float FireRate = 0.06f; // MUY RÁPIDO - 16 disparos/seg
		[Export] public float LoadCost = 3f;    // Bajo costo - disparar es divertido
		[Export] public PackedScene ProjectileScene;
		
		private IWeapon _currentWeapon;
		private float _fireTimer = 0f;
		private Node2D _weaponOwner;
		private CpuComponent _cpuComponent;

		protected override void OnInitialize()
		{
			_weaponOwner = _owner as Node2D;
			_cpuComponent = _owner.GetNodeOrNull<CpuComponent>("CpuComponent");
			
			// Arma inicial por defecto (Firewall)
			SetWeapon(new FirewallWeapon());
		}

		protected override void OnUpdate(double delta)
		{
			if (_fireTimer > 0)
			{
				_fireTimer -= (float)delta;
			}
		}

		protected override void OnCleanup()
		{
			_currentWeapon = null;
		}

		/// <summary>
		/// Cambia el arma actual (Strategy Pattern)
		/// </summary>
		public void SetWeapon(IWeapon weapon)
		{
			_currentWeapon = weapon;
			
			// Configurar dueño
			_currentWeapon.SetOwner(_weaponOwner);

			// Si el arma es BaseWeapon, configurar el nodo raíz para spawning
			if (weapon is BaseWeapon baseWeapon)
			{
				baseWeapon.SetSceneRoot(_weaponOwner.GetTree()?.Root);
			}
			
			GD.Print($"Arma equipada: {weapon.GetWeaponName()}");
		}

		public bool TryFire(Vector2 direction)
		{
			if (_currentWeapon == null || !CanFire()) return false;

			_currentWeapon.Fire(_weaponOwner.GlobalPosition, direction);
			_fireTimer = FireRate;
			
			// Generar carga de CPU
			if (_cpuComponent != null)
			{
				_cpuComponent.AddLoad(LoadCost);
			}
			
			return true;
		}

		/// <summary>
		/// Dispara desde una posición específica (ej: morro de la nave)
		/// </summary>
		public bool TryFireFrom(Vector2 position, Vector2 direction)
		{
			if (_currentWeapon == null || !CanFire()) return false;

			_currentWeapon.Fire(position, direction);
			_fireTimer = FireRate;
			
			// Generar carga de CPU
			if (_cpuComponent != null)
			{
				_cpuComponent.AddLoad(LoadCost);
			}
			
			return true;
		}

		/// <summary>
		/// Dispara forzando el disparo sin respetar cooldown (para multishot/bursts)
		/// </summary>
		public void ForceFire(Vector2 position, Vector2 direction)
		{
			if (_currentWeapon == null) return;

			_currentWeapon.Fire(position, direction);
			
			// Generar carga de CPU también
			if (_cpuComponent != null)
			{
				_cpuComponent.AddLoad(LoadCost);
			}
		}

		public bool CanFire()
		{
			// Verificar si hay sobrecarga de CPU
			if (_cpuComponent != null && _cpuComponent.IsOverloaded())
			{
				return false;
			}

			return _fireTimer <= 0 && _currentWeapon != null && _currentWeapon.CanFire();
		}

		public IWeapon GetCurrentWeapon() => _currentWeapon;

		public void SetFireRate(float rate)
		{
			FireRate = Mathf.Max(0.05f, rate); // Mínimo 0.05 segundos
		}
	}
}
