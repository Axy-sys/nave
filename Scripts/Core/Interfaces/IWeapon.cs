using Godot;

namespace CyberSecurityGame.Core.Interfaces
{
	/// <summary>
	/// Interface para sistema de armas (Strategy Pattern)
	/// </summary>
	public interface IWeapon
	{
		void Fire(Vector2 position, Vector2 direction);
		bool CanFire();
		void Reload();
		string GetWeaponName();
		WeaponType GetWeaponType();
		void SetOwner(Node2D owner); // Added for context access
	}
	
	/// <summary>
	/// Tipos de armas temáticas de ciberseguridad
	/// </summary>
	public enum WeaponType
	{
		Firewall,      // Arma básica tipo firewall
		Antivirus,     // Proyectiles antivirus
		Encryption,    // Escudo de encriptación
		Honeypot,      // Trampas honeypot
		IDS,           // Sistema de detección de intrusos
		Patch          // Parches de seguridad
	}
}
