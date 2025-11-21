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

		// Dash Configuration
		[Export] public float DashSpeed = 800f;
		[Export] public float DashDuration = 0.2f;
		[Export] public float DashCooldown = 1.0f;
		[Export] public float DashLoadCost = 15f; // Costo de CPU por Dash

		private Vector2 _velocity = Vector2.Zero;
		private CharacterBody2D _body;
		
		private bool _isDashing = false;
		private float _dashTimer = 0f;
		private float _dashCooldownTimer = 0f;
		private Vector2 _dashDirection;
		
		private CpuComponent _cpuComponent;

		protected override void OnInitialize()
		{
			_body = _owner as CharacterBody2D;
			if (_body == null)
			{
				GD.PrintErr("MovementComponent requiere un CharacterBody2D como owner");
			}
			_cpuComponent = _owner.GetNodeOrNull<CpuComponent>("CpuComponent");
		}

		protected override void OnUpdate(double delta)
		{
			if (_body == null) return;

			// Update Dash Timers
			if (_dashCooldownTimer > 0)
			{
				_dashCooldownTimer -= (float)delta;
			}

			if (_isDashing)
			{
				_dashTimer -= (float)delta;
				if (_dashTimer <= 0)
				{
					EndDash();
				}
				else
				{
					// During dash, force movement in dash direction
					_velocity = _dashDirection * DashSpeed;
					_body.Velocity = _velocity;
					_body.MoveAndSlide();
					return; // Skip normal movement logic
				}
			}

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

		public bool TryDash(Vector2 direction)
		{
			if (_isDashing || _dashCooldownTimer > 0) return false;
			
			// Check CPU Overload
			if (_cpuComponent != null && _cpuComponent.IsOverloaded())
			{
				return false;
			}
			
			StartDash(direction);
			return true;
		}

		private void StartDash(Vector2 direction)
		{
			_isDashing = true;
			_dashTimer = DashDuration;
			_dashCooldownTimer = DashCooldown;
			
			// Add CPU Load
			if (_cpuComponent != null)
			{
				_cpuComponent.AddLoad(DashLoadCost);
			}
			
			// If no direction input, dash forward (based on current velocity or default up)
			if (direction == Vector2.Zero)
			{
				_dashDirection = _velocity.Length() > 0 ? _velocity.Normalized() : Vector2.Up;
			}
			else
			{
				_dashDirection = direction.Normalized();
			}
			
			GD.Print("Dash Started!");
		}

		private void EndDash()
		{
			_isDashing = false;
			_velocity = _dashDirection * Speed; // Retain some momentum but clamp to normal speed
		}

		public bool IsDashing() => _isDashing;

		protected override void OnCleanup()
		{
			_velocity = Vector2.Zero;
		}

		public void Move(Vector2 direction, double delta)
		{
			if (_body == null || _isDashing) return; // Don't allow normal movement control during dash

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
