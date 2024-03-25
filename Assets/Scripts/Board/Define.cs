using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public interface IBoardView
    {
        public void DrawMovingBoard(int[] board);

        /* �غϿ�ʼ */
        public void OnRoundStart();
        /**
         * �������� �� ��Ϸ�Ƿ����
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
        Prepare, // ��Ϸ��δ��ʼ
        RoundRuning, // ��Ϸ�غ����ڽ���
        RoundOver, //  ��Ϸ�غϼ�����
        Pause, // ��Ϸ��ͣ
        GameOver // ��Ϸ����
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
