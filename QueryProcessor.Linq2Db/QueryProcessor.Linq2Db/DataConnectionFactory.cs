using System;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;

namespace QueryProcessor.Linq2Db {
    public interface IDataConnectionFactory {
        DataConnection CreateConnection();
    }

    public class DataConnectionFactory : IDataConnectionFactory {
        private readonly Func<DataConnection> _func;

        public DataConnectionFactory(Func<DataConnection> func) {
            _func = func;
        }

        public DataConnection CreateConnection() {
            return _func();
        }
    }
    
    
  /*  public class DecisionSystemDataConnection : DataConnection {
        public DecisionSystemDataConnection(string connectionString)
            : base(new PostgreSQLDataProvider(), connectionString) {
            TurnTraceSwitchOn();
            WriteTraceLine = (message, displayName) => {
                Console.WriteLine($"{message} {displayName}");
            };
        }

    }*/
}