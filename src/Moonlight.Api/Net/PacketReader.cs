using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moonlight.Protocol.Net;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Api.Net
{
    public sealed class PacketReader
    {
        private delegate IPacket DeserializeDelegate(ref SequenceReader<byte> reader);
        private static readonly FrozenDictionary<int, DeserializeDelegate> _packetDeserializers;

        public readonly PipeReader _pipeReader;
        private readonly ILogger<PacketReader> _logger;

        static PacketReader()
        {
            Dictionary<int, DeserializeDelegate> packetDeserializers = [];

            // Iterate through the assembly and find all classes that implement IServerPacket<>
            foreach (Type type in typeof(IServerPacket<>).Assembly.GetExportedTypes())
            {
                // Ensure we grab a fully implemented packet, not IServerPacket<> or an abstract class that implements it
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                // Grab the generic argument of IServerPacket<>
                Type? packetType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServerPacket<>))?.GetGenericArguments()[0];
                if (packetType is null)
                {
                    continue;
                }

                // Grab the static Id property from Packet<>
                VarInt packetId = (VarInt)type.GetProperty("Id", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

                // Grab the deserialize method
                MethodInfo deserializeMethod = type.GetMethod("Deserialize", [(typeof(SequenceReader<byte>).MakeByRefType())])!;

                // Convert the method into a delegate
                DeserializeDelegate deserializeDelegate = (DeserializeDelegate)Delegate.CreateDelegate(typeof(DeserializeDelegate), null, deserializeMethod);

                // Now we store the pointer in a dictionary for later use
                packetDeserializers[packetId.Value] = deserializeDelegate;
            }

            _packetDeserializers = packetDeserializers.ToFrozenDictionary();
        }

        public PacketReader(Stream stream, ILogger<PacketReader>? logger = null)
        {
            _pipeReader = PipeReader.Create(stream);
            _logger = logger ?? NullLogger<PacketReader>.Instance;
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
            if (!_packetDeserializers.TryGetValue(packetId.Value, out DeserializeDelegate? packetDeserializerPointer))
            {
                // Grab the unknown packet deserializer
                packetDeserializerPointer = _packetDeserializers[-1];

                // Rewind so the unknown packet can store the received packet ID.
                reader.Rewind(packetId.Length);
            }

            reader = new(reader.Sequence.Slice(reader.Position, length.Value));
            IPacket packet = packetDeserializerPointer(ref reader);
            position = reader.Position;
            return packet;
        }
    }
}
