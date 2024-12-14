using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moonlight.Protocol.Components.Chat;
using Moonlight.Protocol.VariableTypes;

namespace Moonlight.Protocol.Net.StatusState
{
    public sealed record StatusResponsePacket : IPacket<StatusResponsePacket>
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public sealed record ServerVersion
        {
            public required string Name { get; init; }
            public required int Protocol { get; init; }
        }

        public sealed record ServerPlayers
        {
            public sealed record OfflinePlayer
            {
                public required string Name { get; init; }
                public required Guid Id { get; init; }
            }

            public required int Max { get; init; }
            public required int Online { get; init; }
            public IReadOnlyList<OfflinePlayer> Sample { get; init; } = Array.Empty<OfflinePlayer>();
        }

        public static VarInt Id => 0x00;
        public required ServerVersion Version { get; init; }
        public required ServerPlayers Players { get; init; }
        public required ChatComponent Description { get; init; }
        public string? Favicon { get; init; }

        [JsonPropertyName("enforcesSecureChat")]
        public required bool EnforcesSecureChat { get; init; }

        public StatusResponsePacket() { }

        public static int CalculateSize(StatusResponsePacket packet)
        {
            int jsonLength = JsonSerializer.Serialize(packet, _jsonSerializerOptions).Length;
            return VarInt.CalculateSize(jsonLength) + jsonLength;
        }

        public static int Serialize(StatusResponsePacket packet, Span<byte> target)
        {
            byte[] array = JsonSerializer.SerializeToUtf8Bytes(packet, _jsonSerializerOptions);
            int position = VarInt.Serialize(array.Length, target);
            array.CopyTo(target[position..]);
            return position + array.Length;
        }

        public static bool TryDeserialize(ref SequenceReader<byte> reader, out StatusResponsePacket packet)
        {
            try
            {
                Utf8JsonReader jsonReader = new(reader.UnreadSequence);
                packet = JsonSerializer.Deserialize<StatusResponsePacket>(ref jsonReader, _jsonSerializerOptions)!;
                return packet is not null;
            }
            catch (Exception)
            {
                packet = null!;
                return false;
            }
        }

        public static StatusResponsePacket Deserialize(ref SequenceReader<byte> reader)
        {
            Utf8JsonReader jsonReader = new(reader.UnreadSequence);
            return JsonSerializer.Deserialize<StatusResponsePacket>(ref jsonReader, _jsonSerializerOptions)!;
        }

        public static string? GetServerIcon(string? filepath)
        {
            if (filepath is null)
            {
                return null;
            }
            else if (!File.Exists(filepath))
            {
                throw new FileNotFoundException("The specified file does not exist.", filepath);
            }

            return $"data:image/png;base64,{Convert.ToBase64String(File.ReadAllBytes(filepath))}";
        }
    }
}
