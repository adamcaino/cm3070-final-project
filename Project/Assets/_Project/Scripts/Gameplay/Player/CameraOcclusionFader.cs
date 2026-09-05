using System.Collections.Generic;
using UnityEngine;

public class CameraOcclusionFader : MonoBehaviour
{
  [Header("Target")]
  [SerializeField] Transform target;

  [Header("Occlusion")]
  [SerializeField] LayerMask occluderMask;
  [Tooltip("Radius of the cast towards the target, so occluders clipping the target's edges (not just dead-centre) still trigger a fade. Roughly half the target's width.")]
  [SerializeField, Min(0f)] float castRadius = 0.375f;
  [SerializeField] Material occlusionMaterial;
  [SerializeField, Range(0f, 1f)] float fadedAlpha = 0.35f;
  [SerializeField, Min(0f)] float fadeSpeed = 8f;
  [SerializeField] string colorProperty = "_BaseColor";

  int colorPropertyId;

  class FadeState
  {
    public Material[] originalMaterials;
    public float alpha;
  }

  readonly Dictionary<Renderer, FadeState> states = new();
  readonly HashSet<Renderer> hitThisFrame = new();
  readonly RaycastHit[] hitBuffer = new RaycastHit[8];

  void Awake()
  {
    colorPropertyId = Shader.PropertyToID(colorProperty);
  }

  void LateUpdate()
  {
    if (target == null || occlusionMaterial == null)
    {
      return;
    }

    hitThisFrame.Clear();

    Vector3 origin = transform.position;
    Vector3 toTarget = target.position - origin;
    float distance = toTarget.magnitude;

    if (distance > 0.01f)
    {
      int hitCount = Physics.SphereCastNonAlloc(origin, castRadius, toTarget / distance, hitBuffer, distance, occluderMask, QueryTriggerInteraction.Ignore);
      for (int i = 0; i < hitCount; i++)
      {
        Renderer hitRenderer = hitBuffer[i].collider.GetComponentInChildren<Renderer>();
        if (hitRenderer != null)
        {
          hitThisFrame.Add(hitRenderer);
        }
      }
    }

    foreach (Renderer occluder in hitThisFrame)
    {
      BeginFade(occluder);
      UpdateFade(occluder, fadedAlpha);
    }

    RestoreRenderersNoLongerHit();
  }

  void RestoreRenderersNoLongerHit()
  {
    List<Renderer> finished = null;

    foreach (KeyValuePair<Renderer, FadeState> entry in states)
    {
      Renderer renderer = entry.Key;
      if (hitThisFrame.Contains(renderer))
      {
        continue;
      }

      float alpha = UpdateFade(renderer, 1f);
      if (alpha >= 0.999f)
      {
        EndFade(renderer);
        (finished ??= new List<Renderer>()).Add(renderer);
      }
    }

    if (finished == null)
    {
      return;
    }

    foreach (Renderer renderer in finished)
    {
      states.Remove(renderer);
    }
  }

  void BeginFade(Renderer renderer)
  {
    if (states.ContainsKey(renderer))
    {
      return;
    }

    Material[] originals = renderer.sharedMaterials;
    Material[] swapped = new Material[originals.Length];
    for (int i = 0; i < swapped.Length; i++)
    {
      swapped[i] = occlusionMaterial;
    }

    renderer.materials = swapped;
    states[renderer] = new FadeState { originalMaterials = originals, alpha = 1f };
  }

  float UpdateFade(Renderer renderer, float targetAlpha)
  {
    FadeState state = states[renderer];
    state.alpha = Mathf.MoveTowards(state.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

    MaterialPropertyBlock block = new();
    renderer.GetPropertyBlock(block);
    Color color = occlusionMaterial.HasProperty(colorPropertyId) ? occlusionMaterial.GetColor(colorPropertyId) : Color.white;
    color.a = state.alpha;
    block.SetColor(colorPropertyId, color);
    renderer.SetPropertyBlock(block);

    return state.alpha;
  }

  void EndFade(Renderer renderer)
  {
    FadeState state = states[renderer];
    renderer.materials = state.originalMaterials;
    renderer.SetPropertyBlock(null);
  }
}
