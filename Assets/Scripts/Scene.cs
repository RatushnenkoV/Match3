using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{
    public GameObject btnPlay, btnRestart, btnPause, btnSound;
    public GameObject score, timeLeft, best;

    public Game game;

    public void BtnPlayClick()
    {
        if (btnPlay != null)
        {
            game = new Game();
            game.Init();
            game.StartGame();
            btnPlay.GetComponent<UiController>().MoveOut();
        }

        ShowUI(btnRestart);
        ShowUI(btnPause);
        ShowUI(btnSound);
        ShowUI(score);
        ShowUI(timeLeft);
    }

    public void BtnRestartClick()
    {
        if (game != null)
        {
            best.GetComponent<UiController>().Hide();
            game.StartGame();
        }
    }

    public void BtnPauseClick()
    {
        if (game != null)
        {
            if (game.pause)
            {
                btnPause.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/button_PauseOn");
                game.Play();
            } else
            {
                btnPause.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/button_PauseOff");
                game.Pause();
            }
        }
    }

    public void BtnSoundClick()
    {
        if (AudioListener.volume == 0)
        {
            btnSound.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/button_audioUnMute");
            AudioListener.volume = 1;
        } else
        {
            btnSound.GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>("Sprites/button_audioMute");
            AudioListener.volume = 0;
        }
    }

    public void SetScore()
    {
        if (game != null && score != null)
        {
            score.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "Score: "+game.score;
            if (game.score == game.bestScore)
            {
                UiController bestController = best.GetComponent<UiController>();
                if (!bestController.shown)
                {
                    bestController.Show();
                }
            }
        }
    }

    public void SetTimeLeft()
    {
        if (game != null && timeLeft != null)
        {
            timeLeft.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = "" + Mathf.Floor(game.timeLeft+1);
        }
    }

    public void ShowUI(GameObject ui)
    {
        if (ui != null)
        {
            UiController uiController = ui.GetComponent<UiController>();
            if (uiController != null)
            {
                uiController.Show();
            }
        }
    }

    void Update()
    {
        if (game != null)
        {
            game.Update();
        }
        SetScore();
        SetTimeLeft();
    }
}
