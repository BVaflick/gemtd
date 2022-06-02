using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class MVPAura : TowerBuff {

   public override void Modify(Tower tower) {
      tower.Damage += 10f * level;
   }
}