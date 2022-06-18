using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Poison : EnemyBuff {

   public override void Modify(Tower tower, TargetPoint target, float damage) {
      WarEntity b = (WarEntity) target.Enemy.VisualEffects.Behaviors.Find(effect => ((WarEntity) effect).name1 == GetType().Name + level);
      if (isRepeatable || b == null) {
         Toxin toxin = Game.SpawnToxin();
         toxin.Initialize(tower, target, this.GetType().Name + level, icon);
         target.Enemy.VisualEffects.Add(toxin);
      } 
      else {
         b.age = 0f;
      }
   }
}