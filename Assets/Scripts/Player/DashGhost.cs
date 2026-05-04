using UnityEngine;

public class DashGhost : MonoBehaviour

{
    public SpriteRenderer sr;
    private Color color;
    private float alpha;
    private float fadeSpeed;

    public void Init(Sprite sprite, Vector3 position, Quaternion rotation, Vector3 scale, Color ghostColor, float fadeSpeed)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
        sr.sprite = sprite;
        color = ghostColor;
        alpha = ghostColor.a;
        this.fadeSpeed = fadeSpeed;
        sr.color = color;
    }

    void Update()
    {
        alpha -= fadeSpeed * Time.deltaTime;
        color.a = alpha;
        sr.color = color;

        if (alpha <= 0)
        {
            Destroy(gameObject);
        }
    }
}