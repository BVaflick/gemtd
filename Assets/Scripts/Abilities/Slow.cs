using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Slow : EnemyBuff {

   public override void Modify(Tower tower, TargetPoint target, float damage) {
      WarEntity b = (WarEntity) target.Enemy.VisualEffects.Behaviors.Find(effect => ((WarEntity) effect).name1 == this.GetType().Name + level);
      if (b != null) {
         b.age = 0f;
      } else {
         Ice ice = Game.SpawnIce();
         ice.Initialize(tower, target, this.GetType().Name + level, icon);
         target.Enemy.VisualEffects.Add(ice);
      }
   }
}