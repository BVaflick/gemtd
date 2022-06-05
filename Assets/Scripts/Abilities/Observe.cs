using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class Observe : EnemyAuraBuff {

   public override void Modify(Enemy enemy) {
      if (enemy.isInvisible) {
         enemy.transform.GetComponentInChildren<SkinnedMeshRenderer>().material = enemy.revealedInvisibleMaterial; 
         enemy.healthBar.gameObject.SetActive(true);
         enemy.isInvisible = false;
      }
   }
}