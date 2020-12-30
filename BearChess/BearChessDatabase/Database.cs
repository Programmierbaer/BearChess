using System;
using System.Data.SQLite;
using System.IO;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    public class Database  : IDisposable
    {
        private readonly string _fileName;
        private readonly ILogging _logging;
        private SQLiteConnection _connection;
        private bool _dbExists;
        private bool _inError;

        public Database(string pathName, ILogging logging)
        {
            _fileName = Path.Combine(pathName, "bearchess.db");
            _logging = logging;
            _dbExists = File.Exists(_fileName);
            try
            {
                if (!_dbExists)
                {
                    SQLiteConnection.CreateFile(_fileName);
                }

                _connection = new SQLiteConnection($"Data Source = {_fileName}; Version = 3;");
                _inError = false;
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
                _inError = true;
            }
        }

        public bool Open()
        {
            try
            {
                _connection.Open();
                _inError = false;
                return true;

            }
            catch (Exception ex)
            {
                _inError = true;
                _logging?.LogError(ex);
            }

            return false;
        }

        public void Close()
        {
            _connection.Close();
            _inError = false;
        }


        public void Drop()
        {
            Close();
            try
            {
                File.Delete(_fileName);
                SQLiteConnection.CreateFile(_fileName);
                _connection?.Dispose();
                _connection = new SQLiteConnection($"Data Source = {_fileName}; Version = 3;");
                _inError = false;
            }
            catch (Exception ex)
            {
                _inError = true;
                _logging?.LogError(ex);
            }
        }

        public bool CreateTables()
        {
            if (_inError || !Open())
            {
                return false;
            }
            try
            {
                if (!TableExists("storageVersion"))
                {
                    var sql = "CREATE TABLE storageVersion " +
                              "(version INTEGER NOT NULL);";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                       command.ExecuteNonQuery();
                       command.CommandText = "INSERT INTO storageVersion (version) VALUES (1);";
                       command.ExecuteNonQuery();
                    }
                }
                if (!TableExists("games"))
                {
                    var sql = "CREATE TABLE games " +
                              "(id INTEGER PRIMARY KEY," +
                              " white TEXT NOT NULL," +
                              " black TEXT NOT NULL," +
                              " result INTEGER NOT NULL," +
                              " gameDate INTEGER NOT NULL, " +
                              " pgn TEXT NOT NULL," +
                              " pgnHash TEXT NOT NULL);";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_games_white ON games(white);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_games_black ON games(black);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_games_pgnHash ON games(pgnHash);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_games_date ON games(gameDate);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
                if (!TableExists("fens"))
                {
                    var sql = "CREATE TABLE fens " +
                              "(id INTEGER PRIMARY KEY," +
                              " shortFen TEXT NOT NULL," +
                              " fullFen TEXT NOT NULL);";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_fens_shortFen ON fens(shortFen);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
                if (!TableExists("fenToGames"))
                {
                    var sql = "CREATE TABLE fenToGames " +
                              "(id INTEGER PRIMARY KEY," +
                              " fens_id INTEGER NOT NULL," +
                              " games_id INTEGER NOT NULL," +
                              " moveNumber INTEGER NOT NULL);";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_fenToGames_fens_id ON fenToGames(fens_id);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_fenToGames_games_id ON fenToGames(games_id);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
                _dbExists = true;
                Close();
                return true;
            }
            catch (Exception ex)
            {
                _inError = true;
                _logging?.LogError(ex);
            }
            return false;
        }

        public bool Save(DatabaseGame game)
        {
            if (_inError)
            {
                return false;
            }
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return false;
                }
            }

            var deterministicHashCode = game.Pgn.GetDeterministicHashCode();
            return true;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        private bool TableExists(string tableName)
        {
            var sql = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{tableName}' COLLATE NOCASE";
            using (var command = new SQLiteCommand(sql, _connection))
            {
                var executeScalar = command.ExecuteScalar();
                return executeScalar != null;
            }
        }
    }
}
