using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face.WPF.Models
{
    public class UserModel
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public GenderType Gender { get; set; }

        [Column(IsNullable = true)]
        public DepartmentType Department { get; set; }

        [Column(IsNullable = true)]
        public string? Position { get; set; }

        [Column(IsNullable = true)]
        public string? Account { get; set; }

        [Column(IsNullable = true)]
        public string? Pssword { get; set; }

        [Column(IsNullable = true)]
        public byte? FaseId { get; set; }

        [Column(IsNullable = true)]
        public string? Image { get; set; }

        public UserType Auth { get; set; }

        [Column(ServerTime = DateTimeKind.Utc, CanUpdate = false)]
        public DateTime CreateTime { get; set; }

        [Column(ServerTime = DateTimeKind.Utc)]
        public DateTime UpdateTime { get; set; }
    }

    public enum UserType
    {
        [Description("超级管理员")] Root,
        [Description("管理员")] Admin,
        [Description("操作员")] Operator,
        [Description("维护员")] Maintain,
        [Description("访客")] Visitor
    }

    public enum GenderType
    {
        [Description("男")] Men,
        [Description("女")] Women
    }

    public enum DepartmentType
    {
        [Description("生产一部")] ProduceOne,
        [Description("生产二部")] ProduceTwo,
        [Description("生产三部")] ProduceThree,
    }
}
