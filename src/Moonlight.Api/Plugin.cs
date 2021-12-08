using NuGet;

namespace Moonlight.Api
{
    public abstract class Plugin
    {
        /// <summary>
        /// The name of the plugin. Will be used when looking for updates on the plugin store.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// The name of the plugin, when displayed to a Minecraft player or server owner.
        /// </summary>
        public abstract string PrettyName { get; }

        /// <summary>
        /// A small description of the plugin.
        /// </summary>
        public abstract string ShortDescription { get; }

        /// <summary>
        /// The plugin version.
        /// </summary>
        public abstract SemanticVersion PluginVersion { get; }

        /// <summary>
        /// The server version that the plugin targets. Defaults to "0.0.0", which is used to target any server version (which is rarely required, but best for future compatibility).
        /// </summary>
        public virtual SemanticVersion ServerVersion { get; } = new SemanticVersion("0.0.0");



        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Called when the plugin is unloaded.
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// Called when the plugin is to be reloaded.
        /// </summary>
        public abstract void Reload();
    }
}