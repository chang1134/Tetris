using System.Collections;
using System.Collections.Generic;
using Tetris.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IMain
{
    /* 分数的变化 */
    public void OnScoreChange(int score);

    /* 消除行的变化 */
    public void OnEliminate(int lineCount);

    public void OnSpeedChange(int speed);

    public void OnNextBlock(Block block);
}

public class MainView : MonoBehaviour, IMain
{

    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtEliminate;
    public TextMeshProUGUI txtSpeed;
    public TextMeshProUGUI txtTime;

    public Button btnLeft;
    public Button btnRight;
    public Button btnDrop;
    public Button btnSpeedUp;
    public Button btnRotate;

    public IBoardView board;

    void Start()
    {
        btnLeft.onClick.AddListener(OnLeftClick);
        btnRight.onClick.AddListener(OnRightClick);
        btnDrop.onClick.AddListener(OnDropClick);
        btnSpeedUp.onClick.AddListener(OnSpeedUpClick);
        btnRotate.onClick.AddListener(OnRotateClick);
    }

    void OnLeftClick()
    {
    }

    void OnRightClick()
    {

    }

    void OnSpeedUpClick()
    {

    }

    void OnDropClick()
    {

    }

    void OnRotateClick()
    {

    }

    void Update()
    {
        
    }

    public void OnScoreChange(int score)
    {
        this.txtScore.text = score.ToString();
    }

    public void OnEliminate(int lineCount)
    {
        this.txtEliminate.text = lineCount.ToString();
    }

    public void OnSpeedChange(int speed)
    {
        this.txtSpeed.text = speed.ToString();
    }

    public void OnNextBlock(Block block)
    {
    }
}
