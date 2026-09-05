using UnityEngine;
using UnityEngine.UI;

public class SeedDisplayUI : MonoBehaviour
{
  const int FontSize = 20;

  Text seedText;

  void Awake()
  {
    GameObject labelObject = new GameObject("SeedLabel", typeof(RectTransform), typeof(Text));
    labelObject.transform.SetParent(transform, false);

    RectTransform labelTransform = labelObject.GetComponent<RectTransform>();
    labelTransform.anchorMin = new Vector2(0f, 1f);
    labelTransform.anchorMax = new Vector2(0f, 1f);
    labelTransform.pivot = new Vector2(0f, 1f);
    labelTransform.anchoredPosition = new Vector2(24f, -24f);
    labelTransform.sizeDelta = new Vector2(280f, 32f);

    seedText = labelObject.GetComponent<Text>();
    seedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    seedText.fontSize = FontSize;
    seedText.color = Color.white;
    seedText.alignment = TextAnchor.UpperLeft;
    seedText.raycastTarget = false;
    seedText.text = string.Empty;
  }

  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
  }

  void Start()
  {
    UpdateSeedText();
  }

  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  void HandleDungeonReady()
  {
    UpdateSeedText();
  }

  void UpdateSeedText()
  {
    DungeonGridGenerator2D generator = FindFirstObjectByType<DungeonGridGenerator2D>();
    if (generator == null || !generator.HasGenerated)
    {
      return;
    }

    seedText.text = $"Seed: {generator.LastUsedSeed}";
  }
}