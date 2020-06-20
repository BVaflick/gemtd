using UnityEngine;

[CreateAssetMenu]
public class Ability : ScriptableObject {

    [SerializeField, Range(1, 5)]
    protected int level;

    [SerializeField]
    protected Buff buff = default;

    public void Modify(Tower tower) {
        buff.level = level;
        tower.StatusEffects.Add(buff);
    }
}