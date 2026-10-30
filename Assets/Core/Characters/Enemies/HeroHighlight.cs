using UnityEngine;

public class HeroHighlight : MonoBehaviour
{
    [SerializeField] float radius = 1f;      // à ajuster selon la taille de tes tuiles
    [SerializeField] float height = 0.05f;     // légèrement au-dessus du sol
    [SerializeField] Color color = new Color(0.2f, 1f, 0.3f, 1f);
    [SerializeField] bool tintModel = true;
    [SerializeField, Range(0f, 1f)] float tintStrength = 0.35f;

    LineRenderer ring;
    Renderer[] renderers;
    MaterialPropertyBlock block;
    bool active;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
        BuildRing();
        SetHighlighted(false);
    }

    void BuildRing()
    {
        var go = new GameObject("TurnRing");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0, height, 0);

        ring = go.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.widthMultiplier = 0.08f;
        ring.material = new Material(Shader.Find("Sprites/Default"));
        ring.startColor = ring.endColor = color;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        const int seg = 48;
        ring.positionCount = seg;
        for (int i = 0; i < seg; i++)
        {
            float a = i * Mathf.PI * 2f / seg;
            ring.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius));
        }
    }

    public void SetHighlighted(bool on)
    {
        active = on;
        ring.enabled = on;

        if (!tintModel) return;
        foreach (var r in renderers)
        {
            if (r == ring) continue;
            if (!on) { r.SetPropertyBlock(null); continue; }

            r.GetPropertyBlock(block);
            Color baseCol = r.sharedMaterial.HasProperty("_BaseColor") ? r.sharedMaterial.GetColor("_BaseColor")
                          : r.sharedMaterial.HasProperty("_Color") ? r.sharedMaterial.GetColor("_Color")
                          : Color.white;
            Color tinted = Color.Lerp(baseCol, color, tintStrength);
            block.SetColor("_BaseColor", tinted); // URP
            block.SetColor("_Color", tinted);     // Built-in
            r.SetPropertyBlock(block);
        }
    }

    void Update()
    {
        if (!active) return;
        // petite pulsation pour mieux le repérer
        float k = 0.75f + 0.25f * Mathf.Sin(Time.time * 4f);
        var c = new Color(color.r, color.g, color.b, k);
        ring.startColor = ring.endColor = c;
    }
}