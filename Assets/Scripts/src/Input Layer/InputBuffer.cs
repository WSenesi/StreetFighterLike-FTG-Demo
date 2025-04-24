using System;

namespace src.Input_Layer
{
    /// <summary>
    /// 输入缓冲区，内部采用数组的环形缓冲区实现。
    /// 只从单侧读写数据，定义新写入的数据为顶部，先前的数据为底部，超出容量时会覆盖最底部的数据
    /// 容量由创建时输入的参数确定，不可扩充
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InputBuffer<T> where T : ISignal
    {
        private T[] _data;                 
        private int _capacity;                 // 容量
        private int _front, _rear;            // 头尾指针。从尾指针添加元素，从头指针读取元素

        public T[] Buffer => _data;
        public int Capacity => _capacity;
        public int Front => _front;
        public int Rear => _rear;
    
        /// <summary>
        /// 构造函数，实例化顺序表队列。初始化头尾指针。
        /// </summary>
        /// <param name="queueSize"></param>
        public InputBuffer(int queueSize = 50)
        {
            if (queueSize < 1)
            {
                throw new Exception("容量不可小于1");
            }
            _capacity = queueSize;
            _data = new T[queueSize];

            _front = _rear = 0;
        }

        /// <summary>
        /// 从顶部向缓冲区写入数据
        /// </summary>
        /// <param name="item">写入的数据</param>
        public void Write(T item)
        {
            // 两种情况：满了 和 没满
            // 判断条件：尾指针在头指针前一位
            // 没满: 尾指针向后一位,并填入数据
            // 满了：还需要覆盖头指针指向的元素，同时调整头尾指针
            int rearNextIndex = (_rear + 1) % _capacity;
            if (rearNextIndex == _front)
            {
                _front = (_front + 1) % _capacity;
            }
            _rear = rearNextIndex;
            _data[_rear] = item;
        }

        /// <summary>
        /// 从顶部读取第 index 位的数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Read(int index)
        {
            if (index < 0 || index >= _capacity)
                throw new IndexOutOfRangeException();
            int realIndex = (_rear - index + _capacity) % _capacity;
            return _data[realIndex];
        }
    
        /// <summary>
        /// 从输入缓冲区中截取指定<b>帧数长度</b>的窗口。
        /// 窗口起始索引一定从顶部开始（从当前帧开始，追溯一定帧数以内的输入序列）
        /// 由于存储的信号包含持续帧数，因此帧数长度不等于窗口长度
        /// </summary>
        /// <param name="windowLength">窗口的帧数长度</param>
        /// <returns>返回窗口内的元素数量。当缓冲区为空时返回-1。正常情况下返回值至少为1</returns>
        public int GetWindowIndices(int windowLength)
        {
            // 特殊情况：缓冲区为空
            if (IsEmpty())
                return -1;
        
            int currentIndex = _rear;
            int remainingFrames = windowLength;
            int count = 0;
        
            while (remainingFrames > 0 && currentIndex != _front)
            {
                remainingFrames -= _data[currentIndex].Duration;
                count++;
                currentIndex = (currentIndex - 1 + _capacity) % _capacity;
            }

            return count;
        }
    
        /// <summary>
        /// 从队首弹出一个元素, 队列为空时抛出异常
        /// </summary>
        /// <returns></returns>
        public T DeQueue()
        {
            if (IsEmpty())
            {
                throw new Exception("队列为空, 无法出队元素");
            }
            T ret = _data[_front];
            _front = (_front - 1 + _capacity) % _capacity;
            return ret;
        }

        /// <summary>
        /// 判断队列是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            if (_capacity == 1)
            {
                throw new Exception("容量为1的队列无法判断是否为空");
            }
            return _front == _rear;
        }
 
        /// <summary>
        /// 清空队列,初始化头尾指针
        /// </summary>
        public void Clear()
        {
            _data = new T[_capacity];
            _front = _rear = 0;
        }

        /// <summary>
        /// 输入两个索引，比较两个元素是否相同
        /// </summary>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <returns></returns>
        public bool IsTwoElementEqual(int index1, int index2)
        {
            int realIndex1 = (_rear - index1 + _capacity) % _capacity;
            int realIndex2 = (_rear - index2 + _capacity) % _capacity;

            return _data[realIndex1].Equals(_data[realIndex2]);
        }
    }
}
