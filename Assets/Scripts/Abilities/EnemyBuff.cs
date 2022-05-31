using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EnemyBuff : Buff {

    public abstract void Modify(Tower tower, TargetPoint enemy, float damage);
 
}