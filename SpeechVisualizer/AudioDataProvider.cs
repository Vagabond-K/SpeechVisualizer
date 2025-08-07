namespace SpeechVisualizer
{
    public abstract class AudioDataProvider(AudioData audioData)
    {
        public AudioData AudioData { get; } = audioData;
    }
}
