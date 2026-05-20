using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceSpellCaster : MonoBehaviour
{
    [SerializeField] private PlayerSpellCaster playerSpellCaster;

    private KeywordRecognizer keywordRecognizer;

    private void Start()
    {
        string[] keywords = { "fireball" };

        keywordRecognizer = new KeywordRecognizer(keywords);
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("Voice spell caster started. Say: fireball");
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        string command = args.text.ToLower();

        Debug.Log("Voice command recognized: " + command);

        if (command == "fireball")
        {
            playerSpellCaster.TriggerFireball();
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