using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using QueryProcessor.Linq2Db;

namespace Sandbox {
    class Program {
        private const string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123";

        static async Task Main(string[] args) {
            var connectionFactory = new DataConnectionFactory((() => new DecisionSystemDataConnection(connectionString) {
            }));
            try {
                var type = typeof(Test);

                var requestProcessor = new RequestProcessor(connectionFactory);
                var result = await requestProcessor.Process<Test>(new RequestModel {
                    Filters = new List<Filter> {
                        new Filter {
                            Field = "Name",
                            Value = "Dima",
                            FilterType = FilterType.Contains
                        },
                        new Filter {
                            Field = "Id",
                            Value = "Dima",
                            FilterType = FilterType.Contains
                        },
                        new Filter {
                            Field = "Age",
                            Value = "3",
                            FilterType = FilterType.From
                        },
                        new Filter {
                            Field = "Age",
                            Value = "7",
                            FilterType = FilterType.To
                        }
                    },
                    Sort = new Sort {
                        Direction = Direction.Desc,
                        Field = "Id"
                    },
                    Paging = new Paging {
                        PageNumber = 1,
                        PageSize = 1
                    }
                });
            } catch (Exception exception) {
                Console.WriteLine(exception);
                throw;
            }
        }
        
    }
    
}