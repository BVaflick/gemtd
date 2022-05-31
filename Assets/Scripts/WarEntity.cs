using UnityEngine;

public abstract class WarEntity : GameBehavior {

    WarFactory originFactory;

    public string name1;
    
    public float scale = 1f;

    public Tower tower;

    public float age { get; set; }
    
    public Sprite icon { get; set; }

    protected TargetPoint target;
    
    public void Initialize(Tower tower, TargetPoint target, string name, Sprite icon) {
        name1 = name;
        this.icon = icon;
        this.tower = tower;
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