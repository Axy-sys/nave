using Godot;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Core.Interfaces;
using CyberSecurityGame.Components;

namespace CyberSecurityGame.Systems
{
	/// <summary>
	/// Sistema de power-ups educativos con efectos visuales satisfactorios
	/// Factory Pattern para crear diferentes tipos de power-ups
	/// </summary>
	public partial class PowerUpSystem : Node
	{
		private static PowerUpSystem _instance;
		public static PowerUpSystem Instance => _instance;

		// Colores temáticos
		private static readonly Color HEAL_GREEN = new Color("#00ff41");
		private static readonly Color SHIELD_BLUE = new Color("#00d4ff");
		private static readonly Color POWER_ORANGE = new Color("#ffaa00");
		private static readonly Color SPECIAL_PURPLE = new Color("#bf00ff");

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
			// Solo spawneamos los power-ups más útiles
			var commonTypes = new[] { 
				PowerUpType.AntivirusBoost, 
				PowerUpType.FirewallUpgrade, 
				PowerUpType.EncryptionShield,
				PowerUpType.PatchUpdate,
				PowerUpType.BackupRestore
			};
			var randomType = commonTypes[random.Next(commonTypes.Length)];

			SpawnPowerUp(randomType, GetRandomSpawnPosition());
		}

		public void SpawnPowerUp(PowerUpType type, Vector2 position)
		{
			var powerUp = CreatePowerUp(type);
			if (powerUp != null)
			{
				powerUp.GlobalPosition = position;
				GetTree().Root.GetNodeOrNull("Main")?.AddChild(powerUp);
				GD.Print($"Power-up spawned: {type}");
			}
		}

		private Node2D CreatePowerUp(PowerUpType type)
		{
			var powerUp = new Area2D();
			powerUp.Name = $"PowerUp_{type}";
			powerUp.CollisionLayer = 8; // Capa de power-ups
			powerUp.CollisionMask = 1;  // Solo colisiona con jugador

			// ═══════════════════════════════════════════════════════════
			// VISUAL: Icono con glow pulsante
			// ═══════════════════════════════════════════════════════════
			var data = GetPowerUpData(type);
			
			// Fondo circular con glow
			var glowCircle = new Sprite2D();
			glowCircle.Name = "GlowCircle";
			glowCircle.Texture = CreateCircleTexture(32);
			glowCircle.Modulate = new Color(data.Color, 0.3f);
			glowCircle.Scale = new Vector2(2.5f, 2.5f);
			powerUp.AddChild(glowCircle);

			// Círculo principal
			var mainCircle = new Sprite2D();
			mainCircle.Name = "MainCircle";
			mainCircle.Texture = CreateCircleTexture(24);
			mainCircle.Modulate = new Color(data.Color, 0.8f);
			powerUp.AddChild(mainCircle);

			// Icono del power-up (texto/símbolo)
			var iconLabel = new Label();
			iconLabel.Name = "Icon";
			iconLabel.Text = data.Icon;
			iconLabel.Position = new Vector2(-12, -15);
			iconLabel.AddThemeColorOverride("font_color", Colors.White);
			iconLabel.AddThemeFontSizeOverride("font_size", 20);
			powerUp.AddChild(iconLabel);

			// Nombre flotante arriba
			var nameLabel = new Label();
			nameLabel.Name = "NameLabel";
			nameLabel.Text = data.ShortName;
			nameLabel.Position = new Vector2(-30, -45);
			nameLabel.AddThemeColorOverride("font_color", data.Color);
			nameLabel.AddThemeFontSizeOverride("font_size", 10);
			powerUp.AddChild(nameLabel);

			// Partículas
			var particles = new CpuParticles2D();
			particles.Name = "Particles";
			particles.Emitting = true;
			particles.Amount = 12;
			particles.Lifetime = 1.0f;
			particles.SpeedScale = 1.5f;
			particles.EmissionShape = CpuParticles2D.EmissionShapeEnum.Sphere;
			particles.EmissionSphereRadius = 20;
			particles.Direction = new Vector2(0, -1);
			particles.Spread = 180;
			particles.InitialVelocityMin = 15;
			particles.InitialVelocityMax = 30;
			particles.ScaleAmountMin = 1;
			particles.ScaleAmountMax = 3;
			particles.Color = new Color(data.Color, 0.6f);
			powerUp.AddChild(particles);

			// Colisión
			var collision = new CollisionShape2D();
			var shape = new CircleShape2D();
			shape.Radius = 25f;
			collision.Shape = shape;
			powerUp.AddChild(collision);

			// Script de comportamiento
			var script = new PowerUpBehavior
			{
				Type = type,
				Name = "PowerUpBehavior"
			};
			powerUp.AddChild(script);
			script.Initialize(powerUp);

			return powerUp;
		}

		private Texture2D CreateCircleTexture(int radius)
		{
			var image = Image.CreateEmpty(radius * 2, radius * 2, false, Image.Format.Rgba8);
			image.Fill(Colors.White);
			return ImageTexture.CreateFromImage(image);
		}

		private Vector2 GetRandomSpawnPosition()
		{
			var random = new System.Random();
			var viewport = GetViewport().GetVisibleRect().Size;
			return new Vector2(
				(float)random.NextDouble() * (viewport.X - 200) + 100,
				(float)random.NextDouble() * (viewport.Y - 200) + 100
			);
		}

		private PowerUpData GetPowerUpData(PowerUpType type)
		{
			return type switch
			{
				PowerUpType.AntivirusBoost => new PowerUpData(
					"Actualización de Antivirus", "ANTIVIRUS", "+",
					"Restaura salud y aumenta resistencia a malware",
					"Mantén tu antivirus siempre actualizado para máxima protección",
					HEAL_GREEN
				),
				PowerUpType.FirewallUpgrade => new PowerUpData(
					"Mejora de Firewall", "FIREWALL", "▣",
					"Activa escudo temporal",
					"Los firewalls son la primera línea de defensa contra amenazas",
					SHIELD_BLUE
				),
				PowerUpType.EncryptionShield => new PowerUpData(
					"Escudo de Encriptación", "ENCRYPT", "◈",
					"Protección contra todos los tipos de daño",
					"La encriptación protege tus datos incluso si son interceptados",
					SPECIAL_PURPLE
				),
				PowerUpType.PatchUpdate => new PowerUpData(
					"Parche de Seguridad", "PATCH", "⬆",
					"Elimina vulnerabilidades activas",
					"Instala parches de seguridad tan pronto como estén disponibles",
					POWER_ORANGE
				),
				PowerUpType.TwoFactorAuth => new PowerUpData(
					"Autenticación 2FA", "2FA", "✓",
					"Vida extra y protección adicional",
					"2FA añade una capa extra de seguridad a tus cuentas",
					HEAL_GREEN
				),
				PowerUpType.BackupRestore => new PowerUpData(
					"Restauración de Backup", "BACKUP", "↺",
					"Restaura salud completa",
					"Las copias de seguridad son esenciales para recuperarse de ataques",
					HEAL_GREEN
				),
				_ => new PowerUpData("Power-up", "???", "?", "Beneficio", "", POWER_ORANGE)
			};
		}
	}

	/// <summary>
	/// Comportamiento de un power-up individual con efectos visuales
	/// </summary>
	public partial class PowerUpBehavior : Node
	{
		public PowerUpType Type { get; set; }
		private Area2D _area;
		private float _lifetime = 20f;
		private float _timer = 0f;
		private float _pulseTimer = 0f;
		private Sprite2D _glowCircle;
		private bool _isCollected = false;

		public void Initialize(Node owner)
		{
			_area = owner as Area2D;
			if (_area != null)
			{
				_area.BodyEntered += OnBodyEntered;
				_glowCircle = _area.GetNodeOrNull<Sprite2D>("GlowCircle");
			}
		}

		public override void _Process(double delta)
		{
			if (_isCollected) return;

			_timer += (float)delta;
			_pulseTimer += (float)delta;

			// Animación de pulso
			if (_glowCircle != null)
			{
				float pulse = 1.0f + 0.3f * Mathf.Sin(_pulseTimer * 4f);
				_glowCircle.Scale = new Vector2(2.5f * pulse, 2.5f * pulse);
			}

			// Flotar arriba/abajo
			if (_area != null)
			{
				_area.Position = new Vector2(
					_area.Position.X,
					_area.Position.Y + Mathf.Sin(_pulseTimer * 2f) * 0.5f
				);
			}

			// Parpadeo al final de vida
			if (_timer >= _lifetime - 3f)
			{
				float blink = Mathf.Abs(Mathf.Sin(_timer * 10f));
				if (_area != null) _area.Modulate = new Color(1, 1, 1, blink);
			}

			if (_timer >= _lifetime)
			{
				_area?.QueueFree();
			}
		}

		private void OnBodyEntered(Node2D body)
		{
			if (_isCollected) return;
			if (body.Name != "Player") return;

			_isCollected = true;
			ApplyPowerUp(body);
			ShowCollectEffect();
			GameEventBus.Instance.EmitPowerUpCollected(Type.ToString());
		}

		private void ApplyPowerUp(Node2D player)
		{
			var playerEntity = player as Entities.Player;
			if (playerEntity == null) return;

			switch (Type)
			{
				case PowerUpType.AntivirusBoost:
					playerEntity.Heal(30f);
					break;
				case PowerUpType.FirewallUpgrade:
					playerEntity.ActivateShield(ShieldType.Firewall, 50f);
					break;
				case PowerUpType.EncryptionShield:
					playerEntity.ActivateShield(ShieldType.Encryption, 75f);
					break;
				case PowerUpType.PatchUpdate:
					playerEntity.Heal(15f);
					break;
				case PowerUpType.BackupRestore:
					playerEntity.Heal(100f);
					break;
				case PowerUpType.TwoFactorAuth:
					playerEntity.Heal(25f);
					playerEntity.ActivateShield(ShieldType.Firewall, 25f);
					break;
			}

			GD.Print($"⭐ Power-up aplicado: {Type}");
		}

		private void ShowCollectEffect()
		{
			if (_area == null) return;

			// Efecto de recolección: expansión + fade
			var tween = _area.CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(_area, "scale", new Vector2(2f, 2f), 0.2f);
			tween.TweenProperty(_area, "modulate:a", 0.0f, 0.2f);
			tween.Chain().TweenCallback(Callable.From(_area.QueueFree));

			// Mostrar nombre del power-up recogido
			ShowCollectPopup();
		}

		private void ShowCollectPopup()
		{
			if (_area == null) return;

			var data = PowerUpSystem.Instance != null ? 
				GetPowerUpDataLocal(Type) : 
				new PowerUpData("Power-up", "???", "?", "", "", Colors.White);

			var popup = new Label();
			popup.Text = $"+ {data.ShortName}";
			popup.GlobalPosition = _area.GlobalPosition + new Vector2(-40, -60);
			popup.AddThemeColorOverride("font_color", data.Color);
			popup.AddThemeFontSizeOverride("font_size", 18);
			popup.ZIndex = 200;
			
			_area.GetTree().Root.GetNodeOrNull("Main")?.AddChild(popup);

			var tween = popup.CreateTween();
			tween.TweenProperty(popup, "position:y", popup.Position.Y - 50, 0.6f);
			tween.Parallel().TweenProperty(popup, "modulate:a", 0.0f, 0.6f).SetDelay(0.2f);
			tween.TweenCallback(Callable.From(popup.QueueFree));

			// Mostrar tip educativo
			GameEventBus.Instance.EmitSecurityTipShown(data.EducationalTip);
		}

		private PowerUpData GetPowerUpDataLocal(PowerUpType type)
		{
			return type switch
			{
				PowerUpType.AntivirusBoost => new PowerUpData("Antivirus", "ANTIVIRUS", "+", "", "Mantén tu antivirus actualizado", new Color("#00ff41")),
				PowerUpType.FirewallUpgrade => new PowerUpData("Firewall", "FIREWALL", "▣", "", "Los firewalls bloquean amenazas", new Color("#00d4ff")),
				PowerUpType.EncryptionShield => new PowerUpData("Encriptación", "ENCRYPT", "◈", "", "La encriptación protege tus datos", new Color("#bf00ff")),
				PowerUpType.PatchUpdate => new PowerUpData("Parche", "PATCH", "⬆", "", "Instala parches de seguridad", new Color("#ffaa00")),
				PowerUpType.BackupRestore => new PowerUpData("Backup", "BACKUP", "↺", "", "Los backups son esenciales", new Color("#00ff41")),
				PowerUpType.TwoFactorAuth => new PowerUpData("2FA", "2FA", "✓", "", "2FA añade seguridad extra", new Color("#00ff41")),
				_ => new PowerUpData("???", "???", "?", "", "", Colors.White)
			};
		}
	}

	public enum PowerUpType
	{
		AntivirusBoost,
		FirewallUpgrade,
		EncryptionShield,
		PatchUpdate,
		TwoFactorAuth,
		BackupRestore,
		IDSUpgrade,
		SecureVPN,
		QuizBonus,
		MultiFactorBoost
	}

	public struct PowerUpData
	{
		public string Name;
		public string ShortName;
		public string Icon;
		public string Description;
		public string EducationalTip;
		public Color Color;

		public PowerUpData(string name, string shortName, string icon, string description, string tip, Color color)
		{
			Name = name;
			ShortName = shortName;
			Icon = icon;
			Description = description;
			EducationalTip = tip;
			Color = color;
		}
	}
}
