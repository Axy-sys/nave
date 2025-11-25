using Godot;
using System;
using System.Collections.Generic;
using CyberSecurityGame.Core.Events;
using CyberSecurityGame.Views;

namespace CyberSecurityGame.Education
{
    /// <summary>
    /// Sistema de Aprendizaje Contextual
    /// 
    /// PROBLEMA ORIGINAL:
    /// Los quizzes aparecían en momentos aleatorios, interrumpiendo el gameplay
    /// y frustrando al jugador.
    /// 
    /// SOLUCIÓN:
    /// Quizzes aparecen en MOMENTOS EDUCATIVOS ÓPTIMOS:
    /// 1. Al morir por un tipo de enemigo (máximo engagement emocional)
    /// 2. Al completar una oleada (pausa natural)
    /// 3. Al descubrir una nueva amenaza (curiosidad activa)
    /// 4. Después de N muertes por el mismo enemigo (frustración = necesidad de aprender)
    /// 
    /// INSPIRADO EN:
    /// - Dark Souls "You Died" screens (reflexión tras fallo)
    /// - Duolingo timing (momentos de máxima retención)
    /// - Roguelike death screens (análisis post-mortem)
    /// </summary>
    public partial class ContextualLearningSystem : Node
    {
        private static ContextualLearningSystem _instance;
        public static ContextualLearningSystem Instance => _instance;

        // Tracking de muertes para aprendizaje contextual
        private Dictionary<string, int> _deathsByEnemyType = new Dictionary<string, int>();
        private string _lastDamageSource = "";
        private int _consecutiveDeathsSameEnemy = 0;
        
        // Configuración de timing
        [Export] public int DeathsBeforeQuiz = 2;        // Quiz después de N muertes por mismo tipo
        [Export] public int WavesBeforeReviewQuiz = 5;   // Quiz de repaso cada N oleadas
        [Export] public float QuizDelayAfterDeath = 1.5f; // Delay antes de mostrar quiz
        
        // Estado
        private bool _quizPending = false;
        private string _pendingQuizCategory = "";
        private int _lastQuizWave = 0;
        private int _currentWave = 0;
        
        // Estadísticas de aprendizaje
        public int ContextualQuizzesShown { get; private set; } = 0;
        public int ContextualQuizzesCorrect { get; private set; } = 0;
        public float LearningEfficiency => ContextualQuizzesShown > 0 
            ? (float)ContextualQuizzesCorrect / ContextualQuizzesShown * 100f 
            : 0f;

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                QueueFree();
                return;
            }
            _instance = this;
            
            SubscribeToEvents();
            GD.Print("[ContextualLearning] Sistema de aprendizaje contextual iniciado");
        }

        private void SubscribeToEvents()
        {
            GameEventBus.Instance.OnPlayerDied += OnPlayerDied;
            GameEventBus.Instance.OnPlayerDamagedByEnemy += OnPlayerDamaged;
            GameEventBus.Instance.OnWaveCompleted += OnWaveCompleted;
            GameEventBus.Instance.OnQuestionAnswered += OnQuestionAnswered;
            GameEventBus.Instance.OnWaveAnnounced += OnWaveAnnounced;
        }

        // ═══════════════════════════════════════════════════════════════════
        // MOMENTO EDUCATIVO #1: Muerte por enemigo específico
        // 
        // UX: El jugador acaba de morir y está emocionalmente activo.
        // Es el MEJOR momento para enseñar sobre ese tipo de amenaza.
        // "¿Por qué morí? ¿Cómo lo evito?"
        // ═══════════════════════════════════════════════════════════════════

        private void OnPlayerDied()
        {
            if (string.IsNullOrEmpty(_lastDamageSource)) return;
            
            // Registrar muerte por este tipo
            if (!_deathsByEnemyType.ContainsKey(_lastDamageSource))
            {
                _deathsByEnemyType[_lastDamageSource] = 0;
            }
            _deathsByEnemyType[_lastDamageSource]++;
            
            int deaths = _deathsByEnemyType[_lastDamageSource];
            
            GD.Print($"[ContextualLearning] Muerte por {_lastDamageSource} (total: {deaths})");
            
            // ═══════════════════════════════════════════════════════════════════
            // DECISIÓN: ¿Mostrar quiz ahora?
            // 
            // Criterios:
            // 1. Primera muerte por este tipo → Mostrar info (no quiz)
            // 2. Segunda muerte → Quiz contextual
            // 3. Tercera+ muerte → Quiz + tip de gameplay
            // ═══════════════════════════════════════════════════════════════════
            
            if (deaths == 1)
            {
                // Primera vez - Mostrar información básica
                ShowThreatInfo(_lastDamageSource);
            }
            else if (deaths >= DeathsBeforeQuiz)
            {
                // Múltiples muertes - Es hora de un quiz
                ScheduleContextualQuiz(_lastDamageSource, "death");
                
                // Reset contador para no spamear
                _deathsByEnemyType[_lastDamageSource] = 0;
            }
        }

        private void OnPlayerDamaged(string enemyType, float damage)
        {
            _lastDamageSource = enemyType;
        }

        // ═══════════════════════════════════════════════════════════════════
        // MOMENTO EDUCATIVO #2: Completar oleada
        // 
        // UX: Pausa natural en el juego, jugador aliviado.
        // Buen momento para quiz de repaso (no contextual a enemigo específico)
        // ═══════════════════════════════════════════════════════════════════

        private void OnWaveCompleted(int wave)
        {
            _currentWave = wave;
            
            // Quiz de repaso cada N oleadas
            if (wave - _lastQuizWave >= WavesBeforeReviewQuiz)
            {
                ScheduleReviewQuiz();
                _lastQuizWave = wave;
            }
            
            // Bonus: Si completó oleada sin daño, felicitar
            // (El tracking de "sin daño" está en AdaptiveDifficultySystem)
        }

        private void OnWaveAnnounced(int wave, string title, string desc)
        {
            _currentWave = wave;
        }

        // ═══════════════════════════════════════════════════════════════════
        // MOMENTO EDUCATIVO #3: Descubrir nueva amenaza
        // 
        // UX: Curiosidad natural del jugador al ver algo nuevo.
        // "¿Qué es esto?" - Aprovechamos para enseñar.
        // (Manejado por ThreatEncyclopedia)
        // ═══════════════════════════════════════════════════════════════════

        // ═══════════════════════════════════════════════════════════════════
        // MOSTRAR CONTENIDO EDUCATIVO
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Muestra información sobre una amenaza (sin quiz)
        /// </summary>
        private void ShowThreatInfo(string enemyType)
        {
            var threat = ThreatEncyclopedia.Instance?.GetThreat(enemyType);
            if (threat == null) return;
            
            string title = $"{threat.Icon} {threat.Name}";
            string description = threat.ShortDescription;
            string defense = $"💡 {threat.Weakness}";
            
            // Notificar para que QuizView muestre info
            GameEventBus.Instance.EmitNewEnemyEncountered(title, description, defense);
            
            GD.Print($"[ContextualLearning] Mostrando info de {enemyType}");
        }

        /// <summary>
        /// Programa un quiz contextual relacionado con un enemigo
        /// </summary>
        private void ScheduleContextualQuiz(string enemyType, string trigger)
        {
            _quizPending = true;
            _pendingQuizCategory = enemyType;
            
            // Mostrar quiz después de un pequeño delay (para que el jugador procese la muerte)
            var timer = GetTree().CreateTimer(QuizDelayAfterDeath);
            timer.Timeout += () => ExecuteContextualQuiz(enemyType, trigger);
            
            GD.Print($"[ContextualLearning] Quiz programado para {enemyType} (trigger: {trigger})");
        }

        private void ExecuteContextualQuiz(string enemyType, string trigger)
        {
            if (!_quizPending) return;
            _quizPending = false;
            
            // Obtener quiz de la categoría relacionada
            var threat = ThreatEncyclopedia.Instance?.GetThreat(enemyType);
            if (threat == null)
            {
                // Fallback a quiz random
                ShowRandomQuiz();
                return;
            }
            
            // Buscar pregunta de la categoría del enemigo
            var question = QuizSystem.Instance?.GetRandomQuestionByCategory(threat.QuizCategory);
            if (question == null)
            {
                question = QuizSystem.Instance?.GetNextQuestion();
            }
            
            if (question != null)
            {
                ContextualQuizzesShown++;
                
                // Personalizar el contexto del quiz
                string customContext = GetContextualMessage(enemyType, trigger);
                
                // El QuizView se encarga de mostrarlo
                // Emitimos un evento o lo llamamos directamente
                ShowQuizWithContext(question, customContext, enemyType);
            }
        }

        /// <summary>
        /// Quiz de repaso (no específico a un enemigo)
        /// </summary>
        private void ScheduleReviewQuiz()
        {
            // Solo si hay amenazas descubiertas
            var discovered = ThreatEncyclopedia.Instance?.GetDiscoveredThreats();
            if (discovered == null || discovered.Count == 0) return;
            
            // Elegir una amenaza con nivel de conocimiento bajo
            var lowKnowledgeThreats = discovered.FindAll(t => t.KnowledgeLevel < 3);
            if (lowKnowledgeThreats.Count == 0) return;
            
            var rng = new Random();
            var targetThreat = lowKnowledgeThreats[rng.Next(lowKnowledgeThreats.Count)];
            
            ScheduleContextualQuiz(targetThreat.Id, "review");
        }

        private void ShowRandomQuiz()
        {
            var question = QuizSystem.Instance?.GetNextQuestion();
            if (question != null)
            {
                ContextualQuizzesShown++;
                ShowQuizWithContext(question, "SECURITY_ASSESSMENT", "General");
            }
        }

        private void ShowQuizWithContext(QuizQuestion question, string customContext, string relatedThreat)
        {
            // Buscar QuizView en el árbol y mostrar el quiz
            var quizView = GetTree().Root.GetNodeOrNull<QuizView>("Main/QuizView");
            if (quizView != null)
            {
                // Modificar el contexto de la pregunta temporalmente
                quizView.ShowQuestionWithContext(question, customContext);
            }
        }

        /// <summary>
        /// Genera mensaje contextual basado en por qué se muestra el quiz
        /// </summary>
        private string GetContextualMessage(string enemyType, string trigger)
        {
            var threat = ThreatEncyclopedia.Instance?.GetThreat(enemyType);
            string threatName = threat?.Name ?? enemyType;
            string icon = threat?.Icon ?? "⚠️";
            
            return trigger switch
            {
                "death" => $"💀 HAS SIDO DERROTADO POR {icon} {threatName}\n¡Es hora de aprender a defenderte!",
                "review" => $"📚 REPASO DE SEGURIDAD: {icon} {threatName}\n¿Recuerdas cómo combatir esta amenaza?",
                "discovery" => $"🔍 NUEVA AMENAZA: {icon} {threatName}\nDemuestra tu conocimiento.",
                "wave_complete" => $"✓ OLEADA COMPLETADA\nPon a prueba tus conocimientos.",
                _ => $"⚠️ EVALUACIÓN DE SEGURIDAD: {icon} {threatName}"
            };
        }

        // ═══════════════════════════════════════════════════════════════════
        // FEEDBACK DE RESPUESTAS
        // ═══════════════════════════════════════════════════════════════════

        private void OnQuestionAnswered(bool correct)
        {
            if (correct)
            {
                ContextualQuizzesCorrect++;
                
                // Feedback positivo contextual
                var messages = new[]
                {
                    "✓ ¡Correcto! Tu firewall mental se fortalece.",
                    "✓ ¡Bien hecho! Eres más resistente a esta amenaza.",
                    "✓ ¡Excelente! Conocimiento = Poder.",
                    "✓ ¡Amenaza analizada! Tu defensa mejora."
                };
                var rng = new Random();
                
                // Delay para no superponer con la explicación del quiz
                var timer = GetTree().CreateTimer(2.0f);
                timer.Timeout += () => {
                    GameEventBus.Instance.EmitSecurityTipShown(messages[rng.Next(messages.Length)]);
                };
            }
            else
            {
                // Feedback que incentiva a aprender más
                var messages = new[]
                {
                    "✗ No te preocupes, aprenderás. ¡Sigue intentando!",
                    "✗ Error detectado. Revisa la explicación para mejorar.",
                    "✗ Esta amenaza sigue siendo un misterio. ¡Investiga más!",
                    "✗ Fallo en el firewall mental. Tiempo de actualizarse."
                };
                var rng = new Random();
                
                var timer = GetTree().CreateTimer(2.0f);
                timer.Timeout += () => {
                    GameEventBus.Instance.EmitSecurityTipShown(messages[rng.Next(messages.Length)]);
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Fuerza un quiz sobre un tema específico (para tutoriales)
        /// </summary>
        public void TriggerQuizForThreat(string threatId)
        {
            ScheduleContextualQuiz(threatId, "manual");
        }

        /// <summary>
        /// Obtiene estadísticas de aprendizaje por tipo de enemigo
        /// </summary>
        public Dictionary<string, int> GetDeathStatistics()
        {
            return new Dictionary<string, int>(_deathsByEnemyType);
        }

        /// <summary>
        /// Reset para nueva partida
        /// </summary>
        public void ResetForNewGame()
        {
            _deathsByEnemyType.Clear();
            _lastDamageSource = "";
            _lastQuizWave = 0;
            _currentWave = 0;
            _quizPending = false;
        }

        public override void _ExitTree()
        {
            if (GameEventBus.Instance != null)
            {
                GameEventBus.Instance.OnPlayerDied -= OnPlayerDied;
                GameEventBus.Instance.OnPlayerDamagedByEnemy -= OnPlayerDamaged;
                GameEventBus.Instance.OnWaveCompleted -= OnWaveCompleted;
                GameEventBus.Instance.OnQuestionAnswered -= OnQuestionAnswered;
                GameEventBus.Instance.OnWaveAnnounced -= OnWaveAnnounced;
            }
        }
    }
}
