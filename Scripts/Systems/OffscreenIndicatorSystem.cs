using Godot;
using System.Collections.Generic;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Systems
{
    /// <summary>
    /// Sistema de Indicadores de Enemigos Fuera de Pantalla
    /// 
    /// UX PROBLEM:
    /// El jugador no sabe de dónde vienen los enemigos cuando están
    /// fuera de la pantalla, causando muertes inesperadas.
    /// 
    /// SOLUCIÓN:
    /// Flechas/indicadores en los bordes de la pantalla que apuntan
    /// hacia enemigos fuera del área visible.
    /// 
    /// INSPIRADO EN:
    /// - Geometry Wars (flechas de borde)
    /// - Halo (indicadores de daño direccional)
    /// - Fortnite (indicadores de enemigos)
    /// </summary>
    public partial class OffscreenIndicatorSystem : CanvasLayer
    {
        private static OffscreenIndicatorSystem _instance;
        public static OffscreenIndicatorSystem Instance => _instance;

        // Configuración
        [Export] public float IndicatorMargin = 40f;        // Distancia del borde
        [Export] public float IndicatorSize = 24f;          // Tamaño de las flechas
        [Export] public float MinDistance = 100f;           // Distancia mínima para mostrar
        [Export] public bool ShowDistance = true;           // Mostrar distancia al enemigo
        [Export] public int MaxIndicators = 10;             // Máximo de indicadores
        
        // Colores por tipo de enemigo
        private static readonly Color MALWARE_COLOR = new Color("#ff5555");      // Rojo
        private static readonly Color PHISHING_COLOR = new Color("#ffaa00");     // Naranja
        private static readonly Color DDOS_COLOR = new Color("#00d4ff");         // Cyan
        private static readonly Color SQL_COLOR = new Color("#bf00ff");          // Púrpura
        private static readonly Color BOSS_COLOR = new Color("#ff0000");         // Rojo brillante
        private static readonly Color DEFAULT_COLOR = new Color("#ffffff");      // Blanco
        
        // Pool de indicadores
        private List<IndicatorNode> _indicators = new List<IndicatorNode>();
        private Node2D _player;
        private Rect2 _screenRect;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            Layer = 50; // Encima del juego, debajo de UI
            
            // Crear pool de indicadores
            for (int i = 0; i < MaxIndicators; i++)
            {
                var indicator = new IndicatorNode();
                indicator.Visible = false;
                AddChild(indicator);
                _indicators.Add(indicator);
            }
            
            GD.Print("[OffscreenIndicator] Sistema de indicadores inicializado");
        }

        public override void _Process(double delta)
        {
            // Buscar jugador si no lo tenemos
            if (_player == null || !IsInstanceValid(_player))
            {
                _player = GetTree().Root.GetNodeOrNull<Node2D>("Main/Player");
                if (_player == null) return;
            }
            
            // Obtener tamaño de pantalla
            _screenRect = GetViewport().GetVisibleRect();
            Vector2 screenCenter = _screenRect.Size / 2;
            
            // Obtener todos los enemigos
            var enemies = GetTree().GetNodesInGroup("Enemy");
            
            // Ocultar todos los indicadores primero
            foreach (var indicator in _indicators)
            {
                indicator.Visible = false;
            }
            
            int indicatorIndex = 0;
            
            foreach (var enemyNode in enemies)
            {
                if (indicatorIndex >= MaxIndicators) break;
                if (enemyNode is not Node2D enemy) continue;
                if (!IsInstanceValid(enemy)) continue;
                
                // Verificar si está fuera de pantalla
                Vector2 enemyScreenPos = enemy.GlobalPosition;
                
                // Convertir a coordenadas de pantalla relativas al jugador
                Vector2 playerPos = _player.GlobalPosition;
                Vector2 relativePos = enemyScreenPos - playerPos + screenCenter;
                
                // Verificar si está dentro de la pantalla (con margen)
                float margin = 50f;
                bool isOnScreen = relativePos.X > margin && 
                                  relativePos.X < _screenRect.Size.X - margin &&
                                  relativePos.Y > margin && 
                                  relativePos.Y < _screenRect.Size.Y - margin;
                
                if (!isOnScreen)
                {
                    // Calcular posición del indicador en el borde
                    Vector2 direction = (enemyScreenPos - playerPos).Normalized();
                    float distance = playerPos.DistanceTo(enemyScreenPos);
                    
                    if (distance < MinDistance) continue;
                    
                    // Calcular posición en el borde de la pantalla
                    Vector2 indicatorPos = CalculateEdgePosition(direction, screenCenter);
                    
                    // Obtener tipo de enemigo para color
                    string enemyType = GetEnemyType(enemy);
                    Color color = GetColorForType(enemyType);
                    bool isBoss = enemy.IsInGroup("Boss");
                    
                    // Configurar indicador
                    var indicator = _indicators[indicatorIndex];
                    indicator.SetIndicator(indicatorPos, direction, color, distance, isBoss);
                    indicator.Visible = true;
                    
                    indicatorIndex++;
                }
            }
        }

        private Vector2 CalculateEdgePosition(Vector2 direction, Vector2 screenCenter)
        {
            // Calcular intersección con los bordes de la pantalla
            Vector2 screenSize = _screenRect.Size;
            float margin = IndicatorMargin;
            
            // Límites de la pantalla con margen
            float left = margin;
            float right = screenSize.X - margin;
            float top = margin;
            float bottom = screenSize.Y - margin;
            
            // Calcular punto de intersección
            Vector2 pos = screenCenter;
            
            // Escalar la dirección para que llegue al borde
            float scale = 1000f;
            Vector2 endPoint = screenCenter + direction * scale;
            
            // Clamp al área de pantalla
            pos.X = Mathf.Clamp(endPoint.X, left, right);
            pos.Y = Mathf.Clamp(endPoint.Y, top, bottom);
            
            // Si llegamos a una esquina, ajustar
            if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
            {
                // Más horizontal
                pos.X = direction.X > 0 ? right : left;
                pos.Y = Mathf.Clamp(screenCenter.Y + direction.Y * (right - screenCenter.X) / Mathf.Abs(direction.X), top, bottom);
            }
            else
            {
                // Más vertical
                pos.Y = direction.Y > 0 ? bottom : top;
                pos.X = Mathf.Clamp(screenCenter.X + direction.X * (bottom - screenCenter.Y) / Mathf.Abs(direction.Y), left, right);
            }
            
            return pos;
        }

        private string GetEnemyType(Node2D enemy)
        {
            // Intentar obtener el tipo del nombre o metadata
            string name = enemy.Name.ToString().ToLower();
            
            if (name.Contains("malware")) return "Malware";
            if (name.Contains("phishing")) return "Phishing";
            if (name.Contains("ddos")) return "DDoS";
            if (name.Contains("sql")) return "SQLInjection";
            if (name.Contains("brute")) return "BruteForce";
            if (name.Contains("ransom")) return "Ransomware";
            if (name.Contains("worm")) return "Worm";
            if (name.Contains("trojan")) return "Trojan";
            if (name.Contains("boss")) return "Boss";
            
            return "Unknown";
        }

        private Color GetColorForType(string enemyType)
        {
            return enemyType switch
            {
                "Malware" => MALWARE_COLOR,
                "Phishing" => PHISHING_COLOR,
                "DDoS" => DDOS_COLOR,
                "SQLInjection" => SQL_COLOR,
                "BruteForce" => new Color("#ff8800"),
                "Ransomware" => new Color("#ff0066"),
                "Worm" => new Color("#00ff88"),
                "Trojan" => new Color("#8800ff"),
                "Boss" => BOSS_COLOR,
                _ => DEFAULT_COLOR
            };
        }
    }

    /// <summary>
    /// Nodo individual de indicador (flecha + opcional distancia)
    /// </summary>
    public partial class IndicatorNode : Node2D
    {
        private Polygon2D _arrow;
        private Label _distanceLabel;
        private float _pulseTime = 0f;
        private bool _isBoss = false;

        public override void _Ready()
        {
            // Crear flecha
            _arrow = new Polygon2D();
            _arrow.Polygon = new Vector2[]
            {
                new Vector2(0, -12),    // Punta
                new Vector2(-8, 8),     // Esquina izquierda
                new Vector2(0, 4),      // Centro trasero
                new Vector2(8, 8),      // Esquina derecha
            };
            _arrow.Color = Colors.White;
            AddChild(_arrow);
            
            // Etiqueta de distancia
            _distanceLabel = new Label();
            _distanceLabel.Position = new Vector2(-20, 15);
            _distanceLabel.AddThemeFontSizeOverride("font_size", 10);
            _distanceLabel.AddThemeColorOverride("font_color", Colors.White);
            AddChild(_distanceLabel);
        }

        public override void _Process(double delta)
        {
            if (!Visible) return;
            
            // Efecto de pulso para bosses
            if (_isBoss)
            {
                _pulseTime += (float)delta * 5f;
                float pulse = 0.7f + 0.3f * Mathf.Sin(_pulseTime);
                Scale = new Vector2(pulse, pulse) * 1.5f;
            }
        }

        public void SetIndicator(Vector2 position, Vector2 direction, Color color, float distance, bool isBoss)
        {
            GlobalPosition = position;
            Rotation = direction.Angle() + Mathf.Pi / 2; // Apuntar hacia el enemigo
            
            _arrow.Color = color;
            _isBoss = isBoss;
            
            if (isBoss)
            {
                Scale = new Vector2(1.5f, 1.5f);
                _arrow.Color = new Color(color.R, color.G, color.B, 1f);
            }
            else
            {
                Scale = Vector2.One;
            }
            
            // Mostrar distancia
            if (distance > 200)
            {
                _distanceLabel.Text = $"{(int)distance}";
                _distanceLabel.Visible = true;
                _distanceLabel.Rotation = -Rotation; // Mantener texto horizontal
            }
            else
            {
                _distanceLabel.Visible = false;
            }
            
            // Transparencia basada en distancia (más cerca = más visible)
            float alpha = Mathf.Clamp(1f - (distance - 100f) / 800f, 0.4f, 1f);
            Modulate = new Color(1, 1, 1, alpha);
        }
    }
}
