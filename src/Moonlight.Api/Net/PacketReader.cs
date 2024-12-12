using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moonlight.Protocol.Net;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Api.Net
{
    public sealed class PacketReader : IDisposable
    {
        private readonly PacketReaderFactory _factory;
        private readonly Stream _stream;
        private readonly ILogger<PacketReader> _logger;
        private readonly PipeReader _pipeReader;
        private object? _disposed;

        public PacketReader(PacketReaderFactory factory, Stream stream, ILogger<PacketReader> logger)
        {
            _factory = factory;
            _stream = stream;
            _pipeReader = PipeReader.Create(_stream);
            _logger = logger;
        }

        public async ValueTask<T> ReadPacketAsync<T>(CancellationToken cancellationToken = default) where T : IPacket<T>
        {
            ReadResult readResult = await _pipeReader.ReadAsync(cancellationToken);
            if (readResult.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            T? packet = ReadPacket<T>(readResult.Buffer, out SequencePosition position);
            if (packet is null)
            {
                _pipeReader.AdvanceTo(readResult.Buffer.Start, position);
                return await ReadPacketAsync<T>(cancellationToken);
            }

            _pipeReader.AdvanceTo(position);
            return packet;
        }

        public async ValueTask<IPacket> ReadPacketAsync(CancellationToken cancellationToken = default)
        {
            ReadResult readResult = await _pipeReader.ReadAsync(cancellationToken);
            if (readResult.IsCanceled || cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            _logger.LogInformation("Read {Bytes} bytes", readResult.Buffer.Length);
            _logger.LogDebug("Buffer: [{Buffer}]", string.Join(", ", readResult.Buffer.ToArray().Select(b => b.ToString("X2"))));
            IPacket? packet = ReadPacket(readResult.Buffer, out SequencePosition position);
            if (packet is null)
            {
                _pipeReader.AdvanceTo(readResult.Buffer.Start, position);
                return await ReadPacketAsync(cancellationToken);
            }

            _pipeReader.AdvanceTo(position);
            return packet;
        }

        public T? ReadPacket<T>(ReadOnlySequence<byte> sequence, out SequencePosition position) where T : IPacket<T>
        {
            SequenceReader<byte> reader = new(sequence);
            VarInt length = VarInt.Deserialize(ref reader);
            if (length.Value > reader.Remaining)
            {
                position = sequence.End;
                return default;
            }

            VarInt packetId = VarInt.Deserialize(ref reader);
            if (T.Id != packetId)
            {
                _pipeReader.CancelPendingRead();
                throw new InvalidDataException($"Expected packet ID {T.Id}, but got {packetId}");
            }

            reader = new(reader.Sequence.Slice(reader.Position, length.Value));
            T packet = T.Deserialize(ref reader);
            position = reader.Position;
            return packet;
        }

        public IPacket? ReadPacket(ReadOnlySequence<byte> sequence, out SequencePosition position)
        {
            SequenceReader<byte> reader = new(sequence);
            VarInt length = VarInt.Deserialize(ref reader);
            if (length.Value > reader.Remaining)
            {
                position = sequence.End;
                return null;
            }

            VarInt packetId = VarInt.Deserialize(ref reader);
            if (!_factory.PreparedPacketDeserializers.TryGetValue(packetId.Value, out DeserializerDelegate? packetDeserializerPointer))
            {
                // Grab the unknown packet deserializer
                packetDeserializerPointer = _factory.PreparedPacketDeserializers[-1];

                // Rewind so the unknown packet can store the received packet ID.
                reader.Rewind(packetId.Length);
            }

            reader = new(reader.Sequence.Slice(reader.Position, length.Value));
            IPacket packet = packetDeserializerPointer(ref reader);
            position = reader.Position;
            return packet;
        }

        public void Dispose()
        {
            if (_disposed is not null)
            {
                return;
            }

            _disposed = new object();
            GC.SuppressFinalize(this);
        }
    }
}
