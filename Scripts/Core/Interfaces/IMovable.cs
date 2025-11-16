using Godot;

namespace CyberSecurityGame.Core.Interfaces
{
    /// <summary>
    /// Interface para entidades con movimiento
    /// Principio de Interface Segregation
    /// </summary>
    public interface IMovable
    {
        void Move(Vector2 direction, double delta);
        void SetSpeed(float speed);
        float GetSpeed();
        Vector2 GetVelocity();
    }
}
