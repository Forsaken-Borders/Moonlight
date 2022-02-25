using Moonlight.Api.Minecraft.Abstractions.Networking.Packets;
using Moonlight.Api.Minecraft.Objects.Networking;

namespace Moonlight.Api.Minecraft.Abstractions.Networking
{
    public interface IPacketHandler : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// The stream used to communicate with the server.
        /// </summary>
        MinecraftStream Stream { get; }

        /// <summary>
        /// The cancellation token used in async methods.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Whether the client is disconnected from the server.
        /// </summary>
        bool IsDisposed { get; }

        #region ReadMethods
        /// <summary>
        /// Reads a byte from the stream.
        /// </summary>
        /// <returns>A singular byte.</returns>
        byte ReadByte();

        /// <summary>
        /// Reads a signed byte from the stream.
        /// </summary>
        /// <returns>A singular signed byte.</returns>
        sbyte ReadSignedByte();

        /// <summary>
        /// Reads a boolean from the stream.
        /// </summary>
        /// <returns>A boolean.</returns>
        bool ReadBool();

        /// <summary>
        /// Reads a short from the stream.
        /// </summary>
        /// <returns>A short.</returns>
        short ReadShort();

        /// <summary>
        /// Reads a unsigned short from the stream.
        /// </summary>
        /// <returns>A unsigned short.</returns>
        ushort ReadUnsignedShort();

        /// <summary>
        /// Reads an integer from the stream.
        /// </summary>
        /// <returns>An integer.</returns>
        int ReadInt();

        /// <summary>
        /// Reads a unsigned integer from the stream.
        /// </summary>
        /// <returns>A unsigned integer.</returns>
        uint ReadUnsignedInt();

        /// <summary>
        /// Reads a long from the stream.
        /// </summary>
        /// <returns>A long.</returns>
        long ReadLong();

        /// <summary>
        /// Reads a unsigned long from the stream.
        /// </summary>
        /// <returns>A unsigned long.</returns>
        ulong ReadUnsignedLong();

        /// <summary>
        /// Reads a float from the stream.
        /// </summary>
        /// <returns>A float.</returns>
        float ReadFloat();

        /// <summary>
        /// Reads a double from the stream.
        /// </summary>
        /// <returns>A double.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads a byte array from the stream.
        /// </summary>
        /// <param name="length">The length of the array, if known.</param>
        /// <returns>A byte array.</returns>
        byte[] ReadByteArray(int length = 0);

        /// <summary>
        /// Reads a string from the stream.
        /// </summary>
        /// <param name="length">The length of the string. Max string length is <see cref="short.MaxValue"/>, however a custom limit can be defined.</param>
        /// <returns>A string.</returns>
        string ReadString(int maxLength = short.MaxValue);

        /// <summary>
        /// Reads a variable integer from the stream.
        /// </summary>
        /// <returns>A variable integer.</returns>
        /// <remarks>
        /// Variable integers are network compressed integers, and cannot be read the same way a normal integer can.
        /// </remarks>
        int ReadVarInt();

        /// <summary>
        /// Reads a variable long from the stream.
        /// </summary>
        /// <returns>A variable long.</returns>
        /// <remarks>
        /// Variable longs are network compressed longs, and cannot be read the same way a normal long can.
        /// </remarks>
        long ReadVarLong();

        /// <summary>
        /// Reads a Minecraft packet from the stream.
        /// </summary>
        /// <param name="packetLength">The amount of bytes that the packet will use. The packet length, if known.</param>
        /// <returns>A Minecraft packet.</returns>
        AbstractPacket ReadPacket(int packetLength = -1);

        /// <summary>
        /// Reads a Minecraft packet from the stream.
        /// </summary>
        /// <param name="packetLength">The amount of bytes that the packet will use. The packet length, if known.</param>
        /// <typeparam name="T">T must be a Minecraft packet interface.</typeparam>
        /// <returns>A Minecraft packet in the requested format.</returns>
        T ReadPacket<T>(int packetLength = -1) where T : AbstractPacket, new();
        #endregion

        #region ReadMethodsAsync
        /// <summary>
        /// Reads a ushort from the stream asynchronously.
        /// </summary>
        /// <returns>A ushort.</returns>
        Task<ushort> ReadUnsignedShortAsync();

        /// <summary>
        /// Reads a short from the stream asynchronously.
        /// </summary>
        /// <returns>A short.</returns>
        Task<short> ReadShortAsync();

        /// <summary>
        /// Reads a int from the stream asynchronously.
        /// </summary>
        /// <returns>A int.</returns>
        Task<int> ReadIntAsync();

        /// <summary>
        /// Reads a long from the stream asynchronously.
        /// </summary>
        /// <returns>A long.</returns>
        Task<long> ReadLongAsync();

        /// <summary>
        /// Reads a float from the stream asynchronously.
        /// </summary>
        /// <returns>A float.</returns>
        Task<float> ReadFloatAsync();

        /// <summary>
        /// Reads a double from the stream asynchronously.
        /// </summary>
        /// <returns>A double.</returns>
        Task<double> ReadDoubleAsync();

        /// <summary>
        /// Reads a byte array from the stream asynchronously.
        /// </summary>
        /// <param name="length">The length of the array, if known.</param>
        /// <returns>A byte array.</returns>
        Task<byte[]> ReadByteArrayAsync(int length = 0);

        /// <summary>
        /// Reads a string from the stream asynchronously.
        /// </summary>
        /// <param name="length">The length of the string. Max string length is <see cref="short.MaxValue"/>, however a custom limit can be defined.</param>
        /// <returns>A string.</returns>
        Task<string> ReadStringAsync(int maxLength = short.MaxValue);

        /// <summary>
        /// Reads a Minecraft packet from the stream asynchronously.
        /// </summary>
        /// <param name="packetLength">The amount of bytes that the packet will use. The packet length, if known.</param>
        /// <returns>A Minecraft packet.</returns>
        Task<AbstractPacket> ReadPacketAsync(int packetLength = -1);

        /// <summary>
        /// Reads a Minecraft packet from the stream asynchronously.
        /// </summary>
        /// <param name="packetLength">The amount of bytes that the packet will use. The packet length, if known.</param>
        /// <typeparam name="T">T must be a Minecraft packet interface.</typeparam>
        /// <returns>A Minecraft packet in the requested format.</returns>
        Task<T> ReadPacketAsync<T>(int packetLength = -1) where T : AbstractPacket, new();
        #endregion

        #region WriteMethods
        /// <summary>
        /// Writes a byte to the stream.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        void WriteByte(byte value);

        /// <summary>
        /// Writes a signed byte to the stream.
        /// </summary>
        /// <param name="value">The signed byte to write.</param>
        void WriteSignedByte(sbyte value);

        /// <summary>
        /// Writes a boolean to the stream.
        /// </summary>
        /// <param name="value">The boolean to write.</param>
        void WriteBool(bool value);

        /// <summary>
        /// Writes a short to the stream.
        /// </summary>
        /// <param name="value">The short to write.</param>
        void WriteShort(short value);

        /// <summary>
        /// Writes a unsigned short to the stream.
        /// </summary>
        /// <param name="value">The unsigned short to write.</param>
        void WriteUnsignedShort(ushort value);

        /// <summary>
        /// Writes a int to the stream.
        /// </summary>
        /// <param name="value">The int to write.</param>
        void WriteInt(int value);

        /// <summary>
        /// Writes a unsigned int to the stream.
        /// </summary>
        /// <param name="value">The unsigned int to write.</param>
        void WriteUnsignedInt(uint value);

        /// <summary>
        /// Writes a long to the stream.
        /// </summary>
        /// <param name="value">The long to write.</param>
        void WriteLong(long value);

        /// <summary>
        /// Writes a unsigned long to the stream.
        /// </summary>
        /// <param name="value">The unsigned long to write.</param>
        void WriteUnsignedLong(ulong value);

        /// <summary>
        /// Writes a float to the stream.
        /// </summary>
        /// <param name="value">The float to write.</param>
        void WriteFloat(float value);

        /// <summary>
        /// Writes a double to the stream.
        /// </summary>
        /// <param name="value">The double to write.</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a byte array to the stream.
        /// </summary>
        /// <param name="value">The byte array to write.</param>
        void WriteByteArray(byte[] value);

        /// <summary>
        /// Writes a string to the stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="maxLength">The maximum length of the string. Max string length is <see cref="short.MaxValue"/>, however a custom limit can be defined.</param>
        void WriteString(string value, int maxLength = short.MaxValue);

        /// <summary>
        /// Writes a variable integer to the stream.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        void WriteVarInt(int value);

        /// <summary>
        /// Writes a variable long to the stream.
        /// </summary>
        /// <param name="value">The long to write.</param>
        void WriteVarLong(long value);

        /// <summary>
        /// Writes a Minecraft packet to the stream.
        /// </summary>
        /// <param name="packet">The Minecraft packet to write.</param>
        void WritePacket(AbstractPacket packet);
        #endregion

        #region WriteMethodsAsync
        /// <summary>
        /// Writes a short to the stream asynchronously.
        /// </summary>
        /// <param name="value">The short to write.</param>
        Task WriteShortAsync(short value);

        /// <summary>
        /// Writes a unsigned short to the stream asynchronously.
        /// </summary>
        /// <param name="value">The unsigned short to write.</param>
        Task WriteUnsignedShortAsync(ushort value);

        /// <summary>
        /// Writes a int to the stream asynchronously.
        /// </summary>
        /// <param name="value">The int to write.</param>
        Task WriteIntAsync(int value);

        /// <summary>
        /// Writes a unsigned int to the stream asynchronously.
        /// </summary>
        /// <param name="value">The unsigned int to write.</param>
        Task WriteUnsignedIntAsync(uint value);

        /// <summary>
        /// Writes a long to the stream asynchronously.
        /// </summary>
        /// <param name="value">The long to write.</param>
        Task WriteLongAsync(long value);

        /// <summary>
        /// Writes a unsigned long to the stream asynchronously.
        /// </summary>
        /// <param name="value">The unsigned long to write.</param>
        Task WriteUnsignedLongAsync(ulong value);

        /// <summary>
        /// Writes a float to the stream asynchronously.
        /// </summary>
        /// <param name="value">The float to write.</param>
        Task WriteFloatAsync(float value);

        /// <summary>
        /// Writes a double to the stream asynchronously.
        /// </summary>
        /// <param name="value">The double to write.</param>
        Task WriteDoubleAsync(double value);

        /// <summary>
        /// Writes a byte array to the stream asynchronously.
        /// </summary>
        /// <param name="value">The byte array to write.</param>
        Task WriteByteArrayAsync(byte[] value);

        /// <summary>
        /// Writes a string to the stream asynchronously.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <param name="maxLength">The maximum length of the string. Max string length is <see cref="short.MaxValue"/>, however a custom limit can be defined.</param>
        Task WriteStringAsync(string value, int maxLength = short.MaxValue);

        /// <summary>
        /// Writes a variable integer to the stream asynchronously.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        Task WriteVarIntAsync(int value);

        /// <summary>
        /// Writes a variable long to the stream asynchronously.
        /// </summary>
        /// <param name="value">The long to write.</param>
        Task WriteVarLongAsync(long value);

        /// <summary>
        /// Writes a Minecraft packet to the stream asynchronously.
        /// </summary>
        /// <param name="packet">The Minecraft packet to write.</param>
        Task WritePacketAsync(AbstractPacket packet);
        #endregion
    }
}