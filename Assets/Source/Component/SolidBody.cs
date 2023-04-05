using System;
using UnityEngine;

public class SolidBody : Body {
    private Solid solid;
    private EaseMove gravityMove;

    public float gravityRate = 1;

    public Vector3 GroundPosition {
        get;
    }

    public Vector3 LegalPosition {
        get;
    }

    public Vector3 Height {
        get;
    }

    public Vector3 High {
        get;
    }

    public bool InGround {
        get;
    }

    public Vector3 Velocity {
        get;
        private set;
    }
    
    public SolidBody(Transform transform, CharacterController controller) : base(transform) {
        
    }

    public void Update() {

    }

    public override void Move(Vector3 velocity) {
        
    }

    public void BindFallEvent(Action OnFall) {
        this.solid.FallEvent += OnFall;
    }

    public void UnbindFallEvent(Action OnFall) {
        this.solid.FallEvent -= OnFall;
    }
}