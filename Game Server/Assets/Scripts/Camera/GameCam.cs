using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCam : MonoBehaviour
{
    public PlayerFlag currentFocus { get; private set; }

    private Player m_owner;

    public void Init(Player player)
    {
        m_owner = player;
        currentFocus = player.m_flagProp;
    }

    public void MoveCameraToPositionWithRotation(Vector3 pos, Vector3 rot, float seconds)
    {
        StartCoroutine(GameUtil.MoveToPosInSec(transform, pos, seconds));
        StartCoroutine(GameUtil.RotateToTargetInSec(transform, rot, seconds));
    }

    public void SetCameraToPlayer(Player player)
    {
        Vector3 cameraRotation = transform.eulerAngles;
        float y = MathUtil.WrapAngle(player.transform.eulerAngles.y);
        cameraRotation.y = y;
        cameraRotation.x = 60f;
        Camera.main.transform.rotation = Quaternion.Euler(cameraRotation);
        Camera.main.transform.position = AdjustCameraPositionToObject(player.gameObject, y);

        AttributeBarManager.updateAttributeBar(player.m_flagProp);
        currentFocus = player.m_flagProp;
    }

    private Vector3 AdjustCameraPositionToObject(GameObject gameObj, float rotationY)
    {
        Vector3 ownerPos = gameObj.transform.position;
        ownerPos.y += 29;

        if (rotationY == 0f)
            ownerPos.z -= 17f;
        else if (rotationY == 180f)
            ownerPos.z += 17f;
        else if (rotationY == 90f)
            ownerPos.x -= 17f;
        else if (rotationY == -90f)
            ownerPos.x += 17f;

        return ownerPos;
    }

    public void ResetCam()
    {
        if (m_owner == null)
            return;
            
        SetCameraToPlayer(m_owner);
    }
}
