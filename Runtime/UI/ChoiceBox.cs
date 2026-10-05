using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MelodySuite.Dialogue.Runtime
{
    public class ChoiceBox : MonoBehaviour
    {
        private readonly List<GameObject> _choicesObjs = new();
        [SerializeField]
        private GameObject choicePrefab;

        [SerializeField] private Transform container;

        [SerializeField] private DialogueManager dialogueManager;
    
        public bool displayed { get; private set; }

        public void Show() {
            if (!TryGetComponent<CanvasGroup>(out var canvas)) {
                canvas = gameObject.AddComponent<CanvasGroup>();
            }
            canvas.alpha = 1f;
            canvas.blocksRaycasts = true;
        }

        public void Hide() {
            if (!TryGetComponent<CanvasGroup>(out var canvas)) {
                canvas = gameObject.AddComponent<CanvasGroup>();
            }
            canvas.alpha = 0f;
            canvas.blocksRaycasts = false;
        }
    
        private void Awake()
        {
            dialogueManager.onNextSentence.AddListener(s =>
            {
                RemoveActive();
            });
        
            dialogueManager.onChoices.AddListener(choices =>
            {
                CreateChoices(choices.ToList(), i =>
                {
                    dialogueManager.SelectChoice(i);
                });
            });
        
            dialogueManager.onChoiceSelected.AddListener(c =>
            {
                RemoveActive();
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

        public void CreateChoices(List<Choice> choices, Action<int> onChoiceSelected) {
            RemoveActive();
            displayed = true;
            foreach (var choice in choices) {
                var choiceObj = Instantiate(choicePrefab, container);
        
                var comp = choiceObj.GetComponent<ChoiceButton>();
                comp.onSelect.AddListener(() => onChoiceSelected?.Invoke(choice.index));
                comp.choice = choice;
                choiceObj.SetActive(true);
                _choicesObjs.Add(choiceObj);
            }
        }

        public void RemoveActive() {
            displayed = false;
            // Debug.Log("Removing active: " + _choicesObjs.Count);
            foreach (var obj in _choicesObjs) {
                DestroyImmediate(obj);
            }
            _choicesObjs.Clear();
        }
    }
}
