using Godot;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Entities
{
    public partial class DataNode : Area2D
    {
        [Export] public float HackDuration = 2.0f;
        [Export] public int PointsReward = 500;
        
        private bool _isPlayerNearby = false;
        private float _currentHackTime = 0f;
        private bool _isHacked = false;
        private ColorRect _visual;
        private Label _label;

        public override void _Ready()
        {
            // Setup Collision
            var shape = new CollisionShape2D();
            var circle = new CircleShape2D();
            circle.Radius = 30f;
            shape.Shape = circle;
            AddChild(shape);

            // Setup Visuals
            _visual = new ColorRect();
            _visual.Size = new Vector2(40, 40);
            _visual.Position = new Vector2(-20, -20);
            _visual.Color = new Color(0, 0, 1); // Blue (Unsecured)
            AddChild(_visual);

            _label = new Label();
            _label.Text = "DATA NODE";
            _label.Position = new Vector2(-30, -40);
            _label.AddThemeFontSizeOverride("font_size", 10);
            AddChild(_label);

            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        public override void _Process(double delta)
        {
            if (_isHacked) return;

            if (_isPlayerNearby && Input.IsKeyPressed(Key.E))
            {
                _currentHackTime += (float)delta;
                float progress = _currentHackTime / HackDuration;
                
                // Visual Feedback
                _visual.Color = new Color(progress, 0, 1 - progress); // Blue to Red
                _label.Text = $"HACKING... {(int)(progress * 100)}%";

                if (_currentHackTime >= HackDuration)
                {
                    CompleteHack();
                }
            }
            else if (_currentHackTime > 0)
            {
                // Decay progress if key released
                _currentHackTime -= (float)delta * 2;
                _currentHackTime = Mathf.Max(0, _currentHackTime);
                _visual.Color = new Color(0, 0, 1);
                _label.Text = "DATA NODE";
            }
        }

        private void CompleteHack()
        {
            _isHacked = true;
            _visual.Color = new Color(0, 1, 0); // Green (Secured)
            _label.Text = "SECURED";
            
            GameEventBus.Instance.EmitEnemyDefeated("Data Node Secured", PointsReward);
            GD.Print("🔓 Data Node Hacked!");
            
            // Optional: Heal player or give powerup
            var player = GetTree().Root.GetNodeOrNull<Player>("Main/Player");
            if (player != null)
            {
                player.Heal(20);
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is Player)
            {
                _isPlayerNearby = true;
                _label.Text = "PRESS 'E' TO HACK";
            }
        }

        private void OnBodyExited(Node2D body)
        {
            if (body is Player)
            {
                _isPlayerNearby = false;
                if (!_isHacked) _label.Text = "DATA NODE";
            }
        }
    }
}
