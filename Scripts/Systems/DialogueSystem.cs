using Godot;
using System;
using System.Collections.Generic;

namespace CyberSecurityGame.Systems
{
    public partial class DialogueSystem : Node
    {
        private static DialogueSystem _instance;
        public static DialogueSystem Instance => _instance;

        [Signal] public delegate void DialogueStartedEventHandler(string speaker, string text);
        [Signal] public delegate void DialogueEndedEventHandler();

        private Queue<DialogueLine> _dialogueQueue = new Queue<DialogueLine>();
        private bool _isDialogueActive = false;

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
            Engine.TimeScale = 0.1f; // Matrix/Hacker slow motion effect
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

            // Wait for duration (adjusted for TimeScale)
            // Since TimeScale is 0.1, a 3s timer will take 30s real time if we use SceneTreeTimer without ignoring time scale.
            // We must use ProcessAlways or ignore time scale?
            // GetTree().CreateTimer(duration, false) -> false means respect time scale.
            // We want the dialogue to last 3 seconds REAL time.
            // So we should use CreateTimer(duration, true, false, true) -> ignore_time_scale = true.
            // In Godot 4 C#: CreateTimer(timeSec, processAlways, processInPhysics, ignoreTimeScale)
            
            await ToSignal(GetTree().CreateTimer(line.Duration, true, false, true), "timeout");

            ShowNextLine();
        }

        private void EndDialogueSequence()
        {
            _isDialogueActive = false;
            Engine.TimeScale = 1.0f; // Restore normal speed
            EmitSignal(SignalName.DialogueEnded);
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
