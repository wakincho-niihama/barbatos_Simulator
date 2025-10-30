using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    // 爆発エフェクトが残る時間（この時間だけダメージ判定が発生する）
    public float explosionLifeTime = 0.5f;

    void Start()
    {
        // n秒後にこの爆発オブジェクト自体を消滅させる
        Destroy(gameObject, explosionLifeTime);
    }

    // このトリガー（爆発範囲）に何かが入ってきたら呼び出される
    void OnTriggerEnter(Collider other)
    {
        // 1. 相手が「Indestructible」コンポーネントを持っているか確認
        if (other.GetComponent<Indestructible>() != null)
        {
            // 持っていたら、破壊せずに処理を中断する
            return;
        }

        // 2. Indestructible を持っていない相手だけ、タグをチェック
        // 接触した相手が "Enemy" タグを持っていたら
        if (other.CompareTag("Enemy"))
        {
            // 敵オブジェクトを消滅させる
            Destroy(other.gameObject);
        }
    }
}