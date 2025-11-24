using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Entities
{
	/// <summary>
	/// Proyectil genérico para todas las armas
	/// Principio de Single Responsibility
	/// </summary>
	public partial class Projectile : Area2D
	{
		private Vector2 _direction;
		private float _speed;
		private float _damage;
		private DamageType _damageType;
		private float _lifetime = 5f;
		private float _timer = 0f;

		public void Initialize(Vector2 direction, float speed, float damage, int damageType)
		{
			_direction = direction.Normalized();
			_speed = speed;
			_damage = damage;
			_damageType = (DamageType)damageType;

			// Rotar sprite hacia dirección
			Rotation = direction.Angle() + Mathf.Pi / 2;  // +90°

			// Setup colisión - Usamos la forma ya existente en la escena si es posible
			if (GetNodeOrNull<CollisionShape2D>("CollisionShape2D") == null)
			{
				var collision = new CollisionShape2D();
				var shape = new CircleShape2D();
				shape.Radius = 5f;
				collision.Shape = shape;
				AddChild(collision);
			}

			// Conectar señales
			if (!IsConnected(SignalName.BodyEntered, Callable.From<Node2D>(OnBodyEntered)))
			{
				BodyEntered += OnBodyEntered;
			}
			if (!IsConnected(SignalName.AreaEntered, Callable.From<Area2D>(OnAreaEntered)))
			{
				AreaEntered += OnAreaEntered;
			}
		}

		public override void _Process(double delta)
		{
			// Movimiento
			Position += _direction * _speed * (float)delta;

			// Lifetime
			_timer += (float)delta;
			if (_timer >= _lifetime)
			{
				QueueFree();
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			// Verificar si es un enemigo
			if (body.Name.ToString().Contains("Enemy") || body.HasNode("HealthComponent"))
			{
				var healthComp = body.GetNodeOrNull<Components.HealthComponent>("HealthComponent");
				if (healthComp != null)
				{
					healthComp.TakeDamage(_damage, _damageType);
				}
				
				QueueFree();
			}
		}

		private void OnAreaEntered(Area2D area)
		{
			// Colisión con otras áreas
			if (area.Name.ToString().Contains("Enemy"))
			{
				QueueFree();
			}
		}
	}
}
