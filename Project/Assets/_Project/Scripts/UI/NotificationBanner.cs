using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Displays a short fading text notification from the persistent UI canvas (e.g. "Health Full").
public class NotificationBanner : MonoBehaviour
{
  const int FontSize = 28;
  const float DisplayDuration = 1.2f;
  const float FadeDuration = 0.4f;

  static NotificationBanner instance;

  Text bannerText;
  CanvasGroup canvasGroup;
  Coroutine activeNotification;

  // Creates the notification label under this canvas and keeps it hidden at startup.
  void Awake()
  {
    instance = this;

    GameObject bannerObject = new GameObject("NotificationBannerText", typeof(RectTransform), typeof(CanvasGroup), typeof(Text));
    bannerObject.transform.SetParent(transform, false);

    RectTransform bannerTransform = bannerObject.GetComponent<RectTransform>();
    bannerTransform.anchorMin = new Vector2(0.5f, 0.5f);
    bannerTransform.anchorMax = new Vector2(0.5f, 0.5f);
    bannerTransform.pivot = new Vector2(0.5f, 0.5f);
    bannerTransform.anchoredPosition = new Vector2(0f, 160f);
    bannerTransform.sizeDelta = new Vector2(600f, 60f);

    canvasGroup = bannerObject.GetComponent<CanvasGroup>();
    canvasGroup.alpha = 0f;
    canvasGroup.blocksRaycasts = false;
    canvasGroup.interactable = false;

    bannerText = bannerObject.GetComponent<Text>();
    bannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    bannerText.fontSize = FontSize;
    bannerText.alignment = TextAnchor.MiddleCenter;
    bannerText.raycastTarget = false;
    bannerText.text = string.Empty;
  }

  // Clears the singleton reference when the persistent UI canvas is torn down.
  void OnDestroy()
  {
    if (instance == this) instance = null;
  }

  // Shows a message on the persistent UI notification banner, restarting the fade if one is already active.
  public static void Show(string message, Color color)
  {
    if (instance == null)
    {
      Debug.LogWarning($"NotificationBanner not found in the scene. Add the component to the UI scene canvas to show '{message}'.");
      return;
    }

    instance.DisplayMessage(message, color);
  }

  // Applies the message and (re)starts the show-then-fade routine.
  void DisplayMessage(string message, Color color)
  {
    bannerText.text = message;
    bannerText.color = color;

    if (activeNotification != null)
    {
      StopCoroutine(activeNotification);
    }

    activeNotification = StartCoroutine(ShowAndFadeRoutine());
  }

  // Holds the message fully visible, then fades it out over FadeDuration.
  IEnumerator ShowAndFadeRoutine()
  {
    canvasGroup.alpha = 1f;
    yield return new WaitForSeconds(DisplayDuration);

    float elapsed = 0f;
    while (elapsed < FadeDuration)
    {
      elapsed += Time.deltaTime;
      canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / FadeDuration);
      yield return null;
    }

    canvasGroup.alpha = 0f;
    activeNotification = null;
  }
}
