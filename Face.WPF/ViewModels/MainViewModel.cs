using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AForge.Video.DirectShow;
using System.Windows.Threading;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using AForge.Video;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics.Tracing;
using Face.WPF.Utils;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.IO.Ports;
using Face.WPF.Models;
using Face.WPF.Views;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using static Face.WPF.Models.FaceModel;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Security;
using System.Windows.Controls;
using System.Windows.Forms;
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Face.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private FilterInfoCollection videoDevices; // 存储可用摄像头设备的集合
        private VideoCaptureDevice videoSource; // 视频采集设备
        private static SolidColorBrush blueBrush = new SolidColorBrush(Color.FromRgb(45, 110, 244));
        private static SolidColorBrush redBrush = new SolidColorBrush(Color.FromRgb(248, 73, 30));
        private static SolidColorBrush greenBrush = new SolidColorBrush(Color.FromRgb(49, 184, 74));
        private byte[] dataReceivedBuff = new byte[] { };
        private Stopwatch stopwatch = new Stopwatch();
        private bool isReadImageData = false;
        private byte[] imageData;
        private static System.Threading.Timer heartbeatTimer;
        private byte heartbeat = 255;

        [ObservableProperty] private List<string> videoList = new List<string>();
        [ObservableProperty] private int videoIndex = -1;
        [ObservableProperty] private string[] portName = new string[] { };
        [ObservableProperty] private string serialName = "COM4";
        [ObservableProperty] private string baudRate = "115200";
        [ObservableProperty] private string parity = "None";
        [ObservableProperty] private string dataBits = "8";
        [ObservableProperty] private string stopBits = "One";
        [ObservableProperty] private string handshake = "None";
        [ObservableProperty] private bool isConnect = false;
        [ObservableProperty] private int tabIndex = 0;
        [ObservableProperty] private bool faceDirIsEnble = false;
        [ObservableProperty] private string account;
        [ObservableProperty] private string password;
        [ObservableProperty] private Object contentControl;
        [ObservableProperty] private Object vedioContentControl;
        [ObservableProperty] private string runningText;
        [ObservableProperty] private bool isHeartbeat = true;
        [ObservableProperty] private int userImageIndex = -1;
        [ObservableProperty] private string[] userInfo = new string[3];
        [ObservableProperty] private bool isLogOutCheck = false;
        [ObservableProperty] private bool mIDisEnabled = false;

        private byte scanFaceTimeOut = 10;
        public MainWindow MainWindow { get; set; }
        public ConsoleView ConsoleObj { get; set; }
        public bool[] BtnRadioIsCheck { get; set; } = new bool[50];
        public int ImageRotateFlipIndex
        {
            get => MainModel.imageRotateFlipIndex;
            set => MainModel.imageRotateFlipIndex = value;
        }
        public byte ScanFaceSuccessShutdown { get; set; } = 0;
        public byte ScanFaceTimeOut { get => scanFaceTimeOut; set { scanFaceTimeOut = value; Gl.scanFaceTimeOut = value * 1000; } }
        public bool MsgEnrollData_Admin { get; set; } = false;
        public string MsgEnrollData_Name { get; set; } = string.Empty;
        public byte MsgEnrollData_Face { get; set; } = 0;
        public ushort MsgEnrollData_Time { get; set; } = 10;
        public ushort[] ImageDataCapture { get; set; } = { 10, 1 };
        public ushort SaveImageNo { get; set; } = 1;
        public ushort UserID { get => (ushort)Gl.faceUserID; set => Gl.faceUserID = value; }
        public byte RegisterType { get; set; } = 0;
        public byte ReRegisterType { get; set; } = 0;
        public byte[] StartOTA { get; set; } = { 0x00, 0x00, 0x00, 0x00 };
        public byte[] EncryptionData { get; set; } = { 255, 255, 255, 255, 1 };
        public byte DevBaudrateIndex { get; set; } = 0;
        public string SetKey { get; set; } = "ABCD1234567890";
        public byte LogTypeIndex { get; set; } = 1;
        public byte[] ThresholdLevel { get; set; } = { 2, 2 };
        public byte DebugMode { get; set; } = 0;

        public SerialPort SerialPort
        {
            get
            {
                if (Gl.MySerialPort == null ||
                    Gl.MySerialPort.PortName != SerialName ||
                    Gl.MySerialPort.BaudRate.ToString() != BaudRate ||
                    Gl.MySerialPort.Parity.ToString() != Parity ||
                    Gl.MySerialPort.DataBits.ToString() != DataBits ||
                    Gl.MySerialPort.StopBits.ToString() != StopBits ||
                    Gl.MySerialPort.Handshake.ToString() != Handshake)
                {
                    Gl.MySerialPort = new SerialPort()
                    {
                        PortName = SerialName,
                        BaudRate = int.Parse(BaudRate),
                        Parity = Enum.GetValues(typeof(Parity)).Cast<Parity>().Where(w => w.ToString() == Parity).First(),
                        DataBits = int.Parse(DataBits),
                        StopBits = Enum.GetValues(typeof(StopBits)).Cast<StopBits>().Where(w => w.ToString() == StopBits).First(),
                        Handshake = Enum.GetValues(typeof(Handshake)).Cast<Handshake>().Where(w => w.ToString() == Handshake).First(),
                    };
                    Gl.MySerialPort.DataReceived += DataReceivedHandler;
                }
                return Gl.MySerialPort;
            }
        }

        public MainViewModel()
        {
            Gl.closeVideo = ShutCameara;

            Gl.Login = Login;

            // 获取可用摄像头设备列表
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            for (int i = 0; i < videoDevices.Count; i++)
            {
                videoList.Add(videoDevices[i].Name);
                if (videoDevices[i].Name == "2EOUMBS.1")
                    videoIndex = i;
            }

            //选择指定摄像头设备
            if (videoIndex > -1)
            {
                videoSource = new VideoCaptureDevice(videoDevices[videoIndex].MonikerString);
                videoSource.NewFrame += MainModel.NewFrame;
                videoSource.Start();
            }

            //串口连接与心跳
            SerialPortConnect();
            Gl.SerialWrite = SerialWrite;
            heartbeatTimer = new System.Threading.Timer(TimerCallback, null, 3000, 5000);

            ContentControl = new LoginView();
        }

        private void Login(UserModel userModel)
        {
            Gl.IsStartVedio = false;
            MainWindow.Width = 800;
            MainWindow.Height = 730;
            UserInfo[0] = userModel.Name;
            UserInfo[1] = userModel.Account == null ? userModel.FaseId.ToString() : HashEncrypMD5.Md5Decrypt(userModel.Account, HashEncrypMD5.Key);
            UserInfo[2] = UtilsHelper.GetEnumDescription<DepartmentType, byte>((byte)userModel.Department);
            OnPropertyChanged(nameof(UserInfo));

            int tabIndex = 0;
            switch (userModel.Auth)
            {
                case UserType.Root:
                    tabIndex = 0;
                    MainWindow.Height = 755;
                    MainWindow.ScrView.Visibility = Visibility.Visible;
                    VedioContentControl = Gl.VideoView;
                    Gl.IsStartVedio = true;
                    break;
                case UserType.Admin: tabIndex = 1; break;
                case UserType.Operator: tabIndex = 2; break;
                case UserType.Maintain: tabIndex = 3; break;
                case UserType.Visitor: tabIndex = 4; break;
                default: break;
            }
            MainWindow.SetBtn.Visibility = Visibility.Hidden;
            UserImageIndex = tabIndex + 1;
            ContentControl = new UserView(tabIndex, MainWindow);
            MIDisEnabled = true;
            WindowCenter();
        }

        [RelayCommand] private void LoginOutCheck() => IsLogOutCheck = !IsLogOutCheck;

        [RelayCommand]
        private void LogOut()
        {
            UserImageIndex = -1;
            IsLogOutCheck = false;
            MainWindow.Width = 330;
            MainWindow.Height = 720;
            MainWindow.ScrView.Visibility = Visibility.Hidden;
            MainWindow.SetBtn.Visibility = Visibility.Visible;
            MainWindow.ScrView.Visibility = Visibility.Hidden;
            ContentControl = new LoginView();
            MIDisEnabled = false;
            WindowCenter();
        }

        /// <summary>
        /// 切换视频
        /// </summary>
        [RelayCommand]
        private void SwitchVideo()
        {
            Task.Run(() =>
            {
                ShutCameara();
                videoSource = new VideoCaptureDevice(videoDevices[VideoIndex].MonikerString);
                videoSource.NewFrame += MainModel.NewFrame;
                videoSource.Start();
            });
        }

        [RelayCommand]
        private void SwitchMid()
        {
            if (BtnRadioIsCheck[2])
                TabIndex = 1;
            else if (BtnRadioIsCheck[3] || BtnRadioIsCheck[7])
            {
                TabIndex = 2;
                FaceDirIsEnble = BtnRadioIsCheck[3] ? true : false;
            }
            else if (BtnRadioIsCheck[4])
                TabIndex = 3;
            else if (BtnRadioIsCheck[5])
                TabIndex = 4;
            else if (BtnRadioIsCheck[6])
                TabIndex = 5;
            else if (BtnRadioIsCheck[8] || BtnRadioIsCheck[10])
                TabIndex = 6;
            else if (BtnRadioIsCheck[13])
                TabIndex = 7;
            else if (BtnRadioIsCheck[15])
                TabIndex = 8;
            else if (BtnRadioIsCheck[18] ||
                     BtnRadioIsCheck[19] ||
                     BtnRadioIsCheck[25] ||
                     BtnRadioIsCheck[30] ||
                     BtnRadioIsCheck[31])
                TabIndex = 9; // 未实现
            else if (BtnRadioIsCheck[20])
                TabIndex = 10;
            else if (BtnRadioIsCheck[21])
                TabIndex = 11;
            else if (BtnRadioIsCheck[22] || BtnRadioIsCheck[23])
                TabIndex = 12;
            else if (BtnRadioIsCheck[24])
                TabIndex = 13;
            else if (BtnRadioIsCheck[26])
                TabIndex = 14;
            else if (BtnRadioIsCheck[28] || BtnRadioIsCheck[32])
                TabIndex = 15;
            else TabIndex = 0;
        }

        [RelayCommand]
        private void SerialPortConnect()
        {
            try
            {
                if (SerialPort.IsOpen)
                {
                    Gl.MySerialPort.Close();
                    Gl.faseState = 0;
                    IsConnect = false;
                    RunningText = "串口未连接";
                    MIDisEnabled = false;
                }
                else
                {
                    Gl.MySerialPort.Open();
                    Gl.faseState = 1;
                    IsConnect = true;
                    RunningText = String.Empty;
                    if (MainWindow != null && MainWindow.contentControl.Content is UserView)
                        MIDisEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Gl.faseState = 0;
                IsConnect = false;
                RunningText = $"串口异常:{ex.Message}";
            }
        }

        [RelayCommand]
        private void SerialSend()
        {
            for (int i = 0; i < BtnRadioIsCheck.Length; i++)
            {
                if (BtnRadioIsCheck[i])
                {
                    SerialWrite(i);
                    break;
                }
            }
        }

        [RelayCommand]
        private void HearbeatControl()
        {
            if (!IsHeartbeat)
            {
                heartbeatTimer.Change(0, Timeout.Infinite);
                RunningText = String.Empty;
            }
            else
                heartbeatTimer.Change(0, 5000);
        }

        private void TimerCallback(object state)
        {
            heartbeat = 255;
            if (SerialPort.IsOpen)
            {
                SerialWrite(1);
                long ticks = DateTime.Now.Ticks;
                while (DateTime.Now.Ticks - ticks < 5_000_0000 && IsHeartbeat)
                {
                    switch (heartbeat)
                    {
                        case 0: Gl.faseState = 1; RunningText = string.Empty; return;
                        case 1: Gl.faseState = 1; RunningText = string.Empty; return;
                        case 2: Gl.faseState = 0; RunningText = UtilsHelper.GetEnumDescription<ModuleState, byte>(heartbeat); return;
                        case 3: Gl.faseState = 0; RunningText = UtilsHelper.GetEnumDescription<ModuleState, byte>(heartbeat); return;
                        case 4: Gl.faseState = 0; RunningText = UtilsHelper.GetEnumDescription<ModuleState, byte>(heartbeat); return;
                        default: break;
                    }

                    Thread.Sleep(100);
                }
                if (IsHeartbeat)
                {
                    Gl.faseState = 0;
                    RunningText = "串口心跳响应超时";
                }
            }
            else
            {
                Gl.faseState = 0;
                RunningText = "串口未连接";
            }
        }

        private void SerialWrite(int index)
        {
            Gl.faceID = -1;

            byte[] bytes = new byte[] { 0xEF, 0xAA };
            byte[] bytesBuff = null;
            switch (index)
            {   //0x10 停止当前处理 
                case 0: bytesBuff = new byte[] { MID_RESET, 0x00, 0x00 }; break;
                //0x11 获取当前状态
                case 1: bytesBuff = new byte[] { MID_GETSTATUS, 0x00, 0x00 }; break;
                //0x12 鉴权解锁
                case 2: bytesBuff = new byte[] { MID_VERIFY, 0x00, 0x02, ScanFaceSuccessShutdown, ScanFaceTimeOut }.ToArray(); break;
                //0x13 交互录入
                case 3:
                    byte[] nameBytes = Encoding.UTF8.GetBytes(MsgEnrollData_Name);
                    byte[] nameArray = new byte[32];
                    Array.Copy(nameBytes, 0, nameArray, 0, Math.Min(32, nameBytes.Length));

                    byte faceDir = 0x00;
                    switch (MsgEnrollData_Face)
                    {
                        case 0: faceDir = (byte)FaceDir.FACE_DIRECTION_UNDEFINE; break;
                        case 1: faceDir = (byte)FaceDir.FACE_DIRECTION_MIDDLE; break;
                        case 2: faceDir = (byte)FaceDir.FACE_DIRECTION_RIGHT; break;
                        case 3: faceDir = (byte)FaceDir.FACE_DIRECTION_LEFT; break;
                        case 4: faceDir = (byte)FaceDir.FACE_DIRECTION_DOWN; break;
                        case 5: faceDir = (byte)FaceDir.FACE_DIRECTION_UP; break;
                        default: break;
                    }

                    bytesBuff = new byte[] { }.Append(MID_ENROLL)
                                              .Concat(new byte[] { 0x00, 0x23 })
                                              .Concat(new Msg_enroll_data(BitConverter.GetBytes(MsgEnrollData_Admin)[0], nameArray, faceDir, (byte)MsgEnrollData_Time).GetBytes())
                                              .ToArray();
                    break;
                // 抓拍本地存储
                case 4: bytesBuff = new byte[] { MID_SNAPIMAGE, 0x00, 0x02, (byte)ImageDataCapture[0], (byte)ImageDataCapture[1] }.ToArray(); break;
                // 上传图片大小
                case 5: bytesBuff = new byte[] { MID_GETSAVEDIMAGE, 0x00, 0x01, (byte)SaveImageNo }; break;
                //0x1D 单帧录入
                case 7:
                    nameBytes = Encoding.UTF8.GetBytes(MsgEnrollData_Name);
                    nameArray = new byte[32];
                    Array.Copy(nameBytes, 0, nameArray, 0, Math.Min(32, nameBytes.Length));

                    bytesBuff = new byte[] { }.Append(MID_ENROLL_SINGLE)
                                              .Concat(new byte[] { 0x00, 0x23 })
                                              .Concat(new Msg_enroll_data(BitConverter.GetBytes(MsgEnrollData_Admin)[0], nameArray, 0x00, (byte)MsgEnrollData_Time).GetBytes())
                                              .ToArray();
                    break;
                //0x20 删除指定用户
                case 8:
                    byte[] userID = BitConverter.GetBytes(UserID);
                    bytesBuff = new byte[] { MID_DELUSER, 0x00, 0x02, userID[1], userID[0] };
                    var userCount = DB.Fsql.Select<UserModel>().Where(w => w.FaseId == UserID).Count();
                    if (userCount > 0)
                        if (MessageBox.Show($"是否删除面部ID[{UserID}] ? ", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        { 
                            DB.Fsql.Delete<UserModel>().Where(w => w.FaseId == UserID).ExecuteAffrows();
                            Gl.RefreshUserInfos();
                        }
                        else
                        {
                            Gl.PrintLogColor("删除指定用户操作取消", Brushes.Yellow, null);
                            return;
                        }
                    break;
                //0x21 删除全部用户
                case 9: 
                    bytesBuff = new byte[] { MID_DELALL, 0x00, 0x00 };
                    if (MessageBox.Show("是否删除全部面部ID ? ", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    { 
                        DB.Fsql.Delete<UserModel>().Where(w => w.FaseId > 0).ExecuteAffrows();
                        Gl.RefreshUserInfos();
                    }
                    else
                    {
                        Gl.PrintLogColor("删除全部用户操作取消", Brushes.Yellow, null);
                        return;
                    }
                    break;
                //0x22 获取用户信息
                case 10:
                    userID = BitConverter.GetBytes(UserID);
                    bytesBuff = new byte[] { MID_GETUSERINFO, 0x00, 0x02, userID[1], userID[0] }; break;
                //0x23 清除录入状态
                case 11: bytesBuff = new byte[] { MID_FACERESET, 0x00, 0x00 }; break;
                //0x24 所有用户信息
                case 12: bytesBuff = new byte[] { MID_GET_ALL_USERID, 0x00, 0x00 }; break;
                //0x26 扩展录入
                case 13:
                    nameBytes = Encoding.UTF8.GetBytes(MsgEnrollData_Name);
                    nameArray = new byte[32];
                    faceDir = 0x00;
                    switch (MsgEnrollData_Face)
                    {
                        case 0: faceDir = (byte)FaceDir.FACE_DIRECTION_UNDEFINE; break;
                        case 1: faceDir = (byte)FaceDir.FACE_DIRECTION_MIDDLE; break;
                        case 2: faceDir = (byte)FaceDir.FACE_DIRECTION_RIGHT; break;
                        case 3: faceDir = (byte)FaceDir.FACE_DIRECTION_LEFT; break;
                        case 4: faceDir = (byte)FaceDir.FACE_DIRECTION_DOWN; break;
                        case 5: faceDir = (byte)FaceDir.FACE_DIRECTION_UP; break;
                        default: break;
                    }
                    Array.Copy(nameBytes, 0, nameArray, 0, Math.Min(32, nameBytes.Length));
                    bytesBuff = new byte[] { MID_ENROLL_ITG, 0x00, 0x28, BitConverter.GetBytes(MsgEnrollData_Admin)[0] }
                                .Concat(nameArray)
                                .Concat(new byte[] { faceDir, RegisterType, ReRegisterType, (byte)MsgEnrollData_Time, 0x00, 0x00, 0x00 })
                                .ToArray();
                    break;
                //0x30 获取版本信息
                case 14: bytesBuff = new byte[] { MID_GET_VERSION, 0x00, 0x00 }; break;
                //0x40 进入OTA模式
                case 15: bytesBuff = new byte[] { MID_START_OTA, 0x00, 0x04 }.Concat(StartOTA).ToArray(); break;
                //0x41 退出OTA模式
                case 16: bytesBuff = new byte[] { MID_STOP_OTA, 0x00, 0x00 }; break;
                //0x42 获取OTA状态
                case 17: bytesBuff = new byte[] { MID_GET_OTA_STATUS, 0x00, 0x00 }; break;
                //0x50 设解密随机数
                case 20:
                    int seconds = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
                    bytesBuff = new byte[] { MID_INIT_ENCRYPTION, 0x00, 0x09 }
                                .Concat(EncryptionData)
                                .Concat(BitConverter.GetBytes(seconds))
                                .ToArray();
                    break;
                //0x51 OTA设波特率
                case 21: bytesBuff = new byte[] { MID_CONFIG_BAUDRATE, 0x00, 0x01, DevBaudrateIndex }; break;
                //0x52 设定Release密钥 
                case 22:
                    byte[] keyBytes = new byte[16];
                    byte[] keyArray = Encoding.UTF8.GetBytes(SetKey);
                    int keyLength = keyArray.Length < 16 ? keyArray.Length : 16;
                    Array.Copy(keyArray, keyBytes, keyLength);
                    bytesBuff = new byte[] { MID_SET_RELEASE_ENC_KEY, 0x00, 0x10 }.Concat(keyBytes).ToArray();
                    break;
                //0x52 设定Debug密钥 
                case 23:
                    keyBytes = new byte[16];
                    keyArray = Encoding.UTF8.GetBytes(SetKey);
                    keyLength = keyArray.Length < 16 ? keyArray.Length : 16;
                    Array.Copy(keyArray, keyBytes, keyLength);
                    bytesBuff = new byte[] { MID_SET_DEBUG_ENC_KEY, 0x00, 0x10 }.Concat(keyBytes).ToArray();
                    break;
                //0x60 日志存志内存
                case 24: bytesBuff = new byte[] { MID_GET_LOGFILE, 0x00, 0x01, LogTypeIndex }; break;
                //0xD4 设置算法等级
                case 26: bytesBuff = new byte[] { MID_SET_THRESHOLD_LEVEL, 0x00, 0x02 }.Concat(ThresholdLevel).ToArray(); break;
                //0xED 模组准备关机
                case 27: bytesBuff = new byte[] { MID_POWERDOWN, 0x00, 0x00 }; break;
                //0xF0 调试模式
                case 28: bytesBuff = new byte[] { MID_DEBUG_MODE, 0x00, 0x01, DebugMode }; break;
                //0xF1 获取调试存储数据包
                case 29: bytesBuff = new byte[] { MID_GET_DEBUG_INFO, 0x00, 0x00 }; break;
                //0xFE 演示模式
                case 32: bytesBuff = new byte[] { MID_DEMOMODE, 0x00, 0x01, DebugMode }; break;
                //0x4D 抓拍图片存储
                case 33:
                    bytes = new byte[] { 0x4D, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x00 };
                    Gl.PrintLogColor(BitConverter.ToString(bytes).Replace("-", " "), blueBrush, null);
                    stopwatch.Restart();
                    SerialPort.Write(bytes, 0, bytes.Length);
                    return;
                default: break;
            }
            if (bytesBuff is null) return;
            byte parityCheck = UtilsHelper.PerformXOR(bytesBuff);
            bytes = bytes.Concat(bytesBuff).Append(parityCheck).ToArray();
            Gl.PrintLogColor(BitConverter.ToString(bytes).Replace("-", " "), blueBrush, null);
            try
            {
                stopwatch.Restart();
                SerialPort.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Gl.PrintLogColor(ex.Message, redBrush, null);
            }
        }

        /// <summary>
        /// 串口回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;

            int byteToRead = sp.BytesToRead;
            byte[] buffer = new byte[byteToRead];

            int byteRead = 0;
            try
            {
                byteRead = sp.Read(buffer, 0, byteToRead);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            if (byteRead == 0) return;
            Debug.WriteLine(DateTime.Now.ToString() + "\t" + BitConverter.ToString(buffer).Replace('-', ' '));

            //条件跳过
            if (isReadImageData) goto readImage;
            else if (buffer[0] == 0x6D && buffer[1] == 0x78)
            {
                imageData = new byte[] { }.Concat(buffer).ToArray();
                isReadImageData = true;
                goto readImage;
            }
            else if (buffer[3] == 0x19) goto encrypted;

            #region 数据分包传输，长度不对需拼接
            if (buffer.First() == 0xEF)
            {
                int byteSize = BitConverter.ToInt16(new byte[] { buffer[4], buffer[3] }, 0) + 5 + 1;
                if (buffer.Length != byteSize)
                {
                    var arrCopy = new byte[byteRead];
                    Array.Copy(buffer, 0, arrCopy, 0, byteRead);
                    dataReceivedBuff = dataReceivedBuff.Concat(arrCopy).ToArray(); // buffer可用长度不准确需要截取
                    return;
                }
            }
            else
            {
                dataReceivedBuff = dataReceivedBuff.Concat(buffer).ToArray();
                if (dataReceivedBuff.First() == 0xEF)
                {
                    int byteSize = BitConverter.ToInt16(new byte[] { buffer[4], buffer[3] }, 0) + 5 + 1;
                    if (buffer.Length != byteSize)
                    {
                        buffer = new byte[] { }.Concat(dataReceivedBuff).ToArray();
                        dataReceivedBuff = new byte[] { };
                    }
                    else if (buffer.Length < byteSize)
                    {
                        dataReceivedBuff = new byte[] { };
                        return;
                    }
                    else return;
                }
                else
                {
                    dataReceivedBuff = new byte[] { };
                    return;
                }
            }
        #endregion

        encrypted:
            stopwatch.Stop();
            Gl.PrintLogColor(BitConverter.ToString(buffer).Replace('-', ' '), greenBrush, null);
            Gl.PrintLog("耗时(ms)：" + stopwatch.ElapsedMilliseconds);

            switch (buffer[2])
            {
                case 0x00: // MID_REPLY
                    Gl.PrintLog(UtilsHelper.GetEnumDescription<MID_REPLY_RES, byte>(buffer[6]));
                    //只能特定的状态下才能进行解析报文内容
                    if (buffer[6] == 0 ||
                        buffer[6] == 10 ||
                        buffer[5] == 0x12 ||
                        (buffer[5] == 0x11 && buffer[6] == 1 ||
                         buffer[5] == 0x11 && buffer[6] == 2 ||
                         buffer[5] == 0x11 && buffer[6] == 3 ||
                         buffer[5] == 0x11 && buffer[6] == 4)) { }
                    else return;
                    switch (buffer[5])
                    {
                        case 0x11: Gl.PrintLog(UtilsHelper.GetEnumDescription<ModuleState, byte>(buffer[6])); heartbeat = buffer[6]; break;
                        case 0x12:
                            Gl.faseReplyRes = buffer[6];
                            short id;
                            string userName, isAdmin;
                            try
                            {
                                if (buffer[6] == 0x00)
                                {
                                    id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                                    userName = Encoding.UTF8.GetString(buffer.Skip(9).Take(32).ToArray());
                                    isAdmin = buffer[41] == 0x00 ? "否" : "是";
                                    string unlockStatu = buffer[42] == 0x00 ? "睁眼" : "闭眼";
                                    Gl.PrintLog("已注册用户ID：" + id);
                                    Gl.PrintLog("用户名字：" + userName);
                                    Gl.PrintLog("是否为管理员：" + isAdmin);
                                    Gl.PrintLog("解锁中眼状态(有问题)：" + unlockStatu);
                                    Gl.faceID = id;
                                }
                                else if (buffer[6] == 0x08)
                                    Gl.faceID = -2;
                            }
                            catch (Exception ex) { Gl.PrintLogColor(ex.Message, redBrush, null); }
                            break;
                        case 0x13:
                            id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                            Gl.PrintLog("已注册用户ID：" + id);
                            Gl.PrintLog("人脸朝向：" + UtilsHelper.GetEnumDescription<FaceDir, byte>(buffer[9]));
                            break;
                        case 0x1D:
                            id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                            Gl.PrintLog("已注册用户ID：" + id);
                            Gl.PrintLog("人脸朝向：" + UtilsHelper.GetEnumDescription<FaceDir, byte>(buffer[9]));
                            break;
                        case 0x22:
                            id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                            userName = Encoding.UTF8.GetString(buffer.Skip(9).Take(32).ToArray());
                            isAdmin = buffer[41] == 0x00 ? "否" : "是";
                            Gl.PrintLog("已注册用户ID：" + id);
                            Gl.PrintLog("用户名字：" + userName);
                            Gl.PrintLog("是否为管理员：" + isAdmin);
                            break;
                        case 0x24:
                            try
                            {
                                int allUserCount = BitConverter.ToInt16(new byte[] { buffer[7], buffer[8] }, 0);
                                string userId = string.Empty;
                                for (int i = 0; i < allUserCount; i++)
                                    userId += BitConverter.ToInt16(new byte[] { buffer[9 + 2 * i], buffer[8 + 2 * i] }, 0).ToString() + "  ";
                                Gl.PrintLog("已注册用户数量：" + allUserCount);
                                Gl.PrintLog($"分别是：[ {userId}]");
                            }
                            catch (Exception) { Gl.PrintLog("没有已录入的用户"); }
                            break;
                        case 0x26:
                            if (buffer[4] == 2)
                            {
                                Gl.faseReplyRes = buffer[6];
                                Gl.PrintLog(UtilsHelper.GetEnumDescription<FaceDir, byte>(buffer[6]));
                                Gl.faceID = -2;
                            }
                            else
                            {
                                Gl.isCapture = true;
                                id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                                Gl.PrintLog("已注册用户ID：" + id);
                                Gl.faceID = id;
                            }
                            break;
                        case 0x30: Gl.PrintLog("版本：" + Encoding.UTF8.GetString(buffer.Skip(6).Take(32).ToArray())); break;
                        default: break;
                    }
                    break;
                case 0x01: // MID_NOTE
                    try
                    {
                        Gl.PrintLog(UtilsHelper.GetEnumDescription<MID_NOTE_RES, byte>(buffer[5]));

                        int[] stateBytes = new int[]
                        {
                            BitConverter.ToInt16(new byte[] { buffer[6], buffer[7] }, 0),  // 人脸状态
                            BitConverter.ToInt16(new byte[] { buffer[8], buffer[9] }, 0),  // 距图片最左侧距离
                            BitConverter.ToInt16(new byte[] { buffer[10], buffer[11] }, 0),// 距图片最上方距离
                            BitConverter.ToInt16(new byte[] { buffer[12], buffer[13] }, 0),// 距图片最右方距离
                            BitConverter.ToInt16(new byte[] { buffer[14], buffer[15] }, 0),// 距图片最下方距离
                            BitConverter.ToInt16(new byte[] { buffer[16], buffer[17] }, 0),// yaw
                            BitConverter.ToInt16(new byte[] { buffer[18], buffer[19] }, 0),// pitch
                            BitConverter.ToInt16(new byte[] { buffer[20], buffer[21] }, 0) // roll
                        };
                        Gl.fasePos = stateBytes;
                        Gl.PrintLog("人脸状态：" + UtilsHelper.GetEnumDescription<FaceState, int>(stateBytes[0]));
                        Gl.PrintLog("距图片最左侧距离：" + stateBytes[1]);
                        Gl.PrintLog("距图片最上方距离：" + stateBytes[2]);
                        Gl.PrintLog("距图片最右方距离：" + stateBytes[3]);
                        Gl.PrintLog("距图片最下方距离：" + stateBytes[4]);
                        int yawNum = stateBytes[5];
                        int pitchNum = stateBytes[6];
                        int rollNum = stateBytes[7];
                        string yaw = yawNum < 0 ? "左转头" : yawNum > 0 ? "右转头" : yawNum.ToString();
                        string pitch = pitchNum < 0 ? "上抬头" : pitchNum > 0 ? "下低头" : pitchNum.ToString();
                        string roll = rollNum < 0 ? "右歪头" : rollNum > 0 ? "左歪头" : rollNum.ToString();
                        Gl.PrintLog("yaw：" + yaw);
                        Gl.PrintLog("pitch：" + pitch);
                        Gl.PrintLog("roll：" + roll);
                        stopwatch.Restart();
                    }
                    catch (Exception ex) { Gl.PrintLogColor(ex.Message, redBrush, null); }
                    break;
                case 0x02: // MID_IMAGE
                    break;
                default: break;
            }
            return;

        readImage:
            imageData = imageData.Concat(buffer).ToArray();
            if (imageData.Length >= 78320)
            {
                isReadImageData = false;
                Gl.PrintLogColor(BitConverter.ToString(imageData).Replace('-', ' '), greenBrush, null);
                Gl.PrintLog("耗时(ms)：" + stopwatch.ElapsedMilliseconds);
            }
        }

        private void WindowCenter()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            MainWindow.Left = (screenWidth - MainWindow.Width) / 2;
            MainWindow.Top = (screenHeight - MainWindow.Height) / 2;
        }

        private void ShutCameara()
        {
            if (videoSource != null)
            {
                videoSource.SignalToStop();
                videoSource.Stop();
                videoSource = null;
            }
        }
    }
}