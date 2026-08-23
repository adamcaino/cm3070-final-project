using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sits on a full-screen Image in the UI scene. Flashes it to damageColour then fades back to
/// transparent whenever the player takes damage - mirrors HitFlash's hold-then-fade timing, just
/// driving an Image's alpha instead of a MaterialPropertyBlock.
/// </summary>
[RequireComponent(typeof(Image))]
public class ScreenDamageFlash : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [SerializeField] Color damageColour = new Color(0.7f, 0f, 0f, 0.35f);
  [SerializeField, Min(0f)] float holdDuration = 0.05f;
  [SerializeField, Min(0f)] float fadeDuration = 0.25f;

  Health health;
  Image image;
  Coroutine activeFlash;

  void Awake()
  {
    image = GetComponent<Image>();

    GameObject player = GameObject.FindGameObjectWithTag(PLAYERTAG);
    if (player == null)
    {
      Debug.LogWarning("ScreenDamageFlash: no GameObject tagged 'Player' found in the loaded scenes.");
      return;
    }

    health = player.GetComponent<Health>();
    if (health == null)
    {
      Debug.LogWarning("ScreenDamageFlash: player has no Health component.");
    }

    SetAlpha(0f);
  }

  void OnEnable()
  {
    if (health == null) return;

    health.OnDamaged += HandleDamaged;
  }

  void OnDisable()
  {
    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
  }

  void HandleDamaged(Vector3 _)
  {
    if (activeFlash != null)
    {
      StopCoroutine(activeFlash);
    }
    activeFlash = StartCoroutine(FlashAndFadeRoutine());
  }

  IEnumerator FlashAndFadeRoutine()
  {
    SetAlpha(damageColour.a);

    if (holdDuration > 0f)
    {
      yield return new WaitForSeconds(holdDuration);
    }

    float elapsed = 0f;
    while (elapsed < fadeDuration)
    {
      elapsed += Time.deltaTime;
      float t = fadeDuration > 0f ? Mathf.Clamp01(elapsed / fadeDuration) : 1f;
      SetAlpha(Mathf.Lerp(damageColour.a, 0f, t));
      yield return null;
    }

    SetAlpha(0f);
    activeFlash = null;
  }

  void SetAlpha(float alpha)
  {
    Color c = damageColour;
    c.a = alpha;
    image.color = c;
  }
}
