using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
  AudioSource source;
  Coroutine crossfadeRoutine;

  void Awake()
  {
    source = GetComponent<AudioSource>();
    source.loop = true;
  }

  public void Play(AudioClip clip)
  {
    if (crossfadeRoutine != null)
    {
      StopCoroutine(crossfadeRoutine);
      crossfadeRoutine = null;
    }

    if (clip == null)
    {
      Stop();
      return;
    }

    if (source.clip == clip && source.isPlaying) return;

    source.clip = clip;
    source.Play();
  }

  public void PlayCrossfade(AudioClip clip, float duration)
  {
    if (crossfadeRoutine != null)
    {
      StopCoroutine(crossfadeRoutine);
      crossfadeRoutine = null;
    }

    crossfadeRoutine = StartCoroutine(Crossfade(clip, duration));
  }

  IEnumerator Crossfade(AudioClip clip, float duration)
  {
    if (duration <= 0f)
    {
      if (clip == null)
      {
        Stop();
      }
      else
      {
        source.clip = clip;
        source.Play();
      }

      crossfadeRoutine = null;
      yield break;
    }

    float startVolume = source.volume;
    float elapsed = 0f;

    while (elapsed < duration && source.isPlaying)
    {
      elapsed += Time.unscaledDeltaTime;
      source.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(elapsed / duration));
      yield return null;
    }

    source.volume = 0f;

    if (clip == null)
    {
      Stop();
      source.volume = startVolume;
      crossfadeRoutine = null;
      yield break;
    }

    source.clip = clip;
    source.Play();

    elapsed = 0f;
    while (elapsed < duration)
    {
      elapsed += Time.unscaledDeltaTime;
      source.volume = Mathf.Lerp(0f, startVolume, Mathf.Clamp01(elapsed / duration));
      yield return null;
    }

    source.volume = startVolume;
    crossfadeRoutine = null;
  }

  public void Stop()
  {
    source.Stop();
    source.clip = null;
  }
}
