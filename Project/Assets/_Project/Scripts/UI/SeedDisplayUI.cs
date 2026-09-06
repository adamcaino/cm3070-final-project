using UnityEngine;
using UnityEngine.UI;

// Displays the seed used by the currently generated dungeon.
public class SeedDisplayUI : MonoBehaviour
{
  const int FontSize = 20;

  Text seedText;

  // Creates and configures the seed label under this UI object.
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

  // Subscribes to dungeon readiness updates.
  void OnEnable()
  {
    DungeonReadySignal.Raised += HandleDungeonReady;
  }

  // Attempts an initial seed update after scene startup.
  void Start()
  {
    UpdateSeedText();
  }

  // Removes the dungeon readiness subscription.
  void OnDisable()
  {
    DungeonReadySignal.Raised -= HandleDungeonReady;
  }

  // Refreshes the label when generation completes.
  void HandleDungeonReady()
  {
    UpdateSeedText();
  }

  // Finds the generated grid and writes its last-used seed to the label.
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