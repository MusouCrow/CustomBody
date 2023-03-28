using UnityEngine;

public interface IMover {
    public Vector3 GroundPosition {
        get;
    }

    public void Move(Vector3 velocity);
}