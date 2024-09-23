using System.Collections.Generic;
using UnityEngine;

public class Shell : WarEntity {

	[SerializeField]
	GameObject blastPatricals = null;

	List<EnemyBuff> debuffs = new List<EnemyBuff>();

	Vector3 launchPoint, launchVelocity;

	float speed, blastRadius = 1f, damage;

	public void Initialize(Vector3 launchPoint, TargetPoint target, Tower tower, float damage, float speed, List<EnemyBuff> debuffs) {
		this.launchPoint = launchPoint;
		transform.localPosition = launchPoint;
		this.tower = tower;
		this.target = target;
		this.damage = damage;
		this.debuffs = debuffs;
		this.speed = speed;
	}

	
	public override bool GameUpdate() {
		if (target != null) {
			transform.LookAt(target.Position);
			launchVelocity = target.Position - transform.position;
			float distance = speed * Time.deltaTime;
			if (launchVelocity.magnitude <= distance) {
				foreach (var debuff in debuffs) {
					debuff.Modify(tower, target, damage);
				}	
				// GameObject effectInstance = Instantiate(blastPatricals, transform.position, transform.rotation);
				// Destroy(effectInstance, .5f);
				target.Enemy.ApplyDamage(tower, damage, true);
				OriginFactory.Reclaim(this);
				return false;
			}
			transform.Translate(launchVelocity.normalized * distance, Space.World);
		} else {
			OriginFactory.Reclaim(this);
			return false;
		}
		return true;
	}
}