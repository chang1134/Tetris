using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public interface IBoardView
    {
        public void DrawMovingBoard(int[] board);

        /* 回合开始 */
        public void OnRoundStart();
        /**
         * 消除行数 与 游戏是否结束
         */
        void OnRoundOver(int[] beforeEliminateBoardDatas, int[] afterEliminateBoardDatas, bool gameOver);
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
        RoundRuning, // 游戏回合正在进行
        RoundOver, //  游戏回合计算中
        Pause, // 游戏暂停
        GameOver // 游戏结束
    }

    public enum BlockOperation
    {
        Down,
        Left,
        Right,
        Drop,
        Rotate,
    }
}
