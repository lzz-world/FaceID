using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face.WPF.Utils
{
    public class DB
    {
        public static IFreeSql Fsql => fsql.Value;

        private static Lazy<IFreeSql> fsql = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
          .UseMonitorCommand(cmd => Trace.WriteLine($"Sql：{cmd.CommandText}"))
          .UseConnectionString(FreeSql.DataType.Sqlite, @"Data Source=./Database/freedb.db")
          .UseAutoSyncStructure(true)
          .Build());
    }
}
