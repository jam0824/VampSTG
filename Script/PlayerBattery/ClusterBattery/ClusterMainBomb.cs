using UnityEngine;
using System.Collections;

/// <summary>
/// クラスター爆弾のメイン本体
/// 落下→前方加速→外装分離→弾散布の一連の動作を行う
/// </summary>
public class ClusterMainBomb : MonoBehaviour
{
    [Header("外装prefab")]
    [SerializeField] private GameObject outerShellPrefab; // 分離する外装のprefab
    
    [Header("子弾prefab")]
    [SerializeField] private GameObject bulletPrefab; // 散布する子弾のprefab

    [Header("バックファイア")]
    [SerializeField] private GameObject backFirePrefab; // バックファイアのprefab
    [SerializeField] private Transform backFirePoint; // バックファイアの発射位置
    [SerializeField] private AudioClip backFireSe;
    [SerializeField] private float backFireSeVolume = 0.8f;
    [Header("動作設定")]
    [SerializeField] private float dropDuration = 0.1f; // 重力での落下時間（秒）
    [SerializeField] private float forwardAcceleration = 5f; // 前方への加速度
    [SerializeField] private float maxForwardSpeed = 10f; // 前方への最大速度制限
    [SerializeField] private float yDecelerationRate = 8f; // Y軸速度の減速率（落下を止める）
    [SerializeField] private float upwardForce = 2f; // 上向きの浮上力
    [SerializeField] private float shellEjectDelay = 0.2f; // 外装分離までの遅延時間（秒）
    [SerializeField] private float shellEjectForce = 10f; // 外装を吹き飛ばす力の強さ
    [SerializeField] private float shellDestroyDelay = 2f; // 外装を削除するまでの時間（秒）
    [SerializeField] private float shellRotationForce = 5f; // 外装の回転力（前転回転）
    [SerializeField] private float rotationSpeed = 5f; // ミサイル本体の回転速度（未使用）
    
    [Header("弾射出設定")]
    [SerializeField] private float bulletShootInterval = 0.1f; // 弾射出の間隔（秒）
    [SerializeField] private float bulletSpeed = 10f; // 弾の射出速度
    [SerializeField] private float bulletAngleVariation = 10f; // 弾射出角度のバリエーション（度）
    [SerializeField] public int bulletRound = 1; // 1回の射出で発射する弾数
    
    public float damage = 5f; // 子弾のダメージ値
    
    // コンポーネント参照とフラグ
    private Rigidbody rigidBody; // 物理演算用のRigidbody
    private bool isAccelerating = false; // 前方加速中フラグ
    private bool isShooting = false; // 弾射出中フラグ
    
    /// <summary>
    /// 初期化処理
    /// Rigidbodyの設定と落下→加速のコルーチンを開始
    /// </summary>
    void Start()
    {
        // Rigidbodyコンポーネントの取得または追加
        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
        }
        
        // 最初は重力を有効にして落下開始
        rigidBody.useGravity = true;
        
        // 落下→加速のコルーチンを開始
        StartCoroutine(DropAndAccelerateCoroutine());
    }

    /// <summary>
    /// フレーム毎の更新処理
    /// 前方加速とY軸減速の物理制御を行う
    /// </summary>
    void Update()
    {
        stopShooting();
        // 前方加速中の処理
        if (isAccelerating)
        {
            Vector3 currentVelocity = rigidBody.linearVelocity;
            
            // 前方向に徐々に加速
            Vector3 forwardForce = transform.forward * forwardAcceleration;
            rigidBody.AddForce(forwardForce, ForceMode.Acceleration);
            
            // 少し上向きの力を加える（ググググっと上昇する効果）
            Vector3 upwardForceVector = Vector3.up * upwardForce;
            rigidBody.AddForce(upwardForceVector, ForceMode.Acceleration);
            
            // Y軸速度を徐々に0に近づける（落下を徐々に止める）
            float newYVelocity = Mathf.Lerp(currentVelocity.y, 0f, yDecelerationRate * Time.deltaTime);
            rigidBody.linearVelocity = new Vector3(currentVelocity.x, newYVelocity, currentVelocity.z);
            
            // 最大速度制限（前方向のみに適用）
            Vector3 horizontalVelocity = new Vector3(rigidBody.linearVelocity.x, 0, rigidBody.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxForwardSpeed)
            {
                Vector3 limitedHorizontalVelocity = horizontalVelocity.normalized * maxForwardSpeed;
                rigidBody.linearVelocity = new Vector3(limitedHorizontalVelocity.x, rigidBody.linearVelocity.y, limitedHorizontalVelocity.z);
            }
        }
    }
    
    /// <summary>
    /// 落下→加速の一連の動作を制御するコルーチン
    /// </summary>
    private IEnumerator DropAndAccelerateCoroutine()
    {
        // 指定時間だけ重力で落下
        yield return new WaitForSeconds(dropDuration);
        
        // 重力を無効化して制御された動きに移行
        rigidBody.useGravity = false;
        
        // 前方加速開始フラグを立てる（UpdateでY軸速度は徐々に減速）
        isAccelerating = true;
        
        // 外装分離のコルーチンを開始
        StartCoroutine(EjectOuterShellCoroutine());
        // バックファイアを発射
        Quaternion rotationWithOffset = backFirePoint.rotation * Quaternion.Euler(180, 0, 0);
        GameObject backFire = Instantiate(backFirePrefab, backFirePoint.position, rotationWithOffset);
        backFire.transform.SetParent(gameObject.transform);
        SoundManager.Instance.PlaySE(backFireSe, backFireSeVolume);
    }
    
    /// <summary>
    /// 外装分離のタイミングを制御するコルーチン
    /// </summary>
    private IEnumerator EjectOuterShellCoroutine()
    {
        // 前方加速開始から指定時間待機
        yield return new WaitForSeconds(shellEjectDelay);
        
        // 外装分離処理を実行
        EjectOuterShell();
    }
    
    /// <summary>
    /// 外装分離処理
    /// 外装を後方に吹き飛ばし、弾射出を開始する
    /// </summary>
    private void EjectOuterShell()
    {
        if (outerShellPrefab != null)
        {
            // 子オブジェクトから分離（独立したオブジェクトにする）
            outerShellPrefab.transform.SetParent(null);
            
            // Rigidbodyを取得または追加
            Rigidbody shellRigidbody = outerShellPrefab.GetComponent<Rigidbody>();
            if (shellRigidbody == null)
            {
                shellRigidbody = outerShellPrefab.AddComponent<Rigidbody>();
            }
            
            // 後ろ向きの力を加える（本体と逆方向に分離）
            Vector3 backwardForce = -transform.forward * shellEjectForce;
            shellRigidbody.AddForce(backwardForce, ForceMode.Impulse);
            
            // 少し上向きの力も加えて自然な軌道にする
            Vector3 upwardEjectForce = Vector3.up * (shellEjectForce * 0.3f);
            shellRigidbody.AddForce(upwardEjectForce, ForceMode.Impulse);
            
            // x軸で上向きの回転を加える（前転回転効果）
            Vector3 rotationTorque = Vector3.right * shellRotationForce;
            shellRigidbody.AddTorque(rotationTorque, ForceMode.Impulse);
            
            // 指定時間後に外装を削除するコルーチンを開始
            StartCoroutine(DestroyShellCoroutine(outerShellPrefab));
        }
        
        // 外装分離後、弾射出を開始
        isShooting = true;
        StartCoroutine(ShootBulletsCoroutine());
    }
    
    /// <summary>
    /// 外装オブジェクトを遅延削除するコルーチン
    /// </summary>
    /// <param name="shell">削除対象の外装オブジェクト</param>
    private IEnumerator DestroyShellCoroutine(GameObject shell)
    {
        // 指定時間待機
        yield return new WaitForSeconds(shellDestroyDelay);
        
        // 外装オブジェクトを削除
        if (shell != null)
        {
            Destroy(shell);
        }
    }
    
    /// <summary>
    /// 弾射出を継続するコルーチン
    /// 画面端まで達するか、OnBecameInvisibleが呼ばれるまで継続
    /// </summary>
    private IEnumerator ShootBulletsCoroutine()
    {
        while (isShooting)
        {
            
            // 指定回数分の弾を連続発射
            for(int i = 0; i < bulletRound; i++){
                ShootBullets();
            }
            
            // 次の射出まで待機
            yield return new WaitForSeconds(bulletShootInterval);
        }
    }
    
    /// <summary>
    /// 1発の弾を射出する処理
    /// 下方向にランダムな角度で弾を発射し、ダメージ値を設定
    /// </summary>
    private void ShootBullets()
    {
        if (bulletPrefab != null)
        {
            // 下方向±指定角度のランダムな方向を計算
            float randomXAngle = Random.Range(-bulletAngleVariation, bulletAngleVariation);
            float randomZAngle = Random.Range(-bulletAngleVariation, bulletAngleVariation);
            
            // 下方向を基準にランダムな角度を追加
            Vector3 shootDirection = Quaternion.Euler(randomXAngle, 0f, randomZAngle) * Vector3.down;
            
            // 弾オブジェクトを生成
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(shootDirection));
            
            // 弾のダメージ値を設定
            ConfigPlayerBullet configPlayerBullet = bullet.GetComponent<ConfigPlayerBullet>();
            configPlayerBullet.damage = damage;
            
            // Rigidbodyを取得または追加
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            if (bulletRigidbody == null)
            {
                bulletRigidbody = bullet.AddComponent<Rigidbody>();
            }
            
            // 計算した方向に弾を射出
            bulletRigidbody.linearVelocity = shootDirection * bulletSpeed;
        }
    }

    private void stopShooting(){
        // 画面端（maxZ）を超えた場合は射出を停止
        if(transform.position.z > GameManager.Instance.maxZ || 
            transform.position.z < GameManager.Instance.minZ){
            isShooting = false;
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// カメラの視界外になった時に呼ばれるUnityのコールバック
    /// 弾射出を停止し、オブジェクトを削除する
    /// </summary>
    void OnBecameInvisible()
    {
        isShooting = false; // 弾射出を停止
        Destroy(gameObject); // オブジェクト削除
    }
}
