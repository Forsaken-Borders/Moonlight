using Moonlight.Api.Server.Abstractions;
using NuGet;

namespace Moonlight.MoonshadePlugin
{
    public class MoonshadePlugin : Plugin
    {
        public override string Name => "Moonshade-Plugin";
        public override string PrettyName => "Moonshade";
        public override string ShortDescription => "Enables basic playability of the Moonlight Minecraft server. Can be disabled for another plugin to completely override default functionality.";
        public override SemanticVersion PluginVersion => new("0.2.0");

        public override void Load() => throw new NotImplementedException();
        public override void Unload() => throw new NotImplementedException();
        public override void Reload() => throw new NotImplementedException();
    }
}