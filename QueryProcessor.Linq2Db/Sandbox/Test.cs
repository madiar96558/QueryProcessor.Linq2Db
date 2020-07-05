using LinqToDB.Mapping;

namespace Sandbox {
    [Table(Schema = "public", Name = "test")]
    public class Test {
        [Column("id"), Identity, PrimaryKey]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("age")]
        public int Age { get; set; }
    }
}