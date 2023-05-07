using CJ_DOCKER.Models;
using CJ_DOCKER.Views;
using Prism.Events;
using Prism.Ioc;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using WPF.Common.Mvvm;

namespace CJ_DOCKER.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IViewLoadedAndUnloadedAware<MainWindow>
    {
        private int useStatus;
        public int UseStatus
        {
            get => useStatus;
            set => SetProperty(ref useStatus, value);
        }
        
        private int notUseStatus;
        public int NotUseStatus
        {
            get => notUseStatus;
            set => SetProperty(ref notUseStatus, value);
        } 
        
        private int inCarStatus;
        public int InCarStatus
        {
            get => inCarStatus;
            set => SetProperty(ref inCarStatus, value);
        }
        
        private int detectionStatus;
        public int DetectionStatus
        {
            get => detectionStatus;
            set => SetProperty(ref detectionStatus, value);
        }
        
        private int dangerStatus;
        public int DangerStatus
        {
            get => dangerStatus;
            set => SetProperty(ref dangerStatus, value);
        }

        private string serverStatus;
        public string ServerStatus
        {
            get => serverStatus;
            set => SetProperty(ref serverStatus, value);
        }
        
        private string connectStatus;
        public string ConnectStatus
        {
            get => connectStatus;
            set => SetProperty(ref connectStatus, value);
        }

        private MainWindow view;

        private Color inColor = Color.FromArgb(255, 237, 125, 49);
        private Color outColor = Color.FromArgb(255, 112, 173, 71);

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="container"></param>
        public MainWindowViewModel(IContainerExtension container) : base(container)
        {
            EventAggregator.GetEvent<MessageReceivedEvent>().Subscribe(MessageReceived, ThreadOption.UIThread);

            UseStatus = 30;
            NotUseStatus = 0;
            InCarStatus = 0;
            DetectionStatus = 0;
            DangerStatus = 0;
        }

        /// <summary>
        /// OnLoaded
        /// </summary>
        /// <param name="view"></param>
        public void OnLoaded(MainWindow view)
        {
            this.view = view;

            Task.Run(() =>
            {
                TCPServer();
            });

            //Thread thread = new Thread(TCPServer); // Thread 객채 생성
            //thread.IsBackground = true; // Form이 종료되면 thread도 종료.
            //thread.Start();
        }

        /// <summary>
        /// OnUnloaded
        /// </summary>
        /// <param name="view"></param>
        public void OnUnloaded(MainWindow view)
        {
        }

        /// <summary>
        /// TCP Server
        /// </summary>
        private void TCPServer()
        {
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 10000);
                listener.Start();

                ServerStatus = "서버 시작";
                ConnectStatus = "연결 기다리는 중";

                byte[] buff = new byte[1024];

                while (true)
                {
                    // (2) TcpClient Connection 요청을 받아들여 서버에서 새 TcpClient 객체를 생성하여 리턴
                    TcpClient tcpClient = listener.AcceptTcpClient();

                    //Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
                    //acceptTask.Wait();
                    //TcpClient tcpClient = acceptTask.Result;

                    ConnectStatus = "연결됨";

                    // (3) TcpClient 객체에서 NetworkStream을 얻어옴 
                    NetworkStream stream = tcpClient.GetStream();

                    // (4) 클라이언트가 연결을 끊을 때까지 데이터 수신
                    int nbytes;
                    while ((nbytes = stream.Read(buff, 0, buff.Length)) > 0)
                    {
                        string response = Encoding.UTF8.GetString(buff, 0, nbytes);
                        EventAggregator.GetEvent<MessageReceivedEvent>().Publish(response);
                    }

                    // (6) 스트림과 TcpClient 객체 
                    stream.Close();
                    tcpClient.Close();

                    ConnectStatus = "연결 기다리는 중";

                    // (7) 계속 반복
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ServerStatus = "서버 종료";
            }
        }

        /// <summary>
        /// 메시지 응답 시
        /// </summary>
        /// <param name="message"></param>
        private void MessageReceived(string message)
        {
            Console.WriteLine(message);
            if (!string.IsNullOrEmpty(message) && message.StartsWith("EVENT"))
            {
                string[] str = message.Split(',');
                if (str.Length == 2)
                {
                    string result = str[1];
                    if (result == "IN")
                    {
                        this.view.Docker1_1.Fill = new SolidColorBrush(inColor);
                        this.view.Docker1_2.Fill = new SolidColorBrush(inColor);
                    }
                    else if (result == "OUT")
                    {
                        this.view.Docker1_1.Fill = new SolidColorBrush(outColor);
                        this.view.Docker1_2.Fill = new SolidColorBrush(outColor);
                    }
                }
            }
        }

        private void SocketConnect()
        {
            using (Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // 어떤 ip이던 5000 포트로 --> 0.0.0.0:5000
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);

                listener.Bind(endPoint);
                listener.Listen(100); // 클리이언트 한계는 100

                while (true)
                {
                    Socket handler = listener.Accept(); // 클라이언트 허용

                    // Receive message.
                    byte[] buffer = new byte[1024];
                    int received = handler.Receive(buffer, SocketFlags.None);
                    string response = Encoding.UTF8.GetString(buffer, 0, received);

                    Console.WriteLine(response);

                    handler.Close();
                }
            }
        }
    }
}
