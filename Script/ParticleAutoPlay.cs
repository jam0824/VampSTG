using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAutoPlay : MonoBehaviour
{
    ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        // Awake 時点で GameObject がアクティブなら不要ですが、
        // 安全のため Start() でも Play() を呼ぶようにしておくと確実です
    }

    void Start()
    {
        if (!ps.isPlaying)
            ps.Play();
    }
}
