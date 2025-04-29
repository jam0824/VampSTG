using UnityEngine;

public class StageWave : MonoBehaviour
{
    [Header("ウェーブの時間")]
    [SerializeField] public float startWaveTime = 0f;
    [SerializeField] public float endWaveTime = 60f;

    [Header("Enemy Settings")]
    [SerializeField] public GameObject[] enemies;      // スポーンする敵プレハブ
    [SerializeField] public int spawnCount = 1;        // 一度のタイミングでスポーンする数

    [Header("Spawn Timing")]
    [SerializeField] public float initialInterval = 5f;    // ゲーム開始直後のスポーン間隔（秒）
    [SerializeField] public float minInterval = 0.5f;      // 最短スポーン間隔（秒）
    [SerializeField] public float decayRate = 0.05f;       // インターバル減少率（秒／秒）

   
}
