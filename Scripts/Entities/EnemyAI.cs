using Godot;
using CyberSecurityGame.Components;

namespace CyberSecurityGame.Entities
{
	/// <summary>
	/// Clase base para comportamiento AI de enemigos
	/// Implementa Strategy Pattern para diferentes comportamientos
	/// </summary>
	public abstract partial class BaseEnemyAI : Node
	{
		[Export] public float DetectionRange = 500f;
		[Export] public float AttackRange = 100f;
		
		protected CharacterBody2D _enemy;
		protected Node2D _target;
		protected MovementComponent _movementComponent;
		protected AIState _currentState = AIState.Idle;

		public override void _Ready()
		{
			_enemy = GetParent() as CharacterBody2D;
			_movementComponent = _enemy?.GetNode<MovementComponent>("MovementComponent");
			FindTarget();
		}

		public override void _Process(double delta)
		{
			if (_target == null || !IsInstanceValid(_target))
			{
				FindTarget();
				return;
			}

			UpdateAI(delta);
		}

		protected abstract void UpdateAI(double delta);

		protected void FindTarget()
		{
			// Buscar el jugador en el árbol de escena
			_target = GetTree().Root.GetNode<Node2D>("Main/Player");
		}

		protected float GetDistanceToTarget()
		{
			if (_target == null || _enemy == null) return float.MaxValue;
			return _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);
		}

		protected Vector2 GetDirectionToTarget()
		{
			if (_target == null || _enemy == null) return Vector2.Zero;
			return (_target.GlobalPosition - _enemy.GlobalPosition).Normalized();
		}

		protected void MoveTowardsTarget(double delta)
		{
			if (_movementComponent != null && _target != null)
			{
				Vector2 direction = GetDirectionToTarget();
				_movementComponent.Move(direction, delta);
			}
		}

		protected enum AIState
		{
			Idle,
			Patrol,
			Chase,
			Attack,
			Retreat
		}
	}

	/// <summary>
	/// AI básica - Persigue al jugador directamente
	/// </summary>
	public partial class BasicEnemyAI : BaseEnemyAI
	{
		protected override void UpdateAI(double delta)
		{
			float distance = GetDistanceToTarget();

			if (distance < DetectionRange)
			{
				if (distance > AttackRange)
				{
					_currentState = AIState.Chase;
					MoveTowardsTarget(delta);
				}
				else
				{
					_currentState = AIState.Attack;
					// Lógica de ataque se manejaría aquí
				}
			}
			else
			{
				_currentState = AIState.Idle;
			}
		}
	}

	/// <summary>
	/// AI de Malware - Se mueve en zigzag
	/// </summary>
	public partial class MalwareAI : BaseEnemyAI
	{
		private float _zigzagTimer = 0f;
		private float _zigzagInterval = 0.5f;
		private int _zigzagDirection = 1;

		protected override void UpdateAI(double delta)
		{
			float distance = GetDistanceToTarget();

			if (distance < DetectionRange)
			{
				Vector2 direction = GetDirectionToTarget();
				
				// Añadir movimiento zigzag
				_zigzagTimer += (float)delta;
				if (_zigzagTimer >= _zigzagInterval)
				{
					_zigzagDirection *= -1;
					_zigzagTimer = 0f;
				}

				Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * _zigzagDirection;
				Vector2 finalDirection = (direction + perpendicular * 0.5f).Normalized();

				if (_movementComponent != null)
				{
					_movementComponent.Move(finalDirection, delta);
				}
			}
		}
	}

	/// <summary>
	/// AI de Phishing - Se acerca lentamente y luego ataca rápido
	/// </summary>
	public partial class PhishingAI : BaseEnemyAI
	{
		private bool _isLurking = true;
		private float _normalSpeed;
		private float _attackSpeedMultiplier = 2.5f;

		public override void _Ready()
		{
			base._Ready();
			if (_movementComponent != null)
			{
				_normalSpeed = _movementComponent.Speed;
			}
		}

		protected override void UpdateAI(double delta)
		{
			float distance = GetDistanceToTarget();

			if (distance < DetectionRange * 0.5f && _isLurking)
			{
				// Cambia a modo ataque
				_isLurking = false;
				if (_movementComponent != null)
				{
					_movementComponent.Speed = _normalSpeed * _attackSpeedMultiplier;
				}
				GD.Print("⚠️ Phishing activado!");
			}

			if (distance < DetectionRange)
			{
				MoveTowardsTarget(delta);
			}
		}
	}

	/// <summary>
	/// AI de DDoS - Ataca en grupo, se mueve en formación
	/// </summary>
	public partial class DDoSAI : BaseEnemyAI
	{
		[Export] public float FormationOffset = 50f;
		private Vector2 _formationPosition;

		protected override void UpdateAI(double delta)
		{
			float distance = GetDistanceToTarget();

			if (distance < DetectionRange)
			{
				// Calcular posición de formación
				Vector2 baseDirection = GetDirectionToTarget();
				Vector2 perpendicular = new Vector2(-baseDirection.Y, baseDirection.X);
				
				// Offset basado en el ID del enemigo (simulado con posición inicial)
				float offset = (_enemy.GlobalPosition.X % 100) - 50;
				_formationPosition = _target.GlobalPosition + perpendicular * offset;

				Vector2 toFormation = (_formationPosition - _enemy.GlobalPosition).Normalized();
				
				if (_movementComponent != null)
				{
					_movementComponent.Move(toFormation, delta);
				}
			}
		}
	}

	/// <summary>
	/// AI de SQL Injection - Se mueve hacia puntos débiles (random)
	/// </summary>
	public partial class SQLInjectionAI : BaseEnemyAI
	{
		private Vector2 _targetPoint;
		private float _retargetTimer = 0f;
		private float _retargetInterval = 2f;

		protected override void UpdateAI(double delta)
		{
			_retargetTimer += (float)delta;

			if (_retargetTimer >= _retargetInterval || _targetPoint == Vector2.Zero)
			{
				// Encuentra un nuevo punto objetivo cerca del jugador
				Vector2 randomOffset = new Vector2(
					(float)GD.RandRange(-200, 200),
					(float)GD.RandRange(-200, 200)
				);
				_targetPoint = _target.GlobalPosition + randomOffset;
				_retargetTimer = 0f;
			}

			if (_enemy != null)
			{
				Vector2 direction = (_targetPoint - _enemy.GlobalPosition).Normalized();
				if (_movementComponent != null)
				{
					_movementComponent.Move(direction, delta);
				}
			}
		}
	}

	/// <summary>
	/// AI de Brute Force - Ataque directo y persistente
	/// </summary>
	public partial class BruteForceAI : BaseEnemyAI
	{
		private float _chargeTimer = 0f;
		private float _chargeDuration = 1f;
		private bool _isCharging = false;
		private Vector2 _chargeDirection;

		protected override void UpdateAI(double delta)
		{
			float distance = GetDistanceToTarget();

			if (!_isCharging && distance < AttackRange * 2)
			{
				// Inicia carga
				_isCharging = true;
				_chargeDirection = GetDirectionToTarget();
				if (_movementComponent != null)
				{
					_movementComponent.Speed *= 2f;
				}
				GD.Print("💥 Brute Force iniciando ataque!");
			}

			if (_isCharging)
			{
				_chargeTimer += (float)delta;
				if (_movementComponent != null)
				{
					_movementComponent.Move(_chargeDirection, delta);
				}

				if (_chargeTimer >= _chargeDuration)
				{
					_isCharging = false;
					_chargeTimer = 0f;
					if (_movementComponent != null)
					{
						_movementComponent.Speed /= 2f;
					}
				}
			}
			else if (distance < DetectionRange)
			{
				MoveTowardsTarget(delta);
			}
		}
	}

	/// <summary>
	/// AI de Ransomware - Boss enemigo, múltiples fases
	/// </summary>
	public partial class RansomwareAI : BaseEnemyAI
	{
		private int _phase = 1;
		private float _phaseChangeHealth = 0.5f;

		protected override void UpdateAI(double delta)
		{
			// Verificar cambio de fase
			var healthComp = _enemy?.GetNode<HealthComponent>("HealthComponent");
			if (healthComp != null)
			{
				float healthPercent = healthComp.GetHealthPercentage();
				if (healthPercent < _phaseChangeHealth && _phase == 1)
				{
					_phase = 2;
					EnterPhaseTwo();
				}
			}

			float distance = GetDistanceToTarget();

			if (_phase == 1)
			{
				// Fase 1: Movimiento circular
				if (distance < DetectionRange)
				{
					Vector2 toTarget = GetDirectionToTarget();
					Vector2 tangent = new Vector2(-toTarget.Y, toTarget.X);
					
					if (_movementComponent != null)
					{
						_movementComponent.Move(tangent, delta);
					}
				}
			}
			else
			{
				// Fase 2: Ataque agresivo
				if (distance < DetectionRange)
				{
					MoveTowardsTarget(delta);
				}
			}
		}

		private void EnterPhaseTwo()
		{
			GD.Print("🔴 Ransomware entrando en Fase 2!");
			if (_movementComponent != null)
			{
				_movementComponent.Speed *= 1.5f;
			}
		}
	}
}
