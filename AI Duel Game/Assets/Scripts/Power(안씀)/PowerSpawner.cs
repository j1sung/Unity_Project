using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSpawner : MonoBehaviour
{
    public GameObject PowerPrefab;  // Power ������
    public int PowerNumber = 7;  // Power 7�� ����
    public Camera mainCamera;      // ī�޶� ����

    void Start()
    {
        SpawnPowers();
    }
    
    void SpawnPowers()
    {
        for(int i = 0; i < PowerNumber; i++)
        {
            // Dash ��ų�� ī�޶� ������ ���� ��ġ�� ����
            Vector2 PowerPosition = GetRandomPositionInCamera();
            Instantiate(PowerPrefab, PowerPosition, Quaternion.identity);
        }
    }

    Vector2 GetRandomPositionInCamera()
    {
        // ī�޶� ����Ʈ ��ǥ�迡�� (0, 0)�� ���� �Ʒ�, (1, 1)�� ������ ��
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0.4f, 0.2f, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(0.6f, 0.8f, mainCamera.nearClipPlane));

        // ī�޶� ������ X, Y �������� ���� ��ġ ����
        float randomX = Random.Range(bottomLeft.x, topRight.x);
        float randomY = Random.Range(bottomLeft.y, topRight.y);

        // Z���� 2D ��鿡�� �ʿ� �����Ƿ� 0���� ����
        return new Vector2(randomX, randomY);
    }
}
