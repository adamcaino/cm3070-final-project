using UnityEngine;
using UnityEngine.Audio;

// Converts linear 0..1 UI slider values to the decibel scale AudioMixer exposed parameters use,
// and persists them to PlayerPrefs under the same key as the exposed parameter name.
public static class AudioMixerVolume
{
  public const string MasterParam = "MasterVolume";
  public const string MusicParam = "MusicVolume";
  public const string SFXParam = "SFXVolume";

  const float DefaultLinearVolume = 0.75f;
  const float MinLinearVolume = 0.0001f;

  // Applies all persisted mixer parameter values to the supplied mixer.
  public static void ApplySaved(AudioMixer mixer)
  {
    if (mixer == null) return;

    Apply(mixer, MasterParam);
    Apply(mixer, MusicParam);
    Apply(mixer, SFXParam);
  }

  // Applies and persists one linear volume setting.
  public static void Set(AudioMixer mixer, string exposedParam, float linear01)
  {
    SetRuntime(mixer, exposedParam, linear01);

    PlayerPrefs.SetFloat(exposedParam, linear01);
    PlayerPrefs.Save();
  }

  // Applies one linear volume setting without writing PlayerPrefs.
  public static void SetRuntime(AudioMixer mixer, string exposedParam, float linear01)
  {
    if (mixer != null)
    {
      mixer.SetFloat(exposedParam, LinearToDecibel(linear01));
    }
  }

  // Returns the saved linear volume or the default volume when no value exists.
  public static float GetSaved(string exposedParam)
  {
    return PlayerPrefs.GetFloat(exposedParam, DefaultLinearVolume);
  }

  // Applies one persisted parameter to the mixer.
  static void Apply(AudioMixer mixer, string exposedParam)
  {
    mixer.SetFloat(exposedParam, LinearToDecibel(GetSaved(exposedParam)));
  }

  // Converts a linear slider value into the mixer decibel scale.
  static float LinearToDecibel(float linear01)
  {
    return Mathf.Log10(Mathf.Max(linear01, MinLinearVolume)) * 20f;
  }
}
