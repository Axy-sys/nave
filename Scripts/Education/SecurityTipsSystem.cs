using Godot;
using System.Collections.Generic;

namespace CyberSecurityGame.Education
{
    /// <summary>
    /// Sistema que gestiona tips y mensajes educativos durante el gameplay
    /// </summary>
    public partial class SecurityTipsSystem : Node
    {
        private static SecurityTipsSystem _instance;
        public static SecurityTipsSystem Instance => _instance;

        private Dictionary<string, List<string>> _tipsByCategory;
        private Queue<string> _tipsQueue;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            InitializeTips();
        }

        private void InitializeTips()
        {
            _tipsByCategory = new Dictionary<string, List<string>>
            {
                ["Malware"] = new List<string>
                {
                    "💡 Mantén tu antivirus actualizado",
                    "💡 No descargues software de fuentes no confiables",
                    "💡 Los antivirus detectan y eliminan software malicioso",
                    "💡 Analiza archivos descargados antes de abrirlos",
                    "💡 El malware puede robar información personal"
                },
                
                ["Phishing"] = new List<string>
                {
                    "💡 Verifica siempre la URL antes de hacer clic",
                    "💡 Los bancos nunca piden contraseñas por email",
                    "💡 Busca errores ortográficos en emails sospechosos",
                    "💡 No compartas información personal por email",
                    "💡 El phishing intenta robar tus credenciales"
                },
                
                ["DDoS"] = new List<string>
                {
                    "💡 Los ataques DDoS saturan servidores con tráfico falso",
                    "💡 Los CDN ayudan a mitigar ataques DDoS",
                    "💡 El rate limiting previene saturación de servicios",
                    "💡 Los botnets se usan para ejecutar ataques DDoS"
                },
                
                ["SQLInjection"] = new List<string>
                {
                    "💡 Usa consultas parametrizadas para prevenir SQL Injection",
                    "💡 Nunca concatenes strings para formar consultas SQL",
                    "💡 Valida y sanitiza todas las entradas de usuario",
                    "💡 SQL Injection puede exponer toda tu base de datos"
                },
                
                ["BruteForce"] = new List<string>
                {
                    "💡 Usa contraseñas largas y complejas",
                    "💡 Activa la autenticación de dos factores (2FA)",
                    "💡 Limita los intentos de inicio de sesión",
                    "💡 Los ataques de fuerza bruta prueban miles de contraseñas"
                },
                
                ["Ransomware"] = new List<string>
                {
                    "💡 Haz copias de seguridad regularmente",
                    "💡 Nunca pagues el rescate del ransomware",
                    "💡 Mantén sistemas operativos actualizados",
                    "💡 El ransomware cifra tus archivos y pide dinero"
                },
                
                ["General"] = new List<string>
                {
                    "💡 Usa un gestor de contraseñas",
                    "💡 Habilita actualizaciones automáticas",
                    "💡 Usa VPN en redes WiFi públicas",
                    "💡 Configura firewall en todos tus dispositivos",
                    "💡 Revisa permisos de aplicaciones regularmente",
                    "💡 Desconfía de ofertas demasiado buenas",
                    "💡 Cifra datos sensibles",
                    "💡 Usa contraseñas diferentes para cada servicio"
                }
            };

            _tipsQueue = new Queue<string>();
        }

        public string GetTipByCategory(string category)
        {
            if (_tipsByCategory.ContainsKey(category))
            {
                var tips = _tipsByCategory[category];
                var random = new System.Random();
                return tips[random.Next(tips.Count)];
            }
            
            return GetRandomTip();
        }

        public string GetRandomTip()
        {
            var allTips = new List<string>();
            foreach (var tipsList in _tipsByCategory.Values)
            {
                allTips.AddRange(tipsList);
            }

            var random = new System.Random();
            return allTips[random.Next(allTips.Count)];
        }

        public List<string> GetAllTipsForCategory(string category)
        {
            return _tipsByCategory.ContainsKey(category) ? 
                _tipsByCategory[category] : 
                new List<string>();
        }
    }
}
