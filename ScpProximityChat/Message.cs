using System.ComponentModel;
using ScpProximityChat.Enums;

namespace ScpProximityChat
{
    public sealed class Message
    {
        [Description("Type du message : Broadcast ou Hint.")]
        public MessageType Type { get; set; }

        [Description("Contenu du message.")]
        public string Content { get; set; }

        [Description("Duree du message en secondes.")]
        public ushort Duration { get; set; }

        [Description("Indique si le message doit etre affiche.")]
        public bool Show { get; set; }
    }
}
