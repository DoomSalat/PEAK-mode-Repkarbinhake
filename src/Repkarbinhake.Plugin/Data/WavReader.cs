using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class WavReader
    {
        private const string RiffTag = "RIFF";
        private const string FormatTag = "fmt ";
        private const string DataTag = "data";
        private const int RiffHeaderBytes = 12;
        private const int PcmFormat = 1;
        private const int BitsPerSample = 16;
        private const float SampleScale = 1f / 32768f;
        private const int TagBytes = 4;
        private const int ChunkHeaderBytes = 8;
        private const int ChannelsOffset = 2;
        private const int FrequencyOffset = 4;
        private const int BitsOffset = 14;
        private const int BytesPerSample = 2;

        public AudioClip Read(byte[] bytes, string name)
        {
            if (Encoding.ASCII.GetString(bytes, 0, RiffTag.Length) != RiffTag)
                throw new InvalidDataException($"{nameof(WavReader)}: not a RIFF file: {name}");

            int channels = 0;
            int frequency = 0;
            int offset = RiffHeaderBytes;

            while (offset + ChunkHeaderBytes <= bytes.Length)
            {
                string tag = Encoding.ASCII.GetString(bytes, offset, TagBytes);
                int size = BitConverter.ToInt32(bytes, offset + TagBytes);
                int body = offset + ChunkHeaderBytes;

                if (tag == FormatTag)
                {
                    if (BitConverter.ToInt16(bytes, body) != PcmFormat || BitConverter.ToInt16(bytes, body + BitsOffset) != BitsPerSample)
                        throw new InvalidDataException($"{nameof(WavReader)}: only 16-bit PCM is supported: {name}");

                    channels = BitConverter.ToInt16(bytes, body + ChannelsOffset);
                    frequency = BitConverter.ToInt32(bytes, body + FrequencyOffset);
                }
                else if (tag == DataTag)
                    return CreateClip(name, bytes, body, size, channels, frequency);

                offset = body + size + (size & 1);
            }

            throw new InvalidDataException($"{nameof(WavReader)}: no data chunk: {name}");
        }

        private AudioClip CreateClip(string name, byte[] bytes, int start, int size, int channels, int frequency)
        {
            int sampleCount = size / BytesPerSample;
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
                samples[i] = BitConverter.ToInt16(bytes, start + i * BytesPerSample) * SampleScale;

            AudioClip clip = AudioClip.Create(name, sampleCount / channels, channels, frequency, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
