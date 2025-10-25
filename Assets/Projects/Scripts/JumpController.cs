using UnityEngine;

public class JumpController : MonoBehaviour
{
    Animator animator;
    bool canMove = true; // 座標を動かすかどうか

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        // 溜めモーション中（例：ステート名 "JumpCharge"）なら座標を固定
        if (state.IsName("Jumping_charge"))
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }

        // 座標移動処理（例）
        if (canMove)
        {
            MoveCharacter();
        }
    }

    void MoveCharacter()
    {
        // WASD移動や重力処理など
    }
}
