using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Linq;

namespace SpeechVisualizer
{
    class AudioAnalyzer
    {
        public AudioAnalyzer(int bandCount)
        {
            bandEnergies = new float[bandCount];
            AudioData.Bands = [.. Enumerable.Range(0, bandCount).Select(i => new AudioData())];
            capture = new WasapiCapture();
            waveFormat = capture.WaveFormat;

            rmsBuffer = new float[waveFormat.SampleRate / 50];
            fftBuffer = new Complex[(int)Math.Pow(2, Math.Ceiling(Math.Log2(waveFormat.SampleRate / 50)))];
            capture.DataAvailable += Capture_DataAvailable;
            capture.RecordingStopped += Capture_RecordingStopped;
            capture.StartRecording();
        }

        private readonly WasapiCapture capture;
        private readonly WaveFormat waveFormat;
        private readonly float[] rmsBuffer;
        private readonly Complex[] fftBuffer;
        private readonly float[] bandEnergies;
        private int rmsBufferIndex = 0;
        private int rmsSampleCount = 0;
        private float rmsSum = 0;
        private int fftBufferIndex = 0;

        public AudioData AudioData { get; } = new AudioData();

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            int bytesPerSample = waveFormat.BitsPerSample / 8;
            int samplesRecorded = e.BytesRecorded / bytesPerSample / waveFormat.Channels;

            if (samplesRecorded > 0)
            {
                for (int i = 0; i < samplesRecorded; i++)
                {
                    float sampleSum = 0;

                    for (int ch = 0; ch < waveFormat.Channels; ch++)
                    {
                        int byteIndex = (i * waveFormat.Channels + ch) * bytesPerSample;
                        float sample;

                        if (waveFormat.BitsPerSample == 16)
                        {
                            short sample16 = BitConverter.ToInt16(e.Buffer, byteIndex);
                            sample = sample16 / 32768f;
                        }
                        else if (waveFormat.BitsPerSample == 32 && waveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                        {
                            sample = BitConverter.ToSingle(e.Buffer, byteIndex);
                        }
                        else
                        {
                            // 다른 비트 깊이, 인코딩은 상황에 맞게 추가 구현 필요
                            throw new NotSupportedException("BitsPerSample or Encoding not supported");
                        }

                        sampleSum += sample;
                    }

                    var rmsSample = sampleSum * sampleSum;
                    rmsSum -= rmsBuffer[rmsBufferIndex];
                    rmsBuffer[rmsBufferIndex] = rmsSample;
                    rmsSum += rmsSample;
                    rmsBufferIndex++;

                    if (rmsBufferIndex >= rmsBuffer.Length)
                        rmsBufferIndex = 0;

                    if (rmsSampleCount < rmsBuffer.Length)
                        rmsSampleCount++;

                    AudioData.Volume = Math.Clamp(Math.Sqrt(rmsSum / rmsSampleCount), 0, 1);

                    fftBuffer[fftBufferIndex].X = sampleSum;
                    fftBuffer[fftBufferIndex].Y = 0;
                    fftBufferIndex++;

                    if (fftBufferIndex >= fftBuffer.Length)
                    {
                        ProcessFFT();
                        fftBufferIndex = 0;
                    }
                }
            }
        }

        private void ProcessFFT()
        {
            for (int i = 0; i < fftBuffer.Length; i++)
                fftBuffer[i].X *= (float)FastFourierTransform.HammingWindow(i, fftBuffer.Length);

            FastFourierTransform.FFT(true, (int)Math.Log(fftBuffer.Length, 2.0), fftBuffer);

            int sampleRate = waveFormat.SampleRate;
            float binWidth = sampleRate / (float)fftBuffer.Length;
            float maxFreq = sampleRate / 2;
            float bandWidth = maxFreq / bandEnergies.Length;

            Array.Clear(bandEnergies);

            for (int i = 0; i < fftBuffer.Length / 2; i++)
            {
                float magnitude = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
                float freq = i * binWidth;
                int band = (int)(freq / bandWidth);

                if (band >= 0 && band < bandEnergies.Length)
                    bandEnergies[band] += magnitude;
            }

            for (int i = 0; i < bandEnergies.Length; i++)
                AudioData.Bands[i].Volume = bandEnergies[i];
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            capture.Dispose();
        }
    }
}
