using System;
using System.Linq;
using UnityEngine;
using RND = UnityEngine.Random;

namespace SteamSelector
{
    public class IntAnswer : Question
    {
        private int CurrentAnswer;
        private int Answer;

        private int? Min;
        private int? Max;
        
        private readonly SteamAchievement[] UnlockedAchievements;

        public override bool CorrectAnswer
        {
            get
            {
                return CurrentAnswer == Answer;
            }
        }

        public override int Weight
        {
            get
            {
                return UnlockedAchievements.Length > 0 ? 2 : 1;
            }
        }

        protected override void WriteAnswer(bool override_wait = true)
        {
            Module.StartCoroutine(TextMeshUtils.WriteText(Input, CurrentAnswer.ToString(), EnableButtons, true,
                override_wait && Module.settings.FastInputAnimation ? 0f : TextMeshUtils.WaitTime));
        }

        public override void Generate()
        {
            base.Generate();
            CurrentQuestion = "How many friends do you have on Steam?";
            Answer = Service.Friends.Count;
            Min = 0;
            bool format = true;
            if (UnlockedAchievements.Length > 0 && RND.Range(1, 3) == 1)
            {
                format = false;
                var SelectedAchievement = UnlockedAchievements[RND.Range(0, UnlockedAchievements.Length)];
                string AchievementName = String.Format("the \"{0}\"\nachievement?", SelectedAchievement.Name);
                var UnlockDate = SelectedAchievement.UnlockDate;
                switch (RND.Range(1, 6))
                {
                    case 1:     //Year
                        CurrentQuestion = "In which year have you unlocked\n" + AchievementName;
                        Answer = UnlockDate.Year;
                        Min = 2015;
                        break;
                    case 2:     //Month
                        CurrentQuestion = "In which month have you unlocked\n" + AchievementName;
                        Answer = UnlockDate.Month;
                        Min = 1;
                        Max = 12;
                        break;
                    case 3:     //Day
                        CurrentQuestion = "On which day have you unlocked\n" + AchievementName;
                        Answer = UnlockDate.Day;
                        Min = 1;
                        Max = 31;
                        break;
                    case 4:     //Hour
                        CurrentQuestion = "When have you unlocked\n" + AchievementName + " (hour)";
                        Answer = UnlockDate.Hour;
                        Max = 23;
                        break;
                    case 5:     //Minute
                        CurrentQuestion = "When have you unlocked\n" + AchievementName + " (minute)";
                        Answer = UnlockDate.Minute;
                        Max = 59;
                        break;
                }
            }
            CurrentAnswer = Min ?? 0;
            WriteQuestion(format);
            Module.Log("Answer: {0}", Answer);
        }

        public override void Cycle(int increment)
        {
            ButtonsEnabled = false;
            unchecked
            {
                CurrentAnswer += increment;
            }
            if(CurrentAnswer < Min)
                CurrentAnswer = Max ?? (int)Min;
            if (CurrentAnswer > Max)
                CurrentAnswer = Min ?? (int)Max;
            WriteAnswer();
        }
        
        public IntAnswer(TextMesh display_text, TextMesh input_text, qkSteamSelector module) :
            base(display_text, input_text, module)
        {
            UnlockedAchievements = Service.Achievements.Where(ach => ach.Unlocked).ToArray();
        }
    }
}