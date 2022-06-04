using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class SlowAura : EnemyAuraBuff {

   public override void Modify(Enemy enemy) {
      enemy.additionalSpeed -= 1f;
   }
}