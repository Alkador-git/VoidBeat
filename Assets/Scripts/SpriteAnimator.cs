using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();
    [SerializeField] private float totalAnimationTime = 1f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool playOnStart = true;

    // Nouvelle configuration : délai aléatoire entre changements (en secondes)
    [SerializeField] private float minDelay = 0f;
    [SerializeField] private float maxDelay = 0f;

    // Compteur d'entrées dans un collider trigger (utilisé pour alterner la rotation Y)
    private int triggerEntryCount;

    private float elapsedTime;
    private bool isPlaying;

    // Utilisé en mode timer (quand minDelay/maxDelay configurés)
    private float currentDelay;
    private int currentSpriteIndex;

    private void OnValidate()
    {
        // Garantir valeurs valides en édition
        minDelay = Mathf.Max(0f, minDelay);
        maxDelay = Mathf.Max(0f, maxDelay);
        if (maxDelay < minDelay)
        {
            var t = minDelay;
            minDelay = maxDelay;
            maxDelay = t;
        }
    }

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (sprites.Count > 0)
        {
            currentSpriteIndex = 0;
            spriteRenderer.sprite = sprites[0];
        }

        // Initialiser délai courant
        currentDelay = GetNextDelay();

        if (playOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying || sprites.Count == 0)
        {
            return;
        }

        if (minDelay <= 0f && maxDelay <= 0f)
        {
            elapsedTime += Time.deltaTime;

            float transitionTime = totalAnimationTime / sprites.Count;

            // Utiliser modulo pour boucler correctement le temps
            float normalizedTime = elapsedTime % totalAnimationTime;
            int index = (int)(normalizedTime / transitionTime);

            if (index < 0 || index >= sprites.Count)
            {
                index = 0;
            }

            currentSpriteIndex = index;
            spriteRenderer.sprite = sprites[currentSpriteIndex];
            return;
        }

        // Mode timer avec délai aléatoire entre chaque changement
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= currentDelay)
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Count;
            spriteRenderer.sprite = sprites[currentSpriteIndex];

            elapsedTime -= currentDelay;
            currentDelay = GetNextDelay();
        }
    }

    public void Play()
    {
        isPlaying = true;
        elapsedTime = 0f;
        if (sprites.Count > 0 && (currentSpriteIndex < 0 || currentSpriteIndex >= sprites.Count))
        {
            currentSpriteIndex = 0;
            spriteRenderer.sprite = sprites[0];
        }
        currentDelay = GetNextDelay();
    }

    public void Stop()
    {
        isPlaying = false;
    }

    public void SetAnimationTime(float newTime)
    {
        totalAnimationTime = newTime;
    }

    public float GetTransitionTime()
    {
        if (sprites.Count == 0) return 0f;
        if (minDelay <= 0f && maxDelay <= 0f)
            return totalAnimationTime / sprites.Count;
        // Si mode aléatoire, renvoyer le délai courant si disponible, sinon la moyenne
        return currentDelay > 0f ? currentDelay : (minDelay + maxDelay) / 2f;
    }

    private float GetNextDelay()
    {
        if (minDelay <= 0f && maxDelay <= 0f)
        {
            return sprites.Count > 0 ? totalAnimationTime / sprites.Count : 0f;
        }

        // Random.Range pour float retourne [min, max)
        return Random.Range(minDelay, maxDelay);
    }

    /// Appel public à invoquer lorsqu'un ennemi entre dans votre collider trigger.
    public void ToggleRotationOnTriggerEntry()
    {
        triggerEntryCount++;

        // si impair -> -180, si pair -> 360
        if ((triggerEntryCount & 1) == 1)
        {
            SetYRotation(180f);
        }
        else
        {
            SetYRotation(0f);
        }
    }

    private void SetYRotation(float y)
    {
        Vector3 e = transform.eulerAngles;
        e.y = y;
        transform.eulerAngles = e;
    }
}