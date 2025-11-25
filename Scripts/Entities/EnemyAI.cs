using Godot;
using CyberSecurityGame.Components;
using System.Collections.Generic;

namespace CyberSecurityGame.Entities
{
	/// <summary>
	/// Sistema de AI avanzado para enemigos
	/// 
	/// CARACTERÍSTICAS:
	/// - SIEMPRE saben dónde está el jugador
	/// - Patrones de movimiento únicos por tipo
	/// - Capacidad de evadir proyectiles del jugador
	/// - Comportamientos de ataque inteligentes
	/// </summary>
	public abstract partial class BaseEnemyAI : Node
	{
		[Export] public float DetectionRange = 3000f;  // Detectan en toda la pantalla
		[Export] public float AttackRange = 120f;
		[Export] public float AggroTime = 2f;          // Tiempo para volverse agresivo
		[Export] public float EvasionRange = 80f;      // Rango para detectar proyectiles
		[Export] public float EvasionStrength = 0.6f;  // Qué tanto evaden (0-1)
		
		protected CharacterBody2D _enemy;
		protected Node2D _target;
		protected MovementComponent _movementComponent;
		protected AIState _currentState = AIState.Chase;
		protected float _timeAlive = 0f;
		protected float _baseSpeed;
		protected bool _isAggro = false;
		
		// Sistema de evasión
		protected Vector2 _lastPlayerPosition;
		protected Vector2 _playerVelocityEstimate;
		protected float _predictionTime = 0.5f;

		public override void _Ready()
		{
			_enemy = GetParent() as CharacterBody2D;
			_movementComponent = _enemy?.GetNode<MovementComponent>("MovementComponent");
			_baseSpeed = _movementComponent?.Speed ?? 150f;
			FindTarget();
		}

		public override void _Process(double delta)
		{
			if (_target == null || !IsInstanceValid(_target))
			{
				FindTarget();
				return;
			}

			// Estimar velocidad del jugador para predicción
			Vector2 currentPlayerPos = _target.GlobalPosition;
			_playerVelocityEstimate = (currentPlayerPos - _lastPlayerPosition) / (float)delta;
			_lastPlayerPosition = currentPlayerPos;

			// Aumentar agresividad con el tiempo
			_timeAlive += (float)delta;
			if (_timeAlive > AggroTime && !_isAggro)
			{
				BecomeAggressive();
			}

			UpdateAI(delta);
		}

		protected virtual void BecomeAggressive()
		{
			_isAggro = true;
			if (_movementComponent != null)
			{
				_movementComponent.Speed = _baseSpeed * 1.4f;
			}
		}

		protected abstract void UpdateAI(double delta);

		protected void FindTarget()
		{
			_target = GetTree().Root.GetNodeOrNull<Node2D>("Main/Player");
			if (_target != null)
			{
				_lastPlayerPosition = _target.GlobalPosition;
			}
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

		/// <summary>
		/// Predice dónde estará el jugador en el futuro
		/// </summary>
		protected Vector2 GetPredictedTargetPosition()
		{
			if (_target == null) return Vector2.Zero;
			return _target.GlobalPosition + _playerVelocityEstimate * _predictionTime;
		}

		/// <summary>
		/// Dirección hacia la posición predicha del jugador
		/// </summary>
		protected Vector2 GetDirectionToPredictedTarget()
		{
			if (_enemy == null) return Vector2.Zero;
			Vector2 predicted = GetPredictedTargetPosition();
			return (predicted - _enemy.GlobalPosition).Normalized();
		}

		/// <summary>
		/// Calcula un vector de evasión basado en proyectiles cercanos
		/// </summary>
		protected Vector2 CalculateEvasionVector()
		{
			if (_enemy == null) return Vector2.Zero;

			Vector2 evasion = Vector2.Zero;
			var projectiles = GetTree().GetNodesInGroup("PlayerProjectiles");

			foreach (Node node in projectiles)
			{
				if (node is Node2D projectile && IsInstanceValid(projectile))
				{
					float distance = _enemy.GlobalPosition.DistanceTo(projectile.GlobalPosition);
					if (distance < EvasionRange)
					{
						// Vector alejándose del proyectil
						Vector2 awayFromProjectile = (_enemy.GlobalPosition - projectile.GlobalPosition).Normalized();
						// Más fuerte cuanto más cerca
						float strength = 1.0f - (distance / EvasionRange);
						evasion += awayFromProjectile * strength;
					}
				}
			}

			return evasion.Normalized() * EvasionStrength;
		}

		protected void MoveTowardsTarget(double delta)
		{
			if (_movementComponent != null && _target != null)
			{
				Vector2 direction = GetDirectionToTarget();
				Vector2 evasion = CalculateEvasionVector();
				Vector2 finalDirection = (direction + evasion).Normalized();
				_movementComponent.Move(finalDirection, delta);
			}
		}

		/// <summary>
		/// Movimiento con predicción - anticipa el movimiento del jugador
		/// </summary>
		protected void MoveTowardsPredictedTarget(double delta)
		{
			if (_movementComponent != null && _target != null)
			{
				Vector2 direction = GetDirectionToPredictedTarget();
				Vector2 evasion = CalculateEvasionVector();
				Vector2 finalDirection = (direction + evasion).Normalized();
				_movementComponent.Move(finalDirection, delta);
			}
		}

		protected enum AIState
		{
			Idle,
			Patrol,
			Chase,
			Attack,
			Retreat,
			Evade
		}
	}

	/// <summary>
	/// AI básica mejorada - Persecución directa con evasión
	/// </summary>
	public partial class BasicEnemyAI : BaseEnemyAI
	{
		protected override void UpdateAI(double delta)
		{
			if (_target == null || _enemy == null) return;
			
			float distance = GetDistanceToTarget();
			
			if (distance > AttackRange)
			{
				_currentState = AIState.Chase;
				MoveTowardsTarget(delta);
			}
			else
			{
				_currentState = AIState.Attack;
			}
		}
	}

	/// <summary>
	/// AI de Malware - Zigzag impredecible + evasión
	/// Patrón: Se mueve en zigzag para ser difícil de apuntar
	/// </summary>
	public partial class MalwareAI : BaseEnemyAI
	{
		private float _zigzagTimer = 0f;
		private float _zigzagInterval = 0.4f;
		private int _zigzagDirection = 1;
		private float _zigzagAmplitude = 0.7f;

		protected override void UpdateAI(double delta)
		{
			if (_target == null || _enemy == null) return;
			
			Vector2 direction = GetDirectionToTarget();
			if (direction == Vector2.Zero) return;
			
			// Zigzag más pronunciado cuando está agresivo
			_zigzagTimer += (float)delta;
			if (_zigzagTimer >= _zigzagInterval)
			{
				_zigzagDirection *= -1;
				_zigzagTimer = 0f;
				// Variar intervalo para ser impredecible
				_zigzagInterval = (float)GD.RandRange(0.25, 0.6);
			}

			Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * _zigzagDirection;
			float amplitude = _isAggro ? _zigzagAmplitude * 1.3f : _zigzagAmplitude;
			Vector2 zigzagMovement = direction + perpendicular * amplitude;
			
			// Añadir evasión
			Vector2 evasion = CalculateEvasionVector();
			Vector2 finalDirection = (zigzagMovement + evasion).Normalized();

			if (_movementComponent != null)
			{
				_movementComponent.Move(finalDirection, delta);
			}
		}
	}

	/// <summary>
	/// AI de Phishing - Acecho sigiloso + ataque sorpresa
	/// Patrón: Se acerca lento, luego embosca rápidamente
	/// </summary>
	public partial class PhishingAI : BaseEnemyAI
	{
		private bool _isLurking = true;
		private float _normalSpeed;
		private float _attackSpeedMultiplier = 3.0f;
		private float _lurkDistance = 350f;
		private float _attackCooldown = 0f;
		private Vector2 _attackDirection;

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
			if (_target == null || _enemy == null) return;
			
			float distance = GetDistanceToTarget();
			_attackCooldown -= (float)delta;

			if (_isLurking)
			{
				// Modo acecho: moverse lateralmente, manteniendo distancia
				if (distance < _lurkDistance && _attackCooldown <= 0)
				{
					// ¡ATAQUE SORPRESA!
					_isLurking = false;
					_attackDirection = GetDirectionToPredictedTarget(); // Predice posición
					if (_movementComponent != null)
					{
						_movementComponent.Speed = _normalSpeed * _attackSpeedMultiplier;
					}
					GD.Print("⚠️ ¡Phishing ataca!");
				}
				else
				{
					// Orbitar alrededor del jugador
					Vector2 toTarget = GetDirectionToTarget();
					Vector2 orbit = new Vector2(-toTarget.Y, toTarget.X);
					
					// Acercarse gradualmente
					Vector2 approach = toTarget * 0.3f;
					Vector2 finalDir = (orbit + approach + CalculateEvasionVector()).Normalized();
					
					if (_movementComponent != null)
					{
						_movementComponent.Move(finalDir, delta);
					}
				}
			}
			else
			{
				// Modo ataque: ir directo al punto de emboscada
				if (_movementComponent != null)
				{
					_movementComponent.Move(_attackDirection, delta);
				}

				// Volver a modo acecho después de pasar
				if (distance < 50f || distance > _lurkDistance * 1.5f)
				{
					_isLurking = true;
					_attackCooldown = 2.0f;
					if (_movementComponent != null)
					{
						_movementComponent.Speed = _normalSpeed;
					}
				}
			}
		}
	}

	/// <summary>
	/// AI de DDoS - Ataque en enjambre coordinado
	/// Patrón: Se mueven en grupo, rodean al jugador
	/// </summary>
	public partial class DDoSAI : BaseEnemyAI
	{
		[Export] public float FormationOffset = 60f;
		private float _orbitAngle;
		private float _orbitSpeed = 1.5f;
		private bool _isClosing = false;
		private float _closeTimer = 0f;

		public override void _Ready()
		{
			base._Ready();
			// Ángulo inicial aleatorio para distribución
			_orbitAngle = (float)GD.RandRange(0, Mathf.Tau);
		}

		protected override void UpdateAI(double delta)
		{
			if (_target == null || _enemy == null) return;
			
			float distance = GetDistanceToTarget();
			_closeTimer += (float)delta;

			// Cada 3-5 segundos, cerrar el cerco
			if (_closeTimer > 4f)
			{
				_isClosing = !_isClosing;
				_closeTimer = 0f;
			}

			// Orbitar alrededor del jugador
			_orbitAngle += _orbitSpeed * (float)delta;
			
			float targetRadius = _isClosing ? 100f : 200f;
			Vector2 orbitPosition = _target.GlobalPosition + new Vector2(
				Mathf.Cos(_orbitAngle) * targetRadius,
				Mathf.Sin(_orbitAngle) * targetRadius
			);

			Vector2 direction = (orbitPosition - _enemy.GlobalPosition).Normalized();
			Vector2 evasion = CalculateEvasionVector() * 0.5f; // Menos evasión, más coordinación
			Vector2 finalDir = (direction + evasion).Normalized();
			
			if (_movementComponent != null)
			{
				_movementComponent.Move(finalDir, delta);
			}
		}
	}

	/// <summary>
	/// AI de SQL Injection - Movimiento errático + teleportación
	/// Patrón: Aparece en puntos inesperados cerca del jugador
	/// </summary>
	public partial class SQLInjectionAI : BaseEnemyAI
	{
		private Vector2 _targetPoint;
		private float _retargetTimer = 0f;
		private float _retargetInterval = 1.5f;
		private float _teleportChance = 0.15f;
		private bool _isTeleporting = false;
		private float _teleportTimer = 0f;

		protected override void UpdateAI(double delta)
		{
			if (_target == null || _enemy == null) return;
			
			_retargetTimer += (float)delta;

			// Teleport visual (fade out/in)
			if (_isTeleporting)
			{
				_teleportTimer += (float)delta;
				if (_teleportTimer > 0.3f)
				{
					// Aparecer en nueva posición
					Vector2 offset = new Vector2(
						(float)GD.RandRange(-150, 150),
						(float)GD.RandRange(-150, 150)
					);
					_enemy.GlobalPosition = _target.GlobalPosition + offset;
					_isTeleporting = false;
					_teleportTimer = 0f;
				}
				return;
			}

			// Chance de teleportarse
			if (_retargetTimer >= _retargetInterval)
			{
				if (GD.Randf() < _teleportChance && _isAggro)
				{
					_isTeleporting = true;
					_teleportTimer = 0f;
					return;
				}

				// Nuevo punto objetivo impredecible
				float angle = (float)GD.RandRange(0, Mathf.Tau);
				float dist = (float)GD.RandRange(50, 200);
				_targetPoint = _target.GlobalPosition + new Vector2(
					Mathf.Cos(angle) * dist,
					Mathf.Sin(angle) * dist
				);
				_retargetTimer = 0f;
				_retargetInterval = (float)GD.RandRange(0.8, 2.0);
			}

			Vector2 direction = (_targetPoint - _enemy.GlobalPosition).Normalized();
			Vector2 evasion = CalculateEvasionVector();
			Vector2 finalDir = (direction + evasion).Normalized();
			
			if (_movementComponent != null)
			{
				_movementComponent.Move(finalDir, delta);
			}
		}
	}

	/// <summary>
	/// AI de Brute Force - Embestida imparable
	/// Patrón: Carga directa, alta velocidad, difícil de detener
	/// </summary>
	public partial class BruteForceAI : BaseEnemyAI
	{
		private float _chargeTimer = 0f;
		private float _chargeDuration = 1.2f;
		private float _chargeCooldown = 0f;
		private bool _isCharging = false;
		private Vector2 _chargeDirection;
		private float _chargeSpeedMultiplier = 2.5f;

		protected override void UpdateAI(double delta)
		{
			if (_target == null || _enemy == null) return;
			
			float distance = GetDistanceToTarget();
			_chargeCooldown -= (float)delta;

			if (_isCharging)
			{
				// Durante la carga: NO evade, va directo
				_chargeTimer += (float)delta;
				if (_movementComponent != null)
				{
					_movementComponent.Move(_chargeDirection, delta);
				}

				// Terminar carga
				if (_chargeTimer >= _chargeDuration)
				{
					_isCharging = false;
					_chargeTimer = 0f;
					_chargeCooldown = 1.5f;
					if (_movementComponent != null)
					{
						_movementComponent.Speed = _baseSpeed;
					}
				}
			}
			else
			{
				// Preparar carga
				if (distance < AttackRange * 3 && _chargeCooldown <= 0)
				{
					_isCharging = true;
					// Apuntar a posición predicha
					_chargeDirection = GetDirectionToPredictedTarget();
					if (_movementComponent != null)
					{
						_movementComponent.Speed = _baseSpeed * _chargeSpeedMultiplier;
					}
					GD.Print("💥 ¡Brute Force carga!");
				}
				else
				{
					// Acercarse normalmente con evasión
					MoveTowardsTarget(delta);
				}
			}
		}

		protected override void BecomeAggressive()
		{
			base.BecomeAggressive();
			_chargeCooldown = 0f; // Puede cargar inmediatamente
			_chargeSpeedMultiplier = 3.0f;
		}
	}

	/// <summary>
	/// AI de Ransomware - Boss con múltiples fases
	/// Patrón: Fase 1 = Circular defensivo, Fase 2 = Agresivo con summons
	/// </summary>
	public partial class RansomwareAI : BaseEnemyAI
	{
		private int _phase = 1;
		private float _phaseChangeHealth = 0.5f;
		private float _circleAngle = 0f;
		private float _circleSpeed = 1.0f;
		private float _circleRadius = 250f;
		private float _attackTimer = 0f;

		protected override void UpdateAI(double delta)
		{
			if (_target == null || _enemy == null) return;
			
			// Verificar cambio de fase
			var healthComp = _enemy.GetNodeOrNull<HealthComponent>("HealthComponent");
			if (healthComp != null)
			{
				float healthPercent = healthComp.GetHealthPercentage();
				if (healthPercent < _phaseChangeHealth && _phase == 1)
				{
					_phase = 2;
					EnterPhaseTwo();
				}
			}

			_attackTimer += (float)delta;

			if (_phase == 1)
			{
				// Fase 1: Movimiento circular defensivo
				_circleAngle += _circleSpeed * (float)delta;
				
				Vector2 circlePos = _target.GlobalPosition + new Vector2(
					Mathf.Cos(_circleAngle) * _circleRadius,
					Mathf.Sin(_circleAngle) * _circleRadius
				);

				Vector2 direction = (circlePos - _enemy.GlobalPosition).Normalized();
				Vector2 evasion = CalculateEvasionVector();
				Vector2 finalDir = (direction + evasion).Normalized();
				
				if (_movementComponent != null)
				{
					_movementComponent.Move(finalDir, delta);
				}

				// Reducir radio gradualmente
				_circleRadius = Mathf.Max(100f, _circleRadius - 5f * (float)delta);
			}
			else
			{
				// Fase 2: Persecución agresiva con predicción
				if (_attackTimer > 0.5f)
				{
					MoveTowardsPredictedTarget(delta);
				}
				else
				{
					// Breve pausa para "telegrafiar" ataques
					var evasion = CalculateEvasionVector();
					if (_movementComponent != null && evasion != Vector2.Zero)
					{
						_movementComponent.Move(evasion, delta);
					}
				}

				if (_attackTimer > 3f)
				{
					_attackTimer = 0f;
				}
			}
		}

		private void EnterPhaseTwo()
		{
			GD.Print("🔴 ¡RANSOMWARE FASE 2!");
			if (_movementComponent != null)
			{
				_movementComponent.Speed *= 1.8f;
			}
			_baseSpeed = _movementComponent?.Speed ?? _baseSpeed;
		}
	}
}
