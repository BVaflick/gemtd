using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Splash : EnemyBuff {

   public override void Modify(Tower tower, TargetPoint enemy, float damage) {
      TargetPoint.FillBuffer(enemy.Position, 3.5f, Game.enemyLayerMask);
      for (int i = 0; i < TargetPoint.BufferedCount; i++) {
         TargetPoint localTarget = TargetPoint.GetBuffered(i);
         if (localTarget != enemy) {
            localTarget.Enemy.ApplyDamage(tower,damage / 2f, false);
            Explosion explosion = Game.SpawnExplosion(false);         
            explosion.Initialize(tower, localTarget, this.GetType().Name + level, icon);         
            localTarget.Enemy.VisualEffects.Add(explosion);
         }
      }
   }

}