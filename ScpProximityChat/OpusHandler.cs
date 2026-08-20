using System;
using System.Collections.Generic;
using Exiled.API.Features;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

namespace ScpProximityChat
{
    public sealed class OpusHandler : IDisposable
    {
        public const int SampleBufferSize = 480;
        public const int EncodedBufferSize = 512;

        private static readonly Dictionary<string, OpusHandler> Handlers = new Dictionary<string, OpusHandler>();

        private bool disposed;

        public OpusDecoder Decoder { get; } = new OpusDecoder();

        public OpusEncoder Encoder { get; } = new OpusEncoder(OpusApplicationType.Voip);

        public float[] SampleBuffer { get; } = new float[SampleBufferSize];

        public byte[] EncodedBuffer { get; } = new byte[EncodedBufferSize];

        public static OpusHandler Get(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return null;

            if (Handlers.TryGetValue(player.UserId, out OpusHandler handler))
                return handler;

            handler = new OpusHandler();
            Handlers.Add(player.UserId, handler);
            return handler;
        }

        public static void Remove(Player player)
        {
            if (player is null || string.IsNullOrEmpty(player.UserId))
                return;

            Remove(player.UserId);
        }

        public static void Remove(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Handlers.TryGetValue(userId, out OpusHandler handler))
                return;

            handler.Dispose();
            Handlers.Remove(userId);
        }

        public static void ClearAll()
        {
            foreach (KeyValuePair<string, OpusHandler> entry in Handlers)
                entry.Value.Dispose();

            Handlers.Clear();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            Decoder.Dispose();
            Encoder.Dispose();
        }
    }
}
