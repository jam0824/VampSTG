using UnityEngine;

// 敵の撃つ機能を持つクラスが実装するインターフェース
public interface IEnemyShooter
{
    public void Fire();
    public void Fire(float baseAngleDeg);
}
