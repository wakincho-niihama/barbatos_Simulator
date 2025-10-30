using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 100;
    public int currentHP;

    // スライダーへの参照を保持する変数
    [Header("UI")]
    public Slider hpSlider;

    [Header("Game Over")]
    public GameObject gameOverScreen; // (Panel) GameOver_Screen をアタッチ
    public TextMeshProUGUI countdownText;
    public float restartDelay = 3f;   // 復活までの秒数

    void Start()
    {
        currentHP = maxHP;

        // ゲーム開始時にスライダーの最大値と現在値を設定する
        UpdateHP_UI();

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        Debug.Log("Player HP: " + currentHP + " / " + maxHP);

        // ダメージを受けた時にスライダーの値を更新する
        UpdateHP_UI();

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    // スライダーを更新する専用の関数
    void UpdateHP_UI()
    {
        // スライダーが設定されているか確認
        if (hpSlider != null)
        {
            // スライダーの最大値を maxHP に設定
            hpSlider.maxValue = maxHP;
            // スライダーの現在の値を currentHP に設定
            hpSlider.value = currentHP;
        }
    }

    void Die()
    {
        Debug.Log("The player has died.");

        // ゲームオーバー画面を表示
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        // プレイヤーの操作を無効にする
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // リスタートのコルーチンを開始
        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        float countdown = restartDelay; // (例: 3f)

        // countdown が 0 より大きい間、ループする
        while (countdown > 0)
        {
            // カウントダウンテキストを更新
            // Mathf.Ceil (切り上げ) で「3」「2」「1」と表示
            if (countdownText != null)
            {
                countdownText.text = "Until revival..." + Mathf.Ceil(countdown) + "s";
            }

            // 1秒待つ
            yield return new WaitForSeconds(1f);

            // カウントを減らす
            countdown -= 1f;
        }

        // ループ終了後 (0秒になったら)
        countdownText.text = "Until revival...0s";

        // シーンをリロード（リスタート）
        // (現在のシーンの名前を取得して、それをロードする)
        Debug.Log("Restarting.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}