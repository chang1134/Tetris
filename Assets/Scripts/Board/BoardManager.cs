using System;
using UnityEngine;

namespace Tetris.Runtime
{
    public class BoardManager
    {
        private int boardWidth;
        private int boardHeight;

        public GameObject go_moving;
        public GameObject go_fixed;

        public GameObject go_gridItem;

        private MovingBoard movingBoard;

        private FixedBoard fixedBoard;

        private Block nextBlock;

        private IBoardView view;

        public void Init(IBoardView view, int boardWidth, int boardHeight)
        {
            this.view = view;
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            this.movingBoard = new MovingBoard(boardWidth, this.boardHeight);
            this.fixedBoard = new FixedBoard(boardWidth, this.boardHeight);
        }

        public void PrepareBoard()
        {
            CleanNextBlock();
            // ��Ϸ��ʼʱ����׼����һ�����
            PrepareNextBlock();
        }

        public void NextRoundStart()
        {
            view.OnRoundStart();

            var block = ExtractNextBlock();

            movingBoard.Reset(block);

            PrepareNextBlock();
        }

        private void RoundOver()
        {
            // ���̶����̽��кϲ�
            this.fixedBoard.Combine(this.movingBoard);

            var beforeEliminateBoardDatas = new int[this.fixedBoard.datas.Length];
            Array.Copy(this.fixedBoard.datas, beforeEliminateBoardDatas, beforeEliminateBoardDatas.Length);

            // �������
            this.fixedBoard.Eliminate();

            this.movingBoard.CleanBoard();
            view.DrawMovingBoard(this.movingBoard.datas);

            view.OnRoundOver(beforeEliminateBoardDatas, this.fixedBoard.datas, this.fixedBoard.IsGameOver());

            //view.DrawFixedBoard(this.fixedBoard.datas);

            //view.DrawMovingBoard(this.movingBoard.datas);

            //if (this.fixedBoard.IsGameOver())
            //{
            //    view.OnGameOver();
            //    Debug.Log("��Ϸ����");
            //}
            //else
            //{
            //    RunNextBlock();
            //}
        }

        /**
         * ���ݲ��������ƶ�������
         */
        public void UpdateMoveingBoard(BlockOperation operation)
        {
            switch (operation)
            {
                case BlockOperation.Down:
                    if (!canDown())
                    {
                        this.RoundOver();
                        return;
                    }
                    this.movingBoard.Down();
                    view.DrawMovingBoard(this.movingBoard.datas);
                    break;
                case BlockOperation.Left:
                    {
                        if (!this.canLeft()) return;
                        this.movingBoard.Left();
                        view.DrawMovingBoard(this.movingBoard.datas);
                    }
                    break;
                case BlockOperation.Right:
                    {
                        if (!this.canRight()) return;
                        this.movingBoard.Right();
                        view.DrawMovingBoard(this.movingBoard.datas);
                    }
                    break;
                case BlockOperation.ToBottom:
                    // TODO ��׼���õİ�� ���ܻ�û���ֵ������ϣ��ͱ��õ��ˣ� Ӧ�����ٳ�����һ�񣬲�����׹
                    var partBoardData = Utils.GenPartBoardData(this.movingBoard.block.binaryArray, this.movingBoard.x, boardWidth);
                    var y = this.movingBoard.y;
                    while(y >= 0)
                    {
                        var isPass = true;
                        for (int i = partBoardData.Length - 1; i >= 0; i--)
                        {
                            if ((partBoardData[i] > 0 && y - i < 0) || (y - i >= 0 && (this.fixedBoard.datas[y - i] & partBoardData[i]) > 0))
                            {
                                isPass = false;
                                break;
                            }
                        }
                        if (!isPass)
                        {
                            y++; // ��ǰ���޷��ϲ������Ե÷�����һ��
                            break;
                        }
                        y--;
                    }
                    if (y >= this.movingBoard.y) return;
                    this.movingBoard.ToBottom(y);
                    this.RoundOver();
                    break;
                case BlockOperation.Rotate:
                    if (!canRotate()) return;
                    this.movingBoard.Rotate();
                    view.DrawMovingBoard(this.movingBoard.datas);
                    break;
                default:
                    break;
            }
        }

        /**
         * �����һ�������ܷ�ִ�
         */
        private bool canDown()
        {
            if (this.movingBoard.datas[0] > 0) return false;
            for (int i = 0; i < this.movingBoard.block.size; i++)
            {
                var lineIdx = this.movingBoard.y - i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                if (currentMovingValue > 0 && lineIdx - 1 < 0) return false; // �ƶ����Ѿ����ڵ׶ˣ������������ƶ�
                if ((currentMovingValue & this.fixedBoard.datas[lineIdx - 1]) > 0) return false; // �ƶ������ص��� �򲻿ɵִ�
                //if (this.fixedBoard.datas[i] == 0 || this.movingBoard.datas[i + 1] == 0) continue;
                //if ((this.fixedBoard.datas[i] & this.movingBoard.datas[i + 1]) > 0) return false; 
            }
            return true;
        }

        private bool canLeft()
        {
            //if (this.movingBoard.x - 1 + this.movingBoard.block.rect.x - 1 < 0) return false;
            for (int i = 0; i < this.movingBoard.block.size; i++)
            {
                var lineIdx = this.movingBoard.y - i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                // ���������һ����������˽��棬���ٴ����ƻ��������ݻ��ԭʼ���ݲ�һ��
                var valueAfterLeftMoving = currentMovingValue >> 1;
                if (currentMovingValue != valueAfterLeftMoving << 1) return false;
                // �����ںϲ����Ƿ����ص����ݣ�����У����޷��ƶ�
                if ((valueAfterLeftMoving & this.fixedBoard.datas[lineIdx]) > 0) return false;
            }
            return true;
        }

        private bool canRight()
        {
            for (int i = 0; i < this.movingBoard.block.size; i++)
            {
                var lineIdx = this.movingBoard.y - i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                // ������ұߴ��ڸ��ӣ����޷�����
                if ((currentMovingValue & (int)Math.Pow(2, this.boardWidth - 1)) > 0) return false;
                // �ƶ���������ڸ��̶��������ݺϲ���������ص����ݣ����޷��ƶ�
                if (((currentMovingValue << 1) & this.fixedBoard.datas[lineIdx]) > 0) return false;
            }
            return true;
        }

        //  �����߼�������
        private bool canRotate()
        {
            // ����ƶ�����ת��İ�����Ч�����Ƿ�ᳬ������
            var rotatedBlockData = Block.Rotate(this.movingBoard.block);

            // ����ƶ���������ܷ�͹̶����̺ϲ�
            var binaryArray = Block.ToBinaryArray(rotatedBlockData);
            var partBoardData = Utils.GenPartBoardData(binaryArray, this.movingBoard.x, boardWidth);
            for (int i = 0; i < partBoardData.Length; i++)
            {
                if (partBoardData[i] >> this.movingBoard.x != binaryArray[i]) return false; // ����ںϲ����ֲ�����ʱ�������Ƿ�����ʧ
                if ((this.fixedBoard.datas[this.movingBoard.y - i] & partBoardData[i]) > 0) return false; // �����ת���ܷ���̶����̺ϲ�
            }

            return true;
        }


        private void PrepareNextBlock(BlockType blockType)
        {
            var block = new Block(blockType, UnityEngine.Random.Range(0, 4));

            nextBlock = block;
            // TODO ��ʾ��������
        }

        private void PrepareNextBlock()
        {
            if (nextBlock != null)
            {
                throw new Exception("nextBlock is exist!!");
            }

            var blockValues = Enum.GetValues(typeof(BlockType));

            int randomIdx = UnityEngine.Random.Range(0, blockValues.Length);

            PrepareNextBlock((BlockType)blockValues.GetValue(randomIdx));
        }

        private Block ExtractNextBlock()
        {
            if (nextBlock == null)
            {
                throw new Exception("nextBlock is not exist!!");
            }

            var tempBlock = nextBlock;
            CleanNextBlock();
            return tempBlock;
        }

        private void CleanNextBlock() { nextBlock = null; }
    }
}
