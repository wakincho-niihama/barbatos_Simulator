using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;    // 生成する敵のプレハブ
    public int maxEnemies = 100;      // 敵の最大数
    public float spawnInterval = 1.5f; // 敵を生成する間隔（秒）
    public float spawnAreaRadius = 50f; // 敵を生成する範囲の半径

    [Header("Spawn")]
    public Transform enemyContainer;

    private int currentEnemyCount = 0;

    void OnEnable()
    {
        // EnemyAIからの死亡通知を受け取る
        EnemyAI.OnEnemyDestroyed += OnEnemyDestroyed;
    }

    void OnDisable()
    {
        // 通知の登録を解除
        EnemyAI.OnEnemyDestroyed -= OnEnemyDestroyed;
    }

    void Start()
    {
        // スポーン処理をコルーチンで開始
        StartCoroutine(SpawnEnemyRoutine());
    }

    System.Collections.IEnumerator SpawnEnemyRoutine()
    {
        // 無限ループ
        while (true)
        {
            // 敵の数が上限未満なら
            if (currentEnemyCount < maxEnemies)
            {
                // 敵を一体スポーンさせる
                TrySpawnEnemy();
            }

            // 指定した間隔だけ待つ
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void TrySpawnEnemy()
    {
        // 1. ランダムな地点を計算
        // (このスポナーオブジェクトの位置を中心とした、半径 spawnAreaRadius の円内)
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnAreaRadius;

        NavMeshHit hit; // NavMesh の情報を格納する変数

        // 2. NavMesh.SamplePosition で「最も近い NavMesh 上の地点」を探す
        //    randomPoint から 10.0f の範囲内で探す (spawnAreaRadius とは別)
        if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
        {
            // 3. 有効な地点 (hit.position) が見つかった場合
            //    障害物に埋まらず、NavMesh上にあることが保証される
            Instantiate(enemyPrefab, hit.position, Quaternion.identity, enemyContainer);
            currentEnemyCount++;
        }
        else
        {
            // 4. 有効な地点が見つからなかった場合（例: ランダム地点が壁の中すぎた）
            //    今回は何もしない（次のインターバルで再試行）
        }
    }

    // 敵が破壊されたときに EnemyAI から呼び出される
    void OnEnemyDestroyed()
    {
        if (currentEnemyCount > 0)
        {
            currentEnemyCount--;
        }
    }
}