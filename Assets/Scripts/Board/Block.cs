/*
 * 板块定义及常用方法
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public enum BlockType
    {
        I, O, S, Z, L, J, T,
    }


    public class Block
    {

        public BlockType type;

        /**
         * 并不是指data.Length，而是指 一个正方形板块 形状的宽度，
         */
        public int size;

        /**
         * 这里的数据还是全量的， 并没有进行二进制压缩
         */
        public int[] data { get; private set; }

        private int[] _bynaryArray;

        public int[] binaryArray
        {
            get {
                _bynaryArray = _bynaryArray ?? ToBinaryArray(this.data);
                return _bynaryArray; 
            }
        }

        public Block(BlockType type, int rotateCount)
        {
            var array = BlockMap[type];
            this.type = type;
            this.size = (int)Mathf.Sqrt(array.Length);
            this.data = array;
            for (int i = 0; i < rotateCount; i++)
               this.Rotate();
    
        }

        // 顺时针旋转90度
        public void Rotate()
        {
            this.data = Block.Rotate(this);
            // 旋转过后，二进制数组需要重新生成
            this._bynaryArray = null;
        }

        public static int[] Rotate(Block block)
        {
            if (RotateBlockMap.ContainsKey(block.type))
            {
                return block.data == BlockMap[block.type] ? RotateBlockMap[block.type] : BlockMap[block.type];
            }
            var size = (int)Mathf.Sqrt(block.data.Length);
            int[] rotated = new int[block.data.Length];

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    rotated[j * size + (size - i - 1)] = block.data[i * size + j];
                }
            }
            return rotated;
        }

        public static int[] ToBinaryArray(int[] data)
        {
            var size = (int)Mathf.Sqrt(data.Length);
            var bynaryArray = new int[size];
            for (int i = 0; i < size; i++)
            {
                var newIdx = i * size;
                for (int j = newIdx; j < newIdx + size; j++)
                {
                    bynaryArray[i] += data[j] > 0 ? (int)Math.Pow(2, j - newIdx) : 0;
                }
            }
            return bynaryArray;
        }

        public static int MAX_SIZE = 4;

        private static Dictionary<BlockType, int[]> BlockMap = new Dictionary<BlockType, int[]>() {
            {
                BlockType.I, new int[]
                {
                    0,0,0,0,
                    1,1,1,1,
                    0,0,0,0,
                    0,0,0,0,
                }
            },
            {
                BlockType.O, new int[]
                {
                    1,1,
                    1,1,
                }
            },
            {
                BlockType.S, new int[]
                {
                    1,0,0,
                    1,1,0,
                    0,1,0,
                }
            },
            {
                BlockType.Z, new int[]
                {
                    0,0,0,
                    1,1,0,
                    0,1,1,
                }
                /**
                 * 0,1,0
                 * 1,1,0
                 * 1,0,0
                 */
            },
            {
                BlockType.L, new int[]
                {
                    0,1,0,
                    0,1,0,
                    0,1,1,
                }
                

            },
            {
                BlockType.J, new int[]
                {
                    0,1,0,
                    0,1,0,
                    1,1,0,
                }
            },
            {
                BlockType.T, new int[]
                {
                    0,0,0,
                    1,1,1,
                    0,1,0,
                }
            }
        };

        //部分板块需要固定旋转
        private static Dictionary<BlockType, int[]> RotateBlockMap = new Dictionary<BlockType, int[]>() {
            {
                BlockType.I, new int[]
                {
                    0,1,0,0,
                    0,1,0,0,
                    0,1,0,0,
                    0,1,0,0,
                }
            },
            {
                BlockType.S, new int[]
                {
                    0,1,1,
                    1,1,0,
                    0,0,0,
                }
            },
            {
                BlockType.Z, new int[]
                {
                    0,1,0,
                    1,1,0,
                    1,0,0,
                }
            },
        };
    }
}
