using UnityEngine;

public class CockpitCoverBackAnime : MonoBehaviour
{
    // about cover
    private Vector3 coverStartPos;
    [Header("CoverObject")]
    public Transform cockpitCover;

    [Header("Cover's MoveAmount")]
    public float coverOffset_limit = 0.0f;

    bool cockpitCoverStop;
    float coverPos = 0.0f;

    // about CockpitBody
    private Vector3 CockpitBodyStartPos;
    [Header("CockpitBodyObject")]
    public Transform CockpitBody;

    [Header("CockpitBody's MoveAmount")]
    public float CockpitBodyOffset_limit = 0.0f;

    bool CockpitBodyStop = false;
    float CockpitBodyPos = 0.0f;

    // about door
    [Header("DoorObject")]
    public Transform cockpitDoor;

    [Header("Door's RotateAmount")]
    public float cockpitDoorRotate_Limit = 0.0f;

    bool cockpitDoorStop = false;
    float cockpitDoorRotate = 0.0f;

    //発射シークエンス制御フラグ
    bool AnimationPlay = false;
    //コクピット外装透過フラグ
    public bool isFade = false;
    //シークエンス完了フラグ
    public bool isFinished = false;

    //アニメ用インスタンス
    public NewWalk newWalk;


    void Start()
    {
        coverStartPos = cockpitCover.localPosition;
        CockpitBodyStartPos = CockpitBody.localPosition;

        cockpitCover.localPosition = cockpitCover.localPosition + cockpitCover.forward * coverOffset_limit;
        CockpitBody.localPosition = new Vector3(CockpitBody.localPosition.x, CockpitBody.localPosition.y + CockpitBodyOffset_limit, CockpitBody.localPosition.z);
        cockpitDoor.transform.localRotation = Quaternion.Euler(0f, -1 * cockpitDoorRotate_Limit, 0f);
    }

    void Update()
    {

        // スペースキーで開閉トグル
        if (Input.GetKey(KeyCode.Space) || newWalk.isCockpitActivate)
        {
            AnimationPlay = !AnimationPlay;
            Debug.Log($"is sapcekey : {Input.GetKey(KeyCode.Space)}");
        }

        if (AnimationPlay && !isFinished)
        {
            // CloseDoor
            if (!cockpitDoorStop)
            {
                if (cockpitDoor.localRotation.y < 0)
                {
                    cockpitDoorRotate += 0.1f;
                    cockpitDoor.transform.localRotation = Quaternion.Euler(0f, -1 * cockpitDoorRotate_Limit + cockpitDoorRotate, 0f);
                }
                else
                {
                    cockpitDoorStop = true;//完了フラグ
                }
            }//OK

            // DownCockpitBody
            if (cockpitDoorStop && !CockpitBodyStop)
            {
                if (CockpitBody.localPosition.y > CockpitBodyStartPos.y)
                {
                    CockpitBodyPos -= 0.000005f;
                    CockpitBody.localPosition = new Vector3(CockpitBody.localPosition.x, CockpitBody.localPosition.y + CockpitBodyPos, CockpitBody.localPosition.z);
                }
                else
                {
                    CockpitBodyStop = true;//完了フラグ
                }
            }//OK

            // closeCover
            if (CockpitBodyStop && !cockpitCoverStop)
            {
                if (cockpitCover.localPosition.z > coverStartPos.z)
                {
                    Debug.Log($"check{cockpitCover.localPosition}");
                    coverPos += 0.000005f;
                    cockpitCover.localPosition = cockpitCover.localPosition - 1 * cockpitCover.forward * coverPos;
                }
                else
                {
                    cockpitCoverStop = true;//完了フラグ
                }
            }//OK

            //全シークエンス完了後　コクピット外装のマテリアルをマジックミラーのように変更する 別スクリプトで対応済み
            if (cockpitCoverStop) isFade = true;

        }
    }
}