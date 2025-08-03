# VampSTG Tests

このフォルダには VampSTG プロジェクトの全てのテストファイルが含まれています。

## フォルダ構成

- `GameManagerFpsTests.cs` - GameManager の FPS 機能テスト
- `BaseEnemyUnitTests.cs` - BaseEnemy のユニットテスト
- `BaseEnemyIntegrationTestsRefactored.cs` - BaseEnemy の統合テスト
- `Stage4MidBossTests.cs` - Stage4MidBoss のテスト
- `SimpleTest.cs` - 基本機能のテスト
- `TestHelpers/` - テスト用のヘルパークラス
- `TestCoverage.md` - テストカバレッジの詳細

## テスト実行方法

1. Unity エディタで `Window > General > Test Runner` を開く
2. `EditMode` タブを選択
3. 実行したいテストを選択して実行

## 注意事項

- このフォルダはプロジェクトルートに配置されており、ビルド時には含まれません
- 新しいテストを追加する際は、適切な namespace `VampSTG.Tests` を使用してください