using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceCommandTester : MonoBehaviour
{
    public PlayerMovement playerMovement;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> commands;

    private void Start()
    {
        commands = new Dictionary<string, System.Action>
        {
            { "jump", JumpCommand }
        };

        keywordRecognizer = new KeywordRecognizer(new string[] { "jump" });

        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();

        Debug.Log("VoiceCommandTester started. Say: jump");
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Recognised voice command: " + args.text);

        if (commands.TryGetValue(args.text, out System.Action command))
        {
            command.Invoke();
        }
    }

    private void JumpCommand()
    {
        Debug.Log("Jump command triggered by voice.");

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement is not assigned.");
            return;
        }

        playerMovement.TryJump();
    }

    private void OnDestroy()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}