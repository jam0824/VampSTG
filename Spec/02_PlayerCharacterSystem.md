# プレイヤー・キャラクターシステム詳細仕様書

## 1. プレイヤー操作システム

### 1.1 基本操作
- **移動**: WASD キー または 方向キー
  - W/↑: Y軸正方向（上）
  - S/↓: Y軸負方向（下）
  - A/←: Z軸負方向（後ろ）
  - D/→: Z軸正方向（前）
- **射撃**: 自動射撃（ボタンを押さなくても自動で発射される）
- **向き制御**: Z キー または ゲームパッド1ボタン で向き固定

### 1.2 移動システム仕様
- **移動速度**: 基本値 5.0f（キャラクターごとに調整可能）
- **移動範囲制限**:
  - Z軸: -9.0f ～ 9.0f
  - Y軸: -4.0f ～ 6.0f
- **向きシステム**:
  - 前後向き切り替え: プレイヤーは前方向・後方向の両方を向ける
  - 向き固定: Zキー または ゲームパッド1ボタンで現在の向きを固定
  - 回転時間: 0.2秒
  - 向き判定: facingRight フラグで管理

### 1.3 アニメーションシステム
#### 1.3.1 アニメーション状態
- **Idle**: 待機状態（浮遊アニメーション付き・自動射撃継続）
- **Move**: 移動状態（自動射撃継続）
- **MoveBack**: 後退状態（自動射撃継続）
- **Shoot**: 射撃アニメーション（自動射撃による）
- **MoveShoot**: 移動しながら射撃アニメーション
- **MoveBackShoot**: 後退しながら射撃アニメーション

**注意**: 射撃は常時自動で行われるため、射撃アニメーションは射撃タイミングに応じて自動的に再生されます。

#### 1.3.2 浮遊システム
- **振幅**: 0.05f（調整可能）
- **周波数**: 0.5Hz（調整可能）
- **適用**: Idle状態時のみ

### 1.4 プレイヤーモデル管理
- **CoreOffset**: プレイヤーモデルのコア配置オフセット
- **PlayerModel**: "PlayerModel"タグで自動検索・取得
- **動的取得**: Update()内で PlayerModel が null の場合は再検索

## 2. キャラクターシステム

### 2.1 キャラクターデータ構造
```csharp
public class CharacterData : ScriptableObject
{
    string characterId;           // キャラクター識別子
    string[] characterName;       // 名前（多言語対応）
    GameObject previewPrefab;     // プレビュー用3Dモデル
    GameObject playModel;         // ゲーム用モデル
    Vector3 playerModelOffset;    // Core配置オフセット
    Sprite characterSprite;       // キャラクター画像
    int life;                    // 体力パラメータ
    int power;                   // 攻撃力パラメータ
    int speed;                   // 移動速度パラメータ
    int pickupRange;             // アイテム取得範囲
    ItemData initialItemData;     // 初期装備アイテム
    string[] description;         // 説明文（多言語対応）
}
```

### 2.2 利用可能キャラクター
1. **CharacterData_A**: キャラクターA
2. **CharacterData_B**: キャラクターB
3. **CharacterData_C**: キャラクターC

### 2.3 キャラクター選択システム
- **選択画面**: CharacterSelectController が管理
- **プレビュー機能**: previewPrefab を使用
- **パラメータ表示**: PowerStars コンテナでパワー可視化
- **アンロック管理**: GameManager の gotCharacters で管理

### 2.4 キャラクターパラメータ詳細
- **Life**: プレイヤーの耐久力
- **Power**: 攻撃力倍率に影響
- **Speed**: 移動速度の基準値
- **PickupRange**: アイテム自動取得範囲

## 3. プレイヤー装備システム

### 3.1 バッテリーシステム
プレイヤーの主武器は「バッテリー」と呼ばれるシステムで管理されます。

#### 3.1.1 バッテリータイプ
- **BaseBattery**: 基本バッテリークラス
- **ClusterBattery**: クラスター弾バッテリー
- **GrenadeBattery**: グレネード弾バッテリー
- **SmineBattery**: スマイン弾バッテリー
- **BitBattery**: ビット砲バッテリー

#### 3.1.2 弾薬システム
- **BaseBullet**: 基本弾クラス
- **ConfigPlayerBullet**: プレイヤー弾設定
  - damage: ダメージ値
  - powerMagnification: パワー倍率

### 3.2 アイテム管理
- **PlayerItemManager**: アイテム取得処理
- **PowerMagnification**: パワー倍率システム
- **初期装備**: キャラクターごとの initialItemData

## 4. プレイヤー管理システム

### 4.1 PlayerManager
- **PowerMagnification**: 1.0f（基本値）
- **死亡処理**: ステージ死亡回数カウント
- **リスポーン**: 設定されたシーンに遷移

### 4.2 当たり判定
- **プレイヤーコア**: "Core" タグで識別
- **無敵時間**: 被弾後の無敵状態管理
- **エフェクト**: ヒットエフェクト（hitPlayer）

### 4.3 移動制限
GameManager で設定された範囲内での移動制限:
- **maxZ**: 9.0f
- **minZ**: -9.0f  
- **maxY**: 6.0f
- **minY**: -4.0f

## 5. プレイヤー状態管理

### 5.1 操作可能状態
- **canMove**: 移動可能フラグ
- **GameManager.canMove**: 全体の操作制御

### 5.2 向き管理
- **facingRight**: 向き判定フラグ
- **回転アニメーション**: turnDuration で制御

### 5.3 射撃状態
- **自動射撃**: 常時自動で弾が発射される
- **向き制御**: Zキー または ゲームパッド1ボタンで向き固定
- **射撃アニメーション**: 自動射撃タイミングでのアニメーション制御