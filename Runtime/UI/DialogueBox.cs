using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// using UnityEngine.InputSystem;

namespace MelodySuite.Dialogue.Runtime {
    
    public class DialogueBox : MonoBehaviour {

        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private float textSpeed = 20f;
        [SerializeField] private UnityEvent<string> setSentenceText = new(); 
        [SerializeField] private UnityEvent<int> setMaxVisibleCharacters = new(); 
        [SerializeField] private UnityEvent<string> setNameText = new(); 
        
        public bool writing { get; private set; }
        
        // private string _text;
        private bool _skipped;
        private Coroutine _textCoroutine;
        public bool interactable = true;

        private void Awake()
        {
            dialogueManager.onNextSentence.AddListener(s =>
            {
                var content = s.content;
                
                // _text = "";
                if (_textCoroutine != null)
                    StopCoroutine(_textCoroutine);

                var arg = s.tags.Find(a => a.StartsWith("name:"))?.Replace("name:", "") ?? "";
                _textCoroutine = StartCoroutine(WriteText(arg, content));
            });
            
            dialogueManager.onDialogueStart.AddListener(c =>
            {
                Show();
            });
            dialogueManager.onDialogueEnd.AddListener(c =>
            {
                Hide();
            });
            
            Hide();
        }
        
        private IEnumerator WriteText(string name, string sentence)
        {
            _skipped = false;
            
            setNameText.Invoke(name);
            setSentenceText.Invoke(sentence);
            
            writing = true;
            
            var chars = sentence.ToCharArray();
            if (!TryGetComponent<CanvasGroup>(out var canvas)) {
                canvas = gameObject.AddComponent<CanvasGroup>();
            }

            setMaxVisibleCharacters.Invoke(0);
            
            for (var i = 0; i < chars.Length; i++) {
                while (canvas.alpha < 1)
                    yield return null;
                
                if (_skipped)
                    break;

                setMaxVisibleCharacters.Invoke(i + 1);
                
                var total = 1f / textSpeed;
                yield return new WaitForSeconds(total);
            }
            
            setMaxVisibleCharacters.Invoke(chars.Length);
            
            writing = false;
        }
        
        public void Show() {
            if (!TryGetComponent<CanvasGroup>(out var canvas)) {
                canvas = gameObject.AddComponent<CanvasGroup>();
            }
            canvas.alpha = 1f;
        }

        public void Hide() {
            if (!TryGetComponent<CanvasGroup>(out var canvas)) {
                canvas = gameObject.AddComponent<CanvasGroup>();
            }
            canvas.alpha = 0f;
        }
        
        public void Skip()
        {
            _skipped = true;
        }

        public void Next()
        {
            if (!interactable)
                return;
            // if (!context.performed)
            //     return;

            if (writing)
                Skip();
            else
                dialogueManager.Next();  
        }
        
        // todo remove these
        

        
        public void WriteAll() {
        }
    }
}