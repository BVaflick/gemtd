using UnityEngine;

[CreateAssetMenu]
public class Burn : Aura {
	
	float cooldown = 0f;
	
	public override void Modify(Tower tower) {
		cooldown += Time.deltaTime;
		if (cooldown >= 1f) {
			TargetPoint.FillBuffer(tower.transform.localPosition, 3.5f, Game.enemyLayerMask);
			for (int i = 0; i < TargetPoint.BufferedCount; i++) {
				TargetPoint localTarget = TargetPoint.GetBuffered(i);
				localTarget.Enemy.ApplyDamage(tower, 40f, false);
				Fire fire = Game.SpawnFire(true);
				fire.Initialize(tower, localTarget, this.GetType().Name + level, icon);
				localTarget.Enemy.VisualEffects.Add(fire);
			}
			cooldown = 0f;
		}
	}
}