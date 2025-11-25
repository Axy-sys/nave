using Godot;
using CyberSecurityGame.Components;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Entities
{
	/// <summary>
	/// Factory para crear diferentes tipos de enemigos (Factory Pattern)
	/// Cada tipo tiene su propio sprite único y características visuales
	/// </summary>
	public static class EnemyFactory
	{
		// Colores temáticos web
		private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
		private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
		private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
		private static readonly Color ALERT_RED = new Color("#ff5555");
		private static readonly Color CYBER_BLUE = new Color("#00d4ff");
		private static readonly Color DARK_RED = new Color("#8b0000");

		public static void Initialize()
		{
			// Pre-cargar texturas si es necesario
		}

		/// <summary>
		/// Crea un enemigo según el tipo especificado
		/// </summary>
		public static Node2D CreateEnemy(EnemyType type, Vector2 position)
		{
			var enemy = CreateEnemyInstance(type);
			if (enemy != null)
			{
				enemy.GlobalPosition = position;
				ConfigureEnemy(enemy, type);
				AddVisualEffects(enemy, type);
			}
			return enemy;
		}

		private static Node2D CreateEnemyInstance(EnemyType type)
		{
			var enemy = new CharacterBody2D();
			enemy.Name = type.ToString();
			enemy.CollisionLayer = 4; // Capa de enemigos
			enemy.CollisionMask = 3;  // Jugador y Proyectiles
			
			// ═══════════════════════════════════════════════════════════
			// SPRITE ÚNICO POR TIPO DE ENEMIGO
			// ═══════════════════════════════════════════════════════════
			var sprite = new Sprite2D();
			sprite.Name = "Sprite";
			sprite.Texture = GD.Load<Texture2D>(GetEnemySpritePath(type));
			sprite.Modulate = GetEnemyGlow(type);
			sprite.Scale = GetEnemyScale(type);
			enemy.AddChild(sprite);

			// ═══════════════════════════════════════════════════════════
			// LABEL DE IDENTIFICACIÓN (debug visual)
			// ═══════════════════════════════════════════════════════════
			var label = new Label();
			label.Name = "TypeLabel";
			label.Text = GetEnemyShortName(type);
			label.Position = new Vector2(-30, -50);
			label.AddThemeColorOverride("font_color", GetEnemyColor(type));
			label.AddThemeFontSizeOverride("font_size", 10);
			enemy.AddChild(label);

			// ═══════════════════════════════════════════════════════════
			// COLISIÓN
			// ═══════════════════════════════════════════════════════════
			var collision = new CollisionShape2D();
			var shape = new CircleShape2D();
			shape.Radius = GetCollisionRadius(type);
			collision.Shape = shape;
			enemy.AddChild(collision);

			AddComponents(enemy, type);
			
			return enemy;
		}

		private static string GetEnemySpritePath(EnemyType type)
		{
			// Cada tipo usa su sprite dedicado
			return type switch
			{
				EnemyType.Malware => "res://Assets/enemy_malware.svg",
				EnemyType.Phishing => "res://Assets/enemy_phishing.svg",
				EnemyType.DDoS => "res://Assets/enemy_ddos.svg",
				EnemyType.SQLInjection => "res://Assets/enemy_sql_injection.svg",
				EnemyType.BruteForce => "res://Assets/enemy_bruteforce.svg",
				EnemyType.Ransomware => "res://Assets/enemy_ransomware.svg",
				EnemyType.Trojan => "res://Assets/enemy_malware.svg", // Reusa malware con color diferente
				EnemyType.Worm => "res://Assets/enemy_bug.svg",
				_ => "res://Assets/enemy_bug.svg"
			};
		}

		private static string GetEnemyShortName(EnemyType type)
		{
			return type switch
			{
				EnemyType.Malware => "MAL",
				EnemyType.Phishing => "PHSH",
				EnemyType.DDoS => "DDoS",
				EnemyType.SQLInjection => "SQL",
				EnemyType.BruteForce => "BRUT",
				EnemyType.Ransomware => "RANS",
				EnemyType.Trojan => "TROJ",
				EnemyType.Worm => "WORM",
				_ => "???"
			};
		}

		private static Vector2 GetEnemyScale(EnemyType type)
		{
			return type switch
			{
				EnemyType.Ransomware => new Vector2(1.2f, 1.2f), // Boss más grande
				EnemyType.DDoS => new Vector2(0.7f, 0.7f), // Más pequeño (son muchos)
				EnemyType.Worm => new Vector2(0.6f, 0.6f), // Pequeño y rápido
				_ => new Vector2(0.8f, 0.8f)
			};
		}

		private static float GetCollisionRadius(EnemyType type)
		{
			return type switch
			{
				EnemyType.Ransomware => 30f,
				EnemyType.DDoS => 15f,
				EnemyType.Worm => 12f,
				_ => 20f
			};
		}

		private static Color GetEnemyGlow(EnemyType type)
		{
			// Brillo sutil, no recolorea completamente
			return type switch
			{
				EnemyType.Ransomware => new Color(1.2f, 0.8f, 0.8f), // Tono rojizo
				EnemyType.Trojan => new Color(0.8f, 1.2f, 0.8f), // Tono verdoso
				_ => Colors.White // Sin modificación
			};
		}

		private static Color GetEnemyColor(EnemyType type)
		{
			return type switch
			{
				EnemyType.Malware => ALERT_RED,
				EnemyType.Phishing => FLUX_ORANGE,
				EnemyType.DDoS => RIPPIER_PURPLE,
				EnemyType.SQLInjection => CYBER_BLUE,
				EnemyType.BruteForce => DARK_RED,
				EnemyType.Ransomware => new Color("#ff0000"),
				EnemyType.Trojan => TERMINAL_GREEN,
				EnemyType.Worm => FLUX_ORANGE,
				_ => Colors.White
			};
		}

		private static void AddVisualEffects(Node2D enemy, EnemyType type)
		{
			// Añadir partículas o efectos según tipo
			var particles = new CpuParticles2D();
			particles.Name = "GlowParticles";
			particles.Emitting = true;
			particles.Amount = 8;
			particles.Lifetime = 0.5f;
			particles.SpeedScale = 2;
			particles.EmissionShape = CpuParticles2D.EmissionShapeEnum.Sphere;
			particles.EmissionSphereRadius = 15;
			particles.Direction = new Vector2(0, 1);
			particles.Spread = 180;
			particles.InitialVelocityMin = 10;
			particles.InitialVelocityMax = 30;
			particles.ScaleAmountMin = 1;
			particles.ScaleAmountMax = 3;
			particles.Color = new Color(GetEnemyColor(type), 0.5f);
			
			enemy.AddChild(particles);
		}

		private static void AddComponents(Node2D enemy, EnemyType type)
		{
			// Añadir HealthComponent
			var healthComponent = new HealthComponent();
			healthComponent.Name = "HealthComponent";
			enemy.AddChild(healthComponent);

			// Añadir MovementComponent
			var movementComponent = new MovementComponent();
			movementComponent.Name = "MovementComponent";
			enemy.AddChild(movementComponent);

			// Añadir comportamiento AI
			var aiComponent = CreateAIComponent(type);
			if (aiComponent != null)
			{
				aiComponent.Name = "AIComponent";
				enemy.AddChild(aiComponent);
			}
		}

		private static void ConfigureEnemy(Node2D enemy, EnemyType type)
		{
			var stats = GetEnemyStats(type);
			
			// Configurar salud
			var healthComp = enemy.GetNode<HealthComponent>("HealthComponent");
			if (healthComp != null)
			{
				healthComp.MaxHealth = stats.Health;
				healthComp.Initialize(enemy);
			}

			// Configurar movimiento
			var moveComp = enemy.GetNode<MovementComponent>("MovementComponent");
			if (moveComp != null)
			{
				moveComp.Speed = stats.Speed;
				moveComp.Initialize(enemy);
			}

			GD.Print($"Enemigo creado: {type} | HP: {stats.Health} | Speed: {stats.Speed}");
		}

		private static Node CreateAIComponent(EnemyType type)
		{
			return type switch
			{
				EnemyType.Malware => new MalwareAI(),
				EnemyType.Phishing => new PhishingAI(),
				EnemyType.DDoS => new DDoSAI(),
				EnemyType.SQLInjection => new SQLInjectionAI(),
				EnemyType.BruteForce => new BruteForceAI(),
				EnemyType.Ransomware => new RansomwareAI(),
				_ => new BasicEnemyAI()
			};
		}

		private static EnemyStats GetEnemyStats(EnemyType type)
		{
			return type switch
			{
				EnemyType.Malware => new EnemyStats(30, 200, 10, DamageType.Malware, "Malware básico que infecta sistemas"),
				EnemyType.Phishing => new EnemyStats(20, 250, 15, DamageType.Phishing, "Intento de robo de credenciales"),
				EnemyType.DDoS => new EnemyStats(50, 150, 20, DamageType.DDoS, "Saturador de recursos del sistema"),
				EnemyType.SQLInjection => new EnemyStats(25, 220, 25, DamageType.SQLInjection, "Ataque a bases de datos"),
				EnemyType.BruteForce => new EnemyStats(40, 180, 15, DamageType.BruteForce, "Ataque de contraseñas por fuerza bruta"),
				EnemyType.Ransomware => new EnemyStats(100, 120, 50, DamageType.Malware, "Cifra archivos y pide rescate"),
				EnemyType.Trojan => new EnemyStats(35, 200, 20, DamageType.Malware, "Malware disfrazado de software legítimo"),
				EnemyType.Worm => new EnemyStats(25, 280, 12, DamageType.Malware, "Se replica automáticamente en la red"),
				_ => new EnemyStats(20, 200, 10, DamageType.Physical, "Amenaza desconocida")
			};
		}
	}

	/// <summary>
	/// Tipos de enemigos basados en amenazas de ciberseguridad
	/// </summary>
	public enum EnemyType
	{
		Malware,
		Phishing,
		DDoS,
		SQLInjection,
		BruteForce,
		Ransomware,
		Trojan,
		Worm,
		Spyware,
		Adware
	}

	/// <summary>
	/// Estructura de datos para estadísticas de enemigos
	/// </summary>
	public struct EnemyStats
	{
		public float Health;
		public float Speed;
		public int PointValue;
		public DamageType AttackType;
		public string Description;

		public EnemyStats(float health, float speed, int points, DamageType attackType, string description)
		{
			Health = health;
			Speed = speed;
			PointValue = points;
			AttackType = attackType;
			Description = description;
		}
	}
}
