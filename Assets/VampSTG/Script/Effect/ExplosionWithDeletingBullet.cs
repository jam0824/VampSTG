using UnityEngine;

public class ExplosionWithDeletingBullet : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemyBullet"))
        {
            ConfigEnemyBullet configEnemyBullet = other.GetComponent<ConfigEnemyBullet>();
            if (configEnemyBullet != null)
            {
                configEnemyBullet.DestroyBullet();
            }
        }
        else if (other.gameObject.CompareTag("PlayerBullet"))
        {
            other.gameObject.SetActive(false);
        }
    }
}
