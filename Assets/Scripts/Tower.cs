﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class Tower : GameTileContent {

	[SerializeField, Range(1.5f, 10.5f)]
	float targetingRange = 1.5f;

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
	Transform newTowerCircle = default;
	
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
	
	public List<Ability> Abilities => abilities;

	public List<Aura> Auras => auras;

	public int TargetNumber { get => additionalTargets; set => additionalTargets = value; }

	public float Dmg { get => damage;}
	public float Damage { get => additionalDamage; set => additionalDamage = value; }
	public float Range { get => targetingRange; set => targetingRange = value; }

	public TowerType TowerType => towerType;
	public TowerType[] Combo => combo;
	public float AS { get => attackSpeed;}
	public float AttackSpeed { get => additionalAttackSpeed; set => additionalAttackSpeed = value; }


	public bool aimTarget(TargetPoint preferredTarget) {
		return AcquireTarget(ref targets, preferredTarget);
	}
	
	protected bool AcquireTarget(ref List<TargetPoint> targets, TargetPoint preferredTarget) {
		if (TargetPoint.FillBuffer(transform.localPosition, targetingRange, Game.enemyLayerMask)) {
			if (preferredTarget != null) {
				targets.Clear();
				targets.Add(preferredTarget);
			}
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
		swapEffect = new GameObject().transform;
		swapEffect.SetParent(transform);
		// float x = targetingRange + 0.25001f;
		// float y = -turret.position.y;
		// launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
	}

	public override void GameUpdate() {
		if (swapEffect.childCount > 0) {
			swapEffect.GetChild(0).LookAt(transform.position + Camera.main.transform.forward);
		}
		additionalTargets = 0;
		additionalAttackSpeed = 0f;
		additionalDamage = 0f;
		abilities.ForEach(ability => ability.Modify(this));
		statusEffects.FindAll(statusEffect => statusEffect is TowerBuff).ForEach(statusEffect => ((TowerBuff) statusEffect).Modify(this));
		// Buff burn = statusEffects.Find(statusEffect => statusEffect is Burn);
		// if (burn) {
		// 	launchProgress += (attackSpeed + additionalAttackSpeed) * Time.deltaTime;
		// 	if (launchProgress >= 1f) {
		// 		((Burn) burn).Modify(this, damage);
		// 		launchProgress -= 1f;
		// 	}
		// }
		if (TrackTarget(ref targets) || AcquireTarget(ref targets, null)) {
			Vector3 rot = turret.transform.eulerAngles;
			turret.LookAt(targets[0].Position);
			Vector3 rot2 = turret.transform.eulerAngles;
			turret.eulerAngles = new Vector3(rot.x, rot2.y, rot.z);
			launchProgress += (attackSpeed + additionalAttackSpeed) * Time.deltaTime;
			if (launchProgress >= 1f && damage > 0) {
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
			if(!target.Enemy.isInvisible) Game.SpawnShell().Initialize(launchPoint, target, this, damage + additionalDamage, launchSpeed, debuffs);
		}
	}
	
	public void switchNewTowerCircle() {
		newTowerCircle.gameObject.SetActive(!newTowerCircle.gameObject.activeSelf);
	}
	
	// void OnDrawGizmosSelected() {
	void OnDrawGizmos() {
		if (GizmoExtensions.showTowerRange && TowerType != TowerType.FlyingTower) {
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.white;
			Vector3 position = transform.localPosition;
			// position.y += 0.01f;
			// Handles.color = new Color(1, 1, 1, 0.05f);
			// Handles.DrawSolidDisc(position, transform.up, (float) targetingRange);
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
				for (var i = 0; i < targets.Count; i++) {
					if (targets[i] != null) {
						Gizmos.color = i == 0 ? Color.red : Color.yellow;
						Gizmos.DrawLine(turret.transform.position, targets[i].Position);
					}
				}
			}
		}
	}
}