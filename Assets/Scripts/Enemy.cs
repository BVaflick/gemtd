using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

[SelectionBase]
public class Enemy : GameBehavior {

	[SerializeField]
	Transform model = default;

	[SerializeField]
	GameObject blastPatricals = null;
	
	[SerializeField]
	Material material = null;
	
	[SerializeField]
	EnemyAnimationConfig animationConfig = default;

	[SerializeField]
	HealthBar healthBar = default;
	
	EnemyAnimator animator;
	
	EnemyFactory originFactory;

	GameTile currentTile, nextTile;
	Vector3 positionFrom, positionTo, positionPrev;
	private Quaternion direction;
	private bool directionChanged;
	float progress, rotationProgress, progressFactor;
	List<GameTile> path;
	public float armor { get; set; }
	public float additionalArmor { get; set; }
	public float speed { get; set; }
	public float additionalSpeed { get; set; }
	float originalSpeed;
	int num = 0;
	public float Health { get; set; }
	public float Scale { get; private set; }
	public GameBehaviorCollection Effects { get; set; }

	public EnemyFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}
	public override bool GameUpdate() {
		animator.GameUpdate();
		healthBar.transform.LookAt(transform.position + Camera.main.transform.forward);
		if (animator.CurrentClip == EnemyAnimator.Clip.Intro) {
			if (!animator.IsDone) {
				return true;
			}
			animator.PlayMove(animationConfig.MoveAnimationSpeed * speed / Scale);
		}
		else if (animator.CurrentClip == EnemyAnimator.Clip.Outro) {
			if (animator.IsDone) {
				Recycle();
				return false;
			}
			return true;
		}
		additionalSpeed = 0f;
		additionalArmor = 0f;
		Effects.GameUpdate();
		if (Health <= 0f) {
			Vector3 position = transform.position;
			position.y += 0.5f;
			GameObject effectInstance = (GameObject) Instantiate(blastPatricals, position, transform.rotation);
			Destroy(effectInstance, 2f);
			Recycle();
			return false;
		}

		progressFactor = speed + additionalSpeed;
		progress += Time.deltaTime * (progressFactor / Vector3.Distance(positionFrom, positionTo));

		while (progress >= 1f) {
			progress = (progress - 1f) / progressFactor;
			progress = 0;
			PrepareNextState();
			if (nextTile == null) {
				Game.EnemyReachedDestination((int) Mathf.Ceil(Health));
				animator.PlayOutro();
				return true;
			}
		}
		if(directionChanged)
			transform.rotation = Quaternion.Slerp(transform.rotation, direction, progress);
		transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);

		return true;
	}

	public void Initialize(float scale, float speed, float pathOffset, float health, float armor) {
		Scale = scale;
		Vector3 s = model.localScale;
		model.localScale = new Vector3(scale * s.x, scale * s.y, scale * s.z);
		this.speed = speed;
		this.armor = armor;
		Health = health;
		Effects = new GameBehaviorCollection();
		blastPatricals.transform.GetComponent<ParticleSystemRenderer>().material = material;
		animator.PlayIntro();
		healthBar.setMaxValue((int) health);
		// Health = 20f * scale;
	}
	
	void Awake () {
		animator.Configure(
			model.GetChild(0).gameObject.AddComponent<Animator>(),
			animationConfig
		);
	}

	public override void Recycle() {
		animator.Stop();
		OriginFactory.Reclaim(this);
	}

	public void ApplyDamage(float damage, bool isDamagePhysical) {
		Debug.Assert(damage >= 0f, "Negative damage applied.");
		float modifier = 1f;
		if (isDamagePhysical) {
			modifier = 1f - ((0.052f * (armor + additionalArmor)) / (0.9f + (0.048f * Math.Abs(armor + additionalArmor))));
		}
		Health -= damage * modifier;
		healthBar.setValue((int) Health);
	}

	public void Spawn(List<GameTile> path) {
		this.path = path;
		currentTile = path[num];
		nextTile = path[++num];
		progress = 0f;
		rotationProgress = 0f;
		directionChanged = false;
		PrepareIntro();
	}

	void PrepareNextState() {
		currentTile = nextTile;
		nextTile = ++num == path.Count ? null : path[num];
		positionPrev = positionFrom;
		positionFrom = positionTo;
		if (nextTile == null) {
			num = 0;
			PrepareOutro();
			return;
		}
		positionTo = new Vector3(nextTile.transform.localPosition.x, transform.localPosition.y, nextTile.transform.localPosition.z);
		Quaternion currentRotation = Quaternion.LookRotation(positionFrom - positionPrev);
		Quaternion nextRotation = Quaternion.LookRotation(positionTo - positionFrom);
		directionChanged = currentRotation != nextRotation;
		if (directionChanged) direction = nextRotation;
	}
	
	void PrepareIntro() {
		positionFrom = currentTile.transform.localPosition;
		transform.localPosition = new Vector3(positionFrom.x, transform.localPosition.y, positionFrom.z);
		positionTo = new Vector3(nextTile.transform.localPosition.x, transform.localPosition.y, nextTile.transform.localPosition.z);
		transform.LookAt(nextTile.transform);
	}

	void PrepareOutro() {
		positionTo = currentTile.transform.localPosition;
	}
	
	void OnDrawGizmos() {
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.white;
		Handles.color = Color.red;
		Vector3 position = transform.localPosition;
		Handles.Label(position, "HP: " + Mathf.Ceil(Health), style);
		position.z -= 0.3f;
		Handles.Label(position, "SP: " + speed + (additionalSpeed != 0 ? "" + additionalSpeed : ""), style);
		position.z -= 0.3f;
		Handles.Label(position, "Armor: " + armor + (additionalArmor != 0 ? "" + additionalArmor : ""), style);
		position.z -= 0.3f;
		Handles.Label(position, "Effects: " + Effects.Count, style);
	}
}