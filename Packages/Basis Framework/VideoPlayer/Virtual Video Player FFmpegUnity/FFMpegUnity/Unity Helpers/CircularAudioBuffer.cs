using System;

public class CircularAudioBuffer
{
    private float[] _buffer;
    private int _readPosition;
    private int _writePosition;

    public CircularAudioBuffer(int size)
    {
        _buffer = new float[size];
        Clear();
    }

    public void Clear()
    {
        Array.Fill(_buffer, 0f);
        _readPosition = 0;
        _writePosition = 0;
    }

    public void Read(float[] target)
    {
        if (_readPosition + target.Length < _buffer.Length)
        {
            Array.Copy(_buffer, _readPosition, target, 0, target.Length);
        }
        else
        {
            int length = _buffer.Length - _readPosition;
            Array.Copy(_buffer, _readPosition, target, 0, length);
            Array.Copy(_buffer, 0, target, length, target.Length - length);
        }
        _readPosition += target.Length;
        _readPosition %= _buffer.Length;
    }

    public void Write(float[] data)
    {
        if (_writePosition + data.Length < _buffer.Length)
        {
            Array.Copy(data, 0, _buffer, _writePosition, data.Length);
        }
        else
        {
            int length = _buffer.Length - _writePosition;
            Array.Copy(data, 0, _buffer, _writePosition, length);
            Array.Copy(data, length, _buffer, 0, data.Length - length);
        }
        _writePosition += data.Length;
        _writePosition %= _buffer.Length;
    }
}