using UnityEngine;

public class Game {

    ElementController elementController;
    
    public int score, bestScore = 0;
    public float timeLeft;
    public bool pause = false;

    public void Init()
    {
        elementController.Init();
    }

    public Game()
    {
        elementController = ElementController.GetInstance();
        elementController.game = this;
    }

    public void StartGame()
    {
        timeLeft = 60;
        score = 0;
        elementController.FillField();
        Play();
    }

    public void Pause()
    {
        pause = true;
        elementController.canPlay = false;
        elementController.field.SetElementsBlure(true);
    }

    public void Play()
    {
        pause = false;
        elementController.canPlay = true;
        elementController.field.SetElementsBlure(false);
    }

    public void Update()
    {
        if (!pause)
        {
            elementController.Update();
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                Pause();
            }
        }
    }

    public void addScore()
    {
        score++;
        if (score >= bestScore)
        {
            bestScore = score;
        }
    }
}
