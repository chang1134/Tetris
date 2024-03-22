using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public interface IBoardView
    {
        public void DrawFixedBoard(int[] board);
        public void DrawMovingBoard(int[] board);

        /* 回合开始 */
        public void OnRoundStart();
        /* 游戏结束 */ 
        public void OnGameOver();

    }

    public interface IBoard
    {
        public int[] datas { get; }
    }

    public enum BoardType
    {
        Fixed,
        Moving,
    }


    public enum GameStatus
    {
        Prepare, // 游戏还未开始
        Running, // 游戏正在进行
        Pause, // 游戏暂停
        GameOver // 游戏结束
    }

    public enum BlockOperation
    {
        Down,
        Left,
        Right,
        ToBottom,
        Rotate,
    }
}
