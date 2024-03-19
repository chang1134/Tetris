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
            Prepare, // ��Ϸ��δ��ʼ
            Running, // ��Ϸ���ڽ���
            Pause, // ��Ϸ��ͣ
            GameOver // ��Ϸ����
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
