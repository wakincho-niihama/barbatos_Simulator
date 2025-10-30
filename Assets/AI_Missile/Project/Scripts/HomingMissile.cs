//参考: https://kurokumasoft.com/2022/05/21/unity-homing-missile/
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingMissile : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // プレイヤーが発射時に設定する

    [Header("Missile")]
    public float speed = 15f;          // ミサイルの速度
    public float turnSpeed = 8f;       // ミサイルが曲がる速度
    public float lifeTime = 10f;       // 最大生存時間 (秒)

    [Header("Explosion Effect")]
    public GameObject explosionPrefab; // 爆発プレハブ

    private Rigidbody rb;

    private Transform effectContainer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // n秒後にターゲットがいてもいなくても自爆する
        Destroy(gameObject, lifeTime);

        // ゲーム開始時に「_EffectContainer」を名前で検索して保持する
        GameObject containerGO = GameObject.Find("_EffectContainer");
        if (containerGO != null)
        {
            effectContainer = containerGO.transform;
        }
    }

    void FixedUpdate()
    {
        // ターゲットがいない場合
        if (target == null)
        {
            // まっすぐ飛ぶだけ
            rb.linearVelocity = transform.forward * speed;
        }
        else // ターゲットがいる場合
        {
            // 1. ターゲットへの方向を計算
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // 2. ターゲットの方向を向くための回転を計算
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // 3. 現在の向きからターゲットの向きへ、Slerpを使ってスムーズに回転する
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));

            // 4. ミサイルの「前方」に向かって推進力を与え続ける
            rb.linearVelocity = transform.forward * speed;
        }
    }

    // 何かに衝突したとき
    void OnCollisionEnter(Collision collision)
    {
        // 爆発エフェクトを「衝突した場所」に生成する
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity, effectContainer);
        }

        // ミサイル自体を消滅させる
        Destroy(gameObject);
    }
}