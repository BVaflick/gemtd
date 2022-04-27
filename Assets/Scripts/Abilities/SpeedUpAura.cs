using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class SpeedUpAura : TowerBuff {

   public override void Modify(Tower tower) {
      tower.AttackSpeed += 1f * level;
   }
}