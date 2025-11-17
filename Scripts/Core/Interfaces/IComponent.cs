using Godot;

namespace CyberSecurityGame.Core.Interfaces
{
	/// <summary>
	/// Interface base para sistema de componentes (Component Pattern)
	/// Implementa el principio de Interface Segregation (SOLID)
	/// </summary>
	public interface IComponent
	{
		/// <summary>
		/// Inicializa el componente con su owner
		/// </summary>
		void Initialize(Node owner);
		
		/// <summary>
		/// Actualiza el componente cada frame
		/// </summary>
		void UpdateComponent(double delta);
		
		/// <summary>
		/// Limpia recursos del componente
		/// </summary>
		void Cleanup();
		
		/// <summary>
		/// Indica si el componente está activo
		/// </summary>
		bool IsActive { get; set; }
	}
}
