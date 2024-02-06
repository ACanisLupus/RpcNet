// Copyright by Artur Wolf

namespace RpcNet.Internal;

using System.Net.Sockets;

// Public for tests
public sealed class TcpReader : INetworkReader
{
    private const int TcpHeaderLength = 4;

    private readonly byte[] _buffer;
    private readonly ILogger? _logger;

    private int _bodyIndex;
    private int _headerIndex;
    private bool _lastPacket;
    private PacketState _packetState = PacketState.Header;
    private int _readIndex;
    private Socket _tcpClient;
    private int _writeIndex;

    public TcpReader(Socket tcpClient, ILogger? logger = default) : this(tcpClient, 65536, logger)
    {
    }

    public TcpReader(Socket tcpClient, int bufferSize, ILogger? logger = default)
    {
        if ((bufferSize < (TcpHeaderLength + sizeof(int))) || ((bufferSize % 4) != 0))
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        _logger = logger;

        _tcpClient = tcpClient;
        _buffer = new byte[bufferSize];
    }

    public void Reset(Socket tcpClient) => _tcpClient = tcpClient;

    public NetworkReadResult BeginReading()
    {
        _readIndex = 0;
        _writeIndex = 0;
        _lastPacket = false;
        _headerIndex = 0;
        _bodyIndex = 0;
        _packetState = PacketState.Header;

        return FillBuffer();
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
        NetworkReadResult networkReadResult = FillBuffer();
        if (networkReadResult.HasError)
        {
            throw new RpcException($"Could not receive from TCP stream. Socket error: {networkReadResult.SocketError}.");
        }

        if (networkReadResult.IsDisconnected)
        {
            throw new RpcException("Could not receive from TCP stream. Remote end point disconnected.");
        }

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
    // - Packet is complete and XDR read is not complete? Return and wait for XDF read
    // - Packet and XDR read is complete? Read next header. Or finish if previous packet was the last packet
    private NetworkReadResult FillBuffer()
    {
        bool readFromNetwork = false;
        while (true)
        {
            if (_packetState == PacketState.Complete)
            {
                return NetworkReadResult.CreateSuccess();
            }

            if (_packetState == PacketState.Header)
            {
                ReadHeader(ref readFromNetwork);
            }

            if (_packetState == PacketState.Body)
            {
                if (ReadBody(ref readFromNetwork))
                {
                    return NetworkReadResult.CreateSuccess();
                }
            }

            if (readFromNetwork)
            {
                NetworkReadResult networkReadResult = ReadFromNetwork(ref readFromNetwork);
                if (networkReadResult.HasError || networkReadResult.IsDisconnected)
                {
                    return networkReadResult;
                }
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

    private NetworkReadResult ReadFromNetwork(ref bool readFromNetwork)
    {
        int receivedLength;
        SocketError socketError;
        try
        {
            receivedLength = _tcpClient.Receive(_buffer, _writeIndex, _buffer.Length - _writeIndex, SocketFlags.None, out socketError);
        }
        catch (SocketException exception)
        {
            return NetworkReadResult.CreateError(exception.SocketErrorCode);
        }
        catch (Exception exception)
        {
            _logger?.Error($"Unexpected error while receiving TCP data from {_tcpClient.RemoteEndPoint}: {exception}");
            return NetworkReadResult.CreateError(SocketError.SocketError);
        }

        if (socketError != SocketError.Success)
        {
            return NetworkReadResult.CreateError(socketError);
        }

        if (receivedLength == 0)
        {
            return NetworkReadResult.CreateDisconnected();
        }

        _writeIndex += receivedLength;
        readFromNetwork = false;
        return NetworkReadResult.CreateSuccess();
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
