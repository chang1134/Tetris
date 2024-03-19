using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public class FixedBoard : IBoard
    {
        private int boardWidth;

        public FixedBoard(int boardHeight, int boardWidth)
        {
            this.datas = new int[boardHeight + Block.MAX_SIZE];
            this.boardWidth = boardWidth;    
        }

        public int[] datas { get; private set; }

        public void Combine(IBoard targetBoard)
        {
            var length = Mathf.Min(targetBoard.datas.Length, this.datas.Length);
            for (int i = 0; i < length; i++)
            {
                this.datas[i] |= targetBoard.datas[i];
            }
        }
    }
}