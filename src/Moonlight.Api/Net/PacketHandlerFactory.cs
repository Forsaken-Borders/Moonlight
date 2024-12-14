using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moonlight.Protocol.Net;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Api.Net
{
    public delegate IPacket DeserializerDelegate(ref SequenceReader<byte> reader);
    public delegate int CalculateSizeDelegate(IPacket packet);
    public delegate int SerializerDelegate(IPacket packet, Span<byte> target);

    public sealed class PacketHandlerFactory
    {
        public Dictionary<int, DeserializerDelegate> PacketDeserializers { get; init; } = [];
        public Dictionary<int, CalculateSizeDelegate> PacketSizeCalculators { get; init; } = [];
        public Dictionary<int, SerializerDelegate> PacketSerializers { get; init; } = [];

        public FrozenDictionary<int, DeserializerDelegate> PreparedPacketDeserializers { get; private set; } = FrozenDictionary<int, DeserializerDelegate>.Empty;
        public FrozenDictionary<int, CalculateSizeDelegate> PreparedPacketSizeCalculators { get; private set; } = FrozenDictionary<int, CalculateSizeDelegate>.Empty;
        public FrozenDictionary<int, SerializerDelegate> PreparedPacketSerializers { get; private set; } = FrozenDictionary<int, SerializerDelegate>.Empty;

        private readonly ILoggerFactory _loggerFactory;

        public PacketHandlerFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

        public PacketHandler Create(Stream stream) => new(this, stream, _loggerFactory.CreateLogger<PacketHandler>());

        public void Prepare()
        {
            PreparedPacketDeserializers = PacketDeserializers.ToFrozenDictionary();
            PreparedPacketSizeCalculators = PacketSizeCalculators.ToFrozenDictionary();
            PreparedPacketSerializers = PacketSerializers.ToFrozenDictionary();
        }


        public void RegisterPacket<T>() where T : IPacket<T>
        {
            PacketDeserializers[T.Id] = (ref SequenceReader<byte> reader) => T.Deserialize(ref reader);
            PacketSizeCalculators[T.Id] = (IPacket packet) => T.CalculateSize((T)packet);
            PacketSerializers[T.Id] = (IPacket packet, Span<byte> target) => T.Serialize((T)packet, target);
        }

        public void RegisterPacket<T>(VarInt id) where T : IPacket<T>
        {
            PacketDeserializers[id] = (ref SequenceReader<byte> reader) => T.Deserialize(ref reader);
            PacketSizeCalculators[id] = (IPacket packet) => T.CalculateSize((T)packet);
            PacketSerializers[id] = (IPacket packet, Span<byte> target) => T.Serialize((T)packet, target);
        }

        public void RegisterPacket(Type type)
        {
            if (!type.GetInterfaces().Any(inter => inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(IPacket<>)))
            {
                throw new ArgumentException("Type must implement IPacket<T>.");
            }
            else if (type.GetProperty("Id")?.GetValue(null) is not VarInt id)
            {
                throw new ArgumentException("Type must have a static Id property.");
            }
            else
            {
                RegisterPacket(id, type);
            }
        }

        public void RegisterPacket(VarInt id, Type type)
        {
            if (!type.GetInterfaces().Any(inter => inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(IPacket<>)))
            {
                throw new ArgumentException("Type must implement IPacket<T>.");
            }

            typeof(PacketHandlerFactory).GetMethod(nameof(RegisterPacket), [typeof(VarInt)])!.MakeGenericMethod(type).Invoke(this, [id]);
        }

        public void UnregisterPacket<T>() where T : IPacket<T> => UnregisterPacket(T.Id);
        public void UnregisterPacket(Type type)
        {
            if (!type.GetInterfaces().Any(inter => inter.IsGenericType && inter.GetGenericTypeDefinition() == typeof(IPacket<>)))
            {
                throw new ArgumentException("Type must implement IPacket<T>.");
            }
            else if (type.GetProperty("Id")?.GetValue(null) is not VarInt id)
            {
                throw new ArgumentException("Type must have a static Id property.");
            }
            else
            {
                UnregisterPacket(id);
            }
        }

        public void UnregisterPacket(int id)
        {
            PacketDeserializers.Remove(id);
            PacketSizeCalculators.Remove(id);
            PacketSerializers.Remove(id);
        }
    }
}
