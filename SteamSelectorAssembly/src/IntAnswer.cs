using System;
using System.Linq;
using Steamworks;
using UnityEngine;
using Random = System.Random;
using RND = UnityEngine.Random;

namespace SteamSelector
{
    public class IntAnswer : Question
    {
        private int CurrentAnswer;
        private int Answer;
        private Func<int> CalculateOnSubmit;

        private int? Min;
        private int? Max;
        
        private readonly SteamAchievement[] UnlockedAchievements;

        public override bool CorrectAnswer
        {
            get
            {
                int ans = Answer;
                if (CalculateOnSubmit != null)
                {
                    ans = CalculateOnSubmit();
                    Module.Log("Correct answer: {0}", ans);
                }
                return CurrentAnswer == ans;
            }
        }

        public override int Weight
        {
            get
            {
                return Convert.ToInt16(UnlockedAchievements.Length > 0) +
                       Convert.ToInt16(Service.Friends.Count > 0) * 2 + 1;
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
            CalculateOnSubmit = () => Service.Friends.Count;
            CurrentQuestion = "How many friends do you have on Steam?";
            Min = 0;
            bool format = true;
            if (UnlockedAchievements.Length > 0 && RandomBool)
            {
                CalculateOnSubmit = null;
                format = false;
                var SelectedAchievement = UnlockedAchievements[RND.Range(0, UnlockedAchievements.Length)];
                string AchievementName = $"the \n\"{SelectedAchievement.Name}\"\nachievement?";
                var UnlockDate = SelectedAchievement.UnlockDate;
                switch (RND.Range(1, 6))
                {
                    case 1:     //Year
                        CurrentQuestion = "In which year have you\nunlocked " + AchievementName;
                        Answer = UnlockDate.Year;
                        Min = 2015;
                        break;
                    case 2:     //Month
                        CurrentQuestion = "In which month have you\nunlocked " + AchievementName;
                        Answer = UnlockDate.Month;
                        Min = 1;
                        Max = 12;
                        break;
                    case 3:     //Day
                        CurrentQuestion = "On which day have you\nunlocked " + AchievementName;
                        Answer = UnlockDate.Day;
                        Min = 1;
                        Max = 31;
                        break;
                    case 4:     //Hour
                        CurrentQuestion = "When have you\nunlocked " + AchievementName + " (hour)";
                        Answer = UnlockDate.Hour;
                        Max = 23;
                        break;
                    case 5:     //Minute
                        CurrentQuestion = "When have you\nunlocked " + AchievementName + " (minute)";
                        Answer = UnlockDate.Minute;
                        Max = 59;
                        break;
                }
            }
            else switch (Service.Friends.Count > 0)
            {
                case true when RandomBool:
                {
                    SteamFriendState selected_state = (SteamFriendState)RND.Range(0, 3);
                    CurrentQuestion =
                        $"How many Steam friends do you have who are {(selected_state == SteamFriendState.Offline ? "offline" : "online/away")}?";
                    CalculateOnSubmit = () => Service.Friends.Values.Count(f => CheckState(selected_state, f));
                    break;
                }
                case true when RandomBool:
                    CurrentQuestion = "How many of your friends are playing KTaNE right now?";
                    CalculateOnSubmit = () => Service.Friends.Values.Count(f => f.IsPlayingKtane);
                    break;
            }
            CurrentAnswer = Min ?? 0;
            WriteQuestion(format);
            Module.Log("Answer: {0}", CalculateOnSubmit != null ? "*calculated on submission*" : Answer.ToString());
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

        private bool CheckState(SteamFriendState SelectedState, SteamFriend Friend)
        {
            var state = Friend.CurrentState;
            return SelectedState == SteamFriendState.Offline
                ? state == SteamFriendState.Offline
                : state == SteamFriendState.Online || state == SteamFriendState.Away;
        }
        
        public IntAnswer(TextMesh display_text, TextMesh input_text, qkSteamSelector module) :
            base(display_text, input_text, module)
        {
            UnlockedAchievements = Service.Achievements.Where(ach => ach.Unlocked).ToArray();
        }
    }
}
