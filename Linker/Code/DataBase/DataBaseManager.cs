using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;
using Microsoft.Data.Sqlite;
using Serilog.Events;
using Windows.Storage;
using Windows.UI.Xaml;
using System.Linq;
using Linker.Code.Behaviours;
using System.ComponentModel;
using Linker.Nodes;
using System.Collections;
using Linker.Code.Buddys;
using System.Collections.Specialized;

namespace Linker.Code.DataBase
{
    /// <summary>
    /// Singlleton patern
    /// </summary>
    public sealed class DatabaseManager
    {
        public event EventHandler<ValuesUpdateEventArgs> OnDatabaseValuesUpdated;
        public static string DatabaseName { get; } = "MeasureDate.db";

        private string connectionString;
        private static readonly DatabaseManager uniqueInstanceEager = new DatabaseManager();
        StringBuilder pathstringBuilder = new StringBuilder();
        StringBuilder wherestringBuilder = new StringBuilder();

        
        private DatabaseManager()
        { }


        public static DatabaseManager Instance
        {
            get { return uniqueInstanceEager; }
        }


        public async Task InitializeDatabaseAsync()
        {
            var result = ApplicationData.Current.LocalFolder.CreateFileAsync(DatabaseName, CreationCollisionOption.OpenIfExists).AsTask();
            connectionString = $"Filename={Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseName)}";
            await result;
            try
            {
                using (SqliteConnection db = new SqliteConnection(connectionString))
                {
                    await db.OpenAsync();
                    SqliteCommand BuffelCommand = new SqliteCommand("CREATE TABLE IF NOT EXISTS BuffelData (Primary_Key INTEGER PRIMARY KEY, DateTime TEXT, Send_Succesfull INTEGER)", db);
                    await BuffelCommand.ExecuteReaderAsync();
                }
            }
            catch (Exception ex)
            {
                LogBuddy.Log(this, "Failed to Initialize BuffelData database reason:" + ex.Message);
            }
        }



        /// <summary>
        /// Add new values to the database
        /// </summary>
        /// <param name="columnNames">column names where to add values</param>
        /// <param name="newValues">The values to add</param>
        public void AddNewValues(string[] columnNames, object[] newValues)
        {
            string names = string.Empty;
            foreach (string name in columnNames)
                names = string.Concat(names, $"'{name}',");
            names = names.TrimEnd(',');

            string values = string.Empty;
            foreach (var value in newValues)
                values = string.Concat(values, $"'{value}',");
            values = values.TrimEnd(',');

            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string query = $"insert into BuffelData ('DateTime',{names}) values ('{dateTime}',{values})";
            
            try
            {
                using (SqliteConnection db = new SqliteConnection(connectionString))
                {
                    db.Open();
                    SqliteCommand command = new SqliteCommand(query, db);
                    command.ExecuteReader();
                    db.Close();

                    OnDatabaseValuesUpdated?.Invoke(this, new ValuesUpdateEventArgs(columnNames, newValues));
                }
            }
            catch (Exception ex)
            {
                LogBuddy.Log(this, "Failed to update values database reason:" + ex.Message);
            }
        }

        private string[] nameArray;
        private string sqlCommand;
        private PropertyObserveCollection<MeasureNode> oldObservable;
        private bool observatableCollectionChanged;

        public void InsertData(PropertyObserveCollection<MeasureNode> observable)
        {
            if (observable == null)
                return;

            bool changeDetected = observatableCollectionChanged;
            if (oldObservable != observable)
            {
                observable.CollectionChanged += Observable_CollectionChanged; ;
                if (oldObservable != null)                
                    oldObservable.CollectionChanged -= Observable_CollectionChanged;
                oldObservable = observable;
                changeDetected = true;
            }
            if(changeDetected)
            {
                nameArray = observable.Select(conv => conv.Behaviour.DataBaseRecord).ToArray();
                CreateNonExsistingColumns(nameArray);
                sqlCommand = CreatSqliteCommand(nameArray);
            }

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction())
                {
                    using (SqliteCommand insertCommand = connection.CreateCommand())
                    {
                        insertCommand.CommandText = sqlCommand;
                        insertCommand.Parameters.AddWithValue("$DateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

                        for (int i = 0; i < observable.Count; i++)
                            insertCommand.Parameters.AddWithValue(string.Concat("$", observable[i].Behaviour.DataBaseRecord), observable[i].Behaviour.Value);
                                                    
                        try
                        {
                            insertCommand.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            LogBuddy.Log(this, LogEventLevel.Error, $"Insert data failed {ex.Message}");
                        }
                    }
                }
            }
            OnDatabaseValuesUpdated?.Invoke(this, new ValuesUpdateEventArgs(nameArray, observable));
        }

        

        private string CreatSqliteCommand(string[] nameArray)
        {
            string names = string.Empty;
            string values = string.Empty;

            foreach (var value in nameArray)
            {
                names = string.Concat(names, $"{value},");
                values = string.Concat(values, $"${value},");
            }
            names = names.TrimEnd(',');
            values = values.TrimEnd(',');

            return $"INSERT INTO BuffelData(DateTime,{names}) VALUES($DateTime,{values});";
        }




        private void Observable_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            observatableCollectionChanged = true;
        }

        public void CreateNonExsistingColumns(string[] columnNames)
        {
            if (columnNames == null)
                return;

            var nonExsistingColumns = GetNonExsistingColumns(columnNames);

            if (nonExsistingColumns.Count > 0)
            {                
                using (SqliteConnection db = new SqliteConnection(connectionString))
                {
                    string command;
                    db.Open();                        
                    foreach (string columnname in nonExsistingColumns)
                    {
                        command = $"ALTER TABLE BuffelData ADD '{columnname}' nvarchar(20) null"; 
                        SqliteCommand BuffelCommand = new SqliteCommand(command, db);
                        BuffelCommand.ExecuteReader();
                    }
                    db.Close();
                }
            }
        }



      


        public async Task<List<object[]>> LoadLastNrOfRecordsAsync(int rowAmount)
        {
            var returnList = new List<object[]>();
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                //SqliteCommand command = new SqliteCommand($"SELECT * FROM BuffelData ORDER BY Primary_Key DESC LIMIT {rowAmount}", connection);  //DESC 
                // limit 10 OFFSET (select count(*) from BuffelData) -15
                SqliteCommand command = new SqliteCommand($"select * FROM BuffelData LIMIT {rowAmount} OFFSET(select count(*) from BuffelData) -{rowAmount}", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    object[] returnItems;
                    while (await reader.ReadAsync())
                    {
                        returnItems = new object[reader.VisibleFieldCount];
                        reader.GetValues(returnItems);
                        returnList.Add(returnItems);
                        //items.Add(new Item() { Name = reader.GetString(nameOrdinal), Description = reader.GetString(descriptionOrdinal) });
                    }
                }
            }
            return returnList;
        }

        

        /// <summary>
        /// returns the last values recorded of the specified paths
        /// </summary>
        /// <param name="paths">Returns * if paths is null</param>
        /// <returns></returns>
        public async Task<List<object[]>> LoadLastRecords(string[] paths, int rowAmount)
        {
            List<object[]> returnItems = new List<object[]>();
            object[] itemRow;           

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                var openTask = connection.OpenAsync().ConfigureAwait(true);
                SqliteCommand command = new SqliteCommand($"SELECT {ConcatedPathString(paths)} FROM BuffelData LIMIT {rowAmount} OFFSET(select count(*) from BuffelData) -{rowAmount}");
                await openTask;
                command.Connection = connection;
                var reader = command.ExecuteReader();                

                while (reader.Read())
                {
                    itemRow = new object[reader.VisibleFieldCount];
                    reader.GetValues(itemRow);
                    returnItems.Add(itemRow);
                }
                reader.Dispose();
                command.Dispose();
            }
            return returnItems;
        }


        public async void WriteSendResult(int[] primaryKeys, int result)
        {
            if (primaryKeys == null)
                return;

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                var openTask = connection.OpenAsync().ConfigureAwait(false);
                SqliteCommand command = new SqliteCommand($"UPDATE BuffelData SET [Send_Succesfull] = {result} WHERE {ConcatedPrimaryKeyString(primaryKeys)}");
                await openTask;
                command.Connection = connection;
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }

        /// <summary>
        /// Concantate the primary keys in a string
        /// </summary>
        private string ConcatedPrimaryKeyString(int[] primaryKeys)
        {
            wherestringBuilder.Clear();

            foreach (int key in primaryKeys)
            {
                wherestringBuilder.Append(" or Primary_Key= ");
                wherestringBuilder.Append(key);
            }
                
            wherestringBuilder.Remove(0, 4);

            return wherestringBuilder.ToString();
        }


        /// <summary>
        /// returns the last values recorded of the specified paths where the condition is met
        /// </summary>
        /// <param name="paths">Returns * if paths is null</param>
        /// <returns></returns>
        public async Task<List<object[]>> ConditionalLoadLastRecords(string[] paths, int rowAmount, string where, WhereCondition condition)
        {
            List<object[]> returnItems = new List<object[]>();
            object[] itemRow;
            
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                var openTask = connection.OpenAsync().ConfigureAwait(true);
                SqliteCommand command = new SqliteCommand($"SELECT {ConcatedPathString(paths)} FROM BuffelData {CreateConditionString(condition, where)} LIMIT {rowAmount}");
                await openTask;
                command.Connection = connection;
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    itemRow = new object[reader.VisibleFieldCount];
                    reader.GetValues(itemRow);
                    returnItems.Add(itemRow);
                }
                reader.Dispose();
                command.Dispose();
            }
            return returnItems;
        }

        private string CreateConditionString(WhereCondition condition, string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            string returnResult = $"where [{path}] "; 

            switch (condition)
            {
                case WhereCondition.IsNull:
                    return string.Concat(returnResult, "IS NULL");
                case WhereCondition.IsNotNull:
                    return string.Concat(returnResult, "IS NOT NULL");
                case WhereCondition.IsTrue:
                    return string.Concat(returnResult, ">= 1");
                case WhereCondition.NotIs200:
                    return string.Concat(returnResult, "IS NULL or [",path , "] != 200");
                default:
                    return string.Empty;
            }
        }

       


        /// <summary>
        /// returns the last values recorded of the specified paths
        /// </summary>
        /// <param name="paths">Returns * if paths is null</param>
        /// <returns></returns>
        public async Task<object[]> LoadLastRecord(string[] paths)
        {
            object[] itemRow = null;

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                var openTask = connection.OpenAsync().ConfigureAwait(true);
                SqliteCommand command = new SqliteCommand($"SELECT {ConcatedPathString(paths)} FROM[BuffelData] WHERE[Primary_Key] = (SELECT max([Primary_Key]) FROM[BuffelData])");
                await openTask;
                command.Connection = connection;
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    itemRow = new object[reader.VisibleFieldCount];
                    reader.GetValues(itemRow);
                }
                reader.Dispose();
                command.Dispose();
            }
            return itemRow;
            // return with yield ?
        }

        

        /// <summary>
        /// Concats the string paths like [path1}, [path2], if paths are null select all symbol * is returned 
        /// </summary>
        /// <param name="paths">the paths to concencate</param>
        /// <returns>the list concat in [path1}, [path2]</returns>
        private string ConcatedPathString(string[] paths)
        {            
            if (paths == null)
                return "*";

            pathstringBuilder.Clear();
            pathstringBuilder.Append("[");
            for (int i = 0; i < paths.Length; i++)
            {
                pathstringBuilder.Append(paths[i]);
                pathstringBuilder.Append("],[");
            }
            if(pathstringBuilder.Length > 1)
                pathstringBuilder.Remove(pathstringBuilder.Length-2, 2);
            return pathstringBuilder.ToString();         
        }




        /// <summary>
        /// Returns the names that do not excist in the database based on the specified names
        /// </summary>
        /// <param name="columnNames"></param>
        /// <returns>a list with all the items that do not excist</returns>
        private List<string> GetNonExsistingColumns(string[] columnNames)
        {
            List<string> nonExsistingColumns = new List<string>();
            List<string> excistingColumns = GetColumnNames();
            foreach (string columnname in columnNames)
                if(!excistingColumns.Contains(columnname))
                    nonExsistingColumns.Add(columnname);
            return nonExsistingColumns;
        }


        /// <summary>
        /// Returns all column names of BuffelData
        /// </summary>
        /// <returns>a list with all the column names</returns>
        public List<string> GetColumnNames()
        {
            var nameList = new List<string>();
            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA table_info(BuffelData)";

                var reader = cmd.ExecuteReader();
                int nameIndex = reader.GetOrdinal("name");
                while (reader.Read())
                    if (!reader.IsDBNull(nameIndex))
                        nameList.Add(reader.GetString(nameIndex));
                
                conn.Close();
            }
            return nameList;
        }


        //private bool CheckIfColumnExists(string columnName, string tableName)
        //{
        //    using (var conn = new SqliteConnection(connectionString))
        //    {
        //        conn.Open();
        //        var cmd = conn.CreateCommand();
        //        cmd.CommandText = string.Format("PRAGMA table_info({0})", tableName);

        //        var reader = cmd.ExecuteReader();
        //        int nameIndex = reader.GetOrdinal("name");
        //        while (reader.Read())
        //        {
        //            if (!reader.IsDBNull(nameIndex))
        //            {
        //                string value = reader.GetString(nameIndex);
        //                if (value.Equals(columnName))
        //                {
        //                    conn.Close();
        //                    return true;
        //                }

        //            }
        //        }
        //        conn.Close();
        //    }
        //    return false;
        //}


        /// <summary>
        /// Excecutes a custom command, null if there was an error
        /// </summary>
        public SqliteDataReader GiveSqlCommand(string commandQuery)
        {
            try
            {
                using (SqliteConnection db = new SqliteConnection(connectionString))
                {
                    db.Open();

                    SqliteCommand command = new SqliteCommand(commandQuery, db);

                    return command.ExecuteReader();
                }
            }
            catch(Exception ex)
            {
                LogBuddy.Log(this, LogEventLevel.Error, ex, "GiveSqlCommand failed, reason" + ex.Message);
            }
            return null;
        }
    }

    public enum WhereCondition
    {
        IsNull,
        IsNotNull,
        IsTrue,
        NotIs200
    }

    ///// <summary>
    ///// updates if there was a database insert
    ///// </summary>
    //public class ValuesUpdateEventArgs : EventArgs
    //{
    //    public string[] Columns { get; private set; }
    //    public object[] Values { get; private set; }
    //    public ValuesUpdateEventArgs(string[] columnNames, object[] newValues)
    //    {
    //        Columns = columnNames;
    //        Values = newValues;
    //    }
    //}


    /// <summary>
    /// updates if there was a database insert
    /// </summary>
    public class ValuesUpdateEventArgs : EventArgs
    {
        public string[] Columns { get; private set; }
        public IList Values { get; private set; }
        public ValuesUpdateEventArgs(string[] columnNames, IList newValues)
        {
            Columns = columnNames;
            Values = newValues;
        }
    }


    
}
