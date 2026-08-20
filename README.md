# ScpProximityChat 2.0


> Portage EXILED 9.14.2 d'un plugin de **Bolton**. Depot non affilie a
> l'auteur d'origine. Voir [NOTICE.md](NOTICE.md) pour l'attribution.

Permet aux SCP de parler dans le chat de proximite, entendus par les humains a portee.

**EXILED 9.14.2** — `dotnet build -c Release ScpProximityChat/ScpProximityChat.csproj`

## Fonctionnement

Quand un SCP active le mode proximite, un `SpeakerToy` est attache a son
transform. Ses paquets voix du canal SCP sont decodes, amplifies, reencodes et
renvoyes aux joueurs a portee via ce haut-parleur.

## Activation

Deux modes via `activation_type` :

- `ServerSpecificSettings` (defaut) : une touche configurable est exposee dans les
  parametres serveur du client.
- `NoClip` : la touche noclip sert de bascule pour les roles concernes.

## Configuration

| Cle | Defaut | Role |
|---|---|---|
| `scp_roles` | 049, 049-2, 096, 106, 173, 939 | Roles autorises. SCP-079 est retire automatiquement |
| `use_default_scp_chat` | `true` | Si `false`, les SCP n'entendent plus le locuteur hors portee |
| `volume` | `10` | Gain applique a la voix |
| `max_volume` | `25` | Garde-fou contre la saturation |
| `min_distance` / `max_distance` | `2` / `10` | Portee du haut-parleur |
| `toggle_cooldown_seconds` | `1` | Anti-spam sur la bascule |
| `disable_on_role_change` | `true` | Coupe la proximite a la mort ou au changement de role |

Les messages d'activation, de desactivation et de rappel de touche sont
configurables individuellement (type broadcast ou hint, duree, affichage).
