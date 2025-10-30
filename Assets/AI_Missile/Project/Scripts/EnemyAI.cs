using UnityEngine;
using UnityEngine.AI;

// Rigidbody も必須コンポーネントとして追加
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Material")]
    public Material greenMaterial;
    public Material redMaterial;

    [Header("Search")]
    public float detectionRange = 15f; // プレイヤーを検知する範囲
    public float chaseSpeed = 5f;      // 追跡時の速度

    [Header("Patrol")]
    public float patrolSpeed = 2f;     // 通常時（巡回時）の速度
    public float patrolRadius = 20f; // スポーン地点からの巡回範囲
    public float patrolWaitTime = 3.0f; // 巡回地点に到着してから次に移動するまでの待機時間

    [Header("Attack")]
    public int attackDamage = 1; // 攻撃力

    private NavMeshAgent agent;
    private MeshRenderer meshRenderer;
    private Rigidbody rb; // Rigidbody を保持する変数
    private bool isChasing = false;
    private Vector3 startPosition; // スポーン地点（初期位置）を記憶
    private bool isPatrolWaiting = false; // 巡回待機中フラグ
    private float attackCooldown = 1.0f; // 攻撃のクールダウン（1秒ごと）
    private float lastAttackTime = -1.0f;   // 最後に攻撃した時間（ゲーム開始直後に攻撃できるように-1で初期化）

    // スポナーに死亡を通知するためのイベント
    public static event System.Action OnEnemyDestroyed;

    void Start()
    {
        // コンポーネントを取得
        agent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>(); // Rigidbody を取得

        // NavMeshAgentが自動で動かないように設定
        agent.updatePosition = false;
        agent.updateRotation = false;

        startPosition = transform.position; // 初期位置を記憶

        // プレイヤーをタグで自動検索
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // 初期状態（緑）
        SetState(false);
    }

    void Update()
    {
        // プレイヤーが見つからないなら何もしない
        if (player == null) return;

        // 既に追跡中の場合
        if (isChasing)
        {
            // プレイヤーの位置を常に更新し、追い続ける
            agent.SetDestination(player.position);
        }
        // まだ追跡していない場合
        else
        {
            // プレイヤーとの距離を計算
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // プレイヤーが索敵範囲内に入ったら
            if (distanceToPlayer <= detectionRange)
            {
                // 追跡モードに切り替える
                SetState(true);

                // 発見したフレームですぐに追跡を開始する
                agent.SetDestination(player.position);

                return;
            }

            // 2.巡回到着判定

            // 目的地（パス）が設定されていて、待機中でないか？
            if (agent.hasPath && !isPatrolWaiting)
            {
                // 重要: agent.remainingDistance ではなく、
                // 物理的な現在地(transform.position)と最終目的地(agent.destination)の
                // 実際の距離を計算する
                float distanceToDestination = Vector3.Distance(transform.position, agent.destination);

                // 物理的な距離が、エージェントの停止距離（stoppingDistance）以下になったら
                // 「到着した」とみなす
                if (distanceToDestination <= agent.stoppingDistance)
                {
                    // 待機コルーチンを開始する
                    StartCoroutine(PatrolWaitRoutine());
                }
            }
            // そもそもパスがない（巡回が完了or失敗）していて、待機中でもない場合
            // (ゲーム開始直後や、何らかの理由でパスが消えた場合のリカバリ)
            else if (!agent.hasPath && !isPatrolWaiting)
            {
                // 次の巡回を開始する（待機なしで即時）
                StartPatrol();
            }
        }
    }

    void FixedUpdate()
    {
        // Rigidbodyの現在位置をNavMeshAgentに同期させる
        // (プレイヤーに押されてズレた物理的な位置をAIに教える)
        agent.nextPosition = rb.position;

        if (isChasing && agent.hasPath)
        {
            // AIが計算した「次に向かうべき方向」
            Vector3 desiredDirection = (agent.steeringTarget - transform.position).normalized;
            // AIが設定した速度
            float targetSpeed = agent.speed;

            // 目標の速度ベクトル
            Vector3 targetVelocity = desiredDirection * targetSpeed;

            // 現在の速度との差を計算し、力を加える
            Vector3 velocityChange = (targetVelocity - rb.linearVelocity);
            velocityChange.y = 0; // Y軸(上下)の力は加えない

            rb.AddForce(velocityChange, ForceMode.VelocityChange);

            // --- Rigidbodyによる回転処理 ---
            if (desiredDirection != Vector3.zero)
            {
                // 向きたい方向（AIが指示する方向）を向く
                Quaternion lookRotation = Quaternion.LookRotation(desiredDirection);

                // agent.angularSpeed (角速度) を使ってスムーズに回転させる
                // (Time.fixedDeltaTime は FixedUpdate で使う時間)
                float turnSpeed = agent.angularSpeed * Time.fixedDeltaTime;
                Quaternion targetRotation = Quaternion.Slerp(rb.rotation, lookRotation, turnSpeed);

                rb.MoveRotation(targetRotation);
            }
        }
        // 待機・巡回モードの場合
        else if (!isChasing && agent.hasPath)
        {
            // 巡回地点に向かって移動・回転する
            // (追跡時と全く同じロジックだが、速度は patrolSpeed (agent.speed) が使われる)

            Vector3 desiredDirection = (agent.steeringTarget - transform.position).normalized;
            float targetSpeed = agent.speed; // SetState(false)で patrolSpeed が設定されている

            Vector3 targetVelocity = desiredDirection * targetSpeed;
            Vector3 velocityChange = (targetVelocity - rb.linearVelocity);
            velocityChange.y = 0;
            rb.AddForce(velocityChange, ForceMode.VelocityChange);

            if (desiredDirection != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(desiredDirection);
                float turnSpeed = agent.angularSpeed * Time.fixedDeltaTime;
                Quaternion targetRotation = Quaternion.Slerp(rb.rotation, lookRotation, turnSpeed);
                rb.MoveRotation(targetRotation);
            }
        }
        // 巡回先にも到達し、パスがない場合（念のためブレーキ）
        else if (!isChasing && !agent.hasPath)
        {
            Vector3 velocityChange = (Vector3.zero - rb.linearVelocity);
            velocityChange.y = 0;
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    // 敵の状態を切り替える関数
    void SetState(bool chase)
    {
        isChasing = chase;
        if (isChasing)
        {
            // 追跡モード
            meshRenderer.material = redMaterial;
            agent.speed = chaseSpeed;

            // もし巡回待機コルーチンが動いていたら停止する
            StopAllCoroutines(); // このAIが実行中のコルーチンをすべて停止
            isPatrolWaiting = false; // 待機フラグをリセット
        }
        else
        {
            // 通常モード
            meshRenderer.material = greenMaterial;
            agent.speed = patrolSpeed;

            // 巡回を開始する
            StartPatrol();
        }
    }

    // オブジェクトが破棄されるときにスポナーに通知
    void OnDestroy()
    {
        OnEnemyDestroyed?.Invoke();
    }

    void StartPatrol()
    {
        // 1. ランダムな地点を計算
        Vector3 randomPoint = startPosition + Random.insideUnitSphere * patrolRadius;

        NavMeshHit hit;

        // 2.探索範囲
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
        {
            // 3. 目的地を設定
            agent.SetDestination(hit.position);
        }
        else
        {
            // 4. (念のため) もし patrolRadius 内でも見つからなかった場合、
            //    とりあえずスポーン地点に戻るようにする (オプション)
            agent.SetDestination(startPosition);
        }
    }

    // 巡回待機コルーチン
    System.Collections.IEnumerator PatrolWaitRoutine()
    {
        // 1. 待機状態フラグを立てる
        isPatrolWaiting = true;

        // 2. 指定した時間だけ待つ
        yield return new WaitForSeconds(patrolWaitTime);

        // 3. 待機終了後、次の巡回を開始する
        StartPatrol();

        // 4. 待機状態フラグを下ろす
        isPatrolWaiting = false;
    }

    // オブジェクトが他のコライダー/リジッドボディと接触し続けている間、毎フレーム呼び出される
    void OnCollisionStay(Collision collision)
    {
        // 1. 接触した相手が "Player" タグを持っているか確認
        if (collision.gameObject.CompareTag("Player"))
        {
            // 2. 現在時刻が「最後に攻撃した時刻 + クールダウン」を過ぎているか確認
            //    (攻撃可能かどうか)
            if (Time.time > lastAttackTime + attackCooldown)
            {
                // 3. プレイヤーのHPコンポーネントを取得
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

                // 4. コンポーネントが見つかったら
                if (playerHealth != null)
                {
                    // ダメージを与える
                    playerHealth.TakeDamage(attackDamage);

                    // 5. 最後に攻撃した時刻を現在時刻に更新
                    lastAttackTime = Time.time;
                }
            }
        }
    }
}