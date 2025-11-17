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
		[Export] public float FireRate = 0.2f; // Tiempo entre disparos
		[Export] public PackedScene ProjectileScene;
		
		private IWeapon _currentWeapon;
		private float _fireTimer = 0f;
		private Node2D _weaponOwner;

		protected override void OnInitialize()
		{
			_weaponOwner = _owner as Node2D;
			
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
			
			return true;
		}

		public bool CanFire()
		{
			return _fireTimer <= 0 && _currentWeapon != null && _currentWeapon.CanFire();
		}

		public IWeapon GetCurrentWeapon() => _currentWeapon;

		public void SetFireRate(float rate)
		{
			FireRate = Mathf.Max(0.05f, rate); // Mínimo 0.05 segundos
		}
	}
}
