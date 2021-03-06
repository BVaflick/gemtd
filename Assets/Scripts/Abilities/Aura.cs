﻿using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class Aura : ScriptableObject {

    [SerializeField, Range(1, 5)]
    protected int level;

    [SerializeField]
    protected Buff buff = default;

    public void Modify(Tower tower) {
        buff.level = level;
        buff.name1 = buff.GetType().Name + level;
        TargetPoint.FillBuffer(tower.transform.localPosition, 3.5f, Game.towerLayerMask);
        Debug.Log(TargetPoint.BufferedCount);
        for (int i = 0; i < TargetPoint.BufferedCount; i++) {
            TargetPoint localTarget = TargetPoint.GetBuffered(i);
            Debug.Log("LT: " + localTarget);
            Debug.Log("Tower: " + localTarget.Tower);
            if(localTarget != null && !localTarget.Tower.StatusEffects.Any(effect => effect.name1 == buff.name1)) {                
                localTarget.Tower.StatusEffects.Add(buff);
            }
        }
    }
}