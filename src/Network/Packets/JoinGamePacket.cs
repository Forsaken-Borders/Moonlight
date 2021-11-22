using System;
using fNbt;
using Microsoft.Extensions.Configuration;
using Moonlight.Types;

namespace Moonlight.Network.Packets;

public class JoinGamePacket
{
    public int EntityId { get; init; }
    public bool IsHardcore { get; init; }
    public Gamemode Gamemode { get; init; }
    public Gamemode PreviousGamemode { get; init; }
    public int WorldCount { get; init; }
    public string[] WorldNames { get; init; }
    public NbtCompound DimensionCodec { get; init; } = new();
    public NbtCompound Dimension { get; init; } = new();
    public string WorldName { get; init; }
    public long HashedSeed { get; init; }

    [Obsolete("Notchian Minecraft clients now ignore this property.", false)]
    public int MaxPlayers { get; init; } = Server.Configuration.GetValue("server:max_players", 100);
    public int ViewDistance { get; init; }
    public bool ReducedDebugInfo { get; init; }
    public bool EnableRespawnScreen { get; init; }
    public bool IsDebug { get; init; }
    public bool IsFlat { get; init; }
}
