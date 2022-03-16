using UnityEngine;
using UnityEditor;

public class Ice : WarEntity {

    static int colorPropertyID = Shader.PropertyToID("_Color");

    static MaterialPropertyBlock propertyBlock;

    [SerializeField]
    AnimationCurve opacityCurve = default;

    [SerializeField]
    AnimationCurve scaleCurve = default;

    [SerializeField, Range(0f, 10f)]
    float duration = 6f;

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
        target.Enemy.additionalSpeed -= 1f;   
        if (cooldown >= 1f) {
            cooldown = 0f;
        }
        if (propertyBlock == null) {
            propertyBlock = new MaterialPropertyBlock();
        }
        transform.localPosition = new Vector3(target.Position.x, 0f, target.Position.z);
        float t = cooldown;
        Color c = Color.clear;
        c.a = opacityCurve.Evaluate(t);
        propertyBlock.SetColor(colorPropertyID, c);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));
        return true;
    }
}