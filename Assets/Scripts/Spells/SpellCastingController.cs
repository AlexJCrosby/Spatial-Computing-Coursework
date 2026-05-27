using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows.Speech;
using System.Collections;

public class SpellCastingController : MonoBehaviour
{
    [Header("Spells")]
    [SerializeField] private Fireball fireball;
    [SerializeField] private Frostbolt frostbolt;
    [SerializeField] private AOEKnockback knockback;

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

    [Header("Voice Volley Settings")]
    [SerializeField] private int fireballVoiceCastCount = 5;
    [SerializeField] private float fireballVoiceCastInterval = 1f;
    [SerializeField] private float fireballVoiceCooldown = 8f;

    [SerializeField] private int frostboltVoiceCastCount = 5;
    [SerializeField] private float frostboltVoiceCastInterval = 1f;
    [SerializeField] private float frostboltVoiceCooldown = 8f;

    private float fireballVoiceCooldownRemaining;
    private float frostboltVoiceCooldownRemaining;
    private bool fireballVolleyActive;
    private bool frostboltVolleyActive;
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
        TickVoiceCooldowns();
        HandleKeyboardInput();
    }

    private void TickVoiceCooldowns()
    {
        if (fireballVoiceCooldownRemaining > 0f)
        {
            fireballVoiceCooldownRemaining -= Time.deltaTime;
        }

        if (frostboltVoiceCooldownRemaining > 0f)
        {
            frostboltVoiceCooldownRemaining -= Time.deltaTime;
        }
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[fireballKey].wasPressedThisFrame)
        {
            CastFireball(SpellInputSource.Keyboard);
        }

        if (Keyboard.current[frostboltKey].wasPressedThisFrame)
        {
            CastFrostbolt(SpellInputSource.Keyboard);
        }
    }

    private void CastFireballFromVoice()
    {
        if (fireballVolleyActive)
        {
            Debug.Log("Fireball voice volley is already active.");
            return;
        }

        if (fireballVoiceCooldownRemaining > 0f)
        {
            Debug.Log("Fireball voice volley is on cooldown.");
            return;
        }

        StartCoroutine(VoiceVolleyRoutine(
            castAction: () => CastFireball(SpellInputSource.Voice, true),
            castCount: fireballVoiceCastCount,
            interval: fireballVoiceCastInterval,
            onStart: () => fireballVolleyActive = true,
            onEnd: () =>
            {
                fireballVolleyActive = false;
                fireballVoiceCooldownRemaining = fireballVoiceCooldown;
            }
        ));
    }

    private void CastFrostboltFromVoice()
    {
        if (frostboltVolleyActive)
        {
            Debug.Log("Frostbolt voice volley is already active.");
            return;
        }

        if (frostboltVoiceCooldownRemaining > 0f)
        {
            Debug.Log("Frostbolt voice volley is on cooldown.");
            return;
        }

        StartCoroutine(VoiceVolleyRoutine(
            castAction: () => CastFrostbolt(SpellInputSource.Voice, true),
            castCount: frostboltVoiceCastCount,
            interval: frostboltVoiceCastInterval,
            onStart: () => frostboltVolleyActive = true,
            onEnd: () =>
            {
                frostboltVolleyActive = false;
                frostboltVoiceCooldownRemaining = frostboltVoiceCooldown;
            }
        ));
    }

    private void CastFireball(SpellInputSource inputSource, bool bypassCooldown = false)
    {
        if (fireball == null)
        {
            Debug.LogWarning("SpellCastingController is missing Fireball reference.");
            return;
        }

        SpellCastRequest request = BuildRequest(inputSource, bypassCooldown);

        if (!request.HasValidTarget)
        {
            Debug.Log("No valid target for fireball.");
            return;
        }

        TriggerAnimation(fireballTrigger);
        fireball.Cast(request);
    }

    private void CastFrostbolt(SpellInputSource inputSource, bool bypassCooldown = false)
    {
        if (frostbolt == null)
        {
            Debug.LogWarning("SpellCastingController is missing Frostbolt reference.");
            return;
        }

        SpellCastRequest request = BuildRequest(inputSource, bypassCooldown);

        if (!request.HasValidTarget)
        {
            Debug.Log("No valid target for frostbolt.");
            return;
        }

        TriggerAnimation(frostboltTrigger);
        frostbolt.Cast(request);
    }

    private IEnumerator VoiceVolleyRoutine(
        System.Action castAction,
        int castCount,
        float interval,
        System.Action onStart,
        System.Action onEnd
    )
    {
        onStart?.Invoke();

        for (int i = 0; i < castCount; i++)
        {
            castAction?.Invoke();

            if (i < castCount - 1)
            {
                yield return new WaitForSeconds(interval);
            }
        }

        onEnd?.Invoke();
    }

    private SpellCastRequest BuildRequest(
        SpellInputSource inputSource,
        bool bypassCooldown = false
    )
    {
        EyeTargetable target = GetTargetForInputSource(inputSource);
        Transform castPoint = GetCastPointForInputSource(inputSource);

        return new SpellCastRequest(
            target,
            castPoint,
            inputSource,
            bypassCooldown
        );
    }

    private EyeTargetable GetTargetForInputSource(SpellInputSource inputSource)
    {
        if (targetingSystem == null)
        {
            Debug.LogWarning("SpellCastingController is missing TargetingSystem reference.");
            return null;
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

    public float GetCooldownFillAmount(SpellDefinition spell)
    {
        if (spell == null) return 0f;

        switch (spell.spellID)
        {
            case SpellID.Fireball:
                if (fireball == null) return 0f;
                return fireball.CooldownRemaining / fireball.CooldownDuration;

            case SpellID.Frostbolt:
                if (frostbolt == null) return 0f;
                return frostbolt.CooldownRemaining / frostbolt.CooldownDuration;

            case SpellID.Knockback:
                if (knockback == null) return 0f;
                return knockback.CooldownRemaining / knockback.CooldownDuration;

            case SpellID.FireVolley:
                return fireballVoiceCooldown > 0f
                    ? fireballVoiceCooldownRemaining / fireballVoiceCooldown
                    : 0f;

            case SpellID.FrostVolley:
                return frostboltVoiceCooldown > 0f
                    ? frostboltVoiceCooldownRemaining / frostboltVoiceCooldown
                    : 0f;

            default:
                return 0f;
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