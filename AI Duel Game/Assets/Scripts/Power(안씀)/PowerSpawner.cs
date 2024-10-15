using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSpawner : MonoBehaviour
{
    public GameObject PowerPrefab;  // Power 프리팹
    public int PowerNumber = 7;  // Power 7개 생성
    public Camera mainCamera;      // 카메라 참조

    void Start()
    {
        SpawnPowers();
    }
    
    void SpawnPowers()
    {
        for(int i = 0; i < PowerNumber; i++)
        {
            // Dash 스킬을 카메라 내부의 랜덤 위치에 생성
            Vector2 PowerPosition = GetRandomPositionInCamera();
            Instantiate(PowerPrefab, PowerPosition, Quaternion.identity);
        }
    }

    Vector2 GetRandomPositionInCamera()
    {
        // 카메라 뷰포트 좌표계에서 (0, 0)은 왼쪽 아래, (1, 1)은 오른쪽 위
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0.4f, 0.2f, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(0.6f, 0.8f, mainCamera.nearClipPlane));

        // 카메라 내부의 X, Y 범위에서 랜덤 위치 생성
        float randomX = Random.Range(bottomLeft.x, topRight.x);
        float randomY = Random.Range(bottomLeft.y, topRight.y);

        // Z축은 2D 평면에서 필요 없으므로 0으로 설정
        return new Vector2(randomX, randomY);
    }
}
