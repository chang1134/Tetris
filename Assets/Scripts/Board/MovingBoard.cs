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
                        this.datas[this.y - i] = this.block.bynaryArray[i] << this.x;
                    } else
                    {
                        this.datas[this.y - i] = this.block.bynaryArray[i] >> Math.Abs(this.x);
                    }
                }
            }
        }

        /**
         * Ĭ������һ��
         * toBottomΪtrueʱ���������׶�
         * 
         * return true��ʾ�ƶ��ɹ����ƶ��󣬰����Ч���ݲ��ᳬ�����̣�
         */
        public bool Down(bool toBottom = false)
        {
            if (toBottom)
            {
                var offsetY = this.block.size - this.block.rect.y - this.block.rect.height;
                if (this.y == -offsetY) return false;
                this.y = -offsetY;
            }
            else
            {
                if (CheckOutBounding(x, y - 1, this.block.rect)) return false;
                this.y -= 1;
            }
            this.UpdateBoard();
            return true;
        }

        public bool Left()
        {
            if (CheckOutBounding(x - 1, y, this.block.rect)) return false;

            this.x -= 1;
            this.UpdateBoard();
            return true;
        }

        public bool Right()
        {
            if (CheckOutBounding(x + 1, y, this.block.rect)) return false;

            this.x += 1;
            this.UpdateBoard();
            return true;
        }

        public bool Rotate()
        {
            var rotatedBlockData = Block.Rotate(this.block.data);
            var rect = Block.GetRect(rotatedBlockData);
            if (this.CheckOutBounding(x, y, rect)) return false;
            this.block.Rotate();
            this.UpdateBoard();
            return true;
        }

        /**
         * ����ƶ�����ת��İ�����Ч�����Ƿ�ᳬ������
         */
        private bool CheckOutBounding(int x, int y, RectInt rect)
        {
            // ������
            if (x + rect.x - 1 < 0) return true;
            // ����ұ�
            if (x + rect.x + rect.width + 1 >= this.boardWidth) return true;
            // ���ײ�
            if (y + this.block.size - rect.x - rect.width - 1 < 0) return true;
            return false;
        }
    }
}
