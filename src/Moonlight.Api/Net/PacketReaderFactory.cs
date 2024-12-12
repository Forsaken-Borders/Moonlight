using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Moonlight.Protocol.Net;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Api.Net
{
    public delegate IPacket DeserializerDelegate(ref SequenceReader<byte> reader);

    public sealed class PacketReaderFactory
    {
        public Dictionary<int, DeserializerDelegate> PacketDeserializers { get; init; } = [];
        public FrozenDictionary<int, DeserializerDelegate> PreparedPacketDeserializers { get; private set; } = FrozenDictionary<int, DeserializerDelegate>.Empty;

        private readonly ILoggerFactory _loggerFactory;

        public PacketReaderFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

        public void Prepare() => PreparedPacketDeserializers = PacketDeserializers.ToFrozenDictionary();

        public PacketReader CreatePacketReader(Stream stream) => new(this, stream, _loggerFactory.CreateLogger<PacketReader>());

        public void AddPacketDeserializer<T>(T serverPacket) where T : IServerPacket<T> =>
            PacketDeserializers[T.Id] = (DeserializerDelegate)Delegate.CreateDelegate(typeof(T), serverPacket, ((Delegate)T.Deserialize).Method);

        public void AddPacketDeserializer<T>() where T : IServerPacket<T> => PacketDeserializers[T.Id] = Unsafe.As<DeserializerDelegate>((Delegate)T.Deserialize);
        public void AddPacketDeserializer(Type type)
        {
            if (type.IsAbstract)
            {
                throw new InvalidOperationException("Cannot use an abstract class as a packet deserializer.");
            }
            // See if the type implements IServerPacket<>
            else if (type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServerPacket<>))?.GetGenericArguments()[0] is null)
            {
                throw new InvalidOperationException("Cannot use a class that does not implement IServerPacket<> as a packet deserializer.");
            }
            // Grab the IPacket<>.Id property
            else if (type.GetProperty("Id", BindingFlags.Public | BindingFlags.Static)!.GetValue(null) is not VarInt packetId)
            {
                throw new InvalidOperationException("Cannot use a class that does not have a static Id property as a packet deserializer.");
            }
            else
            {
                // Grab the Deserialize method
                MethodInfo deserializeMethod = type.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static, null, [(typeof(SequenceReader<byte>).MakeByRefType())], null)!;

                // Convert the method into a delegate
                DeserializerDelegate deserializeDelegate = (DeserializerDelegate)Delegate.CreateDelegate(typeof(DeserializerDelegate), null, deserializeMethod);

                // Store the delegate in the dictionary
                PacketDeserializers[packetId] = deserializeDelegate;
            }
        }

        public void AddPacketDeserializer(int packetId, Type type)
        {
            if (type.IsAbstract)
            {
                throw new InvalidOperationException("Cannot use an abstract class as a packet deserializer.");
            }

            Type? packetType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServerPacket<>))?.GetGenericArguments()[0];
            if (packetType is null)
            {
                return;
            }

            MethodInfo deserializeMethod = type.GetMethod("Deserialize", [(typeof(SequenceReader<byte>).MakeByRefType())])!;
            DeserializerDelegate deserializeDelegate = (DeserializerDelegate)Delegate.CreateDelegate(typeof(DeserializerDelegate), null, deserializeMethod);
            PacketDeserializers[packetId] = deserializeDelegate;
        }

        public void AddDefaultPacketDeserializers() => AddPacketDeserializers(typeof(IServerPacket<>).Assembly.GetExportedTypes());

        public void AddPacketDeserializers(IEnumerable<Type> types)
        {
            ILogger<PacketReaderFactory> logger = _loggerFactory.CreateLogger<PacketReaderFactory>();

            // Iterate through the assembly and find all classes that implement IServerPacket<>
            foreach (Type type in types)
            {
                // Ensure we grab a fully implemented packet, not IServerPacket<> or an abstract class that implements it
                if (type.IsAbstract)
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
                MethodInfo deserializeMethod = type.GetMethod("Deserialize", [(typeof(SequenceReader<byte>).MakeByRefType())]) ?? throw new InvalidOperationException($"Could not find the method 'Deserialize' in '{type.Name}'.");

                // Convert the method into a delegate
                DeserializerDelegate deserializeDelegate = (DeserializerDelegate)Delegate.CreateDelegate(typeof(DeserializerDelegate), deserializeMethod);

                // Now we store the pointer in a dictionary for later use
                if (PacketDeserializers.TryGetValue(packetId.Value, out DeserializerDelegate? existingDelegate))
                {
                    logger.LogWarning("Failed to add packet deserializer for packet {PacketId} ({PacketType}), a deserializer for this packet already exists: {ExistingDelegate}", packetId, packetType, existingDelegate);
                    continue;
                }

                PacketDeserializers[packetId.Value] = deserializeDelegate;
            }
        }

        public void RemovePacketDeserializer(int packetId) => PacketDeserializers.Remove(packetId);
        public void RemovePacketDeserializer<T>() where T : IServerPacket<T> => PacketDeserializers.Remove(T.Id);
        public void RemovePacketDeserializer(Type type)
        {
            if (type.IsAbstract)
            {
                return;
            }

            Type? packetType = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServerPacket<>))?.GetGenericArguments()[0];
            if (packetType is null)
            {
                return;
            }

            VarInt packetId = (VarInt)type.GetProperty("Id", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
            PacketDeserializers.Remove(packetId);
        }
    }
}
