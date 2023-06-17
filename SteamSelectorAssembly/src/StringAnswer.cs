using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RND = UnityEngine.Random;

namespace SteamSelector
{
    public class StringAnswer : Question
    {
        private string[] Answers;
        private string Current;
        private string[] Possibles;
        private int index;
        private bool enable_avatar;
#pragma warning disable 649
        private bool enable_extras;
#pragma warning restore 649
        private bool write_extras;

        private List<int> Options = new List<int>() { 1 };

        private readonly Renderer QuestionAvatar;

        private readonly string[] Characters = new[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
            "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };

        private static readonly string[] Statuses = Enum.GetNames(typeof(SteamFriendState));

        public override int Weight
        {
            get
            {
                return Options.Distinct().Count();
            }
        }
        
        public override bool CorrectAnswer
        {
            get
            {
                return Answers.Contains(Current);
            }
        }

        protected override void WriteAnswer(bool override_wait = true)
        {
            QuestionAvatar.enabled = enable_avatar;
            if (write_extras)
            {
                Module.flashing = enable_extras;
                write_extras = false;
            }

            Module.StartCoroutine(TextMeshUtils.WriteText(Input, Current, EnableButtons, true,
                override_wait && Module.settings.FastInputAnimation ? 0f : 0.05f));
        }

        public override void Generate()
        {
            base.Generate();
            enable_avatar = false;
            bool format = true;
            switch (Options[RND.Range(0, Options.Count)])
            {
                case 1:     //Achievement
                    var SelectedAchievement = Service.Achievements[RND.Range(0, Service.Achievements.Count)];
                    CurrentQuestion = $"Have you unlocked the\n\"{SelectedAchievement.Name}\"\nachievement?";
                    Possibles = new[] { "Yes", "No" };
                    Answers = new[] { SelectedAchievement.Unlocked ? "Yes" : "No" };
                    format = false;
                    break;
                case 2:     //Friends
                    var Friends = Service.Friends.Values.ToArray();
                    if (RandomBool)   //Avatar
                    {
                        var selected_avatar = Friends[RND.Range(0, Friends.Length)].Avatar;
                        QuestionAvatar.material.mainTexture = selected_avatar.AvatarTexture;
                        CurrentQuestion = "Who does this avatar belong to?";
                        Answers = Friends.Where(f => f.Avatar.Equals(selected_avatar)).Select(f => f.Name).ToArray();
                        Possibles = Friends.Select(f => f.Name).Distinct().ToArray();
                        Array.Sort(Possibles);
                        enable_avatar = true;
                    }
                    else    //Sort
                    {
                        bool reverse = RandomBool;
                        /*if (RandomBool && Friends.Any(f => f.Level > 0))   //Level
                        {
                            var no_levels = Friends.Where(f => f.Level == 0).Select(f => f.Name).ToArray();
                            Friends = reverse
                                ? Friends.Where(f => f.Level > 0).OrderByDescending(f => f.Level).ThenBy(f => f.Name).ToArray()
                                : Friends.Where(f => f.Level > 0).OrderBy(f => f.Level).ThenBy(f => f.Name).ToArray();
                            if (no_levels.Length > 0)
                            {
                                enable_extras = true;
                                write_extras = true;
                                List<string> screens = new List<string>();
                                string screen = "Exclude the following users:";
                                int rows = 1;
                                foreach (var no_level in no_levels)
                                {
                                    screen += (rows++ > 0 ? "\n" : "") + no_level;
                                    if (rows == 9)
                                    {
                                        screens.Add(screen);
                                        screen = "";
                                        rows = 0;
                                    }
                                }
                                if (rows > 0)
                                    screens.Add(screen);
                                ExtrasScreen = screens.ToArray();
                            }
                            Possibles = Friends.Select(f => f.Name).Distinct().ToArray();
                            int index = RND.Range(0, Friends.Length);
                            CurrentQuestion =
                                String.Format(
                                    "Who is the {0} on the list of your Steam friends sorted by their Steam level? ({1})",
                                    GetStringByNum(index + 1), reverse ? "High-Low" : "Low-High");
                            Answers = new[] { Friends[index].Name };
                        }*/
                            //Name
                            Friends = reverse
                                ? Friends.OrderByDescending(f => f.Name).ToArray()
                                : Friends.OrderBy(f => f.Name).ToArray();
                            Possibles = Characters;
                            int index;
                            string name;
                            do
                            {
                                index = RND.Range(0, Friends.Length);
                            } while (!ModifyName(Friends[index].Name, out name));
                            int letter = RND.Range(0, name.Length);
                            CurrentQuestion =
                                $"What is the {GetStringByNum(letter + 1)} letter/digit of the {GetStringByNum(index + 1)} person on your list of Steam friends sorted alphabetically? ({(reverse ? "Z-A" : "A-Z")})";
                            Module.Log("Person in question: {0}", name);
                            Answers = new[] { name[letter].ToString().ToUpperInvariant() };
                        }
                    break;
                case 3:     //Messages
                    var messages = Service.Messages.ToList();
                    messages.Reverse();
                    int ind;
                    string message;
                    string user;
                    do
                    {
                        ind = RND.Range(0, messages.Count);
                    } while (!ModifyMessage(messages[ind], out message, out user));
                    if (RandomBool)
                    {
                        int letter = RND.Range(0, message.Length);
                        CurrentQuestion =
                            $"What is the {GetStringByNum(letter + 1)} letter/digit of the {(ind == 0 ? "" : GetStringByNum(ind + 1) + " ")}{"last message you've received on Steam?"}";
                        Answers = new[] { message[letter].ToString().ToUpperInvariant() };
                        Possibles = Characters;
                    }
                    else
                    {
                        CurrentQuestion =
                            $"From whom have you received your {(ind == 0 ? "" : GetStringByNum(ind + 1) + " ")}last message on Steam?";
                        var _Possibles = Service.Friends.Values.Select(f => f.Name).ToList();
                        if(!_Possibles.Contains(user))
                            _Possibles.Add(user);
                        Answers = new[] { user };
                        Possibles = _Possibles.Distinct().ToArray();
                        Array.Sort(Possibles);
                    }
                    break;
                case 4:     //Status
                    format = false;
                    var friends = Service.Friends.Values.ToArray();
                    var selected_friend = friends[RND.Range(0, friends.Length)];
                    CurrentQuestion = $"What is the current state of \n{selected_friend.Name}?";
                    Answers = new[] { selected_friend.CurrentState.ToString() };
                    Possibles = Statuses;
                    break;
            }
            Current = Possibles[0];
            WriteQuestion(format);
            Module.Log(Answers.Length > 1
                ? $"Accepted answers: {String.Join(", ", Answers)}"
                : $"Answer: {Answers[0]}");
        }

        private bool ModifyMessage(SteamMessage message, out string msg, out string user)
        {
            msg = ModifyName(message.Message);
            user = ModifyName(message.User);
            return msg.Length > 0 && user.Length > 0;
        }

        private bool ModifyName(string name, out string output)
        {
            output = ModifyName(name);
            return output.Length > 0;
        }
        
        public override void Cycle(int increment)
        {
            ButtonsEnabled = false;
            index += increment;
            if(index < 0)
                index = Possibles.Length+index;
            index %= Possibles.Length;
            Current = Possibles[index];
            WriteAnswer();
        }
        
        string GetStringByNum(int num)
        {
            switch (num) 
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
                default: 
                    return $"{num}{(num % 10 == 1 ? "st" : num % 10 == 2 ? "nd" : num % 10 == 3 ? "rd" : "th")}";
            }
        }

        private string ModifyName(string name)
        {
            name = name.ToUpperInvariant();
            var l = new List<string>(name.Select(c => c.ToString()));
            var final = new List<string>();
            foreach (string c in l)
            {
                if (Characters.Contains(c)) final.Add(c);
            }
            return String.Join("", final.ToArray());
        }
        
        public StringAnswer(TextMesh display_text, TextMesh input_text, qkSteamSelector module, Renderer question_avatar) :
            base(display_text, input_text, module)
        {
            QuestionAvatar = question_avatar;
            if (Service.Friends.Count > 1)
            {
                Options.Add(2);
                Options.Add(2);
                Options.Add(4);
            }
            string _;
            string __;
            if (Service.Messages.Any(msg => ModifyMessage(msg, out _, out __)))
            {
                Options.Add(3);
                Options.Add(3);
            }
        }
    }
}
