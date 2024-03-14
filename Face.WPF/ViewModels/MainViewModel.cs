using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

        public ConsoleView ConsoleObj { get; set; }
        public bool[] BtnRadioIsCheck { get; set; } = new bool[50];
        public int ImageRotateFlipIndex
        {
            get => MainModel.imageRotateFlipIndex;
            set => MainModel.imageRotateFlipIndex = value;
        }
        public ushort[] MsgVerifyData { get; set; } = new ushort[2] { 0, 10 };
        public bool MsgEnrollData_Admin { get; set; } = false;
        public string MsgEnrollData_Name { get; set; } = string.Empty;
        public byte MsgEnrollData_Face { get; set; } = 0;
        public ushort MsgEnrollData_Time { get; set; } = 10;
        public ushort[] ImageDataCapture { get; set; } = new ushort[] { 10, 1 };
        public ushort SaveImageNo { get; set; } = 1;
        public ushort UserID { get; set; } = 1;
        public byte RegisterType { get; set; } = 0;
        public bool IsReRegister { get; set; } = true;

        public SerialPort SerialPort
        {
            get
            {
                if (Gl.MySerialPort == null)
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
            Gl.closeVideo = () => videoSource?.Stop();

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

            SerialPortConnect();
        }

        /// <summary>
        /// 切换视频
        /// </summary>
        [RelayCommand]
        private void SwitchVideo()
        {
            Task.Run(() =>
            {
                videoSource.Stop();
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
                    IsConnect = false;
                }
                else
                {
                    Gl.MySerialPort.Open();
                    IsConnect = true;
                }
            }
            catch (Exception ex)
            {
                IsConnect = false;
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

        private void SerialWrite(int index)
        {
            byte[] bytes = new byte[] { 0xEF, 0xAA };
            byte[] bytesBuff = null;
            switch (index)
            {   //0x10 停止当前处理 
                case 0: bytesBuff = new byte[] { MID_RESET, 0x00, 0x00 }; break;
                //0x11 获取当前状态
                case 1: bytesBuff = new byte[] { MID_GETSTATUS, 0x00, 0x00 }; break;
                //0x12 鉴权解锁
                case 2: bytesBuff = new byte[] { MID_VERIFY, 0x00, 0x02, (byte)MsgVerifyData[0], (byte)MsgVerifyData[1] }.ToArray(); break;
                //0x13 交互录入
                case 3:
                    byte[] nameBytes = Encoding.UTF8.GetBytes(MsgEnrollData_Name);
                    byte[] nameArray = new byte[32];
                    Array.Copy(nameBytes, 0, nameArray, 0, Math.Min(32, nameBytes.Length));

                    byte face = 0x00;
                    switch (MsgEnrollData_Face)
                    {
                        case 0: face = (byte)FaceDir.FACE_DIRECTION_UNDEFINE; break;
                        case 1: face = (byte)FaceDir.FACE_DIRECTION_MIDDLE; break;
                        case 2: face = (byte)FaceDir.FACE_DIRECTION_RIGHT; break;
                        case 3: face = (byte)FaceDir.FACE_DIRECTION_LEFT; break;
                        case 4: face = (byte)FaceDir.FACE_DIRECTION_DOWN; break;
                        case 5: face = (byte)FaceDir.FACE_DIRECTION_UP; break;
                        default: break;
                    }

                    bytesBuff = new byte[] { }.Append(MID_ENROLL)
                                              .Concat(new byte[] { 0x00, 0x23 })
                                              .Concat(new Msg_enroll_data(BitConverter.GetBytes(MsgEnrollData_Admin)[0], nameArray, face, (byte)MsgEnrollData_Time).GetBytes())
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
                    bytesBuff = new byte[] { MID_DELUSER, 0x00, 0x02, userID[1], userID[0] }; break;
                //0x21 删除全部用户
                case 9: bytesBuff = new byte[] { MID_DELALL, 0x00, 0x00 }; break;
                //0x22 获取用户信息
                case 10:
                    userID = BitConverter.GetBytes(UserID);
                    bytesBuff = new byte[] { MID_GETUSERINFO, 0x00, 0x02, userID[1], userID[0] }; break;
                //0x23 清除录入状态
                case 11: bytesBuff = new byte[] { MID_FACERESET, 0x00, 0x00 }; break;
                //0x24 所有用户信息
                case 12: bytesBuff = new byte[] { MID_GET_ALL_USERID, 0x00, 0x00 }; break;
                //0x26 扩展录入
                case 13:break;
                default: break;
            }
            if (bytesBuff is null) return;
            byte parityCheck = UtilsHelper.PerformXOR(bytesBuff);
            bytes = bytes.Concat(bytesBuff).Append(parityCheck).ToArray();
            Gl.printLogColor(BitConverter.ToString(bytes).Replace("-", " "), blueBrush, null);
            SerialPort.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 串口回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            int byteToRead = Gl.MySerialPort.BytesToRead;
            byte[] buffer = new byte[byteToRead];

            int byteRead = Gl.MySerialPort.Read(buffer, 0, byteToRead);

            if (byteRead == 0) return;

            #region 数据分包传输，长度不对需拼接
            if (buffer.First() == 0xEF)
            {
                int byteSize = BitConverter.ToInt16(new byte[] { buffer[4], buffer[3] }, 0) + 5 + 1;
                if (buffer.Length != byteSize)
                {
                    dataReceivedBuff = dataReceivedBuff.Concat(buffer).ToArray();
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

            Gl.printLogColor(BitConverter.ToString(buffer).Replace('-', ' '), greenBrush, null);

            switch (buffer[2])
            {
                case 0x00: // MID_REPLY
                    Gl.printLog(UtilsHelper.GetEnumDescription<MID_REPLY_RES, byte>(buffer[6]));
                    if (buffer[6] != 0) return;
                    switch (buffer[5])
                    {
                        case 0x11: Gl.printLog(UtilsHelper.GetEnumDescription<ModuleState, byte>(buffer[6])); break;
                        case 0x12:
                            short id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                            string userName = Encoding.UTF8.GetString(buffer.Skip(9).Take(32).ToArray());
                            string isAdmin = buffer[41] == 0x00 ? "否" : "是";
                            string unlockStatu = buffer[42] == 0x00 ? "睁眼" : "闭眼";
                            Gl.printLog("已验证用户ID：" + id);
                            Gl.printLog("用户名字：" + userName);
                            Gl.printLog("是否为管理员：" + isAdmin);
                            Gl.printLog("解锁中眼状态：" + unlockStatu);
                            break;
                        case 0x13:
                            id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                            Gl.printLog("已验证用户ID：" + id);
                            Gl.printLog("人脸朝向：" + UtilsHelper.GetEnumDescription<FaceDir, byte>(buffer[9]));
                            break;
                        case 0x1D:
                            id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                            Gl.printLog("已验证用户ID：" + id);
                            Gl.printLog("人脸朝向：" + UtilsHelper.GetEnumDescription<FaceDir, byte>(buffer[9]));
                            break;
                        case 0x22:
                            id = BitConverter.ToInt16(new byte[] { buffer[8], buffer[7] }, 0);
                            userName = Encoding.UTF8.GetString(buffer.Skip(9).Take(32).ToArray());
                            isAdmin = buffer[41] == 0x00 ? "否" : "是";
                            Gl.printLog("已验证用户ID：" + id);
                            Gl.printLog("用户名字：" + userName);
                            Gl.printLog("是否为管理员：" + isAdmin);
                            break;
                        case 0x24:
                            int allUserCount = BitConverter.ToInt16(new byte[] { buffer[7], buffer[8] }, 0);
                            string userId = string.Empty;
                            for (int i = 0; i < allUserCount; i++)
                                userId += BitConverter.ToInt16(new byte[] { buffer[9 + 2 * i], buffer[8 + 2 * i] }, 0).ToString()+"  ";
                            Gl.printLog("已注册用户数量：" + allUserCount);
                            Gl.printLog($"分别是：[ {userId}]");
                            break;
                        default: break;
                    }
                    break;
                case 0x01: // MID_NOTE
                    try
                    {
                        Gl.printLog(UtilsHelper.GetEnumDescription<MID_NOTE_RES, byte>(buffer[6]));
                        Gl.printLog("人脸状态：" + UtilsHelper.GetEnumDescription<FaceState, short>(BitConverter.ToInt16(new byte[] { buffer[6], buffer[7] }, 0)));
                        Gl.printLog("距图片最左侧距离：" + BitConverter.ToInt16(new byte[] { buffer[8], buffer[9] }, 0));
                        Gl.printLog("距图片最上方距离：" + BitConverter.ToInt16(new byte[] { buffer[10], buffer[11] }, 0));
                        Gl.printLog("距图片最右方距离：" + BitConverter.ToInt16(new byte[] { buffer[12], buffer[13] }, 0));
                        Gl.printLog("距图片最下方距离：" + BitConverter.ToInt16(new byte[] { buffer[14], buffer[15] }, 0));
                        int yawNum = BitConverter.ToInt16(new byte[] { buffer[16], buffer[17] }, 0);
                        int pitchNum = BitConverter.ToInt16(new byte[] { buffer[18], buffer[19] }, 0);
                        int rollNum = BitConverter.ToInt16(new byte[] { buffer[20], buffer[21] }, 0);
                        string yaw = yawNum < 0 ? "左转头" : yawNum > 0 ? "右转头" : yawNum.ToString();
                        string pitch = pitchNum < 0 ? "上抬头" : pitchNum > 0 ? "下低头" : pitchNum.ToString();
                        string roll = rollNum < 0 ? "右歪头" : rollNum > 0 ? "左歪头" : rollNum.ToString();
                        Gl.printLog("yaw：" + yaw);
                        Gl.printLog("pitch：" + pitch);
                        Gl.printLog("roll：" + roll);
                    }
                    catch (Exception ex) { Gl.printLogColor(ex.Message, redBrush, null); }
                    break;
                case 0x02: // MID_IMAGE
                    break;
                default: break;
            }
        }
    }
}