using Godot;
using System.Collections.Generic;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de Notificaciones No Intrusivas
    /// 
    /// DISEÑO UX:
    /// - Las notificaciones aparecen en la esquina sin pausar el juego
    /// - Se apilan y desaparecen automáticamente
    /// - El jugador puede seguir jugando mientras lee
    /// - Diferentes prioridades: Info, Warning, Critical
    /// </summary>
    public partial class NonIntrusiveNotificationSystem : CanvasLayer
    {
        // ═══════════════════════════════════════════════════════════════════
        // CLASE HELPER (declarada primero para evitar errores)
        // ═══════════════════════════════════════════════════════════════════

        public enum NotificationType
        {
            Info,
            Warning,
            Critical,
            Wave,
            Learning
        }

        private class NotificationItem
        {
            public Panel Panel { get; set; }
            public Label Label { get; set; }
            public NotificationType Type { get; set; }
        }

        // ═══════════════════════════════════════════════════════════════════
        // SINGLETON Y CONSTANTES
        // ═══════════════════════════════════════════════════════════════════

        private static NonIntrusiveNotificationSystem _instance;
        public static NonIntrusiveNotificationSystem Instance => _instance;

        // Configuración
        private const float DEFAULT_DURATION = 3.5f;
        private const float FADE_DURATION = 0.4f;
        private const int MAX_NOTIFICATIONS = 4;
        private const float NOTIFICATION_SPACING = 8f;

        // Colores por tipo
        private static readonly Color INFO_COLOR = new Color("#00ff41");      // Verde terminal
        private static readonly Color WARNING_COLOR = new Color("#ffaa00");   // Naranja
        private static readonly Color CRITICAL_COLOR = new Color("#ff5555");  // Rojo
        private static readonly Color WAVE_COLOR = new Color("#bf00ff");      // Púrpura
        private static readonly Color LEARN_COLOR = new Color("#00d4ff");     // Cyan educativo

        // Contenedor de notificaciones
        private VBoxContainer _notificationContainer;
        private List<NotificationItem> _activeNotifications = new List<NotificationItem>();

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            // Asegurar que funcione durante pausa
            ProcessMode = ProcessModeEnum.Always;
            Layer = 100; // Encima de todo

            CreateUI();
            SubscribeToEvents();
        }

        private void CreateUI()
        {
            // Contenedor en la esquina superior derecha
            _notificationContainer = new VBoxContainer();
            _notificationContainer.Name = "NotificationContainer";
            _notificationContainer.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _notificationContainer.GrowHorizontal = Control.GrowDirection.Begin;
            _notificationContainer.OffsetRight = -15;
            _notificationContainer.OffsetTop = 80; // Debajo del score
            _notificationContainer.OffsetLeft = -350;
            _notificationContainer.AddThemeConstantOverride("separation", (int)NOTIFICATION_SPACING);
            AddChild(_notificationContainer);
        }

        private void SubscribeToEvents()
        {
            // Wave announcements - ahora son notificaciones no intrusivas
            GameEventBus.Instance.OnWaveAnnounced += OnWaveAnnounced;
            GameEventBus.Instance.OnWaveCompleted += OnWaveCompleted;
            GameEventBus.Instance.OnBossSpawned += OnBossSpawned;
            GameEventBus.Instance.OnSecurityTipShown += OnTipShown;
        }

        public override void _ExitTree()
        {
            if (GameEventBus.Instance != null)
            {
                GameEventBus.Instance.OnWaveAnnounced -= OnWaveAnnounced;
                GameEventBus.Instance.OnWaveCompleted -= OnWaveCompleted;
                GameEventBus.Instance.OnBossSpawned -= OnBossSpawned;
                GameEventBus.Instance.OnSecurityTipShown -= OnTipShown;
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // EVENTOS
        // ═══════════════════════════════════════════════════════════════════

        private void OnWaveAnnounced(int wave, string title, string desc)
        {
            // Solo mostrar número de wave, no descripción completa
            string icon = wave % 10 == 0 ? "👹" : (wave % 5 == 0 ? "⚠️" : "▶");
            ShowNotification($"{icon} WAVE {wave}", NotificationType.Wave, 2.5f);
        }

        private void OnWaveCompleted(int wave)
        {
            ShowNotification($"✓ Wave {wave} completada", NotificationType.Info, 2f);
        }

        private void OnBossSpawned(string bossName)
        {
            ShowNotification($"👹 ¡{bossName}!", NotificationType.Critical, 4f);
        }

        // Control anti-spam de tips
        private float _lastTipTime = 0f;
        private string _lastTipMessage = "";
        
        private void OnTipShown(string tip)
        {
            // Evitar tips duplicados o muy frecuentes
            float currentTime = Time.GetTicksMsec() / 1000f;
            if (tip == _lastTipMessage || currentTime - _lastTipTime < 3.0f)
            {
                return; // Ignorar duplicados o muy frecuentes
            }
            
            _lastTipTime = currentTime;
            _lastTipMessage = tip;
            
            // Solo mostrar tips cortos, no intrusivos
            if (tip.Length <= 50)
            {
                ShowNotification(tip, NotificationType.Info, 3f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Muestra una notificación no intrusiva
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info, float duration = DEFAULT_DURATION)
        {
            // Limitar cantidad de notificaciones
            while (_activeNotifications.Count >= MAX_NOTIFICATIONS)
            {
                RemoveOldestNotification();
            }

            var notification = CreateNotificationPanel(message, type);
            _notificationContainer.AddChild(notification.Panel);
            _activeNotifications.Add(notification);

            // Animar entrada
            AnimateIn(notification);

            // Programar salida
            var timer = GetTree().CreateTimer(duration);
            timer.Timeout += () => RemoveNotification(notification);
        }

        /// <summary>
        /// Muestra un tip de aprendizaje contextual (más destacado)
        /// </summary>
        public void ShowLearningTip(string concept, string tip)
        {
            ShowNotification($"💡 {concept}: {tip}", NotificationType.Learning, 5f);
        }

        /// <summary>
        /// Muestra feedback de respuesta de quiz
        /// </summary>
        public void ShowQuizFeedback(bool correct, string shortExplanation)
        {
            if (correct)
            {
                ShowNotification($"✓ ¡Correcto! {shortExplanation}", NotificationType.Info, 3f);
            }
            else
            {
                ShowNotification($"✗ {shortExplanation}", NotificationType.Warning, 4f);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // CREACIÓN DE UI
        // ═══════════════════════════════════════════════════════════════════

        private NotificationItem CreateNotificationPanel(string message, NotificationType type)
        {
            var panel = new Panel();
            panel.CustomMinimumSize = new Vector2(320, 0);

            // Estilo según tipo
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.02f, 0.02f, 0.02f, 0.85f);
            style.BorderColor = GetColorForType(type);
            style.BorderWidthLeft = 3;
            style.BorderWidthTop = 1;
            style.BorderWidthBottom = 1;
            style.BorderWidthRight = 1;
            style.SetCornerRadiusAll(4);
            style.ContentMarginLeft = 12;
            style.ContentMarginRight = 12;
            style.ContentMarginTop = 8;
            style.ContentMarginBottom = 8;
            panel.AddThemeStyleboxOverride("panel", style);

            // Contenido
            var label = new Label();
            label.Text = message;
            label.AutowrapMode = TextServer.AutowrapMode.Word;
            label.AddThemeColorOverride("font_color", GetColorForType(type));
            label.AddThemeFontSizeOverride("font_size", 14);
            panel.AddChild(label);

            // Posicionar label dentro del panel
            label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            label.OffsetLeft = 12;
            label.OffsetRight = -12;
            label.OffsetTop = 8;
            label.OffsetBottom = -8;

            return new NotificationItem { Panel = panel, Label = label, Type = type };
        }

        private Color GetColorForType(NotificationType type)
        {
            return type switch
            {
                NotificationType.Info => INFO_COLOR,
                NotificationType.Warning => WARNING_COLOR,
                NotificationType.Critical => CRITICAL_COLOR,
                NotificationType.Wave => WAVE_COLOR,
                NotificationType.Learning => LEARN_COLOR,
                _ => INFO_COLOR
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // ANIMACIONES
        // ═══════════════════════════════════════════════════════════════════

        private void AnimateIn(NotificationItem notification)
        {
            notification.Panel.Modulate = new Color(1, 1, 1, 0);
            notification.Panel.Position = new Vector2(50, notification.Panel.Position.Y);

            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(notification.Panel, "modulate:a", 1.0f, 0.2f);
            tween.TweenProperty(notification.Panel, "position:x", 0f, 0.25f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }

        private void AnimateOut(NotificationItem notification)
        {
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(notification.Panel, "modulate:a", 0.0f, FADE_DURATION);
            tween.TweenProperty(notification.Panel, "position:x", 50f, FADE_DURATION);
            tween.Chain().TweenCallback(Callable.From(() => {
                notification.Panel.QueueFree();
                _activeNotifications.Remove(notification);
            }));
        }

        private void RemoveNotification(NotificationItem notification)
        {
            if (_activeNotifications.Contains(notification))
            {
                AnimateOut(notification);
            }
        }

        private void RemoveOldestNotification()
        {
            if (_activeNotifications.Count > 0)
            {
                var oldest = _activeNotifications[0];
                AnimateOut(oldest);
            }
        }
    }
}
