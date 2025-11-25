using Godot;
using CyberSecurityGame.Education;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Views
{
    /// <summary>
    /// Vista para mostrar preguntas de quiz durante el juego
    /// 
    /// MEJORA UX: Ahora incluye "pistas educativas" que ayudan al usuario
    /// a aprender mientras responde, no solo evaluar conocimiento.
    /// </summary>
    public partial class QuizView : CanvasLayer
    {
        private Panel _quizPanel;
        private Label _questionLabel;
        private Label _contextLabel;
        private Label _hintLabel;        // NUEVO: Pista educativa
        private VBoxContainer _answersContainer;
        private Label _explanationLabel;
        private Timer _hideTimer;
        
        private QuizQuestion _currentQuestion;
        private int _selectedAnswer = -1;

        public override void _Ready()
        {
            // Asegurar que la UI funcione cuando el juego está pausado
            ProcessMode = Node.ProcessModeEnum.Always;
            
            InitializeUI();
            Visible = false;
        }

        private void InitializeUI()
        {
            // Contenedor para centrar el panel automáticamente
            var centerContainer = new CenterContainer();
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(centerContainer);

            // Panel principal con estilo de Terminal Hacker
            _quizPanel = new Panel();
            _quizPanel.Name = "QuizPanel";
            _quizPanel.CustomMinimumSize = new Vector2(750, 580); // Más grande para incluir pista
            
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.02f, 0.02f, 0.02f, 0.98f); // Negro profundo
            panelStyle.BorderColor = new Color("bf00ff"); // Rippier Purple
            panelStyle.SetBorderWidthAll(2);
            panelStyle.SetCornerRadiusAll(5);
            panelStyle.ShadowColor = new Color("bf00ff"); // Glow Purple
            panelStyle.ShadowSize = 20;
            _quizPanel.AddThemeStyleboxOverride("panel", panelStyle);
            
            centerContainer.AddChild(_quizPanel);

            // Barra de título del terminal
            var titleBar = new Panel();
            titleBar.Size = new Vector2(700, 40);
            titleBar.Position = Vector2.Zero;
            var titleBarStyle = new StyleBoxFlat();
            titleBarStyle.BgColor = new Color("bf00ff"); // Purple transparent
            titleBarStyle.BgColor = new Color(titleBarStyle.BgColor.R, titleBarStyle.BgColor.G, titleBarStyle.BgColor.B, 0.2f);
            titleBarStyle.BorderColor = new Color("bf00ff");
            titleBarStyle.BorderWidthBottom = 2;
            titleBar.AddThemeStyleboxOverride("panel", titleBarStyle);
            _quizPanel.AddChild(titleBar);

            // Título de Alerta
            var titleLabel = new Label();
            titleLabel.Text = "> SYSTEM_BREACH_DETECTED // SECURITY_CHALLENGE";
            titleLabel.Position = new Vector2(20, 0);
            titleLabel.Size = new Vector2(660, 40);
            titleLabel.VerticalAlignment = VerticalAlignment.Center;
            titleLabel.AddThemeColorOverride("font_color", new Color("00ff41")); // Terminal Green
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleBar.AddChild(titleLabel);

            // Contexto (Situación)
            _contextLabel = new Label();
            _contextLabel.Name = "ContextLabel";
            _contextLabel.Position = new Vector2(40, 50);
            _contextLabel.Size = new Vector2(670, 40);
            _contextLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _contextLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _contextLabel.AddThemeColorOverride("font_color", new Color("bf00ff")); // Rippier Purple
            _contextLabel.AddThemeFontSizeOverride("font_size", 14);
            _quizPanel.AddChild(_contextLabel);

            // NUEVO: Pista educativa - ayuda al usuario a responder
            _hintLabel = new Label();
            _hintLabel.Name = "HintLabel";
            _hintLabel.Position = new Vector2(40, 90);
            _hintLabel.Size = new Vector2(670, 50);
            _hintLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _hintLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _hintLabel.AddThemeColorOverride("font_color", new Color("00d4ff")); // Cyan educativo
            _hintLabel.AddThemeFontSizeOverride("font_size", 13);
            _quizPanel.AddChild(_hintLabel);

            // Pregunta
            _questionLabel = new Label();
            _questionLabel.Name = "QuestionLabel";
            _questionLabel.Position = new Vector2(40, 145);
            _questionLabel.Size = new Vector2(670, 60);
            _questionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _questionLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _questionLabel.AddThemeColorOverride("font_color", new Color("00ff41")); // Terminal Green
            _questionLabel.AddThemeFontSizeOverride("font_size", 20);
            _quizPanel.AddChild(_questionLabel);

            // Contenedor de respuestas
            _answersContainer = new VBoxContainer();
            _answersContainer.Name = "AnswersContainer";
            _answersContainer.Position = new Vector2(50, 215);
            _answersContainer.Size = new Vector2(650, 240);
            _answersContainer.AddThemeConstantOverride("separation", 12);
            _quizPanel.AddChild(_answersContainer);

            // Explicación (aparece después de responder)
            _explanationLabel = new Label();
            _explanationLabel.Name = "ExplanationLabel";
            _explanationLabel.Position = new Vector2(40, 470);
            _explanationLabel.Size = new Vector2(670, 100);
            _explanationLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _explanationLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _explanationLabel.AddThemeColorOverride("font_color", new Color("ffaa00")); // Flux Orange
            _explanationLabel.AddThemeFontSizeOverride("font_size", 14);
            _explanationLabel.Visible = false;
            _quizPanel.AddChild(_explanationLabel);

            // Timer para ocultar
            _hideTimer = new Timer();
            _hideTimer.Name = "HideTimer";
            _hideTimer.WaitTime = 4.0;
            _hideTimer.OneShot = true;
            _hideTimer.Timeout += HideQuiz;
            AddChild(_hideTimer);
        }

        public void ShowQuestion(QuizQuestion question)
        {
            ShowQuestionWithContext(question, question.Context);
        }
        
        /// <summary>
        /// Muestra una pregunta con un contexto personalizado
        /// Usado por ContextualLearningSystem para mensajes contextuales
        /// 
        /// MEJORA UX: Ahora incluye una pista educativa que ayuda al usuario
        /// a aprender el concepto, no solo a adivinar la respuesta.
        /// </summary>
        public void ShowQuestionWithContext(QuizQuestion question, string customContext)
        {
            _currentQuestion = question;
            _selectedAnswer = -1;
            
            Visible = true;
            GetTree().Paused = true;

            _contextLabel.Text = customContext;
            _questionLabel.Text = question.Question;
            _explanationLabel.Visible = false;
            
            // NUEVO: Mostrar pista educativa que ayuda a responder
            _hintLabel.Text = GetEducationalHint(question);
            _hintLabel.Visible = true;

            // Limpiar respuestas anteriores
            foreach (Node child in _answersContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Crear botones de respuesta con estilo Terminal y números
            for (int i = 0; i < question.Answers.Length; i++)
            {
                Button answerButton = new Button();
                answerButton.Text = $"[{i + 1}] {question.Answers[i]}"; // Número para tecla rápida
                answerButton.CustomMinimumSize = new Vector2(640, 42);
                answerButton.Alignment = HorizontalAlignment.Left;
                answerButton.AddThemeColorOverride("font_color", new Color("00ff41"));
                answerButton.AddThemeColorOverride("font_hover_color", Colors.Black);
                answerButton.AddThemeFontSizeOverride("font_size", 16);
                
                var normalStyle = new StyleBoxFlat();
                normalStyle.BgColor = new Color(0, 0, 0, 0.5f);
                normalStyle.BorderColor = new Color("bf00ff");
                normalStyle.SetBorderWidthAll(1);
                normalStyle.SetCornerRadiusAll(5);
                answerButton.AddThemeStyleboxOverride("normal", normalStyle);
                
                var hoverStyle = new StyleBoxFlat();
                hoverStyle.BgColor = new Color("00ff41");
                hoverStyle.BorderColor = new Color("00ff41");
                hoverStyle.SetBorderWidthAll(1);
                hoverStyle.SetCornerRadiusAll(5);
                answerButton.AddThemeStyleboxOverride("hover", hoverStyle);

                var pressedStyle = new StyleBoxFlat();
                pressedStyle.BgColor = new Color(0, 0.8f, 0);
                answerButton.AddThemeStyleboxOverride("pressed", pressedStyle);

                int index = i;
                answerButton.Pressed += () => OnAnswerSelected(index);
                _answersContainer.AddChild(answerButton);
            }
        }
        
        /// <summary>
        /// Genera una pista educativa basada en la categoría de la pregunta.
        /// UX: El usuario aprende mientras responde, no solo es evaluado.
        /// </summary>
        private string GetEducationalHint(QuizQuestion question)
        {
            return question.Category switch
            {
                QuizCategory.Malware => "💡 PISTA: El malware es software diseñado para dañar. Incluye virus, troyanos, ransomware y gusanos.",
                QuizCategory.Phishing => "💡 PISTA: El phishing intenta engañarte para robar información. Siempre verifica el remitente y los enlaces.",
                QuizCategory.Authentication => "💡 PISTA: La autenticación verifica tu identidad. Contraseñas fuertes + 2FA = mejor seguridad.",
                QuizCategory.Encryption => "💡 PISTA: La encriptación convierte datos en código ilegible. Solo quien tiene la clave puede leerlos.",
                QuizCategory.NetworkSecurity => "💡 PISTA: La seguridad de red protege contra intrusos. Firewalls, VPNs y monitoreo son herramientas clave.",
                QuizCategory.WebSecurity => "💡 PISTA: La seguridad web protege sitios y aplicaciones. SQL Injection y XSS son ataques comunes.",
                QuizCategory.BestPractices => "💡 PISTA: Las buenas prácticas incluyen: actualizar software, no reusar contraseñas, y hacer backups.",
                QuizCategory.DDoS => "💡 PISTA: DDoS (Denegación de Servicio) satura servidores con tráfico falso para dejarlos inaccesibles.",
                _ => "💡 PISTA: Piensa en cómo protegerías tu información personal y sistemas."
            };
        }

        public void ShowInfo(string title, string description, string extraInfo)
        {
            Visible = true;
            GetTree().Paused = true;

            // Configurar UI para modo Info
            _contextLabel.Text = "NEW_THREAT_IDENTIFIED";
            _contextLabel.AddThemeColorOverride("font_color", new Color("ff0000")); // Red
            
            _questionLabel.Text = title;
            _questionLabel.AddThemeColorOverride("font_color", new Color("ffaa00")); // Flux Orange

            // Usar el espacio de respuestas para la descripción
            foreach (Node child in _answersContainer.GetChildren())
            {
                child.QueueFree();
            }

            var descLabel = new Label();
            descLabel.Text = description + "\n\n" + extraInfo;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            descLabel.CustomMinimumSize = new Vector2(550, 100);
            descLabel.AddThemeColorOverride("font_color", Colors.White);
            _answersContainer.AddChild(descLabel);

            // Botón de continuar
            var continueButton = new Button();
            continueButton.Text = "> ACKNOWLEDGE // RESUME_PROCESS";
            continueButton.CustomMinimumSize = new Vector2(550, 45);
            continueButton.Pressed += HideQuiz;
            
            // Estilo del botón
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0, 0.5f, 0);
            continueButton.AddThemeStyleboxOverride("normal", style);
            
            _answersContainer.AddChild(continueButton);

            _explanationLabel.Visible = false;
        }

        private void OnAnswerSelected(int answerIndex)
        {
            _selectedAnswer = answerIndex;
            bool correct = QuizSystem.Instance.CheckAnswer(answerIndex);

            // Mostrar explicación
            _explanationLabel.Text = QuizSystem.Instance.GetExplanation();
            _explanationLabel.Visible = true;

            // Deshabilitar botones
            foreach (Button button in _answersContainer.GetChildren())
            {
                button.Disabled = true;
            }

            // Resaltar respuesta correcta/incorrecta
            var buttons = _answersContainer.GetChildren();
            if (answerIndex < buttons.Count)
            {
                var selectedButton = buttons[answerIndex] as Button;
                if (selectedButton != null)
                {
                    selectedButton.AddThemeColorOverride("font_color", 
                        correct ? Colors.Green : Colors.Red);
                }
            }

            // Resaltar respuesta correcta si falló
            if (!correct && _currentQuestion.CorrectAnswerIndex < buttons.Count)
            {
                var correctButton = buttons[_currentQuestion.CorrectAnswerIndex] as Button;
                if (correctButton != null)
                {
                    correctButton.AddThemeColorOverride("font_color", Colors.Green);
                }
            }

            // Emitir evento
            GameEventBus.Instance.EmitQuestionAnswered(correct);

            // Ocultar después de un tiempo más corto (2.5s)
            _hideTimer.WaitTime = 2.5f;
            _hideTimer.Start();
            
            GD.Print($"[QuizView] Respuesta seleccionada: {(correct ? "CORRECTA" : "INCORRECTA")} - Cerrando en 2.5s");
        }

        private void HideQuiz()
        {
            GD.Print("[QuizView] Cerrando quiz y resumiendo juego");
            _hideTimer.Stop(); // Asegurar que el timer se detenga
            Visible = false;
            GetTree().Paused = false;
            
            // Resetear estado
            _currentQuestion = null;
            _selectedAnswer = -1;
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            // Permitir cerrar con ESC
            if (@event.IsActionPressed("ui_cancel"))
            {
                HideQuiz();
                GetViewport().SetInputAsHandled();
                return;
            }
            
            // Teclas 1-4 para responder rápido
            if (_currentQuestion != null && _selectedAnswer == -1)
            {
                int answerKey = -1;
                if (@event is InputEventKey keyEvent && keyEvent.Pressed)
                {
                    answerKey = keyEvent.Keycode switch
                    {
                        Key.Key1 => 0,
                        Key.Key2 => 1,
                        Key.Key3 => 2,
                        Key.Key4 => 3,
                        _ => -1
                    };
                    
                    if (answerKey >= 0 && answerKey < _currentQuestion.Answers.Length)
                    {
                        OnAnswerSelected(answerKey);
                        GetViewport().SetInputAsHandled();
                    }
                }
            }
        }
    }
}
