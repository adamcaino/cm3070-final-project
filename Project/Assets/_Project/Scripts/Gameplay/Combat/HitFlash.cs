using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Flashes a solid colour over its renderers then fades back to the original texture/tint.
/// Health is optional - when present it auto-triggers on damage/heal, but any script can also
/// call Flash(colour) directly (e.g. a shield reacting to a block with no Health of its own).
/// </summary>
public class HitFlash : MonoBehaviour
{
  static readonly int BaseColourId = Shader.PropertyToID("_BaseColor");
  static readonly int ColorId = Shader.PropertyToID("_Color");
  static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
  static readonly int MainTexId = Shader.PropertyToID("_MainTex");

  [SerializeField] Color damageColour = Color.red;
  [SerializeField] Color healColour = Color.green;
  [SerializeField, Min(0f)] float holdDuration = 0.06f;
  [SerializeField, Min(0f)] float flashDuration = 0.15f;

  Health health;
  Renderer[] renderers;
  MaterialPropertyBlock propertyBlock;
  Coroutine activeFlash;
  Dictionary<Color, Texture2D> flashTextures;

  void Awake()
  {
    health = GetComponent<Health>();
    renderers = GetComponentsInChildren<Renderer>();
    propertyBlock = new MaterialPropertyBlock();
    flashTextures = new Dictionary<Color, Texture2D>();
  }

  void OnDestroy()
  {
    foreach (Texture2D texture in flashTextures.Values)
    {
      Destroy(texture);
    }
  }

  void OnEnable()
  {
    if (health == null) return;

    health.OnDamaged += HandleDamaged;
    health.OnHealed += HandleHealed;
  }

  void OnDisable()
  {
    if (health == null) return;

    health.OnDamaged -= HandleDamaged;
    health.OnHealed -= HandleHealed;
  }

  void HandleDamaged(Vector3 hitPoint) => Flash(damageColour);

  void HandleHealed(Vector3 healPoint) => Flash(healColour);

  public void Flash(Color colour)
  {
    if (activeFlash != null)
    {
      StopCoroutine(activeFlash);
    }

    activeFlash = StartCoroutine(FlashAndFadeRoutine(colour, GetOrCreateTexture(colour)));
  }

  Texture2D GetOrCreateTexture(Color colour)
  {
    if (!flashTextures.TryGetValue(colour, out Texture2D texture))
    {
      texture = CreateSolidTexture(colour);
      flashTextures[colour] = texture;
    }

    return texture;
  }

  static Texture2D CreateSolidTexture(Color colour)
  {
    Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
    {
      hideFlags = HideFlags.HideAndDontSave
    };
    texture.SetPixel(0, 0, colour);
    texture.Apply();
    return texture;
  }

  IEnumerator FlashAndFadeRoutine(Color flashColour, Texture2D flashTexture)
  {
    Color[] originalColours = new Color[renderers.Length];
    Texture[] originalTextures = new Texture[renderers.Length];

    for (int i = 0; i < renderers.Length; i++)
    {
      Material material = renderers[i].sharedMaterial;
      int colourId = ColourPropertyId(material);
      int textureId = TexturePropertyId(material);
      originalColours[i] = material != null && material.HasProperty(colourId) ? material.GetColor(colourId) : Color.white;
      originalTextures[i] = material != null && material.HasProperty(textureId) ? material.GetTexture(textureId) : null;
    }

    // Solid flat swap - overrides the base map itself so the hit reads as a full colour flash
    // rather than a tint of the existing texture detail, then reverts to a tint-based fade below.
    for (int i = 0; i < renderers.Length; i++)
    {
      Renderer renderer = renderers[i];
      Material material = renderer.sharedMaterial;
      renderer.GetPropertyBlock(propertyBlock);
      propertyBlock.SetColor(ColourPropertyId(material), Color.white);
      propertyBlock.SetTexture(TexturePropertyId(material), flashTexture);
      renderer.SetPropertyBlock(propertyBlock);
    }

    if (holdDuration > 0f)
    {
      yield return new WaitForSeconds(holdDuration);
    }

    for (int i = 0; i < renderers.Length; i++)
    {
      Renderer renderer = renderers[i];
      Material material = renderer.sharedMaterial;
      renderer.GetPropertyBlock(propertyBlock);
      propertyBlock.SetTexture(TexturePropertyId(material), originalTextures[i] != null ? originalTextures[i] : Texture2D.whiteTexture);
      renderer.SetPropertyBlock(propertyBlock);
    }

    float elapsed = 0f;
    while (elapsed < flashDuration)
    {
      elapsed += Time.deltaTime;
      float t = flashDuration > 0f ? Mathf.Clamp01(elapsed / flashDuration) : 1f;

      for (int i = 0; i < renderers.Length; i++)
      {
        Renderer renderer = renderers[i];
        Material material = renderer.sharedMaterial;
        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(ColourPropertyId(material), Color.Lerp(flashColour, originalColours[i], t));
        renderer.SetPropertyBlock(propertyBlock);
      }

      yield return null;
    }

    foreach (Renderer r in renderers)
    {
      r.SetPropertyBlock(null);
    }

    activeFlash = null;
  }

  static int ColourPropertyId(Material material)
  {
    return material != null && material.HasProperty(BaseColourId) ? BaseColourId : ColorId;
  }

  static int TexturePropertyId(Material material)
  {
    return material != null && material.HasProperty(BaseMapId) ? BaseMapId : MainTexId;
  }
}
