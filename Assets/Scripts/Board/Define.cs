using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public interface IBoardView
    {
        public void DrawFixedBoard(int[] board);
        public void DrawMovingBoard(int[] board);

    }

    public interface IBoard
    {
        public int[] datas { get; }
    }


    public class BoardDefine
    {
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
}
