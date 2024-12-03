using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CoreAudio;
using NAudio.CoreAudioApi;

partial class WinPan
{
    private AudioSessionManager audioSessionManager;

    private int minScreenX;
    private int maxHorizontalPixels;
    private HashSet<AudioSessionControl> modifiedSessions = new HashSet<AudioSessionControl>();



    public WinPan()
    {
        // Initialize Core Audio API Manager
        // Create an MMDeviceEnumerator
        MMDeviceEnumerator enumerator = new();

        // Get the default audio endpoint
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        Console.WriteLine($"Default Audio Device: {device.FriendlyName}");

        // Get the AudioSessionManager from the default device
        this.audioSessionManager = device.AudioSessionManager;

        // Get the maximum width to be able to calculate relative horizontal position %age
        SetScreenGeometry();
    }

    private void SetScreenGeometry()
    {
        // Get information about all screens
        Screen[] screens = Screen.AllScreens;
        foreach (Screen screen in screens)
        {
            maxHorizontalPixels += screen.Bounds.Width;
            if (screen.Bounds.X < minScreenX) minScreenX = screen.Bounds.X;
        }
    }

    public void UpdateAudioPanning()
    {
        AudioWindowDetector detector = new(audioSessionManager);

        var audioSessions = detector.DetectAudioWindows();
        foreach (var session in audioSessions)
        {
            try
            {
                Process process = Process.GetProcessById((int)session.GetProcessID);
                int windowPosition = GetHorizontalWindowPosition(process.MainWindowHandle);
                ApplyPanning(session, windowPosition + -minScreenX);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }

    private void ApplyPanning(AudioSessionControl audioSessionControl, int position)
    {
        // Calculate the panning value based on the position and maxHorizontalPixels
        double panningValue = new BalanceCalculator().CalculatePanningValue(position, maxHorizontalPixels, minScreenX);

        // Apply the scaled panning value to the audio
        SetAudioSessionPanning(audioSessionControl, panningValue);
    }

    private void SetAudioSessionPanning(AudioSessionControl audioSessionControl, double panningValue)
    {
        // Calculate left and right channel volumes
        double leftChannelVolume = Math.Max(0, Math.Min(1, Math.Abs(panningValue - 1)));
        double rightChannelVolume = Math.Max(0, Math.Min(1, Math.Abs(panningValue + 1)));

        ChannelAudioVolume channelAudioVolume = new ChannelAudioVolume(audioSessionControl); 

        // Check if the endpoint has separate volume controls for left and right channels
        if (channelAudioVolume.ChannelCount >= 2)
        {
            channelAudioVolume.SetChannelVolume(0, (float)leftChannelVolume);
            channelAudioVolume.SetChannelVolume(1, (float)rightChannelVolume);
            modifiedSessions.Add(audioSessionControl);
        }
    }

    internal void ResetBalance()
    {
        Console.WriteLine("Exiting: Resetting to center.");
        foreach (var session in modifiedSessions.ToList())
        {
            try
            {
                SetAudioSessionPanning(session, 0);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
    
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

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

class Program
{
    static void Main()
    {
        var audioPanner = new WinPan();

        // Add Ctrl-C (interrupt) event handler
        Console.CancelKeyPress += (sender, e) =>
        {
            audioPanner.ResetBalance();
            e.Cancel = false;
        };

        Console.WriteLine("Press Ctrl-C to exit.");
        
        // Main loop to continuously update panning
        while (true)
        {
            audioPanner.UpdateAudioPanning();
            Thread.Sleep(500);
        }
    }
}