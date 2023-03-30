using UnityEngine;

public interface IBody {
    public bool InGround {
        get;
    }

    public bool InLegalGround {
        get;
    }

    public Vector3 Velocity {
        get;
    }

    public Vector3 LegalGroundPosition {
        get;
    }

    public void LateUpdate();
    public void DrawGizmos();
    public void Move(Vector3 velocity);
    public void SetPosition(Vector3 position);
}