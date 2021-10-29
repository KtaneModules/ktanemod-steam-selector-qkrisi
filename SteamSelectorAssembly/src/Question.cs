using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SteamSelector
{
    public abstract class Question
    {
        public bool ButtonsEnabled { get; protected set; }
        protected string[] ExtrasScreen = new string[0];
        
        private int ScreenIndex;
        private string question_cache;
        private const float ExtrasScreenWait = 0.02f;

        public bool ExtrasEnabled
        {
            get
            {
                return ExtrasScreen.Length > 0;
            }
        }
        
        public abstract bool CorrectAnswer {get;}

        public abstract int Weight {get;}

        public virtual void Generate()
        {
            ButtonsEnabled = false;
            Module.flashing = false;
            ExtrasScreen = new string[0];
            ScreenIndex = 0;
        }

        public abstract void Cycle(int increment);
        protected abstract void WriteAnswer();

        protected string CurrentQuestion;

        private void WriteExtras()
        {
            Module.StartCoroutine(TextMeshUtils.WriteText(Display, ExtrasScreen[ScreenIndex], EnableButtons, false,
                ExtrasScreenWait));
        }
        
        public void ToggleScreen(bool enable)
        {
            ButtonsEnabled = false;
            ScreenIndex = 0;
            if (enable)
                WriteExtras();
            else Module.StartCoroutine(TextMeshUtils.WriteText(Display, question_cache, EnableButtons, false, ExtrasScreenWait));
        }
        
        public void CycleScreen(int increment)
        {
            ButtonsEnabled = false;
            bool write = true;
            ScreenIndex += increment;
            if (ScreenIndex >= ExtrasScreen.Length)
            {
                ScreenIndex = ExtrasScreen.Length - 1;
                write = false;
            }
            else if (ScreenIndex < 0)
            {
                ScreenIndex = 0;
                write = false;
            }
            if (write)
                WriteExtras();
            else EnableButtons();
        }

        protected void EnableButtons()
        {
            ButtonsEnabled = true;
        }

        protected void WriteQuestion(bool format)
        {
            Module.Log("Question: {0}", CurrentQuestion.Replace("\n", " "));
            string modified = CurrentQuestion;
            if (format)
            {
                List<string> question = CurrentQuestion.Split(new char[] { ' ' }).ToList();
                modified = "";
                int Counter = 0;
                while (question.Count > 0)
                {
                    bool _break = false;
                    modified = Counter == 0 ? modified + question[0] : modified + " " + question[0];
                    if (question[0].Length > 12) _break = true;
                    question.RemoveAt(0);
                    Counter++;
                    if (_break || Counter == 4)
                    {
                        modified = modified + "\n";
                        Counter = 0;
                    }
                }
            }
            question_cache = modified;
            Module.StartCoroutine(TextMeshUtils.WriteText(Display, modified, WriteAnswer, false));
        }

        protected readonly TextMesh Display;
        protected readonly TextMesh Input;
        protected readonly qkSteamSelector Module;
        protected readonly qkSteamSelectorService Service;

        protected Question(TextMesh display_text, TextMesh input_text, qkSteamSelector module)
        {
            Display = display_text;
            Input = input_text;
            Module = module;
            Service = qkSteamSelectorService.Instance;
        }
    }
}