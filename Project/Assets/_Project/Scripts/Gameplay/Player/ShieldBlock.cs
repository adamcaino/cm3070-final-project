using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ShieldBlock : MonoBehaviour, IDamageBlocker
{
  [SerializeField] AudioClip blockSfxClip;

  [SerializeField, Range(0f, 180)] float blockAngle = 120f;

  [SerializeField] Color flashColour = Color.white;

  AudioSource audioSource;
  PlayerAttack playerAttack;
  HitFlash shieldFlash;

  void Awake()
  {
    audioSource = GetComponent<AudioSource>();
    playerAttack = GetComponent<PlayerAttack>();
    shieldFlash = GetComponent<HitFlash>();
  }

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
