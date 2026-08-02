# Med Rebalance

**Ребаланс медицини** · **SPT 4.0.13** · **v1.3.0** · Server + Client

Medicine rebalance: faster surgery/splints, continuous multi-limb healing, scratch top-ups from medkit resource, and interrupt-on-damage.

Developed and tested against **SPT 4.0.13**.

Formerly *Fast Surgery* — remove old `SPT/user/mods/FastSurgery/` and `BepInEx/plugins/FastSurgery.Client.dll` / Continuous Healing before installing.

[Latest release](https://github.com/gadjed/MedRebalance-SPT-mod/releases/latest) · [License: MIT](LICENSE)

## Features

- **Server:** configurable `MedUseTime` for splints and surgical kits (default **5s**)
- **Client — continuous healing:** after one limb finishes, healing continues to the next (bleed / fracture treatment still uses vanilla `DoMedEffect`)
- **Client — scratch heal:** while continuing, other limbs missing only a few HP get **2–3 HP** topped up from the **medkit resource**; empty kit ends healing
- **Client — cancel on damage:** any HP damage during healing cancels the med use, fast-forwards the put-away, and restores the last weapon
- Incompatible with standalone Continuous Healing (`com.lacyway.ch`) — remove that plugin

## Affected items (server)

| Config key | Template ID |
|------------|-------------|
| `Immobilizing splint` | `544fb3364bdc2d34748b456a` |
| `Aluminum splint` | `5af0454c86f7746bf20992e8` |
| `CMS surgical kit` | `5d02778e86f774203e7dedbe` |
| `Surv12 field surgical kit` | `5d02797c86f774203f38e30a` |

## Install

1. Download `MedRebalance-*.zip` from [Releases](https://github.com/gadjed/MedRebalance-SPT-mod/releases)
2. Extract into your **SPT game root** (folder with `EscapeFromTarkov.exe` / `BepInEx/` / `SPT/`)
3. Restart the SPT **server** and the **game client**
4. Remove leftovers if upgrading: `FastSurgery`, `ContinuousHealing`

```text
SPT/user/mods/MedRebalance/MedRebalance.dll
SPT/user/mods/MedRebalance/config.json
BepInEx/plugins/MedRebalance.Client.dll
```

## Config

### Server — `SPT/user/mods/MedRebalance/config.json`

```json
{
  "UseTimeSeconds": 5,
  "Items": {
    "Immobilizing splint": true,
    "Aluminum splint": true,
    "CMS surgical kit": true,
    "Surv12 field surgical kit": true
  }
}
```

### Client — F12 (BepInEx)

| Section | Key | Default | Notes |
|---------|-----|---------|-------|
| Continuous Healing | Enabled | on | Continue across limbs |
| Continuous Healing | Heal Limbs | on | Include surgery kits / splints |
| Continuous Healing | Heal Delay | 0 | Seconds between limbs |
| Continuous Healing | Reset Animations | on | Off = keep starting anim / skip resets |
| Interrupt | Cancel On Damage | on | Stop heal + restore weapon |
| Scratch Heal | Enabled | on | Top up scratched limbs |
| Scratch Heal | Heal Amount | 2.5 | HP per scratched limb per tick |
| Scratch Heal | Max Missing HP | 8 | Limb counts as scratch if missing ≤ this |

## Build from source

Requires **.NET 9** SDK (server) and references under `mods/SAIN/References` + `mods/QuickSearch/References` (client).

```bash
dotnet build Server/MedRebalance.csproj -c Release
dotnet build Client/MedRebalance.Client.csproj -c Release
```

Output:

```text
Build/SPT/user/mods/MedRebalance/
Build/SPT/BepInEx/plugins/MedRebalance.Client.dll
```

## License

MIT — see [LICENSE](LICENSE).

Continuous multi-limb healing behavior was reimplemented for this mod (inspired by the idea of Continuous Healing by Lacyway); do not ship the original Continuous Healing plugin alongside this build.
