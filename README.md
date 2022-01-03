# Moonlight
## Looking for contributors skilled in:
| Title | Linked Issue | Description |
| --- | --- | --- |
| Technical Writing | PR Whenever | Someone who's not afraid to read and understand the code, while subsequently writing documentation on it. Everything needs to be documented, regardless of accessibility modifiers. |
| Testing | PR Whenever | Tests need to be written fully mocking the client. All packets and API classes/methods (I.E ChatComponent or ServerStatus packets) should have their own test, which can be ran when new contributors are making a PR. |
| Benchmarking | PR Whenever | The goal of the benchmarks is to efficiently present memory usage and execution time of each method or process. This allows contributors to see which methods or classes need to be improved upon in varies ways. |
| AI Enthusiast | Not Ready | Implementing the vanilla behavior of each mob in Minecraft, or something closely resembling such. |

## Current Status
Finished with the MOTD/Server List Ping, supporting all Minecraft versions. Currently working on the login process. This includes implementing NBT, the anvil world format, inventories, etc.

## What is Moonlight?
Moonlight: A C# implementation of the Minecraft Server protocol. Our goal is to recreate the vanilla Minecraft server in C#.

## Does Moonlight have plugin support?
It is planned, but not currently supported. This will be updated when this changes.

## Does Moonlight have mod support?
Mod development is not directly supported. We may go slightly out of our way to make it easier to add support, we do not make Moonlight with mod support in mind. A fork of Moonlight is expected to happen if you want mod support. If you wish to PR mod support into Moonlight, the PR will be highly considered.

## How do I set it up?
Refer to the [Wiki](https://github.com/Forsaken-Borders/Moonlight/wiki), which is where all of our official documentation is.

## Why is Moonlight being developed?
Moonlight was started because the Minecraft Server that Mojang hands out is too resource intensive. The Bukkit/Spigot/Paper API is also very confusing, full of obselete methods or 20 other ways to do the same task. C# was originally made to replace Java's mistakes, and the same concept applies to Moonlight: To fix Mojang's server software mistakes.

## Got a Discord Server?
Our whole orginzation does: [Forsaken Borders Discord Server](https://discord.gg/Bsv7zSFygc), checkout the #moonlight channel.

## Disclaimer
Moonlight is unofficial Minecraft server software and is not associated with Mojang or Microsoft. Minecraft™ is a registered trademark of Mojang AB.
