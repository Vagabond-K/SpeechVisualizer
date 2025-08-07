namespace SpeechVisualizer
{
    public class Speaker(AudioData audioData, bool inCamera) : AudioDataProvider(audioData)
    {
        public bool InCamera { get; } = inCamera;
    }
}
