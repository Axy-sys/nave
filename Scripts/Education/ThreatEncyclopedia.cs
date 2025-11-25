using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Education
{
    /// <summary>
    /// Enciclopedia de Amenazas - Sistema de Progresión Educativa
    /// 
    /// DISEÑO UX EDUCATIVO:
    /// - El jugador "descubre" amenazas al encontrarlas
    /// - Cada amenaza tiene niveles de conocimiento (0-3)
    /// - Responder quizzes correctamente sube el nivel
    /// - Desbloquea tips, debilidades y lore
    /// 
    /// INSPIRADO EN:
    /// - Pokédex (Pokémon) - Coleccionar y completar
    /// - Codex (Mass Effect) - Lore profundo
    /// - Bestiary (Witcher) - Debilidades de enemigos
    /// </summary>
    public partial class ThreatEncyclopedia : Node
    {
        private static ThreatEncyclopedia _instance;
        public static ThreatEncyclopedia Instance => _instance;

        // Diccionario de amenazas descubiertas
        private Dictionary<string, ThreatEntry> _threats = new Dictionary<string, ThreatEntry>();
        
        // Estadísticas educativas
        public int TotalThreatsDiscovered { get; private set; } = 0;
        public int TotalThreatsCompleted { get; private set; } = 0; // Nivel 3
        public int TotalQuizzesCorrect { get; private set; } = 0;
        public int TotalQuizzesWrong { get; private set; } = 0;
        
        // Eventos
        [Signal] public delegate void ThreatDiscoveredEventHandler(string threatId, string threatName);
        [Signal] public delegate void ThreatLevelUpEventHandler(string threatId, int newLevel);
        [Signal] public delegate void EncyclopediaProgressEventHandler(int discovered, int total);

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            InitializeAllThreats();
            SubscribeToEvents();
            
            GD.Print($"[Encyclopedia] Sistema iniciado - {_threats.Count} amenazas registradas");
        }

        private void InitializeAllThreats()
        {
            // ═══════════════════════════════════════════════════════════════════
            // MALWARE - Software malicioso
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "Malware",
                Name = "MALWARE",
                Category = ThreatCategory.Malware,
                Icon = "🦠",
                
                // Nivel 0: Nombre visible al descubrir
                ShortDescription = "Software malicioso que daña sistemas",
                
                // Nivel 1: Descripción completa
                FullDescription = "El malware (malicious software) es cualquier programa diseñado para infiltrarse o dañar un sistema informático sin consentimiento del usuario. Incluye virus, troyanos, gusanos y spyware.",
                
                // Nivel 2: Cómo defenderse (gameplay + real)
                HowToDefend = "En el juego: Mantén distancia y dispara desde lejos.\n\nEn la vida real: Mantén tu antivirus actualizado, no descargues archivos de fuentes no confiables, y analiza archivos antes de abrirlos.",
                
                // Nivel 3: Lore del juego + datos reales
                DeepLore = "El primer virus de PC conocido fue 'Brain' en 1986, creado en Pakistán. Hoy existen más de 1,000 millones de variantes de malware. En CODE RIPPIER, el malware representa las amenazas más básicas pero persistentes del ciberespacio.",
                
                // Datos para quiz contextual
                QuizCategory = QuizCategory.Malware,
                RelatedTips = new[] { 
                    "💡 Mantén tu antivirus actualizado",
                    "💡 No descargues software de fuentes no confiables",
                    "💡 Analiza archivos descargados antes de abrirlos"
                },
                
                // Gameplay data
                DamageType = "Daño continuo por contacto",
                Weakness = "Vulnerable a disparos rápidos"
            });
            
            // ═══════════════════════════════════════════════════════════════════
            // PHISHING - Ingeniería social
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "Phishing",
                Name = "PHISHING",
                Category = ThreatCategory.SocialEngineering,
                Icon = "🎣",
                
                ShortDescription = "Engaño para robar credenciales",
                
                FullDescription = "El phishing es una técnica de ingeniería social donde atacantes se hacen pasar por entidades legítimas (bancos, empresas, etc.) para engañar a usuarios y robar información sensible como contraseñas o datos bancarios.",
                
                HowToDefend = "En el juego: Los enemigos Phishing cambian de color para confundirte. ¡No te fíes de las apariencias!\n\nEn la vida real: Verifica siempre el remitente de emails, busca errores ortográficos, y NUNCA hagas clic en enlaces sospechosos. Los bancos jamás piden contraseñas por email.",
                
                DeepLore = "El término 'phishing' viene de 'fishing' (pescar), porque los atacantes lanzan 'anzuelos' esperando que alguien 'muerda'. El primer ataque de phishing documentado fue en 1995 contra usuarios de AOL. Hoy, el 91% de los ciberataques comienzan con un email de phishing.",
                
                QuizCategory = QuizCategory.Phishing,
                RelatedTips = new[] {
                    "💡 Verifica siempre la URL antes de hacer clic",
                    "💡 Los bancos nunca piden contraseñas por email",
                    "💡 Busca errores ortográficos en emails sospechosos"
                },
                
                DamageType = "Confusión y daño por engaño",
                Weakness = "Observación cuidadosa revela su verdadera forma"
            });
            
            // ═══════════════════════════════════════════════════════════════════
            // DDoS - Denegación de servicio
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "DDoS",
                Name = "DDoS ATTACK",
                Category = ThreatCategory.NetworkAttack,
                Icon = "⚡",
                
                ShortDescription = "Saturación de sistemas con tráfico falso",
                
                FullDescription = "DDoS (Distributed Denial of Service) es un ataque donde múltiples sistemas comprometidos (botnet) envían enormes cantidades de tráfico a un servidor para sobrecargarlo y dejarlo inaccesible para usuarios legítimos.",
                
                HowToDefend = "En el juego: Los DDoS vienen en oleadas masivas. Usa tu Encryption Burst [TAB] para limpiar la pantalla.\n\nEn la vida real: Implementa rate limiting, usa CDN (Content Delivery Network), y ten un plan de respuesta a incidentes.",
                
                DeepLore = "El ataque DDoS más grande registrado fue de 3.47 Tbps contra Microsoft Azure en 2021. Los botnets como Mirai infectaron millones de dispositivos IoT para ejecutar ataques masivos. En CODE RIPPIER, los DDoS representan el caos de la sobrecarga de sistemas.",
                
                QuizCategory = QuizCategory.DDoS,
                RelatedTips = new[] {
                    "💡 Los ataques DDoS saturan servidores con tráfico falso",
                    "💡 Los CDN ayudan a mitigar ataques DDoS",
                    "💡 El rate limiting previene saturación de servicios"
                },
                
                DamageType = "Oleadas de proyectiles en masa",
                Weakness = "Eliminar al líder dispersa la oleada"
            });
            
            // ═══════════════════════════════════════════════════════════════════
            // SQL INJECTION - Ataque a bases de datos
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "SQLInjection",
                Name = "SQL INJECTION",
                Category = ThreatCategory.WebAttack,
                Icon = "💉",
                
                ShortDescription = "Inyección de código en bases de datos",
                
                FullDescription = "SQL Injection es una técnica donde atacantes insertan código SQL malicioso en campos de entrada (formularios, URLs) para manipular la base de datos. Puede exponer, modificar o eliminar datos sensibles.",
                
                HowToDefend = "En el juego: Los SQLi tienen patrones de ataque predecibles. Memoriza sus secuencias.\n\nEn la vida real: SIEMPRE usa consultas parametrizadas (prepared statements), nunca concatenes strings para formar SQL, y valida todas las entradas de usuario.",
                
                DeepLore = "SQL Injection fue descubierto en 1998 por Jeff Forristal. El ataque más famoso fue contra Heartland Payment Systems en 2008, exponiendo 130 millones de tarjetas de crédito. La frase clásica ' OR '1'='1 es el 'Hello World' de los hackers.",
                
                QuizCategory = QuizCategory.WebSecurity,
                RelatedTips = new[] {
                    "💡 Usa consultas parametrizadas para prevenir SQL Injection",
                    "💡 Nunca concatenes strings para formar consultas SQL",
                    "💡 Valida y sanitiza todas las entradas de usuario"
                },
                
                DamageType = "Ataques precisos que penetran defensas",
                Weakness = "Patrones de ataque predecibles si prestas atención"
            });
            
            // ═══════════════════════════════════════════════════════════════════
            // RANSOMWARE - Secuestro de datos
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "Ransomware",
                Name = "RANSOMWARE",
                Category = ThreatCategory.Malware,
                Icon = "🔐",
                
                ShortDescription = "Cifra archivos y exige rescate",
                
                FullDescription = "El ransomware es un tipo de malware que cifra los archivos del usuario y exige un pago (generalmente en criptomonedas) para devolver el acceso. Es una de las amenazas más destructivas y lucrativas para los cibercriminales.",
                
                HowToDefend = "En el juego: El Ransomware es un mini-boss. Tiene mucha vida pero es lento. Mantén distancia y dispara constantemente.\n\nEn la vida real: Haz copias de seguridad OFFLINE regularmente, NUNCA pagues el rescate (no garantiza recuperación), y mantén sistemas actualizados.",
                
                DeepLore = "WannaCry (2017) afectó a más de 200,000 computadoras en 150 países, incluyendo hospitales del NHS británico. El ransomware NotPetya causó $10 mil millones en daños globales. En CODE RIPPIER, el Ransomware representa la codicia y destrucción del cibercrimen organizado.",
                
                QuizCategory = QuizCategory.Malware,
                RelatedTips = new[] {
                    "💡 Haz copias de seguridad regularmente",
                    "💡 Nunca pagues el rescate del ransomware",
                    "💡 Mantén sistemas operativos actualizados"
                },
                
                DamageType = "Alto daño, movimiento lento",
                Weakness = "Lento pero resistente - requiere paciencia"
            });
            
            // ═══════════════════════════════════════════════════════════════════
            // BRUTE FORCE - Ataque de fuerza bruta
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "BruteForce",
                Name = "BRUTE FORCE",
                Category = ThreatCategory.Authentication,
                Icon = "🔨",
                
                ShortDescription = "Prueba miles de contraseñas hasta acertar",
                
                FullDescription = "Un ataque de fuerza bruta intenta adivinar contraseñas probando sistemáticamente todas las combinaciones posibles. Con herramientas automatizadas, pueden probar millones de combinaciones por segundo.",
                
                HowToDefend = "En el juego: Los BruteForce son rápidos y persistentes. No paran hasta destruirte o ser destruidos.\n\nEn la vida real: Usa contraseñas largas (+12 caracteres) con mayúsculas, minúsculas, números y símbolos. Activa 2FA (autenticación de dos factores) siempre que sea posible.",
                
                DeepLore = "Una contraseña de 6 caracteres puede ser crackeada en segundos. Una de 12 caracteres con complejidad puede tomar siglos. La contraseña más común sigue siendo '123456'. En CODE RIPPIER, el BruteForce representa la persistencia implacable de los atacantes automatizados.",
                
                QuizCategory = QuizCategory.Authentication,
                RelatedTips = new[] {
                    "💡 Usa contraseñas largas y complejas",
                    "💡 Activa la autenticación de dos factores (2FA)",
                    "💡 Limita los intentos de inicio de sesión"
                },
                
                DamageType = "Ataques rápidos y repetitivos",
                Weakness = "Individualmente débiles, peligrosos en grupo"
            });
            
            // ═══════════════════════════════════════════════════════════════════
            // WORM - Gusano informático
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "Worm",
                Name = "WORM",
                Category = ThreatCategory.Malware,
                Icon = "🐛",
                
                ShortDescription = "Se replica y propaga automáticamente",
                
                FullDescription = "Un gusano informático es malware que se replica a sí mismo para propagarse a otras computadoras. A diferencia de los virus, no necesita un archivo huésped y puede moverse por redes sin intervención humana.",
                
                HowToDefend = "En el juego: Los Worms se multiplican si no los eliminas rápido. ¡Priorízalos!\n\nEn la vida real: Mantén firewall activo, segmenta redes, y actualiza sistemas para cerrar vulnerabilidades que los gusanos explotan.",
                
                DeepLore = "El gusano Morris (1988) fue el primer worm de Internet y afectó al 10% de todas las computadoras conectadas. Stuxnet (2010) fue un worm que destruyó centrifugadoras nucleares de Irán, mostrando el potencial de ciberarmas.",
                
                QuizCategory = QuizCategory.Malware,
                RelatedTips = new[] {
                    "💡 Los gusanos se propagan sin intervención humana",
                    "💡 Segmenta redes para limitar propagación",
                    "💡 Actualiza sistemas para cerrar vulnerabilidades"
                },
                
                DamageType = "Se multiplica con el tiempo",
                Weakness = "Elimínalos rápido antes de que se repliquen"
            });
            
            // ═══════════════════════════════════════════════════════════════════
            // TROJAN - Caballo de Troya
            // ═══════════════════════════════════════════════════════════════════
            
            RegisterThreat(new ThreatEntry
            {
                Id = "Trojan",
                Name = "TROJAN",
                Category = ThreatCategory.Malware,
                Icon = "🐴",
                
                ShortDescription = "Se disfraza de software legítimo",
                
                FullDescription = "Un troyano es malware disfrazado de software legítimo o útil. Una vez instalado, puede robar datos, instalar más malware, o dar acceso remoto a atacantes. No se replica como virus o gusanos.",
                
                HowToDefend = "En el juego: Los Trojans parecen power-ups pero te atacan. Observa su comportamiento antes de acercarte.\n\nEn la vida real: Descarga software solo de fuentes oficiales, verifica firmas digitales, y desconfía de software 'gratuito' sospechoso.",
                
                DeepLore = "El nombre viene del caballo de Troya de la mitología griega. El troyano Zeus (2007) robó más de $100 millones de cuentas bancarias. En CODE RIPPIER, el Trojan representa el engaño y la falsa confianza.",
                
                QuizCategory = QuizCategory.Malware,
                RelatedTips = new[] {
                    "💡 Los troyanos se disfrazan de software legítimo",
                    "💡 Descarga solo de fuentes oficiales",
                    "💡 Verifica firmas digitales de software"
                },
                
                DamageType = "Engaño seguido de ataque sorpresa",
                Weakness = "La observación cuidadosa revela su naturaleza"
            });
        }

        private void RegisterThreat(ThreatEntry threat)
        {
            _threats[threat.Id] = threat;
        }

        private void SubscribeToEvents()
        {
            GameEventBus.Instance.OnEnemyDefeated += OnEnemyDefeated;
            GameEventBus.Instance.OnPlayerDamagedByEnemy += OnPlayerDamagedByEnemy;
            GameEventBus.Instance.OnQuestionAnswered += OnQuestionAnswered;
        }

        // ═══════════════════════════════════════════════════════════════════
        // EVENT HANDLERS - Aprendizaje contextual
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Al derrotar un enemigo, descubrirlo si es nuevo
        /// </summary>
        private void OnEnemyDefeated(string enemyType, int points)
        {
            DiscoverThreat(enemyType);
        }

        /// <summary>
        /// Al ser dañado por un enemigo, mostrar tip contextual
        /// </summary>
        private void OnPlayerDamagedByEnemy(string enemyType, float damage)
        {
            // Descubrir si es nuevo
            DiscoverThreat(enemyType);
            
            // Mostrar tip contextual si existe
            if (_threats.TryGetValue(enemyType, out var threat))
            {
                if (threat.RelatedTips.Length > 0)
                {
                    var rng = new Random();
                    string tip = threat.RelatedTips[rng.Next(threat.RelatedTips.Length)];
                    GameEventBus.Instance.EmitSecurityTipShown($"{threat.Icon} {tip}");
                }
            }
        }

        /// <summary>
        /// Al responder quiz, subir nivel de conocimiento
        /// </summary>
        private void OnQuestionAnswered(bool correct)
        {
            if (correct)
            {
                TotalQuizzesCorrect++;
                
                // Subir nivel de una amenaza relacionada
                // (el QuizSystem debería decirnos cuál, pero por ahora usamos la última descubierta)
                var incompleteThreats = _threats.Values
                    .Where(t => t.IsDiscovered && t.KnowledgeLevel < 3)
                    .ToList();
                
                if (incompleteThreats.Count > 0)
                {
                    var threat = incompleteThreats[0];
                    LevelUpThreat(threat.Id);
                }
            }
            else
            {
                TotalQuizzesWrong++;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // DISCOVERY & PROGRESSION
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Descubre una amenaza (primer encuentro)
        /// </summary>
        public bool DiscoverThreat(string threatId)
        {
            if (!_threats.TryGetValue(threatId, out var threat)) return false;
            if (threat.IsDiscovered) return false;
            
            threat.IsDiscovered = true;
            threat.KnowledgeLevel = 1;
            TotalThreatsDiscovered++;
            
            EmitSignal(SignalName.ThreatDiscovered, threatId, threat.Name);
            EmitSignal(SignalName.EncyclopediaProgress, TotalThreatsDiscovered, _threats.Count);
            
            GD.Print($"[Encyclopedia] 🔍 Nueva amenaza descubierta: {threat.Icon} {threat.Name}");
            
            // Notificar al jugador
            GameEventBus.Instance.EmitSecurityTipShown($"🔍 ¡Nueva amenaza descubierta: {threat.Icon} {threat.Name}!");
            
            return true;
        }

        /// <summary>
        /// Sube el nivel de conocimiento de una amenaza
        /// </summary>
        public bool LevelUpThreat(string threatId)
        {
            if (!_threats.TryGetValue(threatId, out var threat)) return false;
            if (!threat.IsDiscovered) return false;
            if (threat.KnowledgeLevel >= 3) return false;
            
            threat.KnowledgeLevel++;
            
            if (threat.KnowledgeLevel == 3)
            {
                TotalThreatsCompleted++;
                GD.Print($"[Encyclopedia] ⭐ Amenaza DOMINADA: {threat.Name}");
                GameEventBus.Instance.EmitSecurityTipShown($"⭐ ¡{threat.Name} DOMINADO! Conocimiento completo desbloqueado.");
            }
            
            EmitSignal(SignalName.ThreatLevelUp, threatId, threat.KnowledgeLevel);
            
            return true;
        }

        // ═══════════════════════════════════════════════════════════════════
        // GETTERS - Para UI
        // ═══════════════════════════════════════════════════════════════════

        public ThreatEntry GetThreat(string threatId)
        {
            return _threats.TryGetValue(threatId, out var threat) ? threat : null;
        }

        public List<ThreatEntry> GetAllThreats()
        {
            return _threats.Values.ToList();
        }

        public List<ThreatEntry> GetDiscoveredThreats()
        {
            return _threats.Values.Where(t => t.IsDiscovered).ToList();
        }

        public List<ThreatEntry> GetThreatsByCategory(ThreatCategory category)
        {
            return _threats.Values.Where(t => t.Category == category).ToList();
        }

        public float GetCompletionPercentage()
        {
            if (_threats.Count == 0) return 0;
            return (float)TotalThreatsDiscovered / _threats.Count * 100f;
        }

        public float GetMasteryPercentage()
        {
            if (_threats.Count == 0) return 0;
            return (float)TotalThreatsCompleted / _threats.Count * 100f;
        }

        /// <summary>
        /// Obtiene un tip contextual para un tipo de enemigo
        /// </summary>
        public string GetContextualTip(string enemyType)
        {
            if (!_threats.TryGetValue(enemyType, out var threat)) 
                return "💡 Mantén la calma y sigue disparando";
            
            if (!threat.IsDiscovered)
                return $"💡 ¡Amenaza desconocida! Descúbrela derrotándola.";
            
            switch (threat.KnowledgeLevel)
            {
                case 1:
                    return $"{threat.Icon} {threat.ShortDescription}";
                case 2:
                    return $"{threat.Icon} Debilidad: {threat.Weakness}";
                case 3:
                    if (threat.RelatedTips.Length > 0)
                    {
                        var rng = new Random();
                        return threat.RelatedTips[rng.Next(threat.RelatedTips.Length)];
                    }
                    break;
            }
            
            return $"{threat.Icon} {threat.ShortDescription}";
        }

        public override void _ExitTree()
        {
            if (GameEventBus.Instance != null)
            {
                GameEventBus.Instance.OnEnemyDefeated -= OnEnemyDefeated;
                GameEventBus.Instance.OnPlayerDamagedByEnemy -= OnPlayerDamagedByEnemy;
                GameEventBus.Instance.OnQuestionAnswered -= OnQuestionAnswered;
            }
        }
    }

    /// <summary>
    /// Entrada individual en la enciclopedia de amenazas
    /// </summary>
    public class ThreatEntry
    {
        // Identificación
        public string Id { get; set; }
        public string Name { get; set; }
        public ThreatCategory Category { get; set; }
        public string Icon { get; set; }
        
        // Estado de progresión
        public bool IsDiscovered { get; set; } = false;
        public int KnowledgeLevel { get; set; } = 0; // 0=No descubierto, 1=Básico, 2=Intermedio, 3=Experto
        
        // Contenido educativo por nivel
        public string ShortDescription { get; set; }  // Nivel 1
        public string FullDescription { get; set; }   // Nivel 2
        public string HowToDefend { get; set; }       // Nivel 2
        public string DeepLore { get; set; }          // Nivel 3
        
        // Datos de gameplay
        public string DamageType { get; set; }
        public string Weakness { get; set; }
        
        // Datos educativos
        public QuizCategory QuizCategory { get; set; }
        public string[] RelatedTips { get; set; } = Array.Empty<string>();
        
        /// <summary>
        /// Obtiene la descripción según el nivel de conocimiento
        /// </summary>
        public string GetDescriptionForLevel()
        {
            return KnowledgeLevel switch
            {
                0 => "??? - Derrota este enemigo para descubrirlo",
                1 => ShortDescription,
                2 => FullDescription,
                3 => FullDescription + "\n\n" + DeepLore,
                _ => ShortDescription
            };
        }
    }

    public enum ThreatCategory
    {
        Malware,
        SocialEngineering,
        NetworkAttack,
        WebAttack,
        Authentication
    }
}
