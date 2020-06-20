using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Splash : EnemyBuff {

   public override void Modify(TargetPoint enemy, float damage) {
      TargetPoint.FillBuffer(enemy.Position, 3.5f, Game.enemyLayerMask);
      for (int i = 0; i < TargetPoint.BufferedCount; i++) {
         TargetPoint localTarget = TargetPoint.GetBuffered(i);
         if (localTarget != enemy) {
            localTarget.Enemy.ApplyDamage(damage / 2f, false);
            Explosion explosion = Game.SpawnExplosion(false);         
            explosion.Initialize(localTarget, this.GetType().Name + level);         
            localTarget.Enemy.Effects.Add(explosion);
         }
      }
   }

}