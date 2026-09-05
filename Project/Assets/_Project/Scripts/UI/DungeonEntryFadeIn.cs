using UnityEngine;
using UnityEngine.Audio;

// Lives in the UI scene next to the fade Image (authored fully opaque at rest, so a freshly loaded
// Dungeon stays hidden while it generates). Fades the screen in once DungeonReadySignal fires,
// rather than on scene Start, so the player never sees the level being built mid-fade.
[RequireComponent(typeof(ScreenFader))]
public class DungeonEntryFadeIn : MonoBehaviour
{
  [SerializeField] AudioMixer mixer;
  [SerializeField, Min(0f)] float fadeInDuration = 1.5f;

  ScreenFader fader;
  bool hasStartedFadeIn;

  void Awake()
  {
    fader = GetComponent<ScreenFader>();
    AudioMixerVolume.ApplySaved(mixer);
    AudioMixerVolume.SetRuntime(mixer, AudioMixerVolume.MasterParam, 0f);
  }

  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;

    if (DungeonReadySignal.IsReady)
    {
      HandleDungeonReady();
    }
  }

  void Start()
  {
    if (DungeonReadySignal.IsReady)
    {
      HandleDungeonReady();
    }
  }

  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  void HandleDungeonReady()
  {
    if (hasStartedFadeIn)
    {
      return;
    }

    hasStartedFadeIn = true;
    StartCoroutine(FadeInAudioAndScreen());
  }

  System.Collections.IEnumerator FadeInAudioAndScreen()
  {
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
