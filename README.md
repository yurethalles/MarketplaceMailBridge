# Marketplace Mail Bridge

Marketplace Mail Bridge is a server-side administrative delivery bridge for **Valheim**. It adds a secure console command that creates item mail through **Marketplace and Server NPCs Revamped**.

Built by **Dorn**.

## Features

- Send a Valheim item directly to a player's Marketplace mailbox.
- Deliver to an online or offline player using their 17-digit SteamID.
- Uses Marketplace's native persistent mailbox data; the recipient can claim the attachment from the normal Mail NPC UI.
- Restricts the command to the server context and administrators.
- Works from the dedicated-server console and from an RCON/panel integration that runs server console commands.
- Validates SteamID, item quantity, and item quality before creating mail.
- Configurable sender name, expiry time, and maximum quantity per command.

## Requirements

- Valheim
- BepInEx 5
- [Marketplace and Server NPCs Revamped](https://valheim.hexium.gg/mods/KG/Marketplace_And_Server_NPCs_Revamped) **10.0.6 or newer**

Marketplace must be installed on the server and on players who need to open the mailbox. Marketplace Mail Bridge itself is required **only on the server**.

## Installation

1. Install Marketplace and Server NPCs Revamped.
2. Copy `MarketplaceMailBridge.dll` into the server's `BepInEx/plugins/` directory.
3. Restart the server.
4. Configure the generated file if needed:

   ```text
   BepInEx/config/dorn.marketplace-mail-bridge.cfg
   ```

## Command

Run this command in the server console or through a trusted RCON/panel integration:

```text
mpmail send <steamId> <prefab> <quantity> [quality]
```

Example:

```text
mpmail send 76561198000000000 ArmorBloodcallerChest_DO 1 1
```

`quality` is optional and defaults to `1`.

On success, the console reports the destination SteamID, quantity, and prefab. On failure, it reports an invalid parameter or instructs the administrator to inspect `BepInEx/LogOutput.log`.

## Configuration

| Setting | Default | Meaning |
| --- | --- | --- |
| `Mail.SenderName` | `Glitnir` | Display name shown as the sender of delivered mail. |
| `Mail.ExpiryMinutes` | `43200` | How long mail remains available, in minutes (30 days by default). |
| `Safety.MaximumStack` | `100` | Largest quantity accepted by a single command. |

## Security and behavior

`mpmail` is a **server administration** command. It does not remove an item from an administrator's inventory; it creates a server-issued delivery. Regular players cannot invoke it from their game client.

Protect the credentials of any RCON or server-panel integration that can run this command. Anyone with that access can create server deliveries.

Mail belongs to the SteamID, not to an individual character. Multiple characters using the same Steam account on the same server share the mailbox.

## Building from source

The included `build.ps1` compiles the plugin against a local Valheim installation and BepInEx profile:

```powershell
.\build.ps1
```

You can override the paths if your installation differs:

```powershell
.\build.ps1 -ValheimPath "D:\SteamLibrary\steamapps\common\Valheim" -ProfilePath "D:\ValheimProfile"
```

The compiled DLL is written to `dist/MarketplaceMailBridge.dll`.

## Packaging for Hexium

Run:

```powershell
.\package.ps1
```

The upload-ready archive is written to `package/Dorn-MarketplaceMailBridge-0.1.0.zip`.

## License

MIT. See [LICENSE](LICENSE).
