using Godot;
using System.Collections.Generic;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema dedicado a la gestión de tips educativos y notificaciones dinámicas.
    /// Escucha eventos del juego y decide cuándo mostrar información relevante.
    /// </summary>
    public partial class TipSystem : Node
    {
        private static TipSystem _instance;
        public static TipSystem Instance => _instance;

        // Control de frecuencia para no saturar al jugador
        private Dictionary<string, bool> _shownTips = new Dictionary<string, bool>();
        private float _tipCooldown = 0f;
        private const float MIN_TIME_BETWEEN_TIPS = 5.0f;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;

            SubscribeToEvents();
        }

        public override void _Process(double delta)
        {
            if (_tipCooldown > 0)
            {
                _tipCooldown -= (float)delta;
            }
        }

        private void SubscribeToEvents()
        {
            GameEventBus.Instance.OnNewEnemyEncountered += OnNewEnemyEncountered;
            GameEventBus.Instance.OnPlayerHealthChanged += OnHealthChanged;
            GameEventBus.Instance.OnShieldActivated += OnShieldActivated;
            GameEventBus.Instance.OnPowerUpCollected += OnPowerUpCollected;
        }

        private void OnNewEnemyEncountered(string name, string description, string weakness)
        {
            // Pausar el juego brevemente para mostrar la ficha del enemigo (Efecto educativo)
            ShowEducationalCard("NUEVA AMENAZA IDENTIFICADA", name, description, weakness);
        }

        private void OnHealthChanged(float health)
        {
            if (health < 30 && !_shownTips.ContainsKey("LowHealth"))
            {
                GameEventBus.Instance.EmitSecurityTipShown("¡INTEGRIDAD CRÍTICA! Busca Nodos de Datos para restaurar tu sistema.");
                _shownTips["LowHealth"] = true;
            }
        }

        private void OnShieldActivated(string shieldType)
        {
            if (!_shownTips.ContainsKey(shieldType))
            {
                string tip = shieldType switch
                {
                    "Firewall" => "FIREWALL ACTIVO: Bloquea conexiones no autorizadas (Phishing).",
                    "Encriptación" => "ENCRIPTACIÓN ACTIVA: Protege tus datos contra Ransomware.",
                    "Antivirus" => "ANTIVIRUS ACTIVO: Escanea y elimina Malware y Troyanos.",
                    _ => "ESCUDO ACTIVO"
                };
                GameEventBus.Instance.EmitSecurityTipShown(tip);
                _shownTips[shieldType] = true;
            }
        }

        private void OnPowerUpCollected(string type)
        {
             GameEventBus.Instance.EmitSecurityTipShown($"MEJORA ADQUIRIDA: {type}");
        }

        private void ShowEducationalCard(string title, string subtitle, string body, string footer)
        {
            // Aquí podríamos instanciar una UI modal si quisiéramos pausar
            // Por ahora, usamos el sistema de notificaciones existente pero con formato especial
            string formattedMsg = $"{subtitle}\n\n{body}\n\nTIP: {footer}";
            GameEventBus.Instance.EmitSecurityTipShown(formattedMsg);
        }

        public override void _ExitTree()
        {
            GameEventBus.Instance.OnNewEnemyEncountered -= OnNewEnemyEncountered;
            GameEventBus.Instance.OnPlayerHealthChanged -= OnHealthChanged;
            GameEventBus.Instance.OnShieldActivated -= OnShieldActivated;
            GameEventBus.Instance.OnPowerUpCollected -= OnPowerUpCollected;
        }
    }
}
