using Godot;
using System.Collections.Generic;
using CyberSecurityGame.Education;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Views
{
    /// <summary>
    /// Vista de la Enciclopedia de Amenazas
    /// 
    /// UX DESIGN:
    /// - Accesible desde el HUD con [TAB] o botón
    /// - Muestra progreso de descubrimiento
    /// - Cada amenaza tiene niveles de desbloqueo visual
    /// - Incentiva al jugador a "completar" la enciclopedia
    /// 
    /// INSPIRADO EN:
    /// - Pokédex (Pokémon) - Siluetas de no descubiertos
    /// - Codex (Mass Effect) - Pestañas organizadas
    /// - Bestiary (Witcher 3) - Info de combate
    /// </summary>
    public partial class EncyclopediaView : CanvasLayer
    {
        // Colores del tema
        private static readonly Color BG_COLOR = new Color("#050505");
        private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
        private static readonly Color TERMINAL_DIM = new Color("#008F11");
        private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
        private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
        private static readonly Color ALERT_RED = new Color("#ff5555");
        private static readonly Color LOCKED_GRAY = new Color("#333333");

        // UI Elements
        private Panel _mainPanel;
        private VBoxContainer _threatList;
        private Panel _detailPanel;
        private Label _titleLabel;
        private Label _progressLabel;
        private Label _detailTitle;
        private Label _detailDescription;
        private Label _detailDefense;
        private Label _detailLore;
        private ProgressBar _knowledgeBar;
        private Button _closeButton;
        
        // Estado
        private ThreatEntry _selectedThreat;
        private bool _isVisible = false;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            CreateUI();
            Visible = false;
            
            // Suscribirse a eventos de descubrimiento
            if (ThreatEncyclopedia.Instance != null)
            {
                ThreatEncyclopedia.Instance.ThreatDiscovered += OnThreatDiscovered;
                ThreatEncyclopedia.Instance.ThreatLevelUp += OnThreatLevelUp;
            }
        }

        private void CreateUI()
        {
            // ═══════════════════════════════════════════════════════════════════
            // PANEL PRINCIPAL
            // ═══════════════════════════════════════════════════════════════════
            
            var centerContainer = new CenterContainer();
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(centerContainer);

            _mainPanel = new Panel();
            _mainPanel.CustomMinimumSize = new Vector2(900, 600);
            
            var mainStyle = new StyleBoxFlat();
            mainStyle.BgColor = new Color(BG_COLOR, 0.98f);
            mainStyle.BorderColor = RIPPIER_PURPLE;
            mainStyle.SetBorderWidthAll(2);
            mainStyle.SetCornerRadiusAll(8);
            mainStyle.ShadowColor = new Color(RIPPIER_PURPLE, 0.4f);
            mainStyle.ShadowSize = 20;
            _mainPanel.AddThemeStyleboxOverride("panel", mainStyle);
            centerContainer.AddChild(_mainPanel);

            // ═══════════════════════════════════════════════════════════════════
            // HEADER
            // ═══════════════════════════════════════════════════════════════════
            
            var header = new Panel();
            header.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            header.OffsetBottom = 60;
            var headerStyle = new StyleBoxFlat();
            headerStyle.BgColor = new Color(RIPPIER_PURPLE, 0.15f);
            headerStyle.BorderColor = RIPPIER_PURPLE;
            headerStyle.BorderWidthBottom = 2;
            header.AddThemeStyleboxOverride("panel", headerStyle);
            _mainPanel.AddChild(header);

            // Título
            _titleLabel = new Label();
            _titleLabel.Text = "📚 THREAT ENCYCLOPEDIA";
            _titleLabel.Position = new Vector2(30, 15);
            _titleLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            header.AddChild(_titleLabel);

            // Progreso
            _progressLabel = new Label();
            _progressLabel.Text = "DISCOVERED: 0/8 | MASTERED: 0/8";
            _progressLabel.Position = new Vector2(30, 42);
            _progressLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _progressLabel.AddThemeFontSizeOverride("font_size", 14);
            header.AddChild(_progressLabel);

            // Botón cerrar
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.Position = new Vector2(850, 15);
            _closeButton.CustomMinimumSize = new Vector2(35, 35);
            _closeButton.AddThemeColorOverride("font_color", ALERT_RED);
            _closeButton.AddThemeFontSizeOverride("font_size", 20);
            _closeButton.Pressed += Hide;
            
            var closeStyle = new StyleBoxFlat();
            closeStyle.BgColor = Colors.Transparent;
            _closeButton.AddThemeStyleboxOverride("normal", closeStyle);
            var closeHover = new StyleBoxFlat();
            closeHover.BgColor = new Color(ALERT_RED, 0.3f);
            closeHover.SetCornerRadiusAll(4);
            _closeButton.AddThemeStyleboxOverride("hover", closeHover);
            header.AddChild(_closeButton);

            // ═══════════════════════════════════════════════════════════════════
            // CONTENIDO - Dos columnas
            // ═══════════════════════════════════════════════════════════════════
            
            var content = new HBoxContainer();
            content.Position = new Vector2(0, 70);
            content.Size = new Vector2(900, 520);
            content.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(content);

            // ═══════════════════════════════════════════════════════════════════
            // LISTA DE AMENAZAS (izquierda)
            // ═══════════════════════════════════════════════════════════════════
            
            var listContainer = new Panel();
            listContainer.CustomMinimumSize = new Vector2(280, 510);
            var listStyle = new StyleBoxFlat();
            listStyle.BgColor = new Color(0.02f, 0.02f, 0.02f);
            listStyle.BorderColor = TERMINAL_DIM;
            listStyle.SetBorderWidthAll(1);
            listStyle.SetCornerRadiusAll(4);
            listContainer.AddThemeStyleboxOverride("panel", listStyle);
            content.AddChild(listContainer);

            var scrollContainer = new ScrollContainer();
            scrollContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            scrollContainer.OffsetLeft = 5;
            scrollContainer.OffsetTop = 5;
            scrollContainer.OffsetRight = -5;
            scrollContainer.OffsetBottom = -5;
            listContainer.AddChild(scrollContainer);

            _threatList = new VBoxContainer();
            _threatList.AddThemeConstantOverride("separation", 5);
            scrollContainer.AddChild(_threatList);

            // ═══════════════════════════════════════════════════════════════════
            // PANEL DE DETALLE (derecha)
            // ═══════════════════════════════════════════════════════════════════
            
            _detailPanel = new Panel();
            _detailPanel.CustomMinimumSize = new Vector2(590, 510);
            _detailPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            var detailStyle = new StyleBoxFlat();
            detailStyle.BgColor = new Color(0.02f, 0.02f, 0.02f);
            detailStyle.BorderColor = TERMINAL_DIM;
            detailStyle.SetBorderWidthAll(1);
            detailStyle.SetCornerRadiusAll(4);
            _detailPanel.AddThemeStyleboxOverride("panel", detailStyle);
            content.AddChild(_detailPanel);

            var detailContent = new VBoxContainer();
            detailContent.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            detailContent.OffsetLeft = 20;
            detailContent.OffsetTop = 15;
            detailContent.OffsetRight = -20;
            detailContent.OffsetBottom = -15;
            detailContent.AddThemeConstantOverride("separation", 12);
            _detailPanel.AddChild(detailContent);

            // Título del detalle
            _detailTitle = new Label();
            _detailTitle.Text = "Selecciona una amenaza";
            _detailTitle.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _detailTitle.AddThemeFontSizeOverride("font_size", 24);
            detailContent.AddChild(_detailTitle);

            // Barra de conocimiento
            var knowledgeContainer = new HBoxContainer();
            knowledgeContainer.AddThemeConstantOverride("separation", 10);
            detailContent.AddChild(knowledgeContainer);

            var knowledgeLabel = new Label();
            knowledgeLabel.Text = "KNOWLEDGE:";
            knowledgeLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            knowledgeLabel.AddThemeFontSizeOverride("font_size", 12);
            knowledgeContainer.AddChild(knowledgeLabel);

            _knowledgeBar = new ProgressBar();
            _knowledgeBar.CustomMinimumSize = new Vector2(200, 16);
            _knowledgeBar.ShowPercentage = false;
            _knowledgeBar.MaxValue = 3;
            _knowledgeBar.Value = 0;
            
            var kbBgStyle = new StyleBoxFlat();
            kbBgStyle.BgColor = new Color(0.1f, 0.1f, 0.1f);
            kbBgStyle.SetCornerRadiusAll(4);
            _knowledgeBar.AddThemeStyleboxOverride("background", kbBgStyle);
            
            var kbFillStyle = new StyleBoxFlat();
            kbFillStyle.BgColor = TERMINAL_GREEN;
            kbFillStyle.SetCornerRadiusAll(4);
            _knowledgeBar.AddThemeStyleboxOverride("fill", kbFillStyle);
            knowledgeContainer.AddChild(_knowledgeBar);

            // Separador
            var sep = new ColorRect();
            sep.CustomMinimumSize = new Vector2(0, 1);
            sep.Color = new Color(TERMINAL_DIM, 0.5f);
            detailContent.AddChild(sep);

            // Descripción
            _detailDescription = new Label();
            _detailDescription.Text = "";
            _detailDescription.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailDescription.CustomMinimumSize = new Vector2(540, 100);
            _detailDescription.AddThemeColorOverride("font_color", Colors.White);
            _detailDescription.AddThemeFontSizeOverride("font_size", 15);
            detailContent.AddChild(_detailDescription);

            // Cómo defenderse
            _detailDefense = new Label();
            _detailDefense.Text = "";
            _detailDefense.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailDefense.CustomMinimumSize = new Vector2(540, 80);
            _detailDefense.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _detailDefense.AddThemeFontSizeOverride("font_size", 14);
            detailContent.AddChild(_detailDefense);

            // Lore
            _detailLore = new Label();
            _detailLore.Text = "";
            _detailLore.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailLore.CustomMinimumSize = new Vector2(540, 100);
            _detailLore.AddThemeColorOverride("font_color", TERMINAL_DIM);
            _detailLore.AddThemeFontSizeOverride("font_size", 13);
            detailContent.AddChild(_detailLore);

            // Mensaje de instrucciones inicial
            ShowEmptyState();
        }

        /// <summary>
        /// Actualiza la lista de amenazas
        /// </summary>
        public void RefreshThreatList()
        {
            if (ThreatEncyclopedia.Instance == null) return;

            // Limpiar lista actual
            foreach (Node child in _threatList.GetChildren())
            {
                child.QueueFree();
            }

            var allThreats = ThreatEncyclopedia.Instance.GetAllThreats();
            int discovered = 0;
            int mastered = 0;

            foreach (var threat in allThreats)
            {
                var button = CreateThreatButton(threat);
                _threatList.AddChild(button);

                if (threat.IsDiscovered)
                {
                    discovered++;
                    if (threat.KnowledgeLevel >= 3) mastered++;
                }
            }

            // Actualizar progreso
            _progressLabel.Text = $"DISCOVERED: {discovered}/{allThreats.Count} | MASTERED: {mastered}/{allThreats.Count}";
        }

        private Button CreateThreatButton(ThreatEntry threat)
        {
            var button = new Button();
            button.CustomMinimumSize = new Vector2(260, 50);
            button.Alignment = HorizontalAlignment.Left;
            
            if (threat.IsDiscovered)
            {
                button.Text = $"  {threat.Icon} {threat.Name}";
                button.AddThemeColorOverride("font_color", TERMINAL_GREEN);
                
                // Indicador de nivel
                string levelIndicator = threat.KnowledgeLevel switch
                {
                    1 => " ●○○",
                    2 => " ●●○",
                    3 => " ●●●",
                    _ => " ○○○"
                };
                button.Text += levelIndicator;
            }
            else
            {
                button.Text = "  ??? UNKNOWN THREAT";
                button.AddThemeColorOverride("font_color", LOCKED_GRAY);
            }

            button.AddThemeFontSizeOverride("font_size", 14);

            // Estilos
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = threat.IsDiscovered 
                ? new Color(TERMINAL_GREEN, 0.05f) 
                : new Color(0.05f, 0.05f, 0.05f);
            normalStyle.BorderColor = threat.IsDiscovered ? TERMINAL_DIM : LOCKED_GRAY;
            normalStyle.SetBorderWidthAll(1);
            normalStyle.SetCornerRadiusAll(4);
            button.AddThemeStyleboxOverride("normal", normalStyle);

            var hoverStyle = new StyleBoxFlat();
            hoverStyle.BgColor = threat.IsDiscovered 
                ? new Color(TERMINAL_GREEN, 0.15f) 
                : new Color(0.1f, 0.1f, 0.1f);
            hoverStyle.BorderColor = threat.IsDiscovered ? TERMINAL_GREEN : LOCKED_GRAY;
            hoverStyle.SetBorderWidthAll(1);
            hoverStyle.SetCornerRadiusAll(4);
            button.AddThemeStyleboxOverride("hover", hoverStyle);

            // Evento click
            button.Pressed += () => SelectThreat(threat);

            return button;
        }

        private void SelectThreat(ThreatEntry threat)
        {
            _selectedThreat = threat;

            if (!threat.IsDiscovered)
            {
                _detailTitle.Text = "??? UNKNOWN THREAT";
                _detailTitle.AddThemeColorOverride("font_color", LOCKED_GRAY);
                _knowledgeBar.Value = 0;
                _detailDescription.Text = "Esta amenaza aún no ha sido descubierta.\n\n¡Encuéntrala en el juego para desbloquear información!";
                _detailDefense.Text = "";
                _detailLore.Text = "";
                return;
            }

            // Título con icono
            _detailTitle.Text = $"{threat.Icon} {threat.Name}";
            _detailTitle.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            
            // Barra de conocimiento
            _knowledgeBar.Value = threat.KnowledgeLevel;

            // Descripción según nivel
            switch (threat.KnowledgeLevel)
            {
                case 1:
                    _detailDescription.Text = threat.ShortDescription;
                    _detailDefense.Text = "🔒 Responde quizzes para desbloquear más información";
                    _detailLore.Text = "";
                    break;
                case 2:
                    _detailDescription.Text = threat.FullDescription;
                    _detailDefense.Text = $"🛡️ DEFENSA:\n{threat.HowToDefend}";
                    _detailLore.Text = "🔒 Responde más quizzes para desbloquear el lore completo";
                    break;
                case 3:
                    _detailDescription.Text = threat.FullDescription;
                    _detailDefense.Text = $"🛡️ DEFENSA:\n{threat.HowToDefend}";
                    _detailLore.Text = $"📖 HISTORIA:\n{threat.DeepLore}";
                    break;
                default:
                    _detailDescription.Text = threat.ShortDescription;
                    _detailDefense.Text = "";
                    _detailLore.Text = "";
                    break;
            }
        }

        private void ShowEmptyState()
        {
            _detailTitle.Text = "🎯 SELECCIONA UNA AMENAZA";
            _detailTitle.AddThemeColorOverride("font_color", TERMINAL_DIM);
            _knowledgeBar.Value = 0;
            _detailDescription.Text = "Haz clic en una amenaza de la lista para ver sus detalles.\n\nDerrota enemigos para descubrir nuevas amenazas.\nResponde quizzes correctamente para subir tu nivel de conocimiento.";
            _detailDefense.Text = "";
            _detailLore.Text = "💡 Tip: Presiona E durante el juego para abrir esta enciclopedia.";
        }

        // ═══════════════════════════════════════════════════════════════════
        // EVENTOS
        // ═══════════════════════════════════════════════════════════════════

        private void OnThreatDiscovered(string threatId, string threatName)
        {
            RefreshThreatList();
        }

        private void OnThreatLevelUp(string threatId, int newLevel)
        {
            RefreshThreatList();
            
            // Si estamos viendo esta amenaza, actualizar detalle
            if (_selectedThreat?.Id == threatId)
            {
                SelectThreat(_selectedThreat);
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // VISIBILIDAD
        // ═══════════════════════════════════════════════════════════════════

        public new void Show()
        {
            _isVisible = true;
            Visible = true;
            GetTree().Paused = true;
            RefreshThreatList();
            
            // Animación de entrada
            _mainPanel.Scale = new Vector2(0.9f, 0.9f);
            _mainPanel.Modulate = new Color(1, 1, 1, 0);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_mainPanel, "scale", Vector2.One, 0.2f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.15f);
        }

        public new void Hide()
        {
            _isVisible = false;
            
            var tween = CreateTween();
            tween.TweenProperty(_mainPanel, "modulate:a", 0f, 0.1f);
            tween.TweenCallback(Callable.From(() => {
                Visible = false;
                GetTree().Paused = false;
            }));
        }

        public void Toggle()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        public override void _Input(InputEvent @event)
        {
            if (!_isVisible) return;

            // ESC para cerrar
            if (@event.IsActionPressed("ui_cancel"))
            {
                Hide();
                GetViewport().SetInputAsHandled();
            }
        }

        public override void _ExitTree()
        {
            if (ThreatEncyclopedia.Instance != null)
            {
                ThreatEncyclopedia.Instance.ThreatDiscovered -= OnThreatDiscovered;
                ThreatEncyclopedia.Instance.ThreatLevelUp -= OnThreatLevelUp;
            }
        }
    }
}
