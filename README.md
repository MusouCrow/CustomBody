## KinematicBody

* 位移
    * 给单位输入速度，在最后的更新阶段结算
    * 使用胶囊体射线判断位移程度
    * 结算位移，赋予坐标
* 解除重叠
    * 使用胶囊体碰撞判断重叠体
    * 调用`Physics.ComputePenetration`与重叠体结算出偏移值
    * 结算偏移，赋予坐标