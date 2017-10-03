using System.Data.Entity;

namespace FrameLog.Example
{
    public class ExampleContextInitializer : IDatabaseInitializer<ExampleContext>
    {
        private DropCreateDatabaseIfModelChanges<ExampleContext> wrapped;

        public ExampleContextInitializer()
        {
            wrapped = new DropCreateDatabaseIfModelChanges<ExampleContext>();
        }

        public void InitializeDatabase(ExampleContext context)
        {
            string databaseName = context.Database.Connection.Database;
            wrapped.InitializeDatabase(context);
            context.Database.ExecuteSqlCommand(
                TransactionalBehavior.DoNotEnsureTransaction,
                string.Format("ALTER DATABASE [{0}] SET READ_COMMITTED_SNAPSHOT ON", databaseName));
            context.Database.ExecuteSqlCommand(
                TransactionalBehavior.DoNotEnsureTransaction,
                string.Format("ALTER DATABASE [{0}] SET ALLOW_SNAPSHOT_ISOLATION ON", databaseName));
        }
    }
}
