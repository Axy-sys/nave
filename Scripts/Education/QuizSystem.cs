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
					"SITUACIÓN: El sistema ha detectado un archivo ejecutable desconocido intentando replicarse.",
					QuizCategory.Malware
				),
				
				// Preguntas sobre Phishing
				new QuizQuestion(
					"¿Cuál es la mejor manera de identificar un correo de phishing?",
					new[] { "Verificar el remitente", "Hacer clic en todos los enlaces", "Responder inmediatamente", "Compartir información personal" },
					0,
					"Siempre verifica el remitente, busca errores ortográficos y no hagas clic en enlaces sospechosos.",
					"SITUACIÓN: Has recibido un correo urgente de tu banco pidiendo confirmar tu contraseña.",
					QuizCategory.Phishing
				),
				
				new QuizQuestion(
					"¿Qué debes hacer si recibes un email sospechoso?",
					new[] { "Eliminarlo sin abrir", "Hacer clic en los enlaces", "Descargar los adjuntos", "Responder al remitente" },
					0,
					"Nunca abras adjuntos ni hagas clic en enlaces de emails sospechosos. Elimínalos directamente.",
					"SITUACIÓN: Un remitente desconocido te envía una factura adjunta que no esperabas.",
					QuizCategory.Phishing
				),

				// Preguntas sobre Contraseñas
				new QuizQuestion(
					"¿Cuál es una contraseña segura?",
					new[] { "P@ssw0rd!2024", "123456", "password", "miNombre" },
					0,
					"Una contraseña segura debe tener mayúsculas, minúsculas, números y símbolos, con al menos 12 caracteres.",
					"SITUACIÓN: Estás configurando una nueva cuenta de administrador y necesitas definir el acceso.",
					QuizCategory.Authentication
				),

				new QuizQuestion(
					"¿Qué es la autenticación de dos factores (2FA)?",
					new[] { "Una capa extra de seguridad", "Un tipo de virus", "Una contraseña débil", "Un ataque hacker" },
					0,
					"2FA añade una capa extra de protección requiriendo dos formas de verificación de identidad.",
					"SITUACIÓN: Alguien intenta acceder a tu cuenta desde un dispositivo nuevo.",
					QuizCategory.Authentication
				),

				// Preguntas sobre Encriptación
				new QuizQuestion(
					"¿Para qué sirve la encriptación?",
					new[] { "Proteger datos confidenciales", "Hacer copias de seguridad", "Acelerar internet", "Crear virus" },
					0,
					"La encriptación convierte datos legibles en código para protegerlos de accesos no autorizados.",
					"SITUACIÓN: Necesitas enviar un archivo con datos de clientes a través de internet.",
					QuizCategory.Encryption
				),

				// Preguntas sobre Firewall
				new QuizQuestion(
					"¿Qué hace un firewall?",
					new[] { "Bloquea tráfico no autorizado", "Acelera la conexión", "Crea virus", "Borra archivos" },
					0,
					"Un firewall monitorea y controla el tráfico de red basándose en reglas de seguridad.",
					"SITUACIÓN: Se detectan múltiples intentos de conexión desde una IP externa desconocida.",
					QuizCategory.NetworkSecurity
				),

				// Preguntas sobre SQL Injection
				new QuizQuestion(
					"¿Qué es una inyección SQL?",
					new[] { "Un ataque a bases de datos", "Un medicamento", "Un tipo de firewall", "Un antivirus" },
					0,
					"SQL Injection inserta código malicioso en consultas SQL para manipular bases de datos.",
					"SITUACIÓN: Un formulario de login está recibiendo caracteres extraños como ' OR '1'='1.",
					QuizCategory.WebSecurity
				),

				// Preguntas sobre DDoS
				new QuizQuestion(
					"¿Qué significa DDoS?",
					new[] { "Distributed Denial of Service", "Direct Data of Server", "Delete Data on System", "Digital Defense of Security" },
					0,
					"DDoS satura servidores con tráfico falso para hacerlos inaccesibles a usuarios legítimos.",
					"SITUACIÓN: El servidor web está recibiendo millones de peticiones por segundo y no responde.",
					QuizCategory.NetworkSecurity
				),

				// Preguntas sobre Ransomware
				new QuizQuestion(
					"¿Qué hace el ransomware?",
					new[] { "Cifra archivos y pide rescate", "Protege archivos", "Acelera el PC", "Limpia virus" },
					0,
					"El ransomware cifra tus archivos y exige pago para devolverte el acceso.",
					"SITUACIÓN: Todos tus archivos han cambiado de extensión y aparece una nota de rescate en pantalla.",
					QuizCategory.Malware
				),

				// Preguntas sobre Seguridad General
				new QuizQuestion(
					"¿Qué es un certificado SSL/TLS?",
					new[] { "Verifica identidad de sitios web", "Un tipo de virus", "Una contraseña", "Un firewall" },
					0,
					"SSL/TLS cifra la comunicación entre tu navegador y el servidor web para proteger datos.",
					"SITUACIÓN: Los usuarios reportan que el navegador marca tu sitio web como 'No Seguro'.",
					QuizCategory.WebSecurity
				),

				new QuizQuestion(
					"¿Qué es un honeypot en ciberseguridad?",
					new[] { "Trampa para atraer atacantes", "Un tipo de dulce", "Un antivirus", "Una contraseña" },
					0,
					"Un honeypot es un sistema señuelo para detectar y estudiar ataques cibernéticos.",
					"SITUACIÓN: Quieres analizar las técnicas de los atacantes sin arriesgar tu red principal.",
					QuizCategory.NetworkSecurity
				),

				new QuizQuestion(
					"¿Con qué frecuencia debes actualizar tu software?",
					new[] { "Tan pronto como estén disponibles", "Una vez al año", "Nunca", "Solo cuando falla" },
					0,
					"Las actualizaciones incluyen parches de seguridad críticos. Instálalas inmediatamente.",
					"SITUACIÓN: El sistema operativo notifica que hay una actualización de seguridad crítica pendiente.",
					QuizCategory.BestPractices
				),

				// Preguntas sobre Ingeniería Social
				new QuizQuestion(
					"¿Qué es la Ingeniería Social?",
					new[] { "Manipular personas para obtener información", "Programar redes sociales", "Diseñar edificios", "Un curso de ingeniería" },
					0,
					"La ingeniería social explota la psicología humana para engañar a usuarios y obtener datos confidenciales.",
					"SITUACIÓN: Un empleado recibe una llamada de alguien que dice ser el CEO pidiendo una transferencia urgente.",
					QuizCategory.Phishing
				),

				new QuizQuestion(
					"Si un 'técnico' te llama pidiendo tu contraseña, ¿qué haces?",
					new[] { "Colgar y reportarlo", "Dársela para arreglar el problema", "Preguntar su nombre", "Esperar en línea" },
					0,
					"Ningún soporte técnico legítimo te pedirá tu contraseña por teléfono. Es un intento de vishing.",
					"SITUACIÓN: Recibes una llamada de soporte técnico alegando que tu cuenta tiene un virus.",
					QuizCategory.Phishing
				),

				// Preguntas sobre IoT (Internet of Things)
				new QuizQuestion(
					"¿Por qué son vulnerables los dispositivos inteligentes (IoT)?",
					new[] { "Suelen tener contraseñas por defecto", "Son muy caros", "Usan mucha electricidad", "Son difíciles de usar" },
					0,
					"Muchos dispositivos IoT vienen con contraseñas predeterminadas débiles y reciben pocas actualizaciones de seguridad.",
					"SITUACIÓN: Has instalado cámaras de seguridad conectadas a internet en la oficina.",
					QuizCategory.NetworkSecurity
				),

				// Preguntas sobre Privacidad de Datos
				new QuizQuestion(
					"¿Qué es una VPN?",
					new[] { "Red Privada Virtual", "Video Personal Network", "Virus Protection Node", "Very Private Number" },
					0,
					"Una VPN (Virtual Private Network) cifra tu conexión a internet, ocultando tu actividad y ubicación.",
					"SITUACIÓN: Necesitas conectarte al WiFi de una cafetería para trabajar.",
					QuizCategory.Encryption
				),

				new QuizQuestion(
					"¿Qué información es seguro compartir públicamente en redes sociales?",
					new[] { "Tus gustos musicales", "Tu dirección de casa", "Tu fecha de nacimiento completa", "Tus planes de vacaciones futuros" },
					0,
					"Compartir datos personales como dirección o fechas exactas puede ser usado para robo de identidad o robos físicos.",
					"SITUACIÓN: Estás actualizando tu perfil público en una red social.",
					QuizCategory.BestPractices
				),

				// Preguntas sobre Malware Avanzado
				new QuizQuestion(
					"¿Qué es un Keylogger?",
					new[] { "Software que registra tus pulsaciones", "Un gestor de contraseñas", "Una llave digital", "Un tipo de teclado" },
					0,
					"Un Keylogger es un malware que graba todo lo que escribes para robar contraseñas y mensajes.",
					"SITUACIÓN: Notas que el teclado responde lento y aparecen procesos extraños en el administrador de tareas.",
					QuizCategory.Malware
				),

				// --- NUEVAS PREGUNTAS NIVEL 3 (CLOUD / APT / RANSOMWARE) ---
				new QuizQuestion(
					"¿Qué es un ataque de 'Día Cero' (Zero-Day)?",
					new[] { "Ataque a una vulnerabilidad no conocida", "Un ataque que dura 0 días", "Un virus que borra todo", "Un error de fecha" },
					0,
					"Un Zero-Day explota una vulnerabilidad que el fabricante del software aún no conoce o no ha parcheado.",
					"SITUACIÓN: Un hacker entra al sistema usando un fallo en el software que nadie sabía que existía.",
					QuizCategory.BestPractices
				),
				new QuizQuestion(
					"¿Qué es una APT (Amenaza Persistente Avanzada)?",
					new[] { "Un ataque prolongado y sigiloso", "Un virus rápido", "Un antivirus potente", "Una contraseña segura" },
					0,
					"Una APT es un ataque donde un intruso se infiltra en una red y permanece oculto por largo tiempo para robar datos.",
					"SITUACIÓN: Se descubren logs de acceso no autorizado que datan de hace 6 meses.",
					QuizCategory.NetworkSecurity
				),
				new QuizQuestion(
					"¿Cómo te proteges del Ransomware?",
					new[] { "Backups regulares y no abrir adjuntos", "Pagar el rescate siempre", "Reiniciar el PC", "Usar modo incógnito" },
					0,
					"La mejor defensa es tener copias de seguridad (backups) actualizadas y desconectadas de la red.",
					"SITUACIÓN: Archivos críticos han sido cifrados. ¿Tienes una copia de seguridad reciente?",
					QuizCategory.Malware
				),
				new QuizQuestion(
					"¿Qué es la 'Nube' (Cloud Computing)?",
					new[] { "Servidores accesibles por internet", "Vapor de agua", "Un disco duro externo", "Una red social" },
					0,
					"La nube son servidores remotos que almacenan datos y ejecutan aplicaciones a través de internet.",
					"SITUACIÓN: La empresa migra sus bases de datos a un servicio de almacenamiento remoto.",
					QuizCategory.BestPractices
				),
				new QuizQuestion(
					"¿Qué es la responsabilidad compartida en la nube?",
					new[] { "Proveedor protege la nube, tú tus datos", "El proveedor hace todo", "Tú haces todo", "Nadie es responsable" },
					0,
					"El proveedor asegura la infraestructura, pero tú eres responsable de configurar la seguridad de tus datos y accesos.",
					"SITUACIÓN: Has subido archivos confidenciales a la nube pero olvidaste ponerles contraseña.",
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

		public QuizQuestion GetQuestionForLevel(int level)
		{
			// Filtrar preguntas según el nivel
			// Nivel 1: Malware, Phishing
			// Nivel 2: DDoS, NetworkSecurity, WebSecurity (SQLi), Authentication
			// Nivel 3: Encryption, Ransomware, APT, Cloud
			
			var allowedCategories = new List<QuizCategory>();
			
			switch (level)
			{
				case 1:
					allowedCategories.Add(QuizCategory.Malware);
					allowedCategories.Add(QuizCategory.Phishing);
					break;
				case 2:
					allowedCategories.Add(QuizCategory.NetworkSecurity);
					allowedCategories.Add(QuizCategory.DDoS);
					allowedCategories.Add(QuizCategory.WebSecurity); // SQL Injection
					allowedCategories.Add(QuizCategory.Authentication); // Brute Force
					break;
				case 3:
					allowedCategories.Add(QuizCategory.Encryption);
					allowedCategories.Add(QuizCategory.Malware); // Ransomware es tipo Malware
					allowedCategories.Add(QuizCategory.BestPractices); // Cloud, Zero-Day
					break;
				default:
					// Todos
					return GetNextQuestion();
			}

			var levelQuestions = _questions.FindAll(q => allowedCategories.Contains(q.Category));
			if (levelQuestions.Count == 0) return GetNextQuestion();

			var random = new System.Random();
			_currentQuestion = levelQuestions[random.Next(levelQuestions.Count)];
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
		public string Context { get; }
		public QuizCategory Category { get; }

		public QuizQuestion(string question, string[] answers, int correctIndex, string explanation, string context, QuizCategory category)
		{
			Question = question;
			Answers = answers;
			CorrectAnswerIndex = correctIndex;
			Explanation = explanation;
			Context = context;
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
		BestPractices,
		DDoS
	}
}
