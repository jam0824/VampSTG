# VampSTG 仕様書ドキュメント

VampSTG（Vampire Shooting Game）の包括的な仕様書ドキュメント集です。

## 📋 ドキュメント構成

### [01_GameOverview.md](./01_GameOverview.md)
**ゲーム概要・基本仕様**
- ゲームコンセプトと基本情報
- 主要システムの概要
- 技術仕様・アーキテクチャ
- スコアシステム・データ管理概要

### [02_PlayerCharacterSystem.md](./02_PlayerCharacterSystem.md)
**プレイヤー・キャラクターシステム詳細**
- プレイヤー操作システム（移動・射撃・アニメーション）
- キャラクターデータ構造・パラメータ
- 装備システム（バッテリー・弾薬）
- プレイヤー管理・状態制御

### [03_EnemyBattleSystem.md](./03_EnemyBattleSystem.md)
**敵・バトルシステム詳細**
- BaseEnemyクラス・敵システム基本仕様
- 攻撃システム・ダメージ計算
- エフェクト・音響システム
- ボス敵・特殊行動敵・AI システム

### [04_StageItemSystem.md](./04_StageItemSystem.md)
**ステージ・アイテムシステム詳細**
- ステージデータ構造・選択システム
- アイテムシステム・データベース管理
- アンロックシステム・進行度管理
- バランス調整・統計システム

### [05_UIDataSystem.md](./05_UIDataSystem.md)
**UI・データ管理システム詳細**
- GameManagerシステム・シングルトン管理
- UIシステム（メニュー・HUD・設定画面）
- データ管理（セーブ・ロード・多言語）
- パフォーマンス最適化・デバッグ支援

## 🎮 ゲーム基本情報

- **ゲーム名**: VampSTG (Vampire Shooting Game)
- **ジャンル**: 横スクロールシューティングゲーム（YZ軸面）
- **プラットフォーム**: Unity 3D
- **対応言語**: 英語・日本語
- **フレームレート**: 60FPS（設定可能）

## 🏗️ アーキテクチャ概要

### 主要クラス構成
```
GameManager (Singleton)
├── PlayerController
├── CharacterData (ScriptableObject)
├── StageData (ScriptableObject)
├── ItemDataDB
├── EffectController (Singleton)
└── Various UI Controllers
```

### システム間連携
```
Player System ←→ Item System
    ↓              ↓
Battle System ←→ Stage System
    ↓              ↓
Effect System ←→ UI System
    ↓              ↓
    Data Management System
```

## 📊 主要パラメータ

### スコア計算
- **敵撃破**: MaxHP × 100
- **弾数**: BulletCount × 10  
- **アイテム**: ItemCount × 1000
- **ノーミス**: 100,000ボーナス

### 移動範囲
- **Z軸**: -9.0 〜 9.0
- **Y軸**: -4.0 〜 6.0
- **移動速度**: 5.0（基準値）

### ステージ構成
- **Total**: 6ステージ + テストステージ
- **Stage1**: 基準ステージ（密度計算基準）
- **Stage3**: Gastaroidボス登場
- **Stage4**: MidBoss登場

## 🔧 開発・保守情報

### コーディング規約
```csharp
// 変数命名
private field: m_VariableName
constants: c_ConstantName
static field: s_StaticName
properties: PropertyName
methods: MethodName()
arguments: _argumentName
```

### テスト構成
- **EditMode Tests**: プロジェクトルート/Tests/
- **Test Coverage**: GameManager, BaseEnemy, Effects
- **テストフレームワーク**: Unity Test Framework

### データ形式
- **セーブデータ**: JSON形式（persistentDataPath）
- **設定データ**: ScriptableObject
- **多言語**: 配列インデックス方式

## 📝 更新履歴

- **v1.0**: 初期仕様書作成
  - 全システムの詳細仕様記載
  - プレイヤー・敵・ステージ・アイテムシステム
  - UI・データ管理システム

## 🎯 今後の拡張予定

- **追加ステージ**: Stage7以降
- **新キャラクター**: 特殊能力キャラクター
- **マルチプレイ**: 協力・対戦モード
- **実績システム**: Steam実績連携

---

**注意**: この仕様書は現在のコードベース（VampSTG2）を分析して作成されています。実装と異なる場合は、実装を優先してください。