// ReadAloudService.cs — text-to-speech ("read aloud") via Windows 11 WinRT
// speech synthesis. Speech lives in C#/WinRT because it is tied to audio
// playback and the UI, not document parsing.
using System;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace SentinelForge.Services;

public sealed class ReadAloudService : IDisposable
{
    // Voice synthesis has an input-size ceiling; cap very long docs for now.
    private const int MaxChars = 10000;

    private readonly SpeechSynthesizer _synth = new();
    private readonly MediaPlayer _player = new();

    /// <summary>Synthesize and play the given text. Replaces any current playback.</summary>
    public async Task SpeakAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (text.Length > MaxChars) text = text.Substring(0, MaxChars);

        SpeechSynthesisStream stream = await _synth.SynthesizeTextToStreamAsync(text);
        _player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
        _player.Play();
    }

    /// <summary>Stop any current playback.</summary>
    public void Stop()
    {
        try
        {
            _player.Pause();
            _player.Source = null;
        }
        catch { }
    }

    public void Dispose()
    {
        _player.Dispose();
        _synth.Dispose();
    }
}
