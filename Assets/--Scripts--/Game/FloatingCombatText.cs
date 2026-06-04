using TMPro;
using UnityEngine;

public class FloatingCombatText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text text;

    [Header("Motion")]
    [SerializeField] private Vector3 worldMove = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float lifetime = 1.2f;

    [Header("Scale")]
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1.2f, 1f, 1f);

    private Camera mainCamera;
    private Vector3 startPosition;
    private float elapsed;

    private void Awake()
    {
        if (text == null)
        {
            text = GetComponentInChildren<TMP_Text>();
        }

        mainCamera = Camera.main;
        startPosition = transform.position;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        float t = elapsed / lifetime;

        transform.position = Vector3.Lerp(
            startPosition,
            startPosition + worldMove,
            t
        );

        transform.localScale = Vector3.one * scaleCurve.Evaluate(t);

        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }

        if (text != null)
        {
            Color color = text.color;
            color.a = Mathf.Lerp(1f, 0f, t);
            text.color = color;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}