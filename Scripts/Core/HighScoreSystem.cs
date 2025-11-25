using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CyberSecurityGame.Core
{
    /// <summary>
    /// Sistema de puntuaciones altas con persistencia local
    /// Guarda los récords en un archivo JSON
    /// </summary>
    public partial class HighScoreSystem : Node
    {
        private static HighScoreSystem _instance;
        public static HighScoreSystem Instance => _instance;

        private const string SAVE_PATH = "user://highscores.json";
        private const int MAX_SCORES = 10;

        private List<ScoreEntry> _highScores = new List<ScoreEntry>();
        
        public IReadOnlyList<ScoreEntry> HighScores => _highScores.AsReadOnly();

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            LoadScores();
        }

        /// <summary>
        /// Intenta añadir una puntuación a la tabla
        /// </summary>
        /// <returns>La posición en el ranking (1-10) o -1 si no entró</returns>
        public int TryAddScore(string playerName, int score, int wave, int level)
        {
            var entry = new ScoreEntry
            {
                PlayerName = string.IsNullOrWhiteSpace(playerName) ? "ANON" : playerName.ToUpper(),
                Score = score,
                Wave = wave,
                Level = level,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
            };

            _highScores.Add(entry);
            _highScores = _highScores
                .OrderByDescending(s => s.Score)
                .ThenByDescending(s => s.Wave)
                .Take(MAX_SCORES)
                .ToList();

            SaveScores();

            int position = _highScores.FindIndex(s => 
                s.Score == entry.Score && 
                s.PlayerName == entry.PlayerName && 
                s.Date == entry.Date) + 1;

            return position <= MAX_SCORES ? position : -1;
        }

        /// <summary>
        /// Verifica si una puntuación entraría en el top 10
        /// </summary>
        public bool IsHighScore(int score)
        {
            if (_highScores.Count < MAX_SCORES) return true;
            return score > _highScores.Last().Score;
        }

        /// <summary>
        /// Obtiene la puntuación más alta
        /// </summary>
        public int GetTopScore()
        {
            return _highScores.Count > 0 ? _highScores[0].Score : 0;
        }

        /// <summary>
        /// Obtiene el ranking formateado para mostrar
        /// </summary>
        public string GetFormattedLeaderboard()
        {
            if (_highScores.Count == 0)
            {
                return "> NO HAY RÉCORDS AÚN\n> ¡SÉ EL PRIMERO EN DEFENDER EL SISTEMA!";
            }

            var lines = new List<string>();
            lines.Add("╔════╦════════════╦════════╦═══════╦════════════╗");
            lines.Add("║ #  ║ NOMBRE     ║ SCORE  ║ NIVEL ║ FECHA      ║");
            lines.Add("╠════╬════════════╬════════╬═══════╬════════════╣");

            for (int i = 0; i < _highScores.Count; i++)
            {
                var s = _highScores[i];
                string rank = (i + 1).ToString().PadLeft(2);
                string name = s.PlayerName.PadRight(10).Substring(0, 10);
                string score = s.Score.ToString("N0").PadLeft(6);
                string level = $"L{s.Level}W{s.Wave}".PadLeft(5);
                string date = s.Date.Substring(0, 10);
                
                lines.Add($"║ {rank} ║ {name} ║ {score} ║ {level} ║ {date} ║");
            }

            lines.Add("╚════╩════════════╩════════╩═══════╩════════════╝");

            return string.Join("\n", lines);
        }

        private void LoadScores()
        {
            if (!FileAccess.FileExists(SAVE_PATH))
            {
                _highScores = new List<ScoreEntry>();
                GD.Print("📊 No hay récords previos. Iniciando tabla vacía.");
                return;
            }

            try
            {
                using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
                string json = file.GetAsText();
                
                var data = JsonSerializer.Deserialize<HighScoreData>(json);
                _highScores = data?.Scores ?? new List<ScoreEntry>();
                
                GD.Print($"📊 Cargados {_highScores.Count} récords");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error cargando récords: {e.Message}");
                _highScores = new List<ScoreEntry>();
            }
        }

        private void SaveScores()
        {
            try
            {
                var data = new HighScoreData { Scores = _highScores };
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
                file.StoreString(json);
                
                GD.Print("💾 Récords guardados");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error guardando récords: {e.Message}");
            }
        }

        /// <summary>
        /// Borra todos los récords (para debug)
        /// </summary>
        public void ClearAllScores()
        {
            _highScores.Clear();
            SaveScores();
            GD.Print("🗑️ Récords borrados");
        }
    }

    [Serializable]
    public class ScoreEntry
    {
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int Wave { get; set; }
        public int Level { get; set; }
        public string Date { get; set; }
    }

    [Serializable]
    public class HighScoreData
    {
        public List<ScoreEntry> Scores { get; set; }
    }
}
