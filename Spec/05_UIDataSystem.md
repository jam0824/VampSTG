# UI・データ管理システム詳細仕様書

## 1. GameManager システム

### 1.1 GameManager 基本仕様
```csharp
public class GameManager : MonoBehaviour
{
    // システム設定
    int languageIndex;              // 言語設定（0:英語, 1:日本語）
    int m_targetFrameRate;          // フレームレート設定
    CharacterData selectedCharacter; // 選択キャラクター
    StageData selectedStage;        // 選択ステージ
    
    // 音量設定
    float globalBgmVol;             // BGM音量（デフォルト:0.6）
    float globalSeVol;              // SE音量（デフォルト:1.0）
    
    // 移動範囲制限
    float maxZ, minZ;               // Z軸移動範囲（-9〜9）
    float maxY, minY;               // Y軸移動範囲（-4〜6）
    
    // スコア設定
    int bulletMagnification;        // 弾数倍率（×10）
    float scoreMagnification;       // スコア倍率（×100）
    int itemMagnification;          // アイテム倍率（×1000）
    int noMissBonus;               // ノーミスボーナス（100,000）
}
```

### 1.2 シングルトンパターン
- **Instance**: 静的インスタンス管理
- **DontDestroyOnLoad**: シーン跨ぎでの永続化
- **重複防止**: 既存インスタンスがある場合は削除

### 1.3 フレームレート管理
- **SetTargetFrameRate()**: フレームレート設定メソッド
- **SetFrameRate(int)**: 動的フレームレート変更
- **デフォルト値**: 60FPS

## 2. UI システム

### 2.1 メニューシステム
#### 2.1.1 CharacterSelectController
- **キャラクター選択**: 3キャラクターから選択
- **プレビュー表示**: 3Dモデルプレビュー
- **パラメータ表示**: Life/Power/Speed/PickupRange
- **PowerStars表示**: Power値の星表示システム
- **アンロック管理**: 未解放キャラクターのロック表示

#### 2.1.2 StageSelectController  
- **ステージ選択**: 6ステージ + テストステージ
- **プレビュー画像**: ステージのスクリーンショット表示
- **難易度表示**: 難易度パラメータの可視化
- **アンロック管理**: 未解放ステージのロック表示
- **ミッション説明**: ステージの説明文表示

### 2.2 ゲーム内UI
#### 2.2.1 HUD（Head-Up Display）
- **HP表示**: プレイヤーの現在HP
- **スコア表示**: リアルタイムスコア更新
- **弾数カウンター**: 発射弾数表示
- **アイテム数**: 取得アイテム数表示
- **パワーゲージ**: 現在のパワー倍率表示

#### 2.2.2 ポーズメニュー
- **ゲーム一時停止**: Time.timeScale = 0
- **設定変更**: 音量・言語設定
- **リスタート**: ステージ最初から
- **メインメニュー**: タイトル画面への復帰

### 2.3 リザルト画面
#### 2.3.1 スコア表示
- **総合スコア**: 最終スコア計算・表示
- **内訳表示**: 撃破/弾数/アイテム/ボーナス別
- **ランキング**: ハイスコア順位表示
- **新記録**: ハイスコア更新時の演出

#### 2.3.2 統計情報
- **撃破数**: killCount / allKillCount
- **使用弾数**: bulletCount / allBulletCount  
- **取得アイテム**: itemCount
- **死亡回数**: stageDeadCount
- **プレイ時間**: stageAllSecond

### 2.4 設定画面
#### 2.4.1 音響設定
- **BGM音量**: スライダーによる調整（0.0〜1.0）
- **SE音量**: スライダーによる調整（0.0〜1.0）
- **音声プレビュー**: 設定変更時のサンプル再生

#### 2.4.2 システム設定
- **言語切り替え**: 英語/日本語の切り替え
- **フレームレート**: 30/60/120FPS選択
- **操作設定**: キーバインド変更（将来拡張）

## 3. データ管理システム

### 3.1 セーブ・ロードシステム
#### 3.1.1 保存形式
- **ファイル形式**: JSON
- **保存場所**: Application.persistentDataPath + "/saveData.json"
- **自動保存**: ステージクリア時・アプリ終了時

#### 3.1.2 セーブデータ構造
```json
{
    "killCount": 0,
    "allKillCount": 0,
    "score": 0,
    "bulletCount": 0,
    "allBulletCount": 0,
    "itemCount": 0,
    "gotItems": [],
    "gotCharacters": [],
    "gotStages": [],
    "highScores": [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
    "languageIndex": 0,
    "globalBgmVol": 0.6,
    "globalSeVol": 1.0
}
```

#### 3.1.3 エラー処理
- **ファイル不存在**: 初期データでの起動
- **JSON解析エラー**: ログ出力・初期化
- **保存失敗**: エラーログ・リトライ機構

### 3.2 多言語システム
#### 3.2.1 言語データ管理
- **characterName[]**: キャラクター名（[0]:英語, [1]:日本語）
- **description[]**: 説明文（多言語配列）
- **missionTitle[]**: ミッション名（多言語配列）

#### 3.2.2 言語切り替え
- **languageIndex**: 0=英語, 1=日本語
- **リアルタイム切り替え**: ゲーム中の即座反映
- **フォント対応**: 日本語フォント（NotoSansJP）

### 3.3 ハイスコアシステム
#### 3.3.1 スコア管理
- **配列サイズ**: int[20] で上位20位まで記録
- **ソート**: 降順での自動ソート
- **重複許可**: 同一スコアの複数記録可能

#### 3.3.2 スコア更新処理
```csharp
// スコア登録フロー
1. 現在スコアとハイスコア配列比較
2. 更新対象の場合、配列に挿入
3. 降順ソート実行
4. 配列サイズを20に制限
5. セーブデータ更新
```

## 4. 進行管理システム

### 4.1 ステージ進行管理
#### 4.1.1 ステージ開始処理
```csharp
void ResetStageSaveData()
{
    killCount = 0;
    score = 0;
    bulletCount = 0;
    itemCount = 0;
    stageDeadCount = 0;
    stageGetNewItems.Clear();
    stageGetNewCharacters.Clear();
    stageAllEnemyCount = 0;
    stageAllItemCount = 0;
    stageAllHp = 0f;
}
```

#### 4.1.2 リアルタイム統計
- **AddStageAllEnemyCount()**: 敵数カウント・密度計算
- **AddStageAllItemCount()**: アイテム数カウント・密度計算
- **AddStageAllHp(float)**: HP累計・密度計算

### 4.2 アンロック進行管理
#### 4.2.1 一時アンロック
- **AddNewItemList(string)**: 一時アイテムアンロック
- **AddNewCharacterList(string)**: 一時キャラクターアンロック
- **重複チェック**: 既存リストとの重複防止

#### 4.2.2 永続化処理
- **AddNewItemListToGotItems()**: アイテム正式アンロック
- **AddNewCharacterListToGotCharacters()**: キャラクター正式アンロック
- **ステージクリア時**: 一時 → 永続の変換実行

### 4.3 死亡・リスタート管理
- **stageDeadCount**: ステージ内死亡回数
- **whenDeathToSceneName**: 死亡時復帰シーン（デフォルト: CharacterSelect）
- **ノーミスボーナス**: 死亡回数0での高額ボーナス

## 5. パフォーマンス・最適化

### 5.1 オブジェクトプーリング
- **EffectController**: エフェクト・弾薬のプール管理
- **DeactivateAllPooledBullets()**: 全弾薬の一括非アクティブ化
- **FindInactivePooledObject()**: 非アクティブオブジェクトの検索・再利用

### 5.2 メモリ管理
- **DontDestroyOnLoad**: 必要最小限のオブジェクトのみ永続化
- **適切な破棄**: シーン切り替え時の不要オブジェクト削除
- **リソース最適化**: テクスチャ・オーディオの圧縮設定

### 5.3 処理最適化
- **フレームレート制御**: Application.targetFrameRate
- **更新頻度制御**: 必要時のみの Update() 処理
- **キャッシュ活用**: 頻繁アクセスするコンポーネントのキャッシュ

## 6. デバッグ・開発支援

### 6.1 デバッグ出力
- **Debug.Log**: ゲーム状況の詳細ログ
- **密度計算ログ**: ステージバランス確認用
- **エラーハンドリング**: Debug.LogError での例外記録

### 6.2 テストサポート
- **testScene**: 開発用テストステージ
- **統計データ**: リアルタイムバランス確認
- **チート機能**: 開発用の特殊機能（リリース時削除）

### 6.3 設定の柔軟性
- **Inspector編集**: 全パラメータのInspector調整可能
- **ScriptableObject**: データの外部化・管理容易性
- **設定継承**: 基底クラスでの共通設定管理