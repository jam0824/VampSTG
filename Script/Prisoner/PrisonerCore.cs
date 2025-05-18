using UnityEngine;
using UnityEngine.Rendering;  // BlendMode を使うために追加

public class PrisonerCore : MonoBehaviour
{
    [SerializeField] public float hp = 10;
    private float maxHp = 10;
    [SerializeField] float offsetExplosionY = 0f;

    [Header("移動設定")]
    [Tooltip("上下あわせた移動にかける総時間（秒）")]
    public float totalDuration = 10f;
    [Tooltip("頂点（最高点）の高さ（ワールド単位）")]
    public float peakHeight = 2f;

    Transform playerTransform;
    bool isDead = false;


    private float _elapsed = 0;
    private Vector3 _startPos;

    void Start()
    {
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        maxHp = hp;
    }

    void Update()
    {
        if (playerTransform == null) return;

        if ((!isDead) && (hp <= 0)) enemyDie();

        if (isDead)
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / totalDuration;
            // 放物線（頂点 at t=0.5）: y = -4*(t-0.5)^2 + 1
            float parabola = -4f * (t - 0.5f) * (t - 0.5f) + 1f;
            float yOffset = parabola * peakHeight;

            transform.position = _startPos + Vector3.up * yOffset;

            //画面の下までいったら消える。カメラ外時に処理しないのは上に飛んで画面外になることもあるため下限で処理
            if (transform.position.y < (GameManager.Instance.minY - 1f))
                Destroy(gameObject);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerBullet"))
        {
            if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;
            hp = hit(bullet, hp);
            // 近似的に当たり位置を計算
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            if (bullet.triggerEffect != null)
            {
                Instantiate(bullet.triggerEffect, hitPoint, other.gameObject.transform.rotation);
            }
            if (bullet.isDestroy) Destroy(other.gameObject);
        }
        if (hp <= 0) enemyDie();
    }

    float hit(ConfigPlayerBullet bullet, float enemyHp)
    {
        enemyHp -= bullet.getDamage();
        AudioClip hitSe = bullet.hitSe;
        if (hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie()
    {
        isDead = true;

        // ─── ここから透明化処理 ───────────────────────────
        // MeshRenderer / SkinnedMeshRenderer の両方に対応させたい場合は
        // GetComponentsInChildren<Renderer>() を使っても良いです
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            MakeMaterialTransparent(rend.material);
        }
        // ───────────────────────────────────────────────────

        transform.parent = null;    // 親を外す
        _startPos = transform.position;
        Vector3 pos = transform.position;
        if (offsetExplosionY != 0) pos.y += offsetExplosionY;
        EffectController.Instance.PlayLargeExplosion(pos, gameObject.transform.rotation);
        AddScore(maxHp);
    }

    // 標準シェーダーのマテリアルを Fade モードに切り替えて完全透明にするヘルパー
    void MakeMaterialTransparent(Material mat)
    {
        // 1) 描画モードを Fade に
        mat.SetFloat("_Mode", 2);  // 0=Opaque, 1=Cutout, 2=Fade, 3=Transparent
        mat.SetOverrideTag("RenderType", "Transparent");
        // 2) ブレンド設定
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)RenderQueue.Transparent;
        // 3) カラーアルファを 0 に
        Color c = mat.color;
        c.a = 0f;
        mat.color = c;
    }

    void AddScore(float maxHp)
    {
        GameManager.Instance.AddScore(maxHp);
    }

}
