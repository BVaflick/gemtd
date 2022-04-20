using System.Globalization;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class Aura : ScriptableObject {

    [SerializeField, Range(1, 5)]
    protected int level;

    [SerializeField]
    public Buff buff = default;
        
    [SerializeField]
    public Sprite icon = default;
    
    public void Modify(Tower tower) {
        buff.level = level;
        buff.icon = icon;
        buff.name1 = buff.GetType().Name + level;
        TargetPoint.FillBuffer(tower.transform.localPosition, 3.5f, Game.towerLayerMask);
        Debug.Log(TargetPoint.BufferedCount);
        for (int i = 0; i < TargetPoint.BufferedCount; i++) {
            TargetPoint localTarget = TargetPoint.GetBuffered(i);
            if(localTarget != null && localTarget.Tower.StatusEffects.All(effect => effect.name1 != buff.name1)) {                
                localTarget.Tower.StatusEffects.Add(buff);
            }
        }
    }
}