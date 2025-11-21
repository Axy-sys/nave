using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Components
{
	/// <summary>
	/// Componente de escudo de protección
	/// Representa diferentes tipos de protección de ciberseguridad
	/// </summary>
	public partial class ShieldComponent : BaseComponent
	{
		[Export] public float MaxShieldStrength = 50f;
		[Export] public float RechargeRate = 5f; // Puntos por segundo
		[Export] public float RechargeDelay = 3f; // Segundos antes de empezar a recargar
		
		// Parry Configuration
		[Export] public float ParryWindow = 0.2f; // Time window for perfect parry
		[Export] public float ParryCooldown = 1.0f;
		[Export] public float FluxDamageRatio = 0.5f; // Cuánta carga de CPU genera el daño absorbido

		private float _currentStrength;
		private float _rechargeTimer = 0f;
		private bool _isActive = false;
		private ShieldType _shieldType = ShieldType.Firewall;

		private bool _isParrying = false;
		private float _parryTimer = 0f;
		private float _parryCooldownTimer = 0f;
		
		private CpuComponent _cpuComponent;

		protected override void OnInitialize()
		{
			_currentStrength = 0f;
			_isActive = false;
			_cpuComponent = _owner.GetNodeOrNull<CpuComponent>("CpuComponent");
		}

		protected override void OnUpdate(double delta)
		{
			// Update Parry Timers
			if (_parryCooldownTimer > 0)
			{
				_parryCooldownTimer -= (float)delta;
			}

			if (_isParrying)
			{
				_parryTimer -= (float)delta;
				if (_parryTimer <= 0)
				{
					_isParrying = false;
				}
			}

			if (!_isActive) return;

			// Sistema de recarga automática
			if (_currentStrength < MaxShieldStrength)
			{
				if (_rechargeTimer > 0)
				{
					_rechargeTimer -= (float)delta;
				}
				else
				{
					_currentStrength += RechargeRate * (float)delta;
					_currentStrength = Mathf.Min(_currentStrength, MaxShieldStrength);
				}
			}
		}

		public bool TriggerParry()
		{
			if (_parryCooldownTimer > 0 || _isParrying) return false;

			_isParrying = true;
			_parryTimer = ParryWindow;
			_parryCooldownTimer = ParryCooldown;
			
			GD.Print("🛡️ Parry Attempt!");
			return true;
		}

		protected override void OnCleanup()
		{
			_isActive = false;
		}

		public void ActivateShield(ShieldType type, float strength)
		{
			_shieldType = type;
			MaxShieldStrength = strength;
			_currentStrength = strength;
			_isActive = true;
			
			string shieldName = GetShieldName(type);
			GameEventBus.Instance.EmitShieldActivated(shieldName);
			GD.Print($"🛡️ Escudo activado: {shieldName}");
		}

		public float AbsorbDamage(float damage)
		{
			// Check for Parry
			if (_isParrying)
			{
				GD.Print("✨ PERFECT PARRY! Damage Negated.");
				GameEventBus.Instance.EmitShieldActivated("PARRY SUCCESS");
				// Bonus: Reducir carga de CPU al hacer parry
				if (_cpuComponent != null)
				{
					_cpuComponent.AddLoad(-10f); // Ventilación instantánea
				}
				return 0f;
			}

			// Si hay sobrecarga, el escudo no funciona
			if (_cpuComponent != null && _cpuComponent.IsOverloaded())
			{
				return damage;
			}

			if (!_isActive || _currentStrength <= 0) return damage;

			float absorbed = Mathf.Min(damage, _currentStrength);
			_currentStrength -= absorbed;
			
			// Generar carga de CPU por daño absorbido
			if (_cpuComponent != null)
			{
				_cpuComponent.AddLoad(absorbed * FluxDamageRatio);
			}
			
			// Reiniciar timer de recarga cuando recibe daño
			_rechargeTimer = RechargeDelay;

			if (_currentStrength <= 0)
			{
				_currentStrength = 0;
				_isActive = false;
				GD.Print("🛡️ Escudo agotado");
			}

			return damage - absorbed;
		}

		public new bool IsActive() => _isActive;
		public float GetStrength() => _currentStrength;
		public float GetStrengthPercentage() => MaxShieldStrength > 0 ? _currentStrength / MaxShieldStrength : 0f;

		private string GetShieldName(ShieldType type)
		{
			return type switch
			{
				ShieldType.Firewall => "Firewall",
				ShieldType.Encryption => "Encriptación",
				ShieldType.Antivirus => "Antivirus",
				ShieldType.IDS => "Sistema de Detección de Intrusos",
				_ => "Escudo"
			};
		}
	}

	public enum ShieldType
	{
		Firewall,
		Encryption,
		Antivirus,
		IDS
	}
}
