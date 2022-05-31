using UnityEngine;

[CreateAssetMenu]
public class Burn : Buff {
	
	public void Modify(Tower tower, float damage) {
		TargetPoint.FillBuffer(tower.transform.localPosition, 3.5f, Game.enemyLayerMask);
		for (int i = 0; i < TargetPoint.BufferedCount; i++) {
			TargetPoint localTarget = TargetPoint.GetBuffered(i);
			localTarget.Enemy.ApplyDamage(tower, 40f, false);
			Explosion explosion = Game.SpawnExplosion(true);
			explosion.Initialize(tower, localTarget, this.GetType().Name + level, icon);
		}
	}
}