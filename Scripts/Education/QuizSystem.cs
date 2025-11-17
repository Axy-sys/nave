using Godot;
using System.Collections.Generic;

namespace CyberSecurityGame.Education
{
	/// <summary>
	/// Sistema de preguntas educativas sobre ciberseguridad
	/// Implementa el aspecto educativo del juego
	/// </summary>
	public partial class QuizSystem : Node
	{
		private static QuizSystem _instance;
		public static QuizSystem Instance => _instance;

		private List<QuizQuestion> _questions;
		private Queue<QuizQuestion> _questionQueue;
		private QuizQuestion _currentQuestion;

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}
			_instance = this;
			
			InitializeQuestions();
		}

		private void InitializeQuestions()
		{
			_questions = new List<QuizQuestion>
			{
				// Preguntas sobre Malware
				new QuizQuestion(
					"¿Qué es un malware?",
					new[] { "Software malicioso", "Un virus de computadora", "Un programa antivirus", "Un tipo de firewall" },
					0,
					"El malware es cualquier software diseñado para dañar o explotar dispositivos, servicios o redes.",
					QuizCategory.Malware
				),
				
				// Preguntas sobre Phishing
				new QuizQuestion(
					"¿Cuál es la mejor manera de identificar un correo de phishing?",
					new[] { "Verificar el remitente", "Hacer clic en todos los enlaces", "Responder inmediatamente", "Compartir información personal" },
					0,
					"Siempre verifica el remitente, busca errores ortográficos y no hagas clic en enlaces sospechosos.",
					QuizCategory.Phishing
				),
				
				new QuizQuestion(
					"¿Qué debes hacer si recibes un email sospechoso?",
					new[] { "Eliminarlo sin abrir", "Hacer clic en los enlaces", "Descargar los adjuntos", "Responder al remitente" },
					0,
					"Nunca abras adjuntos ni hagas clic en enlaces de emails sospechosos. Elimínalos directamente.",
					QuizCategory.Phishing
				),

				// Preguntas sobre Contraseñas
				new QuizQuestion(
					"¿Cuál es una contraseña segura?",
					new[] { "P@ssw0rd!2024", "123456", "password", "miNombre" },
					0,
					"Una contraseña segura debe tener mayúsculas, minúsculas, números y símbolos, con al menos 12 caracteres.",
					QuizCategory.Authentication
				),

				new QuizQuestion(
					"¿Qué es la autenticación de dos factores (2FA)?",
					new[] { "Una capa extra de seguridad", "Un tipo de virus", "Una contraseña débil", "Un ataque hacker" },
					0,
					"2FA añade una capa extra de protección requiriendo dos formas de verificación de identidad.",
					QuizCategory.Authentication
				),

				// Preguntas sobre Encriptación
				new QuizQuestion(
					"¿Para qué sirve la encriptación?",
					new[] { "Proteger datos confidenciales", "Hacer copias de seguridad", "Acelerar internet", "Crear virus" },
					0,
					"La encriptación convierte datos legibles en código para protegerlos de accesos no autorizados.",
					QuizCategory.Encryption
				),

				// Preguntas sobre Firewall
				new QuizQuestion(
					"¿Qué hace un firewall?",
					new[] { "Bloquea tráfico no autorizado", "Acelera la conexión", "Crea virus", "Borra archivos" },
					0,
					"Un firewall monitorea y controla el tráfico de red basándose en reglas de seguridad.",
					QuizCategory.NetworkSecurity
				),

				// Preguntas sobre SQL Injection
				new QuizQuestion(
					"¿Qué es una inyección SQL?",
					new[] { "Un ataque a bases de datos", "Un medicamento", "Un tipo de firewall", "Un antivirus" },
					0,
					"SQL Injection inserta código malicioso en consultas SQL para manipular bases de datos.",
					QuizCategory.WebSecurity
				),

				// Preguntas sobre DDoS
				new QuizQuestion(
					"¿Qué significa DDoS?",
					new[] { "Distributed Denial of Service", "Direct Data of Server", "Delete Data on System", "Digital Defense of Security" },
					0,
					"DDoS satura servidores con tráfico falso para hacerlos inaccesibles a usuarios legítimos.",
					QuizCategory.NetworkSecurity
				),

				// Preguntas sobre Ransomware
				new QuizQuestion(
					"¿Qué hace el ransomware?",
					new[] { "Cifra archivos y pide rescate", "Protege archivos", "Acelera el PC", "Limpia virus" },
					0,
					"El ransomware cifra tus archivos y exige pago para devolverte el acceso.",
					QuizCategory.Malware
				),

				// Preguntas sobre Seguridad General
				new QuizQuestion(
					"¿Qué es un certificado SSL/TLS?",
					new[] { "Verifica identidad de sitios web", "Un tipo de virus", "Una contraseña", "Un firewall" },
					0,
					"SSL/TLS cifra la comunicación entre tu navegador y el servidor web para proteger datos.",
					QuizCategory.WebSecurity
				),

				new QuizQuestion(
					"¿Qué es un honeypot en ciberseguridad?",
					new[] { "Trampa para atraer atacantes", "Un tipo de dulce", "Un antivirus", "Una contraseña" },
					0,
					"Un honeypot es un sistema señuelo para detectar y estudiar ataques cibernéticos.",
					QuizCategory.NetworkSecurity
				),

				new QuizQuestion(
					"¿Con qué frecuencia debes actualizar tu software?",
					new[] { "Tan pronto como estén disponibles", "Una vez al año", "Nunca", "Solo cuando falla" },
					0,
					"Las actualizaciones incluyen parches de seguridad críticos. Instálalas inmediatamente.",
					QuizCategory.BestPractices
				)
			};

			ShuffleQuestions();
		}

		private void ShuffleQuestions()
		{
			var random = new System.Random();
			for (int i = _questions.Count - 1; i > 0; i--)
			{
				int j = random.Next(i + 1);
				var temp = _questions[i];
				_questions[i] = _questions[j];
				_questions[j] = temp;
			}

			_questionQueue = new Queue<QuizQuestion>(_questions);
		}

		public QuizQuestion GetNextQuestion()
		{
			if (_questionQueue.Count == 0)
			{
				ShuffleQuestions(); // Reiniciar preguntas si se acaban
			}

			_currentQuestion = _questionQueue.Dequeue();
			return _currentQuestion;
		}

		public QuizQuestion GetRandomQuestionByCategory(QuizCategory category)
		{
			var categoryQuestions = _questions.FindAll(q => q.Category == category);
			if (categoryQuestions.Count == 0) return GetNextQuestion();

			var random = new System.Random();
			return categoryQuestions[random.Next(categoryQuestions.Count)];
		}

		public bool CheckAnswer(int answerIndex)
		{
			if (_currentQuestion == null) return false;
			return answerIndex == _currentQuestion.CorrectAnswerIndex;
		}

		public string GetExplanation()
		{
			return _currentQuestion?.Explanation ?? "";
		}
	}

	/// <summary>
	/// Clase que representa una pregunta del quiz
	/// </summary>
	public class QuizQuestion
	{
		public string Question { get; }
		public string[] Answers { get; }
		public int CorrectAnswerIndex { get; }
		public string Explanation { get; }
		public QuizCategory Category { get; }

		public QuizQuestion(string question, string[] answers, int correctIndex, string explanation, QuizCategory category)
		{
			Question = question;
			Answers = answers;
			CorrectAnswerIndex = correctIndex;
			Explanation = explanation;
			Category = category;
		}
	}

	public enum QuizCategory
	{
		Malware,
		Phishing,
		Authentication,
		Encryption,
		NetworkSecurity,
		WebSecurity,
		BestPractices
	}
}
