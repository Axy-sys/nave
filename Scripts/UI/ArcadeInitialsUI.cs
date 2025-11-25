using Godot;
using System;
using CyberSecurityGame.Core;

namespace CyberSecurityGame.UI
{
    /// <summary>
    /// UI de entrada de iniciales estilo ARCADE RETRO
    /// 
    /// Como las máquinas de los 80s:
    /// - 3 letras seleccionables con ↑↓ o A-Z
    /// - Visual tipo terminal/CRT
    /// - Sonido de selección (si disponible)
    /// </summary>
    public partial class ArcadeInitialsUI : CanvasLayer
    {
        // Colores retro
        private static readonly Color ARCADE_GREEN = new Color("#00ff41");
        private static readonly Color ARCADE_AMBER = new Color("#ffaa00");
        private static readonly Color ARCADE_RED = new Color("#ff0044");
        private static readonly Color BG_DARK = new Color("#0a0a0a");

        // Letras del alfabeto
        private static readonly char[] ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        // Estado
        private int[] _letterIndices = { 0, 0, 0 }; // Índices en ALPHABET (A=0, B=1, etc.)
        private int _currentPosition = 0; // 0, 1, o 2
        private int _score;
        private int _wave;
        private int _position;
        private bool _isActive = false;

        // UI Elements
        private Panel _mainPanel;
        private Label _titleLabel;
        private Label _scoreLabel;
        private Label _positionLabel;
        private Label[] _letterLabels = new Label[3];
        private Label[] _arrowLabels = new Label[3];
        private Label _instructionsLabel;
        private ColorRect _overlay;
        private float _blinkTimer = 0f;

        // Señales
        [Signal] public delegate void InitialsEnteredEventHandler(string initials);
        [Signal] public delegate void CancelledEventHandler();

        public override void _Ready()
        {
            Layer = 100;
            ProcessMode = ProcessModeEnum.Always;
            CreateUI();
            Hide();
        }

        private void CreateUI()
        {
            // Overlay
            _overlay = new ColorRect();
            _overlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _overlay.Color = new Color(0, 0, 0, 0.9f);
            AddChild(_overlay);

            // Panel principal
            _mainPanel = new Panel();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(500, 400);
            _mainPanel.GrowHorizontal = Control.GrowDirection.Both;
            _mainPanel.GrowVertical = Control.GrowDirection.Both;

            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = BG_DARK;
            panelStyle.BorderColor = ARCADE_GREEN;
            panelStyle.SetBorderWidthAll(3);
            panelStyle.SetCornerRadiusAll(0); // Bordes rectos = más retro
            _mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
            AddChild(_mainPanel);

            var content = new VBoxContainer();
            content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            content.OffsetTop = 30;
            content.OffsetBottom = -30;
            content.OffsetLeft = 30;
            content.OffsetRight = -30;
            content.AddThemeConstantOverride("separation", 20);
            _mainPanel.AddChild(content);

            // Título
            _titleLabel = new Label();
            _titleLabel.Text = "★ NEW HIGH SCORE ★";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeColorOverride("font_color", ARCADE_AMBER);
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            content.AddChild(_titleLabel);

            // Posición y Score
            _positionLabel = new Label();
            _positionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _positionLabel.AddThemeColorOverride("font_color", ARCADE_GREEN);
            _positionLabel.AddThemeFontSizeOverride("font_size", 20);
            content.AddChild(_positionLabel);

            _scoreLabel = new Label();
            _scoreLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _scoreLabel.AddThemeColorOverride("font_color", Colors.White);
            _scoreLabel.AddThemeFontSizeOverride("font_size", 36);
            content.AddChild(_scoreLabel);

            // Container para las 3 letras
            var lettersContainer = new HBoxContainer();
            lettersContainer.Alignment = BoxContainer.AlignmentMode.Center;
            lettersContainer.AddThemeConstantOverride("separation", 30);
            content.AddChild(lettersContainer);

            // Crear las 3 columnas de letras
            for (int i = 0; i < 3; i++)
            {
                var letterColumn = new VBoxContainer();
                letterColumn.AddThemeConstantOverride("separation", 5);

                // Flecha arriba
                var arrowUp = new Label();
                arrowUp.Text = "▲";
                arrowUp.HorizontalAlignment = HorizontalAlignment.Center;
                arrowUp.AddThemeColorOverride("font_color", ARCADE_GREEN);
                arrowUp.AddThemeFontSizeOverride("font_size", 24);
                letterColumn.AddChild(arrowUp);

                // Letra
                var letterLabel = new Label();
                letterLabel.Text = "A";
                letterLabel.HorizontalAlignment = HorizontalAlignment.Center;
                letterLabel.AddThemeColorOverride("font_color", ARCADE_GREEN);
                letterLabel.AddThemeFontSizeOverride("font_size", 64);
                letterLabel.CustomMinimumSize = new Vector2(60, 70);
                _letterLabels[i] = letterLabel;
                letterColumn.AddChild(letterLabel);

                // Flecha abajo
                var arrowDown = new Label();
                arrowDown.Text = "▼";
                arrowDown.HorizontalAlignment = HorizontalAlignment.Center;
                arrowDown.AddThemeColorOverride("font_color", ARCADE_GREEN);
                arrowDown.AddThemeFontSizeOverride("font_size", 24);
                letterColumn.AddChild(arrowDown);

                // Indicador de posición actual
                var indicator = new Label();
                indicator.Text = "═";
                indicator.HorizontalAlignment = HorizontalAlignment.Center;
                indicator.AddThemeColorOverride("font_color", ARCADE_AMBER);
                indicator.AddThemeFontSizeOverride("font_size", 20);
                _arrowLabels[i] = indicator;
                letterColumn.AddChild(indicator);

                lettersContainer.AddChild(letterColumn);
            }

            // Instrucciones
            _instructionsLabel = new Label();
            _instructionsLabel.Text = "↑↓ CAMBIAR LETRA   ←→ MOVER   ENTER CONFIRMAR";
            _instructionsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _instructionsLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            _instructionsLabel.AddThemeFontSizeOverride("font_size", 14);
            content.AddChild(_instructionsLabel);

            // También puedes escribir directamente
            var typeHint = new Label();
            typeHint.Text = "O ESCRIBE TUS INICIALES (A-Z)";
            typeHint.HorizontalAlignment = HorizontalAlignment.Center;
            typeHint.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
            typeHint.AddThemeFontSizeOverride("font_size", 12);
            content.AddChild(typeHint);
        }

        public void ShowForScore(int score, int wave, int position)
        {
            _score = score;
            _wave = wave;
            _position = position;
            _currentPosition = 0;
            _letterIndices = new[] { 0, 0, 0 }; // Reset a AAA
            _isActive = true;

            _positionLabel.Text = $"RANK #{position}  •  WAVE {wave}";
            _scoreLabel.Text = $"{score:N0}";

            UpdateLetterDisplay();
            Show();
        }

        public override void _Process(double delta)
        {
            if (!Visible || !_isActive) return;

            // Efecto de parpadeo en la letra actual
            _blinkTimer += (float)delta;
            bool showCursor = Mathf.Sin(_blinkTimer * 8) > 0;

            for (int i = 0; i < 3; i++)
            {
                if (i == _currentPosition)
                {
                    _arrowLabels[i].Visible = showCursor;
                    _letterLabels[i].AddThemeColorOverride("font_color", ARCADE_AMBER);
                }
                else
                {
                    _arrowLabels[i].Visible = false;
                    _letterLabels[i].AddThemeColorOverride("font_color", ARCADE_GREEN);
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible || !_isActive) return;

            if (@event is InputEventKey key && key.Pressed && !key.Echo)
            {
                // Teclas de dirección
                switch (key.Keycode)
                {
                    case Key.Up:
                    case Key.W:
                        ChangeCurrentLetter(1);
                        GetViewport().SetInputAsHandled();
                        break;

                    case Key.Down:
                    case Key.S:
                        ChangeCurrentLetter(-1);
                        GetViewport().SetInputAsHandled();
                        break;

                    case Key.Left:
                    case Key.A:
                        MovePosition(-1);
                        GetViewport().SetInputAsHandled();
                        break;

                    case Key.Right:
                    case Key.D:
                        MovePosition(1);
                        GetViewport().SetInputAsHandled();
                        break;

                    case Key.Enter:
                    case Key.KpEnter:
                        ConfirmInitials();
                        GetViewport().SetInputAsHandled();
                        break;

                    case Key.Escape:
                        Cancel();
                        GetViewport().SetInputAsHandled();
                        break;

                    default:
                        // Si es una letra A-Z, usarla directamente
                        if (key.Keycode >= Key.A && key.Keycode <= Key.Z)
                        {
                            int letterIndex = (int)key.Keycode - (int)Key.A;
                            _letterIndices[_currentPosition] = letterIndex;
                            UpdateLetterDisplay();
                            
                            // Avanzar a la siguiente posición
                            if (_currentPosition < 2)
                            {
                                _currentPosition++;
                            }
                            GetViewport().SetInputAsHandled();
                        }
                        break;
                }
            }
        }

        private void ChangeCurrentLetter(int direction)
        {
            _letterIndices[_currentPosition] += direction;

            // Wrap around
            if (_letterIndices[_currentPosition] < 0)
                _letterIndices[_currentPosition] = ALPHABET.Length - 1;
            if (_letterIndices[_currentPosition] >= ALPHABET.Length)
                _letterIndices[_currentPosition] = 0;

            UpdateLetterDisplay();
        }

        private void MovePosition(int direction)
        {
            _currentPosition += direction;

            // Clamp
            if (_currentPosition < 0) _currentPosition = 0;
            if (_currentPosition > 2) _currentPosition = 2;
        }

        private void UpdateLetterDisplay()
        {
            for (int i = 0; i < 3; i++)
            {
                _letterLabels[i].Text = ALPHABET[_letterIndices[i]].ToString();
            }
        }

        private string GetInitials()
        {
            return $"{ALPHABET[_letterIndices[0]]}{ALPHABET[_letterIndices[1]]}{ALPHABET[_letterIndices[2]]}";
        }

        private void ConfirmInitials()
        {
            _isActive = false;
            string initials = GetInitials();

            // Guardar en el sistema arcade
            ArcadeScoreSystem.Instance?.AddScore(initials, _score, _wave);

            // Efecto visual de confirmación
            _titleLabel.Text = $"★ {initials} REGISTERED ★";
            _titleLabel.AddThemeColorOverride("font_color", ARCADE_GREEN);

            // Cerrar después de un momento
            var timer = GetTree().CreateTimer(1.0);
            timer.Timeout += () =>
            {
                Hide();
                EmitSignal(SignalName.InitialsEntered, initials);
            };
        }

        private void Cancel()
        {
            _isActive = false;
            Hide();
            EmitSignal(SignalName.Cancelled);
        }
    }
}
