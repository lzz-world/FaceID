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

namespace Face.WPF.ViewModels
{
    public partial class UserViewModel : ObservableValidator
    {
        private string userName;
        private string account;
        private string department;
        private string position;
        private string pw;
        private string rePw;
        private CancellationTokenSource cts;
        private UserModel oldUserModel;

        public ObservableCollection<MyUserModel> Users { get; set; } = new ObservableCollection<MyUserModel>();

        [ObservableProperty] private int userRegisterTabIndex = 1;
        [ObservableProperty] private int authIndex = 3;
        [ObservableProperty] private int genderIndex = 0;
        [ObservableProperty] private string errorTips;
        [ObservableProperty] private int departmentIndex = 0;
        [ObservableProperty] private int? faseID;
        [ObservableProperty] private bool isRegisterSuccess = false;
        [ObservableProperty] private bool isRegisterProcess = false;
        [ObservableProperty] private string addText = "注册";

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
            List<UserModel>? allUser = DB.Fsql.Select<UserModel>().ToList();
            List<MyUserModel> myUsers = new List<MyUserModel>();
            foreach (var item in allUser)
            {
                item.Account = item.Account == null ? null : HashEncrypMD5.Md5Decrypt(item.Account, HashEncrypMD5.Key);
                myUsers.Add(new MyUserModel() { UserModel = item, IsCheck = false });
            }

            Users = new ObservableCollection<MyUserModel>(myUsers);
        }

        [RelayCommand]
        private void UserRegister()
        {
            Task.Run(() =>
            {
                #region 验证逻辑
                ErrorTips = string.Empty;
                IsRegisterSuccess = false;

                string errorMessage = UtilsHelper.ValidateProperty(this, "UserName");
                if (errorMessage != null)
                {
                    ErrorTips = "姓名：" + errorMessage;
                    return;
                }

                List<MyUserModel> filterInfo = Users.Where(w => w.UserModel.Name == UserName).ToList();
                if (filterInfo.Count > 0) { ErrorTips = "名字已注册"; return; }

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
                    if (filterInfo.Count > 0) { ErrorTips = "工号已注册"; return; }
                }
                else if (UserRegisterTabIndex == 0)
                {
                    if (Account != null)
                    {
                        errorMessage = UtilsHelper.ValidateProperty(this, "Account");
                        if (errorMessage != null) { ErrorTips = "工号：" + errorMessage; return; }
                        filterInfo = Users.Where(w => w.UserModel.Account == Account).ToList();
                        if (filterInfo.Count > 0) { ErrorTips = "工号已注册"; return; }
                    }
                }

                #endregion

                IsRegisterProcess = true;
                Task<bool> res = AddText switch
                {
                    "注册" => AddUserAsync(),
                    "更新" => UpdataUserAsync(oldUserModel),
                    _ => Task.FromResult(false)
                };

                IsRegisterProcess = false;
                if (res.Result)
                    IsRegisterSuccess = true;
                else
                    ErrorTips = "注册异常";
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
                if (!fTask.Result) return false;
                if (await Task.WhenAny(fTask, Task.Delay(Gl.scanFaceTimeOut * 1000)) != fTask)
                {
                    cts.Cancel();
                    ErrorTips = "面部扫描超时";
                    return false;
                }
                if (Gl.faceID == -2)
                {
                    ErrorTips = "面部重复录入";
                    return false;
                }

                FaseID = Gl.faceID;
                userModel = new UserModel()
                {
                    FaseId = (byte)Gl.faceID,
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
            return true;
        }

        private Task<bool> UpdataUserAsync(UserModel oldUserModel)
        {
            if (UserRegisterTabIndex == 1)
            {

            }
            else
            {

            }
            return Task.FromResult(true);
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

        [RelayCommand] private void RegisterTabSelectionChanged() { ErrorTips = string.Empty; IsRegisterSuccess = false; }

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
            AddText = "更新";

            Account = userModel.Account ?? string.Empty;
            string[] auth = Enum.GetNames(typeof(UserType));
            AuthIndex = Array.IndexOf(auth, userModel.Auth.ToString());
            string[] department = Enum.GetNames(typeof(DepartmentType));
            DepartmentIndex = Array.IndexOf(department, userModel.Department.ToString());
            string[] gender = Enum.GetNames(typeof(GenderType));
            GenderIndex = Array.IndexOf(gender, userModel.Gender.ToString());
            FaseID = userModel.FaseId;
            UserName = userModel.Name;
            Position = userModel.Position ?? string.Empty;
            Pw = userModel.Pssword ?? string.Empty;
            RePw = userModel.Pssword ?? string.Empty;
        }

        public partial class MyUserModel : ObservableObject
        {
            public UserModel UserModel { get; set; }

            [ObservableProperty]
            private bool isCheck = false;
        }
    }
}
