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

        private GameStatus _gameStatus = GameStatus.Prepare;

        private float nextDownWaitingTime = 0;

        private static float AUTO_DOWN_INTERVAL = 0.2f;

        private MovingBoard movingBoard;

        private FixedBoard fixedBoard;

        private Block nextBlock;

        private IBoardView view;

        public void Init(IBoardView view, int boardWidth, int boardHeight)
        {
            this.view = view;
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            this.movingBoard = new MovingBoard(boardWidth, boardHeight);
            this.fixedBoard = new FixedBoard(boardWidth, boardHeight);
        }

        public void StartGame()
        {
            // ��Ҫ�Ȳ��ſ�������
            gameStatus = GameStatus.Running;
            nextDownWaitingTime = 0;

            CleanNextBlock();
            // ��Ϸ��ʼʱ����׼����һ�����
            PrepareNextBlock();

            // ��ʼ��һ���
            RunNextBlock();
        }

        private void RunNextBlock()
        {
            var block = ExtractNextBlock();

            movingBoard.Reset(block);

            PrepareNextBlock();
        }

        /**
         * ���ݲ��������ƶ�������
         */
        private void UpdateMoveingBoard(BlockOperation operation)
        {
            switch (operation)
            {
                case BlockOperation.Down:
                    if (!canDown())
                    {
                        // ���̶����̽��кϲ�
                        this.fixedBoard.Combine(this.movingBoard);
                        view.DrawFixedBoard(this.fixedBoard.datas);
                        if (this.fixedBoard.IsGameOver())
                        {
                            gameStatus = GameStatus.GameOver;
                            Debug.Log("��Ϸ����");
                        } else
                        {
                            RunNextBlock();
                        }
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

                    break;
                case BlockOperation.Rotate:
                    //var newLineBlockData = Block.Create(movingBoard.lineBlockInfo.type, movingBoard.lineBlockInfo.rotateCount + 1);


                    //movingBoardInfo.lineBlockInfo.rotateCount++;
                    //movingBoardInfo.lineBlockInfo.lineBlockData = 
                    //DrawBoard(BoardType.Moving);
                    break;
                default:
                    break;
            }
        }

        private GameStatus gameStatus
        {
            get { return _gameStatus; }
            set
            {
                if (_gameStatus == value) return;
                _gameStatus = value;
                if (value == GameStatus.Pause)
                {
                    // ��ʾ��ͣ����
                }
                else
                {
                    // ��ʾ��Ϸ����
                }
            }
        }

        /**
         * 
         */
        public void Tick(float speedRate)
        {

            if (gameStatus == GameStatus.Running)
            {
                // ��������
                nextDownWaitingTime += Time.deltaTime;
                if (nextDownWaitingTime > AUTO_DOWN_INTERVAL / speedRate)
                {
                    nextDownWaitingTime = 0;

                    UpdateMoveingBoard(BlockOperation.Down);
                }
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
                var lineIdx = this.movingBoard.y + i;
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
                var lineIdx = this.movingBoard.y + i;
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
                var lineIdx = this.movingBoard.y + i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                // ���������һ����������˽��棬���ٴ����ƻ��������ݻ��ԭʼ���ݲ�һ��
                var valueAfterLeftMoving = currentMovingValue << 1;
                if (currentMovingValue != valueAfterLeftMoving >> 1) return false;
                // �ƶ���������ڸ��̶��������ݺϲ���������ص����ݣ����޷��ƶ�
                if ((valueAfterLeftMoving & this.fixedBoard.datas[lineIdx]) > 0) return false;
            }
            return true;
        }

        private bool canRotate()
        {
            // ����ƶ�����ת��İ�����Ч�����Ƿ�ᳬ������
            var rotatedBlockData = Block.Rotate(this.movingBoard.block.data);
            var rect = Block.GetRect(rotatedBlockData);

            // ������
            if (this.movingBoard.x + rect.x - 1 < 0) return false;
            // ����ұ�
            if (this.movingBoard.x + rect.x + rect.width + 1 >= this.boardWidth) return false;
            // ���ײ�
            if (this.movingBoard.y + this.movingBoard.block.size - rect.x - rect.width - 1 < 0) return false;

            // ����ƶ���������ܷ�͹̶����̺ϲ�
            var binaryArray = Block.ToBinaryArray(rotatedBlockData);


            return false;
        }


        private void PrepareNextBlock(BlockType blockType, bool isRandom)
        {
            var block = new Block(blockType, UnityEngine.Random.Range(0, 4));

            //LineBlockInfo lineBlockInfo = new LineBlockInfo();
            //lineBlockInfo.type = blockType;
            //var rotateCount = ;
            //var blockData = Block.Create(blockType, rotateCount);
            //var lineBlockData = new int[blockData.Length];
            ////var startX = Mathf.CeilToInt((boardWidth - blockData.Length) / 2f);
            ////for (int i = 0; i < blockData.Length; i++)
            ////{
            ////    lineBlockData[i] = blockData[i] << startX;
            ////}
            ////lineBlockInfo.blockPosX = startX;
            //lineBlockInfo.rotateCount = rotateCount;
            //lineBlockInfo.lineBlockData = lineBlockData;
            nextBlock = block;
            //nextLineBlock = lineBlockInfo;
            // ��ʾ��������
        }

        private void PrepareNextBlock()
        {
            if (nextBlock != null)
            {
                throw new Exception("nextBlock is exist!!");
            }

            var blockValues = Enum.GetValues(typeof(BlockType));

            int randomIdx = UnityEngine.Random.Range(0, blockValues.Length);

            PrepareNextBlock((BlockType)blockValues.GetValue(randomIdx), true);
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
