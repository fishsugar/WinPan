using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CoreAudio.Interfaces
{
    /// <summary>
    /// Windows CoreAudio IChannelAudioVolume interface
    /// Defined in AudioClient.h
    /// </summary>
    [Guid("1C158861-B533-4B30-B1CF-E853E51C59B8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    internal interface IChannelAudioVolume
    {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetChannelCount(out int count);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetChannelVolume([In] int index, [MarshalAs(UnmanagedType.R4), In] float level,
            [MarshalAs(UnmanagedType.LPStruct), In] Guid eventContext);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetChannelVolume([In] int index, [MarshalAs(UnmanagedType.R4)] out float level);
    }
}