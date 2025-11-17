using Godot;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Systems
{
	/// <summary>
	/// Sistema de power-ups educativos
	/// Factory Pattern para crear diferentes tipos de power-ups
	/// </summary>
	public partial class PowerUpSystem : Node
	{
		private static PowerUpSystem _instance;
		public static PowerUpSystem Instance => _instance;

		[Export] public float SpawnInterval = 10f;
		[Export] public PackedScene PowerUpScene;

		private float _spawnTimer = 0f;

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}
			_instance = this;
		}

		public override void _Process(double delta)
		{
			_spawnTimer += (float)delta;

			if (_spawnTimer >= SpawnInterval)
			{
				SpawnRandomPowerUp();
				_spawnTimer = 0f;
			}
		}

		private void SpawnRandomPowerUp()
		{
			var random = new System.Random();
			var types = System.Enum.GetValues(typeof(PowerUpType));
			var randomType = (PowerUpType)types.GetValue(random.Next(types.Length));

			SpawnPowerUp(randomType, GetRandomSpawnPosition());
		}

		public void SpawnPowerUp(PowerUpType type, Vector2 position)
		{
			var powerUp = CreatePowerUp(type);
			if (powerUp != null)
			{
				powerUp.GlobalPosition = position;
				GetTree().Root.AddChild(powerUp);
				GD.Print($"Power-up spawned: {type}");
			}
		}

		private Node2D CreatePowerUp(PowerUpType type)
		{
			var powerUp = new Area2D();
			powerUp.Name = $"PowerUp_{type}";

			// Añadir colisión
			var collision = new CollisionShape2D();
			var shape = new CircleShape2D();
			shape.Radius = 15f;
			collision.Shape = shape;
			powerUp.AddChild(collision);

			// Añadir script de power-up
			var script = new PowerUpBehavior
			{
				Type = type,
				Name = "PowerUpBehavior"
			};
			powerUp.AddChild(script);
			script.Initialize(powerUp);

			return powerUp;
		}

		private Vector2 GetRandomSpawnPosition()
		{
			var random = new System.Random();
			return new Vector2(
				(float)random.NextDouble() * 1000 + 100,
				(float)random.NextDouble() * 600 + 100
			);
		}
	}

	/// <summary>
	/// Comportamiento de un power-up individual
	/// </summary>
	public partial class PowerUpBehavior : Node
	{
		public PowerUpType Type { get; set; }
		private Area2D _area;
		private float _lifetime = 15f; // Segundos antes de desaparecer
		private float _timer = 0f;

		public void Initialize(Node owner)
		{
			_area = owner as Area2D;
			if (_area != null)
			{
				_area.BodyEntered += OnBodyEntered;
			}
		}

		public override void _Process(double delta)
		{
			_timer += (float)delta;
			if (_timer >= _lifetime)
			{
				_area?.QueueFree();
			}

			// Animación de rotación
			if (_area != null)
			{
				_area.Rotation += (float)delta * 2f;
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (body.Name == "Player")
			{
				ApplyPowerUp(body);
				GameEventBus.Instance.EmitPowerUpCollected(Type.ToString());
				_area?.QueueFree();
			}
		}

		private void ApplyPowerUp(Node2D player)
		{
			var powerUpData = GetPowerUpData(Type);
			GD.Print($"⭐ Power-up recogido: {powerUpData.Name}");
			GD.Print($"📚 {powerUpData.EducationalTip}");

			// Aquí se aplicarían los efectos al jugador
			// Por ejemplo: player.ActivateShield(), player.Heal(), etc.
		}

		private PowerUpData GetPowerUpData(PowerUpType type)
		{
			return type switch
			{
				PowerUpType.AntivirusBoost => new PowerUpData(
					"Actualización de Antivirus",
					"Restaura salud y aumenta resistencia a malware",
                    "💡 Mantén tu antivirus siempre actualizado para máxima protección"
				),
				PowerUpType.FirewallUpgrade => new PowerUpData(
					"Mejora de Firewall",
					"Activa escudo temporal",
                    "💡 Los firewalls son la primera línea de defensa contra amenazas"
				),
				PowerUpType.EncryptionShield => new PowerUpData(
					"Escudo de Encriptación",
					"Protección contra todos los tipos de daño",
                    "💡 La encriptación protege tus datos incluso si son interceptados"
				),
				PowerUpType.PatchUpdate => new PowerUpData(
					"Parche de Seguridad",
					"Elimina vulnerabilidades activas",
                    "💡 Instala parches de seguridad tan pronto como estén disponibles"
				),
				PowerUpType.TwoFactorAuth => new PowerUpData(
					"Autenticación 2FA",
					"Vida extra y protección adicional",
                    "💡 2FA añade una capa extra de seguridad a tus cuentas"
				),
				PowerUpType.BackupRestore => new PowerUpData(
					"Restauración de Backup",
					"Restaura salud completa",
                    "💡 Las copias de seguridad son esenciales para recuperarse de ataques"
				),
				PowerUpType.IDSUpgrade => new PowerUpData(
					"Sistema IDS Mejorado",
					"Detecta enemigos a mayor distancia",
                    "💡 Los IDS detectan comportamientos sospechosos en tiempo real"
				),
				PowerUpType.SecureVPN => new PowerUpData(
					"VPN Segura",
					"Aumenta velocidad de movimiento",
                    "💡 Las VPN cifran tu tráfico y protegen tu privacidad"
				),
				PowerUpType.QuizBonus => new PowerUpData(
					"Pregunta Bonus",
					"Responde correctamente para obtener puntos extra",
                    "💡 El conocimiento es la mejor defensa contra amenazas"
				),
				PowerUpType.MultiFactorBoost => new PowerUpData(
					"Autenticación Multifactor",
					"Múltiples capas de protección",
                    "💡 Más factores de autenticación = mayor seguridad"
				),
				_ => new PowerUpData("Power-up", "Beneficio desconocido", "")
			};
		}
	}

	/// <summary>
	/// Tipos de power-ups disponibles
	/// </summary>
	public enum PowerUpType
	{
		AntivirusBoost,      // Restaura salud
		FirewallUpgrade,     // Activa escudo
		EncryptionShield,    // Escudo poderoso
		PatchUpdate,         // Elimina vulnerabilidades
		TwoFactorAuth,       // Vida extra
		BackupRestore,       // Salud completa
		IDSUpgrade,          // Mejor detección
		SecureVPN,           // Velocidad aumentada
		QuizBonus,           // Pregunta educativa
		MultiFactorBoost     // Múltiples beneficios
	}

	/// <summary>
	/// Datos de un power-up
	/// </summary>
	public struct PowerUpData
	{
		public string Name;
		public string Description;
		public string EducationalTip;

		public PowerUpData(string name, string description, string tip)
		{
			Name = name;
			Description = description;
			EducationalTip = tip;
		}
	}
}
