using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance;

    [Header("Composants")]
    public CinemachineCamera vCam;
    private CinemachineBasicMultiChannelPerlin noise;

    [Header("Paramètres de Fluidité")]
    public float shakeSmoothness = 20f;
    public float returnSmoothness = 10f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (vCam != null)
            noise = vCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    /// Shake avec oscillation fluide et retour progressif à zéro.
    public IEnumerator Shake(float duration, float posAmplitude, float dutchAmplitude)
    {
        if (noise == null || vCam == null) yield break;

        float originalDutch = 0f;
        float elapsed = 0.0f;

        noise.AmplitudeGain = posAmplitude;
        noise.FrequencyGain = 1f;

        float targetDutch = 0f;

        while (elapsed < duration)
        {
            if (elapsed % 0.05f < 0.01f)
                targetDutch = Random.Range(-1f, 1f) * dutchAmplitude;

            vCam.Lens.Dutch = Mathf.Lerp(vCam.Lens.Dutch, targetDutch, Time.deltaTime * shakeSmoothness);

            elapsed += Time.deltaTime;
            yield return null;
        }

        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;

        while (!Mathf.Approximately(vCam.Lens.Dutch, originalDutch))
        {
            vCam.Lens.Dutch = Mathf.Lerp(vCam.Lens.Dutch, originalDutch, Time.deltaTime * returnSmoothness);

            if (Mathf.Abs(vCam.Lens.Dutch) < 0.01f)
            {
                vCam.Lens.Dutch = originalDutch;
                break;
            }

            yield return null;
        }
    }
}