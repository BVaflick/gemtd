using UnityEngine;

public abstract class WarEntity : GameBehavior {

    WarFactory originFactory;

    public string name1;
    
    public float scale = 1f;

    public float age { get; set; }

    protected TargetPoint target;
    
    public void Initialize(TargetPoint target, string name) {
        name1 = name;
        this.target = target;
        scale = target.Enemy.Scale * 1.5f;
        transform.localPosition = target.Position;
    }

    public WarFactory OriginFactory {
        get => originFactory;
        set {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public override void Recycle () {
		originFactory.Reclaim(this);
	}
}