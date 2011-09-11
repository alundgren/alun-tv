using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Diagnostics;

namespace WebUi.Infrastructure
{
    public class SqlTraceWriterListener : TraceListener
    {
        private readonly string _connectionString;

        public SqlTraceWriterListener(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override bool IsThreadSafe
        {
            get
            {
                return true;
            }
        }

        public override void Write(string message)
        {
            WriteToLog(message??"");
        }

        public override void Write(object o) 
        {
            string msg = o != null ? o.ToString() : null;
            Write(msg);
        }

        public override void Write(string message, string category) 
        {
            Write(String.Format("{0}:{1}", message, category));
        }

        public override void Write(object o, string category)
        {
            Write(String.Format("{0}:{1}", o, category));
        }

        public override void WriteLine(object o)
        {
            Write(o);
        }

        public override void WriteLine(object o, string category)
        {
            Write(o, category);
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void WriteLine(string message, string category)
        {
            Write(message, category);
        }

        private bool _initDone = false;
        private object _lock = new object();

        public void WriteToLog(string message)
        {
            const string createTablCmd = 
                @"
                    IF OBJECT_ID('dbo.Errors', 'U') IS NULL
                    CREATE TABLE dbo.Errors
                    (
                        Id int identity not null,
                        CreationDate datetime not null default(getdate()),
                        ErrorMessage nvarchar(max) not null
                        CONSTRAINT PK_Errors PRIMARY KEY (Id)
                    )
                ";
            if (!_initDone)
            {
                lock (_lock)
                {
                    if (!_initDone)
                    {
                        WithCmd(cmd => 
                        {
                            cmd.CommandText = createTablCmd;
                            cmd.ExecuteNonQuery();
                        });
                        _initDone = true;
                    }
                }
            }
            const string logCommand = @"insert into Errors(ErrorMessage)values(@msg)";
            WithCmd(cmd => 
            {
                cmd.CommandText = logCommand;
                cmd.Parameters.AddWithValue("@msg", message);
                cmd.ExecuteNonQuery();
            });
        }

        private void WithCmd(Action<SqlCommand> a)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    a(cmd);
                }
            }
        }
    }
}