using System.Windows;
using System.Windows.Input;

namespace SpeechVisualizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var cameraManager = new CameraManager(Title, cameraView);

            Loaded += (sender, e) => cameraManager.Start();
            Closed += (sender, e) => cameraManager.Stop();
            MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            };

            DataContext = new MainViewModel();
        }
    }
}