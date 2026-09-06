using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Flashes a full-screen Image when the player takes damage, then fades it to transparent.
[RequireComponent(typeof(Image))]
// Flashes a full-screen damage colour when the player Health component reports damage.
public class ScreenDamageFlash : MonoBehaviour
{
  const string PLAYERTAG = "Player";

  [SerializeField] Color damageColour = new Color(0.7f, 0f, 0f, 0.35f);
  [SerializeField, Min(0f)] float holdDuration = 0.05f;
  [SerializeField, Min(0f)] float fadeDuration = 0.25f;

  Health health;
  Image image;
  Coroutine activeFlash;

  // Caches the Image and starts it transparent.
  void Awake()
  {
    image = GetComponent<Image>();
    SetAlpha(0f);
  }

  // Subscribes to dungeon readiness and attempts to bind the player health component.
  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
    RefreshHealthReference(false);
  }

  // Removes readiness and health event subscriptions.
  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;

    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
  }

  // Rebinds the player health reference after the dungeon is generated.
  void HandleDungeonReady()
  {
    RefreshHealthReference(true);
  }

  // Replaces the old health subscription with the current player's health component.
  void RefreshHealthReference(bool logIfMissing)
  {
    // Unsubscribe from old health if it exists
    if (health != null)
    {
      health.OnDamaged -= HandleDamaged;
    }

    GameObject player = GameObject.FindGameObjectWithTag(PLAYERTAG);
    if (player == null)
    {
      if (logIfMissing)
      {
        Debug.LogWarning("ScreenDamageFlash: no GameObject tagged 'Player' found in the loaded scenes.");
      }
      return;
    }

    health = player.GetComponent<Health>();
    if (health == null)
    {
      Debug.LogWarning("ScreenDamageFlash: player has no Health component.");
      return;
    }

    health.OnDamaged += HandleDamaged;
  }

  // Restarts the damage flash coroutine when the player is hit.
  void HandleDamaged(Vector3 _)
  {
    if (activeFlash != null)
    {
      StopCoroutine(activeFlash);
    }
    activeFlash = StartCoroutine(FlashAndFadeRoutine());
  }

  // Holds the damage colour briefly, then fades the overlay to transparent.
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

  // Applies an alpha value to the configured damage colour.
  void SetAlpha(float alpha)
  {
    Color c = damageColour;
    c.a = alpha;
    image.color = c;
  }
}
