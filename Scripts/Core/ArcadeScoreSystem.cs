using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace CyberSecurityGame.Core
{
    /// <summary>
    /// Sistema de puntuaciones estilo ARCADE RETRO
    /// - Iniciales de 3 letras (AAA, JON, ACE, etc.)
    /// - Top 10 high scores
    /// - Simple y directo como las máquinas de los 80s
    /// </summary>
    public partial class ArcadeScoreSystem : Node
    {
        private static ArcadeScoreSystem _instance;
        public static ArcadeScoreSystem Instance => _instance;

        private const string SAVE_PATH = "user://arcade_scores.json";
        private const int MAX_SCORES = 10;

        private List<ArcadeScore> _highScores = new();

        [Signal] public delegate void NewHighScoreEventHandler(int position, int score);

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            LoadScores();
            
            // Si no hay scores, crear algunos por defecto estilo arcade
            if (_highScores.Count == 0)
            {
                CreateDefaultScores();
            }
        }

        private void CreateDefaultScores()
        {
            // Scores por defecto como en los arcades clásicos
            _highScores = new List<ArcadeScore>
            {
                new ArcadeScore { Initials = "ACE", Score = 50000, Wave = 10 },
                new ArcadeScore { Initials = "PRO", Score = 40000, Wave = 8 },
                new ArcadeScore { Initials = "CPU", Score = 30000, Wave = 7 },
                new ArcadeScore { Initials = "NET", Score = 25000, Wave = 6 },
                new ArcadeScore { Initials = "SYS", Score = 20000, Wave = 5 },
                new ArcadeScore { Initials = "BIT", Score = 15000, Wave = 4 },
                new ArcadeScore { Initials = "HEX", Score = 10000, Wave = 3 },
                new ArcadeScore { Initials = "RAM", Score = 7500, Wave = 3 },
                new ArcadeScore { Initials = "ROM", Score = 5000, Wave = 2 },
                new ArcadeScore { Initials = "ASM", Score = 2500, Wave = 1 },
            };
            SaveScores();
        }

        /// <summary>
        /// Verifica si el score califica para el top 10
        /// </summary>
        public bool IsHighScore(int score)
        {
            if (_highScores.Count < MAX_SCORES) return true;
            return score > _highScores.Last().Score;
        }

        /// <summary>
        /// Obtiene la posición que tendría este score (1-10, o 0 si no califica)
        /// </summary>
        public int GetScorePosition(int score)
        {
            for (int i = 0; i < _highScores.Count; i++)
            {
                if (score > _highScores[i].Score)
                    return i + 1;
            }
            if (_highScores.Count < MAX_SCORES)
                return _highScores.Count + 1;
            return 0;
        }

        /// <summary>
        /// Añade un nuevo high score con iniciales de 3 letras
        /// </summary>
        public int AddScore(string initials, int score, int wave)
        {
            // Validar y formatear iniciales (exactamente 3 letras)
            initials = FormatInitials(initials);

            var newScore = new ArcadeScore
            {
                Initials = initials,
                Score = score,
                Wave = wave,
                Date = DateTime.Now.ToString("MM/dd")
            };

            _highScores.Add(newScore);
            _highScores = _highScores.OrderByDescending(s => s.Score).Take(MAX_SCORES).ToList();

            int position = _highScores.FindIndex(s => s == newScore) + 1;

            SaveScores();

            if (position > 0 && position <= 10)
            {
                EmitSignal(SignalName.NewHighScore, position, score);
            }

            GD.Print($"🏆 ARCADE SCORE: {initials} - {score:N0} (#{position})");
            return position;
        }

        /// <summary>
        /// Formatea las iniciales: exactamente 3 letras mayúsculas
        /// </summary>
        public static string FormatInitials(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "AAA";

            // Solo letras
            string letters = new string(input.ToUpper().Where(char.IsLetter).ToArray());

            // Exactamente 3 letras
            if (letters.Length >= 3)
                return letters.Substring(0, 3);
            
            // Rellenar con A si faltan
            return letters.PadRight(3, 'A');
        }

        /// <summary>
        /// Obtiene el top de scores
        /// </summary>
        public List<ArcadeScore> GetTopScores(int count = 10)
        {
            return _highScores.Take(Math.Min(count, MAX_SCORES)).ToList();
        }

        /// <summary>
        /// Obtiene el score más alto
        /// </summary>
        public int GetTopScore()
        {
            return _highScores.Count > 0 ? _highScores[0].Score : 0;
        }

        /// <summary>
        /// Formatea el leaderboard estilo arcade
        /// </summary>
        public string GetFormattedLeaderboard()
        {
            var lines = new List<string>();
            lines.Add("═══════════════════════════════");
            lines.Add("      ★ HIGH SCORES ★          ");
            lines.Add("═══════════════════════════════");
            lines.Add(" #   NAME   SCORE     WAVE     ");
            lines.Add("───────────────────────────────");

            for (int i = 0; i < _highScores.Count; i++)
            {
                var s = _highScores[i];
                string rank = (i + 1).ToString().PadLeft(2);
                string initials = s.Initials.PadRight(3);
                string score = s.Score.ToString("N0").PadLeft(9);
                string wave = $"W{s.Wave}".PadLeft(4);
                
                lines.Add($" {rank}. {initials}  {score}   {wave}");
            }

            lines.Add("═══════════════════════════════");
            return string.Join("\n", lines);
        }

        private void LoadScores()
        {
            if (!FileAccess.FileExists(SAVE_PATH))
                return;

            try
            {
                using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
                string json = file.GetAsText();
                _highScores = JsonSerializer.Deserialize<List<ArcadeScore>>(json) ?? new List<ArcadeScore>();
                GD.Print($"🎮 Arcade scores cargados: {_highScores.Count} entries");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error cargando arcade scores: {e.Message}");
                _highScores = new List<ArcadeScore>();
            }
        }

        private void SaveScores()
        {
            try
            {
                string json = JsonSerializer.Serialize(_highScores, new JsonSerializerOptions { WriteIndented = true });
                using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
                file.StoreString(json);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error guardando arcade scores: {e.Message}");
            }
        }

        /// <summary>
        /// Reset para testing
        /// </summary>
        public void ResetScores()
        {
            _highScores.Clear();
            CreateDefaultScores();
        }
    }

    [Serializable]
    public class ArcadeScore
    {
        public string Initials { get; set; } = "AAA";
        public int Score { get; set; }
        public int Wave { get; set; }
        public string Date { get; set; } = "";
    }
}
