using Godot;
using CyberSecurityGame.Components;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Entities
{
	/// <summary>
	/// Factory para crear diferentes tipos de enemigos (Factory Pattern)
	/// Principio de Open/Closed: fácil agregar nuevos tipos sin modificar código existente
	/// </summary>
	public static class EnemyFactory
	{
		public static void Initialize()
		{
			// Inicialización de recursos si es necesario
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
			}
			return enemy;
		}

		private static Node2D CreateEnemyInstance(EnemyType type)
		{
			// En producción, esto cargaría escenas específicas
			// Por ahora creamos una instancia genérica
			var enemy = new CharacterBody2D();
			enemy.Name = type.ToString();
			
			// Añadir componentes según el tipo
			AddComponents(enemy, type);
			
			return enemy;
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
