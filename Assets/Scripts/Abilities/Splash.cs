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
            Enemy targetEnemy = localTarget.Enemy;
            targetEnemy.ApplyDamage(tower,damage / 2f, false);
            if (!targetEnemy.VisualEffects.Behaviors.Exists(effect => effect is Explosion)) {
               Explosion explosion = Game.SpawnExplosion(false);         
               explosion.Initialize(tower, localTarget, this.GetType().Name + level, icon);         
               localTarget.Enemy.VisualEffects.Add(explosion);
            }
         }
      }
   }
}