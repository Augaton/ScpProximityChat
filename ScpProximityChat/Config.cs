using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using PlayerRoles;
using ScpProximityChat.Enums;

namespace ScpProximityChat
{
    public sealed class Config : IConfig
    {
        [Description("Active ou desactive le plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Active les logs de debug.")]
        public bool Debug { get; set; } = false;

        [Description("Mode d'activation : ServerSpecificSettings (touche configurable) ou NoClip (touche noclip).")]
        public ActivationType ActivationType { get; set; } = ActivationType.ServerSpecificSettings;

        [Description("Roles SCP autorises a utiliser le chat de proximite.")]
        public HashSet<RoleTypeId> ScpRoles { get; set; } = new HashSet<RoleTypeId>
        {
            RoleTypeId.Scp049,
            RoleTypeId.Scp0492,
            RoleTypeId.Scp096,
            RoleTypeId.Scp106,
            RoleTypeId.Scp173,
            RoleTypeId.Scp939,
        };

        [Description("Si false, les SCP n'entendent plus le chat SCP d'un joueur ayant active la proximite tant qu'ils ne sont pas a portee.")]
        public bool UseDefaultScpChat { get; set; } = true;

        [Description("Gain applique a la voix. Valeurs elevees = saturation. Borne par MaxVolume.")]
        public float Volume { get; set; } = 10f;

        [Description("Gain maximum autorise, garde-fou contre la saturation.")]
        public float MaxVolume { get; set; } = 25f;

        [Description("Distance en dessous de laquelle l'audio est a plein volume.")]
        public float MinDistance { get; set; } = 2f;

        [Description("Distance maximale a laquelle la voix reste audible.")]
        public float MaxDistance { get; set; } = 10f;

        [Description("Cooldown en secondes entre deux activations du chat de proximite par un meme joueur.")]
        public float ToggleCooldownSeconds { get; set; } = 1f;

        [Description("Desactive automatiquement le chat de proximite quand le joueur change de role ou meurt.")]
        public bool DisableOnRoleChange { get; set; } = true;

        [Description("Message affiche quand un SCP active son chat de proximite.")]
        public Message ProximityChatEnabled { get; set; } = new Message
        {
            Type = MessageType.Hint,
            Content = "<b>Chat de proximite <color=green>active</color>.</b>",
            Duration = 3,
            Show = true,
        };

        [Description("Message affiche quand un SCP desactive son chat de proximite.")]
        public Message ProximityChatDisabled { get; set; } = new Message
        {
            Type = MessageType.Hint,
            Content = "<b>Chat de proximite <color=red>desactive</color>.</b>",
            Duration = 3,
            Show = true,
        };

        [Description("Message affiche a un joueur qui prend un role SCP autorise.")]
        public Message ProximityChatRole { get; set; } = new Message
        {
            Type = MessageType.Broadcast,
            Content = "<b>Vous pouvez activer le chat de proximite avec la touche configuree dans vos parametres.</b>",
            Duration = 10,
            Show = true,
        };


        [Description("Position verticale du hint de ce plugin dans HintServiceMeow. Doit differer des autres plugins.")]
        public float HintYCoordinate { get; set; } = 550f;

        [Description("Taille de police du hint de ce plugin.")]
        public int HintFontSize { get; set; } = 20;

        [Description("Titre de la categorie dans les parametres serveur.")]
        public string SettingHeaderLabel { get; set; } = "ScpProximityChat";

        [Description("Identifiant unique du parametre de touche.")]
        public int KeybindId { get; set; } = 200;

        [Description("Libelle de la touche.")]
        public string KeybindLabel { get; set; } = "Chat de proximite SCP";

        [Description("Info-bulle de la touche.")]
        public string KeybindHint { get; set; } = "Active ou desactive le chat de proximite SCP.";
    }
}
