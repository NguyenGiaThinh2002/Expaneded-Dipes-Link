using SharedProgram.Shared;
using SQLitePCL;
using SQLite;
namespace RelationalDatabaseHelper.SQLite
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique]
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
    }
    public class CreateDefaultUser
    {

        public static async void Init()
        {
            //Create Account Database Directory
            var dbPath = (SharedPaths.PathAccountsDb + "\\AccountDB.db");
            if (!File.Exists(dbPath))
            {
                Directory.CreateDirectory(SharedPaths.PathAccountsDb);
                var databasePath = Path.Combine(SharedPaths.PathAccountsDb, "AccountDB.db");
                SQLiteConnectionString options = new(databasePath, true, key: "nmc@0971340629");
                SQLiteAsyncConnection db = new(options);
                await db.CreateTableAsync<User>();
                User defaultUser = new()
                {
                    username = "Admin",
                    password = "Admin@1234",
                    role = "Administrator",
                };

                await db.RunInTransactionAsync(tran => {
                    // database calls inside the transaction
                    tran.Insert(defaultUser);

                });
            }
        }


        private static void Transaction(SQLiteConnectionWithLock db, object data)
        {
            db.BeginTransaction();
            try
            {
                var t = db.Insert(data);
                db.Commit();

            }
            catch (Exception)
            {
                db.Rollback();
            }
        }
    }
}
