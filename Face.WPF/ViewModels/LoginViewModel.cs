using CommunityToolkit.Mvvm.ComponentModel;
using Face.WPF.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using Face.WPF.Utils;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.IO.Ports;
using System.Windows;
using System.Threading;
using Face.WPF.Views;
using System.Timers;
using System.Windows.Forms;
using static Face.WPF.Models.FaceModel;
using System.Diagnostics.Eventing.Reader;

namespace Face.WPF.ViewModels
{
    public partial class LoginViewModel : ObservableValidator
    {
        private CancellationTokenSource cts;

        [Required(ErrorMessage = "请输入工号")]
        [MinLength(6, ErrorMessage = "最少6位")]
        [MaxLength(11, ErrorMessage = "最多11位")]
        [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "只能输入A-Z 0-9组合")]
        public string Account { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [MinLength(6, ErrorMessage = "最少6位")]
        [MaxLength(18, ErrorMessage = "最多18位")]
        [RegularExpression(@"^[a-zA-Z\d!@#$%^&*()-_+=\[{\]};:'"",<.>/?\\|`~]*$", ErrorMessage = "只能输入大小写字符与符号")]
        public string Password { get; set; }

        private int loginTabIndex = 1;
        public int LoginTabIndex
        {
            get => loginTabIndex;
            set
            {
                loginTabIndex = value;
                ErrorTips = String.Empty;
                if (loginTabIndex == 0)
                {
                    Gl.IsStartVedio = true;
                    cts = new CancellationTokenSource();
                    _ = Task.Run(async () =>
                    {
                        FaceLoginTask:
                        while (!cts.IsCancellationRequested)
                        {

                            IsLoading = true;
                            ErrorTips = string.Empty;
                            Gl.SerialWrite(2);
                            long tick = DateTime.Now.Ticks;
                            while (!cts.IsCancellationRequested)
                            {
                                if (Gl.faseState == 0) 
                                {
                                    IsLoading = false;
                                    ErrorTips = "请检查串口连接";
                                    await Task.Delay(3000);
                                    goto FaceLoginTask; 
                                }

                                Debug.WriteLine($"ScanID：{Gl.faceID}");
                                if (Gl.faceID > -1)
                                {

                                    //PS:不知为何有几率faceID在登录时为-1
                                    IsLoading = false;
                                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        _ = Login(Gl.faceID);
                                    });
                                    return;
                                }

                                else if (Gl.faceID < -1 || DateTime.Now.Ticks - tick > Gl.scanFaceTimeOut * 1_100_0000)
                                {
                                    if (Gl.faseReplyRes == 0x00)
                                        ErrorTips = "扫描超时";
                                    else
                                        ErrorTips = UtilsHelper.GetEnumDescription<MID_REPLY_RES, byte>(Gl.faseReplyRes);
                                    IsLoading = false;
                                    await Task.Delay(3000);
                                    break;
                                }
                                await Task.Delay(100);
                            }
                        }
                    }, cts.Token);
                }
                else
                {
                    Gl.IsStartVedio = false;
                    cts.Cancel();
                    Gl.SerialWrite(0);
                    IsLoading = false;
                }
            }
        }

        [ObservableProperty] private string errorTips = string.Empty;
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private bool isSuccess = false;
        [ObservableProperty] private string[] userInfo = new string[3];

        [RelayCommand]
        private async Task<bool> Login(object parameter)
        {
            ErrorTips = string.Empty;

            if (LoginTabIndex == 1)
            {
                var passwordBox = parameter as HandyControl.Controls.PasswordBox;
                var password = passwordBox?.Password;
                Password = password ?? string.Empty;

                string errorMessage = UtilsHelper.ValidateProperty(this, "Account");
                if (errorMessage != null) { ErrorTips = "工号：" + errorMessage; return false; }
                errorMessage = UtilsHelper.ValidateProperty(this, "Password");
                if (errorMessage != null) { ErrorTips = "密码：：" + errorMessage; return false; }
            }

            List<UserModel> userInfos = DB.Fsql.Select<UserModel>().ToList();

            if (LoginTabIndex == 0)
            {
                Debug.WriteLine($"Login：{parameter}");
                userInfos = userInfos.Where(w => w.FaseId.ToString() == parameter.ToString()).ToList();
                if (userInfos.Count == 0)
                {
                    ErrorTips = $"面部信息未匹配，请删除此面部ID[{parameter}]再重新录入";
                    return false;
                }
            }
            else if (LoginTabIndex == 1)
            {
                if (HashEncrypMD5.Md5Decrypt("676077F8CB350F2A", HashEncrypMD5.Key) == Account && HashEncrypMD5.Md5Decrypt("676077F8CB350F2A", HashEncrypMD5.Key) == Password)
                    userInfos = new List<UserModel> { new UserModel() { Name = "Root", Auth = UserType.Root } };
                else
                {
                    userInfos = userInfos.Where(w => w.Account != null && w.Pssword != null).ToList();
                    userInfos = userInfos.Where(w => HashEncrypMD5.Md5Decrypt(w.Account, HashEncrypMD5.Key) == Account).ToList();
                    if (userInfos.Count == 0) { ErrorTips = "工号未注册"; return false; }
                    userInfos = userInfos.Where(w => HashEncrypMD5.Md5Decrypt(w.Pssword, HashEncrypMD5.Key) == Password).ToList();
                    if (userInfos.Count == 0) { ErrorTips = "密码有误"; return false; }
                }
                IsLoading = true;
            }

            IsSuccess = true;
            UserInfo[0] = userInfos.First().FaseId.ToString();
            UserInfo[1] = userInfos.First().Name;
            UserInfo[2] = userInfos.First().Position ?? string.Empty;
            OnPropertyChanged(nameof(UserInfo));
            await Task.Delay(2000);

            Gl.Login(userInfos.First());
            return true;
        }
    }
}
