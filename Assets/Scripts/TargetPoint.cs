using UnityEngine;
using System.Collections.Generic;

public class TargetPoint : MonoBehaviour {

	static Collider[] buffer = new Collider[100];

	public static int BufferedCount { get; private set; }

	public static void RandomBuffered(ref List<TargetPoint> targets, int count) {
		while(targets.Count < (BufferedCount > count ? count : BufferedCount)) {
			TargetPoint t = GetBuffered(Random.Range(0, BufferedCount));
			if(!targets.Contains(t)) {
				targets.Add(t);
			}
		}		
	}

	public static bool FillBuffer (Vector3 position, float range, int layerMask) {
		Vector3 top = position;
		top.y += 3f;
		BufferedCount = Physics.OverlapCapsuleNonAlloc(
			position, top, range, buffer, layerMask
		);
		return BufferedCount > 0;
	}

	public static TargetPoint GetBuffered (int index) {
		var target = buffer[index].GetComponent<TargetPoint>();
		// Debug.Assert(target != null, "Targeted non-enemy!", buffer[0]);
		return target;
	}

	public Enemy Enemy { get; private set; }

	public Tower Tower { get; private set; }

	public Vector3 Position => transform.position;

	void Awake () {
		Enemy = transform.root.GetComponent<Enemy>();
		Tower = transform.root.GetComponent<Tower>();
		// Debug.Assert(Enemy != null, "Target point without Enemy root!", this);
		// Debug.Assert(
		// 	GetComponent<SphereCollider>() != null,
		// 	"Target point without sphere collider!", this
		// );
		// Debug.Assert(gameObject.layer == 9, "Target point on wrong layer!", this);
	}
}