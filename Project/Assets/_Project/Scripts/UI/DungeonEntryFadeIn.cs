using UnityEngine;
using UnityEngine.Audio;

// Lives in the UI scene next to the fade Image (authored fully opaque at rest, so a freshly loaded
// Dungeon stays hidden while it generates). Fades the screen in once DungeonReadySignal fires,
// rather than on scene Start, so the player never sees the level being built mid-fade.
[RequireComponent(typeof(ScreenFader))]
// Reveals the generated dungeon and restores master volume after generation completes.
public class DungeonEntryFadeIn : MonoBehaviour
{
  [SerializeField] AudioMixer mixer;
  [SerializeField, Min(0f)] float fadeInDuration = 1.5f;

  ScreenFader fader;
  bool hasStartedFadeIn;

  // Caches the fader and starts the mixer at silence while generation runs.
  void Awake()
  {
    fader = GetComponent<ScreenFader>();
    fader.PrepareOpaque();
    AudioMixerVolume.ApplySaved(mixer);
    AudioMixerVolume.SetRuntime(mixer, AudioMixerVolume.MasterParam, 0f);
  }

  // Subscribes to readiness and handles a signal that arrived before this component enabled.
  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;

    if (DungeonReadySignal.IsReady)
    {
      HandleDungeonReady();
    }
  }

  // Handles readiness that was raised during scene activation.
  void Start()
  {
    if (DungeonReadySignal.IsReady)
    {
      HandleDungeonReady();
    }
  }

  // Removes the dungeon readiness subscription.
  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  // Starts the reveal once, after the generation pipeline reports readiness.
  void HandleDungeonReady()
  {
    if (hasStartedFadeIn)
    {
      return;
    }

    hasStartedFadeIn = true;
    StartCoroutine(FadeInAudioAndScreen());
  }

  // Fades the screen and master volume in together using the saved volume target.
  System.Collections.IEnumerator FadeInAudioAndScreen()
  {
    yield return null;

    Coroutine screenFade = fader.FadeInAndStart(fadeInDuration);
    float elapsed = 0f;
    float targetVolume = AudioMixerVolume.GetSaved(AudioMixerVolume.MasterParam);

    while (elapsed < fadeInDuration)
    {
      elapsed += Time.deltaTime;
      float t = fadeInDuration > 0f ? Mathf.Clamp01(elapsed / fadeInDuration) : 1f;
      AudioMixerVolume.SetRuntime(mixer, AudioMixerVolume.MasterParam, Mathf.Lerp(0f, targetVolume, t));
      yield return null;
    }

    AudioMixerVolume.SetRuntime(mixer, AudioMixerVolume.MasterParam, targetVolume);
    yield return screenFade;
  }
}
