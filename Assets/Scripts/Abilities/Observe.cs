using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class Observe : EnemyAuraBuff {

   public override void Modify(Enemy enemy) {
      Color c = enemy.modelMaterial.color;
      c.a = 0.5f;
      enemy.modelMaterial.color = c;
      enemy.healthBar.gameObject.SetActive(true);
   }
}