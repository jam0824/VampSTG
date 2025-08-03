using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class Vamp_Hovl_Laser2 : MonoBehaviour
{
    public float laserScale = 1;
    public Color laserColor = new Vector4(1, 1, 1, 1);
    public GameObject HitEffect;
    public GameObject FlashEffect;
    public float HitOffset = 0;

    public float MaxLength;

    private bool UpdateSaver = false;
    private ParticleSystem laserPS;
    private ParticleSystem[] Flash;
    private ParticleSystem[] Hit;
    private Material laserMat;
    private int particleCount;
    private ParticleSystem.Particle[] particles;
    private Vector3[] particlesPositions;
    private float dissolveTimer = 0f;       // ※ 修正
    private bool startDissolve = false;     // ※ 修正
    private BoxCollider beamTrigger;

    void Start()
    {
        laserPS = GetComponent<ParticleSystem>();
        laserMat = GetComponent<ParticleSystemRenderer>().material;
        Flash = FlashEffect.GetComponentsInChildren<ParticleSystem>();
        Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();
        laserMat.SetFloat("_Scale", laserScale);
        // --- トリガー用 BoxCollider のセットアップ ---
        beamTrigger = gameObject.AddComponent<BoxCollider>();
        beamTrigger.isTrigger = true;
        // Z軸方向に伸ばすので、断面は小さく
        beamTrigger.size = new Vector3(0.1f, 1f, 1f);
        beamTrigger.center = new Vector3(0f, 0f, 0.5f); // 長さ 1 の半分
        // Rigidbody は Inspector 上で追加し、IsKinematic=true にしてください
    }

    void Update()
    {
        // レーザー更新が無効化されていなければ
        if (laserPS != null && !UpdateSaver)
        {
            // 1) シェーダに開始点を渡す
            laserMat.SetVector("_StartPoint", transform.position);

            // 2) Raycast でヒット判定
            RaycastHit hit;
            bool hitOccurred = Physics.Raycast(
                transform.position,
                transform.TransformDirection(Vector3.forward),
                out hit,
                MaxLength
            );

            // ヒット距離 or 最大距離
            float distance = hitOccurred ? hit.distance : MaxLength;
            // 終点座標（ヒット点 or 最大到達点）
            Vector3 endPoint = hitOccurred
                ? hit.point
                : transform.position + transform.forward * MaxLength;

            // 3) パーティクル数を距離から計算して配置
            particleCount = Mathf.CeilToInt(distance / (2 * laserScale));
            particlesPositions = new Vector3[particleCount];
            AddParticles();

            // 4) シェーダに距離・終点を渡す
            laserMat.SetFloat("_Distance", distance);
            laserMat.SetVector("_EndPoint", endPoint);

            // 5) BoxCollider（トリガー）の長さと中心を更新
            if (beamTrigger != null)
            {
                // Z方向をdistance分伸ばす
                beamTrigger.size = new Vector3(beamTrigger.size.x, beamTrigger.size.y, distance);
                // ローカル座標でビームの前方半分の位置に中心をずらす
                beamTrigger.center = new Vector3(0f, 0f, distance * 0.5f);
            }

            // 6) ヒット／フラッシュエフェクトの再生 or 停止
            if (hitOccurred)
            {
                // ヒット位置にエフェクトを移動・向きを合わせる
                HitEffect.transform.position = hit.point + hit.normal * HitOffset;
                HitEffect.transform.LookAt(hit.point);
                // 再生
                foreach (var ps in Hit) if (!ps.isPlaying) ps.Play();
                foreach (var ps in Flash) if (!ps.isPlaying) ps.Play();
            }
            else
            {
                // 衝突なし → ヒットエフェクト停止
                HitEffect.transform.position = endPoint;
                foreach (var ps in Hit)
                    if (ps.isPlaying) ps.Stop();
            }
        }

        // 7) 溶解フェードアウト処理（DisablePrepare() 呼び出し後に走る）
        if (startDissolve)                   // ※ 修正
        {
            dissolveTimer += Time.deltaTime; // ← これでちゃんと存在する変数になります
            float dissolveValue = dissolveTimer * 5f;      // シェーダ側の _Dissolve 値
            laserMat.SetFloat("_Dissolve", dissolveValue);

            // 8) 溶解完了判定（_Dissolve が 1.0 以上になったら破棄）
            if (dissolveValue >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    void AddParticles()
    {
        //Old particles settings
        /*
        var normalDistance = particleCount;
        var sh = LaserPS.shape;
        sh.radius = normalDistance;
        sh.position = new Vector3(0,0, normalDistance);
        LaserPS.emission.SetBursts(new[] { new ParticleSystem.Burst(0f, particleCount + 1) });
        */

        particles = new ParticleSystem.Particle[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            particlesPositions[i] = new Vector3(0f, 0f, 0f) + new Vector3(0f, 0f, i * 2 * laserScale);
            particles[i].position = particlesPositions[i];
            particles[i].startSize3D = new Vector3(0.001f, 0.001f, 2 * laserScale);
            particles[i].startColor = laserColor;
        }
        laserPS.SetParticles(particles, particles.Length);
    }

    public void DisablePrepare()
    {
        transform.parent = null;
        dissolveTimer = 0f;                  // ※ 修正
        startDissolve = true;                // ※ 修正
        UpdateSaver = true;
        if (Flash != null && Hit != null)
        {
            foreach (var AllHits in Hit)
            {
                if (AllHits.isPlaying) AllHits.Stop();
            }
            foreach (var AllFlashes in Flash)
            {
                if (AllFlashes.isPlaying) AllFlashes.Stop();
            }
        }
    }

}
