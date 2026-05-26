using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;

public class SpellCastingController : MonoBehaviour
{
    [Header("Spells")]
    [SerializeField] private Fireball fireball;
    [SerializeField] private Frostbolt frostbolt;

    [Header("Keyboard Input")]
    [SerializeField] private Key fireballKey = Key.R;
    [SerializeField] private Key frostboltKey = Key.Y;

    [Header("Targeting")]
    [SerializeField] private TargetingSystem targetingSystem;

    [Header("Cast Points")]
    [SerializeField] private Transform voiceCastPoint;
    [SerializeField] private Transform keyboardCastPoint;

    [Header("Animation")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string fireballTrigger = "CastSpell";
    [SerializeField] private string frostboltTrigger = "CastFrost";

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        keywordRecognizer = new KeywordRecognizer(new string[]
        {
            "fire",
            "frost"
        });

        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("SpellCastingController ready. Voice: fire/frost. Keys: R/Y.");
    }
    
    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[fireballKey].wasPressedThisFrame)
        {
            CastFireballFromKeyboard();
        }

        if (Keyboard.current[frostboltKey].wasPressedThisFrame)
        {
            CastFrostboltFromKeyboard();
        }
    }

    private void CastFireballFromKeyboard()
    {
        CastFireball(SpellInputSource.Keyboard);
    }

    private void CastFrostboltFromKeyboard()
    {
        CastFrostbolt(SpellInputSource.Keyboard);
    }

    private void CastFireballFromVoice()
    {
        CastFireball(SpellInputSource.Voice);
    }

    private void CastFrostboltFromVoice()
    {
        CastFrostbolt(SpellInputSource.Voice);
    }

    private void CastFireball(SpellInputSource inputSource)
    {
        if (fireball == null)
        {
            Debug.LogWarning("SpellCastingController is missing Fireball reference.");
            return;
        }

        SpellCastRequest request = BuildRequest(inputSource);

        if (!request.HasValidTarget)
        {
            Debug.Log("No valid target for fireball.");
            return;
        }

        TriggerAnimation(fireballTrigger);

        fireball.Cast(request);
    }

    private void CastFrostbolt(SpellInputSource inputSource)
    {
        if (frostbolt == null)
        {
            Debug.LogWarning("SpellCastingController is missing Frostbolt reference.");
            return;
        }

        SpellCastRequest request = BuildRequest(inputSource);

        if (!request.HasValidTarget)
        {
            Debug.Log("No valid target for frostbolt.");
            return;
        }

        TriggerAnimation(frostboltTrigger);

        frostbolt.Cast(request);
    }

    private SpellCastRequest BuildRequest(SpellInputSource inputSource)
    {
        EyeTargetable target = GetTargetForInputSource(inputSource);
        Transform castPoint = GetCastPointForInputSource(inputSource);

        return new SpellCastRequest(
            target,
            castPoint,
            inputSource
        );
    }

    private EyeTargetable GetTargetForInputSource(SpellInputSource inputSource)
    {
        if (targetingSystem == null)
        {
            Debug.LogWarning("SpellCastingController is missing TargetingSystem reference.");
            return null;
        }

        if (inputSource == SpellInputSource.Keyboard)
        {
            return targetingSystem.SelectedTarget;
        }

        return targetingSystem.EyeTarget;
    }

    private Transform GetCastPointForInputSource(SpellInputSource inputSource)
    {
        if (inputSource == SpellInputSource.Voice)
        {
            return voiceCastPoint;
        }

        return keyboardCastPoint;
    }

    private void TriggerAnimation(string triggerName)
    {
        if (characterAnimator == null) return;
        if (string.IsNullOrWhiteSpace(triggerName)) return;

        characterAnimator.SetTrigger(triggerName);
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        string command = args.text.ToLower();

        if (command == "fire")
        {
            CastFireballFromVoice();
        }
        else if (command == "frost")
        {
            CastFrostboltFromVoice();
        }
    }

    private void OnDestroy()
    {
        if (keywordRecognizer == null) return;

        if (keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
        }

        keywordRecognizer.Dispose();
    }
}