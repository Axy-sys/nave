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
        private VBoxContainer _answersContainer;
        private Label _explanationLabel;
        private Timer _hideTimer;
        
        private QuizQuestion _currentQuestion;
        private int _selectedAnswer = -1;

        public override void _Ready()
        {
            InitializeUI();
            Visible = false;
        }

        private void InitializeUI()
        {
            // Panel principal
            _quizPanel = new Panel();
            _quizPanel.Name = "QuizPanel";
            _quizPanel.Position = new Vector2(250, 150);
            _quizPanel.Size = new Vector2(500, 400);
            AddChild(_quizPanel);

            // Pregunta
            _questionLabel = new Label();
            _questionLabel.Name = "QuestionLabel";
            _questionLabel.Position = new Vector2(20, 20);
            _questionLabel.Size = new Vector2(460, 80);
            _questionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _questionLabel.AddThemeColorOverride("font_color", Colors.White);
            _quizPanel.AddChild(_questionLabel);

            // Contenedor de respuestas
            _answersContainer = new VBoxContainer();
            _answersContainer.Name = "AnswersContainer";
            _answersContainer.Position = new Vector2(20, 120);
            _answersContainer.Size = new Vector2(460, 200);
            _quizPanel.AddChild(_answersContainer);

            // Explicación
            _explanationLabel = new Label();
            _explanationLabel.Name = "ExplanationLabel";
            _explanationLabel.Position = new Vector2(20, 330);
            _explanationLabel.Size = new Vector2(460, 50);
            _explanationLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _explanationLabel.AddThemeColorOverride("font_color", Colors.LightBlue);
            _explanationLabel.Visible = false;
            _quizPanel.AddChild(_explanationLabel);

            // Timer para ocultar
            _hideTimer = new Timer();
            _hideTimer.Name = "HideTimer";
            _hideTimer.WaitTime = 3.0;
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

            _questionLabel.Text = question.Question;
            _explanationLabel.Visible = false;

            // Limpiar respuestas anteriores
            foreach (Node child in _answersContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Crear botones de respuesta
            for (int i = 0; i < question.Answers.Length; i++)
            {
                Button answerButton = new Button();
                answerButton.Text = question.Answers[i];
                answerButton.CustomMinimumSize = new Vector2(440, 40);
                int index = i; // Captura para el lambda
                answerButton.Pressed += () => OnAnswerSelected(index);
                _answersContainer.AddChild(answerButton);
            }
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
