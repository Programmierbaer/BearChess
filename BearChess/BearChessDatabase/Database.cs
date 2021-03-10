using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.Pgn;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessDatabase
{
    public class Database : IDisposable
    {
        private readonly ILogging _logging;
        private SQLiteConnection _connection;
        private bool _dbExists;
        private bool _inError;

        public Database(ILogging logging, string fileName)
        {
            _logging = logging;
            FileName = fileName;
        }

        public string FileName { get; private set; }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        private void Load()
        {
            Load(FileName);
        }

        public void Load(string fileName)
        {
            FileName = fileName;
            if (string.IsNullOrWhiteSpace(FileName))
            {
                _logging?.LogError("Load with empty file name");
                _inError = true;
            }
            _dbExists = File.Exists(FileName);
            try
            {
                if (!_dbExists)
                {
                    SQLiteConnection.CreateFile(FileName);
                }

                _connection = new SQLiteConnection($"Data Source = {FileName}; Version = 3;");
                CreateTables();
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
                if (_connection == null)
                {
                    Load();
                    if (_inError)
                    {
                        return false;
                    }
                }
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
                _connection?.Dispose();
                File.Delete(FileName);
                SQLiteConnection.CreateFile(FileName);
                _connection = new SQLiteConnection($"Data Source = {FileName}; Version = 3;");
                _inError = false;
                CreateTables();
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
                              " event TEXT NOT NULL," +
                              " site TEXT NOT NULL," +
                              " result TEXT NOT NULL," +
                              " gameDate INTEGER NOT NULL, " +
                              " pgn TEXT NOT NULL," +
                              " pgnXML TEXT NOT NULL," +
                              " pgnHash INTEGER NOT NULL);";
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
                              " fen_id INTEGER NOT NULL," +
                              " game_id INTEGER NOT NULL," +
                              " moveNumber INTEGER NOT NULL);";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_fenToGames_fens_id ON fenToGames(fen_id);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_fenToGames_games_id ON fenToGames(game_id);";
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

            _connection.Open();
            var sqLiteTransaction = _connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                var deterministicHashCode = game.Pgn.GetDeterministicHashCode();
                var aSerializer = new XmlSerializer(typeof(DatabaseGame));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                aSerializer.Serialize(sw, game);
                var xmlResult = sw.GetStringBuilder().ToString();
                var sql = @"INSERT INTO games (white, black, event,site, result, gameDate, pgn, pgnXml, pgnHash)
                           VALUES (@white, @black, @event, @site,  @result, @gameDate, @pgn, @pgnXml, @pgnHash); ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@white", DbType.String).Value = game.White;
                    command2.Parameters.Add("@black", DbType.String).Value = game.Black;
                    command2.Parameters.Add("@event", DbType.String).Value = game.PgnGame.GameEvent;
                    command2.Parameters.Add("@site", DbType.String).Value = game.PgnGame.GameSite;
                    command2.Parameters.Add("@result", DbType.String).Value = game.Result;
                    command2.Parameters.Add("@gameDate", DbType.Int64).Value = game.GameDate.ToFileTime();
                    command2.Parameters.Add("@pgn", DbType.String).Value = game.PgnGame.MoveList;
                    command2.Parameters.Add("@pgnXml", DbType.String).Value = xmlResult;
                    command2.Parameters.Add("@pgnHash", DbType.Int32).Value = deterministicHashCode;
                    command2.ExecuteNonQuery();
                }

                var gameId = 0;
                using (var command2 = new SQLiteCommand("SELECT LAST_INSERT_ROWID();", _connection))
                {
                    var executeScalar = command2.ExecuteScalar();
                    gameId = int.Parse(executeScalar.ToString());
                }

                var moveNumber = 1;
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();

                foreach (var move in game.MoveList)
                {
                    chessBoard.MakeMove(move);
                    var fen = chessBoard.GetFenPosition();
                    var fenId = 0;
                    var shortFen = fen.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    sql = "SELECT id FROM fens WHERE shortFen=@shortFen;";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.Parameters.Add("@shortFen", DbType.String).Value = shortFen;
                        var executeScalar = command.ExecuteScalar(CommandBehavior.SingleRow);
                        if (executeScalar == null)
                        {
                            sql = @"INSERT INTO fens (shortFen, fullFen) VALUES (@shortFen, @fullFen); ";
                            using (var command2 = new SQLiteCommand(sql, _connection))
                            {
                                command2.Parameters.Add("@shortFen", DbType.String).Value = shortFen;
                                command2.Parameters.Add("@fullFen", DbType.String).Value = fen;
                                command2.ExecuteNonQuery();
                            }

                            using (var command2 = new SQLiteCommand("SELECT LAST_INSERT_ROWID();", _connection))
                            {
                                executeScalar = command2.ExecuteScalar();
                                fenId = int.Parse(executeScalar.ToString());
                            }
                        }
                        else
                        {
                            fenId = int.Parse(executeScalar.ToString());
                        }
                    }

                    sql =
                        @"INSERT INTO fenToGames (fen_id, game_id, moveNumber) VALUES (@fen_id, @game_id, @moveNumber); ";
                    using (var command2 = new SQLiteCommand(sql, _connection))
                    {
                        moveNumber++;
                        command2.Parameters.Add("@fen_id", DbType.Int32).Value = fenId;
                        command2.Parameters.Add("@game_id", DbType.Int32).Value = gameId;
                        command2.Parameters.Add("@moveNumber", DbType.Int32).Value = moveNumber / 2;
                        command2.ExecuteNonQuery();
                    }
                }

                sqLiteTransaction.Commit();
            }
            catch (Exception ex)
            {
                sqLiteTransaction.Rollback();
                _logging?.LogError(ex);
                return false;
            }
            finally
            {
                _connection.Close();
            }

            return true;
        }

        public void Delete(int id)
        {
            try
            {
                _connection.Open();
                var sql = @"DELETE FROM fenToGames  WHERE game_id=@game_id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@game_id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games  WHERE id=@id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public DatabaseGame Load(int id)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return null;
                }
            }

            DatabaseGame databaseGame = null;
            var pgnCreator = new PgnCreator();
            try
            {
                _connection.Open();
                var sql =
                    "SELECT id, white, black, event, site, result, gameDate, pgn, pgnXml, pgnHash FROM games WHERE id=@ID;";
                var xmlSerializer = new XmlSerializer(typeof(DatabaseGame));
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    cmd.Parameters.Add("@ID", DbType.Int32).Value = id;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            using (TextReader reader = new StringReader(rdr.GetString(8)))
                            {
                                databaseGame = (DatabaseGame) xmlSerializer.Deserialize(reader);
                                foreach (var databaseGameAllMove in databaseGame.AllMoves)
                                {
                                    pgnCreator.AddMove(databaseGameAllMove);
                                }

                                foreach (var move in pgnCreator.GetAllMoves(false, false, false))
                                {
                                    databaseGame.PgnGame.AddMove(move);
                                }
                            }
                        }

                        rdr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
            }

            _connection.Close();
            return databaseGame;
        }

        public DatabaseGameSimple[] GetGames()
        {
            if (_inError)
            {
                return new DatabaseGameSimple[0];
            }

            try
            {
                if (!_dbExists)
                {
                    Load();
                    if (!CreateTables())
                    {
                        return new DatabaseGameSimple[0];
                    }
                }

                return GetBySql(
                    "SELECT id, white, black, event, site, result, gameDate, pgn, pgnXml, pgnHash FROM games ORDER BY ID;");
            }
            catch (Exception ex)
            {
                _inError = true;
                _logging?.LogError(ex);
            }

            return new DatabaseGameSimple[0];
        }

        public DatabaseGameSimple[] FilterByFen(string fen)
        {
            if (_inError)
            {
                return new DatabaseGameSimple[0];
            }

            DatabaseGameSimple[] allGames = null;
            _connection.Open();
            using (var cmd = new SQLiteCommand(
                "SELECT g.id, g.white, g.black, g.event, g.site, g.result, g.gameDate, g.pgn, g.pgnXml, g.pgnHash" +
                " FROM games as g JOIN fenToGames as fg ON (g.ID=fg.game_id) " +
                " JOIN fens as f ON (fg.fen_id = f.id)" +
                "WHERE f.shortFen=@shortFen;", _connection))
            {
                cmd.Parameters.Add("@shortFen", DbType.String).Value = fen.Split(" ".ToCharArray())[0];
                using (var rdr = cmd.ExecuteReader())
                {
                    allGames = GetByReader(rdr);
                    rdr.Close();
                }
            }

            _connection.Close();
            return allGames;
        }

        private DatabaseGameSimple[] GetByReader(SQLiteDataReader rdr)
        {
            var allGames = new List<DatabaseGameSimple>();
            while (rdr.Read())
            {
                var databaseGameSimple = new DatabaseGameSimple
                                         {
                                             Id = rdr.GetInt32(0),
                                             White = rdr.GetString(1),
                                             Black = rdr.GetString(2),
                                             GameEvent = rdr.GetString(3),
                                             GameSite = rdr.GetString(4),
                                             Result = rdr.GetString(5),
                                             GameDate = DateTime.FromFileTime(rdr.GetInt64(6)),
                                             MoveList = rdr.GetString(7)
                                         };
                allGames.Add(databaseGameSimple);
            }

            return allGames.ToArray();
        }

        private DatabaseGameSimple[] GetBySql(string sql)
        {
            DatabaseGameSimple[] allGames = null;
            _connection.Open();
            using (var cmd = new SQLiteCommand(sql, _connection))
            {
                using (var rdr = cmd.ExecuteReader())
                {
                    allGames = GetByReader(rdr);
                    rdr.Close();
                }
            }

            _connection.Close();
            return allGames;
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