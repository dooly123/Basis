using System;

public partial class BasisAudioReceiverBase
{
    [System.Serializable]
    public class BasisFloatCircularBuffer
    {
        public float[] Buffer;
        public int Head;
        public int Tail;
        public int BufferSize;
        public int SegmentSize;
        public int SegmentCount;
        public int CurrentCount;

        public BasisFloatCircularBuffer(int segmentSize, int segmentCount)
        {
            SegmentSize = segmentSize;
            SegmentCount = segmentCount;
            BufferSize = segmentSize * segmentCount;
            Buffer = new float[BufferSize];
            Head = 0;
            Tail = 0;
            CurrentCount = 0;
        }

        public void Clear()
        {
            Head = 0;
            Tail = 0;
            CurrentCount = 0;
            Array.Clear(Buffer, 0, BufferSize);
        }

        public void Add(float[] data)
        {
            if (data.Length != SegmentSize)
            {
                throw new ArgumentException($"Data length must be {SegmentSize}");
            }
            if (IsFull())
            {
                // Buffer is full, overwrite the oldest data
                Head = (Head + SegmentSize) % BufferSize;
          //we will want to speed up and slow down      Debug.Log("Buffer was full old data was overwritten");
            }

            Array.Copy(data, 0, Buffer, Tail, SegmentSize);
            Tail = (Tail + SegmentSize) % BufferSize;
            CurrentCount = IsFull() ? SegmentCount : CurrentCount + 1;
        }

        public float[] GetNextSegment()
        {
            if (IsEmpty())
                return null;

            float[] segment = new float[SegmentSize];
            Array.Copy(Buffer, Head, segment, 0, SegmentSize);
            Head = (Head + SegmentSize) % BufferSize;
            CurrentCount--;
            return segment;
        }

        public bool IsFull()
        {
            return CurrentCount == SegmentCount;
        }

        public bool IsEmpty()
        {
            return CurrentCount == 0;
        }
    }
}