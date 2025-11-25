using Godot;
using System;
using System.Collections.Generic;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// NOTA: Este sistema ha sido parcialmente reemplazado por MissionIntroSystem
    /// para los diálogos de inicio de misión.
    /// 
    /// Se mantiene activo para casos específicos pero SIN slow-motion
    /// para no interferir con el gameplay.
    /// </summary>
    public partial class DialogueSystem : Node
    {
        private static DialogueSystem _instance;
        public static DialogueSystem Instance => _instance;

        [Signal] public delegate void DialogueStartedEventHandler(string speaker, string text);
        [Signal] public delegate void DialogueEndedEventHandler();

        private Queue<DialogueLine> _dialogueQueue = new Queue<DialogueLine>();
        private bool _isDialogueActive = false;
        
        // DESACTIVAR slow-motion para no interferir con gameplay
        private bool _enableSlowMotion = false;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
        }

        public void ShowDialogue(string speaker, string text, float duration = 3.0f)
        {
            _dialogueQueue.Enqueue(new DialogueLine(speaker, text, duration));
            if (!_isDialogueActive)
            {
                StartDialogueSequence();
            }
        }

        public void ShowSequence(List<DialogueLine> lines)
        {
            foreach(var line in lines)
            {
                _dialogueQueue.Enqueue(line);
            }
            if (!_isDialogueActive)
            {
                StartDialogueSequence();
            }
        }

        private void StartDialogueSequence()
        {
            _isDialogueActive = true;
            
            // NOTA: Slow-motion desactivado para evitar problemas de UX
            // El MissionIntroSystem maneja la pausa del juego de forma más controlada
            if (_enableSlowMotion)
            {
                Engine.TimeScale = 0.3f; // Menos agresivo que 0.1
            }
            
            ShowNextLine();
        }

        private async void ShowNextLine()
        {
            if (_dialogueQueue.Count == 0)
            {
                EndDialogueSequence();
                return;
            }

            var line = _dialogueQueue.Dequeue();
            
            EmitSignal(SignalName.DialogueStarted, line.Speaker, line.Text);

            // Timer que ignora el time scale para duración consistente
            await ToSignal(GetTree().CreateTimer(line.Duration, true, false, true), "timeout");

            ShowNextLine();
        }

        private void EndDialogueSequence()
        {
            _isDialogueActive = false;
            
            if (_enableSlowMotion)
            {
                Engine.TimeScale = 1.0f;
            }
            
            EmitSignal(SignalName.DialogueEnded);
        }
        
        /// <summary>
        /// Limpiar cola de diálogos pendientes
        /// </summary>
        public void ClearQueue()
        {
            _dialogueQueue.Clear();
            if (_isDialogueActive)
            {
                EndDialogueSequence();
            }
        }
        
        /// <summary>
        /// Habilitar/deshabilitar slow-motion (desactivado por defecto)
        /// </summary>
        public void SetSlowMotionEnabled(bool enabled)
        {
            _enableSlowMotion = enabled;
        }
    }

    public struct DialogueLine
    {
        public string Speaker;
        public string Text;
        public float Duration;

        public DialogueLine(string speaker, string text, float duration)
        {
            Speaker = speaker;
            Text = text;
            Duration = duration;
        }
    }
}
