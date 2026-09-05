using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// Master / Music / SFX volume sliders bound to the AudioMixer's exposed parameters, persisted via
// AudioMixerVolume/PlayerPrefs so they carry over between sessions.
public class SettingsPanelController : MonoBehaviour
{
  [SerializeField] MainMenuController mainMenu;
  [SerializeField] AudioMixer mixer;
  [SerializeField] Slider masterSlider;
  [SerializeField] Slider musicSlider;
  [SerializeField] Slider sfxSlider;
  [SerializeField] Slider sensitivitySlider;
  [SerializeField] Button backButton;

  void Awake()
  {
    EnsureSensitivitySlider();
    backButton.onClick.AddListener(OnBack);

    masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
    musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    if (sensitivitySlider != null)
    {
      sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }
  }

  void OnEnable()
  {
    masterSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.MasterParam));
    musicSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.MusicParam));
    sfxSlider.SetValueWithoutNotify(AudioMixerVolume.GetSaved(AudioMixerVolume.SFXParam));
    if (sensitivitySlider != null)
    {
      sensitivitySlider.SetValueWithoutNotify(PlayerCameraOrbit.GetSensitivity());
    }
  }

  void OnMasterVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.MasterParam, value);
  }

  void OnMusicVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.MusicParam, value);
  }

  void OnSFXVolumeChanged(float value)
  {
    AudioMixerVolume.Set(mixer, AudioMixerVolume.SFXParam, value);
  }

  void OnSensitivityChanged(float value)
  {
    PlayerCameraOrbit.SetSensitivity(value);
  }

  void EnsureSensitivitySlider()
  {
    if (sensitivitySlider != null)
    {
      return;
    }

    GameObject row = new GameObject("MouseSensitivitySlider", typeof(RectTransform), typeof(LayoutElement), typeof(VerticalLayoutGroup));
    row.transform.SetParent(transform, false);

    LayoutElement rowLayout = row.GetComponent<LayoutElement>();
    rowLayout.preferredHeight = 50f;
    rowLayout.preferredWidth = 320f;

    VerticalLayoutGroup rowGroup = row.GetComponent<VerticalLayoutGroup>();
    rowGroup.spacing = 4f;
    rowGroup.childForceExpandWidth = true;
    rowGroup.childForceExpandHeight = false;

    GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(LayoutElement), typeof(Text));
    labelObject.transform.SetParent(row.transform, false);
    labelObject.GetComponent<LayoutElement>().preferredHeight = 28f;
    Text label = labelObject.GetComponent<Text>();
    label.text = "Mouse Sensitivity";
    label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    label.fontSize = 18;
    label.alignment = TextAnchor.MiddleCenter;
    label.color = Color.white;

    GameObject sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(LayoutElement), typeof(Slider));
    sliderObject.transform.SetParent(row.transform, false);
    sliderObject.GetComponent<LayoutElement>().preferredHeight = 20f;

    GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
    background.transform.SetParent(sliderObject.transform, false);
    StretchFull(background.GetComponent<RectTransform>());
    background.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);

    GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
    fill.transform.SetParent(sliderObject.transform, false);
    StretchFull(fill.GetComponent<RectTransform>());
    fill.GetComponent<Image>().color = Color.white;

    GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
    handle.transform.SetParent(sliderObject.transform, false);
    RectTransform handleRect = handle.GetComponent<RectTransform>();
    handleRect.anchorMin = new Vector2(0f, 0.5f);
    handleRect.anchorMax = new Vector2(0f, 0.5f);
    handleRect.sizeDelta = new Vector2(16f, 24f);

    sensitivitySlider = sliderObject.GetComponent<Slider>();
    sensitivitySlider.fillRect = fill.GetComponent<RectTransform>();
    sensitivitySlider.handleRect = handleRect;
    sensitivitySlider.targetGraphic = handle.GetComponent<Image>();
    sensitivitySlider.minValue = 0.1f;
    sensitivitySlider.maxValue = 2f;
    sensitivitySlider.value = PlayerCameraOrbit.GetSensitivity();
  }

  static void StretchFull(RectTransform rect)
  {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.offsetMin = Vector2.zero;
    rect.offsetMax = Vector2.zero;
  }

  void OnBack()
  {
    mainMenu.ShowMain();
  }
}
