using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Ability : ScriptableObject {

    [SerializeField, Range(1, 5)]
    protected int level;

    [SerializeField]
    public Buff buff = default;
    
    [SerializeField]
    public Sprite icon = default;

    public void Modify(Tower tower) {
        buff.level = level;
        buff.icon = icon;
        tower.StatusEffects.Add(buff);
    }
}