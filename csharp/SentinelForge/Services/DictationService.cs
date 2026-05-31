// DictationService.cs — on-device dictation (speech-to-text).
//
// Captures microphone audio with NAudio (16 kHz mono PCM) and transcribes it
// locally with Whisper.net (whisper.cpp). Fully offline after a one-time model
// download. No API keys, no cloud — matching the app's design.
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Whisper.net;

namespace SentinelForge.Services;

public sealed class DictationService : IDisposable
{
    private static readonly WaveFormat Format = new(16000, 16, 1); // Whisper wants 16 kHz mono

    private WaveInEvent? _waveIn;
    private MemoryStream? _pcm;

    public bool IsRecording { get; private set; }

    private static string ModelPath
    {
        get
        {
            string dir;
            try { dir = Windows.Storage.ApplicationData.Current.LocalFolder.Path; }
            catch
            {
                dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SentinelForge");
                Directory.CreateDirectory(dir);
            }
            return Path.Combine(dir, "ggml-small.en.bin");
        }
    }

    /// <summary>Prompt for / confirm microphone access (packaged-app privacy gate).</summary>
    public async Task<bool> RequestMicAccessAsync()
    {
        try
        {
            var cap = Windows.Security.Authorization.AppCapabilityAccess.AppCapability.Create("microphone");
            var status = await cap.RequestAccessAsync();
            return status == Windows.Security.Authorization.AppCapabilityAccess.AppCapabilityAccessStatus.Allowed;
        }
        catch
        {
            return true; // if the API isn't available, let the capture attempt proceed
        }
    }

    public void StartRecording()
    {
        if (IsRecording) return;
        _pcm = new MemoryStream();
        _waveIn = new WaveInEvent { WaveFormat = Format, BufferMilliseconds = 50 };
        _waveIn.DataAvailable += (_, e) => _pcm?.Write(e.Buffer, 0, e.BytesRecorded);
        _waveIn.StartRecording();
        IsRecording = true;
    }

    /// <summary>Stop capture and return the transcribed text. Reports status via progress.</summary>
    public async Task<string> StopAndTranscribeAsync(IProgress<string>? progress = null)
    {
        if (!IsRecording || _waveIn is null || _pcm is null) return "";

        _waveIn.StopRecording();
        _waveIn.Dispose();
        _waveIn = null;
        IsRecording = false;

        byte[] pcm = _pcm.ToArray();
        _pcm.Dispose();
        _pcm = null;
        if (pcm.Length == 0) return "";

        await EnsureModelAsync(progress);

        progress?.Report("Transcribing…");
        NormalizePcm(pcm);                 // boost quiet input for better accuracy
        byte[] wavBytes = BuildWav(pcm);

        // Run inference off the UI thread.
        return await Task.Run(async () =>
        {
            using var factory = WhisperFactory.FromPath(ModelPath);
            using var processor = factory.CreateBuilder().WithLanguage("en").Build();
            using var wav = new MemoryStream(wavBytes);
            var sb = new StringBuilder();
            await foreach (var segment in processor.ProcessAsync(wav))
                sb.Append(segment.Text);
            return sb.ToString().Trim();
        });
    }

    private static async Task EnsureModelAsync(IProgress<string>? progress)
    {
        if (File.Exists(ModelPath) && new FileInfo(ModelPath).Length > 1_000_000) return;

        progress?.Report("Downloading speech model (one-time, ~470 MB)…");
        const string url = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.en.bin";
        using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        using var resp = await http.GetAsync(url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();

        var tmp = ModelPath + ".tmp";
        using (var fs = File.Create(tmp))
        using (var src = await resp.Content.ReadAsStreamAsync())
            await src.CopyToAsync(fs);

        if (File.Exists(ModelPath)) File.Delete(ModelPath);
        File.Move(tmp, ModelPath);
    }

    /// <summary>
    /// Peak-normalize 16-bit PCM so quiet recordings are amplified toward full
    /// scale (helps Whisper). Gain is capped so we don't blow up the noise floor,
    /// and never reduces already-loud audio.
    /// </summary>
    private static void NormalizePcm(byte[] pcm)
    {
        int n = pcm.Length / 2;
        if (n == 0) return;

        int peak = 0;
        for (int i = 0; i < n; i++)
        {
            short s = (short)(pcm[2 * i] | (pcm[2 * i + 1] << 8));
            int a = Math.Abs((int)s);
            if (a > peak) peak = a;
        }
        if (peak == 0) return;

        double gain = (0.95 * 32767.0) / peak;
        if (gain <= 1.0) return;          // already loud enough
        gain = Math.Min(gain, 8.0);       // cap to avoid over-amplifying background noise

        for (int i = 0; i < n; i++)
        {
            short s = (short)(pcm[2 * i] | (pcm[2 * i + 1] << 8));
            int v = (int)Math.Round(s * gain);
            v = Math.Clamp(v, short.MinValue, short.MaxValue);
            pcm[2 * i] = (byte)(v & 0xFF);
            pcm[2 * i + 1] = (byte)((v >> 8) & 0xFF);
        }
    }

    /// <summary>Wrap raw 16-bit PCM (16 kHz mono) in a minimal WAV container.</summary>
    private static byte[] BuildWav(byte[] pcm)
    {
        const int sampleRate = 16000, channels = 1, bits = 16;
        int byteRate = sampleRate * channels * bits / 8;
        int blockAlign = channels * bits / 8;

        using var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
        {
            w.Write(Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + pcm.Length);
            w.Write(Encoding.ASCII.GetBytes("WAVE"));
            w.Write(Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);
            w.Write((short)1);              // PCM
            w.Write((short)channels);
            w.Write(sampleRate);
            w.Write(byteRate);
            w.Write((short)blockAlign);
            w.Write((short)bits);
            w.Write(Encoding.ASCII.GetBytes("data"));
            w.Write(pcm.Length);
            w.Write(pcm);
        }
        return ms.ToArray();
    }

    public void Dispose()
    {
        try { _waveIn?.Dispose(); } catch { }
        _pcm?.Dispose();
    }
}
