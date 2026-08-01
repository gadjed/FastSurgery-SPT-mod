# Fast Surgery

**SPT 4.1.0 Compatible**

Server mod that sets medical use time to **5 seconds** for splints and surgical kits.

Developed and tested against **SPT 4.1.0**.

[Latest release](https://github.com/gadjed/FastSurgery-SPT-mod/releases/latest) · [License: MIT](LICENSE)

## Features

- Configurable use time for surgical / fracture items (default **5s**)
- Human-readable `config.json` keys (item names, not template IDs)
- Optional Continuous Healing companion for healing all limbs in one go

## Affected items

| Config key | Template ID |
|------------|-------------|
| `Immobilizing splint` | `544fb3364bdc2d34748b456a` |
| `Aluminum splint` | `5af0454c86f7746bf20992e8` |
| `CMS surgical kit` | `5d02778e86f774203e7dedbe` |
| `Surv12 field surgical kit` | `5d02797c86f774203f38e30a` |

## Install

1. Download `FastSurgery-*.zip` from [Releases](https://github.com/gadjed/FastSurgery-SPT-mod/releases)
2. Extract the archive into your **SPT game root** (the folder that contains `SPT.Server.exe` / `user/`)
3. Restart the SPT server

The zip already contains the correct paths:

```text
user/mods/FastSurgery/FastSurgery.dll
user/mods/FastSurgery/config.json
```

On startup the server log should show lines like:

```text
[FastSurgery] CMS surgical kit: medUseTime 16 -> 5s
[FastSurgery] Updated use time on 4 medical item(s).
```

## Config

Edit `user/mods/FastSurgery/config.json`:

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

| Key | Description |
|-----|-------------|
| `UseTimeSeconds` | Use time in seconds (default `5`) |
| `Items` | Enable (`true`) / disable (`false`) each item by name |

Notes:
- Names are resolved through an internal catalog (`ItemCatalog`)
- Matching is **case-insensitive**
- Raw template IDs still work as keys if you prefer them

## Recommended companion

Pair with [Continuous Healing](https://forge.sp-tarkov.com/mod/1884/continuous-healing) (client mod) so CMS / Surv12 / splints continue across all limbs.

In Continuous Healing **F12** settings:
- **Heal Limbs** = `true`
- **Heal Delay** = `0`

## Build from source

Requires **.NET 9** SDK.

```bash
dotnet build FastSurgery.csproj -c Release
```

Output is copied to `Build/SPT/user/mods/FastSurgery/`.

## License

MIT — see [LICENSE](LICENSE).
