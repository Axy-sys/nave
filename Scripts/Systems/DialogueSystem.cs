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
                ShowNextLine();
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
                ShowNextLine();
            }
        }

        private async void ShowNextLine()
        {
            if (_dialogueQueue.Count == 0)
            {
                _isDialogueActive = false;
                EmitSignal(SignalName.DialogueEnded);
                return;
            }

            _isDialogueActive = true;
            var line = _dialogueQueue.Dequeue();
            
            EmitSignal(SignalName.DialogueStarted, line.Speaker, line.Text);

            // Wait for duration or input (simplified to duration for flow)
            await ToSignal(GetTree().CreateTimer(line.Duration), "timeout");

            ShowNextLine();
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
