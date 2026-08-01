# Fast Surgery

SPT **4.0.13 Compatible** server mod that sets medical use time to **5 seconds** for surgical / fracture items.

Developed and tested against **SPT 4.0.13**.

## Affected items

| Item | Template ID |
|------|-------------|
| Immobilizing splint | `544fb3364bdc2d34748b456a` |
| Aluminum splint | `5af0454c86f7746bf20992e8` |
| CMS surgical kit | `5d02778e86f774203e7dedbe` |
| Surv12 field surgical kit | `5d02797c86f774203f38e30a` |

## Install

1. Download the latest release zip from [Releases](https://github.com/gadjed/FastSurgery-SPT-mod/releases)
2. Extract into `<SPT>/user/mods/`
3. Restart the SPT server

You should see `[FastSurgery]` lines in the server log on startup.

## Config

Edit `user/mods/FastSurgery/config.json`:

```json
{
  "UseTimeSeconds": 5,
  "Items": {
    "544fb3364bdc2d34748b456a": true,
    "5af0454c86f7746bf20992e8": true,
    "5d02778e86f774203e7dedbe": true,
    "5d02797c86f774203f38e30a": true
  }
}
```

- `UseTimeSeconds` — use time in seconds (default `5`)
- `Items` — enable (`true`) / disable (`false`) each template ID

## Recommended companion

Pair with [Continuous Healing](https://forge.sp-tarkov.com/mod/1884/continuous-healing) (client mod) so CMS / Surv12 / splints continue across all limbs.

In Continuous Healing F12 settings:
- **Heal Limbs** = `true`
- **Heal Delay** = `0`

## Build from source

Requires .NET 9 SDK.

```bash
dotnet build FastSurgery.csproj -c Release
```

Output is copied to `Build/SPT/user/mods/FastSurgery/`.

## License

MIT — see [LICENSE](LICENSE).
