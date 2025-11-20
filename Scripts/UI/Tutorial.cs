using Godot;
using System;
using System.Collections.Generic;

public partial class Tutorial : Node2D
{
	private enum TutorialStep
	{
		Movement,
		Shooting,
		WeaponSwitch,
		PowerUps,
		Complete
	}
	
	private TutorialStep _currentStep = TutorialStep.Movement;
	private CharacterBody2D _player;
	private Panel _overlay;
	
	// UI Labels
	private Label _stepLabel;
	private Label _titleLabel;
	private Label _instructionLabel;
	private Label _progressLabel;
	
	// Flechas direccionales
	private Label _arrowUp;
	private Label _arrowDown;
	private Label _arrowLeft;
	private Label _arrowRight;
	private Panel _highlightPanel;
	
	// Tracking de progreso
	private HashSet<string> _movedDirections = new HashSet<string>();
	private int _shotsFired = 0;
	private int _weaponSwitches = 0;
	private bool _powerUpCollected = false;
	
	private Timer _stepTimer;
	
	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("Player");
		
		// TutorialUI es un CanvasLayer, no un Control
		var tutorialUILayer = GetNode<CanvasLayer>("TutorialUI");
		_overlay = tutorialUILayer.GetNode<Panel>("Overlay");
		_overlay.Visible = false; // Hacer overlay invisible para no obstruir visión
		
		var instructionBox = tutorialUILayer.GetNode<PanelContainer>("InstructionBox");
		
		// Reposicionar InstructionBox al fondo (Bottom Center)
		// Usamos AnchorsPreset.BottomCenter si fuera posible, pero aquí lo hacemos manual o via propiedades
		instructionBox.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
		instructionBox.Position = new Vector2((GetViewportRect().Size.X - 800) / 2, GetViewportRect().Size.Y - 220);
		instructionBox.Size = new Vector2(800, 150);
		
		// Estilo Terminal para la caja de instrucciones
		var terminalStyle = new StyleBoxFlat();
		terminalStyle.BgColor = new Color(0.05f, 0.05f, 0.05f, 0.8f); // Negro terminal semi-transparente
		terminalStyle.BorderColor = new Color(0, 1, 1); // Borde Cyan
		terminalStyle.SetBorderWidthAll(2);
		terminalStyle.SetCornerRadiusAll(4);
		terminalStyle.ShadowColor = new Color(0, 1, 1, 0.1f);
		terminalStyle.ShadowSize = 5;
		instructionBox.AddThemeStyleboxOverride("panel", terminalStyle);

		var vbox = instructionBox.GetNode<VBoxContainer>("VBox");
		
		_stepLabel = vbox.GetNode<Label>("StepLabel");
		_stepLabel.AddThemeColorOverride("font_color", new Color(0, 1, 1)); // Cyan
		
		_titleLabel = vbox.GetNode<Label>("TitleLabel");
		_titleLabel.AddThemeColorOverride("font_color", Colors.White); 
		
		_instructionLabel = vbox.GetNode<Label>("InstructionLabel");
		_instructionLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 1));
		
		_progressLabel = vbox.GetNode<Label>("ProgressLabel");
		_progressLabel.AddThemeColorOverride("font_color", new Color(1, 1, 0)); // Amarillo info
		
		_arrowUp = tutorialUILayer.GetNode<Label>("ArrowUp");
		_arrowDown = tutorialUILayer.GetNode<Label>("ArrowDown");
		_arrowLeft = tutorialUILayer.GetNode<Label>("ArrowLeft");
		_arrowRight = tutorialUILayer.GetNode<Label>("ArrowRight");
		_highlightPanel = tutorialUILayer.GetNode<Panel>("HighlightPanel");
		
		// Estilo botón saltar (Top Right)
		var skipButton = tutorialUILayer.GetNode<Button>("SkipButton");
		skipButton.Position = new Vector2(GetViewportRect().Size.X - 160, 20);
		var btnStyle = new StyleBoxFlat();
		btnStyle.BgColor = new Color(0, 0, 0, 0.5f);
		btnStyle.BorderColor = new Color(1, 0, 0); // Rojo para salir
		btnStyle.SetBorderWidthAll(1);
		skipButton.AddThemeStyleboxOverride("normal", btnStyle);
		skipButton.AddThemeColorOverride("font_color", new Color(1, 0.5f, 0.5f));

		_stepTimer = new Timer();
		AddChild(_stepTimer);
		_stepTimer.WaitTime = 0.5f;
		_stepTimer.Timeout += CheckStepProgress;
		_stepTimer.Start();
		
		UpdateStepUI();
	}
	
	public override void _Process(double delta)
	{
		// Tracking de movimiento
		if (_currentStep == TutorialStep.Movement)
		{
			if (Input.IsActionPressed("move_up")) _movedDirections.Add("up");
			if (Input.IsActionPressed("move_down")) _movedDirections.Add("down");
			if (Input.IsActionPressed("move_left")) _movedDirections.Add("left");
			if (Input.IsActionPressed("move_right")) _movedDirections.Add("right");
			
			UpdateMovementProgress();
		}
		
		// Tracking de disparos
		if (_currentStep == TutorialStep.Shooting)
		{
			if (Input.IsActionJustPressed("fire"))
			{
				_shotsFired++;
				UpdateShootingProgress();
			}
		}
		
		// Tracking de cambio de armas
		if (_currentStep == TutorialStep.WeaponSwitch)
		{
			if (Input.IsActionJustPressed("next_weapon") || Input.IsActionJustPressed("prev_weapon"))
			{
				_weaponSwitches++;
				UpdateWeaponSwitchProgress();
			}
		}
	}
	
	private void CheckStepProgress()
	{
		switch (_currentStep)
		{
			case TutorialStep.Movement:
				if (_movedDirections.Count >= 4)
				{
					AdvanceStep();
				}
				break;
				
			case TutorialStep.Shooting:
				if (_shotsFired >= 5)
				{
					AdvanceStep();
				}
				break;
				
			case TutorialStep.WeaponSwitch:
				if (_weaponSwitches >= 3)
				{
					AdvanceStep();
				}
				break;
				
			case TutorialStep.PowerUps:
				// Se avanza cuando se recolecte un power-up
				break;
		}
	}
	
	private void AdvanceStep()
	{
		_currentStep++;
		
		if (_currentStep == TutorialStep.Complete)
		{
			CompleteTutorial();
			return;
		}
		
		UpdateStepUI();
	}
	
	private void UpdateStepUI()
	{
		int stepNumber = (int)_currentStep + 1;
		_stepLabel.Text = $"Paso {stepNumber}/5";
		
		// Ocultar todas las flechas y highlights
		_arrowUp.Visible = false;
		_arrowDown.Visible = false;
		_arrowLeft.Visible = false;
		_arrowRight.Visible = false;
		_highlightPanel.Visible = false;
		
		switch (_currentStep)
		{
			case TutorialStep.Movement:
				_titleLabel.Text = "🎮 Movimiento Básico";
				_instructionLabel.Text = "Usa las teclas WASD o las flechas para mover tu nave.\n¡Intenta moverte en las 4 direcciones!";
				_progressLabel.Text = "Progreso: Muévete en todas las direcciones";
				
				// Mostrar flechas direccionales
				_arrowUp.Visible = true;
				_arrowDown.Visible = true;
				_arrowLeft.Visible = true;
				_arrowRight.Visible = true;
				break;
				
			case TutorialStep.Shooting:
				_titleLabel.Text = "🔫 Disparar";
				_instructionLabel.Text = "Presiona ESPACIO o CLICK IZQUIERDO para disparar.\n¡Practica disparando 5 veces!";
				_progressLabel.Text = "Progreso: 0/5 disparos";
				break;
				
			case TutorialStep.WeaponSwitch:
				_titleLabel.Text = "⚔️ Cambiar Armas";
				_instructionLabel.Text = "Usa las teclas 1, 2, 3, 4 o la RUEDA DEL RATÓN para cambiar de arma.\n¡Cambia de arma 3 veces!";
				_progressLabel.Text = "Progreso: 0/3 cambios";
				break;
				
			case TutorialStep.PowerUps:
				_titleLabel.Text = "💎 Power-Ups";
				_instructionLabel.Text = "¡Los power-ups te dan mejoras temporales!\nCollisiona con ellos para recogerlos.";
				_progressLabel.Text = "Espera a que aparezca un power-up...";
				SpawnPowerUpForTutorial();
				break;
		}
	}
	
	private void UpdateMovementProgress()
	{
		_progressLabel.Text = $"Progreso: {_movedDirections.Count}/4 direcciones";
		
		// Resaltar flechas según direcciones usadas
		_arrowUp.Modulate = _movedDirections.Contains("up") ? new Color(0, 1, 0.5f) : new Color(0, 1, 1);
		_arrowDown.Modulate = _movedDirections.Contains("down") ? new Color(0, 1, 0.5f) : new Color(0, 1, 1);
		_arrowLeft.Modulate = _movedDirections.Contains("left") ? new Color(0, 1, 0.5f) : new Color(0, 1, 1);
		_arrowRight.Modulate = _movedDirections.Contains("right") ? new Color(0, 1, 0.5f) : new Color(0, 1, 1);
	}
	
	private void UpdateShootingProgress()
	{
		_progressLabel.Text = $"Progreso: {_shotsFired}/5 disparos";
	}
	
	private void UpdateWeaponSwitchProgress()
	{
		_progressLabel.Text = $"Progreso: {_weaponSwitches}/3 cambios";
	}
	
	private void SpawnPowerUpForTutorial()
	{
		// TODO: Crear un power-up de demostración
		// Por ahora, avanzamos automáticamente después de 5 segundos
		var autoAdvanceTimer = new Timer();
		AddChild(autoAdvanceTimer);
		autoAdvanceTimer.WaitTime = 5.0f;
		autoAdvanceTimer.OneShot = true;
		autoAdvanceTimer.Timeout += () => {
			_powerUpCollected = true;
			AdvanceStep();
		};
		autoAdvanceTimer.Start();
	}
	
	private void CompleteTutorial()
	{
		_titleLabel.Text = "✅ ¡Tutorial Completado!";
		_instructionLabel.Text = "¡Excelente trabajo! Ya conoces los controles básicos.\n¿Listo para el juego real?";
		_progressLabel.Text = "Presiona cualquier botón para comenzar";
		_stepLabel.Text = "Paso 5/5";
		
		// Ocultar overlay para mostrar todo el juego
		_overlay.Visible = false;
		
		// Esperar input para continuar
		var continueTimer = new Timer();
		AddChild(continueTimer);
		continueTimer.WaitTime = 0.1f;
		continueTimer.Timeout += () => {
			if (Input.IsAnythingPressed())
			{
				GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
			}
		};
		continueTimer.Start();
	}
	
	private void _on_skip_button_pressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			_on_skip_button_pressed();
		}
	}
}
