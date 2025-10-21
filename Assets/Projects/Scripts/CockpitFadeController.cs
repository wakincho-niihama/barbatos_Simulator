using UnityEngine;
using System.Collections;

public class SharedMaterialFadeController : MonoBehaviour
{
    [SerializeField] private Material targetMaterial; // 対象マテリアル（例: Cockpit_inside）
    [SerializeField] private MonoBehaviour targetScript; // 監視対象スクリプトを任意に指定
    [SerializeField] private string boolVariableName = "isFade"; // 監視するbool変数名
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float targetAlpha = 0.1f;

    private bool hasFaded = false;
    private Color baseColor;

    void Start()
    {
        if (targetMaterial == null || targetScript == null)
        {
            Debug.LogError("targetMaterial または targetScript が指定されていません。");
            enabled = false;
            return;
        }

        baseColor = targetMaterial.GetColor("_BaseColor");
    }

    void Update()
    {
        // リフレクションで指定変数を取得
        var type = targetScript.GetType();
        var field = type.GetField(boolVariableName);
        if (field == null || field.FieldType != typeof(bool))
        {
            Debug.LogWarning($"'{boolVariableName}' は {type.Name} に存在しません。");
            return;
        }

        bool isFade = (bool)field.GetValue(targetScript);

        if (isFade && !hasFaded)
        {
            hasFaded = true;
            StartCoroutine(FadeOutSharedMaterial());
        }
    }

    IEnumerator FadeOutSharedMaterial()
    {
        float startAlpha = baseColor.a;
        float endAlpha = targetAlpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);

            Color newColor = baseColor;
            newColor.a = newAlpha;
            targetMaterial.SetColor("_BaseColor", newColor);

            yield return null;
        }

        baseColor.a = endAlpha;
        targetMaterial.SetColor("_BaseColor", baseColor);
        Debug.Log($"マテリアル '{targetMaterial.name}' フェード完了。");
    }
    // シーン終了・エディタ停止時にアルファをリセット
    void OnDisable()
    {
        ResetAlphaTo230();
    }

    void OnApplicationQuit()
    {
        ResetAlphaTo230();
    }

    private void ResetAlphaTo230()
    {
        if (targetMaterial == null) return;

        Color color = targetMaterial.GetColor("_BaseColor");
        color.a = 245f / 255f; // 約0.9
        targetMaterial.SetColor("_BaseColor", color);

        Debug.Log($"[{name}] シーン終了時に {targetMaterial.name} のアルファを245(約0.9)にリセットしました。");
    }
}
