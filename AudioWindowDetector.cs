using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using static WinPan;

public class AudioWindowDetector {
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool IsIconic(IntPtr hwnd);

    private AudioSessionManager audioSessionManager;

    public AudioWindowDetector(AudioSessionManager manager) {
        audioSessionManager = manager;
    }

    public List<AudioSessionControl> DetectAudioWindows() {        
        // Get all audio sessions
        SessionCollection sessions = audioSessionManager.Sessions;

        var result = new List<AudioSessionControl>();

        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];

            try {
                // filter out inactive sessions
                if (session.State != AudioSessionState.AudioSessionStateActive)
                    continue;

                // Get the process associated with the session
                Process process = Process.GetProcessById((int)session.GetProcessID);

                // filter out minimized windows
                if (IsIconic(process.MainWindowHandle))
                    continue;

                // Get window position of the process
                GetHorizontalWindowPosition(process.MainWindowHandle);

                result.Add(session);
            } catch(NullReferenceException) {
            }
        }
        return result;
    }
    
    private int GetHorizontalWindowPosition(IntPtr windowHandle)
    {
        if (GetWindowRect(windowHandle, out RECT rect))
        {
            int horizontalPosition = rect.Left + ((rect.Right - rect.Left) / 2);
            return horizontalPosition;
        }
        throw new NullReferenceException();
    }
}