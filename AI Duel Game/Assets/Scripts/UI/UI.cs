using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static UI instance;

    // UI - ���� �ؽ�Ʈ ��� ����
    [SerializeField] private Text gameText;
    [SerializeField] private Text roundText;

    // Player ��ü ���� ����
    public HumanPlayer humanPlayer;
    public EnemyPlayer enemyPlayer;

    // UI - Hp, Power ������ ��� ����
    public GameObject hpPrefab;  // hp ������
    public GameObject PowerPrefab;  // Power ������
    [SerializeField] private int PowerNumber = 7;  // Power 7�� ����
    public Camera mainCamera;    // ī�޶� ����
    int hpPlayer;  // �÷��̾� HP ����
    int hpAI;      // AI HP ����

    // Round ��ȯ ����
    private int currentRound; // ���� ����
    private int totalRounds;  // �� ���� ��

    // ���� ���� ����
    private bool gameStarted; // ������ ���۵ƴ��� ���� Ȯ���ϴ� ����
    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (instance == null)
        {
            instance = this;
            Debug.Log("���ο� UI �ν��Ͻ��� �����Ǿ����ϴ�.");
        }
    }

    void Start()
    {
        Debug.Log("Start");
        // Round �ʱ�ȭ&���
        currentRound = 1; // ���� ���� �ʱ�ȭ
        totalRounds = 5;  // �� ���� �ʱ�ȭ
        UpdateRoundText();

        // Player ��ü�� Hp ���� ���
        AssignPlayers();
        SpawnHp();

        // ���� �ʱ� ����
        gameStarted = false; // ���� ���� ����
        Time.timeScale = 0;  // �ð� ���� (��� ���� ������Ʈ ����)
    }

    // Player ��ü ���� & Hp���� �׵�
    void AssignPlayers()
    {
        humanPlayer = FindObjectOfType<HumanPlayer>();  // HumanPlayer ��ũ��Ʈ�� ã�Ƽ� �Ҵ�
        enemyPlayer = FindObjectOfType<EnemyPlayer>();  // EnemyPlayer ��ũ��Ʈ�� ã�Ƽ� �Ҵ�
        hpPlayer = humanPlayer.getHp();
        hpAI = enemyPlayer.getHp();
    }

    // Hp ������ ȭ�� ����!
    void SpawnHp() 
    {
        ClearHp(); // ȭ��� HP�� �� ����� �ٽ� ����

        float spacing = 1.2f;  // HP ������ ���� (���Ƿ� ����)

        // ���� �� Player HP ����
        Vector2 PlayerPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.1f, 0.9f, 0));  // ���� �� ���� ��ġ
        for (int i = 0; i < hpPlayer; i++)
        {
            Vector2 hpPosition = new Vector2(PlayerPosition.x + i * spacing, PlayerPosition.y);
            Instantiate(hpPrefab, hpPosition, Quaternion.identity);
        }

        // ������ ���� Enemy HP ����
        Vector2 AIPlayerPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.75f, 0.9f, 0));  // ������ �� ���� ��ġ
        for (int i = 0; i < hpAI; i++)
        {
            Vector2 hpPosition = new Vector2(AIPlayerPosition.x + i * spacing, AIPlayerPosition.y);
            Instantiate(hpPrefab, hpPosition, Quaternion.identity);
        }
    }
    void ClearHp()
    {
        // "HP" �±׸� ���� ������Ʈ�� ��� ã�Ƽ� ����
        GameObject[] existingHp = GameObject.FindGameObjectsWithTag("HP");
        foreach (GameObject hp in existingHp)
        {
            Destroy(hp);
        }
    }

    // Power ������ ȭ�� ����!
    void SpawnPowers()
    {
        ClearPower();

        Vector2 mapCenter = new Vector2(0, 0);  // �� �߽�
        float mapRadius = 3.5f;                   // �� ������

        for (int i = 0; i < PowerNumber; i++)
        {
            // ���� �� ������ ���� ��ġ�� ����
            Vector2 powerPosition = GetRandomPositionInCircle(mapCenter, mapRadius);
            Instantiate(PowerPrefab, powerPosition, Quaternion.identity);
        }
    }
    public Vector2 GetRandomPositionInCircle(Vector2 center, float radius)
    {
        // ������ ���� (0 ~ 360��) ����
        float angle = Random.Range(0f, Mathf.PI * 2);

        // ������ �Ÿ� (0 ~ radius) ����
        float distance = Random.Range(0f, radius);

        // Polar Coordinates�� Cartesian Coordinates�� ��ȯ
        float x = center.x + Mathf.Cos(angle) * distance;
        float y = center.y + Mathf.Sin(angle) * distance;

        return new Vector2(x, y);
    }

    void ClearPower()
    {
        // "HP" �±׸� ���� ������Ʈ�� ��� ã�Ƽ� ����
        GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
        foreach (GameObject power in existingPower)
        {
            Destroy(power);
        }
    }

    private void Update()
    {
        // ����Ű ������ ���� ����! (gameStarted == True)
        if (!gameStarted && Input.GetKeyDown(KeyCode.Return)) // �� ������ ���ȴ��� Ȯ���ؾ� ��
        {
            StartGame();
        }
    }

    // ���� ����
    public void StartGame()
    {
        gameStarted = true;
        SpawnPowers();     // Power ������!
        StartCoroutine(ShowReadyFight()); // ���� ���� �ȳ�
        
    }

    IEnumerator ShowReadyFight()
    {
        yield return new WaitForSecondsRealtime(1.0f);  // 1�� ���
        // "Ready?" �ؽ�Ʈ ǥ��
        gameText.text = "Ready?"; 
        yield return new WaitForSecondsRealtime(2.0f);  // 2�� ���

        // "Fight!" �ؽ�Ʈ ǥ��
        gameText.text = "Fight!";
        yield return new WaitForSecondsRealtime(1.0f);  // 1�� ���

        // "Fight!" �ؽ�Ʈ ������� �Ϸ��� �� ���ڿ��� ����
        gameText.text = "";

        Time.timeScale = 1; // �ð� �簳 (���� ���� �ӵ��� ����)
    }

    // �� ���尡 ���� �� ȣ���Ͽ� ���� ���带 ���� �Լ�
    public void NextRound()  // ���ݿ� ���� �÷��̾��� ü�� 1�� ���̰� ȣ���
    {
        

        // �� �÷��̾� HP ���� �ٽ� �޾ƿ�
        AssignPlayers();

        if (currentRound <= totalRounds && hpPlayer != 0 && hpAI != 0) // 3�� �̱�� ������ ���� ���� �ʱ�ȭ��
        {
            StartCoroutine(ShowKO());
            currentRound++;   // ���� �� ����
            ResetGameState(); // ���� ���¸� �ʱ�ȭ ��Ŵ
            UpdateRoundText(); // ���� ���� ǥ��

        }
        else
        {
            StartCoroutine(ShowEnd()); // ���� ��� ��°� �� ���� ����!
        }
    }

    IEnumerator ShowKO()
    {
        Time.timeScale = 0; // �ð� ���� (��� ���� ������Ʈ ����)

        // "KO!" �ؽ�Ʈ ǥ��
        gameText.text = "KO!";
        yield return new WaitForSecondsRealtime(1.0f);  // 1�� ���

        // �ؽ�Ʈ �������
        gameText.text = "";

    }

    IEnumerator ShowEnd()
    {
        Time.timeScale = 0; // �ð� ���� (��� ���� ������Ʈ ����)

        if (hpPlayer != 0) // HumanPlayer�� �̱��
        {
            // "You Win!" �ؽ�Ʈ ǥ��
            gameText.text = "You Win!";
        }
        else // HumanPlayer�� ����
        {
            // "You Lose!" �ؽ�Ʈ ǥ��
            gameText.text = "You Lose!";
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2�� ���

        // �ؽ�Ʈ �������
        gameText.text = "";

        Debug.Log("All rounds completed!");
        ResetGame(); // 3�� �̱�� ���� ����
    }

    // ���� ���¸� �ʱ�ȭ - UI
    void ResetGameState()
    {
        // ������ Hp �ٽ� ���
        SpawnHp();

        // �÷��̾� ��ġ �ʱ�ȭ
        humanPlayer.transform.position = humanPlayer.getPosition();
        enemyPlayer.transform.position = enemyPlayer.getPosition();

        // �÷��̾� ���� �ʱ�ȭ
        humanPlayer.transform.rotation = humanPlayer.getRotation();
        enemyPlayer.transform.rotation = enemyPlayer.getRotation();

        StartGame();// GameText �ٽ� ���& ���ӽ���!

    }

    // ���� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    void UpdateRoundText()
    {
        roundText.text = "Round " + currentRound;
    }

    // �� �ʱ�ȭ
    void ResetGame()
    {
        // <��� 3����>
        // 1) Ư�� ������Ʈ�� ��Ȱ��ȭ -> Ȱ��ȭ �� ������Ʈ�� Start() ����� -> �ش� ������Ʈ ��ũ��Ʈ�� �ʱ�ȭ!

        // 2) ��� ������Ʈ�� ��Ȱ��ȭ -> Ȱ��ȭ (�̰� �� �ʱ�ȭ���� ��������)

        // 3) ���� �ٽ� �ε�
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // ���� ����(������)�� �����ϰ� ���� �ʱ�ȭ�� �� �ٽ� �ε��� �� ����
    }

}
