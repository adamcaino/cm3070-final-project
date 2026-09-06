using System.Collections;
using UnityEngine;

// Runs the warning, damage, particle cleanup, and destruction sequence for one area hazard.
public class BossAreaHazardController : MonoBehaviour
{
    [SerializeField] ParticleSystem hazardAreaPrefab;
    [SerializeField] ParticleSystem hazardEffectPrefab;
    [SerializeField] Collider hazardAreaCollider;

    [SerializeField, Range(1f, 3f)] float hazardWarningDuration = 2f;
    [SerializeField, Range(1f, 5f)] float hazardEffectDuration = 3f;
    [SerializeField, Min(0.01f)] float hazardSize = 2f;

    Coroutine hazardRoutine;
    bool isStopping;

    // Starts the hazard warning and effect sequence.
    void Start()
    {
        hazardRoutine = StartCoroutine(HazardSequence());
    }

    // Stops hazard activity and waits for remaining particles before destruction.
    public void StopHazard()
    {
        if (isStopping)
        {
            return;
        }

        isStopping = true;

        if (hazardRoutine != null)
        {
            StopCoroutine(hazardRoutine);
            hazardRoutine = null;
        }

        if (hazardAreaCollider != null)
        {
            hazardAreaCollider.enabled = false;
        }

        if (hazardAreaPrefab != null)
        {
            hazardAreaPrefab.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (hazardEffectPrefab != null)
        {
            hazardEffectPrefab.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        StartCoroutine(DestroyWhenParticlesFinish());
    }

    // Expands the warning area, enables damage for the effect duration, and begins cleanup.
    IEnumerator HazardSequence()
    {
        isStopping = false;
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

        yield return DestroyWhenParticlesFinish();
    }

    // Waits for both particle systems to finish before destroying the hazard object.
    IEnumerator DestroyWhenParticlesFinish()
    {
        while ((hazardAreaPrefab != null && hazardAreaPrefab.IsAlive(true))
            || (hazardEffectPrefab != null && hazardEffectPrefab.IsAlive(true)))
        {
            yield return null;
        }

        Destroy(gameObject);
    }
}
