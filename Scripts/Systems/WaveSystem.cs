using Godot;
using System.Collections.Generic;
using CyberSecurityGame.Entities;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
	/// <summary>
	/// Sistema de spawning de enemigos con oleadas progresivas
	/// Implementa patrón de dificultad creciente
	/// </summary>
	public partial class WaveSystem : Node
	{
		private static WaveSystem _instance;
		public static WaveSystem Instance => _instance;

		[Export] public float TimeBetweenWaves = 5f; // Más rápido para acción constante
		[Export] public int InitialEnemyCount = 3;
		[Export] public float DifficultyScale = 1.2f;
		
		private const int WAVES_PER_LEVEL = 3;

		private int _currentWave = 0;
		private int _currentLevel = 1;
		private float _waveTimer = 0f;
		private int _enemiesRemaining = 0;
		private bool _waveActive = false;
		private bool _levelComplete = false;
		private HashSet<EnemyType> _encounteredEnemies = new HashSet<EnemyType>();

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}
			_instance = this;
			
			EnemyFactory.Initialize();
			_encounteredEnemies.Clear();
			
			// Suscribirse a eventos
			GameEventBus.Instance.OnLevelStarted += OnLevelStarted;
		}

		public override void _ExitTree()
		{
			GameEventBus.Instance.OnLevelStarted -= OnLevelStarted;
		}

		private void OnLevelStarted(int level)
		{
			_currentLevel = level;
			_currentWave = 0;
			_waveActive = false;
			_levelComplete = false;
			_waveTimer = TimeBetweenWaves - 2f; // Iniciar pronto
			
			GD.Print($"🚀 Sistema de Oleadas listo para Nivel {_currentLevel}");
		}

		public override void _Process(double delta)
		{
			if (_levelComplete) return;

			if (!_waveActive)
			{
				_waveTimer += (float)delta;
				
				if (_waveTimer >= TimeBetweenWaves)
				{
					StartNextWave();
					_waveTimer = 0f;
				}
			}
			else
			{
				// Verificar si quedan enemigos
				if (_enemiesRemaining <= 0)
				{
					EndWave();
				}
			}
		}

		private void StartNextWave()
		{
			_currentWave++;
			_waveActive = true;
			
			// Configuración de oleada según progreso
			int enemyCount = CalculateEnemyCount();
			_enemiesRemaining = enemyCount;

			// Definir escenario narrativo
			var (title, desc) = GetWaveScenario(_currentLevel, _currentWave);
			GD.Print($"🌊 Iniciando Oleada {_currentWave}/{WAVES_PER_LEVEL} (Nivel {_currentLevel}) - {title}");
			
			// Ya no emitimos LevelStarted aquí, solo anunciamos la oleada
			GameEventBus.Instance.EmitWaveAnnounced(_currentWave, title, desc);

			// Trigger Dialogue based on Wave
			TriggerWaveDialogue(_currentLevel, _currentWave);

			SpawnWaveEnemies(enemyCount);
			SpawnObstacles(3 + _currentWave); // Spawn obstacles based on wave intensity
			
			// Spawn Bonus Data Node
			if (_currentWave % 2 == 0) // Every 2 waves
			{
				SpawnDataNode();
			}

			// Spawn Lore Terminal occasionally
			if (_currentWave == 3)
			{
				SpawnLoreTerminal();
			}
		}

		private void SpawnDataNode()
		{
			var node = new DataNode();
			node.Position = GetRandomSpawnPosition();
			// Ensure it's within reachable bounds (not too high up)
			node.Position = new Vector2(node.Position.X, 100); 
			GetTree().Root.GetNode("Main").AddChild(node);
			GD.Print("💎 Data Node Spawned!");
		}

		private void SpawnLoreTerminal()
		{
			var terminal = new LoreTerminal();
			terminal.Position = GetRandomSpawnPosition();
			terminal.Position = new Vector2(terminal.Position.X, 150);
			
			// Set content based on level
			if (_currentLevel == 1)
			{
				terminal.Title = "Log de Servidor 0451";
				terminal.Content = "Detectamos tráfico inusual en el puerto 80. Parece que alguien está intentando un ataque de fuerza bruta. La contraseña 'admin123' no fue buena idea.";
			}
			else if (_currentLevel == 2)
			{
				terminal.Title = "Chat Interceptado";
				terminal.Content = "User_X: ¿Ya subiste el payload?\nUser_Y: Sí, está oculto en la imagen del gato. Esteganografía básica.";
			}
			
			GetTree().Root.GetNode("Main").AddChild(terminal);
			GD.Print("📂 Lore Terminal Spawned!");
		}

		/// <summary>
		/// DESACTIVADO: Los diálogos ahora se manejan en MissionIntroSystem
		/// para evitar superposición de UI y slow-motion no deseado
		/// </summary>
		private void TriggerWaveDialogue(int level, int wave)
		{
			// El sistema MissionIntroSystem ahora maneja toda la narrativa
			// de forma integrada con el briefing de misión.
			// No llamamos a DialogueSystem.Instance aquí para evitar:
			// 1. Paneles superpuestos
			// 2. Slow-motion (Engine.TimeScale = 0.1) durante gameplay
			// 3. Confusión de UX con múltiples diálogos
			
			// Solo log para debug
			GD.Print($"[WaveSystem] Wave {wave} de Level {level} - Diálogo manejado por MissionIntro");
		}

		private void SpawnObstacles(int count)
		{
			var random = new System.Random();
			for (int i = 0; i < count; i++)
			{
				var timer = GetTree().CreateTimer(i * 1.5f + 0.5f);
				timer.Timeout += () => {
					var obstacle = new Obstacle();
					obstacle.Position = GetRandomSpawnPosition();
					GetTree().Root.GetNode("Main").AddChild(obstacle);
				};
			}
		}

		private (string, string) GetWaveScenario(int level, int wave)
		{
			// NARRATIVA GLOBAL: MR. ROBOT / DARK WEB THEME
			// Nivel 1: Surface Web / Entry Node (Vigilancia Corporativa)
			// Nivel 2: Deep Web / Onion Routing (Mercados Negros y Nodos Tor)
			// Nivel 3: Dark Web / Corporate Core (Secretos de E-Corp y Zero Days)

			if (level == 1) // SURFACE / ENTRY NODE
			{
				return wave switch
				{
					1 => ("HOLA, AMIGO", "SITUACIÓN: Estás en el nodo de entrada. El tráfico parece normal, pero 'ellos' están mirando.\n\nAMENAZA: Scripts de rastreo y Adware corporativo."),
					2 => ("INGENIERÍA SOCIAL", "SITUACIÓN: Los empleados de E-Corp son el eslabón débil. Sus correos son nuestra puerta trasera.\n\nAMENAZA: Phishing masivo detectado."),
					3 => ("EL OJO QUE TODO LO VE", "SITUACIÓN: Han activado los protocolos de vigilancia. Saben que estamos aquí.\n\nAMENAZA: Botnet de vigilancia lanzando ataque DDoS."),
					_ => ("ALERTA DE SISTEMA", "Intrusión detectada.")
				};
			}
			else if (level == 2) // DEEP WEB / ONION ROUTING
			{
				return wave switch
				{
					1 => ("CAPAS DE CEBOLLA", "SITUACIÓN: Estamos enrutando por la red Tor. El tráfico es anónimo, pero peligroso.\n\nAMENAZA: Nodos maliciosos intentando desanonimizarte."),
					2 => ("MERCADO NEGRO", "SITUACIÓN: Silk Road 3.0. Aquí se vende de todo. Cuidado con las inyecciones en las transacciones.\n\nAMENAZA: SQL Injection en los ledgers de cripto."),
					3 => ("HONEYPOT DEL FBI", "SITUACIÓN: Es una trampa. Este nodo es un señuelo federal. Fuerza bruta inminente.\n\nAMENAZA: Ataque de Fuerza Bruta gubernamental."),
					_ => ("ALERTA CRÍTICA", "Desanonimización en curso.")
				};
			}
			else // DARK WEB / CORE
			{
				return wave switch
				{
					1 => ("SECUESTRO DIGITAL", "SITUACIÓN: Dark Army ha desplegado su ransomware. Quieren borrar los backups.\n\nAMENAZA: Ransomware de grado militar."),
					2 => ("EL EJÉRCITO OSCURO", "SITUACIÓN: Whiterose ha enviado a sus mejores daemons. No dejes rastro.\n\nAMENAZA: Troyanos y Spyware de estado-nación."),
					3 => ("FASE 2", "SITUACIÓN: El núcleo de E-Corp está expuesto. Es hora de ejecutar el hack final.\n\nAMENAZA: Zero-Day Exploit y Gusanos autoreplicantes."),
					_ => ("ERROR FATAL", "Kernel Panic.")
				};
			}
		}		// ═══════════════════════════════════════════════════════════════════
		// FORMACIONES ESTILO SPACE INVADERS / GALAGA
		// ═══════════════════════════════════════════════════════════════════
		
		private enum FormationType
		{
			Line,           // Línea horizontal
			VShape,         // Formación en V
			Diamond,        // Diamante
			Wave,           // Onda sinusoidal
			Grid,           // Cuadrícula clásica Space Invaders
			Pincer,         // Pinza desde los lados
			Cascade,        // Cascada escalonada
			Spiral          // Entrada en espiral
		}

		private void SpawnWaveEnemies(int count)
		{
			var random = new System.Random();
			
			// Definir tipos de enemigos permitidos según la oleada
			var allowedTypes = GetAllowedEnemyTypes(_currentWave);

			// Verificar si hay enemigos nuevos para mostrar información
			CheckForNewEnemies(allowedTypes);

			// Seleccionar formación según nivel y oleada
			var formation = SelectFormation();
			var positions = GenerateFormationPositions(formation, count);
			
			GD.Print($"📐 Formación: {formation} con {count} enemigos");

			// Spawn con delay visual para entrada dramática
			for (int i = 0; i < positions.Count; i++)
			{
				int index = i;
				var timer = GetTree().CreateTimer(GetFormationDelay(formation, i));
				timer.Timeout += () => SpawnEnemyAtFormation(random, allowedTypes, positions[index], formation, index);
			}
		}

		private FormationType SelectFormation()
		{
			// Formaciones más complejas en oleadas/niveles avanzados
			var formations = new List<FormationType>();
			
			if (_currentLevel == 1)
			{
				formations.Add(FormationType.Line);
				if (_currentWave >= 2) formations.Add(FormationType.VShape);
				if (_currentWave >= 3) formations.Add(FormationType.Wave);
			}
			else if (_currentLevel == 2)
			{
				formations.Add(FormationType.VShape);
				formations.Add(FormationType.Grid);
				if (_currentWave >= 2) formations.Add(FormationType.Diamond);
				if (_currentWave >= 3) formations.Add(FormationType.Pincer);
			}
			else // Nivel 3
			{
				formations.Add(FormationType.Grid);
				formations.Add(FormationType.Pincer);
				formations.Add(FormationType.Cascade);
				if (_currentWave >= 2) formations.Add(FormationType.Spiral);
				if (_currentWave >= 3) formations.Add(FormationType.Diamond);
			}

			var random = new System.Random();
			return formations[random.Next(formations.Count)];
		}

		private List<Vector2> GenerateFormationPositions(FormationType formation, int count)
		{
			var viewport = GetViewport().GetVisibleRect().Size;
			float centerX = viewport.X / 2;
			float spacing = 80f;
			var positions = new List<Vector2>();

			switch (formation)
			{
				case FormationType.Line:
					// Línea horizontal entrando desde arriba
					float lineStartX = centerX - (count - 1) * spacing / 2;
					for (int i = 0; i < count; i++)
					{
						positions.Add(new Vector2(lineStartX + i * spacing, -50));
					}
					break;

				case FormationType.VShape:
					// Formación en V (punta hacia abajo)
					for (int i = 0; i < count; i++)
					{
						int side = i % 2 == 0 ? -1 : 1;
						int row = i / 2;
						float x = centerX + side * row * spacing / 1.5f;
						float y = -50 - row * 40;
						positions.Add(new Vector2(x, y));
					}
					break;

				case FormationType.Diamond:
					// Formación diamante
					int halfCount = count / 2;
					for (int i = 0; i < count; i++)
					{
						int row = i < halfCount ? i : count - i - 1;
						float x = centerX + (i < halfCount ? (i - halfCount / 2) : ((count - i - 1) - halfCount / 2)) * spacing;
						float y = -50 - Mathf.Abs(i - count / 2) * 50;
						positions.Add(new Vector2(x, y));
					}
					break;

				case FormationType.Wave:
					// Onda sinusoidal
					for (int i = 0; i < count; i++)
					{
						float x = 100 + i * (viewport.X - 200) / count;
						float y = -50 - Mathf.Sin(i * 0.8f) * 60;
						positions.Add(new Vector2(x, y));
					}
					break;

				case FormationType.Grid:
					// Cuadrícula clásica Space Invaders
					int cols = Mathf.Min(count, 6);
					int rows = Mathf.CeilToInt((float)count / cols);
					float gridStartX = centerX - (cols - 1) * spacing / 2;
					for (int i = 0; i < count; i++)
					{
						int col = i % cols;
						int row = i / cols;
						positions.Add(new Vector2(gridStartX + col * spacing, -50 - row * 60));
					}
					break;

				case FormationType.Pincer:
					// Pinza desde ambos lados
					int leftCount = count / 2;
					int rightCount = count - leftCount;
					// Lado izquierdo
					for (int i = 0; i < leftCount; i++)
					{
						positions.Add(new Vector2(-50, 100 + i * 70));
					}
					// Lado derecho
					for (int i = 0; i < rightCount; i++)
					{
						positions.Add(new Vector2(viewport.X + 50, 100 + i * 70));
					}
					break;

				case FormationType.Cascade:
					// Cascada escalonada
					for (int i = 0; i < count; i++)
					{
						float x = 100 + (i % 3) * (viewport.X - 200) / 3;
						float y = -50 - (i / 3) * 80 - (i % 3) * 30;
						positions.Add(new Vector2(x, y));
					}
					break;

				case FormationType.Spiral:
					// Entrada en espiral
					for (int i = 0; i < count; i++)
					{
						float angle = i * 0.5f;
						float radius = 50 + i * 20;
						float x = centerX + Mathf.Cos(angle) * radius;
						float y = -100 - Mathf.Sin(angle) * radius / 3;
						positions.Add(new Vector2(x, y));
					}
					break;

				default:
					// Fallback: posiciones aleatorias
					for (int i = 0; i < count; i++)
					{
						positions.Add(GetRandomSpawnPosition());
					}
					break;
			}

			return positions;
		}

		private float GetFormationDelay(FormationType formation, int index)
		{
			// Delays diferentes según el tipo de formación para entrada dramática
			return formation switch
			{
				FormationType.Line => index * 0.1f,
				FormationType.VShape => index * 0.15f,
				FormationType.Diamond => index * 0.12f,
				FormationType.Wave => index * 0.08f,
				FormationType.Grid => (index % 6) * 0.1f + (index / 6) * 0.3f,
				FormationType.Pincer => index * 0.2f,
				FormationType.Cascade => index * 0.25f,
				FormationType.Spiral => index * 0.18f,
				_ => index * 0.8f
			};
		}

		private void SpawnEnemyAtFormation(System.Random random, System.Array enemyTypes, Vector2 startPos, FormationType formation, int index)
		{
			var type = (EnemyType)enemyTypes.GetValue(random.Next(enemyTypes.Length));
			
			var enemy = EnemyFactory.CreateEnemy(type, startPos);
			if (enemy != null)
			{
				GetTree().Root.GetNode("Main").AddChild(enemy);
				enemy.TreeExiting += () => OnEnemyDefeated(type);
				
				// Animar entrada según formación
				AnimateFormationEntry(enemy, formation, startPos, index);
			}
		}

		private void AnimateFormationEntry(Node2D enemy, FormationType formation, Vector2 startPos, int index)
		{
			var viewport = GetViewport().GetVisibleRect().Size;
			float targetY = 80 + (index / 6) * 60; // Fila objetivo
			float centerX = viewport.X / 2;

			var tween = enemy.CreateTween();
			tween.SetTrans(Tween.TransitionType.Cubic);
			tween.SetEase(Tween.EaseType.Out);

			switch (formation)
			{
				case FormationType.Line:
				case FormationType.VShape:
				case FormationType.Diamond:
				case FormationType.Wave:
				case FormationType.Grid:
				case FormationType.Cascade:
				case FormationType.Spiral:
					// Entrada vertical suave
					var targetPos = new Vector2(startPos.X, targetY);
					targetPos.X = Mathf.Clamp(targetPos.X, 60, viewport.X - 60);
					tween.TweenProperty(enemy, "position", targetPos, 1.0f + index * 0.05f);
					break;

				case FormationType.Pincer:
					// Entrada horizontal desde los lados
					float targetX = startPos.X < 0 ? 150 + index * 40 : viewport.X - 150 - (index - 5) * 40;
					targetX = Mathf.Clamp(targetX, 60, viewport.X - 60);
					tween.TweenProperty(enemy, "position", new Vector2(targetX, startPos.Y), 1.2f);
					break;
			}
		}

		private void CheckForNewEnemies(System.Array enemyTypes)
		{
			foreach (EnemyType type in enemyTypes)
			{
				if (!_encounteredEnemies.Contains(type))
				{
					_encounteredEnemies.Add(type);
					var (name, desc, weakness) = GetEnemyInfo(type);
					GameEventBus.Instance.EmitNewEnemyEncountered(name, desc, weakness);
				}
			}
		}

		private (string, string, string) GetEnemyInfo(EnemyType type)
		{
			return type switch
			{
				EnemyType.Malware => (
					"AMENAZA DETECTADA: MALWARE", 
					"CÓDIGO MALICIOSO GENÉRICO. Estos programas intentan infiltrarse en tu sistema para dañar archivos o robar recursos.", 
                    "ACCIÓN REQUERIDA: Usa tu ANTIVIRUS (Tecla 2 / Disparo Azul) para escanear y eliminar esta amenaza."
				),
				EnemyType.Phishing => (
					"AMENAZA DETECTADA: PHISHING", 
					"INTENTO DE ENGAÑO. Correos o mensajes falsos que simulan ser entidades legítimas para robar tus contraseñas.", 
                    "ACCIÓN REQUERIDA: Activa tu FIREWALL (Tecla 1 / Disparo Rojo) para bloquear estas conexiones no autorizadas."
				),
				EnemyType.DDoS => (
					"AMENAZA DETECTADA: BOTNET DDoS", 
					"ATAQUE DE DENEGACIÓN DE SERVICIO. Una red de dispositivos infectados intenta saturar tu servidor con tráfico basura.", 
                    "ACCIÓN REQUERIDA: Usa ENCRIPTACIÓN (Tecla 3 / Disparo Verde) para filtrar el tráfico y proteger la integridad de los datos."
				),
				EnemyType.SQLInjection => (
					"AMENAZA DETECTADA: SQL INJECTION",
					"INYECCIÓN DE CÓDIGO. El atacante intenta manipular tu base de datos insertando comandos maliciosos en los formularios.",
					"ACCIÓN REQUERIDA: Usa FIREWALL (Tecla 1) para bloquear las peticiones mal formadas."
				),
				EnemyType.BruteForce => (
					"AMENAZA DETECTADA: FUERZA BRUTA",
					"INTENTO DE ACCESO MASIVO. Múltiples intentos de adivinar contraseñas en muy poco tiempo.",
					"ACCIÓN REQUERIDA: Usa ANTIVIRUS (Tecla 2) para detectar y bloquear las IPs atacantes."
				),
				EnemyType.Ransomware => (
					"AMENAZA DETECTADA: RANSOMWARE",
					"SECUESTRO DE DATOS. Malware avanzado que cifra tus archivos y exige un pago para liberarlos.",
					"ACCIÓN REQUERIDA: Usa ENCRIPTACIÓN (Tecla 3) para proteger tus backups y contrarrestar el cifrado malicioso."
				),
				EnemyType.Trojan => (
					"AMENAZA DETECTADA: TROYANO",
					"SOFTWARE DISFRAZADO. Parece un programa legítimo pero abre una puerta trasera para los atacantes.",
					"ACCIÓN REQUERIDA: Usa ANTIVIRUS (Tecla 2) para escanear y eliminar el código oculto."
				),
				EnemyType.Worm => (
					"AMENAZA DETECTADA: GUSANO (WORM)",
					"AUTOPROPAGACIÓN. Se replica automáticamente a través de la red sin intervención humana.",
					"ACCIÓN REQUERIDA: Usa FIREWALL (Tecla 1) para aislar los segmentos de red infectados."
				),
				_ => ("AMENAZA DESCONOCIDA", "Analizando patrón de ataque...", "Debilidad desconocida")
			};
		}

		private System.Array GetAllowedEnemyTypes(int wave)
		{
			// Progresión de enemigos basada en Nivel y Oleada
			if (_currentLevel == 1)
			{
				if (wave == 1) return new[] { EnemyType.Malware };
				if (wave == 2) return new[] { EnemyType.Malware, EnemyType.Phishing };
				return new[] { EnemyType.Malware, EnemyType.Phishing, EnemyType.DDoS };
			}
			else if (_currentLevel == 2)
			{
				if (wave == 1) return new[] { EnemyType.DDoS };
				if (wave == 2) return new[] { EnemyType.DDoS, EnemyType.SQLInjection };
				return new[] { EnemyType.DDoS, EnemyType.SQLInjection, EnemyType.BruteForce };
			}
			else // Nivel 3
			{
				if (wave == 1) return new[] { EnemyType.Ransomware };
				if (wave == 2) return new[] { EnemyType.Ransomware, EnemyType.Trojan }; // Asumiendo Trojan/Spyware
				return new[] { EnemyType.Ransomware, EnemyType.Trojan, EnemyType.Worm };
			}
		}

		private void SpawnSingleEnemy(System.Random random, System.Array enemyTypes)
		{
			var type = (EnemyType)enemyTypes.GetValue(random.Next(enemyTypes.Length));
			Vector2 spawnPos = GetRandomSpawnPosition();
			
			var enemy = EnemyFactory.CreateEnemy(type, spawnPos);
			if (enemy != null)
			{
				GetTree().Root.GetNode("Main").AddChild(enemy);
				enemy.TreeExiting += () => OnEnemyDefeated(type);
			}
		}

		private void OnEnemyDefeated(EnemyType type)
		{
			_enemiesRemaining--;
			
			int points = CalculatePoints(type);
			GameEventBus.Instance.EmitEnemyDefeated(type.ToString(), points);
		}

		private void EndWave()
		{
			_waveActive = false;
			GD.Print($"✅ Oleada {_currentWave} completada!");
			
			// Verificar fin de nivel
			if (_currentWave >= WAVES_PER_LEVEL)
			{
				// En lugar de completar el nivel inmediatamente, lanzamos el Quiz Final
				GD.Print("📝 Iniciando Evaluación de Nivel");
				GameEventBus.Instance.EmitVulnerabilityDetected($"Level {_currentLevel} Assessment");
			}
		}

		// Método público para completar el nivel (llamado desde MainScene tras aprobar el quiz)
		public void CompleteLevel()
		{
			if (_levelComplete) return;
			
			_levelComplete = true;
			GameEventBus.Instance.EmitLevelCompleted(_currentLevel);
			GameEventBus.Instance.EmitSecurityTipShown($"NIVEL {_currentLevel} COMPLETADO - SISTEMA SEGURO");
		}

		private void SpawnBoss()
		{
			GD.Print("👹 ¡Boss apareciendo!");
			GameEventBus.Instance.EmitBossSpawned("Ransomware Boss");
			
			Vector2 spawnPos = new Vector2(500, 100); // Centro arriba
			var boss = EnemyFactory.CreateEnemy(EnemyType.Ransomware, spawnPos);
			
			if (boss != null)
			{
				GetTree().Root.GetNode("Main").AddChild(boss);
				boss.TreeExiting += () => OnBossDefeated();
			}
		}

		private void OnBossDefeated()
		{
			GD.Print("🏆 ¡Boss derrotado!");
			GameEventBus.Instance.EmitEnemyDefeated("Boss", 5000);
		}

		private int CalculateEnemyCount()
		{
			// Aumenta enemigos por oleada con límite
			int count = (int)(InitialEnemyCount * Mathf.Pow(DifficultyScale, _currentWave - 1));
			return Mathf.Min(count, 20); // Máximo 20 enemigos por oleada
		}

		private int CalculatePoints(EnemyType type)
		{
			int basePoints = type switch
			{
				EnemyType.Malware => 10,
				EnemyType.Phishing => 15,
				EnemyType.DDoS => 20,
				EnemyType.SQLInjection => 25,
				EnemyType.BruteForce => 15,
				EnemyType.Ransomware => 50,
				EnemyType.Trojan => 20,
				EnemyType.Worm => 12,
				_ => 10
			};

			// Multiplicador por oleada
			return (int)(basePoints * (1 + _currentWave * 0.1f));
		}

		private Vector2 GetRandomSpawnPosition()
		{
			var random = new System.Random();
			var viewportSize = GetViewport().GetVisibleRect().Size;
			
			// Spawns desde arriba, usando el ancho del viewport con margen
			float margin = 50f;
			float x = (float)random.NextDouble() * (viewportSize.X - margin * 2) + margin;
			float y = -50; // Fuera de pantalla arriba
			
			return new Vector2(x, y);
		}

		public int GetCurrentWave() => _currentWave;
		public int GetEnemiesRemaining() => _enemiesRemaining;
		public bool IsWaveActive() => _waveActive;
	}
}
