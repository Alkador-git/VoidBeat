using UnityEngine;

public class TutorialPrompt : MonoBehaviour
{
    [Header("Réglages")]
    public SpriteRenderer promptSprite;
    public float fadeInSpeed = 5f;
    private bool isVisible = false;

    void Start()
    {
        if (promptSprite) promptSprite.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        if (promptSprite == null) return;
        float targetAlpha = isVisible ? 1f : 0f;
        float newAlpha = Mathf.MoveTowards(promptSprite.color.a, targetAlpha, Time.deltaTime * fadeInSpeed);
        promptSprite.color = new Color(1, 1, 1, newAlpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Ghost"))
        {
            isVisible = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isVisible = false;
    }
}