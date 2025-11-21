using Godot;
using System;
using System.Collections.Generic;

public partial class Tutorial : Node2D
{
	private enum TutorialStep
	{
		Movement,
		Dash,
		Shooting,
		CpuManagement, // Nuevo paso: Gestión de CPU
		AdaptiveSystems, // Reemplaza WeaponSwitch
		Parry,
		Interaction,
		Complete
	}
	
	private TutorialStep _currentStep = TutorialStep.Movement;
	private CharacterBody2D _player;
	private Panel _overlay;
	
	// UI Labels
	private Label _stepLabel;
	private Label _titleLabel;
	private RichTextLabel _instructionLabel; // Cambiado a RichTextLabel para BBCode
	private Label _progressLabel;
	private AnimationPlayer _animPlayer; // Para feedback visual
	
	// Flechas direccionales
	private Label _arrowUp;
	private Label _arrowDown;
	private Label _arrowLeft;
	private Label _arrowRight;
	private Panel _highlightPanel;
	
	// Tracking de progreso
	private HashSet<string> _movedDirections = new HashSet<string>();
	private int _dashesPerformed = 0;
	private int _shotsFired = 0;
	private float _maxHeatReached = 0f; // Reemplaza weapon switches
	private int _parriesPerformed = 0;
	private bool _interactionPerformed = false;
	private bool _cpuOverloaded = false; // Para el tutorial de CPU
	
	private Timer _stepTimer;
	private float _timeInCurrentStep = 0f;
	private const float HINT_DELAY = 10.0f; // Tiempo antes de mostrar pista extra
	
	public override void _Ready()
	{
		_player = GetNode<CharacterBody2D>("Player");
		
		// TutorialUI es un CanvasLayer
		var tutorialUILayer = GetNode<CanvasLayer>("TutorialUI");
		_overlay = tutorialUILayer.GetNode<Panel>("Overlay");
		_overlay.Visible = false; 
		
		var instructionBox = tutorialUILayer.GetNode<PanelContainer>("InstructionBox");
		
		// Reposicionar InstructionBox (Más grande y accesible)
		instructionBox.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
		instructionBox.Position = new Vector2((GetViewportRect().Size.X - 900) / 2, GetViewportRect().Size.Y - 250);
		instructionBox.Size = new Vector2(900, 180);
		
		// Estilo Terminal Mejorado (Alto Contraste)
		var terminalStyle = new StyleBoxFlat();
		terminalStyle.BgColor = new Color(0.02f, 0.02f, 0.02f, 0.95f); 
		terminalStyle.BorderColor = new Color("bf00ff"); // Rippier Purple
		terminalStyle.SetBorderWidthAll(3);
		terminalStyle.SetCornerRadiusAll(8);
		terminalStyle.ShadowColor = new Color("bf00ff");
		terminalStyle.ShadowSize = 15;
		instructionBox.AddThemeStyleboxOverride("panel", terminalStyle);

		var vbox = instructionBox.GetNode<VBoxContainer>("VBox");
		
		_stepLabel = vbox.GetNode<Label>("StepLabel");
		_stepLabel.AddThemeColorOverride("font_color", new Color("00ff41")); // Terminal Green
		_stepLabel.AddThemeFontSizeOverride("font_size", 16);
		
		_titleLabel = vbox.GetNode<Label>("TitleLabel");
		_titleLabel.AddThemeColorOverride("font_color", Colors.White); 
		_titleLabel.AddThemeFontSizeOverride("font_size", 28); // Más grande
		
		// Reemplazar Label con RichTextLabel si es necesario, o castear si ya lo cambiamos en escena
		// Como estamos editando código, asumimos que el nodo en escena es compatible o lo reemplazamos dinámicamente
		var oldLabel = vbox.GetNodeOrNull<Label>("InstructionLabel");
		if (oldLabel != null)
		{
			oldLabel.QueueFree();
			_instructionLabel = new RichTextLabel();
			_instructionLabel.Name = "InstructionLabel";
			_instructionLabel.BbcodeEnabled = true;
			_instructionLabel.FitContent = true;
			_instructionLabel.CustomMinimumSize = new Vector2(0, 60);
			_instructionLabel.AddThemeFontSizeOverride("normal_font_size", 20); // Texto grande y legible
			vbox.AddChild(_instructionLabel);
			vbox.MoveChild(_instructionLabel, 2);
		}
		else
		{
			_instructionLabel = vbox.GetNode<RichTextLabel>("InstructionLabel");
		}
		
		_progressLabel = vbox.GetNode<Label>("ProgressLabel");
		_progressLabel.AddThemeColorOverride("font_color", new Color("ffaa00")); // Flux Orange
		_progressLabel.AddThemeFontSizeOverride("font_size", 18);
		
		_arrowUp = tutorialUILayer.GetNode<Label>("ArrowUp");
		_arrowDown = tutorialUILayer.GetNode<Label>("ArrowDown");
		_arrowLeft = tutorialUILayer.GetNode<Label>("ArrowLeft");
		_arrowRight = tutorialUILayer.GetNode<Label>("ArrowRight");
		_highlightPanel = tutorialUILayer.GetNode<Panel>("HighlightPanel");
		
		// Botón Saltar Accesible
		var skipButton = tutorialUILayer.GetNode<Button>("SkipButton");
		skipButton.Text = "SKIP_TRAINING_SEQUENCE (ESC)";
		skipButton.Position = new Vector2(GetViewportRect().Size.X - 220, 20);
		skipButton.Size = new Vector2(200, 50);
		
		_stepTimer = new Timer();
		AddChild(_stepTimer);
		_stepTimer.WaitTime = 0.1f;
		_stepTimer.Timeout += CheckStepProgress;
		_stepTimer.Start();
		
		UpdateStepUI();
	}
	
	public override void _Process(double delta)
	{
		_timeInCurrentStep += (float)delta;
		
		// Sistema de Pistas para "Dummies" (Si se atascan)
		if (_timeInCurrentStep > HINT_DELAY)
		{
			ShowHint();
			_timeInCurrentStep = 0; // Reset para no spammear
		}

		// Tracking de movimiento
		if (_currentStep == TutorialStep.Movement)
		{
			if (Input.IsActionPressed("move_up")) _movedDirections.Add("up");
			if (Input.IsActionPressed("move_down")) _movedDirections.Add("down");
			if (Input.IsActionPressed("move_left")) _movedDirections.Add("left");
			if (Input.IsActionPressed("move_right")) _movedDirections.Add("right");
			
			UpdateMovementProgress();
		}
		
		// Tracking de Dash
		if (_currentStep == TutorialStep.Dash)
		{
			// Lógica manejada en _Input
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
		
		// Tracking de CPU
		if (_currentStep == TutorialStep.CpuManagement)
		{
			var cpu = _player.GetNodeOrNull<CyberSecurityGame.Components.CpuComponent>("CpuComponent");
			if (cpu != null && cpu.IsOverloaded())
			{
				_cpuOverloaded = true;
			}
		}
		
		// Tracking de Sistemas Adaptativos
		if (_currentStep == TutorialStep.AdaptiveSystems)
		{
			var cpu = _player.GetNodeOrNull<CyberSecurityGame.Components.CpuComponent>("CpuComponent");
			if (cpu != null)
			{
				float currentHeat = cpu.GetLoadPercentage();
				if (currentHeat > _maxHeatReached) _maxHeatReached = currentHeat;
				UpdateAdaptiveProgress();
			}
		}

		// Tracking de Parry
		if (_currentStep == TutorialStep.Parry)
		{
			// Lógica manejada en _Input
		}
		
		// Tracking de Interacción
		if (_currentStep == TutorialStep.Interaction)
		{
			if (Input.IsKeyPressed(Key.E))
			{
				_interactionPerformed = true;
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			_on_skip_button_pressed();
		}

		if (_currentStep == TutorialStep.Dash && @event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.Space)
		{
			_dashesPerformed++;
			UpdateDashProgress();
			ResetStuckTimer();
		}

		if (_currentStep == TutorialStep.Parry && @event is InputEventKey keyEvent2 && keyEvent2.Pressed && !keyEvent2.Echo && keyEvent2.Keycode == Key.Shift)
		{
			_parriesPerformed++;
			UpdateParryProgress();
			ResetStuckTimer();
		}
		
		// Reset timer on any valid input for current step
		if (_currentStep == TutorialStep.Movement && (@event.IsActionPressed("move_up") || @event.IsActionPressed("move_down"))) ResetStuckTimer();
		if (_currentStep == TutorialStep.Shooting && @event.IsActionPressed("fire")) ResetStuckTimer();
	}
	
	private void ResetStuckTimer()
	{
		_timeInCurrentStep = 0;
	}
	
	private void ShowHint()
	{
		// Feedback visual extra si el jugador tarda mucho
		var tween = CreateTween();
		tween.TweenProperty(_instructionLabel, "modulate", new Color(1, 0.5f, 0.5f), 0.5f); // Rojo suave
		tween.TweenProperty(_instructionLabel, "modulate", Colors.White, 0.5f);
		
		// Podríamos reproducir un sonido aquí
		GD.Print("💡 ¿Necesitas ayuda? Revisa las instrucciones.");
	}
	
	private void CheckStepProgress()
	{
		switch (_currentStep)
		{
			case TutorialStep.Movement:
				if (_movedDirections.Count >= 4) AdvanceStep();
				break;
				
			case TutorialStep.Dash:
				if (_dashesPerformed >= 3) AdvanceStep();
				break;

			case TutorialStep.Shooting:
				if (_shotsFired >= 10) AdvanceStep(); // Aumentado para forzar generación de calor
				break;
				
			case TutorialStep.CpuManagement:
				if (_cpuOverloaded) AdvanceStep();
				break;
				
			case TutorialStep.AdaptiveSystems:
				// Avanzar si el jugador ha experimentado alto calor (Chaos Mode)
				if (_maxHeatReached >= 0.75f) AdvanceStep();
				break;
				
			case TutorialStep.Parry:
				if (_parriesPerformed >= 3) AdvanceStep();
				break;

			case TutorialStep.Interaction:
				if (_interactionPerformed) AdvanceStep();
				break;
		}
	}
	
	private void AdvanceStep()
	{
		_currentStep++;
		_timeInCurrentStep = 0; // Reset timer
		
		// Sonido de éxito (simulado) y feedback visual
		GD.Print("✨ Paso completado!");
		FlashScreen();
		
		if (_currentStep == TutorialStep.Complete)
		{
			CompleteTutorial();
			return;
		}
		
		UpdateStepUI();
	}
	
	private void FlashScreen()
	{
		var flash = new ColorRect();
		flash.Color = new Color(1, 1, 1, 0);
		flash.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		flash.MouseFilter = Control.MouseFilterEnum.Ignore;
		_overlay.GetParent().AddChild(flash);
		
		var tween = CreateTween();
		tween.TweenProperty(flash, "color:a", 0.3f, 0.1f);
		tween.TweenProperty(flash, "color:a", 0.0f, 0.3f);
		tween.TweenCallback(Callable.From(flash.QueueFree));
	}
	
	private void UpdateStepUI()
	{
		int stepNumber = (int)_currentStep + 1;
		_stepLabel.Text = $"TRAINING_MODULE_V1.0 {stepNumber}/8";
		
		// Ocultar flechas por defecto
		_arrowUp.Visible = false;
		_arrowDown.Visible = false;
		_arrowLeft.Visible = false;
		_arrowRight.Visible = false;
		
		switch (_currentStep)
		{
			case TutorialStep.Movement:
				_titleLabel.Text = "BASIC_NAVIGATION_PROTOCOLS";
				_instructionLabel.Text = "Use [color=#ffaa00][b]W, A, S, D[/b][/color] to navigate the grid.\nExplore all vectors to calibrate engines.";
				_progressLabel.Text = "Status: Initializing engines...";
				
				_arrowUp.Visible = true;
				_arrowDown.Visible = true;
				_arrowLeft.Visible = true;
				_arrowRight.Visible = true;
				break;
				
			case TutorialStep.Dash:
				_titleLabel.Text = "PACKET_JUMP (DASH)";
				_instructionLabel.Text = "Press [color=#ffaa00][b]SPACE[/b][/color] while moving for a rapid impulse.\n[i]Note: This consumes CPU cycles.[/i]";
				_progressLabel.Text = "Progress: 0/3 jumps";
				break;

			case TutorialStep.Shooting:
				_titleLabel.Text = "SCRIPT_EXECUTION (FIRE)";
				_instructionLabel.Text = "Press [color=#ffaa00][b]LEFT CLICK[/b][/color] to execute attack scripts.\nFire continuously to test the system.";
				_progressLabel.Text = "Progress: 0/10 scripts executed";
				break;
				
			case TutorialStep.CpuManagement:
				_titleLabel.Text = "CPU_FLUX_MANAGEMENT";
				_instructionLabel.Text = "Firing and Dashing generates [color=#ffaa00]HEAT[/color].\nFire until the bar fills to trigger a controlled [color=#ff0000]OVERLOAD[/color].";
				_progressLabel.Text = "Objective: Overload System (100% CPU)";
				break;
				
			case TutorialStep.AdaptiveSystems:
				_titleLabel.Text = "ADAPTIVE_WEAPON_SYSTEMS";
				_instructionLabel.Text = "Your weapon adapts to CPU Heat.\n[color=#00ff41]LOW HEAT[/color]: Precision | [color=#ffaa00]MED HEAT[/color]: Rapid | [color=#bf00ff]HIGH HEAT[/color]: Chaos\nIncrease heat to > 75% to test Chaos Mode.";
				_progressLabel.Text = "Current Max Heat: 0%";
				break;
				
			case TutorialStep.Parry:
				_titleLabel.Text = "PROTOCOL_REFLECT (PARRY)";
				_instructionLabel.Text = "Press [color=#ffaa00][b]SHIFT[/b][/color] to activate the shield.\nA perfect parry [color=#00ffff]VENTILATES HEAT[/color] instantly.";
				_progressLabel.Text = "Progress: 0/3 attempts";
				break;

			case TutorialStep.Interaction:
				_titleLabel.Text = "SYSTEM_INTERACTION";
				_instructionLabel.Text = "Approach the Data Node and press [color=#ffaa00][b]E[/b][/color].\nRecover integrity or decrypt files.";
				_progressLabel.Text = "Waiting for interaction...";
				SpawnTutorialNode();
				break;
		}
	}
	
	private void UpdateMovementProgress()
	{
		_progressLabel.Text = $"Direcciones verificadas: {_movedDirections.Count}/4";
		
		_arrowUp.Modulate = _movedDirections.Contains("up") ? new Color(0, 1, 0) : new Color(0, 1, 1);
		_arrowDown.Modulate = _movedDirections.Contains("down") ? new Color(0, 1, 0) : new Color(0, 1, 1);
		_arrowLeft.Modulate = _movedDirections.Contains("left") ? new Color(0, 1, 0) : new Color(0, 1, 1);
		_arrowRight.Modulate = _movedDirections.Contains("right") ? new Color(0, 1, 0) : new Color(0, 1, 1);
	}
	
	private void UpdateDashProgress()
	{
		_progressLabel.Text = $"Impulsos realizados: {_dashesPerformed}/3";
	}

	private void UpdateShootingProgress()
	{
		_progressLabel.Text = $"Scripts ejecutados: {_shotsFired}/10";
	}
	
	private void UpdateAdaptiveProgress()
	{
		_progressLabel.Text = $"Current Max Heat: {Mathf.Round(_maxHeatReached * 100)}% / 75%";
		
		if (_maxHeatReached >= 0.75f)
		{
			_progressLabel.AddThemeColorOverride("font_color", new Color("00ff41")); // Green Success
		}
	}

	private void UpdateParryProgress()
	{
		_progressLabel.Text = $"Reflejos activados: {_parriesPerformed}/3";
	}
	
	private void SpawnTutorialNode()
	{
		// Spawn a dummy node for interaction
		var node = new CyberSecurityGame.Entities.DataNode();
		node.Position = new Vector2(GetViewportRect().Size.X / 2, 200);
		AddChild(node);
	}
	
	private void CompleteTutorial()
	{
		_titleLabel.Text = "✅ TRAINING_SEQUENCE_COMPLETE";
		_instructionLabel.Text = "System calibrated. You are ready for the Surface Web.\nRemember: [color=#ffaa00]Manage your CPU_FLUX[/color] or you will be vulnerable.";
		_progressLabel.Text = "Press any key to initialize...";
		_stepLabel.Text = "STATUS: READY";
		
		_overlay.Visible = false;
		
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
}
