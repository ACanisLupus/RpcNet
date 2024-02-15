// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net;
using System.Net.Sockets;

// Public for tests
public sealed class TcpReader : INetworkReader
{
    private const int TcpHeaderLength = 4;

    private readonly byte[] _buffer;
    private readonly Socket _socket;

    private int _bodyIndex;
    private int _headerIndex;
    private bool _lastPacket;
    private PacketState _packetState = PacketState.Header;
    private int _readIndex;
    private int _writeIndex;

    public TcpReader(Socket socket) : this(socket, 65536)
    {
    }

    public TcpReader(Socket socket, int bufferSize)
    {
        if ((bufferSize < (TcpHeaderLength + sizeof(int))) || ((bufferSize % 4) != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _socket = socket;
        _buffer = new byte[bufferSize];
    }

    public EndPoint BeginReading()
    {
        _readIndex = 0;
        _writeIndex = 0;
        _lastPacket = false;
        _headerIndex = 0;
        _bodyIndex = 0;
        _packetState = PacketState.Header;

        FillBuffer();
        return _socket.RemoteEndPoint!;
    }

    public void EndReading()
    {
        // Just read to the end. Obviously, this is an unknown procedure
        while ((_packetState != PacketState.Complete) || (_readIndex != _writeIndex))
        {
            int length = Math.Max(_writeIndex - _readIndex, 1);
            _ = Read(length);
        }
    }

    public ReadOnlySpan<byte> Read(int length)
    {
        FillBuffer();

        int endIndex = Math.Min(_headerIndex, _buffer.Length);
        endIndex = Math.Min(endIndex, _writeIndex);
        int availableBytes = endIndex - _readIndex;
        int bytesToRead = Math.Min(availableBytes, length);

        Span<byte> span = _buffer.AsSpan(_readIndex, bytesToRead);
        _readIndex += bytesToRead;
        return span;
    }

    // On the first iteration, this function will read as many data from the network as available
    // On the following iterations, it depends on the yet received data:
    // - Not enough bytes for header? Read from network again
    // - Packet is not complete and there is space left in the buffer? Read from network again
    // - Packet is not complete and no space available? Return and wait for XDR read
    // - Packet is complete and XDR read is not complete? Return and wait for XDR read
    // - Packet and XDR read is complete? Read next header. Or finish if previous packet was the last packet
    private void FillBuffer()
    {
        bool readFromNetwork = false;
        while (true)
        {
            if (_packetState == PacketState.Complete)
            {
                return;
            }

            if (_packetState == PacketState.Header)
            {
                ReadHeader(ref readFromNetwork);
            }

            if (_packetState == PacketState.Body)
            {
                if (ReadBody(ref readFromNetwork))
                {
                    return;
                }
            }

            if (readFromNetwork)
            {
                ReadFromNetwork();
            }

            ShiftData();
        }
    }

    private void ShiftData()
    {
        if ((_readIndex == _writeIndex) && (_writeIndex > 0))
        {
            _bodyIndex -= _writeIndex;
            _headerIndex -= _writeIndex;
            _writeIndex = 0;
            _readIndex = 0;
        }
    }

    private bool ReadBody(ref bool readFromNetwork)
    {
        if ((_writeIndex == _headerIndex) && _lastPacket)
        {
            _packetState = PacketState.Complete;
            return true;
        }

        if (_readIndex < _headerIndex)
        {
            if ((_writeIndex < _headerIndex) && (_writeIndex < _buffer.Length))
            {
                readFromNetwork = true;
            }
            else if (_readIndex < _buffer.Length)
            {
                return true;
            }
        }
        else
        {
            _packetState = PacketState.Header;
        }

        return false;
    }

    private void ReadFromNetwork()
    {
        try
        {
            int receivedLength = _socket.Receive(_buffer.AsSpan(_writeIndex, _buffer.Length - _writeIndex));
            if (receivedLength == 0)
            {
                throw new RpcException($"{_socket.RemoteEndPoint} disconnected.");
            }

            _writeIndex += receivedLength;
        }
        catch (SocketException e)
        {
            throw new RpcException($"Could not read from {_socket.RemoteEndPoint}. Socket error code: {e.SocketErrorCode}.");
        }
    }

    private void ReadHeader(ref bool readFromNetwork)
    {
        if (_writeIndex >= (_headerIndex + TcpHeaderLength))
        {
            int packetLength = Utilities.ToInt32BigEndian(_buffer.AsSpan(_headerIndex, TcpHeaderLength));
            if (packetLength < 0)
            {
                _lastPacket = true;
                packetLength &= 0x0fffffff;
            }

            if (((packetLength % 4) != 0) || (packetLength == 0))
            {
                throw new RpcException("This is not an XDR stream.");
            }

            _packetState = PacketState.Body;
            _bodyIndex = _headerIndex + TcpHeaderLength;
            _headerIndex = _bodyIndex + packetLength;
            _readIndex = _bodyIndex;
        }
        else
        {
            readFromNetwork = true;
        }
    }

    private enum PacketState
    {
        Header,
        Body,
        Complete
    }
}
