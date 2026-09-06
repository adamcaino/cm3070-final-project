using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Full-screen Image driven directly by alpha, same pattern as ScreenDamageFlash. Purely a fade
// primitive - callers decide when to fade and own the resting state (e.g. authored fully opaque
// in the Inspector so a scene stays hidden until something explicitly fades it in).
[RequireComponent(typeof(Image))]
// Controls a full-screen Image alpha and its raycast blocking state for scene transitions.
public class ScreenFader : MonoBehaviour
{
  [SerializeField] Color fadeColour = Color.black;

  Image image;
  Coroutine activeFade;

  // Caches the Image driven by the fade routines.
  void Awake()
  {
    image = GetComponent<Image>();
  }

  // Sets the fader to its opaque, input-blocking resting state.
  public void PrepareOpaque()
  {
    SetAlpha(1f);
    SetRaycastBlocking(true);
  }

  // Starts a fade to opaque and blocks UI raycasts during the fade.
  public Coroutine FadeOutAndStart(float duration)
  {
    return StartFade(FadeOut(duration));
  }

  // Starts a fade to transparent and releases UI raycasts when complete.
  public Coroutine FadeInAndStart(float duration)
  {
    return StartFade(FadeIn(duration));
  }

  // Fades from the current alpha to opaque.
  public IEnumerator FadeOut(float duration)
  {
    SetRaycastBlocking(true);
    yield return FadeAlpha(0f, 1f, duration);
  }

  // Fades from opaque to transparent.
  public IEnumerator FadeIn(float duration)
  {
    SetAlpha(1f);
    yield return FadeAlpha(1f, 0f, duration);
    SetRaycastBlocking(false);
  }

  // Stops any previous fade and starts the supplied fade routine.
  Coroutine StartFade(IEnumerator routine)
  {
    if (activeFade != null)
    {
      StopCoroutine(activeFade);
    }

    activeFade = StartCoroutine(routine);
    return activeFade;
  }

  // Interpolates image alpha over the requested duration.
  IEnumerator FadeAlpha(float from, float to, float duration)
  {
    float elapsed = 0f;
    while (elapsed < duration)
    {
      elapsed += Time.deltaTime;
      float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
      SetAlpha(Mathf.Lerp(from, to, t));
      yield return null;
    }

    SetAlpha(to);
    activeFade = null;
  }

  // Applies an alpha value while preserving the configured fade colour.
  void SetAlpha(float alpha)
  {
    Color c = fadeColour;
    c.a = alpha;
    image.color = c;
  }

  // Controls whether the fade Image intercepts UI pointer events.
  void SetRaycastBlocking(bool blocking)
  {
    image.raycastTarget = blocking;
  }
}
