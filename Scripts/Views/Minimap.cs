using Godot;
using System.Collections.Generic;
using System.Linq;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Views
{
	/// <summary>
	/// Minimapa con detección de enemigos en área circular
	/// </summary>
	public partial class Minimap : Control
	{
		[Export] public float DetectionRadius = 500f;
		[Export] public float MinimapSize = 150f;
		[Export] public Color BackgroundColor = new Color(0.05f, 0.0f, 0.05f, 0.9f); // Deep Purple/Black
		[Export] public Color BorderColor = new Color("bf00ff"); // Rippier Purple
		[Export] public Color PlayerColor = new Color("00ff41"); // Terminal Green
		[Export] public Color EnemyColor = new Color(1f, 0, 0, 1f); // Alert Red
		[Export] public Color RadarColor = new Color("bf00ff"); // Purple Radar
		
		private Node2D _player;
		private List<Node2D> _enemies = new List<Node2D>();
		private float _radarPulseTime = 0f;
		private const float RADAR_PULSE_SPEED = 2f;

		public override void _Ready()
		{
			SetAnchorsPreset(LayoutPreset.TopRight);
			Position = new Vector2(-MinimapSize - 20, 20);
			CustomMinimumSize = new Vector2(MinimapSize, MinimapSize);
			
			// RadarColor needs transparency
			RadarColor = new Color(RadarColor.R, RadarColor.G, RadarColor.B, 0.3f);
			
			// Suscribirse a eventos
			// Los enemigos se buscarán dinámicamente cada frame
			
			// Buscar el jugador
			CallDeferred(nameof(FindPlayer));
		}

		private void FindPlayer()
		{
			_player = GetTree().Root.GetNode<Node2D>("Main/Player");
			if (_player == null)
			{
				GD.Print("⚠️ Minimap: No se encontró el jugador");
			}
		}

		private void UpdateEnemiesList()
		{
			_enemies.Clear();
			var main = GetTree().Root.GetNodeOrNull<Node2D>("Main");
			if (main != null)
			{
				foreach (var node in main.GetChildren())
				{
					if (node is CharacterBody2D body && node.Name.ToString().Contains("Enemy"))
					{
						_enemies.Add(body);
					}
				}
			}
		}

		public override void _Process(double delta)
		{
			_radarPulseTime += (float)delta * RADAR_PULSE_SPEED;
			if (_radarPulseTime >= 1f) _radarPulseTime = 0f;
			
			// Actualizar lista de enemigos cada frame
			UpdateEnemiesList();
			
			QueueRedraw();
		}

		public override void _Draw()
		{
			if (_player == null || !IsInstanceValid(_player)) return;

			Vector2 center = CustomMinimumSize / 2;
			float radius = MinimapSize / 2 - 5;

			// Fondo circular
			DrawCircle(center, radius, BackgroundColor);
			
			// Borde
			DrawArc(center, radius, 0, Mathf.Tau, 64, BorderColor, 2f);
			
			// Radio de detección (pulso animado)
			float pulseRadius = radius * _radarPulseTime;
			DrawArc(center, pulseRadius, 0, Mathf.Tau, 32, RadarColor, 1.5f);
			
			// Líneas de cuadrícula
			DrawLine(center + new Vector2(-radius, 0), center + new Vector2(radius, 0), 
				new Color(BorderColor, 0.2f), 1f);
			DrawLine(center + new Vector2(0, -radius), center + new Vector2(0, radius), 
				new Color(BorderColor, 0.2f), 1f);
			
			// Círculos de rango
			DrawArc(center, radius * 0.33f, 0, Mathf.Tau, 32, new Color(BorderColor, 0.15f), 1f);
			DrawArc(center, radius * 0.66f, 0, Mathf.Tau, 32, new Color(BorderColor, 0.15f), 1f);

			// Jugador en el centro
			DrawCircle(center, 4, PlayerColor);
			DrawCircle(center, 6, new Color(PlayerColor, 0.3f));
			
			// Dirección del jugador (flecha)
			Vector2 playerRotation = Vector2.Up.Rotated(_player.Rotation);
			DrawLine(center, center + playerRotation * 8, PlayerColor, 2f);

			// Dibujar enemigos dentro del radio de detección
			Vector2 playerPos = _player.GlobalPosition;
			
			foreach (var enemy in _enemies)
			{
				if (!IsInstanceValid(enemy)) continue;
				
				Vector2 enemyPos = enemy.GlobalPosition;
				float distance = playerPos.DistanceTo(enemyPos);
				
				// Solo mostrar enemigos dentro del radio de detección
				if (distance <= DetectionRadius)
				{
					// Convertir posición mundial a posición en minimap
					Vector2 relativePos = (enemyPos - playerPos) / DetectionRadius;
					Vector2 minimapPos = center + relativePos * radius * 0.9f;
					
					// Clampar dentro del círculo
					if (minimapPos.DistanceTo(center) > radius - 3)
					{
						minimapPos = center + (minimapPos - center).Normalized() * (radius - 3);
					}
					
					// Dibujar enemigo
					float enemySize = 3f;
					
					// Pulsar si está cerca
					if (distance < DetectionRadius * 0.3f)
					{
						enemySize += Mathf.Sin(_radarPulseTime * Mathf.Tau) * 1.5f;
					}
					
					DrawCircle(minimapPos, enemySize, EnemyColor);
					DrawCircle(minimapPos, enemySize + 2, new Color(EnemyColor, 0.3f));
					
					// Indicador de distancia (puntos)
					if (distance < DetectionRadius * 0.5f)
					{
						DrawCircle(minimapPos, 1, new Color(1, 1, 0, 0.6f));
					}
				}
			}
			
			// Contador de enemigos detectados
			int enemiesInRange = _enemies.Count(e => 
				IsInstanceValid(e) && playerPos.DistanceTo(e.GlobalPosition) <= DetectionRadius
			);
			
			// Texto de contador
			var font = ThemeDB.FallbackFont;
			var fontSize = ThemeDB.FallbackFontSize;
			string countText = $"{enemiesInRange}";
			Vector2 textSize = font.GetStringSize(countText, HorizontalAlignment.Center, -1, fontSize);
			Vector2 textPos = new Vector2(center.X - textSize.X / 2, MinimapSize - 10);
			
			DrawString(font, textPos, countText, HorizontalAlignment.Center, -1, fontSize, 
				enemiesInRange > 5 ? new Color(1, 0, 0) : EnemyColor);
			
			// Texto "RADAR"
			Vector2 labelPos = new Vector2(center.X - 20, 15);
			DrawString(font, labelPos, "RADAR", HorizontalAlignment.Center, -1, fontSize - 2, 
				new Color(BorderColor, 0.7f));
		}

		public override void _ExitTree()
		{
			// Limpieza si es necesaria
		}
	}
}
