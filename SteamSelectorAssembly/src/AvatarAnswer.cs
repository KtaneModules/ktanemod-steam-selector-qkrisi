using System;
using System.Linq;
using UnityEngine;
using RND = UnityEngine.Random;

namespace SteamSelector
{
    public class AvatarAnswer : Question
    {
        private SteamAvatar Answer;
        private SteamAvatar CurrentAvatar;
        private SteamAvatar[] PossibleAvatars;
        private int index;

        private readonly Renderer AnswerCube;
        
        public override int Weight
        {
            get
            {
                return Service.Friends.Count > 1 ? 1 : 0;
            }
        }
        
        public override bool CorrectAnswer
        {
            get
            {
                return CurrentAvatar.Equals(Answer);
            }
        }
        
        protected override void WriteAnswer(bool override_wait = true)
        {
            AnswerCube.material.mainTexture = CurrentAvatar.AvatarTexture;
            AnswerCube.enabled = true;
            Input.text = "";
            EnableButtons();
        }

        public override void Generate()
        {
            base.Generate();
            var all_avatars = Service.Friends.Values.Select(friend => friend.Avatar).ToArray();
            PossibleAvatars = all_avatars.Distinct().ToArray();
            Answer = all_avatars[RND.Range(0, all_avatars.Length)];
            CurrentAvatar = PossibleAvatars[0];
            CurrentQuestion = $"Please select the avatar of\n{Answer.Username}!";
            WriteQuestion(false);
        }

        public override void Cycle(int increment)
        {
            ButtonsEnabled = false;
            index += increment;
            if(index < 0)
                index = PossibleAvatars.Length+index;
            index %= PossibleAvatars.Length;
            CurrentAvatar = PossibleAvatars[index];
            WriteAnswer();
        }

        public AvatarAnswer(TextMesh display_text, TextMesh input_text, qkSteamSelector module, Renderer answer_cube) :
            base(display_text, input_text, module)
        {
            AnswerCube = answer_cube;
        }
    }
}