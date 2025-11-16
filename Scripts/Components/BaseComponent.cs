using Godot;
using CyberSecurityGame.Core.Interfaces;

namespace CyberSecurityGame.Components
{
    /// <summary>
    /// Clase base abstracta para componentes (Component Pattern)
    /// Implementa Template Method Pattern
    /// Principio de Open/Closed: abierto a extensión, cerrado a modificación
    /// </summary>
    public abstract partial class BaseComponent : Node, IComponent
    {
        protected Node _owner;
        public bool IsActive { get; set; } = true;

        public virtual void Initialize(Node owner)
        {
            _owner = owner;
            OnInitialize();
        }

        public virtual void UpdateComponent(double delta)
        {
            if (!IsActive) return;
            OnUpdate(delta);
        }

        public virtual void Cleanup()
        {
            OnCleanup();
        }

        // Template methods para que las subclases implementen
        protected abstract void OnInitialize();
        protected abstract void OnUpdate(double delta);
        protected abstract void OnCleanup();
    }
}
