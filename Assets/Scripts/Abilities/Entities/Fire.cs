using UnityEngine;

public class Fire : WarEntity {

    static int colorPropertyID = Shader.PropertyToID("_Color");

    static MaterialPropertyBlock propertyBlock;

    [SerializeField]
    AnimationCurve opacityCurve = default;

    [SerializeField]
    AnimationCurve scaleCurve = default;

    [SerializeField, Range(0f, 1f)]
    float duration = 0.3f;

    float age;

    MeshRenderer meshRenderer;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(meshRenderer != null, "Explosion without renderer!");
    } 

    public override bool GameUpdate() {
        if (target != null) {
            transform.localPosition = target.Position;
        }
        age += Time.deltaTime;
        if (age >= duration) {
            OriginFactory.Reclaim(this);
            return false;
        }

        if (propertyBlock == null) {
            propertyBlock = new MaterialPropertyBlock();
        }
        float t = age / duration;
        Color c = Color.clear;
        c.a = opacityCurve.Evaluate(t);
        propertyBlock.SetColor(colorPropertyID, c);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));
        return true;
    }
}