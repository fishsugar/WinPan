using System;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreAudio.Interfaces;
using NAudio.CoreAudioApi;

namespace CoreAudio
{
    public class ChannelAudioVolume
    {
        private readonly IChannelAudioVolume realInterface;

        public ChannelAudioVolume(AudioSessionControl session)
        {
            object rawAudioSessionControl = typeof(AudioSessionControl)
                .GetField("audioSessionControlInterface", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(session);
            if (rawAudioSessionControl is IChannelAudioVolume realInterface)
            {
                this.realInterface = realInterface;
            }
            else
            {
                throw new ArgumentException("parameter is not IChannelAudioVolume");
            }
        }

        public int ChannelCount
        {
            get
            {
                Marshal.ThrowExceptionForHR(realInterface.GetChannelCount(out int count));
                return count;
            }
        }

        public float GetChannelVolume(int index)
        {
            Marshal.ThrowExceptionForHR(realInterface.GetChannelVolume(index, out float level));
            return level;
        }

        public float SetChannelVolume(int index, float level)
        {
            Marshal.ThrowExceptionForHR(realInterface.SetChannelVolume(index, level, Guid.Empty));
            return level;
        }
    }
}