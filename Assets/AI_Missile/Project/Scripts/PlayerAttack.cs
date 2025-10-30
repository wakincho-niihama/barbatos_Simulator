using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Missile")]
    public GameObject missilePrefab;    // ミサイルプレハブ
    public Transform firePoint_R;       // 右の発射位置
    public Transform firePoint_L;       // 左の発射位置
    public float fireRate = 0.5f;       // 発射間隔

    [Header("LaunchAngle")]
    public float minAngleMagnitude = 10f; // 発射角度の「最小値」
    public float maxAngleMagnitude = 30f; // 発射角度の「最大値」

    [Header("Spawn")]
    public Transform missileContainer;

    [Header("Target")]
    public Camera mainCamera;

    private float nextFireTime = 0f;
    private bool fireFromRightSide = true; // 交互撃ちのためのフラグ

    void Update()
    {
        // マウス左クリックを「押している間」
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    void Fire()
    {
        // 1. ターゲットを探す
        Transform target = FindClosestVisibleEnemy();

        Transform currentFirePoint;
        Quaternion launchRotation; // 発射する最終的な角度を格納する変数

        // 2. 発射地点を決定
        if (fireFromRightSide)
        {
            currentFirePoint = firePoint_R;
        }
        else
        {
            currentFirePoint = firePoint_L;
        }

        // 3. ターゲットの有無で、発射角度を変える
        if (target != null)
        {
            // --- ターゲットがいる場合 ---

            // 発射するランダムな「角度の大きさ」を計算
            float randomMagnitude = Random.Range(minAngleMagnitude, maxAngleMagnitude);
            // 左右の符号を決定
            float currentAngle = fireFromRightSide ? randomMagnitude : -randomMagnitude;

            // 'currentFirePoint' の基本の向きを取得
            Quaternion baseRotation = currentFirePoint.rotation;
            // Y軸周りに 'currentAngle' 度だけ回転
            launchRotation = baseRotation * Quaternion.Euler(0, currentAngle, 0);
        }
        else
        {
            // --- ターゲットがいない場合 (カメラの真正面) ---

            // ミサイルの向きを、プレイヤーのカメラの向きと同一にする
            launchRotation = mainCamera.transform.rotation;
        }


        // 4. ミサイルを「currentFirePoint の位置」に「計算した launchRotation の向き」で生成
        GameObject missileGO = Instantiate(missilePrefab, currentFirePoint.position, launchRotation, missileContainer);

        // 5. 次の発射サイドを切り替える
        fireFromRightSide = !fireFromRightSide;

        // 6. ミサイルにターゲットを渡す
        HomingMissile missile = missileGO.GetComponent<HomingMissile>();
        if (missile != null)
        {
            missile.target = target;
        }
    }

    // 「画面中心に最も近い」「カメラに映る」敵を探す関数
    Transform FindClosestVisibleEnemy()
    {
        // "Enemy" タグを持つ全てのゲームオブジェクトを探す
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform closestTarget = null;
        float minDistanceToCenter = Mathf.Infinity; // 画面中心からの「最小距離」を記録

        // 画面の中心座標 (0.5, 0.5) を定義
        Vector2 screenCenter = new Vector2(0.5f, 0.5f);

        foreach (GameObject enemy in enemies)
        {
            Transform enemyTransform = enemy.transform;

            // 1. 視界チェック (カメラのビューポート座標に変換)
            //    (0,0)が左下, (1,1)が右上。Zがプラスならカメラの前方
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(enemyTransform.position);

            // Zがマイナス(背後)か、X/Yが0～1の範囲外(画面外)ならスキップ
            if (viewportPoint.z <= 0 ||
                viewportPoint.x < 0 || viewportPoint.x > 1 ||
                viewportPoint.y < 0 || viewportPoint.y > 1)
            {
                continue; // 次の敵へ
            }

            // 2. 画面中心からの距離を計算
            //    敵のビューポート座標 (x, y) を取り出す
            Vector2 enemyScreenPos = new Vector2(viewportPoint.x, viewportPoint.y);

            // 画面中心 (0.5, 0.5) から敵の座標までの「2D距離」を計算
            float distanceToCenter = Vector2.Distance(screenCenter, enemyScreenPos);

            // 3. 物理的な距離ではなく、この「画面中心からの距離」を比較する
            if (distanceToCenter < minDistanceToCenter)
            {
                // 最も中心に近いターゲットを更新
                minDistanceToCenter = distanceToCenter;
                closestTarget = enemyTransform;
            }
        }

        // 見つかったターゲットを返す
        return closestTarget;
    }
}