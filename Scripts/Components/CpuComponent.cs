using Godot;
using System;
using CyberSecurityGame.Core.Events;

namespace CyberSecurityGame.Components
{
    /// <summary>
    /// Gestiona la carga de CPU (Flux) del sistema.
    /// Mecánica inspirada en Starsector: Disparar y recibir daño en escudo genera carga.
    /// Si llega al máximo, el sistema se sobrecarga (Overload).
    /// </summary>
    public partial class CpuComponent : BaseComponent
    {
        [Export] public float MaxLoad = 100f;
        [Export] public float DissipationRate = 10f; // Cuánto baja por segundo pasivamente
        [Export] public float VentingRate = 50f; // Cuánto baja por segundo al ventilar activamente
        [Export] public float OverloadDuration = 3.0f;

        private float _currentLoad = 0f;
        private bool _isOverloaded = false;
        private bool _isVenting = false;
        private float _overloadTimer = 0f;

        // Eventos para UI y efectos
        public event Action<float, float> OnLoadChanged; // current, max
        public event Action OnOverloadStarted;
        public event Action OnOverloadEnded;

        protected override void OnInitialize()
        {
            _currentLoad = 0f;
            _isOverloaded = false;
            _isVenting = false;
        }

        protected override void OnUpdate(double delta)
        {
            if (_isOverloaded)
            {
                _overloadTimer -= (float)delta;
                // Efecto visual de glitch o chispas aquí
                if (_overloadTimer <= 0)
                {
                    EndOverload();
                }
                return;
            }

            // Disipación pasiva o activa
            if (_currentLoad > 0)
            {
                float rate = _isVenting ? VentingRate : DissipationRate;
                
                // Si estamos disparando o usando escudo, la disipación pasiva podría detenerse
                // Por ahora asumimos que siempre disipa a menos que se añada carga ese frame
                // (Simplificación: disipación constante)
                
                _currentLoad -= rate * (float)delta;
                if (_currentLoad < 0) _currentLoad = 0;
                
                EmitLoadChanged();
            }
        }

        public void AddLoad(float amount)
        {
            if (_isOverloaded) return;

            _currentLoad += amount;
            EmitLoadChanged();

            if (_currentLoad >= MaxLoad)
            {
                TriggerOverload();
            }
        }

        public void StartVenting()
        {
            if (_isOverloaded) return;
            _isVenting = true;
            // Aquí podríamos desactivar armas/escudos desde el Player
        }

        public void StopVenting()
        {
            _isVenting = false;
        }

        private void TriggerOverload()
        {
            _isOverloaded = true;
            _currentLoad = MaxLoad;
            _overloadTimer = OverloadDuration;
            _isVenting = false;
            
            OnOverloadStarted?.Invoke();
            GameEventBus.Instance.EmitCpuOverloadChanged(true);
            GameEventBus.Instance.EmitSecurityTipShown("¡SOBRECARGA DE CPU! SISTEMA REINICIANDO...");
            GD.Print("⚠️ SYSTEM OVERLOAD!");
        }

        private void EndOverload()
        {
            _isOverloaded = false;
            _currentLoad = 0; // Opcional: reiniciar a 0 o a 50%
            OnOverloadEnded?.Invoke();
            GameEventBus.Instance.EmitCpuOverloadChanged(false);
            GD.Print("✅ SYSTEM RESTORED");
        }

        private void EmitLoadChanged()
        {
            OnLoadChanged?.Invoke(_currentLoad, MaxLoad);
            GameEventBus.Instance.EmitCpuLoadChanged(_currentLoad, MaxLoad);
        }

        public bool IsOverloaded() => _isOverloaded;
        public bool IsVenting() => _isVenting;
        public float GetLoadPercentage() => MaxLoad > 0 ? _currentLoad / MaxLoad : 0f;

        protected override void OnCleanup()
        {
            _currentLoad = 0f;
            _isOverloaded = false;
            _isVenting = false;
        }
    }
}
