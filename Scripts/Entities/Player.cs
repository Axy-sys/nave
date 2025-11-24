using Godot;
using CyberSecurityGame.Components;
using CyberSecurityGame.Weapons;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Entities
{
	/// <summary>
	/// Entidad del jugador usando composición de componentes
	/// Principio de Composition over Inheritance
	/// </summary>
	public partial class Player : CharacterBody2D
	{
		// Componentes (Composition Pattern)
		private HealthComponent _healthComponent;
		private MovementComponent _movementComponent;
		private WeaponComponent _weaponComponent;
		private ShieldComponent _shieldComponent;
		private CpuComponent _cpuComponent;

		// Configuración
		[Export] public float MaxHealth = 100f;
		[Export] public float Speed = 400f;

		public override void _Ready()
		{
			InitializeComponents();
			SetupCollision();
		}

		private void InitializeComponents()
		{
			// Buscar componentes existentes en la escena o crearlos
			_healthComponent = GetNodeOrNull<HealthComponent>("HealthComponent");
			if (_healthComponent == null)
			{
				_healthComponent = new HealthComponent
				{
					Name = "HealthComponent",
					MaxHealth = MaxHealth,
					IsPlayer = true
				};
				AddChild(_healthComponent);
			}
			else
			{
				_healthComponent.MaxHealth = MaxHealth;
				_healthComponent.IsPlayer = true;
			}
			_healthComponent.Initialize(this);

			_movementComponent = GetNodeOrNull<MovementComponent>("MovementComponent");
			if (_movementComponent == null)
			{
				_movementComponent = new MovementComponent
				{
					Name = "MovementComponent",
					Speed = Speed,
					UseAcceleration = true
				};
				AddChild(_movementComponent);
			}
			else
			{
				_movementComponent.Speed = Speed;
				_movementComponent.UseAcceleration = true;
			}
			_movementComponent.Initialize(this);

			_weaponComponent = GetNodeOrNull<WeaponComponent>("WeaponComponent");
			if (_weaponComponent == null)
			{
				_weaponComponent = new WeaponComponent
				{
					Name = "WeaponComponent",
					FireRate = 0.15f
				};
				AddChild(_weaponComponent);
			}
			else
			{
				_weaponComponent.FireRate = 0.15f;
			}
			_weaponComponent.Initialize(this);
			
			// Equipar arma adaptativa por defecto
			_weaponComponent.SetWeapon(new AdaptiveWeapon());

			_shieldComponent = GetNodeOrNull<ShieldComponent>("ShieldComponent");
			if (_shieldComponent == null)
			{
				_shieldComponent = new ShieldComponent
				{
					Name = "ShieldComponent",
					MaxShieldStrength = 50f
				};
				AddChild(_shieldComponent);
			}
			else
			{
				_shieldComponent.MaxShieldStrength = 50f;
			}
			_shieldComponent.Initialize(this);

			_cpuComponent = GetNodeOrNull<CpuComponent>("CpuComponent");
			if (_cpuComponent == null)
			{
				_cpuComponent = new CpuComponent
				{
					Name = "CpuComponent",
					MaxLoad = 100f
				};
				AddChild(_cpuComponent);
			}
			_cpuComponent.Initialize(this);

			GD.Print("✓ Player inicializado con componentes");
		}

		private void SetupCollision()
		{
			// Verificar si ya existe CollisionShape2D en la escena
			var existingCollision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (existingCollision == null)
			{
				// Configurar colisión solo si no existe
				var collisionShape = new CollisionShape2D();
				var shape = new CircleShape2D();
				shape.Radius = 20f;
				collisionShape.Shape = shape;
				collisionShape.Name = "CollisionShape2D";
				AddChild(collisionShape);
			}
		}
		
		public override void _Process(double delta){
			HandleInput(delta);
			UpdateComponents(delta);
			
			RotateTowardsMouse();
			}
		private void RotateTowardsMouse(){
			Vector2 dir = GetGlobalMousePosition() - GlobalPosition;
			Rotation = dir.Angle() + Mathf.Pi / 2; // Corrige 90°
		}

		private Vector2 GetFireDirection()
		{
			// Apuntar hacia el mouse para mayor intuición
			return (GetGlobalMousePosition() - GlobalPosition).Normalized();
		}
private void HandleInput(double delta)
{
	Vector2 inputDirection = Vector2.Zero;

	if (Input.IsActionPressed("move_up"))
		inputDirection.Y -= 1;
	if (Input.IsActionPressed("move_down"))
		inputDirection.Y += 1;
	if (Input.IsActionPressed("move_left"))
		inputDirection.X -= 1;
	if (Input.IsActionPressed("move_right"))
		inputDirection.X += 1;

	inputDirection = inputDirection.Normalized();

	// ► FORWARD REAL según rotación (la nave apunta hacia ARRIBA del sprite)
	Vector2 forward = Vector2.Up.Rotated(Rotation);

	// ► RIGHT perpendicular al forward
	Vector2 right = forward.Rotated(Mathf.Pi / 2);

	// Movimiento WASD correcto
	Vector2 moveDir =
		forward * (-inputDirection.Y) +   // Y negativo porque W es -1 pero forward es UP
		right * inputDirection.X;

	_movementComponent?.Move(moveDir, delta);

	// Dash
	if (Input.IsKeyPressed(Key.Space))
	{
		if (_movementComponent != null && _movementComponent.TryDash(moveDir))
		{
			Modulate = new Color(0, 1, 1, 0.5f);
			GetTree().CreateTimer(0.2f).Timeout += () => Modulate = Colors.White;
		}
	}

	// Parry
	if (Input.IsKeyPressed(Key.Shift))
	{
		if (_shieldComponent != null && _shieldComponent.TriggerParry())
		{
			Modulate = new Color(1, 0.8f, 0, 1f);
			GetTree().CreateTimer(0.2f).Timeout += () => Modulate = Colors.White;
		}
	}

	// Disparo
	if (Input.IsActionPressed("fire"))
	{
		Vector2 fireDirection = GetFireDirection();
		_weaponComponent?.TryFire(fireDirection);
	}
}

		private void UpdateComponents(double delta)
		{
			_healthComponent?.UpdateComponent(delta);
			_movementComponent?.UpdateComponent(delta);
			_weaponComponent?.UpdateComponent(delta);
			_shieldComponent?.UpdateComponent(delta);
		}

		public void TakeDamage(float amount, Core.Interfaces.DamageType damageType)
		{
			// El escudo absorbe primero
			if (_shieldComponent != null && _shieldComponent.IsActive())
			{
				amount = _shieldComponent.AbsorbDamage(amount);
			}

			// El daño restante va a la salud
			if (amount > 0)
			{
				_healthComponent?.TakeDamage(amount, damageType);
			}
		}

		public void ActivateShield(ShieldType type, float strength)
		{
			_shieldComponent?.ActivateShield(type, strength);
		}

		public void SwitchWeapon(Core.Interfaces.IWeapon newWeapon)
		{
			_weaponComponent?.SetWeapon(newWeapon);
		}

		public void Heal(float amount)
		{
			_healthComponent?.Heal(amount);
		}

		public float GetHealth() => _healthComponent?.GetCurrentHealth() ?? 0f;
		public bool IsAlive() => _healthComponent?.IsAlive() ?? false;
	}
}
