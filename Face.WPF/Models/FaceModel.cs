using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace Face.WPF.Models
{
    interface IGetBytes
    {
        public byte[] GetBytes();
    }

    public class FaceModel
    {
        private const int USER_NAME_SIZE = 32;

        //MID
        public static byte MID_REPLY = 0x00;
        public static byte MID_NOTE = 0x01;
        public static byte MID_IMAGE = 0x02;
        public static byte MID_RESET = 0x10;
        public static byte MID_GETSTATUS = 0x11;
        public static byte MID_VERIFY = 0x12;
        public static byte MID_ENROLL = 0x13;
        public static byte MID_SNAPIMAGE = 0x16;
        public static byte MID_GETSAVEDIMAGE = 0x17;
        public static byte MID_UPLOADIMAGE = 0x18;
        public static byte MID_ENROLL_SINGLE = 0x1D;
        public static byte MID_DELUSER = 0x20;
        public static byte MID_DELALL = 0x21;
        public static byte MID_GETUSERINFO = 0x22;
        public static byte MID_FACERESET = 0x23;
        public static byte MID_GET_ALL_USERID = 0x24;
        public static byte MID_ENROLL_ITG = 0x26;
        public static byte MID_GET_VERSION = 0x30;
        public static byte MID_START_OTA = 0x40;
        public static byte MID_STOP_OTA = 0x41;
        public static byte MID_GET_OTA_STATUS = 0x42;
        public static byte MID_OTA_HEADER = 0x43;
        public static byte MID_OTA_PACKET = 0x44;
        public static byte MID_INIT_ENCRYPTION = 0x50;
        public static byte MID_CONFIG_BAUDRATE = 0x51;
        public static byte MID_SET_RELEASE_ENC_KEY = 0x52;
        public static byte MID_SET_DEBUG_ENC_KEY = 0x52;
        public static byte MID_GET_LOGFILE = 0x60;
        public static byte MID_UPLOAD_LOGFILE = 0x61;
        public static byte MID_SET_THRESHOLD_LEVEL = 0xD4;
        public static byte MID_POWERDOWN = 0xED;
        public static byte MID_DEBUG_MODE = 0xF0;
        public static byte MID_GET_DEBUG_INFO = 0xF1;
        public static byte MID_UPLOAD_DEBUG_INFO = 0xF2;
        public static byte MID_GETLIBRARY_VERSION = 0xF3;
        public static byte MID_DEMOMODE = 0xFE;

        // Face Dirc
        public enum FaceDir : byte
        {
            [Description("未定义")]
            FACE_DIRECTION_UNDEFINE = 0x00,
            [Description("面中部")]
            FACE_DIRECTION_MIDDLE = 0x01,
            [Description("面朝右")]
            FACE_DIRECTION_RIGHT = 0x02,
            [Description("面朝左")]
            FACE_DIRECTION_LEFT = 0x04,
            [Description("面朝下")]
            FACE_DIRECTION_DOWN = 0x08,
            [Description("面朝上")]
            FACE_DIRECTION_UP = 0x10
        }

        // MID_REPLY ResultC
        public enum MID_REPLY_RES : byte
        {
            [Description("成功")]
            MR_SUCCESS = 0,
            [Description("模组拒绝该命令")]
            MR_REJECTED = 1,
            [Description("录入/解锁算法已终止")]
            MR_ABORTED = 2,
            [Description("相机打开失败")]
            MR_FAILED4_CAMERA = 4,
            [Description("未知错误")]
            MR_FAILED4_UNKNOWNREASON = 5,
            [Description("无效的参数")]
            MR_FAILED4_INVALIDPARAM = 6,
            [Description("内存不足")]
            MR_FAILED4_NOMEMORY = 7,
            [Description("没有已录入的用户")]
            MR_FAILED4_UNKNOWNUSER = 8,
            [Description("录入超过最大用户数量")]
            MR_FAILED4_MAXUSER = 9,
            [Description("人脸已录入")]
            MR_FAILED4_FACEENROLLED = 10,
            [Description("活体检测失败")]
            MR_FAILED4_LIVENESSCHECK = 12,
            [Description("录入或解锁超时")]
            MR_FAILED4_TIMEOUT = 13,
            [Description("加密芯片授权失败")]
            MR_FAILED4_AUTHORIZATION = 14,
            [Description("读文件失败")]
            MR_FAILED4_READ_FILE = 19,
            [Description("写文件失败")]
            MR_FAILED4_WRITE_FILE = 20,
            [Description("通信协议未加密")]
            MR_FAILED4_NO_ENCRYPT = 21,
            [Description("RGB 图像没有 ready")]
            MR_FAILED4_NO_RGBIMAGE = 23
        }

        // Module State
        public enum ModuleState : byte
        {
            [Description("模组处于空闲状态，等待主控命令")]
            MS_STANDBY = 0,
            [Description("模组处于工作状态")]
            MS_BUSY = 1,
            [Description("模组出错，不能正常工作")]
            MS_ERROR = 2,
            [Description("模组未进行初始化")]
            MS_INVALID = 3,
            [Description("模组在OTA模式")]
            MS_OTA = 4
        }


        //MID_NOTE Result
        public enum MID_NOTE_RES : byte
        {
            [Description("模组已准备好")]
            NID_READY = 0,
            [Description("算法执行成功，并且返回人脸信息")]
            NID_FACE_STATE = 1,
            [Description("未知错误")]
            NID_UNKNOWNERROR = 2,
            [Description("OTA 升级完毕")]
            NID_OTA_DONE = 3,
            [Description("解锁过程中睁闭眼状态")]
            NID_EYE_STATE = 4
        }

        // FaceSate
        public enum FaceState : short
        {
            [Description("人脸正常")]
            FACE_STATE_NORMAL = 0,
            [Description("未检测到人脸")]
            FACE_STATE_NOFACE = 1,
            [Description("人脸太靠近图片上边沿，未能录入")]
            FACE_STATE_TOOUP = 2,
            [Description("人脸太靠近图片下边沿，未能录入")]
            FACE_STATE_TOODOWN = 3,
            [Description("人脸太靠近图片左边沿，未能录入")]
            FACE_STATE_TOOLEFT = 4,
            [Description("人脸太靠近图片右边沿，未能录入")]
            FACE_STATE_TOORIGHT = 5,
            [Description("人脸距离太远，未能录入")]
            FACE_STATE_FAR = 6,
            [Description("人脸距离太近，未能录入")]
            FACE_STATE_CLOSE = 7,
            [Description("眉毛遮挡")]
            FACE_STATE_EYEBROW_OCCLUSION = 8,
            [Description("眼睛遮挡")]
            FACE_STATE_EYE_OCCLUSION = 9,
            [Description("脸部遮挡")]
            FACE_STATE_FACE_OCCLUSION = 10,
            [Description("录入人脸方向错误")]
            FACE_STATE_DIRECTION_ERROR = 11,
            [Description("在闭眼模式检测到睁眼状态")]
            FACE_STATE_EYE_CLOSE_STATUS_OPEN_EYE = 12,
            [Description("闭眼状态")]
            FACE_STATE_EYE_CLOSE_STATUS = 13,
            [Description("在闭眼模式检测中无法判定睁闭眼状态")]
            FACE_STATE_EYE_CLOSE_UNKNOW_STATUS = 14,
        }

        public class Msg_enroll_data : IGetBytes
        {
            private readonly byte _admin;
            private readonly byte[] _user_name;
            private readonly byte _s_face_dir;
            private readonly byte _timeout;

            public Msg_enroll_data(byte admin, byte[] user_name, byte s_face_dir, byte timeout)
            {
                if (user_name.Length != USER_NAME_SIZE)
                    throw new ArgumentException("user_name array must be of length 32", nameof(user_name));
                (_admin, _user_name, _s_face_dir, _timeout) = (admin, user_name, s_face_dir, timeout);
            }

            public byte[] GetBytes() => new byte[] { }
                .Append(_admin)
                .Concat(_user_name)
                .Append(_s_face_dir)
                .Append(_timeout)
                .ToArray();
        }
    }
}
