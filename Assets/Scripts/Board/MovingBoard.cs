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

        public MovingBoard(int boardHeight, int boardWidth)
        {
            datas = new int[boardHeight + Block.MAX_SIZE];
            this.boardWidth = boardWidth;
        }

        public int[] datas { get; private set; }

        public void Reset(Block block)
        {
            var offsetY = this.datas.Length - Block.MAX_SIZE + block.size - block.rect.y - block.rect.height;
            var offsetX = Mathf.CeilToInt((this.boardWidth - block.size) / 2f);
            this.Reset(block, offsetX, this.datas.Length - offsetY - 1);
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
            for (int i = 0; i < block.size; i++)
            {
                if (this.y - i >= 0)
                {
                    if (this.x >= 0)
                    {
                        this.datas[this.y - i] = this.block.binaryArray[i] << this.x;
                    } else
                    {
                        this.datas[this.y - i] = this.block.binaryArray[i] >> Math.Abs(this.x);
                    }
                }
            }
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
