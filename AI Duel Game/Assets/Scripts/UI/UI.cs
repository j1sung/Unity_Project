using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static UI instance;

    // UI - ���� �ؽ�Ʈ ��� ����
    [SerializeField] private Text gameText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text playerScoreText;
    [SerializeField] private Text aiScoreText;

    // Player ��ü ���� ����
    public HumanPlayer humanPlayer;
    public EnemyPlayer enemyPlayer;

    // Player �浹 ����
    public enum Player { FirstPlayer, SecondPlayer }
    public Player playerSequence;

    // UI - Power ������ ��� ����
    public GameObject PowerPrefab;  // Power ������
    [SerializeField] private int PowerNumber = 7;  // Power 7�� ����
    public Camera mainCamera;    // ī�޶� ����
    int humanScore;  // �÷��̾� score ����
    int aiScore;      // AI score ����

    // Score ��ȯ ����
    private int currentScore; // ���� ���ھ�

    // �÷��̾� �浹(���º�) �б� ����
    public bool isProcessingCollision = false; // �浹 ó�� ������ ����
    public const float delayThreshold = 0.1f;  // 100ms ������ (���� �浹 ���� Ȯ��)
    public bool isTwo = false; // �ι�° �浹 �߻� ����

    // Round ��ȯ ����
    private int currentRound; // ���� ����
    public bool isRoundTransitioning = false; // ���� �̵� ������ ����

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
        UpdateRoundText();

        // Player Score ���
        AssignPlayers();
        UpdateScoreText();

        // ���� �ʱ� ����
        gameStarted = false; // ���� ���� ����
        Time.timeScale = 0;  // �ð� ���� (��� ���� ������Ʈ ����)
    }

    // Player ��ü ���� & Score���� �׵�
    void AssignPlayers()
    {
        humanPlayer = FindObjectOfType<HumanPlayer>();  // HumanPlayer ��ũ��Ʈ�� ã�Ƽ� �Ҵ�
        enemyPlayer = FindObjectOfType<EnemyPlayer>();  // EnemyPlayer ��ũ��Ʈ�� ã�Ƽ� �Ҵ�
        humanScore = humanPlayer.getScore();
        aiScore = enemyPlayer.getScore();
    }

    // �÷��̾�� ���ھ� �ؽ�Ʈ�� ������Ʈ�ϴ� �Լ�
    void UpdateScoreText()
    {
        playerScoreText.text = "Score: " + humanScore;
        aiScoreText.text = "Score: " + aiScore;

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
    public void NextRound()  // ���ݿ� ������ �÷��̾��� ������ 1�� �ø��� ȣ���
    {
        isProcessingCollision = false;

        // "P_Attack" �±׸� ���� ��� ���� ������Ʈ�� ã�� ����
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("P_Attack");

        foreach (GameObject obj in objectsWithTag)
        {
            Destroy(obj); // �� ������Ʈ�� ����
        }

        // �� �÷��̾� Score ���� �ٽ� �޾ƿ�
        AssignPlayers();
        
        // �� �� �ϳ��� ���� 3���� �����߰�, ������ �ٸ��� �̱� ��Ȳ (���� ����)
        if ((humanScore >= 3 || aiScore >= 3) && (humanScore != aiScore))
        {
            StartCoroutine(ShowEnd()); // ���� ��� ��°� �� ���� ����!

        }
        else // ��� ��Ȳ�̰ų�, �� �� 3�� �̸��� ��� (������ ���� ������ ����)
        {
            StartCoroutine(ShowKO());
            currentRound++;   // ���� �� ����
            ResetGameState(); // ���� ���¸� �ʱ�ȭ ��Ŵ
            UpdateRoundText(); // ���� ���� ǥ��
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

        if (humanScore > aiScore) // HumanPlayer�� �̱��
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
        ResetGame(); // ���� ����
    }

    // ���� ���¸� �ʱ�ȭ - UI
    void ResetGameState()
    {
        // ������ Score �ٽ� ���
        UpdateScoreText();

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
