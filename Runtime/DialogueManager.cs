using System;
using System.Collections.Generic;
using System.Linq;
using Ink.Runtime;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace MelodySuite.Dialogue.Runtime
{
    public class DialogueManager : MonoBehaviour {
        
        private Story _currentStory;
        
        [SerializeField] private UnityEvent<Story> _onDialogueStart = new();
        [SerializeField] private UnityEvent<Story> _onDialogueEnd = new();
        [SerializeField] private UnityEvent<Sentence> _onNextSentence = new();
        [SerializeField] private UnityEvent<Choice[]> _onChoices = new();
        [SerializeField] private UnityEvent<Choice> _onChoiceSelected = new();
        
        public UnityEvent<Story> onDialogueStart => _onDialogueStart;
        public UnityEvent<Story> onDialogueEnd => _onDialogueEnd;
        public UnityEvent<Sentence> onNextSentence => _onNextSentence;
        public UnityEvent<Choice[]> onChoices => _onChoices;
        public UnityEvent<Choice> onChoiceSelected => _onChoiceSelected;
        
        public bool started { get; private set; }
        
        private bool _ended;
        private bool _canContinue = true;

        public bool canContinue
        {
            get => started && !_ended && !choosing && _canContinue;
            set => _canContinue = value;
        }

        public bool choosing { get; private set; }
        
        public void Load(string json)
        {
   
            if (_currentStory == null)
            {
                Debug.LogWarning("DialogueManager: No current story loaded");
                return;
            }
         
            _currentStory.state.LoadJson(json);
            ResetFlags();
            HandleState(_currentStory.currentText);
        }

        public string Save()
        {
            return _currentStory.state.ToJson();
        }
        
        public void StartDialogue(string json) {
            if (string.IsNullOrEmpty(json)) {
                Debug.LogWarning("DialogueManager::StartDialogue: json is null or empty");
                return;
            }
            
            var story = new Story(json);
  
            ResetFlags();
            _currentStory = story;
            _currentStory.onError += (str, type) =>
            {
                Debug.LogError(str);
            };
            onDialogueStart?.Invoke(story);
            Next();
        }

        private void ResetFlags()
        {
            started = true;
            _ended = false;
            choosing = false;
        }

        public void EndDialogue()
        {
            if (_ended)
                return;

            started = false;
            _ended = true;
            var last = _currentStory;
            _currentStory = null;
            onDialogueEnd?.Invoke(last);
        }
        
        public void Next()
        {
            if (_currentStory == null || !canContinue)
                return;
            
            if (_currentStory.canContinue)
            {
                HandleState(_currentStory.Continue().Trim());
            }
            else
            {
                EndDialogue();
            }
        }

        private void HandleState(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = "...";

            onNextSentence?.Invoke(new Sentence(text, _currentStory.currentTags));
            
            if (_currentStory.currentChoices.Count > 0) {
                HandleChoices();
            }
        }

        private void HandleChoices() {
            var choices = _currentStory.currentChoices
                .Select(choice => new Choice(choice.text, choice.index, choice.tags))
                .ToArray();
            onChoices.Invoke(choices);
            choosing = true;
        }

        public void SelectChoice(int i) {
            if (_ended)
                return;
       
            var choice = _currentStory.currentChoices[i];
            onChoiceSelected?.Invoke(new Choice(choice.text, choice.index, choice.tags));
            choosing = false;
            _currentStory.ChooseChoiceIndex(i);
            Next();
        }

        [CanBeNull]
        public object GetVariable(string path)
        {
            if (_currentStory == null)
                return null;
            return _currentStory.variablesState[path];
        }
        
        public void SetVariable(string path, object obj)
        {
            if (_currentStory == null)
                return;
            if (_currentStory.variablesState[path] == null)
            {
                Debug.LogWarning("DialogueManager::SetVariable: cannot find variable " + path);
                return;
            }
            _currentStory.variablesState[path] = obj;
        }
    }
    
    [Serializable]
    public class Choice {
        public string text { get; private set; }
        public int index { get; private set; }
        public List<string> tags { get; private set; }
        public Choice(string text, int index, List<string> tags) {
            this.text = text;
            this.index = index;
            this.tags = tags;
        }
    }
    
    [Serializable]
    public class Sentence {
        public string content { get; private set; }
        public List<string> tags { get; private set; }

        public Sentence(string content, List<string> tags) {
            this.content = content;
            this.tags = tags;
        }
    }
}