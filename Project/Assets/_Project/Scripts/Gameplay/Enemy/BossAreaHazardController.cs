using System.Collections;
using UnityEngine;

public class BossAreaHazardController : MonoBehaviour
{
    [SerializeField] ParticleSystem hazardAreaPrefab;
    [SerializeField] ParticleSystem hazardEffectPrefab;
    [SerializeField] Collider hazardAreaCollider;

    [SerializeField, Range(1f, 3f)] float hazardWarningDuration = 2f;
    [SerializeField, Range(1f, 5f)] float hazardEffectDuration = 3f;
    [SerializeField, Min(0.01f)] float hazardSize = 2f;

    void Start()
    {
        StartCoroutine(HazardSequence());
    }

    IEnumerator HazardSequence()
    {
        hazardAreaCollider.enabled = false;

        ParticleSystem.MainModule areaMain = hazardAreaPrefab.main;

        areaMain.startSizeMultiplier = 0f;

        hazardAreaPrefab.Play();

        float elapsed = 0f;
        while (elapsed < hazardWarningDuration)
        {
            elapsed += Time.deltaTime;
            float size = Mathf.Lerp(0f, hazardSize, elapsed / hazardWarningDuration);

            areaMain.startSizeMultiplier = size;

            yield return null;
        }

        // Brief pause before activating the hazard effect
        yield return new WaitForSeconds(1f);

        areaMain.startSizeMultiplier = hazardSize;

        hazardEffectPrefab.Play();
        hazardAreaCollider.enabled = true;
        yield return new WaitForSeconds(hazardEffectDuration);

        // Deactivate phase: disable collision and stop effect
        hazardAreaCollider.enabled = false;
        hazardAreaPrefab.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        hazardEffectPrefab.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // Cleanup phase: wait for particles to finish fading out before destroying
        while (hazardEffectPrefab.IsAlive(true))
        {
            yield return null;
        }

        Destroy(gameObject);
    }
}
