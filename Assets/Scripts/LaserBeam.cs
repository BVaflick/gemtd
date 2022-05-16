using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : WarEntity {

	[SerializeField]
	GameObject blastPatricals = null;

	List<EnemyBuff> debuffs = new List<EnemyBuff>();

	Vector3 launchPoint, launchVelocity;	

	float speed, blastRadius = 1f, damage;

	public void Initialize(Vector3 launchPoint, TargetPoint target, float damage, float speed, List<EnemyBuff> debuffs) {
		this.launchPoint = launchPoint;
		transform.localPosition = launchPoint;
		this.target = target;
		this.damage = damage;
		this.debuffs = debuffs;
		this.speed = speed;
	}

	public override bool GameUpdate() {
		if (target != null) {
			Vector3 point = target.Position;
			float d = Vector3.Distance(launchPoint, point);
			Vector3 laserBeamScale = transform.localScale;
			laserBeamScale.z = d * Time.deltaTime;
			transform.localPosition = launchPoint + 0.5f * d * transform.forward;
			// launchVelocity = target.Position - transform.position;
			// float distance = speed * Time.deltaTime;
			// if (launchVelocity.magnitude <= distance) {
			// 	foreach (var debuff in debuffs) {
			// 		debuff.Modify(target, damage);
			// 	}	
			// 	GameObject effectInstance = (GameObject) Instantiate(blastPatricals, transform.position, transform.rotation);
			// 	Destroy(effectInstance, 2f);
			// 	target.Enemy.ApplyDamage(damage, true);
			// 	OriginFactory.Reclaim(this);
			// 	return false;
			// }
			// transform.Translate(launchVelocity.normalized * distance, Space.World);
			
		} else {
			OriginFactory.Reclaim(this);
			return false;
		}
		return true;
	}
}