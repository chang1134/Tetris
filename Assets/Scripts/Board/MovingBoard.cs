using System;
using UnityEngine;

namespace Tetris.Runtime
{
    public class LineBlockInfo
    {
        public int rotateCount;
        public int blockPosX;
        public int blockPosY;
        public BlockType type;
        public int[] lineBlockData;
    }

    public class MovingBoard : IBoard
    {
        public Block block { get; private set; }

        /**
         * ��ǰ�Ƕ���������ĵ�0�����ݴ��ڷǶ��������̵�λ��
         * ע�⣺1. ���̵����½�Ϊԭ��
         *      2. ����λ�ÿ��ܻᳬ�����̣����ڿ��У�
         */
        public int x { get; private set; }
        public int y { get; private set; }

        private int boardWidth;

        public MovingBoard(int boardWidth, int boardHeight)
        {
            datas = new int[boardHeight + Block.MAX_SIZE];
            this.boardWidth = boardWidth;
        }

        public int[] datas { get; private set; }

        public void ToTop() {
            this.y = this.datas.Length - 1;
            this.UpdateBoard();
        }

        public void Reset(Block block)
        {
            var offsetX = Mathf.CeilToInt((this.boardWidth - block.size) / 2f);

            var partBoardData = Utils.GenPartBoardData(block.binaryArray, offsetX, this.boardWidth);

            var emptyLineCount = Block.MAX_SIZE - block.size;
            for (int i = partBoardData.Length - 1; i >= 0; i--)
            {
                if (partBoardData[i] > 0) break;
                emptyLineCount++;
            }
            var offsetY = this.datas.Length - 1 - emptyLineCount;
            this.Reset(block, offsetX, offsetY);
        }

        public void Reset(Block block, int x, int y)
        {
            CleanBoard();

            this.block = block;
            this.x = x;
            this.y = y;
            this.UpdateBoard();
        }

        private void CleanBoard()
        {
            if (this.datas == null) return;
            for (int i = 0; i < this.datas.Length; i++)
            {
                this.datas[i] = 0;
            }
        }

        private void UpdateBoard()
        {
            CleanBoard();
            var partBoardData = Utils.GenPartBoardData(block.binaryArray, this.x, this.boardWidth);
            for (int i = 0; i < partBoardData.Length; i++)
            {
                if (y - i >= 0)
                {
                    this.datas[y - i] = partBoardData[i];
                }
            }
        }


        public void ToBottom(int y)
        {
            if (this.y < y)
            {
                throw new Exception("ToBottom currentY < newY ???");
            }
            this.y = y;
            this.UpdateBoard();
        }

        /**
         * ����һ��
         */
        public void Down()
        {
            this.y -= 1;
            this.UpdateBoard();
        }

        public void Left()
        {
            this.x -= 1;
            this.UpdateBoard();
        }

        public void Right()
        {
            this.x += 1;
            this.UpdateBoard();
        }

        public void Rotate()
        {
            this.block.Rotate();
            this.UpdateBoard();
        }
    }
}
