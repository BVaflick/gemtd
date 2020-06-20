using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MinusArmor : EnemyBuff {

   public override void Modify(TargetPoint target, float damage) {
      WarEntity b = (WarEntity) target.Enemy.Effects.Behaviors.Find(effect => ((WarEntity) effect).name1 == this.GetType().Name + level);
      if (b != null) {
         b.age = 0f;
      } else {
         Corrosion corrosion = Game.SpawnCorrosion();
         corrosion.Initialize(target, this.GetType().Name + level);
         target.Enemy.Effects.Add(corrosion);
      }
   }
}