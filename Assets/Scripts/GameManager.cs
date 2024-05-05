using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;

    public GameObject[] Stages;
    public Image[] UIhealth;
    public TMP_Text UIPoint;
    public TMP_Text UIStage;
    public GameObject UIRestartButton;

    private void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();

        if (Input.GetKeyDown("escape"))
            Application.Quit();
    }

    public void OnHealthPointDown(int healthPoint)
    {
        if (healthPoint >= 0)
            UIhealth[healthPoint].color = new Color(1, 0, 0, 0.2f);
    }

    public void OnTakeItem(GameObject gameObject)
    {
        if (gameObject.name.Contains("Bronze"))
        {
            stagePoint += 10;
        }
        else if (gameObject.name.Contains("Silver"))
        {
            stagePoint += 100;
        }
        else if (gameObject.name.Contains("Gold"))
        {
            stagePoint += 1000;
        }
    }
    public void OnKillEnemy()
    {
        stagePoint += 200;
    }

    public void OnFinishStage()
    {
        // calculate point
        totalPoint += stagePoint;
        stagePoint = 0;

        // Change Stage
        if (stageIndex < Stages.Length - 1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);

            UIStage.text = "STAGE " + (stageIndex + 1);
        }
        else
        {
            // game clear
            OnGameClear();
        }
    }

    public void OnGameOver()
    {
        // player control lock
        Time.timeScale = 0;

        // todo: result ui
        Debug.Log("죽었습니다!");

        // retry button ui
        OnRetryButtonEnabled("Game Over!");
    }

    public void OnGameClear()
    {
        // player control lock
        Time.timeScale = 0;

        // todo: Result UI
        Debug.Log("게임 클리어!");

        // Restart Button UI
        OnRetryButtonEnabled("Game Clear!");

    }

    public void OnRetryButtonEnabled(string retryButtonText)
    {
        TMP_Text buttonText = UIRestartButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = retryButtonText;
        UIRestartButton.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
