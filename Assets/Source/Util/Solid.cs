using System;
using UnityEngine;

public class Solid {
    private Transform transform;
    private CharacterController controller;
    private int layerMask;
    private bool isTick;

    public Vector3 velocity;
    public event Action<RaycastHit> GroundEvent;
    public event Action FallEvent;

    public bool InGround {
        get;
        private set;
    }

    public bool InLegal {
        get;
        private set;
    }

    public float GroundY {
        get;
        private set;
    }

    public RaycastHit GroundHit {
        get;
        private set;
    }

    public Solid(Transform transform, CharacterController controller) {
        this.transform = transform;
        this.controller = controller;
        this.controller.material.staticFriction = 1;
        this.layerMask = LayerMask.GetMask("Default");
    }

    public void Start() {
        this.Flush();
    }

    public void Update() {
        if (this.isTick || !this.velocity.Equal(Vector3.zero)) {
            if (this.controller.isGrounded && this.velocity.y.Equal(0)) {
                this.velocity.y -= 0.1f; // 防止在地判定失误
            }

            this.controller.Move(this.velocity);

            bool inGround = this.controller.isGrounded;
            var hit = this.HitGround();
            
            if (this.isTick || this.InGround != inGround) {
                this.InGround = inGround;
                
                if (inGround) {
                    this.GroundEvent?.Invoke(hit);
                }
                else {
                    this.FallEvent?.Invoke();
                }
            }
            
            this.GroundHit = hit;
            this.velocity = Vector3.zero;
            this.isTick = false;
        }
    }

    public void Flush() {
        this.isTick = true;
        this.InGround = false;
    }

    private RaycastHit HitGround() {
        RaycastHit hit;
        bool ok = Physics.SphereCast(this.transform.position + this.controller.height * Vector3.up, this.controller.radius, Vector3.down, out hit, 100, this.layerMask);
        // var ok = Physics.Raycast(this.transform.position, Vector3.down, out hit, 100, this.layerMask);
        var angle = Vector3.Angle(hit.normal, Vector3.up);
        
        this.GroundY = hit.point.y;
        this.InLegal = hit.collider.material.staticFriction < 1 && angle <= this.controller.slopeLimit;

        return hit;
    }
}