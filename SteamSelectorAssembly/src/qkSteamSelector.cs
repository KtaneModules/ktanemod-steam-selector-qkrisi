using System;
using System.Collections;
using System.Collections.Generic;
using SteamSelector;
using UnityEngine;
using RND = UnityEngine.Random;

public class qkSteamSelector : MonoBehaviour
{
    #pragma warning disable 649
    internal class SteamSelectorSettings
    {
        public bool FastInputAnimation = true;
    }
    
    [SerializeField] private TextMesh DisplayText;
    [SerializeField] private TextMesh InputText;
    [SerializeField] private KMBombModule Module;
    [SerializeField] private KMAudio Audio;
    [SerializeField] private KMSelectable LeftButton;
    [SerializeField] private KMSelectable RightButton;
    [SerializeField] private KMSelectable SubmitButton;
    [SerializeField] private Renderer AnswerCube;
    [SerializeField] private Renderer QuestionAvatar;
    [SerializeField] private GameObject ErrorScreen;
    [SerializeField] private KMSelectable SolveButton;
    [SerializeField] private KMSelectable ExtrasButton;
    [SerializeField] private TextMesh ExtrasText;
    #pragma warning restore 649

    internal const string SettingsFile = "SteamSelector.json";
    internal SteamSelectorSettings settings;
    
    private int ModuleID;
    private static int ModuleIDCounter;

    private bool _active;
    private int stage = 1;
    private Question CurrentQuestion;
    bool extras;

    private bool ButtonsEnabled
    {
        get
        {
            return _active && CurrentQuestion != null && CurrentQuestion.ButtonsEnabled;
        }
    }
    
    public void Log(string msg, params object[] args)
    {
        Debug.LogFormat("[Steam Selector #{0}] {1}", ModuleID, String.Format(msg, args));
    }

    private void DeactivateErrorScreen()
    {
        ErrorScreen.SetActive(!qkSteamSelectorService.Instance.initialized);
    }

    void Awake()
    {
        ModuleIDCounter = 0;
        settings = ModConfigHelper.ReadConfig<SteamSelectorSettings>(SettingsFile);
        if(!Application.isEditor)
            DeactivateErrorScreen();
    }

    void GenerateQuestion()
    {
        if(!qkSteamSelectorService.Instance.initialized)
            return;
        AnswerCube.enabled = false;
        Question[] AvailableQuestions = new Question[]
        {
            new AvatarAnswer(DisplayText, InputText, this, AnswerCube),
            new StringAnswer(DisplayText, InputText, this, QuestionAvatar),
            new IntAnswer(DisplayText, InputText, this)
        };
        List<Question> Selection = new List<Question>();
        foreach (var question in AvailableQuestions)
        {
            int weight = question.Weight;
            for (int i = 0; i < weight; i++)
                Selection.Add(question);
        }
        CurrentQuestion = Selection[RND.Range(0, Selection.Count)];
        CurrentQuestion.Generate();
    }
    
    void Start()
    {
        if(Application.isEditor)
            DeactivateErrorScreen();
        ModuleID = ++ModuleIDCounter;
        Log("Steam api {0} initialized.", qkSteamSelectorService.Instance.initialized ? "is" : "isn't");
        Module.OnActivate += () =>
        {
            _active = true;
            GenerateQuestion();
        };
        LeftButton.OnInteract += () => PressButton(LeftButton, -1);
        RightButton.OnInteract += () => PressButton(RightButton, 1);
        SubmitButton.OnInteract += () =>
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SubmitButton.transform);
            SubmitButton.AddInteractionPunch(.5f);
            if (!extras && ButtonsEnabled)
            {
                QuestionAvatar.enabled = false;
                flashing = false;
                if (CurrentQuestion.CorrectAnswer)
                {
                    if (++stage < 4)
                    {
                        Log("Answer correct! Advancing to next stage...");
                        GenerateQuestion();
                    }
                    else
                    {
                        _active = false;
                        AnswerCube.enabled = false;
                        StopCoroutine(FlashRoutine);
                        ExtrasText.text = "▲";
                        Module.HandlePass();
                        Log("Module solved!");
                        StartCoroutine(TextMeshUtils.WriteText(DisplayText, "Module solved :D", () =>
                            StartCoroutine(TextMeshUtils.WriteText(InputText, "GG!",
                                () => { }, true)), false));
                    }
                }
                else
                {
                    Log("Incorrect answer!");
                    Module.HandleStrike();
                    stage = 1;
                    GenerateQuestion();
                }
            }
            return false;
        };
        SolveButton.OnInteract += () =>
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SubmitButton.transform);
            SubmitButton.AddInteractionPunch(.5f);
            if (_active)
                Module.HandlePass();
            return false;
        };
        ExtrasButton.OnInteract += () =>
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ExtrasButton.transform);
            if (ButtonsEnabled && CurrentQuestion.ExtrasEnabled)
            {
                extras = !extras;
                flashing = false;
                CurrentQuestion.ToggleScreen(extras);
            }
            return false;
        };
        FlashRoutine = StartCoroutine(Flash());
    }

    private bool PressButton(KMSelectable button, int increment)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
        if (ButtonsEnabled)
        {
            if (extras)
                CurrentQuestion.CycleScreen(increment);
            else CurrentQuestion.Cycle(increment);
        }
        return false;
    }

    internal bool flashing;
    private const float FlashWait = .3f;
    private Coroutine FlashRoutine;

    private IEnumerator Flash()
    {
        while (true)
        {
            if (flashing)
            {
                ExtrasText.text = "";
                yield return new WaitForSecondsRealtime(FlashWait);
                ExtrasText.text = "▲";
                yield return new WaitForSecondsRealtime(FlashWait);
            }
            yield return null;
        }
    }
    
    #pragma warning disable 414
    public static Dictionary<string, object>[] TweaksEditorSettings =
    {
        new Dictionary<string, object>
        {
            {"Filename", SettingsFile},
            {"Name", "Steam Selector"},
            {
                "Listings", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {"Key", "FastInputAnimation"}, {"Text", "Fast input animation"},
                        {"Description", "Enabling this setting makes scrolling through the answers is faster."}
                    },
                }
            }
        }
    };
    #pragma warning restore 414
}
