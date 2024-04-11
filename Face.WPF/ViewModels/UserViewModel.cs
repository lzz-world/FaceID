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
using HandyControl.Controls;
using MessageBox = HandyControl.Controls.MessageBox;
using HandyControl.Tools.Extension;
using static Face.WPF.ViewModels.UserViewModel;
using System.Security.Policy;
using static Face.WPF.Models.FaceModel;
using System.Diagnostics.Eventing.Reader;
using System.ComponentModel;
using System.Windows;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using Face.WPF.Views.DialogView;

namespace Face.WPF.ViewModels
{
    public partial class UserViewModel : ObservableValidator
    {
        private const string ADD_USER = "注册";
        private const string USER_UPDATA = "更新";
        private string userName;
        private string account;
        private string position;
        private string pw;
        private string rePw;

        private CancellationTokenSource cts;
        private UserModel oldUserModel;

        public ObservableCollection<MyUserModel> Users { get; set; } = new ObservableCollection<MyUserModel>();

        [ObservableProperty] private int authIndex = 3;
        [ObservableProperty] private int genderIndex = 0;
        [ObservableProperty] private string errorTips;
        [ObservableProperty] private int departmentIndex = 0;
        [ObservableProperty] private int? faseID;
        [ObservableProperty] private bool isRegisterSuccess = false;
        [ObservableProperty] private bool isRegisterProcess = false;
        [ObservableProperty] private int userRegisterTabIndex = 1;
        [ObservableProperty] private string addText = ADD_USER;

        [Required(ErrorMessage = "请输入名字")]
        [MinLength(2, ErrorMessage = "最少2位")]
        [MaxLength(5, ErrorMessage = "最多5位")]
        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value, true);
        }

        [Required(ErrorMessage = "请输入工号")]
        [MinLength(6, ErrorMessage = "最少6位")]
        [MaxLength(11, ErrorMessage = "最多11位")]
        [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "只能输入A-Z 0-9组合")]
        public string Account
        {
            get => account;
            set => SetProperty(ref account, value, true);
        }

        [MaxLength(10, ErrorMessage = "最多10位")]
        public string Position
        {
            get => position;
            set => SetProperty(ref position, value, true);
        }

        [Required(ErrorMessage = "请输入密码")]
        [MinLength(6, ErrorMessage = "最少6位")]
        [MaxLength(18, ErrorMessage = "最多18位")]
        [RegularExpression(@"^[a-zA-Z\d!@#$%^&*()-_+=\[{\]};:'"",<.>/?\\|`~]*$", ErrorMessage = "只能输入大小写字符与符号")]
        public string Pw
        {
            get => pw;
            set => SetProperty(ref pw, value, true);
        }

        [Required(ErrorMessage = "请再次输入密码")]
        [MinLength(6, ErrorMessage = "最少6位")]
        [MaxLength(18, ErrorMessage = "最多18位")]
        [RegularExpression(@"^[a-zA-Z\d!@#$%^&*()-_+=\[{\]};:'"",<.>/?\\|`~]*$", ErrorMessage = "只能输入大小写字符与符号")]
        public string RePw
        {
            get => rePw;
            set => SetProperty(ref rePw, value, true);
        }

        public UserViewModel()
        {
            Gl.RefreshUserInfos = RefreshUserInfos;
            RefreshUserInfos();
        }

        private void RefreshUserInfos()
        {
            Users.Clear();
            List<UserModel>? allUser = DB.Fsql.Select<UserModel>().ToList();
            foreach (var item in allUser)
            {
                item.Account = item.Account == null ? null : HashEncrypMD5.Md5Decrypt(item.Account, HashEncrypMD5.Key);
                Users.Add(new MyUserModel() { UserModel = item, IsCheck = false });
            }
        }

        [RelayCommand]
        private void UserRegister()
        {
            Task.Run(async () =>
            {
                #region 验证逻辑
                ErrorTips = string.Empty;
                IsRegisterSuccess = false;

                string errorMessage = UtilsHelper.ValidateProperty(this, "UserName");
                if (errorMessage != null)
                {
                    ErrorTips = "姓名：" + errorMessage; return;
                }

                List<MyUserModel> filterInfo = Users.Where(w => w.UserModel.Name == UserName).ToList();
                if (filterInfo.Count > 0 && AddText == ADD_USER)
                {
                    ErrorTips = "名字已注册"; return;
                }
                else if (filterInfo.Count > 0 && filterInfo.First().UserModel.Id != oldUserModel.Id && AddText == USER_UPDATA)
                {
                    ErrorTips = "名字已注册"; return;
                }

                if (UserRegisterTabIndex == 1)
                {
                    errorMessage = UtilsHelper.ValidateProperty(this, "Account");
                    if (errorMessage != null) { ErrorTips = "工号：" + errorMessage; return; }
                    errorMessage = UtilsHelper.ValidateProperty(this, "Pw");
                    if (errorMessage != null) { ErrorTips = "登录密码：" + errorMessage; return; }
                    errorMessage = UtilsHelper.ValidateProperty(this, "RePw");
                    if (errorMessage != null) { ErrorTips = "确认密码：" + errorMessage; return; }
                    if (RePw != Pw) { ErrorTips = "密码不一致"; return; }

                    filterInfo = Users.Where(w => w.UserModel.Account == Account).ToList();
                    if (filterInfo.Count > 0 && AddText == ADD_USER) { ErrorTips = "工号已注册"; return; }
                    else if (filterInfo.Count > 0 && filterInfo.First().UserModel.Id != oldUserModel.Id && AddText == USER_UPDATA)
                    { ErrorTips = "工号已注册"; return; }
                }
                else if (UserRegisterTabIndex == 0)
                {
                    if (!string.IsNullOrWhiteSpace(Account))
                    {
                        errorMessage = UtilsHelper.ValidateProperty(this, "Account");
                        if (errorMessage != null) { ErrorTips = "工号：" + errorMessage; return; }
                        filterInfo = Users.Where(w => w.UserModel.Account == Account).ToList();
                        if (filterInfo.Count > 0 && AddText == ADD_USER) { ErrorTips = "工号已注册"; return; }
                        else if (filterInfo.Count > 0 && filterInfo.First().UserModel.Id != oldUserModel.Id && AddText == USER_UPDATA)
                        { ErrorTips = "工号已注册"; return; }
                    }
                }

                #endregion

                IsRegisterProcess = true;
                Task<bool> res = AddText switch
                {
                    ADD_USER => AddUserAsync(),
                    USER_UPDATA => UpdataUserAsync(oldUserModel),
                    _ => Task.FromResult(false)
                };

                await res;
                IsRegisterProcess = false;
                if (res.Result)
                    IsRegisterSuccess = true;
            });
        }

        private async Task<bool> AddUserAsync()
        {
            UserModel userModel;
            if (UserRegisterTabIndex == 1)
            {
                userModel = new UserModel()
                {
                    Name = UserName,
                    Gender = GenderIndex == 0 ? GenderType.Men : GenderType.Women,
                    Account = HashEncrypMD5.Md5Encrypt_Key(Account, HashEncrypMD5.Key),
                    Department = DepartmentIndex == 0 ? DepartmentType.ProduceOne :
                                 DepartmentIndex == 1 ? DepartmentType.ProduceTwo : DepartmentType.ProduceThree,
                    Position = Position,
                    Auth = AuthIndex == 0 ? UserType.Admin :
                           AuthIndex == 1 ? UserType.Operator :
                           AuthIndex == 2 ? UserType.Maintain : UserType.Visitor,
                    Pssword = HashEncrypMD5.Md5Encrypt_Key(Pw, HashEncrypMD5.Key),
                    CreateTime = DateTime.Now,
                };
            }
            else
            {
                cts = new CancellationTokenSource();
                Task<bool> fTask = Task.Run(async () =>
                {
                    if (Gl.faseState == 0) { ErrorTips = "请检查串口连接"; return false; }
                    Gl.SerialWrite(13);
                    while (!cts.IsCancellationRequested)
                    {
                        if (Gl.faceID > -1 || Gl.faceID < -1)
                            return true;
                        await Task.Delay(100);
                    }
                    return false;
                });
                if (await Task.WhenAny(fTask, Task.Delay(Gl.scanFaceTimeOut * 1000)) != fTask)
                {
                    cts.Cancel();
                    ErrorTips = "面部扫描超时";
                    return false;
                }
                if (!fTask.Result) return false;
                if (Gl.faceID == -1)
                {
                    ErrorTips = UtilsHelper.GetEnumDescription<MID_REPLY_RES, byte>(Gl.faseReplyRes);
                    return false;
                }
                else if (Gl.faceID == -2)
                {
                    ErrorTips = "人脸已重复录入";
                    return false;
                }

                FaseID = Gl.faceID;

                //图像数据为空，防止数据库无数据
                while (MainModel.imageBytes == null)
                {
                    Gl.isCapture = true;
                    await Task.Delay(500); 
                }

                userModel = new UserModel()
                {
                    FaseId = (byte)Gl.faceID,
                    Image = MainModel.imageBytes,
                    Name = UserName,
                    Gender = GenderIndex == 0 ? GenderType.Men : GenderType.Women,
                    Account = Account == null ? null : HashEncrypMD5.Md5Encrypt_Key(Account, HashEncrypMD5.Key),
                    Department = DepartmentIndex == 0 ? DepartmentType.ProduceOne :
                                 DepartmentIndex == 1 ? DepartmentType.ProduceTwo : DepartmentType.ProduceThree,
                    Position = Position,
                    Auth = AuthIndex == 0 ? UserType.Admin :
                           AuthIndex == 1 ? UserType.Operator :
                           AuthIndex == 2 ? UserType.Maintain : UserType.Visitor,
                    CreateTime = DateTime.Now,
                };
            }
            DB.Fsql.Insert(userModel).ExecuteAffrows();

            int userMax = Users.Count == 0 ? 0 : Users.Max(m => m.UserModel.Id);
            userModel.Id = userMax + 1;
            userModel.Account = Account;
            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Add(new MyUserModel() { UserModel = userModel, IsCheck = false });
            });
            MainModel.imageBytes = null;
            return true;
        }

        private async Task<bool> UpdataUserAsync(UserModel oldUserModel)
        {
            string AccountEncrypMD5 = oldUserModel.Account ?? string.Empty;
            string PwEncrypMD5 = oldUserModel.Pssword ?? string.Empty;

            UserType auth = AuthIndex == 0 ? UserType.Admin :
                            AuthIndex == 1 ? UserType.Operator :
                            AuthIndex == 2 ? UserType.Maintain : UserType.Visitor;
            DepartmentType department = DepartmentIndex == 0 ? DepartmentType.ProduceOne :
                                        DepartmentIndex == 1 ? DepartmentType.ProduceTwo : DepartmentType.ProduceThree;
            GenderType gender = GenderIndex == 0 ? GenderType.Men : GenderType.Women;
            AccountEncrypMD5 = HashEncrypMD5.Md5Encrypt_Key(Account, HashEncrypMD5.Key);

            if (UserRegisterTabIndex == 1)
            {
                PwEncrypMD5 = HashEncrypMD5.Md5Encrypt_Key(Pw, HashEncrypMD5.Key);
            }
            else
            {
                /*
                 1.先判断面部是否录入，如果没有录入过则立即更新
                 2.如果已录入再判断是不是当前的面部ID对应的人像
                 3.如果对应则更新，反之不更新
                 */
                cts = new CancellationTokenSource();
                // 人脸识别
                Task<bool> fTask = Task.Run(async () =>
                {
                    if (Gl.faseState == 0) { ErrorTips = "请检查串口连接"; return false; }
                    Gl.SerialWrite(2);
                    while (!cts.IsCancellationRequested)
                    {
                        if (Gl.faceID > -1 || Gl.faceID < -1)
                            return true;
                        await Task.Delay(100);
                    }
                    return false;
                });
                if (await Task.WhenAny(fTask, Task.Delay(Gl.scanFaceTimeOut * 1000)) != fTask)
                {
                    cts.Cancel();
                    ErrorTips = "面部扫描超时";
                    return false;
                }
                if (!fTask.Result) return false;
                if (Gl.faceID == -1)
                {
                    ErrorTips = UtilsHelper.GetEnumDescription<MID_REPLY_RES, byte>(Gl.faseReplyRes);
                    return false;
                }
                else if (Gl.faceID == -2)
                {

                }
                else if (Gl.faceID != oldUserModel.FaseId)
                {
                    var us = Users.Where(w => w.UserModel.FaseId == Gl.faceID).FirstOrDefault();
                    ErrorTips = $"你已录入为其他角色[{us.UserModel.Name}]";
                    return false;
                }

                // 人脸更新
                fTask = Task.Run(async () =>
                {
                    if (Gl.faseState == 0) { ErrorTips = "请检查串口连接"; return false; }
                    if (oldUserModel.FaseId != null)
                    {
                        Gl.faceUserID = (int)oldUserModel!.FaseId;
                        Gl.SerialWrite(8);
                        DB.Fsql.Update<UserModel>().Set(s => s.FaseId, null).Where(w => w.FaseId == oldUserModel.FaseId);
                    }
                    Gl.SerialWrite(13);
                    while (!cts.IsCancellationRequested)
                    {
                        if (Gl.faceID > -1)
                            return true;
                        await Task.Delay(100);
                    }
                    return false;
                });
                if (await Task.WhenAny(fTask, Task.Delay(Gl.scanFaceTimeOut * 1000)) != fTask)
                {
                    cts.Cancel();
                    ErrorTips = "面部扫描超时";
                    return false;
                }
                if (!fTask.Result) return false;
                if (Gl.faceID == -1)
                {
                    ErrorTips = UtilsHelper.GetEnumDescription<MID_REPLY_RES, byte>(Gl.faseReplyRes);
                    return false;
                }

                FaseID = Gl.faceID;
                //图像数据为空，防止数据库无数据
                while (MainModel.imageBytes == null)
                {
                    Gl.isCapture = true;
                    await Task.Delay(500);
                }
            }

            var upRow = DB.Fsql.Update<UserModel>()
                .Set(s => s.Account, HashEncrypMD5.Md5Encrypt_Key(Account, HashEncrypMD5.Key))
                .Set(s => s.Auth, auth)
                .Set(s => s.FaseId, (byte?)FaseID)
                .Set(s => s.Image, MainModel.imageBytes)
                .Set(s => s.Department, department)
                .Set(s => s.Gender, gender)
                .Set(s => s.Name, UserName)
                .Set(s => s.Position, Position)
                .Set(s => s.Pssword, HashEncrypMD5.Md5Encrypt_Key(Pw, HashEncrypMD5.Key))
                .Where(w => w.Name == oldUserModel.Name)
                .ExecuteAffrows();
            if (upRow != 1)
            {
                ErrorTips = "数据库更新0行";
                return false;
            }

            var listIndex = Users.IndexOf(Users.Where(w => w.UserModel == oldUserModel).First());
            MyUserModel myUserModel = new MyUserModel()
            {
                IsCheck = false,
                UserModel = new UserModel()
                {
                    FaseId = (byte?)FaseID,
                    Account = Account,
                    Auth = auth,
                    Department = department,
                    Gender = gender,
                    Name = UserName,
                    Position = Position,
                    Pssword = HashEncrypMD5.Md5Encrypt_Key(Pw, HashEncrypMD5.Key),
                    Id = oldUserModel.Id,
                    CreateTime = oldUserModel.CreateTime,
                    Image = MainModel.imageBytes,
                    UpdateTime = oldUserModel.UpdateTime
                }
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.RemoveAt(listIndex);
                Users.Insert(listIndex, myUserModel);
            });
            this.oldUserModel = myUserModel.UserModel;
            MainModel.imageBytes = null;
            return true;
        }

        [RelayCommand]
        private void UserSelected(string index)
        {
            switch (index)
            {
                case "All":
                    foreach (var item in Users)
                        item.IsCheck = true;
                    break;
                case "Invert":
                    foreach (var item in Users)
                        item.IsCheck = item.IsCheck ? false : true;
                    break;
                default: break;
            }
        }

        [RelayCommand]
        private void UserDelected()
        {
            var userIsChecks = Users.Where(w => w.IsCheck);
            if (userIsChecks.Count() == 0)
            {
                MessageBox.Show("请选择删除项目", string.Empty, MessageBoxButton.OK, MessageBoxImage.Question);
            }
            {
                for (int i = Users.Count - 1; i >= 0; i--)
                {
                    if (Users[i].IsCheck)
                    {
                        int row = 0;
                        if (Users[i].UserModel.FaseId != null)
                        {
                            Gl.faceUserID = (int)Users[i].UserModel.FaseId;
                            Gl.SerialWrite(8);
                            row = DB.Fsql.Delete<UserModel>().Where(w => w.FaseId == Gl.faceUserID).ExecuteAffrows();
                        }
                        else
                            row = DB.Fsql.Delete<UserModel>().Where(w => w.Name == Users[i].UserModel.Name).ExecuteAffrows();

                        if (row != 1)
                        {
                            MessageBox.Show("数据删除失败");
                            return;
                        }

                        Users.RemoveAt(i);
                    }
                }
            }
        }

        [RelayCommand]
        private void RegisterTabSelectionChanged()
        {
            ErrorTips = string.Empty;
            cts?.Cancel();
            IsRegisterSuccess = false;
        }

        [RelayCommand]
        private void UserAddTab()
        {
            UserRegisterTabIndex = 1;
            AddText = "注册";
            IsRegisterProcess = false;
            IsRegisterSuccess = false;
            ErrorTips = string.Empty;

            Account = string.Empty;
            AuthIndex = 4;
            DepartmentIndex = 0;
            GenderIndex = 0;
            FaseID = null;
            UserName = string.Empty;
            Position = string.Empty;
            Pw = string.Empty;
            RePw = string.Empty;
        }

        [RelayCommand]
        private void UserEdit(UserModel userModel)
        {
            oldUserModel = userModel;
            UserRegisterTabIndex = 1;
            ErrorTips = string.Empty;
            IsRegisterSuccess = false;
            AddText = "更新";

            Account = userModel.Account ?? string.Empty;
            string[] auth = Enum.GetNames(typeof(UserType));
            AuthIndex = Array.IndexOf(auth, userModel.Auth.ToString()) - 1;
            string[] department = Enum.GetNames(typeof(DepartmentType));
            DepartmentIndex = Array.IndexOf(department, userModel.Department.ToString());
            string[] gender = Enum.GetNames(typeof(GenderType));
            GenderIndex = Array.IndexOf(gender, userModel.Gender.ToString());
            FaseID = userModel.FaseId;
            UserName = userModel.Name;
            Position = userModel.Position ?? string.Empty;
            Pw = userModel.Pssword != null ? HashEncrypMD5.Md5Decrypt(userModel.Pssword, HashEncrypMD5.Key) : string.Empty;
            RePw = this.Pw;
        }

        [RelayCommand]
        private void ImageShow(byte[] imageBytes)
        {
            var imageSource = ImageHelper.ConvertToBitmapImage(imageBytes);
            new ImageDialogView(imageSource)
            {
                Owner = App.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Topmost = App.Current.MainWindow.Topmost
            }.ShowDialog();
        }
    }

    public class MyUserModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public UserModel UserModel { get; set; }

        private bool isCheck = false;
        public bool IsCheck
        {
            get => isCheck;
            set
            {
                isCheck = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCheck)));
            }
        }
    }
}
