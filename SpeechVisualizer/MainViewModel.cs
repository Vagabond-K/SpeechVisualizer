namespace SpeechVisualizer
{
    class MainViewModel
    {
        public MainViewModel()
        {
            audioAnalyzer = new AudioAnalyzer(10);
            var audioData = audioAnalyzer.AudioData;

            Speaker = new Speaker(audioData, false);
            SpeakerInCamera = new Speaker(audioData, true);
            Background = new Background(audioData);
        }

        private readonly AudioAnalyzer audioAnalyzer;

        public double CameraWidth => Shared.CameraWidth;
        public double CameraHeight => Shared.CameraHeight;
        public Speaker Speaker { get; }
        public Speaker SpeakerInCamera { get; }
        public Background Background { get; }
    }
}
