using Godot;
using CyberSecurityGame.Education;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Views
{
    /// <summary>
    /// Vista para mostrar preguntas de quiz durante el juego
    /// </summary>
    public partial class QuizView : CanvasLayer
    {
        private Panel _quizPanel;
        private Label _questionLabel;
        private Label _contextLabel;
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
            // Panel principal con estilo de Terminal Hacker
            _quizPanel = new Panel();
            _quizPanel.Name = "QuizPanel";
            _quizPanel.Position = new Vector2(250, 150);
            _quizPanel.Size = new Vector2(650, 450);
            
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.02f, 0.02f, 0.02f, 0.98f); // Negro profundo
            panelStyle.BorderColor = new Color(0, 1, 0); // Borde verde terminal
            panelStyle.SetBorderWidthAll(2);
            panelStyle.SetCornerRadiusAll(2); // Bordes más rectos
            panelStyle.ShadowColor = new Color(0, 1, 0, 0.2f); // Glow verde sutil
            panelStyle.ShadowSize = 10;
            _quizPanel.AddThemeStyleboxOverride("panel", panelStyle);
            
            AddChild(_quizPanel);

            // Barra de título del terminal
            var titleBar = new Panel();
            titleBar.Size = new Vector2(650, 30);
            titleBar.Position = Vector2.Zero;
            var titleBarStyle = new StyleBoxFlat();
            titleBarStyle.BgColor = new Color(0, 1, 0, 0.2f); // Verde transparente
            titleBarStyle.BorderColor = new Color(0, 1, 0);
            titleBarStyle.BorderWidthBottom = 2;
            titleBar.AddThemeStyleboxOverride("panel", titleBarStyle);
            _quizPanel.AddChild(titleBar);

            // Título de Alerta
            var titleLabel = new Label();
            titleLabel.Text = "> ROOT_ACCESS_REQUESTED // SECURITY_CHECK";
            titleLabel.Position = new Vector2(10, 0);
            titleLabel.Size = new Vector2(630, 30);
            titleLabel.VerticalAlignment = VerticalAlignment.Center;
            titleLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            titleBar.AddChild(titleLabel);

            // Contexto (Situación)
            _contextLabel = new Label();
            _contextLabel.Name = "ContextLabel";
            _contextLabel.Position = new Vector2(30, 40);
            _contextLabel.Size = new Vector2(590, 40);
            _contextLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _contextLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _contextLabel.AddThemeColorOverride("font_color", new Color(0.0f, 0.8f, 1.0f)); // Cyan
            _contextLabel.AddThemeFontSizeOverride("font_size", 14);
            _quizPanel.AddChild(_contextLabel);

            // Pregunta
            _questionLabel = new Label();
            _questionLabel.Name = "QuestionLabel";
            _questionLabel.Position = new Vector2(30, 85); // Bajado para dar espacio al contexto
            _questionLabel.Size = new Vector2(590, 60);
            _questionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _questionLabel.HorizontalAlignment = HorizontalAlignment.Left; // Alineación terminal
            _questionLabel.AddThemeColorOverride("font_color", new Color(0.8f, 1, 0.8f)); // Verde claro
            _questionLabel.AddThemeFontSizeOverride("font_size", 18);
            _quizPanel.AddChild(_questionLabel);

            // Contenedor de respuestas
            _answersContainer = new VBoxContainer();
            _answersContainer.Name = "AnswersContainer";
            _answersContainer.Position = new Vector2(50, 160); // Bajado ligeramente
            _answersContainer.Size = new Vector2(550, 200);
            _answersContainer.AddThemeConstantOverride("separation", 15);
            _quizPanel.AddChild(_answersContainer);

            // Explicación
            _explanationLabel = new Label();
            _explanationLabel.Name = "ExplanationLabel";
            _explanationLabel.Position = new Vector2(30, 360);
            _explanationLabel.Size = new Vector2(590, 80);
            _explanationLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _explanationLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _explanationLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0)); // Amarillo para info
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
            _currentQuestion = question;
            _selectedAnswer = -1;
            
            Visible = true;
            GetTree().Paused = true;

            _contextLabel.Text = question.Context;
            _questionLabel.Text = question.Question;
            _explanationLabel.Visible = false;

            // Limpiar respuestas anteriores
            foreach (Node child in _answersContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Crear botones de respuesta con estilo Terminal
            for (int i = 0; i < question.Answers.Length; i++)
            {
                Button answerButton = new Button();
                answerButton.Text = $"> {question.Answers[i]}"; // Prefijo de terminal
                answerButton.CustomMinimumSize = new Vector2(550, 45);
                answerButton.Alignment = HorizontalAlignment.Left;
                answerButton.AddThemeColorOverride("font_color", new Color(0, 1, 0)); // Texto verde
                answerButton.AddThemeColorOverride("font_hover_color", Colors.Black); // Texto negro en hover
                
                // Estilo normal (Transparente con borde verde)
                var normalStyle = new StyleBoxFlat();
                normalStyle.BgColor = new Color(0, 0, 0, 0.5f);
                normalStyle.BorderColor = new Color(0, 0.7f, 0);
                normalStyle.SetBorderWidthAll(1);
                normalStyle.SetCornerRadiusAll(2);
                answerButton.AddThemeStyleboxOverride("normal", normalStyle);
                
                // Estilo hover (Verde sólido)
                var hoverStyle = new StyleBoxFlat();
                hoverStyle.BgColor = new Color(0, 1, 0);
                hoverStyle.BorderColor = new Color(0, 1, 0);
                hoverStyle.SetBorderWidthAll(1);
                hoverStyle.SetCornerRadiusAll(2);
                answerButton.AddThemeStyleboxOverride("hover", hoverStyle);

                // Estilo pressed (Verde oscuro)
                var pressedStyle = new StyleBoxFlat();
                pressedStyle.BgColor = new Color(0, 0.8f, 0);
                answerButton.AddThemeStyleboxOverride("pressed", pressedStyle);

                int index = i; // Captura para el lambda
                answerButton.Pressed += () => OnAnswerSelected(index);
                _answersContainer.AddChild(answerButton);
            }
        }

        public void ShowInfo(string title, string description, string extraInfo)
        {
            Visible = true;
            GetTree().Paused = true;

            // Configurar UI para modo Info
            _contextLabel.Text = "NUEVA AMENAZA DETECTADA";
            _contextLabel.AddThemeColorOverride("font_color", Colors.Red);
            
            _questionLabel.Text = title;
            _questionLabel.AddThemeColorOverride("font_color", Colors.Orange);

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
            continueButton.Text = "> ENTENDIDO // CONTINUAR";
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

            // Ocultar después de un tiempo
            _hideTimer.Start();
        }

        private void HideQuiz()
        {
            Visible = false;
            GetTree().Paused = false;
        }

        public override void _Input(InputEvent @event)
        {
            // Permitir cerrar con ESC
            if (@event.IsActionPressed("ui_cancel") && Visible)
            {
                HideQuiz();
            }
        }
    }
}
