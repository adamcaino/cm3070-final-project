using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Full-screen Image driven directly by alpha, same pattern as ScreenDamageFlash. Purely a fade
// primitive - callers decide when to fade and own the resting state (e.g. authored fully opaque
// in the Inspector so a scene stays hidden until something explicitly fades it in).
[RequireComponent(typeof(Image))]
public class ScreenFader : MonoBehaviour
{
  [SerializeField] Color fadeColour = Color.black;

  Image image;
  Coroutine activeFade;

  void Awake()
  {
    image = GetComponent<Image>();
  }

  public Coroutine FadeOutAndStart(float duration)
  {
    return StartFade(FadeOut(duration));
  }

  public Coroutine FadeInAndStart(float duration)
  {
    return StartFade(FadeIn(duration));
  }

  public IEnumerator FadeOut(float duration)
  {
    SetRaycastBlocking(true);
    yield return FadeAlpha(0f, 1f, duration);
  }

  public IEnumerator FadeIn(float duration)
  {
    SetAlpha(1f);
    yield return FadeAlpha(1f, 0f, duration);
    SetRaycastBlocking(false);
  }

  Coroutine StartFade(IEnumerator routine)
  {
    if (activeFade != null)
    {
      StopCoroutine(activeFade);
    }

    activeFade = StartCoroutine(routine);
    return activeFade;
  }

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

  void SetAlpha(float alpha)
  {
    Color c = fadeColour;
    c.a = alpha;
    image.color = c;
  }

  void SetRaycastBlocking(bool blocking)
  {
    image.raycastTarget = blocking;
  }
}
