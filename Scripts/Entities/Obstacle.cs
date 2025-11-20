using Godot;
using CyberSecurityGame.Components;

namespace CyberSecurityGame.Entities
{
    /// <summary>
    /// Obstáculo del entorno (Data Block, Firewall, Glitch)
    /// </summary>
    public partial class Obstacle : Area2D
    {
        [Export] public float Speed = 150f;
        [Export] public int Damage = 20;
        [Export] public string ObstacleName = "Corrupted Data";

        public override void _Ready()
        {
            AddToGroup("Obstacles");
            BodyEntered += OnBodyEntered;
            
            // Visual simple si no tiene sprite
            if (GetNodeOrNull<Sprite2D>("Sprite2D") == null)
            {
                var sprite = new Sprite2D();
                // Usar un placeholder o generar una textura procedimental
                var image = Image.CreateEmpty(32, 32, false, Image.Format.Rgba8);
                image.Fill(new Color(0.2f, 0, 0.4f)); // Dark Purple (Deep Web vibe)
                
                // Add some "glitch" pixels manually
                for(int i=0; i<10; i++) {
                    int x = (int)(GD.Randi() % 32);
                    int y = (int)(GD.Randi() % 32);
                    image.SetPixel(x, y, new Color(0, 1, 0)); // Green pixels
                }

                var texture = ImageTexture.CreateFromImage(image);
                sprite.Texture = texture;
                AddChild(sprite);

                var collision = new CollisionShape2D();
                var shape = new RectangleShape2D();
                shape.Size = new Vector2(32, 32);
                collision.Shape = shape;
                AddChild(collision);
            }
        }

        public override void _Process(double delta)
        {
            // Mover hacia abajo (simulando avance del jugador hacia arriba/adelante)
            Position += new Vector2(0, Speed * (float)delta);

            // Destruir si sale de la pantalla
            if (Position.Y > 1000) // Asumiendo viewport height ~600-800
            {
                QueueFree();
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.Name == "Player" || body is Player)
            {
                if (body.HasMethod("TakeDamage"))
                {
                    // Usar reflection o interfaz si es posible, por ahora llamada directa
                    body.Call("TakeDamage", Damage, 0); // 0 = Physical/Collision
                }
                QueueFree(); // El obstáculo se destruye al impactar
            }
        }
    }
}
