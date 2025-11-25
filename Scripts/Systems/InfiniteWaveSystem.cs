using Godot;
using System;
using System.Collections.Generic;
using CyberSecurityGame.Entities;
using CyberSecurityGame.Core;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de Oleadas Infinitas estilo Bullet Hell
    /// 
    /// DISEÑO UX:
    /// - Oleadas infinitas con dificultad escalante
    /// - No hay "fin del juego" excepto muerte
    /// - Score = supervivencia + kills + grazing
    /// - Cada 5 oleadas = mini-boss
    /// - Cada 10 oleadas = boss mayor
    /// - Dificultad escala en: cantidad, velocidad, patrones
    /// 
    /// SIMULACIÓN DE USUARIO:
    /// Wave 1-5: Tutorial implícito, enemigos lentos, pocos disparos
    /// Wave 6-10: Introducción de patrones más complejos
    /// Wave 11-20: Bullet hell real, pantalla con balas
    /// Wave 21+: Caos controlado, supervivencia pura
    /// </summary>
    public partial class InfiniteWaveSystem : Node
    {
        private static InfiniteWaveSystem _instance;
        public static InfiniteWaveSystem Instance => _instance;

        // Configuración base
        [Export] public float TimeBetweenWaves = 3f;
        [Export] public int BaseEnemyCount = 3;
        [Export] public float EnemyCountGrowth = 1.12f; // Reducido de 1.15 para curva más suave
        [Export] public float DifficultyGrowth = 1.05f; // Reducido de 1.08 para curva más suave
        [Export] public float WaveTimeLimit = 60f; // Aumentado de 45s para dar más margen

        // Estado
        private int _currentWave = 0;
        private float _waveTimer = 0f;
        private float _waveTimeoutTimer = 0f; // Timer de timeout
        private int _enemiesRemaining = 0;
        private int _enemiesSpawned = 0;
        private bool _waveActive = false;
        private bool _isPaused = false;
        private float _currentDifficulty = 1.0f;
        private bool _waveTimedOut = false;
        
        // Para estadísticas
        private int _totalKills = 0;
        private int _grazeCount = 0;
        private float _survivalTime = 0f;

        // Tipos de enemigos desbloqueados por oleada
        private Dictionary<int, EnemyType[]> _waveUnlocks = new Dictionary<int, EnemyType[]>
        {
            { 1, new[] { EnemyType.Malware } },
            { 3, new[] { EnemyType.Malware, EnemyType.Phishing } },
            { 5, new[] { EnemyType.Malware, EnemyType.Phishing, EnemyType.DDoS } },
            { 8, new[] { EnemyType.Malware, EnemyType.Phishing, EnemyType.DDoS, EnemyType.SQLInjection } },
            { 10, new[] { EnemyType.Malware, EnemyType.Phishing, EnemyType.DDoS, EnemyType.SQLInjection, EnemyType.BruteForce } },
            { 15, new[] { EnemyType.Malware, EnemyType.Phishing, EnemyType.DDoS, EnemyType.SQLInjection, EnemyType.BruteForce, EnemyType.Worm } },
            { 20, new[] { EnemyType.Malware, EnemyType.Phishing, EnemyType.DDoS, EnemyType.SQLInjection, EnemyType.BruteForce, EnemyType.Worm, EnemyType.Trojan } },
        };

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            EnemyFactory.Initialize();
            
            // Suscribirse a eventos
            GameEventBus.Instance.OnGameStateChanged += OnGameStateChanged;
            
            GD.Print("[InfiniteWave] Sistema de oleadas infinitas listo");
        }

        public override void _ExitTree()
        {
            if (GameEventBus.Instance != null)
            {
                GameEventBus.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                StartInfiniteMode();
            }
            else if (state == GameState.GameOver)
            {
                _isPaused = true;
            }
        }

        public void StartInfiniteMode()
        {
            // ═══════════════════════════════════════════════════════════════════
            // INICIALIZACIÓN DEL MODO INFINITO
            // 
            // UX: Este método puede ser llamado desde:
            // 1. OnGameStateChanged(Playing) - inicio normal
            // 2. MainScene.StartGameDeferred() - failsafe tras reinicio
            // 
            // Evitamos doble inicio verificando si ya estamos activos
            // ═══════════════════════════════════════════════════════════════════
            
            // Evitar reinicio si ya estamos jugando
            if (_currentWave > 0 && !_isPaused)
            {
                GD.Print("[InfiniteWave] Ya iniciado, ignorando llamada duplicada");
                return;
            }
            
            _currentWave = 0;
            _waveActive = false;
            _isPaused = false;
            _currentDifficulty = 1.0f;
            _totalKills = 0;
            _survivalTime = 0f;
            _waveTimer = TimeBetweenWaves - 1f; // Empezar rápido (2 segundos)
            
            GD.Print("[InfiniteWave] ¡MODO INFINITO INICIADO!");
            GD.Print($"[InfiniteWave] Primera oleada en {TimeBetweenWaves - _waveTimer:F1}s");
        }

        public override void _Process(double delta)
        {
            if (_isPaused) return;

            _survivalTime += (float)delta;

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
                // ═══════════════════════════════════════════════════════════════════
                // TIMEOUT DE OLEADA
                // Si no se completa a tiempo, penalizar al jugador
                // ═══════════════════════════════════════════════════════════════════
                _waveTimeoutTimer += (float)delta;
                
                if (_waveTimeoutTimer >= WaveTimeLimit && !_waveTimedOut)
                {
                    OnWaveTimeout();
                }
                
                // Verificar fin de oleada
                if (_enemiesRemaining <= 0 && _enemiesSpawned > 0)
                {
                    EndWave();
                }
            }
        }
        
        /// <summary>
        /// Penalización por no completar la oleada a tiempo
        /// BALANCE: Penalización moderada, no frustrante
        /// </summary>
        private void OnWaveTimeout()
        {
            _waveTimedOut = true;
            
            GD.Print($"[InfiniteWave] ⏰ TIMEOUT Wave {_currentWave}! Penalización aplicada");
            
            // Daño al jugador por timeout - REDUCIDO de 25 a 15
            // En waves tempranas (1-5), solo 10 de daño
            float timeoutDamage = _currentWave <= 5 ? 10f : 15f;
            
            var player = GetTree().Root.GetNodeOrNull<Entities.Player>("Main/Player");
            if (player != null && player.IsAlive())
            {
                player.TakeDamage(timeoutDamage, CyberSecurityGame.Core.Interfaces.DamageType.Physical);
                GD.Print($"[InfiniteWave] 💥 Jugador recibe {timeoutDamage} de daño por timeout");
            }
            
            // Emitir evento de timeout
            GameEventBus.Instance?.EmitSecurityTipShown("⚠️ TIMEOUT - ¡Defiende más rápido!");
            
            // Solo spawnear enemigos extra en waves 5+
            if (_currentWave >= 5)
            {
                SpawnTimeoutPenalty();
            }
        }
        
        private void SpawnTimeoutPenalty()
        {
            var viewport = GetViewport().GetVisibleRect().Size;
            var types = GetAvailableEnemyTypes();
            var rng = new Random();
            
            // Spawnear 2-3 enemigos extra desde los lados
            int extraEnemies = 2 + (_currentWave / 10);
            
            for (int i = 0; i < extraEnemies; i++)
            {
                float delay = i * 0.3f;
                var timer = GetTree().CreateTimer(delay);
                int index = i;
                timer.Timeout += () => {
                    var type = types[rng.Next(types.Length)];
                    Vector2 pos = index % 2 == 0 
                        ? new Vector2(-50, 100 + rng.Next(200))  // Izquierda
                        : new Vector2(viewport.X + 50, 100 + rng.Next(200)); // Derecha
                    
                    SpawnEnemy(type, pos);
                    _enemiesRemaining++;
                };
            }
            
            GD.Print($"[InfiniteWave] +{extraEnemies} enemigos de penalización");
        }

        private void StartNextWave()
        {
            _currentWave++;
            _waveActive = true;
            _enemiesSpawned = 0;
            _waveTimeoutTimer = 0f; // Reset timeout timer
            _waveTimedOut = false;

            // Calcular dificultad escalante
            _currentDifficulty = Mathf.Pow(DifficultyGrowth, _currentWave - 1);
            
            // Calcular cantidad de enemigos
            int baseCount = CalculateEnemyCount();
            _enemiesRemaining = baseCount;
            
            // Tiempo límite escala con la dificultad
            // Waves 1-5: 60s base (aprendizaje)
            // Waves 6-10: 55s 
            // Waves 11+: 50s + bonus por cantidad de enemigos
            if (_currentWave <= 5)
            {
                WaveTimeLimit = 60f;
            }
            else if (_currentWave <= 10)
            {
                WaveTimeLimit = 55f;
            }
            else
            {
                // En waves altas, dar más tiempo por la cantidad de enemigos
                WaveTimeLimit = 50f + (baseCount * 1.5f);
            }

            // Actualizar sistema de balas con la dificultad
            BulletHellSystem.Instance?.SetWaveDifficulty(_currentWave);

            // Determinar tipo de oleada
            string waveType = GetWaveType();
            var (title, desc) = GetWaveDescription();

            GD.Print($"[InfiniteWave] === WAVE {_currentWave} ({waveType}) === Enemigos: {baseCount}, Dificultad: x{_currentDifficulty:F2}");
            
            // Emitir evento de oleada
            GameEventBus.Instance?.EmitWaveAnnounced(_currentWave, title, desc);

            // Spawn de enemigos
            SpawnWaveEnemies(baseCount);

            // BALANCE: Mini-boss cada 10 oleadas, Boss cada 20
            // Esto da tiempo al jugador para aprender antes de enfrentar jefes
            if (_currentWave % 20 == 0)
            {
                // Boss mayor cada 20 oleadas
                SpawnBoss();
            }
            else if (_currentWave % 10 == 0)
            {
                // Mini-boss cada 10 oleadas
                SpawnMiniBoss();
            }
        }

        private string GetWaveType()
        {
            if (_currentWave % 10 == 0) return "BOSS";
            if (_currentWave % 5 == 0) return "MINI-BOSS";
            if (_currentWave <= 5) return "TUTORIAL";
            if (_currentWave <= 10) return "NORMAL";
            if (_currentWave <= 20) return "INTENSO";
            return "CAOS";
        }

        private (string, string) GetWaveDescription()
        {
            // Descripciones dinámicas según la oleada
            if (_currentWave <= 3)
            {
                return ($"WAVE {_currentWave} - INICIALIZACIÓN", 
                       "Los primeros intrusos están entrando al sistema. Defiende el perímetro.");
            }
            else if (_currentWave <= 5)
            {
                return ($"WAVE {_currentWave} - ESCALADA", 
                       "El ataque se intensifica. Nuevas amenazas detectadas.");
            }
            else if (_currentWave % 10 == 0)
            {
                return ($"WAVE {_currentWave} - ¡JEFE DE SECTOR!", 
                       "Una amenaza masiva ha sido detectada. Prepárate para el impacto.");
            }
            else if (_currentWave % 5 == 0)
            {
                return ($"WAVE {_currentWave} - AMENAZA ÉLITE", 
                       "Un enemigo de alto nivel se aproxima. Mantén la guardia.");
            }
            else if (_currentWave <= 15)
            {
                return ($"WAVE {_currentWave} - ASALTO", 
                       "El sistema está bajo ataque sostenido. Resiste.");
            }
            else if (_currentWave <= 25)
            {
                return ($"WAVE {_currentWave} - SOBRECARGA", 
                       "Demasiadas conexiones entrantes. El firewall está al límite.");
            }
            else
            {
                return ($"WAVE {_currentWave} - SUPERVIVENCIA", 
                       $"Has sobrevivido {_currentWave} oleadas. ¿Cuánto más puedes aguantar?");
            }
        }

        private int CalculateEnemyCount()
        {
            // Crecimiento exponencial suave con cap
            int count = (int)(BaseEnemyCount * Mathf.Pow(EnemyCountGrowth, _currentWave - 1));
            
            // Cap según fase del juego
            int maxEnemies = _currentWave switch
            {
                <= 5 => 8,
                <= 10 => 15,
                <= 20 => 25,
                <= 30 => 35,
                _ => 50
            };
            
            return Mathf.Min(count, maxEnemies);
        }

        private EnemyType[] GetAvailableEnemyTypes()
        {
            // Encontrar los tipos desbloqueados para esta oleada
            EnemyType[] types = new[] { EnemyType.Malware };
            
            foreach (var kvp in _waveUnlocks)
            {
                if (_currentWave >= kvp.Key)
                {
                    types = kvp.Value;
                }
            }
            
            return types;
        }

        private void SpawnWaveEnemies(int count)
        {
            var random = new Random();
            var types = GetAvailableEnemyTypes();
            var viewport = GetViewport().GetVisibleRect().Size;

            // Seleccionar patrón de spawn
            var formation = SelectFormation();
            var positions = GenerateFormationPositions(formation, count, viewport);

            for (int i = 0; i < count; i++)
            {
                int index = i;
                float delay = GetSpawnDelay(formation, i);
                
                var timer = GetTree().CreateTimer(delay);
                timer.Timeout += () => {
                    var type = types[random.Next(types.Length)];
                    SpawnEnemy(type, positions[index]);
                };
            }
        }

        private void SpawnEnemy(EnemyType type, Vector2 position)
        {
            var enemy = EnemyFactory.CreateEnemy(type, position);
            if (enemy != null)
            {
                // Añadir componente de disparo bullet hell
                var shooter = new EnemyBulletShooter();
                shooter.EnemyType = type.ToString();
                shooter.FireRate = GetFireRateForWave();
                shooter.PatternComplexity = GetPatternComplexity();
                enemy.AddChild(shooter);

                GetTree().Root.GetNode("Main").AddChild(enemy);
                enemy.TreeExiting += () => OnEnemyDefeated(type);
                
                _enemiesSpawned++;
                
                // Animar entrada
                AnimateEntry(enemy, position);
            }
        }

        private float GetFireRateForWave()
        {
            // BALANCE: Enemigos disparan más lento al inicio
            // Wave 1-5: cada 3s (aprendizaje)
            // Wave 6-10: cada 2.5s
            // Wave 11-20: cada 2s
            // Wave 21+: cada 1.5s (nunca menos)
            if (_currentWave <= 5)
                return 3.0f;
            else if (_currentWave <= 10)
                return 2.5f;
            else if (_currentWave <= 20)
                return 2.0f;
            else
                return Mathf.Max(1.5f, 2.0f - (_currentWave - 20) * 0.02f);
        }

        private int GetPatternComplexity()
        {
            // BALANCE: Patrones complejos más tarde
            // Wave 1-7: Patrones simples (aprendizaje)
            // Wave 8-15: Patrones medios
            // Wave 16-25: Patrones complejos
            // Wave 26+: Caos
            if (_currentWave <= 7) return 1;
            if (_currentWave <= 15) return 2;
            if (_currentWave <= 25) return 3;
            return 4;
        }

        private void SpawnMiniBoss()
        {
            GD.Print($"[InfiniteWave] 👹 MINI-BOSS en Wave {_currentWave}!");
            
            var viewport = GetViewport().GetVisibleRect().Size;
            Vector2 pos = new Vector2(viewport.X / 2, -50);
            
            var boss = EnemyFactory.CreateEnemy(EnemyType.Ransomware, pos);
            if (boss != null)
            {
                // Mini-boss tiene más vida y dispara más
                var health = boss.GetNodeOrNull<CyberSecurityGame.Components.HealthComponent>("HealthComponent");
                if (health != null)
                {
                    health.MaxHealth = 200 + _currentWave * 10;
                    health.Heal(health.MaxHealth);
                }

                var shooter = new EnemyBulletShooter();
                shooter.EnemyType = "MiniBoss";
                shooter.FireRate = 1.0f;
                shooter.PatternComplexity = 3;
                boss.AddChild(shooter);

                GetTree().Root.GetNode("Main").AddChild(boss);
                boss.TreeExiting += () => {
                    OnEnemyDefeated(EnemyType.Ransomware);
                    GameEventBus.Instance?.EmitEnemyDefeated("MiniBoss", 500 + _currentWave * 50);
                };
                
                _enemiesRemaining++;
                _enemiesSpawned++;

                GameEventBus.Instance?.EmitBossSpawned($"MINI-BOSS Wave {_currentWave}");
            }
        }

        private void SpawnBoss()
        {
            GD.Print($"[InfiniteWave] 👹👹👹 BOSS MAYOR en Wave {_currentWave}!");
            
            var viewport = GetViewport().GetVisibleRect().Size;
            Vector2 pos = new Vector2(viewport.X / 2, -80);
            
            var boss = EnemyFactory.CreateEnemy(EnemyType.Ransomware, pos);
            if (boss != null)
            {
                // Boss mayor es mucho más fuerte
                var health = boss.GetNodeOrNull<CyberSecurityGame.Components.HealthComponent>("HealthComponent");
                if (health != null)
                {
                    health.MaxHealth = 500 + _currentWave * 20;
                    health.Heal(health.MaxHealth);
                }

                // Escalar visualmente
                var sprite = boss.GetNodeOrNull<Sprite2D>("Sprite");
                if (sprite != null)
                {
                    sprite.Scale = new Vector2(1.5f, 1.5f);
                }

                var shooter = new EnemyBulletShooter();
                shooter.EnemyType = "Boss";
                shooter.FireRate = 0.5f;
                shooter.PatternComplexity = 4;
                boss.AddChild(shooter);

                GetTree().Root.GetNode("Main").AddChild(boss);
                boss.TreeExiting += () => {
                    OnEnemyDefeated(EnemyType.Ransomware);
                    GameEventBus.Instance?.EmitEnemyDefeated("Boss", 1000 + _currentWave * 100);
                    // Limpiar balas al derrotar boss
                    BulletHellSystem.Instance?.ClearAllBullets();
                };
                
                _enemiesRemaining++;
                _enemiesSpawned++;

                GameEventBus.Instance?.EmitBossSpawned($"BOSS - Wave {_currentWave}");
            }
        }

        private void OnEnemyDefeated(EnemyType type)
        {
            _enemiesRemaining--;
            _totalKills++;

            int points = CalculatePoints(type);
            GameEventBus.Instance?.EmitEnemyDefeated(type.ToString(), points);
        }

        private void EndWave()
        {
            _waveActive = false;
            
            GD.Print($"[InfiniteWave] ✓ Wave {_currentWave} completada! Kills: {_totalKills}, Tiempo: {_survivalTime:F1}s");
            
            // Emitir evento de oleada completada (para AdaptiveDifficultySystem)
            GameEventBus.Instance?.EmitWaveCompleted(_currentWave);
            
            // Bonus por completar oleada
            int waveBonus = 100 * _currentWave;
            GameEventBus.Instance?.EmitScoreChanged(waveBonus);
            
            // BALANCE: Curar al jugador al completar oleada
            // Wave 1-5: +15 HP
            // Wave 6-10: +10 HP  
            // Wave 11+: +5 HP
            float healAmount = _currentWave <= 5 ? 15f : (_currentWave <= 10 ? 10f : 5f);
            var player = GetTree().Root.GetNodeOrNull<Entities.Player>("Main/Player");
            if (player != null && player.IsAlive())
            {
                player.Heal(healAmount);
                GD.Print($"[InfiniteWave] 💚 Jugador curado +{healAmount} HP por completar oleada");
            }
            
            // Cada 10 oleadas: Vida extra!
            if (_currentWave % 10 == 0 && _currentWave > 0)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddLife();
                    GameEventBus.Instance?.EmitSecurityTipShown("💚 +1 VIDA por sobrevivir 10 oleadas!");
                    GD.Print($"[InfiniteWave] 🎉 +1 VIDA por completar wave {_currentWave}!");
                }
            }
            
            // Reducir tiempo entre oleadas gradualmente
            TimeBetweenWaves = Mathf.Max(2.0f, 3f - (_currentWave * 0.03f));
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
            return (int)(basePoints * (1 + _currentWave * 0.1f) * _currentDifficulty);
        }

        // ═══════════════════════════════════════════════════════════════════
        // FORMACIONES
        // ═══════════════════════════════════════════════════════════════════

        private enum Formation { Line, V, Grid, Pincer, Random, Spiral }

        private Formation SelectFormation()
        {
            var formations = new List<Formation> { Formation.Line, Formation.Random };
            
            if (_currentWave >= 3) formations.Add(Formation.V);
            if (_currentWave >= 5) formations.Add(Formation.Grid);
            if (_currentWave >= 8) formations.Add(Formation.Pincer);
            if (_currentWave >= 12) formations.Add(Formation.Spiral);

            var rng = new Random();
            return formations[rng.Next(formations.Count)];
        }

        private List<Vector2> GenerateFormationPositions(Formation formation, int count, Vector2 viewport)
        {
            var positions = new List<Vector2>();
            float centerX = viewport.X / 2;
            float spacing = 70f;
            var rng = new Random();

            switch (formation)
            {
                case Formation.Line:
                    float startX = centerX - (count - 1) * spacing / 2;
                    for (int i = 0; i < count; i++)
                        positions.Add(new Vector2(startX + i * spacing, -50 - rng.Next(30)));
                    break;

                case Formation.V:
                    for (int i = 0; i < count; i++)
                    {
                        int side = i % 2 == 0 ? -1 : 1;
                        int row = i / 2;
                        positions.Add(new Vector2(centerX + side * row * spacing * 0.8f, -50 - row * 40));
                    }
                    break;

                case Formation.Grid:
                    int cols = Mathf.Min(count, 6);
                    float gridStart = centerX - (cols - 1) * spacing / 2;
                    for (int i = 0; i < count; i++)
                        positions.Add(new Vector2(gridStart + (i % cols) * spacing, -50 - (i / cols) * 60));
                    break;

                case Formation.Pincer:
                    int half = count / 2;
                    for (int i = 0; i < half; i++)
                        positions.Add(new Vector2(-50, 100 + i * 60));
                    for (int i = half; i < count; i++)
                        positions.Add(new Vector2(viewport.X + 50, 100 + (i - half) * 60));
                    break;

                case Formation.Spiral:
                    for (int i = 0; i < count; i++)
                    {
                        float angle = i * 0.5f;
                        float radius = 30 + i * 15;
                        positions.Add(new Vector2(centerX + Mathf.Cos(angle) * radius, -80 - Mathf.Sin(angle) * radius * 0.3f));
                    }
                    break;

                default:
                    for (int i = 0; i < count; i++)
                        positions.Add(new Vector2(50 + (float)rng.NextDouble() * (viewport.X - 100), -50 - rng.Next(100)));
                    break;
            }

            return positions;
        }

        private float GetSpawnDelay(Formation formation, int index)
        {
            return formation switch
            {
                Formation.Line => index * 0.1f,
                Formation.V => index * 0.15f,
                Formation.Grid => (index % 6) * 0.1f + (index / 6) * 0.3f,
                Formation.Pincer => index * 0.2f,
                Formation.Spiral => index * 0.12f,
                _ => index * 0.08f
            };
        }

        private void AnimateEntry(Node2D enemy, Vector2 startPos)
        {
            var viewport = GetViewport().GetVisibleRect().Size;
            float targetY = 80 + new Random().Next(80);
            float targetX = Mathf.Clamp(startPos.X, 60, viewport.X - 60);

            var tween = enemy.CreateTween();
            tween.SetTrans(Tween.TransitionType.Cubic);
            tween.SetEase(Tween.EaseType.Out);
            tween.TweenProperty(enemy, "position", new Vector2(targetX, targetY), 0.8f);
        }

        // ═══════════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ═══════════════════════════════════════════════════════════════════

        public int GetCurrentWave() => _currentWave;
        public int GetEnemiesRemaining() => _enemiesRemaining;
        public int GetTotalKills() => _totalKills;
        public float GetSurvivalTime() => _survivalTime;
        public float GetDifficulty() => _currentDifficulty;
        public bool IsWaveActive() => _waveActive;
        public float GetWaveTimeRemaining() => Mathf.Max(0, WaveTimeLimit - _waveTimeoutTimer);
        public float GetWaveTimeLimit() => WaveTimeLimit;
        public bool IsWaveTimedOut() => _waveTimedOut;
    }

    /// <summary>
    /// Componente de disparo para enemigos - Genera patrones bullet hell
    /// </summary>
    public partial class EnemyBulletShooter : Node
    {
        public string EnemyType { get; set; } = "Malware";
        public float FireRate { get; set; } = 2.0f;
        public int PatternComplexity { get; set; } = 1;

        private float _fireTimer = 0f;
        private float _patternRotation = 0f;
        private Node2D _owner;
        private Node2D _target;
        private Random _rng = new Random();

        public override void _Ready()
        {
            _owner = GetParent<Node2D>();
            _fireTimer = (float)_rng.NextDouble() * FireRate; // Desync inicial
        }

        public override void _Process(double delta)
        {
            if (_owner == null || !IsInstanceValid(_owner)) return;
            if (BulletHellSystem.Instance == null) return;

            _fireTimer += (float)delta;
            _patternRotation += (float)delta * 0.5f;

            // Buscar jugador
            if (_target == null || !IsInstanceValid(_target))
            {
                _target = GetTree().Root.GetNodeOrNull<Node2D>("Main/Player");
            }

            if (_fireTimer >= FireRate)
            {
                _fireTimer = 0f;
                Fire();
            }
        }

        private void Fire()
        {
            Vector2 pos = _owner.GlobalPosition;
            Color color = BulletHellSystem.Instance.GetBulletColorForEnemy(EnemyType);
            float speed = 150f + PatternComplexity * 30f;

            // Seleccionar patrón según complejidad y tipo
            switch (PatternComplexity)
            {
                case 1: // Simple
                    SimplePattern(pos, color, speed);
                    break;
                case 2: // Medio
                    MediumPattern(pos, color, speed);
                    break;
                case 3: // Complejo
                    ComplexPattern(pos, color, speed);
                    break;
                default: // Caos
                    ChaosPattern(pos, color, speed);
                    break;
            }
        }

        private void SimplePattern(Vector2 pos, Color color, float speed)
        {
            // Disparo simple hacia abajo o al jugador
            if (_target != null && _rng.NextDouble() > 0.5)
            {
                BulletHellSystem.Instance.FireAimedPattern(pos, _target.GlobalPosition, 1, 0, speed, color);
            }
            else
            {
                BulletHellSystem.Instance.FireAimedPattern(pos, pos + Vector2.Down * 100, 1, 0, speed, color);
            }
        }

        private void MediumPattern(Vector2 pos, Color color, float speed)
        {
            int pattern = _rng.Next(3);
            switch (pattern)
            {
                case 0: // Abanico al jugador
                    if (_target != null)
                        BulletHellSystem.Instance.FireAimedPattern(pos, _target.GlobalPosition, 3, 30, speed, color);
                    break;
                case 1: // Radial pequeño
                    BulletHellSystem.Instance.FireRadialPattern(pos, 6, speed, color, _patternRotation);
                    break;
                case 2: // Doble disparo
                    BulletHellSystem.Instance.FireAimedPattern(pos, pos + Vector2.Down * 100, 2, 20, speed, color);
                    break;
            }
        }

        private void ComplexPattern(Vector2 pos, Color color, float speed)
        {
            int pattern = _rng.Next(4);
            switch (pattern)
            {
                case 0: // Espiral
                    BulletHellSystem.Instance.FireSpiralPattern(pos, 4, speed, color, _patternRotation * 2);
                    break;
                case 1: // Radial denso
                    BulletHellSystem.Instance.FireRadialPattern(pos, 12, speed * 0.8f, color, _patternRotation);
                    break;
                case 2: // Abanico amplio
                    if (_target != null)
                        BulletHellSystem.Instance.FireAimedPattern(pos, _target.GlobalPosition, 5, 60, speed, color);
                    break;
                case 3: // Cruz rotatoria
                    BulletHellSystem.Instance.FireCrossPattern(pos, speed, color, _patternRotation * 3);
                    break;
            }
        }

        private void ChaosPattern(Vector2 pos, Color color, float speed)
        {
            int pattern = _rng.Next(5);
            switch (pattern)
            {
                case 0: // Espiral doble
                    BulletHellSystem.Instance.FireSpiralPattern(pos, 6, speed, color, _patternRotation * 2);
                    BulletHellSystem.Instance.FireSpiralPattern(pos, 6, speed * 0.7f, color, -_patternRotation * 2);
                    break;
                case 1: // Radial masivo
                    BulletHellSystem.Instance.FireRadialPattern(pos, 20, speed * 0.6f, color, _patternRotation);
                    break;
                case 2: // Onda
                    BulletHellSystem.Instance.FireWavePattern(pos, 10, speed, color, 0.5f, 3f);
                    break;
                case 3: // Burst al jugador
                    if (_target != null)
                        BulletHellSystem.Instance.FireBurstPattern(pos, (_target.GlobalPosition - pos).Normalized(), 5, speed * 1.5f, color);
                    break;
                case 4: // Random scatter
                    BulletHellSystem.Instance.FireRandomPattern(pos, 8, speed, color);
                    break;
            }
        }
    }
}
