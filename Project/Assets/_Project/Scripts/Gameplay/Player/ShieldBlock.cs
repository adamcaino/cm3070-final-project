using UnityEngine;

[RequireComponent(typeof(AudioSource))]
// Blocks incoming damage while the player defends within the configured front angle.
public class ShieldBlock : MonoBehaviour, IDamageBlocker
{
  [SerializeField] AudioClip blockSfxClip;

  [SerializeField, Range(0f, 180)] float blockAngle = 120f;

  [SerializeField] Color flashColour = Color.white;

  AudioSource audioSource;
  PlayerAttack playerAttack;
  HitFlash shieldFlash;

  // Caches audio, player attack, and shield flash components.
  void Awake()
  {
    audioSource = GetComponent<AudioSource>();
    playerAttack = GetComponent<PlayerAttack>();
    shieldFlash = GetComponent<HitFlash>();
  }

  // Returns true and plays feedback when the incoming hit is inside the block angle.
  public bool TryBlock(GameObject source, Vector3 hitPoint)
  {
    if (playerAttack == null || !playerAttack.IsDefending) return false;

    Vector3 attackerPosition = source != null ? source.transform.position : hitPoint;
    Vector3 toAttacker = attackerPosition - transform.position;
    toAttacker.y = 0f;

    if (toAttacker.sqrMagnitude < 0.0001f) return false;

    float angle = Vector3.Angle(transform.forward, toAttacker.normalized);
    if (angle > blockAngle * 0.5f) return false;

    PlayBlockFeedback();
    return true;
  }

  // Plays the block sound and flashes the shield material.
  void PlayBlockFeedback()
  {
    if (blockSfxClip != null)
    {
      audioSource.PlayOneShot(blockSfxClip);
    }

    if (shieldFlash != null)
    {
      shieldFlash.Flash(flashColour);
    }
  }
}
