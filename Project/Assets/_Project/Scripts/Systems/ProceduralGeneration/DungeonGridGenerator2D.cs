using System;
using UnityEngine;

public abstract class DungeonGridGenerator2D : MonoBehaviour
{
  [Header("Grid Size")]
  [SerializeField, Min(16)] int gridWidth = 64;
  [SerializeField, Min(16)] int gridHeight = 64;

  [Header("Reproducibility")]
  [SerializeField] bool generateOnStart;
  [SerializeField] bool randomizeSeed = true;
  [SerializeField] int fixedSeed = 3070;

  [Header("2D Visualisation")]
  [SerializeField, Min(0.1f)] float tileSize = 0.5f;
  [SerializeField] Color floorColor = new Color(0.5f, 0.5f, 0.5f, 1f);
  [SerializeField] Color wallColor = Color.black;
  [SerializeField] Color doorColor = Color.red;

  [Header("Placement")]
  [Tooltip("Local offset the generated 2D map is parented at, away from wherever the 3D level ends up - keeps this generator's own output usable later as a standalone minimap without overlapping the 3D level.")]
  [SerializeField] Vector3 mapOffset = new Vector3(0f, 0f, 500f);

  Transform generatedRoot;
  static Sprite cachedTileSprite;
  bool hasGeneratedAtLeastOnce;

  protected int GridWidth => gridWidth;
  protected int GridHeight => gridHeight;
  protected int FixedSeed => fixedSeed;
  public bool HasGenerated => hasGeneratedAtLeastOnce;
  public int LastUsedSeed { get; private set; }
  public TileMetadata[,] LastMetadata { get; private set; }

  protected virtual void Start()
  {
    if (generateOnStart)
    {
      Generate();
    }
  }

  protected virtual void OnValidate()
  {
    gridWidth = Mathf.Max(16, gridWidth);
    gridHeight = Mathf.Max(16, gridHeight);
    tileSize = Mathf.Max(0.1f, tileSize);
  }

  [ContextMenu("Generate")]
  public void Generate()
  {
    int seed = randomizeSeed ? Environment.TickCount : fixedSeed;
    Generate(seed);
  }

  public void Generate(int seed)
  {
    Clear();

    LastUsedSeed = seed;
    fixedSeed = seed;
    hasGeneratedAtLeastOnce = true;

    TileType[,] tileMap = BuildMap(seed);
    LastMetadata = DungeonMetadataBuilder.Build(tileMap);
    RenderTileMap(LastMetadata);

    Debug.Log($"{GetType().Name} generated with seed {seed}.");
  }

  [ContextMenu("Regenerate Using Last Seed")]
  public void RegenerateUsingLastSeed()
  {
    int seed = hasGeneratedAtLeastOnce ? LastUsedSeed : fixedSeed;
    Generate(seed);
  }

  [ContextMenu("Clear")]
  public void Clear()
  {
    if (generatedRoot == null)
    {
      Transform existingRoot = transform.Find("Generated Dungeon");
      if (existingRoot != null)
      {
        generatedRoot = existingRoot;
      }
    }

    if (generatedRoot == null)
    {
      return;
    }

    for (int i = generatedRoot.childCount - 1; i >= 0; i--)
    {
      GameObject child = generatedRoot.GetChild(i).gameObject;

#if UNITY_EDITOR
      if (!Application.isPlaying)
      {
        DestroyImmediate(child);
        continue;
      }
#endif

      Destroy(child);
    }
  }

  protected abstract TileType[,] BuildMap(int seed);

  protected TileType[,] CreateFilledMap(TileType fillValue)
  {
    TileType[,] map = new TileType[gridWidth, gridHeight];

    for (int x = 0; x < gridWidth; x++)
    {
      for (int y = 0; y < gridHeight; y++)
      {
        map[x, y] = fillValue;
      }
    }

    return map;
  }

  protected void SetTile(TileType[,] map, int x, int y, TileType tileType)
  {
    if (!IsInsideMap(x, y))
    {
      return;
    }

    map[x, y] = tileType;
  }

  protected TileType GetTile(TileType[,] map, int x, int y)
  {
    if (!IsInsideMap(x, y))
    {
      return TileType.Wall;
    }

    return map[x, y];
  }

  protected bool IsInsideMap(int x, int y)
  {
    return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
  }

  void EnsureGeneratedRoot()
  {
    if (generatedRoot != null)
    {
      return;
    }

    Transform existingRoot = transform.Find("Generated Dungeon");
    if (existingRoot != null)
    {
      generatedRoot = existingRoot;
      return;
    }

    GameObject root = new GameObject("Generated Dungeon");
    root.transform.SetParent(transform, false);
    root.transform.localPosition = mapOffset;
    generatedRoot = root.transform;
  }

  void RenderTileMap(TileMetadata[,] tileMap)
  {
    EnsureGeneratedRoot();

    float xOffset = (gridWidth - 1) * tileSize * 0.5f;
    float yOffset = (gridHeight - 1) * tileSize * 0.5f;

    for (int x = 0; x < gridWidth; x++)
    {
      for (int y = 0; y < gridHeight; y++)
      {
        TileType tile = tileMap[x, y].Type;
        if (tile == TileType.Empty)
        {
          continue;
        }

        GameObject tileObject = new GameObject($"{tile} [{x},{y}]");
        tileObject.transform.SetParent(generatedRoot, false);
        tileObject.transform.localPosition = new Vector3((x * tileSize) - xOffset, (y * tileSize) - yOffset, 0f);
        tileObject.transform.localScale = Vector3.one * tileSize;

        SpriteRenderer spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetOrCreateTileSprite();
        spriteRenderer.color = GetColor(tile);
        spriteRenderer.sortingOrder = GetSortingOrder(tile);
      }
    }
  }

  Color GetColor(TileType tile)
  {
    switch (tile)
    {
      case TileType.Floor:
        return floorColor;
      case TileType.Wall:
        return wallColor;
      case TileType.Door:
        return doorColor;
      default:
        return Color.clear;
    }
  }

  int GetSortingOrder(TileType tile)
  {
    switch (tile)
    {
      case TileType.Wall:
        return 10;
      case TileType.Door:
        return 20;
      default:
        return 0;
    }
  }

  static Sprite GetOrCreateTileSprite()
  {
    if (cachedTileSprite != null)
    {
      return cachedTileSprite;
    }

    Texture2D texture = new Texture2D(1, 1);
    texture.filterMode = FilterMode.Point;
    texture.wrapMode = TextureWrapMode.Clamp;
    texture.SetPixel(0, 0, Color.white);
    texture.Apply();

    cachedTileSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    cachedTileSprite.name = "ProceduralDungeonTile";
    return cachedTileSprite;
  }
}
