using System;
using UnityEngine;

// Defines the shared seeded grid lifecycle used by 2D dungeon generators.
// Subclasses provide the tile algorithm; this class builds metadata and renders the result.
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

  // Optionally runs the initial generation pass when the scene starts.
  protected virtual void Start()
  {
    if (generateOnStart)
    {
      Generate();
    }
  }

  // Keeps grid and visualisation values within the minimum ranges required by generation.
  protected virtual void OnValidate()
  {
    gridWidth = Mathf.Max(16, gridWidth);
    gridHeight = Mathf.Max(16, gridHeight);
    tileSize = Mathf.Max(0.1f, tileSize);
  }

  [ContextMenu("Generate")]
  // Selects either a fresh or fixed seed and starts a generation pass.
  public void Generate()
  {
    int seed = randomizeSeed ? Environment.TickCount : fixedSeed;
    Generate(seed);
  }

  // Clears previous output, builds the tile map, derives metadata, and renders the 2D result.
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
  // Rebuilds the map with the last generated seed, or the configured fixed seed if none exists.
  public void RegenerateUsingLastSeed()
  {
    int seed = hasGeneratedAtLeastOnce ? LastUsedSeed : fixedSeed;
    Generate(seed);
  }

  [ContextMenu("Clear")]
  // Removes generated tile objects while preserving the generator component and settings.
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

  // Produces the generator-specific tile map for the supplied seed.
  protected abstract TileType[,] BuildMap(int seed);

  // Creates a grid initialized to one tile type for use as a generator's working map.
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

  // Assigns a tile only when the coordinate lies inside the configured grid.
  protected void SetTile(TileType[,] map, int x, int y, TileType tileType)
  {
    if (!IsInsideMap(x, y))
    {
      return;
    }

    map[x, y] = tileType;
  }

  // Reads a tile and treats out-of-bounds coordinates as walls.
  protected TileType GetTile(TileType[,] map, int x, int y)
  {
    if (!IsInsideMap(x, y))
    {
      return TileType.Wall;
    }

    return map[x, y];
  }

  // Tests whether a coordinate belongs to the configured grid.
  protected bool IsInsideMap(int x, int y)
  {
    return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
  }

  // Finds or creates the transform that owns generated 2D tile objects.
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

  // Converts non-empty metadata cells into positioned and coloured SpriteRenderer objects.
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

  // Maps a tile type to its configured visualisation colour.
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

  // Selects the draw order that keeps walls and doors visible over floor tiles.
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

  // Creates the shared one-pixel sprite used to render every generated tile.
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
