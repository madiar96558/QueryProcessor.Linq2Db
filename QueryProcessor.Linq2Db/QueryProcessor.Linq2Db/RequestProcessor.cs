using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace QueryProcessor.Linq2Db {
    public class RequestProcessor {
        private readonly IDataConnectionFactory _dataConnectionFactory;

        private readonly int _pageSizeLimit;

        public RequestProcessor(IDataConnectionFactory dataConnectionFactory, int? pageSizeLimit = null) {
            _dataConnectionFactory = dataConnectionFactory;
            _pageSizeLimit = pageSizeLimit ?? 1000;
        }

        public async Task<List<TEntity>> Process<TEntity>(RequestModel requestModel) where TEntity : class {
            var entityType = typeof(TEntity);
            List<PropertyInfo> fields = entityType.GetProperties().Where(e => Attribute.IsDefined(e, typeof(PrimaryKeyAttribute))).ToList();
            if (fields.Count == 0) {
                throw new ArgumentException("POCO class should have PrimaryKey attribute");
            }

            if (fields.Count > 1) {
                throw new ArgumentException("The library doesn't support composite key. Use only 1 PrimaryKey attribute in POCO class");
            }

            if (requestModel.Paging == null) {
                throw new ArgumentException("Paging mustn't be null");
            }

            if (requestModel.Paging.PageSize > _pageSizeLimit || requestModel.Paging.PageSize < 1) {
                throw new ArgumentException("Incorrect page size");
            }

            PropertyInfo primaryKey = fields.First();

            var keys = await GetKeys<TEntity>(requestModel, primaryKey);

            int skipCount = requestModel.Paging.PageNumber * requestModel.Paging.PageSize;
            var dynamicIds = keys.Skip(skipCount).Take(requestModel.Paging.PageSize).ToList();
            Type type = primaryKey.PropertyType;
            Type listType = typeof(List<>).MakeGenericType(type);
            IList resultIds = (IList) Activator.CreateInstance(listType);
            foreach (var id in dynamicIds) {
                resultIds.Add(id);
            }

            using (DataConnection connection = _dataConnectionFactory.CreateConnection()) {
                var table = connection.GetTable<TEntity>();
                IQueryable<TEntity> resultQuery = table.Where($"@0.Contains({primaryKey.Name})", resultIds);
                return await resultQuery.ToListAsync();
            }
        }

        private async Task<List<object>> GetKeys<TEntity>(RequestModel requestModel, PropertyInfo primaryKey) where TEntity : class {
            using (DataConnection connection = _dataConnectionFactory.CreateConnection()) {
                var table = connection.GetTable<TEntity>();
                IQueryable<TEntity> query = table.AsQueryable();
                if (requestModel.Filters != null) {
                    foreach (var filter in requestModel.Filters) {
                        PropertyInfo property = typeof(TEntity).GetProperty(filter.Field);
                        if (property == null) {
                            throw new ArgumentException($"Field with name {filter.Field} not exist in POCO class");
                        }

                        switch (filter.FilterType) {
                            case FilterType.Contains:
                                if (property.PropertyType != typeof(string)) {
                                    continue;
                                }

                                query = query.Where($"{filter.Field}.Contains(@0)", filter.Value);
                                break;
                            case FilterType.From:
                                query = query.Where($"{filter.Field} > @0", filter.Value);
                                break;
                            case FilterType.To:
                                query = query.Where($"{filter.Field} < @0", filter.Value);
                                break;
                            case FilterType.Equals:
                                query = query.Where($"{filter.Field} == @0", filter.Value);
                                break;
                        }
                    }
                }

                if (requestModel.Sort != null) {
                    PropertyInfo property = typeof(TEntity).GetProperty(requestModel.Sort.Field);
                    if (property == null) {
                        throw new ArgumentException($"Field with name {requestModel.Sort.Field} doesn't exist in POCO class");
                    }

                    switch (requestModel.Sort.Direction) {
                        case Direction.Asc:
                            query = query.OrderBy($"{requestModel.Sort.Field} asc");
                            break;
                        case Direction.Desc:
                            query = query.OrderBy($"{requestModel.Sort.Field} desc");
                            break;
                    }
                }

                IQueryable selectQuery = query.Select($"{primaryKey.Name}");

                return await selectQuery.ToDynamicListAsync();
            }
        }
    }
}