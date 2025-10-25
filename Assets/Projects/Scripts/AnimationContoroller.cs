using UnityEngine;

/// <summary>
/// RobotWalk などの移動系スクリプトから
/// 移動入力を受け取り、AnimatorのBlendTreeを制御するクラス。
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// RobotWalkからX,Z方向の移動量を受け取ってBlendTreeを制御
    /// </summary>
    public void UpdateMovement(float moveX, float moveZ)
    {
        // Blend Tree のパラメータに反映
        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveZ", moveZ);
    }

    /// <summary>
    /// その他の状態（ジャンプ、ブーストなど）を制御する
    /// </summary>
    public void UpdateSpecialStates(bool isBoosting, bool isJumpCharge,
     bool isJumping, bool isFalling, bool isTurningRight, bool isTurningLeft,
      bool isHoldGun, bool isGunAttack, bool isMaceAttack, bool isGrounded,
      bool isBackBoost, bool isRightBoost, bool isLeftBoost)
    {
        animator.SetBool("isBoosting", isBoosting);
        animator.SetBool("isBackBoost", isBackBoost);
        animator.SetBool("isRightBoost", isRightBoost);
        animator.SetBool("isLeftBoost", isLeftBoost);

        animator.SetBool("isJumpCharge", isJumpCharge);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);
        animator.SetBool("isGround", isGrounded);

        animator.SetBool("isTurningRight", isTurningRight);
        animator.SetBool("isTurningLeft", isTurningLeft);

        animator.SetBool("isHoldGun", isHoldGun);
        animator.SetBool("isGunAttack", isGunAttack);
        animator.SetBool("isMaceAttack", isMaceAttack);
    }
}
