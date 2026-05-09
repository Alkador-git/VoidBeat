using UnityEngine;
using UnityEngine.UI;

public class LevelNode : MonoBehaviour
{
    public string sceneToLoad;
    public GameObject lockIcon;
    public Image outlineImage;
    public Button button;

    [Header("Couleurs d'état")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public void SetStatus(bool unlocked)
    {
        button.interactable = unlocked;

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (outlineImage != null)
            outlineImage.color = unlocked ? unlockedColor : lockedColor;
            
    }
}