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

		// Configuración -----------> VELOCIDAD AUMENTADA
		[Export] public float MaxHealth = 100f;
		[Export] public float Speed = 450f;           // MÁS rápido - arcade fluido
		[Export] public float AfterburnerSpeed = 700f; // Heroico
		[Export] public float RotationSmoothing = 0.18f; // MUY responsivo
		
		// Sistema de respawn
		private bool _isInvincible = false;
		private float _invincibilityTimer = 0f;
		private const float INVINCIBILITY_DURATION = 3.5f; // Aumentado de 2.5s para dar más margen
		private Vector2 _spawnPosition;
		private bool _isRespawning = false;

		public override void _Ready()
		{
			InitializeComponents();
			SetupCollision();
			
			// Guardar posición de spawn
			_spawnPosition = GlobalPosition;
			
			// Asegurar que estamos en el grupo Player
			if (!IsInGroup("Player"))
				AddToGroup("Player");
				
			// BALANCE ARCADE: Jugador pequeño y ágil
			// Hitbox más pequeño = más fácil esquivar balas (bullet hell)
			Scale = new Vector2(0.6f, 0.6f);
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

			// Equipar arma adaptativa
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
			var existingCollision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (existingCollision == null)
			{
				// Configurar colisión - HITBOX PEQUEÑO estilo bullet hell
				var collisionShape = new CollisionShape2D();
				var shape = new CircleShape2D();
				shape.Radius = 5f; // Muy pequeño para evadir balas
				collisionShape.Shape = shape;
				collisionShape.Name = "CollisionShape2D";
				AddChild(collisionShape);
			}
			else
			{
				// Actualizar hitbox existente
				if (existingCollision.Shape is CircleShape2D circle)
				{
					circle.Radius = 5f;
				}
			}
		}
		
		public override void _Process(double delta){
			// No procesar si está respawneando
			if (_isRespawning) return;
			
			HandleInput(delta);
			UpdateComponents(delta);
			UpdateInvincibility((float)delta);
			
			// Apuntar con el mouse
			LookAt(GetGlobalMousePosition());
			
			// BULLET HELL: Limitar posición al área de juego
			ClampToPlayArea();
		}
		
		/// <summary>
		/// Limita la posición del jugador al área de juego visible
		/// </summary>
		private void ClampToPlayArea()
		{
			const float MARGIN = 20f;
			const float MIN_X = MARGIN;
			const float MAX_X = 1200f - MARGIN;
			const float MIN_Y = MARGIN;
			const float MAX_Y = 900f - MARGIN;
			
			GlobalPosition = new Vector2(
				Mathf.Clamp(GlobalPosition.X, MIN_X, MAX_X),
				Mathf.Clamp(GlobalPosition.Y, MIN_Y, MAX_Y)
			);
		}

		/// <summary>
		/// Obtiene la posición del morro/frente de la nave para spawnar proyectiles
		/// </summary>
		private Vector2 GetMuzzlePosition()
		{
			// El morro está siempre adelante relativo a la rotación actual
			return GlobalPosition + Vector2.Right.Rotated(Rotation) * 30f;
		}

private void HandleInput(double delta)
{
	// ═══════════════════════════════════════════════════════════════════
	// 🎮 TWIN STICK SHOOTER - 8 DIRECCIONES + MOUSE AIM
	// ═══════════════════════════════════════════════════════════════════
	// WASD  = Movimiento 8 direcciones
	// Mouse = Apuntar
	// Click = Disparar hacia el cursor
	// Space = PANIC BURST (Bomba defensiva)
	// Shift = TURBO
	// ═══════════════════════════════════════════════════════════════════
	
	// ─────────────────────────────────────────────────────────────
	// 🕹️ MOVIMIENTO 8 DIRECCIONES
	// ─────────────────────────────────────────────────────────────
	Vector2 inputDir = Vector2.Zero;
	
	if (Input.IsActionPressed("move_up"))    inputDir.Y -= 1;
	if (Input.IsActionPressed("move_down"))  inputDir.Y += 1;
	if (Input.IsActionPressed("move_left"))  inputDir.X -= 1;
	if (Input.IsActionPressed("move_right")) inputDir.X += 1;
	
	// ─────────────────────────────────────────────────────────────
	// ⚡ TURBO (Shift)
	// ─────────────────────────────────────────────────────────────
	bool isTurbo = Input.IsKeyPressed(Key.Shift);
	if (_movementComponent != null)
	{
		_movementComponent.Speed = isTurbo ? AfterburnerSpeed : Speed;
		if (isTurbo && inputDir != Vector2.Zero)
		{
			_cpuComponent?.AddLoad(0.2f * (float)delta * 60f);
		}
	}

	_movementComponent?.Move(inputDir, delta);

	// ─────────────────────────────────────────────────────────────
	// 💣 PANIC BURST (Space) - "Botón de Pánico"
	// Limpia balas cercanas y empuja enemigos
	// ─────────────────────────────────────────────────────────────
	if (Input.IsActionJustPressed("dash") || Input.IsKeyPressed(Key.Space))
	{
		ActivatePanicBurst();
		return;
	}

	// ═══════════════════════════════════════════════════════════
	// 🔥 DISPARO DIRIGIDO - Click = ráfaga hacia el mouse
	// ═══════════════════════════════════════════════════════════
	if (Input.IsMouseButtonPressed(MouseButton.Left))
	{
		FireAtMouse();
	}
}

	private void FireAtMouse()
	{
		if (_weaponComponent == null) return;
		
		Vector2 muzzlePos = GetMuzzlePosition();
		Vector2 baseDir = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		
		// Disparo principal con pequeña variación (spread)
		float spread = (float)GD.RandRange(-0.05, 0.05);
		Vector2 fireDir = baseDir.Rotated(spread);
		
		// Intentar disparar respetando el FireRate base
		if (_weaponComponent.TryFireFrom(muzzlePos, fireDir))
		{
			// 🎲 MECÁNICA ARCADE: Chance de "Multishot" (30%)
			if (GD.Randf() < 0.3f)
			{
				float extraSpread = (float)GD.RandRange(-0.15, 0.15);
				_weaponComponent.ForceFire(muzzlePos, baseDir.Rotated(extraSpread));
			}
		}
	}

	private void ActivatePanicBurst()
	{
		// Costo alto de CPU o cooldown
		if (_cpuComponent != null && _cpuComponent.GetLoadPercentage() > 0.8f)
		{
			// Feedback visual de fallo (sonido error)
			return;
		}
		
		_cpuComponent?.AddLoad(25f); // Costoso
		
		// Efecto visual de onda expansiva
		var shockwave = new CpuParticles2D();
		shockwave.GlobalPosition = GlobalPosition;
		shockwave.Emitting = true;
		shockwave.Amount = 36;
		shockwave.OneShot = true;
		shockwave.Explosiveness = 1.0f;
		shockwave.Spread = 180f;
		shockwave.Gravity = Vector2.Zero;
		shockwave.InitialVelocityMin = 300f;
		shockwave.InitialVelocityMax = 400f;
		shockwave.ScaleAmountMin = 4f;
		shockwave.ScaleAmountMax = 8f;
		shockwave.Color = new Color(0, 1, 1, 0.8f); // Cyan brillante
		GetTree().Root.AddChild(shockwave);
		
		// Eliminar proyectiles enemigos cercanos
		var area = new Area2D();
		var shape = new CollisionShape2D();
		shape.Shape = new CircleShape2D { Radius = 250f };
		area.AddChild(shape);
		area.GlobalPosition = GlobalPosition;
		GetTree().Root.AddChild(area);
		
		// Esperar un frame para detectar colisiones (hack rápido)
		// Mejor: Usar PhysicsDirectSpaceState en _PhysicsProcess, pero esto es visual/lógico simple
		// Iterar sobre balas activas (si tenemos un manager) o usar grupos
		foreach (var node in GetTree().GetNodesInGroup("EnemyProjectiles"))
		{
			if (node is Node2D proj && proj.GlobalPosition.DistanceTo(GlobalPosition) < 250f)
			{
				proj.QueueFree(); // Destruir bala
			}
		}
		
		// Empujar enemigos
		foreach (var node in GetTree().GetNodesInGroup("Enemy"))
		{
			if (node is Node2D enemy && enemy.GlobalPosition.DistanceTo(GlobalPosition) < 300f)
			{
				// Empuje simple
				var pushDir = (enemy.GlobalPosition - GlobalPosition).Normalized();
				enemy.GlobalPosition += pushDir * 100f;
				
				// Daño pequeño
				if (enemy.HasMethod("TakeDamage"))
				{
					// Reflection call o buscar componente
					var health = enemy.GetNodeOrNull<HealthComponent>("HealthComponent");
					health?.TakeDamage(20f, Core.Interfaces.DamageType.Physical);
				}
			}
		}
		
		// Limpiar area temporal
		area.QueueFree();
		
		GD.Print("💥 PANIC BURST ACTIVADO!");
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
			// Inmune durante invincibilidad
			if (_isInvincible || _isRespawning) return;
			
			// El escudo absorbe primero
			if (_shieldComponent != null && _shieldComponent.IsActive())
				amount = _shieldComponent.AbsorbDamage(amount);

			if (amount > 0)
			{
				_healthComponent?.TakeDamage(amount, damageType);
				
				// Flash de daño
				FlashDamage();
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
		public bool IsInvincible() => _isInvincible;
		
		// ═══════════════════════════════════════════════════════════════════
		// SISTEMA DE RESPAWN E INVINCIBILIDAD
		// ═══════════════════════════════════════════════════════════════════
		
		private void UpdateInvincibility(float delta)
		{
			if (!_isInvincible) return;
			
			_invincibilityTimer -= delta;
			
			// Efecto de parpadeo
			float flash = Mathf.Sin(_invincibilityTimer * 15f);
			Modulate = flash > 0 ? new Color(1, 1, 1, 0.5f) : Colors.White;
			
			if (_invincibilityTimer <= 0)
			{
				_isInvincible = false;
				Modulate = Colors.White;
				GD.Print("[Player] Invincibilidad terminada");
			}
		}
		
		private void FlashDamage()
		{
			// Flash rojo de daño
			Modulate = new Color(1, 0.3f, 0.3f, 1);
			var tween = CreateTween();
			tween.TweenProperty(this, "modulate", Colors.White, 0.15f);
		}
		
		/// <summary>
		/// Respawnea al jugador con invincibilidad temporal
		/// </summary>
		public void Respawn()
		{
			if (_isRespawning) return;
			_isRespawning = true;
			
			GD.Print("[Player] Iniciando respawn...");
			
			// Efecto de muerte y respawn
			PlayDeathEffect();
			
			// Timer para respawn
			var timer = GetTree().CreateTimer(1.0f);
			timer.Timeout += CompleteRespawn;
		}
		
		private void CompleteRespawn()
		{
			// BALANCE: Respawnear en posición segura (centro-abajo de pantalla)
			// No en la posición original que puede estar llena de balas
			var viewport = GetViewport().GetVisibleRect().Size;
			Vector2 safePosition = new Vector2(viewport.X / 2, viewport.Y * 0.8f);
			GlobalPosition = safePosition;
			
			// Restaurar salud
			_healthComponent?.ResetForRespawn();
			
			// Activar invincibilidad
			_isInvincible = true;
			_invincibilityTimer = INVINCIBILITY_DURATION;
			_isRespawning = false;
			
			// Efecto de aparición
			Visible = true;
			Scale = Vector2.Zero;
			var tween = CreateTween();
			tween.SetTrans(Tween.TransitionType.Elastic);
			tween.TweenProperty(this, "scale", new Vector2(0.5f, 0.5f), 0.5f);
			
			GD.Print($"[Player] ¡Respawn completado! Invincible por {INVINCIBILITY_DURATION}s");
			
			// Notificar UI
			GameEventBus.Instance.EmitPlayerHealthChanged(MaxHealth);
		}
		
		private void PlayDeathEffect()
		{
			// Ocultar temporalmente
			Visible = false;
			
			// Crear explosión visual
			var particles = new CpuParticles2D();
			particles.GlobalPosition = GlobalPosition;
			particles.Emitting = true;
			particles.Amount = 50;
			particles.Lifetime = 0.8f;
			particles.OneShot = true;
			particles.Explosiveness = 1.0f;
			particles.Spread = 180f; // Spread en Godot 4
			particles.InitialVelocityMin = 100f;
			particles.InitialVelocityMax = 300f;
			particles.ScaleAmountMin = 3f;
			particles.ScaleAmountMax = 8f;
			particles.Color = new Color("#00ffff");
			particles.Finished += () => particles.QueueFree();
			
			GetTree().Root.AddChild(particles);
			
			// Screen shake (si existe el sistema)
			EmitSignal(SignalName.DeathOccurred);
			
			GD.Print("[Player] 💥 Explosión de muerte");
		}
		
		// Signal para notificar muerte
		[Signal]
		public delegate void DeathOccurredEventHandler();

	// ═══════════════════════════════════════════════════════════
	// E = INTERACCIÓN (DataNode, LoreTerminal) - Manejado por esos scripts
	// No hacer nada aquí, ellos leen Input.IsKeyPressed(Key.E)
	// ═══════════════════════════════════════════════════════════
}
}