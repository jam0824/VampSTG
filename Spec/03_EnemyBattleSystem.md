# 敵・バトルシステム詳細仕様書

## 1. 敵システム基本仕様

### 1.1 BaseEnemy クラス
すべての敵キャラクターが継承する基底クラスです。

```csharp
public abstract class BaseEnemy : MonoBehaviour, IEnemy
{
    // 基本パラメータ
    float hp;                    // 体力
    float maxHp;                 // 最大体力
    
    // エフェクト設定
    GameObject explosion;        // 爆発エフェクト
    AudioClip explosionSe;      // 爆発SE
    float explosionSeVolume;    // 爆発SE音量
    float offsetExplosionY;     // 爆発Y軸オフセット
    
    // 攻撃設定
    bool isAttack;              // 攻撃するかどうか
    float attackInterval;       // 攻撃間隔（秒）
    float attackAnimationWait;  // 攻撃アニメーション待機時間
    bool isDirectionAttack;     // 方向指定攻撃かどうか
    float attackDirection;      // 攻撃方向（度）
    
    // その他
    Animator animator;          // アニメーター
    int fromBossDamage;         // ボスからのダメージ値
}
```

### 1.2 敵の基本動作
1. **初期化（Start）**:
   - プレイヤー参照取得（"Core"タグ）
   - 最大HP設定・GameManagerへHP報告
   - 攻撃設定の初期化
   - IEnemyShooter取得・攻撃コルーチン開始

2. **更新処理（Update）**:
   - 子クラス固有の移動処理実行
   - 死亡判定（HP <= 0）

3. **死亡処理（enemyDie）**:
   - アイテムドロップ処理
   - 爆発エフェクト生成
   - GameManagerへ撃破数報告
   - オブジェクト削除

## 2. 攻撃システム

### 2.1 攻撃タイプ
#### 2.1.1 方向指定攻撃（isDirectionAttack = true）
- **攻撃方向**: attackDirection で指定した方向（度）
- **用途**: 固定方向への攻撃パターン

#### 2.1.2 プレイヤー狙い攻撃（isDirectionAttack = false）
- **攻撃対象**: プレイヤー（"Core"タグ）の現在位置
- **用途**: 追尾性のある攻撃パターン

### 2.2 攻撃実行フロー
1. **攻撃間隔待機**: attackInterval 秒間待機
2. **攻撃アニメーション開始**: 攻撃アニメーション実行
3. **アニメーション待機**: attackAnimationWait 秒間待機
4. **弾発射**: IEnemyShooter インターフェースで実行
5. **攻撃間隔待機へ戻る**

### 2.3 弾薬システム
- **敵弾プール**: _enemyBulletPool で管理
- **オブジェクトプーリング**: 非アクティブな弾を再利用
- **弾薬生成**: 新規作成とプール再利用の自動切り替え

## 3. ダメージシステム

### 3.1 被ダメージ処理
- **プレイヤー弾からのダメージ**: ConfigPlayerBullet.damage
- **ボスからのダメージ**: fromBossDamage パラメータ
- **爆発ダメージ**: ExplosionDamage クラス

### 3.2 ダメージ計算
```csharp
// 基本ダメージ計算式
float finalDamage = baseDamage * powerMagnification;
```

### 3.3 死亡判定
- **HP <= 0** で死亡処理開始
- **isDead フラグ**: 重複死亡処理を防止

## 4. エフェクトシステム

### 4.1 爆発エフェクト分類
- **Small Explosion**: 小型敵用爆発
- **Middle Explosion**: 中型敵用爆発  
- **Large Explosion**: 大型敵・ボス用爆発

### 4.2 エフェクト制御
- **位置制御**: offsetExplosionY でY軸調整
- **重複制御**: minimumExplosionDistance で距離制限
- **履歴管理**: maxExplosionHistory で古い位置削除

### 4.3 音響エフェクト
- **爆発SE**: explosionSe + explosionSeVolume
- **3D音響**: SoundManager.Instance.PlaySE()

## 5. 特殊敵システム

### 5.1 ボス敵システム
#### 5.1.1 Stage3 Gastaroid ボス
- **BossMiddleGastaroid**: 中型形態
- **GrowToMiddleGastaroid**: 成長システム
- **特殊攻撃パターン**: 複数フェーズ対応

#### 5.1.2 Stage4 MidBoss
- **多段階HP**: 段階的な行動変化
- **特殊エフェクト**: 専用爆発・SE
- **アニメーション制御**: 複雑なアニメーション管理

### 5.2 特殊行動敵
- **追跡型**: プレイヤーを追跡する敵
- **パターン移動型**: 決められたパスを移動
- **待機型**: 特定条件で攻撃開始

## 6. アイテムドロップシステム

### 6.1 ドロップアイテム設定
- **item プロパティ**: 各敵が持つドロップアイテム
- **確率制御**: 敵種別ごとのドロップ率
- **アイテム種類**: パワーアップ、キャラクター解放など

### 6.2 ドロップ処理フロー
1. **死亡時判定**: enemyDie() 内で実行
2. **アイテム生成**: item が null でない場合生成
3. **位置設定**: 敵の死亡位置に生成
4. **GameManager報告**: アイテム数カウント

## 7. バトル統計システム

### 7.1 密度計算
GameManager で各種密度を計算:
- **敵密度**: stageAllEnemyCount / stageAllSecond
- **アイテム密度**: stageAllItemCount / stageAllSecond  
- **HP密度**: stageAllHp / stageAllSecond

### 7.2 基準値（Stage1 対比）
- **STAGE1_ENEMY_DENSITY**: 2.12
- **STAGE1_ITEM_DENSITY**: 0.22
- **STAGE1_HP_DENSITY**: 6.57

### 7.3 バランス調整
各ステージの密度比較により、ゲームバランスを調整・監視

## 8. AI・行動パターン

### 8.1 移動パターン
- **HandleMovement()**: 子クラスで実装する移動処理
- **プレイヤー追跡**: playerTransform を基準とした移動
- **境界制御**: 画面外への移動制限

### 8.2 攻撃パターン
- **定期攻撃**: attackInterval による一定間隔攻撃
- **条件攻撃**: 特定条件下での攻撃開始
- **連続攻撃**: 複数弾の連続発射

### 8.3 アニメーション連携
- **Animator制御**: isAttackAnimation フラグ
- **状態同期**: アニメーションとゲームロジックの同期
- **パフォーマンス**: 必要時のみAnimator取得