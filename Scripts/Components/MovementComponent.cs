using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Components
{
	/// <summary>
	/// Componente de movimiento usando composición (Component Pattern)
	/// Principio de Single Responsibility: solo maneja movimiento
	/// </summary>
	public partial class MovementComponent : BaseComponent, IMovable
	{
		[Export] public float Speed = 300f;
		[Export] public float Acceleration = 1000f;
		[Export] public float Friction = 500f;
		[Export] public bool UseAcceleration = true;

		private Vector2 _velocity = Vector2.Zero;
		private CharacterBody2D _body;

		protected override void OnInitialize()
		{
			_body = _owner as CharacterBody2D;
			if (_body == null)
			{
				GD.PrintErr("MovementComponent requiere un CharacterBody2D como owner");
			}
		}

		protected override void OnUpdate(double delta)
		{
			if (_body == null) return;

			// Aplicar fricción cuando no hay input
			if (_velocity.Length() > 0 && !UseAcceleration)
			{
				var frictionAmount = Friction * (float)delta;
				if (_velocity.Length() > frictionAmount)
				{
					_velocity -= _velocity.Normalized() * frictionAmount;
				}
				else
				{
					_velocity = Vector2.Zero;
				}
			}
		}

		protected override void OnCleanup()
		{
			_velocity = Vector2.Zero;
		}

		public void Move(Vector2 direction, double delta)
		{
			if (_body == null) return;

			if (UseAcceleration)
			{
				// Movimiento con aceleración suave
				if (direction.Length() > 0)
				{
					_velocity += direction.Normalized() * Acceleration * (float)delta;
					_velocity = _velocity.LimitLength(Speed);
				}
				else
				{
					// Aplicar fricción
					var frictionAmount = Friction * (float)delta;
					if (_velocity.Length() > frictionAmount)
					{
						_velocity -= _velocity.Normalized() * frictionAmount;
					}
					else
					{
						_velocity = Vector2.Zero;
					}
				}
			}
			else
			{
				// Movimiento directo
				_velocity = direction.Normalized() * Speed;
			}

			_body.Velocity = _velocity;
			_body.MoveAndSlide();
		}

		public void SetSpeed(float speed)
		{
			Speed = speed;
		}

		public float GetSpeed() => Speed;

		public Vector2 GetVelocity() => _velocity;

		public void SetVelocity(Vector2 velocity)
		{
			_velocity = velocity;
		}

		public void Stop()
		{
			_velocity = Vector2.Zero;
			if (_body != null)
			{
				_body.Velocity = Vector2.Zero;
			}
		}
	}
}
