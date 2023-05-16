using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;
using PgnCreator = www.SoLaNoSoft.com.BearChessBase.Implementations.pgn.PgnCreator;

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

        private void LoadDb()
        {
            LoadDb(FileName);
        }

        public void LoadDb(string fileName)
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
                    LoadDb();
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

        public string Backup()
        {
            string target = string.Empty;
            try
            {
                target = $"{FileName}.bak_{DateTime.UtcNow.ToFileTime()}";
                Close();
                _connection = null;
                File.Copy(FileName, target);
                LoadDb();
                
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
                LoadDb();
                target = $"Error: {ex.Message}";
            }

            return target;
        }

        public string Restore(string backupFileName)
        {
            try
            {
                if (File.Exists(backupFileName))
                {
                    Close();
                    _connection = null;
                    File.Copy(backupFileName, FileName, true);
                    LoadDb();
                    return string.Empty;
                }

                return $"File{Environment.NewLine}{backupFileName}{Environment.NewLine} not found";
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
                LoadDb();
                return $"Error: {ex.Message}";
            }
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
                int storageVersion = 1;
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
                else
                {
                    storageVersion = GetStorageVersion();
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

                if (!TableExists("tournament"))
                {
                    var sql = "CREATE TABLE tournament " +
                              "(id INTEGER PRIMARY KEY," +
                              " event TEXT NOT NULL," +
                              " eventDate INTEGER NOT NULL, " +
                              " gamesToPlay INTEGER NOT NULL," +
                              " configXML TEXT NOT NULL );";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                if (!TableExists("tournamentGames"))
                {
                    var sql = "CREATE TABLE tournamentGames " +
                              "(id INTEGER PRIMARY KEY," +
                              " tournament_id INTEGER NOT NULL, " +
                              " game_id INTEGER NOT NULL);";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_tournamentGames_tournId ON tournamentGames(tournament_id);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX idx_tournamentGames_gameId ON tournamentGames(game_id);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }

                if (!TableExists("duel"))
                {
                    var sql = "CREATE TABLE duel " +
                              "(id INTEGER PRIMARY KEY," +
                              " event TEXT NOT NULL," +
                              " eventDate INTEGER NOT NULL, " +
                              " gamesToPlay INTEGER NOT NULL," +
                              " configXML TEXT NOT NULL );";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                if (!TableExists("duelGames"))
                {
                    var sql = "CREATE TABLE duelGames " +
                              "(id INTEGER PRIMARY KEY," +
                              " duel_id INTEGER NOT NULL, " +
                              " game_id INTEGER NOT NULL);";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX duelGames_duelId ON duelGames(duel_id);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                        sql = "CREATE INDEX duelGames_gamesId ON duelGames(game_id);";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }

                if (storageVersion == 1)
                {
                    var sql = "ALTER TABLE games ADD COLUMN round INTEGER DEFAULT 1; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();

                    }
                    sql = "ALTER TABLE games ADD COLUMN white_elo text DEFAULT ''; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();

                    }
                    sql = "ALTER TABLE games ADD COLUMN black_elo text DEFAULT ''; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();

                    }
                    sql = "UPDATE storageVersion SET version=3; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();

                    }
                }
                if (storageVersion == 2)
                {
                    var sql = "ALTER TABLE games ADD COLUMN white_elo text DEFAULT ''; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();

                    }
                    sql = "ALTER TABLE games ADD COLUMN black_elo text DEFAULT ''; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();

                    }
                    sql = "UPDATE storageVersion SET version=3; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.ExecuteNonQuery();

                    }
                }
                if (storageVersion == 3)
                {
                    Close();
                    var databaseGameSimples = GetGames(new GamesFilter() { FilterIsActive = false });
                    foreach (var databaseGameSimple in databaseGameSimples)
                    {
                        Save(LoadGame(databaseGameSimple.Id, false), true);
                    }

                    Open();
                    string sql = "UPDATE storageVersion SET version=4; ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
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

        private bool TableExists(string tableName)
        {
            var sql = $"SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{tableName}' COLLATE NOCASE";
            using (var command = new SQLiteCommand(sql, _connection))
            {
                var executeScalar = command.ExecuteScalar();
                return executeScalar != null;
            }
        }

        private int GetStorageVersion()
        {
            var sql = "SELECT version FROM storageVersion;";
            using (var command = new SQLiteCommand(sql, _connection))
            {
                var executeScalar = Convert.ToInt32(command.ExecuteScalar());
                return executeScalar;
            }
        }

        #region Games

        public int Save(DatabaseGame game, bool updateGame)
        {
            if (_inError)
            {
                return -1;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return -1;
                }
            }

            var gameId = game.Id;
           _connection.Open();
            var sqLiteTransaction = _connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                string moveList = string.Empty;
                foreach (var gameAllMove in game.AllMoves)
                {
                    if (gameAllMove == null)
                    {
                        continue;
                    }
                    moveList += gameAllMove.FromFieldName + gameAllMove.ToFieldName;
                }
                var deterministicHashCode = moveList.GetDeterministicHashCode();
                var aSerializer = new XmlSerializer(typeof(DatabaseGame));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                aSerializer.Serialize(sw, game);
                var xmlResult = sw.GetStringBuilder().ToString();
                if (game.CurrentGame != null && (game.CurrentGame.RepeatedGame || updateGame))
                {
                 
                    var sql = @"UPDATE games set result=@result, gameDate=@gameDate, pgn=@pgn, pgnXML=@pgnXML,pgnHash=@pgnHash
                           WHERE id=@id; ";
                    using (var command2 = new SQLiteCommand(sql, _connection))
                    {
                        DateTime gameDate = game.GameDate;
                        if (gameDate.Equals(DateTime.MinValue))
                        {
                            if (!DateTime.TryParse(game.PgnGame.GameDate.Replace("??", "01"), out gameDate))
                            {
                                gameDate = DateTime.UtcNow;
                            }
                        }

                        command2.Parameters.Add("@result", DbType.String).Value = game.Result;
                        command2.Parameters.Add("@gameDate", DbType.Int64).Value = gameDate.ToFileTime();
                        command2.Parameters.Add("@pgn", DbType.String).Value = game.PgnGame.MoveList;
                        command2.Parameters.Add("@pgnXml", DbType.String).Value = xmlResult;
                        command2.Parameters.Add("@pgnHash", DbType.Int32).Value = deterministicHashCode;
                        command2.Parameters.Add("@id", DbType.Int32).Value = game.Id;

                        command2.ExecuteNonQuery();
                    }
                    sql =  @"DELETE FROM fenToGames  WHERE game_id=@game_id; ";
                    using (var command2 = new SQLiteCommand(sql, _connection))
                    {
                        command2.Parameters.Add("@game_id", DbType.String).Value = game.Id;
                        command2.ExecuteNonQuery();
                    }

                }
                else
                {
                    var sql =
                        @"INSERT INTO games (white, black, event,site, result, gameDate, pgn, pgnXml, pgnHash, round, white_elo, black_elo)
                           VALUES (@white, @black, @event, @site,  @result, @gameDate, @pgn, @pgnXml, @pgnHash, @round, @white_elo, @black_elo); ";
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
                        command2.Parameters.Add("@round", DbType.Int32).Value = game.Round;
                        command2.Parameters.Add("@white_elo", DbType.String).Value = game.PgnGame.WhiteElo;
                        command2.Parameters.Add("@black_elo", DbType.String).Value = game.PgnGame.BlackElo;
                        command2.ExecuteNonQuery();
                    }


                    using (var command2 = new SQLiteCommand("SELECT LAST_INSERT_ROWID();", _connection))
                    {
                        var executeScalar = command2.ExecuteScalar();
                        gameId = int.Parse(executeScalar.ToString());
                    }
                }

                var moveNumber = 1;
                var chessBoard = new ChessBoard();
                chessBoard.Init();
                chessBoard.NewGame();

                foreach (var move in game.MoveList)
                {
                    if (move == null)
                    {
                        continue;
                    }
                    chessBoard.MakeMove(move);
                    var fen = chessBoard.GetFenPosition();
                    var fenId = 0;
                    var shortFen = fen.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    var sql = "SELECT id FROM fens WHERE shortFen=@shortFen;";
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
                return -1;
            }
            finally
            {
                _connection.Close();
            }

            return gameId;
        }

        public void DeleteGame(int id)
        {

            _connection.Open();
            var sqLiteTransaction = _connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                var sql = @"DELETE FROM fenToGames WHERE game_id=@game_id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@game_id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games WHERE id=@id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM tournamentGames WHERE game_id=@id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM duelGames WHERE game_id=@id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM fens WHERE id NOT IN (SELECT fen_id FROM fenToGames); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.ExecuteNonQuery();
                }

                sqLiteTransaction.Commit();
            }
            catch (Exception ex)
            {
                sqLiteTransaction.Rollback();
                _logging?.LogError(ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public DatabaseGame LoadGame(int id, bool purePGN)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return null;
                }
            }

            DatabaseGame databaseGame = null;
           
            try
            {
                _connection.Open();
                var sql =
                    "SELECT id, white, black, event, site, result, gameDate, pgn, pgnXml, pgnHash, round FROM games WHERE id=@ID;";
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
                                databaseGame = (DatabaseGame)xmlSerializer.Deserialize(reader);
                                
                                databaseGame.Id = id;

                                var pgnCreator = databaseGame.CurrentGame == null ? new PgnCreator(purePGN) :  new PgnCreator(databaseGame.CurrentGame.StartPosition, purePGN);
                                foreach (var databaseGameAllMove in databaseGame.AllMoves)
                                {
                                    pgnCreator.AddMove(databaseGameAllMove);
                                }

                                foreach (var move in pgnCreator.GetAllMoves())
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

        public DatabaseGameSimple[] GetGames(GamesFilter gamesFilter)
        {
            return GetGames(gamesFilter, string.Empty);
            
        }

        private DatabaseGameSimple[] FilterByFen(string fen)
        {
            if (_inError)
            {
                return Array.Empty<DatabaseGameSimple>();
            }

            if (fen.StartsWith("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"))
            {
                return GetBySql(
                    "SELECT id, white, black, event, site, result, gameDate, pgn, pgnXml, pgnHash, round, white_elo, black_elo FROM games ORDER BY ID;");
            }
            DatabaseGameSimple[] allGames = null;
            _connection.Open();
            using (var cmd = new SQLiteCommand(
                "SELECT g.id, g.white, g.black, g.event, g.site, g.result, g.gameDate, g.pgn, g.pgnXml, g.pgnHash, g.round, g.white_elo, g.black_elo" +
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

        public DatabaseGameSimple[] GetGames(GamesFilter gamesFilter, string fen)
        {
            if (_inError)
            {
                return Array.Empty<DatabaseGameSimple>();
            }

            try
            {
                if (!_dbExists)
                {
                    LoadDb();
                    if (!CreateTables())
                    {
                        return Array.Empty<DatabaseGameSimple>();
                    }
                }

                if (!gamesFilter.FilterIsActive)
                {
                    if (string.IsNullOrWhiteSpace(fen))
                    {
                        return GetBySql(
                            "SELECT id, white, black, event, site, result, gameDate, pgn, pgnXml, pgnHash, round, white_elo, black_elo FROM games ORDER BY ID;");
                    }

                    return FilterByFen(fen);
                }

                string filterSQl = string.Empty;
                if (gamesFilter.NoDuelGames)
                {
                    filterSQl += " AND g.Id NOT IN (SELECT game_id FROM  duelGames) ";
                }
                if (gamesFilter.NoTournamentGames)
                {
                    filterSQl += " AND g.Id NOT IN (SELECT game_id FROM  tournamentGames) ";
                }
                if (gamesFilter.WhitePlayerWhatever)
                {
                    filterSQl += " AND (g.white LIKE @white OR g.black LIKE @white) ";
                }
                else
                {
                    filterSQl += " AND (g.white LIKE @white )";
                }
                if (gamesFilter.BlackPlayerWhatever)
                {
                    filterSQl += " AND (g.black LIKE @black OR g.white LIKE @black) ";
                }
                else
                {
                    filterSQl += " AND (g.black LIKE @black )";
                }

                string fenSQl = string.Empty;
                if (!string.IsNullOrWhiteSpace(fen))
                {
                    fenSQl = " JOIN fenToGames as fg ON (g.ID=fg.game_id) " +
                             " JOIN fens as f ON (fg.fen_id = f.id)";
                    filterSQl += " AND (f.shortFen=@shortFen) ";
                }
                DatabaseGameSimple[] allGames = null;
                _connection.Open();
                using (var cmd = new SQLiteCommand(
                    "SELECT g.id, g.white, g.black, g.event, g.site, g.result, g.gameDate, g.pgn, g.pgnXml, g.pgnHash, g.round, g.white_elo, g.black_elo" +
                    " FROM games g  " +
                    fenSQl+
                    " WHERE g.Event LIKE @gameEvent" +
                    " AND (gameDate BETWEEN @fromDate AND @toDate) " +
                    filterSQl +
                    " ORDER BY g.ID;", _connection))
                {
                    cmd.Parameters.Add("@gameEvent", DbType.String).Value = string.IsNullOrWhiteSpace(gamesFilter.GameEvent) ? "%" : gamesFilter.GameEvent;
                    cmd.Parameters.Add("@fromDate", DbType.Int64).Value = gamesFilter.FromDate?.ToFileTime() ?? 0;
                    cmd.Parameters.Add("@toDate", DbType.Int64).Value = gamesFilter.ToDate?.ToFileTime() ?? long.MaxValue;

                    cmd.Parameters.Add("@black", DbType.String).Value = string.IsNullOrWhiteSpace(gamesFilter.BlackPlayer) ? "%" : gamesFilter.BlackPlayer;
                    cmd.Parameters.Add("@white", DbType.String).Value = string.IsNullOrWhiteSpace(gamesFilter.WhitePlayer) ? "%" : gamesFilter.WhitePlayer;
                    if (!string.IsNullOrWhiteSpace(fen))
                    {
                        cmd.Parameters.Add("@shortFen", DbType.String).Value = fen.Split(" ".ToCharArray())[0];
                    }

                    using (var rdr = cmd.ExecuteReader())
                    {
                        allGames = GetByReader(rdr);
                        rdr.Close();
                    }
                }

                _connection.Close();
                return allGames;
            }
            catch (Exception ex)
            {
                _inError = true;
                _logging?.LogError(ex);
            }

            return Array.Empty<DatabaseGameSimple>();
        }

        public int GetTotalGamesCount()
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return 0;
                }
            }

            try
            {
                _connection.Open();
                var sql = "SELECT COUNT(*) FROM games;";
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    var executeScalar = cmd.ExecuteScalar();
                    return int.Parse(executeScalar.ToString());
                }
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
                return 0;
            }
            finally
            {
                _connection.Close();
            }
        }

        private DatabaseGameSimple[] GetByReader(SQLiteDataReader rdr)
        {

            var allGames = new List<DatabaseGameSimple>(10000);
            while (rdr.Read())
            {
                var pgnHash = rdr.GetInt32(9);
                var databaseGameSimple = new DatabaseGameSimple
                                         {
                                             Id = rdr.GetInt32(0),
                                             White = rdr.GetString(1),
                                             Black = rdr.GetString(2),
                                             GameEvent = rdr.GetString(3),
                                             GameSite = rdr.GetString(4),
                                             Result = rdr.GetString(5),
                                             GameDate = DateTime.FromFileTime(rdr.GetInt64(6)),
                                             MoveList = rdr.GetString(7),
                                             Round = rdr.GetInt32(10).ToString(),
                                             WhiteElo = rdr.GetString(11),
                                             BlackElo = rdr.GetString(12),
                                             PgnHash = pgnHash
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


        #endregion

        #region Duel
        public int SaveDuel(CurrentDuel engineDuel)
        {
            if (_inError)
            {
                return -1;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return -1;
                }
            }

            var tournamentId = 0;
            _connection.Open();

            try
            {

                var aSerializer = new XmlSerializer(typeof(CurrentDuel));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                aSerializer.Serialize(sw, engineDuel);
                var xmlResult = sw.GetStringBuilder().ToString();
                var sql = @"INSERT INTO duel (event, eventDate, gamesToPlay, configXML)
                           VALUES (@event, @eventDate, @gamesToPlay, @configXML); ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@event", DbType.String).Value = engineDuel.GameEvent;
                    command2.Parameters.Add("@eventDate", DbType.Int64).Value = DateTime.UtcNow.ToFileTime();
                    command2.Parameters.Add("@gamesToPlay", DbType.Int32).Value = engineDuel.Cycles;
                    command2.Parameters.Add("@configXML", DbType.String).Value = xmlResult;
                    command2.ExecuteNonQuery();
                }

                using (var command2 = new SQLiteCommand("SELECT LAST_INSERT_ROWID();", _connection))
                {
                    var executeScalar = command2.ExecuteScalar();
                    tournamentId = int.Parse(executeScalar.ToString());
                }
            }
            catch (Exception ex)
            {

                _logging?.LogError(ex);
                return -1;
            }
            finally
            {
                _connection.Close();
            }

            return tournamentId;
        }

        public void SaveDuelGamePair(int duelId, int gameId)
        {
            if (_inError)
            {
                return;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return;
                }
            }

            _connection.Open();
            try
            {
                var sql = @"INSERT INTO duelGames (duel_id, game_id) VALUES (@duel_id, @game_id); ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@duel_id", DbType.Int32).Value = duelId;
                    command2.Parameters.Add("@game_id", DbType.Int32).Value = gameId;
                    command2.ExecuteNonQuery();
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
        
        public void DeleteAllDuel()
        {

            _connection.Open();
            var sqLiteTransaction = _connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                string sql;
                sql = @"DELETE FROM fenToGames WHERE game_id IN (select game_id FROM duelGames); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {

                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games WHERE id IN (select game_id FROM duelGames); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {

                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM duelGames; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM duel; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {

                    command.ExecuteNonQuery();
                }

                sqLiteTransaction.Commit();
            }
            catch (Exception ex)
            {
                sqLiteTransaction.Rollback();
                _logging?.LogError(ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public void DeleteDuelGames(int id)
        {
            try
            {
                _connection.Open();
                string sql;

                sql = @"DELETE FROM fenToGames WHERE game_id IN (select game_id FROM duelGames  WHERE duel_id=@id); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games WHERE id IN (select game_id FROM duelGames  WHERE duel_id=@id); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM duelGames  WHERE duel_id=@id; ";
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

        public void UpdateDuel(int id, CurrentDuel duel)
        {
            if (_inError)
            {
                return;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return;
                }
            }
            _connection.Open();

            try
            {

                var aSerializer = new XmlSerializer(typeof(CurrentDuel));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                aSerializer.Serialize(sw, duel);
                var xmlResult = sw.GetStringBuilder().ToString();
                var sql = @"UPDATE duel SET event=@event, configXML=@configXML
                           WHERE id=@id; ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@event", DbType.String).Value = duel.GameEvent;
                    command2.Parameters.Add("@configXML", DbType.String).Value = xmlResult;
                    command2.Parameters.Add("@id", DbType.Int32).Value = id;
                    command2.ExecuteNonQuery();
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

        public void DeleteDuel(int id)
        {
            try
            {
                _connection.Open();
                string sql;

                sql = @"DELETE FROM fenToGames WHERE game_id IN (select game_id FROM duelGames  WHERE duel_id=@id); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games WHERE id IN (select game_id FROM duelGames  WHERE duel_id=@id); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM duelGames  WHERE duel_id=@id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM duel  WHERE id=@id; ";
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

        public void DeleteDuelGame(int gameId)
        {
            try
            {
                _connection.Open();
                string sql;

                sql = @"DELETE FROM fenToGames WHERE game_id = @gameId; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@gameId", DbType.Int32).Value = gameId;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games WHERE id = @gameId; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@gameId", DbType.Int32).Value = gameId;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM duelGames  WHERE game_id=@gameId; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@gameId", DbType.Int32).Value = gameId;
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
        public DatabaseDuel[] LoadDuel()
        {
            List<DatabaseDuel> allDuels = new List<DatabaseDuel>();
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return allDuels.ToArray();
                }
            }

            try
            {
                _connection.Open();
                var sql = "SELECT t.id, t.configXML, t.gamesToPlay, t.eventDate, count(g.id) as playedGames " +
                          "FROM duel as T left join duelGames as g on (t.id=g.duel_id) " +
                          "group by t.configXML,t.gamesToPlay,t.eventDate " +
                          "ORDER BY t.id;";
                var xmlSerializer = new XmlSerializer(typeof(CurrentDuel));
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            CurrentDuel currentDuel;
                            using (TextReader reader = new StringReader(rdr.GetString(1)))
                            {
                                currentDuel = (CurrentDuel)xmlSerializer.Deserialize(reader);
                            }

                            allDuels.Add(new DatabaseDuel()
                                         {
                                             DuelId = rdr.GetInt32(0),
                                             CurrentDuel = currentDuel,
                                             GamesToPlay = rdr.GetInt32(2),
                                             PlayedGames = rdr.GetInt32(4),
                                             State = rdr.GetInt32(2) == rdr.GetInt32(4) ? "Finished" : "Running",
                                             EventDate = DateTime.FromFileTime(rdr.GetInt64(3)),
                                             Participants = string.Join(", ", currentDuel.Players.Select(c => c.Name))
                                         });
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
            return allDuels.ToArray();
        }

        public DatabaseDuel LoadDuel(int id)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return null;
                }
            }

            DatabaseDuel dbDuel = null;
            try
            {
                _connection.Open();
                var sql = "SELECT t.Id, t.configXML, t.gamesToPlay, t.eventDate, count(g.id) as playedGames " +
                          "FROM duel as T left join duelGames as g on (t.id=g.duel_id) " +
                          "WHERE t.id=@ID " +
                          "group by t.configXML,t.gamesToPlay,t.eventDate " +
                          "ORDER BY t.id;";
                var xmlSerializer = new XmlSerializer(typeof(CurrentDuel));
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    cmd.Parameters.Add("@ID", DbType.Int32).Value = id;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            CurrentDuel currentDuel = null;
                            using (TextReader reader = new StringReader(rdr.GetString(1)))
                            {
                                currentDuel = (CurrentDuel) xmlSerializer.Deserialize(reader);
                            }

                            dbDuel = new DatabaseDuel()
                                     {
                                         DuelId = rdr.GetInt32(0),
                                         CurrentDuel = currentDuel,
                                         GamesToPlay = rdr.GetInt32(2),
                                         PlayedGames = rdr.GetInt32(4),
                                         State = rdr.GetInt32(2) == rdr.GetInt32(4) ? "Finished" : "Running",
                                         EventDate = DateTime.FromFileTime(rdr.GetInt64(3)),
                                         Participants = string.Join(", ", currentDuel.Players.Select(c => c.Name))
                                     };
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
            return dbDuel;
        }

        public DatabaseDuel LoadDuelByGame(int gameId)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return null;
                }
            }

            DatabaseDuel dbDuel = null;


            int id = 0;
            try
            {

                _connection.Open();
                var sql = "SELECT duel_id FROM duelGames WHERE game_id = @game_Id";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@game_id", DbType.Int32).Value = gameId;

                    var executeScalar = command.ExecuteScalar();
                    id = int.Parse(executeScalar.ToString());

                }

                sql = "SELECT t.Id, t.configXML, t.gamesToPlay, t.eventDate, count(g.id) as playedGames " +
                      "FROM duel as T left join duelGames as g on (t.id=g.duel_id) " +
                      "WHERE t.id=@ID " +
                      "group by t.configXML,t.gamesToPlay,t.eventDate " +
                      "ORDER BY t.id;";
                var xmlSerializer = new XmlSerializer(typeof(CurrentDuel));
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    cmd.Parameters.Add("@ID", DbType.Int32).Value = id;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            CurrentDuel currentDuel = null;
                            using (TextReader reader = new StringReader(rdr.GetString(1)))
                            {
                                currentDuel = (CurrentDuel)xmlSerializer.Deserialize(reader);
                            }

                            dbDuel = new DatabaseDuel()
                                     {
                                         DuelId = rdr.GetInt32(0),
                                         CurrentDuel = currentDuel,
                                         GamesToPlay = rdr.GetInt32(2),
                                         PlayedGames = rdr.GetInt32(4),
                                         State = rdr.GetInt32(2) == rdr.GetInt32(4) ? "Finished" : "Running",
                                         EventDate = DateTime.FromFileTime(rdr.GetInt64(3)),
                                         Participants = string.Join(", ", currentDuel.Players.Select(c => c.Name))
                                     };
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
            return dbDuel;
        }

        public int GetDuelGamesCount(int duelId)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return 0;
                }
            }

            int gamesCount = 0;
            try
            {
                _connection.Open();
                var sql = "SELECT COUNT(*) FROM duelGames WHERE duel_id=@duel_id;";

                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    cmd.Parameters.Add("@duel_id", DbType.Int32).Value = duelId;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            gamesCount = rdr.GetInt32(0);
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
            return gamesCount;
        }

        public DatabaseGameSimple[] GetDuelGames(int duelId)
        {

            if (_inError)
            {
                return Array.Empty<DatabaseGameSimple>();
            }

            DatabaseGameSimple[] allGames = null;
            _connection.Open();
            using (var cmd = new SQLiteCommand(
                "SELECT g.id, g.white, g.black, g.event, g.site, g.result, g.gameDate, g.pgn, g.pgnXml, g.pgnHash, g.round, g.white_elo, g.black_elo" +
                " FROM games as g  " +
                " JOIN duelGames t ON (t.game_id=g.id)" +
                "WHERE t.duel_id=@duelId;", _connection))
            {
                cmd.Parameters.Add("@duelId", DbType.Int32).Value = duelId;
                using (var rdr = cmd.ExecuteReader())
                {
                    allGames = GetByReader(rdr);
                    rdr.Close();
                }
            }

            _connection.Close();
            return allGames;
        }

        public bool IsDuelGame(int gameId)
        {
            if (_inError)
            {
                return true;
            }

            bool isDuelGame = false;
            _connection.Open();
            var sql = "SELECT duel_id FROM duelGames WHERE game_id = @game_Id";
            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.Add("@game_id", DbType.Int32).Value = gameId;
                var executeScalar = command.ExecuteScalar();
                isDuelGame = executeScalar != null;
            }
            _connection.Close();
            return isDuelGame;
        }

        public void UpdateDuel(CurrentDuel engineDuel, int id)
        {
            if (_inError)
            {
                return;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return;
                }
            }

            _connection.Open();

            try
            {

                var aSerializer = new XmlSerializer(typeof(CurrentDuel));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                aSerializer.Serialize(sw, engineDuel);
                var xmlResult = sw.GetStringBuilder().ToString();
                var sql = @"UPDATE duel SET gamesToPlay=@gamesToPlay, configXML=@configXML
                           WHERE ID=@ID; ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@ID", DbType.Int32).Value = id;
                    command2.Parameters.Add("@gamesToPlay", DbType.Int32).Value = engineDuel.Cycles;
                    command2.Parameters.Add("@configXML", DbType.String).Value = xmlResult;
                    command2.ExecuteNonQuery();
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
        #endregion

        #region Tournament
        public void SaveTournamentGamePair(int tournamentId, int gameId)
        {
            if (_inError)
            {
                return;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return;
                }
            }

            _connection.Open();
            try
            {
                var sql = @"INSERT INTO tournamentGames (tournament_id, game_id) VALUES (@tournament_id, @game_id); ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@tournament_id", DbType.Int32).Value = tournamentId;
                    command2.Parameters.Add("@game_id", DbType.Int32).Value = gameId;
                    command2.ExecuteNonQuery();
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

        public void UpdateTournament(int id, CurrentTournament tournament)
        {
            if (_inError)
            {
                return;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return;
                }
            }
            _connection.Open();

            try
            {

                var aSerializer = new XmlSerializer(typeof(CurrentTournament));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                aSerializer.Serialize(sw, tournament);
                var xmlResult = sw.GetStringBuilder().ToString();
                var sql = @"UPDATE tournament SET event=@event, configXML=@configXML
                           WHERE id=@id; ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@event", DbType.String).Value = tournament.GameEvent;
                    command2.Parameters.Add("@configXML", DbType.String).Value = xmlResult;
                    command2.Parameters.Add("@id",DbType.Int32).Value = id;
                    command2.ExecuteNonQuery();
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

        public int SaveTournament(CurrentTournament tournament, int gamesToPlay)
        {
            if (_inError)
            {
                return -1;
            }

            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return -1;
                }
            }

            var tournamentId = 0;
            _connection.Open();

            try
            {

                var aSerializer = new XmlSerializer(typeof(CurrentTournament));
                var sb = new StringBuilder();
                var sw = new StringWriter(sb);
                aSerializer.Serialize(sw, tournament);
                var xmlResult = sw.GetStringBuilder().ToString();
                var sql = @"INSERT INTO tournament (event, eventDate, gamesToPlay, configXML)
                           VALUES (@event, @eventDate, @gamesToPlay, @configXML); ";
                using (var command2 = new SQLiteCommand(sql, _connection))
                {
                    command2.Parameters.Add("@event", DbType.String).Value = tournament.GameEvent;
                    command2.Parameters.Add("@eventDate", DbType.Int64).Value = DateTime.UtcNow.ToFileTime();
                    command2.Parameters.Add("@gamesToPlay", DbType.Int32).Value = gamesToPlay;
                    command2.Parameters.Add("@configXML", DbType.String).Value = xmlResult;
                    command2.ExecuteNonQuery();
                }

                using (var command2 = new SQLiteCommand("SELECT LAST_INSERT_ROWID();", _connection))
                {
                    var executeScalar = command2.ExecuteScalar();
                    tournamentId = int.Parse(executeScalar.ToString());
                }
            }
            catch (Exception ex)
            {

                _logging?.LogError(ex);
                return -1;
            }
            finally
            {
                _connection.Close();
            }

            return tournamentId;
        }

        public void DeleteAllTournament()
        {
            _connection.Open();
            var sqLiteTransaction = _connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                string sql;
                sql = @"DELETE FROM fenToGames WHERE game_id IN (select game_id FROM tournamentGames); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {

                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games WHERE id IN (select game_id FROM tournamentGames); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {

                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM tournamentGames; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM tournament; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {

                    command.ExecuteNonQuery();
                }

                sqLiteTransaction.Commit();
            }
            catch (Exception ex)
            {
                sqLiteTransaction.Rollback();
                _logging?.LogError(ex);
            }
            finally
            {
                _connection.Close();
            }
        }

        public void DeleteTournamentGames(int id)
        {
            try
            {
                _connection.Open();
                string sql;

                sql = @"DELETE FROM fenToGames WHERE game_id IN (select game_id FROM tournamentGames  WHERE tournament_id=@id); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM games WHERE id IN (select game_id FROM tournamentGames  WHERE tournament_id=@id); ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM tournamentGames  WHERE tournament_id=@id; ";
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

        public void DeleteTournament(int id)
        {
            try
            {
                _connection.Open();
                string sql;
                
                    sql = @"DELETE FROM fenToGames WHERE game_id IN (select game_id FROM tournamentGames  WHERE tournament_id=@id); "; 
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.Parameters.Add("@id", DbType.Int32).Value = id;
                        command.ExecuteNonQuery();
                    }

                    sql = @"DELETE FROM games WHERE id IN (select game_id FROM tournamentGames  WHERE tournament_id=@id); ";
                    using (var command = new SQLiteCommand(sql, _connection))
                    {
                        command.Parameters.Add("@id", DbType.Int32).Value = id;
                        command.ExecuteNonQuery();
                    }
                
                sql = @"DELETE FROM tournamentGames  WHERE tournament_id=@id; ";
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@id", DbType.Int32).Value = id;
                    command.ExecuteNonQuery();
                }

                sql = @"DELETE FROM tournament  WHERE id=@id; ";
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

        public DatabaseTournament[] LoadTournament()
        {
            List<DatabaseTournament> allTournaments = new List<DatabaseTournament>();
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return allTournaments.ToArray();
                }
            }

            try
            {
                _connection.Open();
                var sql = "SELECT t.id, t.configXML, t.gamesToPlay, t.eventDate, count(g.id) as playedGames " +
                          "FROM tournament as T left join tournamentGames as g on (t.id=g.tournament_id) " +
                          "group by t.configXML,t.gamesToPlay,t.eventDate " +
                          "ORDER BY t.id;";
                var xmlSerializer = new XmlSerializer(typeof(CurrentTournament));
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            CurrentTournament currentTournament;
                            using (TextReader reader = new StringReader(rdr.GetString(1)))
                            {
                                currentTournament = (CurrentTournament)xmlSerializer.Deserialize(reader);
                            }

                            allTournaments.Add(new DatabaseTournament()
                                               {
                                                   TournamentId = rdr.GetInt32(0),
                                                   CurrentTournament = currentTournament,
                                                   GamesToPlay = rdr.GetInt32(2),
                                                   PlayedGames = rdr.GetInt32(4),
                                                   State = rdr.GetInt32(2) == rdr.GetInt32(4) ? "Finished" : "Running",
                                                   EventDate = DateTime.FromFileTime(rdr.GetInt64(3)),
                                                   Participants = string.Join(", ", currentTournament.Players.Select(c => c.Name))
                                               });
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
            return allTournaments.ToArray();
        }

        public DatabaseTournament LoadTournament(int id)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return null;
                }
            }

            DatabaseTournament dbTournament = null;
            try
            {
                _connection.Open();
                var sql = "SELECT t.Id, t.configXML, t.gamesToPlay, t.eventDate, count(g.id) as playedGames " +
                          "FROM tournament as T left join tournamentGames as g on (t.id=g.tournament_id) " +
                          "WHERE t.id=@ID "+
                          "group by t.configXML,t.gamesToPlay,t.eventDate " +
                          "ORDER BY t.id;";
                var xmlSerializer = new XmlSerializer(typeof(CurrentTournament));
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    cmd.Parameters.Add("@ID", DbType.Int32).Value = id;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            CurrentTournament currentTournament = null;
                            using (TextReader reader = new StringReader(rdr.GetString(1)))
                            {
                                currentTournament = (CurrentTournament) xmlSerializer.Deserialize(reader);
                            }
                            dbTournament = new DatabaseTournament()
                                         {
                                TournamentId = rdr.GetInt32(0),
                                CurrentTournament = currentTournament,
                                GamesToPlay = rdr.GetInt32(2),
                                PlayedGames = rdr.GetInt32(4),
                                State = rdr.GetInt32(2) == rdr.GetInt32(4) ? "Finished" : "Running",
                                EventDate = DateTime.FromFileTime(rdr.GetInt64(3)),
                                Participants = string.Join(", ", currentTournament.Players.Select(c => c.Name))
                            };
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
            return dbTournament;
        }

        public DatabaseTournament LoadTournamentByGame(int gameId)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return null;
                }
            }

            try
            {
                _connection.Open();
                var sql = "SELECT tournament_id FROM tournamentGames WHERE game_id = @game_Id";
                int id;
                using (var command = new SQLiteCommand(sql, _connection))
                {
                    command.Parameters.Add("@game_id", DbType.Int32).Value = gameId;

                    var executeScalar = command.ExecuteScalar();
                    id = int.Parse(executeScalar.ToString());

                }
                _connection.Close();
                return LoadTournament(id);
            
            }
            catch (Exception ex)
            {
                _logging?.LogError(ex);
            }

            return null;
        }

        public int GetTournamentGamesCount(int tournamentId)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return 0;
                }
            }

            int gamesCount = 0;
            try
            {
                _connection.Open();
                var sql = "SELECT COUNT(*) FROM tournamentGames WHERE tournament_id=@tournament_id;";
                
                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    cmd.Parameters.Add("@tournament_id", DbType.Int32).Value = tournamentId;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            gamesCount = rdr.GetInt32(0);
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
            return gamesCount;
        }

        public bool IsTournamentGame(int gameId)
        {
            if (_inError)
            {
                return true;
            }

            bool isTournamentGame = false;
            _connection.Open();
            var sql = "SELECT tournament_id FROM tournamentGames WHERE game_id = @game_Id";
            using (var command = new SQLiteCommand(sql, _connection))
            {
                command.Parameters.Add("@game_id", DbType.Int32).Value = gameId;
                var executeScalar = command.ExecuteScalar();
                isTournamentGame = executeScalar != null;
            }
            _connection.Close();
            return isTournamentGame;
        }

        public int GetLatestTournamentGameId(int tournamentId)
        {
            if (!_dbExists)
            {
                if (!CreateTables())
                {
                    return 0;
                }
            }

            int gamesId = 0;
            try
            {
                _connection.Open();
                var sql = "SELECT games_id FROM tournamentGames WHERE id = (select max(id) FROM tournamentGames WHERE tournament_id=@tournament_id);";

                using (var cmd = new SQLiteCommand(sql, _connection))
                {
                    cmd.Parameters.Add("@tournament_id", DbType.Int32).Value = tournamentId;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            gamesId = rdr.GetInt32(0);
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
            return gamesId;
        }

        public DatabaseGameSimple[] GetTournamentGames(int tournamentId)
        {


            if (_inError)
            {
                return Array.Empty<DatabaseGameSimple>();
            }

            DatabaseGameSimple[] allGames = null;
            _connection.Open();
            using (var cmd = new SQLiteCommand(
                "SELECT g.id, g.white, g.black, g.event, g.site, g.result, g.gameDate, g.pgn, g.pgnXml, g.pgnHash, g.round, g.white_elo, g.black_elo" +
                " FROM games as g  " +
                " JOIN tournamentGames t ON (t.game_id=g.id)" +
                "WHERE t.tournament_id=@tournamentId;", _connection))
            {
                cmd.Parameters.Add("@tournamentId", DbType.Int32).Value = tournamentId;
                using (var rdr = cmd.ExecuteReader())
                {
                    allGames = GetByReader(rdr);
                    rdr.Close();
                }
            }

            _connection.Close();
            return allGames;
        }

        #endregion

   
    }
}