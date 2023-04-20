using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Moonlight.Protocol.Net;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Api.Net
{
    public sealed class PacketReader
    {
        private delegate T DeserializeDelegate<T>(Span<byte> buffer);

        public IReadOnlyDictionary<VarInt, nint> PacketDeserializers => _packetDeserializers;

        private readonly Dictionary<VarInt, nint> _packetDeserializers = new();
        private readonly PipeReader _pipeReader;

        public PacketReader(Stream stream)
        {
            _pipeReader = PipeReader.Create(stream);

            Type byteSpanType = typeof(Span<byte>);

            // Iterate through the assembly and find all classes that implement IPacket<>
            foreach (Type type in typeof(IPacket<>).Assembly.GetExportedTypes())
            {
                // Ensure we grab a fully implemented packet, not IPacket<> or an abstract class that implements it
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                // Grab the generic argument of IPacket<>
                Type? packetType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPacket<>))?.GetGenericArguments()[0];
                if (packetType is null)
                {
                    continue;
                }

                // Grab the static Id property from Packet<>
                VarInt packetId = (VarInt)type.GetProperty("Id", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

                // Grab the deserialize method
                MethodInfo deserializeMethod = type.GetMethod("Deserialize", new[] { byteSpanType })!;

                // Convert the method into a delegate
                Delegate deserializeDelegate = Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(byteSpanType, packetType), deserializeMethod);

                // Since the delegate is a managed object, we need to convert it into a native pointer for performance
                nint deserializeMethodPointer = Marshal.GetFunctionPointerForDelegate(deserializeDelegate);

                // Now we store the pointer in a dictionary for later use
                _packetDeserializers.Add(packetId, deserializeMethodPointer);
            }
        }

        public async ValueTask<T> ReadPacketAsync<T>(CancellationToken cancellationToken = default) where T : IPacket<T>
        {
            ReadResult readResult = await _pipeReader.ReadAsync(cancellationToken);
            T packet = T.Deserialize(readResult.Buffer.IsSingleSegment ? readResult.Buffer.FirstSpan : readResult.Buffer.ToArray(), out int offset);
            _pipeReader.AdvanceTo(readResult.Buffer.GetPosition(offset, readResult.Buffer.Start));
            return packet;
        }

        public async ValueTask<(VarInt, IPacket)> ReadPacketAsync(CancellationToken cancellationToken = default)
        {
            ReadResult readResult = await _pipeReader.ReadAsync(cancellationToken);
            VarInt packetId = VarInt.Deserialize(readResult.Buffer.ToArray(), out _);
            if (!_packetDeserializers.TryGetValue(packetId, out nint deserializeMethodPointer))
            {
                throw new InvalidOperationException($"Unknown packet id: {packetId}");
            }

            IPacket packet = DeserializePacket(deserializeMethodPointer, readResult.Buffer.IsSingleSegment ? readResult.Buffer.FirstSpan : readResult.Buffer.ToArray(), out int offset);
            _pipeReader.AdvanceTo(readResult.Buffer.GetPosition(offset, readResult.Buffer.Start));
            return (packetId, packet);
        }

        private static unsafe IPacket DeserializePacket(nint functionPointer, ReadOnlySpan<byte> buffer, out int offset)
            => ((delegate*<ReadOnlySpan<byte>, out int, IPacket>)functionPointer)(buffer, out offset);
    }
}
