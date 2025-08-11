using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 爆弾虫 - 死亡時に赤く点滅してから爆発する敵
/// </summary>
public class BomberFly : BaseEnemy
{
    [Header("移動設定")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float rotateSpeed = 90f;   // 度/秒
    [SerializeField] float stopDistance = 0.1f;
    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 2f;
    [SerializeField] private float initialFlashInterval = 0.5f;
    [SerializeField] private float finalFlashInterval = 0.1f;
    [SerializeField] private Color flashColor = Color.red;
    [Header("警告音")]
    [SerializeField] AudioClip warningSound;
    [SerializeField] float warningSoundVolume = 0.5f;
    [Header("ライト設定")]
    [SerializeField] private Light flashLight;

    private Material[] listMaterials;
    private Color[] listOriginalColors;
    private Coroutine flashCoroutine;
    private bool isFlashing = false;

    protected override void OnStart()
    {
        // 子オブジェクトのマテリアルを全て取得
        CollectMaterials();
        
        // ライトを初期状態でOFFにする
        if (flashLight != null)
        {
            flashLight.enabled = false;
        }
    }

    /// <summary>
    /// 子オブジェクトのマテリアルを収集し、元の色を保存
    /// </summary>
    private void CollectMaterials()
    {
        var listMaterialTemp = new List<Material>();
        var listOriginalColorTemp = new List<Color>();
        
        // 通常のRenderer（MeshRenderer等）を取得
        var listRenderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < listRenderers.Length; i++)
        {
            var materials = listRenderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] != null)
                {
                    listMaterialTemp.Add(materials[j]);
                    
                    // 元の色を保存
                    Color originalColor = Color.white;
                    if (materials[j].HasProperty("_BaseColor"))
                    {
                        originalColor = materials[j].GetColor("_BaseColor");
                    }
                    else if (materials[j].HasProperty("_Color"))
                    {
                        originalColor = materials[j].GetColor("_Color");
                    }
                    listOriginalColorTemp.Add(originalColor);
                }
            }
        }
        
        // SkinnedMeshRendererも明示的に取得
        var listSkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < listSkinnedMeshRenderers.Length; i++)
        {
            var materials = listSkinnedMeshRenderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] != null && !listMaterialTemp.Contains(materials[j]))
                {
                    listMaterialTemp.Add(materials[j]);
                    
                    // 元の色を保存
                    Color originalColor = Color.white;
                    if (materials[j].HasProperty("_BaseColor"))
                    {
                        originalColor = materials[j].GetColor("_BaseColor");
                    }
                    else if (materials[j].HasProperty("_Color"))
                    {
                        originalColor = materials[j].GetColor("_Color");
                    }
                    listOriginalColorTemp.Add(originalColor);
                }
            }
        }
        
        listMaterials = listMaterialTemp.ToArray();
        listOriginalColors = listOriginalColorTemp.ToArray();
    }

    protected override void HandleMovement()
    {
        // 点滅中は移動を停止
        if (isFlashing) return;
        
        if (playerTransform == null) return;

        // ─── 移動 ───
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.x = 0f;
        float dist = toPlayer.magnitude;
        if (dist > stopDistance)
        {
            float step = moveSpeed * Time.deltaTime;
            Vector3 stepPos = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            stepPos.x = 0f;
            transform.position = stepPos;
        }

        // ─── 常にプレイヤー方向を向く ───
        if (toPlayer.sqrMagnitude > 0.001f) // プレイヤーと同位置だとQuaternion.LookRotationでエラーになるので念のため
        {
            // y成分を無視して水平方向だけで向きを計算
            Vector3 dir = toPlayer.normalized;

            // 目標回転
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // スムーズに回転
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    protected override void enemyDie()
    {
        if (isFlashing) return; // 既に点滅中の場合は重複実行を防ぐ
        
        isDead = true;
        
        // 移動と攻撃を停止
        isAttack = false;
        StopAllCoroutines();

        // 警告音を再生
        if (warningSound != null)
        {
            SoundManager.Instance.PlaySE(warningSound, warningSoundVolume);
        }
        
        // 点滅演出を開始
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashAndExplodeCoroutine());
    }

    /// <summary>
    /// 赤く点滅してから爆発するコルーチン
    /// </summary>
    private IEnumerator FlashAndExplodeCoroutine()
    {
        isFlashing = true;
        float elapsed = 0f;
        bool isFlashOn = false;
        
        while (elapsed < flashDuration)
        {
            // 点滅間隔を徐々に短くする
            float t = elapsed / flashDuration;
            float currentInterval = Mathf.Lerp(initialFlashInterval, finalFlashInterval, t);
            
                         // 色を切り替え
             isFlashOn = !isFlashOn;
             SetMaterialColors(isFlashOn ? flashColor : Color.white);
             
             // ライトも連動してON/OFF
             if (flashLight != null)
             {
                 flashLight.enabled = isFlashOn;
             }
            
            yield return new WaitForSeconds(currentInterval);
            elapsed += currentInterval;
        }
        
        // 元の色に戻す
        RestoreOriginalColors();
        
        // ライトをOFFにする
        if (flashLight != null)
        {
            flashLight.enabled = false;
        }
        
        // 爆発処理
        Explosion(maxHp);
        AddKillCount();
        AddScore(maxHp);
        ApearItem(item);
        Destroy(gameObject);
    }

    /// <summary>
    /// 全マテリアルの色を設定
    /// </summary>
    private void SetMaterialColors(Color color)
    {
        if (listMaterials == null) return;
        
        for (int i = 0; i < listMaterials.Length; i++)
        {
            var mat = listMaterials[i];
            if (mat == null) continue;
            
            if (mat.HasProperty("_BaseColor"))
            {
                var originalColor = listOriginalColors[i];
                var flashColor = new Color(color.r, color.g, color.b, originalColor.a);
                mat.SetColor("_BaseColor", flashColor);
            }
            else if (mat.HasProperty("_Color"))
            {
                var originalColor = listOriginalColors[i];
                var flashColor = new Color(color.r, color.g, color.b, originalColor.a);
                mat.SetColor("_Color", flashColor);
            }
        }
    }

    /// <summary>
    /// 元の色に戻す
    /// </summary>
    private void RestoreOriginalColors()
    {
        if (listMaterials == null || listOriginalColors == null) return;
        
        for (int i = 0; i < listMaterials.Length && i < listOriginalColors.Length; i++)
        {
            var mat = listMaterials[i];
            if (mat == null) continue;
            
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", listOriginalColors[i]);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", listOriginalColors[i]);
            }
        }
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        // ライトをOFFにする
        if (flashLight != null)
        {
            flashLight.enabled = false;
        }
    }

    /// <summary>
    /// 点滅を強制停止する（デバッグ用）
    /// </summary>
    public void StopFlashing()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        isFlashing = false;
        RestoreOriginalColors();
        
        // ライトをOFFにする
        if (flashLight != null)
        {
            flashLight.enabled = false;
        }
    }

    /// <summary>
    /// 現在点滅中かどうかを取得
    /// </summary>
    public bool IsFlashing()
    {
        return isFlashing;
    }
}
