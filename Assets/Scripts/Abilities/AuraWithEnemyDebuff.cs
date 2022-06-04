using System.Globalization;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class AuraWithEnemyDebuff : Aura {

	public override void Modify(Tower tower) {
		buff.level = level;
		buff.icon = icon;
		buff.name1 = buff.GetType().Name + level;
		TargetPoint.FillBuffer(tower.transform.localPosition, 3.5f, Game.enemyLayerMask);
		for (int i = 0; i < TargetPoint.BufferedCount; i++) {
			TargetPoint localTarget = TargetPoint.GetBuffered(i);
			// localTarget.Enemy.ApplyDamage(tower, 40f, false);
			// Explosion explosion = Game.SpawnExplosion(true);
			// explosion.Initialize(tower, localTarget, this.GetType().Name + level, icon);
			if (localTarget != null && localTarget.Enemy.StatusEffects.All(effect => effect.name1 != buff.name1)) {
				localTarget.Enemy.StatusEffects.Add(buff);
			}
		}
	}
}