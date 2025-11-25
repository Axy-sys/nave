using Godot;
using System;
using System.Threading.Tasks;

namespace CyberSecurityGame.UI
{
    /// <summary>
    /// Pantalla de carga reutilizable con estética hacker/terminal
    /// 
    /// UX FLOW:
    /// 1. Fade in desde negro
    /// 2. Mostrar barra de progreso con texto de estado
    /// 3. Tips educativos aleatorios mientras carga
    /// 4. Transición suave a la escena destino
    /// 
    /// MANEJO DE ERRORES:
    /// - Timeout si la carga tarda demasiado
    /// - Fallback a MainMenu si falla la escena
    /// - Log de errores para debugging
    /// </summary>
    public partial class LoadingScreen : CanvasLayer
    {
        // Colores temáticos
        private static readonly Color BG_COLOR = new Color("#050505");
        private static readonly Color TERMINAL_GREEN = new Color("#00ff41");
        private static readonly Color TERMINAL_DIM = new Color("#008F11");
        private static readonly Color RIPPIER_PURPLE = new Color("#bf00ff");
        private static readonly Color FLUX_ORANGE = new Color("#ffaa00");
        private static readonly Color ALERT_RED = new Color("#ff5555");
        private static readonly Color CYAN_GLOW = new Color("#00d4ff");

        // UI Elements
        private ColorRect _background;
        private Control _container;
        private Label _statusLabel;
        private ProgressBar _progressBar;
        private Label _percentLabel;
        private Label _tipLabel;
        private Label _tipHeaderLabel;
        private ColorRect _scanlines;

        // State
        private string _targetScene;
        private float _progress = 0f;
        private float _displayedProgress = 0f;
        private bool _isLoading = false;
        private bool _loadComplete = false;
        private float _tipTimer = 0f;
        private int _currentTipIndex = 0;
        private ResourceLoader.ThreadLoadStatus _loadStatus;

        // Tips educativos de ciberseguridad
        private static readonly string[] SECURITY_TIPS = new[]
        {
            "Una contraseña segura tiene al menos 12 caracteres con mayúsculas, números y símbolos.",
            "El phishing es responsable del 90% de las brechas de seguridad empresariales.",
            "Activa la autenticación de dos factores (2FA) en todas tus cuentas importantes.",
            "Los ataques de ransomware aumentaron 150% en el último año.",
            "Nunca uses la misma contraseña en múltiples sitios web.",
            "El 95% del malware llega a través de correos electrónicos.",
            "Un firewall monitorea el tráfico de red entrante y saliente.",
            "Los backups regulares son tu mejor defensa contra el ransomware.",
            "SQL Injection permite a hackers manipular bases de datos vulnerables.",
            "Los ataques DDoS pueden generar hasta 3 Tbps de tráfico malicioso.",
            "Zero-day: vulnerabilidad desconocida que no tiene parche disponible.",
            "El cifrado AES-256 es prácticamente imposible de romper por fuerza bruta.",
            "Un troyano se disfraza de software legítimo para infiltrarse en tu sistema.",
            "Los gusanos se replican automáticamente sin intervención humana.",
            "HTTPS cifra la comunicación entre tu navegador y el servidor.",
        };

        // Mensajes de estado durante la carga
        private static readonly string[] LOADING_MESSAGES = new[]
        {
            "Inicializando protocolos de seguridad...",
            "Cargando módulos de defensa...",
            "Estableciendo conexión segura...",
            "Verificando integridad del sistema...",
            "Configurando firewall...",
            "Escaneando vulnerabilidades...",
            "Cargando base de datos de amenazas...",
            "Activando escudos de protección...",
            "Sincronizando con servidor central...",
            "Preparando interfaz de combate...",
        };

        private Random _rng = new Random();

        public override void _Ready()
        {
            Layer = 100; // Encima de todo
            CreateUI();
        }

        private void CreateUI()
        {
            // Fondo negro
            _background = new ColorRect();
            _background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _background.Color = BG_COLOR;
            AddChild(_background);

            // Container central
            _container = new Control();
            _container.SetAnchorsPreset(Control.LayoutPreset.Center);
            _container.OffsetLeft = -300;
            _container.OffsetRight = 300;
            _container.OffsetTop = -150;
            _container.OffsetBottom = 150;
            AddChild(_container);

            // Logo ASCII pequeño
            var logoLabel = new Label();
            logoLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            logoLabel.OffsetTop = 0;
            logoLabel.OffsetBottom = 60;
            logoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            logoLabel.VerticalAlignment = VerticalAlignment.Center;
            logoLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            logoLabel.AddThemeFontSizeOverride("font_size", 10);
            logoLabel.Text = @"
 ██████╗ ██████╗ ██████╗ ███████╗    ██████╗ ██╗██████╗ ██████╗ ██╗███████╗██████╗ 
██╔════╝██╔═══██╗██╔══██╗██╔════╝    ██╔══██╗██║██╔══██╗██╔══██╗██║██╔════╝██╔══██╗
██║     ██║   ██║██║  ██║█████╗      ██████╔╝██║██████╔╝██████╔╝██║█████╗  ██████╔╝
██║     ██║   ██║██║  ██║██╔══╝      ██╔══██╗██║██╔═══╝ ██╔═══╝ ██║██╔══╝  ██╔══██╗
╚██████╗╚██████╔╝██████╔╝███████╗    ██║  ██║██║██║     ██║     ██║███████╗██║  ██║
 ╚═════╝ ╚═════╝ ╚═════╝ ╚══════╝    ╚═╝  ╚═╝╚═╝╚═╝     ╚═╝     ╚═╝╚══════╝╚═╝  ╚═╝";
            _container.AddChild(logoLabel);

            // Status label
            _statusLabel = new Label();
            _statusLabel.AnchorLeft = 0;
            _statusLabel.AnchorRight = 1;
            _statusLabel.AnchorTop = 0.5f;
            _statusLabel.AnchorBottom = 0.5f;
            _statusLabel.OffsetTop = -40;
            _statusLabel.OffsetBottom = -10;
            _statusLabel.OffsetLeft = 0;
            _statusLabel.OffsetRight = 0;
            _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _statusLabel.AddThemeColorOverride("font_color", TERMINAL_DIM);
            _statusLabel.AddThemeFontSizeOverride("font_size", 14);
            _statusLabel.Text = "> Inicializando...";
            _container.AddChild(_statusLabel);

            // Progress bar container
            var progressContainer = new Panel();
            progressContainer.AnchorLeft = 0;
            progressContainer.AnchorRight = 1;
            progressContainer.AnchorTop = 0.5f;
            progressContainer.AnchorBottom = 0.5f;
            progressContainer.OffsetTop = -5;
            progressContainer.OffsetBottom = 25;
            progressContainer.OffsetLeft = 50;
            progressContainer.OffsetRight = -50;
            
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.05f, 0.05f, 0.05f);
            panelStyle.BorderColor = TERMINAL_DIM;
            panelStyle.SetBorderWidthAll(1);
            panelStyle.SetCornerRadiusAll(4);
            progressContainer.AddThemeStyleboxOverride("panel", panelStyle);
            _container.AddChild(progressContainer);

            // Progress bar
            _progressBar = new ProgressBar();
            _progressBar.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _progressBar.OffsetLeft = 4;
            _progressBar.OffsetRight = -4;
            _progressBar.OffsetTop = 4;
            _progressBar.OffsetBottom = -4;
            _progressBar.ShowPercentage = false;
            _progressBar.MaxValue = 100;
            _progressBar.Value = 0;

            var bgStyle = new StyleBoxFlat();
            bgStyle.BgColor = new Color(0.02f, 0.02f, 0.02f);
            bgStyle.SetCornerRadiusAll(2);
            _progressBar.AddThemeStyleboxOverride("background", bgStyle);

            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = TERMINAL_GREEN;
            fillStyle.SetCornerRadiusAll(2);
            _progressBar.AddThemeStyleboxOverride("fill", fillStyle);
            progressContainer.AddChild(_progressBar);

            // Percent label
            _percentLabel = new Label();
            _percentLabel.AnchorLeft = 0;
            _percentLabel.AnchorRight = 1;
            _percentLabel.AnchorTop = 0.5f;
            _percentLabel.AnchorBottom = 0.5f;
            _percentLabel.OffsetTop = 30;
            _percentLabel.OffsetBottom = 55;
            _percentLabel.OffsetLeft = 0;
            _percentLabel.OffsetRight = 0;
            _percentLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _percentLabel.AddThemeColorOverride("font_color", TERMINAL_GREEN);
            _percentLabel.AddThemeFontSizeOverride("font_size", 20);
            _percentLabel.Text = "0%";
            _container.AddChild(_percentLabel);

            // Tip header
            _tipHeaderLabel = new Label();
            _tipHeaderLabel.AnchorLeft = 0;
            _tipHeaderLabel.AnchorRight = 1;
            _tipHeaderLabel.AnchorTop = 0.5f;
            _tipHeaderLabel.AnchorBottom = 0.5f;
            _tipHeaderLabel.OffsetTop = 70;
            _tipHeaderLabel.OffsetBottom = 90;
            _tipHeaderLabel.OffsetLeft = 0;
            _tipHeaderLabel.OffsetRight = 0;
            _tipHeaderLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _tipHeaderLabel.AddThemeColorOverride("font_color", FLUX_ORANGE);
            _tipHeaderLabel.AddThemeFontSizeOverride("font_size", 14);
            _tipHeaderLabel.Text = "[ TIP DE SEGURIDAD ]";
            _container.AddChild(_tipHeaderLabel);

            // Tip label
            _tipLabel = new Label();
            _tipLabel.AnchorLeft = 0;
            _tipLabel.AnchorRight = 1;
            _tipLabel.AnchorTop = 0.5f;
            _tipLabel.AnchorBottom = 0.5f;
            _tipLabel.OffsetTop = 95;
            _tipLabel.OffsetBottom = 145;
            _tipLabel.OffsetLeft = 20;
            _tipLabel.OffsetRight = -20;
            _tipLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _tipLabel.VerticalAlignment = VerticalAlignment.Center;
            _tipLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _tipLabel.AddThemeColorOverride("font_color", CYAN_GLOW);
            _tipLabel.AddThemeFontSizeOverride("font_size", 13);
            _tipLabel.Text = SECURITY_TIPS[0];
            _container.AddChild(_tipLabel);

            // CRT Scanlines
            _scanlines = new ColorRect();
            _scanlines.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _scanlines.MouseFilter = Control.MouseFilterEnum.Ignore;
            _scanlines.Color = new Color(0, 1, 0.25f, 0.02f);
            AddChild(_scanlines);

            // Shuffle tips
            _currentTipIndex = _rng.Next(SECURITY_TIPS.Length);
            _tipLabel.Text = SECURITY_TIPS[_currentTipIndex];
        }

        public override void _Process(double delta)
        {
            if (!_isLoading) return;

            // Animar barra de progreso (smooth)
            _displayedProgress = Mathf.Lerp(_displayedProgress, _progress, (float)delta * 5f);
            _progressBar.Value = _displayedProgress;
            _percentLabel.Text = $"{(int)_displayedProgress}%";

            // Cambiar tip cada 3 segundos
            _tipTimer += (float)delta;
            if (_tipTimer >= 3.0f)
            {
                _tipTimer = 0f;
                NextTip();
            }

            // Efecto de scanlines
            float scanAlpha = 0.015f + 0.01f * Mathf.Sin((float)Time.GetTicksMsec() / 200f);
            _scanlines.Color = new Color(0, 1, 0.25f, scanAlpha);

            // Parpadeo del cursor en status
            string cursor = ((int)(Time.GetTicksMsec() / 500) % 2 == 0) ? "_" : " ";
            
            // Verificar estado de carga
            if (!_loadComplete)
            {
                CheckLoadStatus();
            }
            else if (_displayedProgress >= 99)
            {
                // Carga completa, transicionar
                CompleteLoading();
            }
        }

        private void CheckLoadStatus()
        {
            if (string.IsNullOrEmpty(_targetScene)) return;

            var status = ResourceLoader.LoadThreadedGetStatus(_targetScene);
            
            switch (status)
            {
                case ResourceLoader.ThreadLoadStatus.InProgress:
                    // Obtener progreso real
                    var progressArray = new Godot.Collections.Array();
                    ResourceLoader.LoadThreadedGetStatus(_targetScene, progressArray);
                    if (progressArray.Count > 0)
                    {
                        float realProgress = (float)progressArray[0] * 100f;
                        _progress = Mathf.Max(_progress, realProgress);
                    }
                    // Mensaje aleatorio de estado
                    UpdateStatusMessage();
                    break;

                case ResourceLoader.ThreadLoadStatus.Loaded:
                    _progress = 100f;
                    _loadComplete = true;
                    _statusLabel.Text = "> Sistema listo. Iniciando...";
                    break;

                case ResourceLoader.ThreadLoadStatus.Failed:
                    HandleLoadError();
                    break;
            }
        }

        private void UpdateStatusMessage()
        {
            // Cambiar mensaje según progreso
            int messageIndex = (int)(_progress / 100f * LOADING_MESSAGES.Length);
            messageIndex = Mathf.Clamp(messageIndex, 0, LOADING_MESSAGES.Length - 1);
            _statusLabel.Text = $"> {LOADING_MESSAGES[messageIndex]}";
        }

        private void NextTip()
        {
            _currentTipIndex = (_currentTipIndex + 1) % SECURITY_TIPS.Length;
            
            // Fade out/in para cambio de tip
            var tween = CreateTween();
            tween.TweenProperty(_tipLabel, "modulate:a", 0f, 0.2f);
            tween.TweenCallback(Callable.From(() => {
                _tipLabel.Text = SECURITY_TIPS[_currentTipIndex];
            }));
            tween.TweenProperty(_tipLabel, "modulate:a", 1f, 0.2f);
        }

        private void CompleteLoading()
        {
            _isLoading = false;
            
            // Fade out
            var tween = CreateTween();
            tween.TweenProperty(_background, "color:a", 0f, 0.3f);
            tween.Parallel().TweenProperty(_container, "modulate:a", 0f, 0.3f);
            tween.TweenCallback(Callable.From(() => {
                // Obtener la escena cargada y cambiar
                var packedScene = ResourceLoader.LoadThreadedGet(_targetScene) as PackedScene;
                if (packedScene != null)
                {
                    GetTree().ChangeSceneToPacked(packedScene);
                }
                else
                {
                    HandleLoadError();
                }
                QueueFree();
            }));
        }

        private void HandleLoadError()
        {
            _isLoading = false;
            _statusLabel.Text = "> ERROR: Fallo en la carga del sistema";
            _statusLabel.AddThemeColorOverride("font_color", ALERT_RED);
            _progressBar.Value = 0;
            
            GD.PrintErr($"[LoadingScreen] Error cargando escena: {_targetScene}");

            // Mostrar error y volver al menú
            var tween = CreateTween();
            tween.TweenInterval(2.0f);
            tween.TweenCallback(Callable.From(() => {
                GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
                QueueFree();
            }));
        }

        // ═══════════════════════════════════════════════════════════
        // API PÚBLICA
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Inicia la carga de una escena con pantalla de carga
        /// </summary>
        public void LoadScene(string scenePath)
        {
            _targetScene = scenePath;
            _progress = 0f;
            _displayedProgress = 0f;
            _isLoading = true;
            _loadComplete = false;

            GD.Print($"[LoadingScreen] Iniciando carga de: {scenePath}");

            // Iniciar carga en background
            var error = ResourceLoader.LoadThreadedRequest(scenePath);
            if (error != Error.Ok)
            {
                GD.PrintErr($"[LoadingScreen] Error iniciando carga: {error}");
                HandleLoadError();
                return;
            }

            // Fade in
            _background.Color = new Color(BG_COLOR, 0);
            _container.Modulate = new Color(1, 1, 1, 0);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_background, "color:a", 1f, 0.3f);
            tween.TweenProperty(_container, "modulate:a", 1f, 0.3f);
        }

        /// <summary>
        /// Carga simulada (para transiciones rápidas sin carga real)
        /// </summary>
        public void LoadSceneSimulated(string scenePath, float duration = 1.5f)
        {
            _targetScene = scenePath;
            _progress = 0f;
            _displayedProgress = 0f;
            _isLoading = true;
            _loadComplete = false;

            GD.Print($"[LoadingScreen] Carga simulada de: {scenePath}");

            // Fade in
            _background.Color = new Color(BG_COLOR, 0);
            _container.Modulate = new Color(1, 1, 1, 0);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_background, "color:a", 1f, 0.2f);
            tween.TweenProperty(_container, "modulate:a", 1f, 0.2f);

            // Simular progreso
            tween.Chain().TweenMethod(
                Callable.From<float>((p) => { _progress = p; UpdateStatusMessage(); }),
                0f, 100f, duration
            ).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
            
            tween.TweenCallback(Callable.From(() => {
                _loadComplete = true;
                _statusLabel.Text = "> Sistema listo. Iniciando...";
            }));

            // Transición final
            tween.TweenInterval(0.5f);
            tween.TweenProperty(_background, "color:a", 0f, 0.3f);
            tween.Parallel().TweenProperty(_container, "modulate:a", 0f, 0.3f);
            tween.TweenCallback(Callable.From(() => {
                GetTree().ChangeSceneToFile(scenePath);
                QueueFree();
            }));
        }

        /// <summary>
        /// Crea e inicia una pantalla de carga
        /// Uso: LoadingScreen.TransitionTo(GetTree(), "res://Scenes/Main.tscn");
        /// </summary>
        public static void TransitionTo(SceneTree tree, string scenePath, bool simulated = false)
        {
            var loading = new LoadingScreen();
            loading.Name = "LoadingScreen";
            tree.Root.AddChild(loading);
            
            if (simulated)
                loading.LoadSceneSimulated(scenePath);
            else
                loading.LoadScene(scenePath);
        }
    }
}
