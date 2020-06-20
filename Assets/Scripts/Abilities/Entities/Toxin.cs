using UnityEngine;

public class Toxin : WarEntity {

    static int colorPropertyID = Shader.PropertyToID("_Color");

    static MaterialPropertyBlock propertyBlock;

    [SerializeField]
    AnimationCurve opacityCurve = default;

    [SerializeField]
    AnimationCurve scaleCurve = default;

    [SerializeField, Range(0f, 10f)]
    float duration = 6f;    

    float scale = 1f;

    float cooldown = 0f;

    MeshRenderer meshRenderer;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(meshRenderer != null, "Explosion without renderer!");
    }

    public override bool GameUpdate() {
        cooldown += Time.deltaTime;
        age += Time.deltaTime;
        if (age >= duration || target == null) {
            OriginFactory.Reclaim(this);
            return false;
        }

        if (cooldown >= 1f) {
            cooldown = 0f;
            target.Enemy.ApplyDamage(5f, false);
        }

        if (propertyBlock == null) {
            propertyBlock = new MaterialPropertyBlock();
        }
        transform.localPosition = target.Position;
        float t = cooldown;
        Color c = Color.clear;
        c.a = opacityCurve.Evaluate(t);
        propertyBlock.SetColor(colorPropertyID, c);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));
        return true;
    }
}