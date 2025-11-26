using Godot;
using CyberSecurityGame.Core.Interfaces;
using CyberSecurityGame.Components;

namespace CyberSecurityGame.Entities
{
	/// <summary>
	/// Proyectil genérico para todas las armas
	/// Principio de Single Responsibility
	/// </summary>
	public partial class Projectile : Area2D
	{
		private Vector2 _direction = Vector2.Up;
		private float _speed = 500f;
		private float _damage = 10f;
		private DamageType _damageType = DamageType.Physical;
		private float _lifetime = 5f;
		private float _timer = 0f;
		private bool _isPlayerProjectile = true;
		private bool _hasHit = false;

		public override void _Ready()
		{
			// Conectar señales
			BodyEntered += OnBodyEntered;
			AreaEntered += OnAreaEntered;
		}

		public void Initialize(Vector2 direction, float speed, float damage, int damageType, bool isPlayerProjectile = true)
		{
			// Asegurar que la dirección es válida
			if (direction == Vector2.Zero || float.IsNaN(direction.X) || float.IsNaN(direction.Y))
			{
				direction = Vector2.Up;
				GD.PrintErr("Projectile: dirección inválida, usando Up");
			}
			
			_direction = direction.Normalized();
			_speed = speed;
			_damage = damage;
			_damageType = (DamageType)damageType;
			_isPlayerProjectile = isPlayerProjectile;
			_hasHit = false;
			_timer = 0f;

			if (_isPlayerProjectile)
			{
				AddToGroup("PlayerProjectiles");
			}

			// Rotar sprite hacia dirección de movimiento
			Rotation = _direction.Angle() + Mathf.Pi / 2;
			
			// Activar procesamiento
			SetProcess(true);
			SetPhysicsProcess(true);
			
			GD.Print($"🚀 Projectile OK: dir={_direction}, speed={_speed}");
		}

		public override void _Process(double delta)
		{
			// Mover el proyectil
			if (_direction != Vector2.Zero && _speed > 0)
			{
				Position += _direction * _speed * (float)delta;
			}

			// Lifetime
			_timer += (float)delta;
			if (_timer >= _lifetime)
			{
				QueueFree();
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (_hasHit) return; // Ya impactó algo
			
			// No dañar al jugador si es proyectil del jugador
			if (_isPlayerProjectile && body.IsInGroup("Player"))
			{
				return;
			}

			// Verificar si es un enemigo - múltiples formas de detectar
			bool isEnemy = body.IsInGroup("Enemy") || 
						   body.Name.ToString().Contains("Enemy") ||
						   body.Name.ToString().Contains("Malware") ||
						   body.Name.ToString().Contains("Phishing") ||
						   body.Name.ToString().Contains("DDoS") ||
						   body.Name.ToString().Contains("Ransomware") ||
						   body.Name.ToString().Contains("SQL") ||
						   body.Name.ToString().Contains("Brute");

			if (isEnemy)
			{
				_hasHit = true;
				
				// Buscar HealthComponent
				var healthComp = body.GetNodeOrNull<HealthComponent>("HealthComponent");
				if (healthComp != null)
				{
					healthComp.TakeDamage(_damage, _damageType);
					GD.Print($"🎯 Proyectil impactó {body.Name}: {_damage} daño");
				}
				else
				{
					GD.Print($"⚠️ Enemigo {body.Name} sin HealthComponent");
				}
				
				// Efecto visual de impacto (opcional)
				SpawnHitEffect();
				
				QueueFree();
			}
		}

		private void OnAreaEntered(Area2D area)
		{
			if (_hasHit) return;
			
			// Colisión con otras áreas enemigas
			bool isEnemy = area.IsInGroup("Enemy") || area.Name.ToString().Contains("Enemy");
			
			if (isEnemy)
			{
				_hasHit = true;
				
				// Buscar HealthComponent en el área o su padre
				var healthComp = area.GetNodeOrNull<HealthComponent>("HealthComponent");
				if (healthComp == null && area.GetParent() is Node parent)
				{
					healthComp = parent.GetNodeOrNull<HealthComponent>("HealthComponent");
				}
				
				if (healthComp != null)
				{
					healthComp.TakeDamage(_damage, _damageType);
				}
				
				QueueFree();
			}
		}

		private void SpawnHitEffect()
		{
			// Efecto de partículas simple al impactar
			var particles = new CpuParticles2D();
			particles.GlobalPosition = GlobalPosition;
			particles.Emitting = true;
			particles.Amount = 8;
			particles.OneShot = true;
			particles.Explosiveness = 1f;
			particles.Lifetime = 0.3f;
			particles.SpeedScale = 3;
			particles.Direction = -_direction;
			particles.Spread = 45;
			particles.InitialVelocityMin = 50;
			particles.InitialVelocityMax = 100;
			particles.ScaleAmountMin = 2;
			particles.ScaleAmountMax = 4;
			particles.Color = new Color("#00ff41");
			
			GetTree().Root.AddChild(particles);
			
			// Auto-destruir partículas
			var timer = GetTree().CreateTimer(0.5);
			timer.Timeout += () => particles.QueueFree();
		}
	}
}
