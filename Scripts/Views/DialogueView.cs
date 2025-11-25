using Godot;
using CyberSecurityGame.Systems;

namespace CyberSecurityGame.Views
{
    /// <summary>
    /// DEPRECADO: Este view ha sido reemplazado por MissionIntroSystem
    /// que maneja los diálogos de forma integrada con el briefing de misión.
    /// 
    /// Se mantiene por compatibilidad pero está desactivado por defecto.
    /// El nuevo sistema evita:
    /// - Paneles superpuestos
    /// - Slow-motion durante gameplay 
    /// - Múltiples UI compitiendo por atención
    /// </summary>
    public partial class DialogueView : CanvasLayer
    {
        private Control _container;
        private Label _speakerLabel;
        private Label _textLabel;
        private bool _isEnabled = false; // DESACTIVADO por defecto

        public override void _Ready()
        {
            // Solo inicializar si está explícitamente habilitado
            if (!_isEnabled)
            {
                GD.Print("[DialogueView] Desactivado - Usando MissionIntroSystem");
                return;
            }
            
            SetupUI();

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.DialogueStarted += OnDialogueStarted;
                DialogueSystem.Instance.DialogueEnded += OnDialogueEnded;
            }
            
            _container.Visible = false;
        }

        private void SetupUI()
        {
            // Crear UI mínima solo si está habilitado
            var panel = new Panel();
            _container = panel;
            panel.Visible = false;
            AddChild(panel);
        }

        private void OnDialogueStarted(string speaker, string text)
        {
            if (!_isEnabled || _container == null) return;
            // No hacer nada - MissionIntroSystem maneja los diálogos
        }

        private void OnDialogueEnded()
        {
            if (!_isEnabled || _container == null) return;
            _container.Visible = false;
        }

        public override void _ExitTree()
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.DialogueStarted -= OnDialogueStarted;
                DialogueSystem.Instance.DialogueEnded -= OnDialogueEnded;
            }
        }
        
        /// <summary>
        /// Permite habilitar este view si es necesario para casos específicos
        /// </summary>
        public void Enable()
        {
            _isEnabled = true;
            if (_container == null) SetupUI();
        }
    }
}
