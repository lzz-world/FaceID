using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face.WPF.Models
{
    class UserModel
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public GenderType Gender { get; set; }
        public DepartmentType Department { get; set; }
        public string? Position {  get; set; }
        public string Account { get; set; }
        public string Pssword { get; set; }
        public byte FaseId { get; set; }
        public string? Image { get; set; }
        public UserType Auth {  get; set; }
    }

    enum UserType{
        [Description("超级管理员")] Root,
        [Description("管理员")] Admin,
        [Description("操作员")] User,
        [Description("维护员")] Maintain,
        [Description("访客")] Visitor
    }

    enum GenderType{ 
        [Description("男")] Men, 
        [Description("女")] Women 
    }

    enum DepartmentType
    {
        [Description("生产一部")] ProduceOne,
        [Description("生产二部")] ProduceTwo,
        [Description("生产三部")] ProduceThree,
    }
}
