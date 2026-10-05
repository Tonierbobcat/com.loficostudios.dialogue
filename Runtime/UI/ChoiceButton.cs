using UnityEngine;
using UnityEngine.Events;

namespace MelodySuite.Dialogue.Runtime {
    
    public class ChoiceButton : MonoBehaviour {
        private Choice _choice;
        
        public UnityEvent onSelect = new();
        
        public Choice choice { get => _choice; set => SetChoice(value); }
        
        public UnityEvent<string> setText = new();
        
        private void SetChoice(Choice c) {
            _choice = c;
            if (c == null)
                return;
            setText.Invoke(c.text);
            // if (text.text == null)
            //     return;
            // text.text = c.text;
        }

        public void Select()
        {
            onSelect?.Invoke();
        }
    }
}