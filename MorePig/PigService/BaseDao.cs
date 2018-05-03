using MySql.Data.MySqlClient;
using PigAccount;
using SharpDapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PigService
{
    public class BaseDao
    {
        protected IDapperConnection Database { get; private set; }
        public BaseDao()
        {
            string connectionString = AccountConfigUtils.sqlConfig;
            var connection = new MySqlConnection(connectionString);
            Database = new DapperConnection(connection);
        }
    }
}
