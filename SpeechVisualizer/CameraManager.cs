using DirectN;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpeechVisualizer
{
    class CameraManager(string title, Visual targetVisual) : IDisposable
    {
        private bool disposedValue;
        private ComObject<IMFVirtualCamera> camera;

        public string Title { get; } = title;
        public Visual TargetVisual { get; } = targetVisual;

        public void Start()
        {
            var width = Shared.CameraWidth;
            var height = Shared.CameraHeight;

            int port = 30000;
            for (; port < 60000; port++)
            {
                try
                {
                    var tcpListener = new TcpListener(IPAddress.Loopback, port);

                    Task.Run(() =>
                    {
                        tcpListener.Start();
                        while (true)
                        {
                            var stream = tcpListener.AcceptTcpClient().GetStream();

                            TargetVisual.Dispatcher.Invoke(() =>
                            {
                                var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                                var buffer = new byte[width * height * 4];
                                Task.Run(() =>
                                {
                                    while (true)
                                    {
                                        try
                                        {
                                            TargetVisual.Dispatcher.Invoke(() =>
                                            {
                                                renderBitmap.Render(TargetVisual);
                                                renderBitmap.CopyPixels(buffer, width * 4, 0);
                                            });
                                            stream.Write(buffer, 0, buffer.Length);
                                            stream.Flush();
                                        }
                                        catch
                                        {
                                            break;
                                        }
                                    }
                                });
                            });
                        }
                    });
                    break;
                }
                catch { }
            }

            var hr = Functions.MFCreateVirtualCamera(
                __MIDL___MIDL_itf_mfvirtualcamera_0000_0000_0001.MFVirtualCameraType_SoftwareCameraSource,
                __MIDL___MIDL_itf_mfvirtualcamera_0000_0000_0002.MFVirtualCameraLifetime_Session,
                __MIDL___MIDL_itf_mfvirtualcamera_0000_0000_0003.MFVirtualCameraAccess_CurrentUser,
                Title,
                $"{{{Shared.CLSID_VCamNet}}}",
            null, 0,
                out var cameraObj);

            if (hr.IsSuccess)
            {
                camera = new ComObject<IMFVirtualCamera>(cameraObj);
                camera.Object.SetUINT32(Shared.FrameStreamPortKey, (uint)port);
                hr = camera.Object.Start(null);
                if (!hr.IsSuccess)
                {
                    //카메라 시작 실패
                }
            }

        }

        public void Stop() => camera?.Object?.Stop();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }
                disposedValue = true;
            }
        }

        // // TODO: 비관리형 리소스를 해제하는 코드가 'Dispose(bool disposing)'에 포함된 경우에만 종료자를 재정의합니다.
        // ~CameraManager()
        // {
        //     // 이 코드를 변경하지 마세요. 'Dispose(bool disposing)' 메서드에 정리 코드를 입력합니다.
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
