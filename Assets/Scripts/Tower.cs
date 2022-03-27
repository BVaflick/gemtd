using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class Tower : GameTileContent {

	[SerializeField, Range(1.5f, 10.5f)]
	protected float targetingRange = 1.5f;

	protected int targetNumber = 1;

	int additionalTargets = 0;

	[SerializeField]
	protected TowerType towerType = default;
	
	[SerializeField]
	protected TowerType[] combo = default;

	[SerializeField, Range(0f, 100f)]
	float damage = 10f;

	float additionalDamage = 0f;

	List<TargetPoint> targets = new List<TargetPoint>();

	[SerializeField]
	Transform turret = default;
	[SerializeField]
	Transform selection = default;
	
	[SerializeField]
	float launchSpeed = 30f;

	float launchProgress;

	float attackSpeed = 1f;

	float additionalAttackSpeed = 0f;

	List<Buff> statusEffects = new List<Buff>();

	[SerializeField]
	List<Ability> abilities = new List<Ability>();

	[SerializeField]
	List<Aura> auras = new List<Aura>();

	public List<Buff> StatusEffects => statusEffects;

	public List<Aura> Auras => auras;

	public int TargetNumber { get => additionalTargets; set => additionalTargets = value; }

	public float Damage { get => additionalDamage; set => additionalDamage = value; }

	public TowerType TowerType => towerType;
	public TowerType[] Combo => combo;
	public float AttackSpeed { get => additionalAttackSpeed; set => additionalAttackSpeed = value; }

	protected bool AcquireTarget(ref List<TargetPoint> targets) {
		if (TargetPoint.FillBuffer(transform.localPosition, targetingRange, Game.enemyLayerMask)) {
			TargetPoint.RandomBuffered(ref targets, targetNumber + additionalTargets);
			return true;
		}
		targets.Clear();
		return false;
	}

	protected bool TrackTarget(ref List<TargetPoint> targets) {
		foreach (var target in targets) {
			if (target != null) {
				Vector3 a = transform.localPosition;
				Vector3 b = target.Position;
				float x = a.x - b.x;
				float z = a.z - b.z;
				float r = targetingRange + 0.125f * target.Enemy.Scale;
				if (x * x + z * z > r * r) {
					targets.Remove(target);
					return false;
				}
			} else {
				targets.Remove(target);
				return false;
			}
		}
		if (targets.Count < targetNumber + additionalTargets) {
			return false;
		}
		return true;
	}

	void Awake() {
		float x = targetingRange + 0.25001f;
		float y = -turret.position.y;
		// launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
	}

	public override void GameUpdate() {
		additionalTargets = 0;
		additionalAttackSpeed = 0f;
		additionalDamage = 0f;
		abilities.ForEach(ability => ability.Modify(this));
		statusEffects.FindAll(statusEffect => statusEffect is TowerBuff).ForEach(statusEffect => ((TowerBuff) statusEffect).Modify(this));
		if (TrackTarget(ref targets) || AcquireTarget(ref targets)) {
			Vector3 rot = turret.transform.eulerAngles;
			turret.LookAt(targets[0].Position);
			Vector3 rot2 = turret.transform.eulerAngles;
			turret.eulerAngles = new Vector3(rot.x, rot2.y, rot.z);
			launchProgress += (attackSpeed + additionalAttackSpeed) * Time.deltaTime;
			if (launchProgress >= 1f) {
				Buff burn = statusEffects.Find(statusEffect => statusEffect is Burn);
				if (burn) {
					((Burn) burn).Modify(this, damage);
				}
				// Shoot();
				Launch(targets);
				launchProgress -= 1f;
			}
		} else {
			launchProgress += (attackSpeed + additionalAttackSpeed) * Time.deltaTime;
			if (launchProgress >= 1f) {
				launchProgress = 0.999f;
			}
			turret.LookAt(turret.transform.position);
		}
	}

	// void Shoot() {
	// 	foreach (var target in targets) {
	// 		Vector3 point = target.Position;
	// 		turret.LookAt(targets[0].Position);
	// 		float d = Vector3.Distance(turret.position, point);
	// 		target.Enemy.ApplyDamage(damage * Time.deltaTime, true);
	// 	}
	// }

	public void Launch(List<TargetPoint> targets) {
		Vector3 launchPoint = turret.position;
		List<EnemyBuff> debuffs = new List<EnemyBuff>();
		statusEffects.FindAll(statusEffect => statusEffect is EnemyBuff).ForEach(statusEffect => debuffs.Add((EnemyBuff) statusEffect));		
		foreach (var target in targets) {
			Game.SpawnShell().Initialize(launchPoint, target, damage + additionalDamage, launchSpeed, debuffs);
		}
	}

	public void swithSelection() {
		selection.gameObject.SetActive(!selection.gameObject.activeSelf);
	}
	
	// void OnDrawGizmosSelected() {
	void OnDrawGizmos() {
		if (GizmoExtensions.showTowerRange && TowerType != TowerType.FlyingTower) {
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.white;
			Gizmos.color = Color.yellow;
			Vector3 position = transform.localPosition;
			position.y += 0.01f;
			Handles.color = new Color(1, 1, 1, 0.05f);
			Handles.DrawSolidDisc(position, transform.up, (float) targetingRange);
			Handles.Label(position, TowerType.ToString(), style);
			position.z -= 0.3f;
			Handles.Label(position,
				"SP: " + attackSpeed + (additionalAttackSpeed != 0 ? "+" + additionalAttackSpeed : ""), style);
			position.z -= 0.3f;
			Handles.Label(position,
				"Effects: " + string.Join(" ",
					statusEffects.Select(statusEffect => $"{statusEffect.name.Split('(')[0]}")), style);
			// Gizmos.DrawWireSphere(position, targetingRange);
			if (targets.Count != 0) {
				foreach (var target in targets) {
					if (target != null) {
						Gizmos.DrawLine(turret.transform.position, target.Position);
					}
				}
			}
		}
	}
}