# ステージ・アイテムシステム詳細仕様書

## 1. ステージシステム

### 1.1 ステージデータ構造
```csharp
public class StageData : ScriptableObject
{
    string[] missionTitle;      // ミッション名（多言語対応）
    string sceneName;           // 遷移するシーン名
    int stageIndex;            // ステージインデックス
    Sprite previewImage;       // プレビュー用画像
    int difficulty;            // 難易度パラメータ
    string[] description;      // 説明文（多言語対応）
}
```

### 1.2 利用可能ステージ
1. **Stage1**: 基本ステージ（基準ステージ）
2. **Stage2**: 難易度上昇
3. **Stage3**: Gastaroidボス登場  
4. **Stage4**: MidBoss登場
5. **Stage5**: 高難易度ステージ
6. **Stage6**: 最終ステージ
7. **testScene**: テスト用ステージ

### 1.3 ステージ選択システム
- **StageSelectController**: ステージ選択画面管理
- **アンロック管理**: GameManager.gotStages で制御
- **プレビュー**: previewImage で視覚的確認
- **難易度表示**: difficulty パラメータによる難易度可視化

### 1.4 ステージ進行管理
#### 1.4.1 ステージ開始
- **データリセット**: ResetStageSaveData()
- **統計初期化**: 敵数・アイテム数・HP値の初期化
- **時間計測開始**: stageAllSecond のカウント開始

#### 1.4.2 ステージクリア
- **新アイテム確定**: stageGetNewItems → gotItems
- **新キャラクター確定**: stageGetNewCharacters → gotCharacters  
- **統計記録**: 密度計算・記録
- **リザルト表示**: Result.unity への遷移

### 1.5 ステージ統計システム
#### 1.5.1 密度計算
- **敵密度**: (stageAllEnemyCount / stageAllSecond)
- **アイテム密度**: (stageAllItemCount / stageAllSecond)
- **HP密度**: (stageAllHp / stageAllSecond)

#### 1.5.2 基準比較（Stage1対比）
- **敵密度倍率**: 現在密度 / STAGE1_ENEMY_DENSITY(2.12)
- **アイテム密度倍率**: 現在密度 / STAGE1_ITEM_DENSITY(0.22)
- **HP密度倍率**: 現在密度 / STAGE1_HP_DENSITY(6.57)

## 2. アイテムシステム

### 2.1 アイテムデータ構造
```csharp
public class ItemData : ScriptableObject
{
    string[] itemName;         // アイテム名（多言語対応）
    string type;              // 判定用タイプ
    GameObject itemObj;       // アイテムのゲームオブジェクト
    Sprite itemSprite;        // アイテムのイメージ
    float damage;             // ダメージパラメータ
    string[] description;     // 説明文（多言語対応）
}
```

### 2.2 アイテムタイプ分類
#### 2.2.1 武器系アイテム
- **type**: 武器の種類を示す文字列
- **damage**: 武器のダメージ値
- **itemObj**: 武器のプレハブ

#### 2.2.2 パワーアップアイテム
- **効果**: プレイヤーの能力向上
- **エフェクト**: PowerUpエフェクト再生
- **SE**: powerUpSe 再生

#### 2.2.3 キャラクター解放アイテム
- **効果**: 新キャラクターのアンロック
- **エフェクト**: CharacterGetエフェクト再生
- **SE**: characterGetSe 再生

### 2.3 アイテムデータベース（ItemDataDB）
```csharp
public class ItemDataDB : MonoBehaviour
{
    List<ItemData> listItemData;          // アイテムデータリスト
    List<CharacterData> listCharacterData; // キャラクターデータリスト
    
    // アイテム取得メソッド
    ItemData GetItemData(string type);
    bool HasItemData(string type);
    
    // キャラクター取得メソッド
    CharacterData GetCharacterData(string characterId);
    List<CharacterData> GetAllCharacterData();
    bool HasCharacterData(string characterId);
}
```

### 2.4 アイテム取得システム
#### 2.4.1 取得処理フロー
1. **当たり判定**: プレイヤーとアイテムの接触判定
2. **タイプ判定**: item.type による処理分岐
3. **効果適用**: PlayerItemManager.getItem()
4. **エフェクト再生**: EffectController によるエフェクト
5. **統計更新**: GameManager.itemCount++

#### 2.4.2 取得範囲
- **PickupRange**: キャラクターパラメータで設定
- **自動取得**: 範囲内への自動吸引機能

### 2.5 アイテム管理システム
#### 2.5.1 一時取得リスト
- **stageGetNewItems**: ステージ中取得の新アイテム
- **stageGetNewCharacters**: ステージ中取得の新キャラクター

#### 2.5.2 永続化処理
- **ステージクリア時**: 一時リスト → 永続リスト
- **ステージ失敗時**: 一時リストクリア
- **セーブデータ**: JSON形式でローカル保存

## 3. アンロックシステム

### 3.1 アイテムアンロック
#### 3.1.1 アンロック条件
- **敵撃破**: 特定敵の撃破でアイテム取得
- **ステージクリア**: ステージ完走でボーナスアイテム
- **隠し条件**: 特殊条件達成でレアアイテム

#### 3.1.2 アンロック管理
```csharp
// GameManager での管理
List<string> gotItems;              // 永続アンロック済みアイテム
List<string> stageGetNewItems;      // ステージ内新規取得アイテム

// メソッド
void AddItem(string itemId);           // 正式アンロック
void AddNewItemList(string type);     // 一時アンロック
void AddNewItemListToGotItems();      // 一時→正式変換
```

### 3.2 キャラクターアンロック
#### 3.2.1 アンロック条件
- **ステージクリア**: 特定ステージクリアで解放
- **アイテム取得**: 特定アイテム取得で解放
- **スコア達成**: 高スコア達成で解放

#### 3.2.2 アンロック管理
```csharp
// GameManager での管理
List<string> gotCharacters;             // 永続アンロック済みキャラ
List<string> stageGetNewCharacters;     // ステージ内新規取得キャラ

// 確認メソッド
bool IsCharacterUnlocked(string characterId);
bool IsStageUnlocked(string stageId);
```

### 3.3 ステージアンロック
- **gotStages**: アンロック済みステージリスト
- **順次解放**: 前ステージクリアで次ステージ解放
- **分岐解放**: 特定条件で隠しステージ解放

## 4. 進行度管理システム

### 4.1 セーブデータ構造
```csharp
public class SaveData
{
    // ゲーム統計
    int killCount;              // 今回撃破数
    int allKillCount;           // 累計撃破数
    int score;                  // 今回スコア
    int bulletCount;            // 今回弾数
    int allBulletCount;         // 累計弾数
    int itemCount;              // 今回アイテム数
    
    // アンロック情報
    List<string> gotItems;      // アンロック済みアイテム
    List<string> gotCharacters; // アンロック済みキャラクター
    List<string> gotStages;     // アンロック済みステージ
    
    // ハイスコア
    int[] highScores;           // 上位20位のスコア
    
    // 設定情報
    int languageIndex;          // 言語設定
    float globalBgmVol;         // BGM音量
    float globalSeVol;          // SE音量
}
```

### 4.2 進行度表示
- **パーセンテージ**: 総アイテム数に対する取得率
- **実績システム**: 特定条件達成の表示
- **統計情報**: プレイ時間・撃破数・使用弾数

### 4.3 データ同期
- **自動保存**: ステージクリア時・アプリ終了時
- **ロード処理**: ゲーム開始時の自動ロード
- **エラー処理**: セーブデータ破損時の初期化

## 5. バランス調整システム

### 5.1 難易度調整
- **敵HP倍率**: ステージごとの敵HP調整
- **攻撃頻度**: ステージごとの敵攻撃間隔調整
- **アイテム出現率**: ステージごとのアイテムドロップ率

### 5.2 プレイヤーパワー管理
- **powerMagnification**: 全体パワー倍率
- **装備効果**: アイテム装備による能力変化
- **キャラクター補正**: キャラクター固有のパラメータ補正

### 5.3 スコアバランス
- **基本倍率**: 敵HP×100, 弾数×10, アイテム×1000
- **ボーナス倍率**: ノーミス×100,000
- **難易度補正**: 高難易度ステージでのスコア倍率向上