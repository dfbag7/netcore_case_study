using FluentMigrator;

namespace ScanApp.Migrations
{
    [Migration(202205311230, "Database Creation")]
    public class Migration_20220531_1230 : Migration
    {
        public override void Down()
        {
            Delete.Table("hashes");
        }

        public override void Up()
        {
            Create.Table("hashes")
                .WithColumn("id").AsInt32().PrimaryKey().Identity()
                .WithColumn("md5").AsBinary().NotNullable()
                .WithColumn("sha1").AsBinary().NotNullable()
                .WithColumn("sha256").AsBinary().NotNullable().Unique()
                .WithColumn("file_size").AsInt64().NotNullable()
                .WithColumn("scanned").AsInt32().NotNullable()
                .WithColumn("last_seen").AsDateTime2().NotNullable();
        }
    }
}
