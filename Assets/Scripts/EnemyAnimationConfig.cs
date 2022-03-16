using UnityEngine;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject {
    
    [SerializeField]
    AnimationClip move = default, intro = default, outro = default;
    
    [SerializeField]
    float moveAnimationSpeed = 1f;
    
    public float MoveAnimationSpeed => moveAnimationSpeed;

    public AnimationClip Move => move;
    
    public AnimationClip Intro => intro;

    public AnimationClip Outro => outro;
    
}