using System;

public class Timer : IGear {
    public float from;
    public float to;
    public bool isLoop;
    public Action Func;

    public bool IsRunning {
        get;
        protected set;
    }

    public void Update(float dt) {
        if (!this.IsRunning) {
            return;
        }

        this.from += dt;

        if (this.from >= this.to) {
            this.from = this.from - this.to;
            this.IsRunning = this.isLoop;
            this.Func?.Invoke();
        }
    }

    public void Enter(float time=0, Action Func=null, bool isLoop=false, bool retain=false) {
        this.IsRunning = true;
        
        this.from = retain ? this.from : 0;
        this.to = time > 0 ? time : this.to;
        this.Func = Func == null ? this.Func : Func;
        this.isLoop = isLoop;
    }

    public void Exit() {
        this.IsRunning = false;
    }
}