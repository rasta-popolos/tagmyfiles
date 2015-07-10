using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyUtils;

namespace DatabaseManager
{
    partial class MyDatabase
    {


        protected SQLiteConnection m_dbConnection;

        public string ConnectionString
        {
            get
            {
                return "Data Source=" + this.DbFile + ";Version=" + this.dbVersion + ";";
            }
        }

        public bool InTransaction
        {
            get { return this.m_dbConnection != null; }
        }


        public MyDatabase()
        {
            //this.ConnectionString = "Data Source=" + this.DbFile + ";Version=" + this.dbVersion + ";";

            // init main database
            this.InitMainDb();

            // init directory database
            this.InitDb();

            MyVideos.Ffmpeg.VIDEO_EXTENSIONS = this.GetVideosExtensions();
        }

        /// <summary>
        /// Ajjout les parameters d'une requete
        /// </summary>
        /// <param name="command"></param>
        /// <param name="aParams"></param>
        protected void addParameters(SQLiteCommand command, Hashtable aParams = null)
        {
            if (aParams != null)
            {
                foreach (DictionaryEntry entry in aParams)
                {
                    command.Parameters.Add(new SQLiteParameter("@" + entry.Key.ToString(), entry.Value.ToString()));
                }
            }
        }

        /// <summary>
        /// Reccup le dernier id inséré
        /// </summary>
        /// <returns></returns>
        public int GetLastInsertId()
        {
            string sql = "select last_insert_rowid() as id";
            List<Hashtable> fId = this.Select(sql);
            if (fId.Count > 0)
            {
                return fId[0]["id"].ToString().ToInt();
            }
            return 0;
        }

        /// <summary>
        /// Requete sur la bdd courrante
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="aParams"></param>
        /// <returns></returns>
        public bool Query(string sql, Hashtable aParams=null)
        {
            try
            {
                if (! this.InTransaction)
                {
                    using (SQLiteConnection c = new SQLiteConnection(this.ConnectionString))
                    {
                        using (SQLiteCommand command = new SQLiteCommand(sql, c))
                        {
                            c.Open();
                            this.addParameters(command, aParams);
                            command.ExecuteNonQuery();
                            return true;
                        }
                    }
                }
                else
                {
                    SQLiteCommand command = new SQLiteCommand(sql, this.m_dbConnection);
                    this.addParameters(command, aParams);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Requete du la bdd courrante en retourant l'id si insert
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="aParams"></param>
        /// <returns></returns>
        public int? QueryWithId(string sql, Hashtable aParams = null)
        {
            try
            {
                if (!this.InTransaction)
                {
                    using (SQLiteConnection c = new SQLiteConnection(this.ConnectionString))
                    {
                        using (SQLiteCommand command = new SQLiteCommand(sql, c))
                        {
                            c.Open();
                            this.addParameters(command, aParams);
                            command.ExecuteNonQuery();

                            sql = "select last_insert_rowid() as id";
                            using (SQLiteCommand cmd = new SQLiteCommand(sql, c))
                            {
                                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                                {
                                    if (reader.Read())
                                    {
                                        return reader["id"].ToString().ToInt();
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
                // Pour transaction
                else
                {
                    SQLiteCommand command = new SQLiteCommand(sql, this.m_dbConnection);
                    this.addParameters(command, aParams);
                    command.ExecuteNonQuery();
                    return this.GetLastInsertId();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Requete select sur la bdd courrante
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="aParams"></param>
        /// <returns></returns>
        public List<Hashtable> Select(string sql, Hashtable aParams = null)
        {
            List<Hashtable> result = new List<Hashtable>();


            if (!this.InTransaction)
            {
                using (SQLiteConnection c = new SQLiteConnection(this.ConnectionString))
                {
                    using (SQLiteCommand command = new SQLiteCommand(sql, c))
                    {
                        c.Open();
                        this.addParameters(command, aParams);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            List<string> readerColumn = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

                            Hashtable row;
                            while (reader.Read())
                            {
                                row = new Hashtable();
                                for (int i = 0; i < readerColumn.Count; i++)
                                {
                                    try
                                    {
                                        row[readerColumn[i]] = reader.GetValue(i);
                                    }
                                    catch
                                    {
                                        row[readerColumn[i]] = reader.GetDateTime(i).ToString("dd/MM/yyyy");
                                    }
                                }
                                result.Add(row);
                            }
                        }
                    }
                }
            }
            // transaction
            else
            {
                SQLiteCommand command = new SQLiteCommand(sql, this.m_dbConnection);
                this.addParameters(command, aParams);
                SQLiteDataReader reader = command.ExecuteReader();

                List<string> readerColumn = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

                Hashtable row;
                while (reader.Read())
                {
                    row = new Hashtable();
                    for (int i = 0; i < readerColumn.Count; i++)
                    {
                        row[readerColumn[i]] = reader.GetValue(i);
                    }
                    result.Add(row);
                }
            }
            return result;
        }

        /// <summary>
        /// Requete select sur la bdd courrant, retourne la première ligne trouvé
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="aParams"></param>
        /// <returns></returns>
        public Hashtable SelectRow(string sql, Hashtable aParams = null)
        {
            Hashtable result = new Hashtable();
            /*
            SQLiteCommand command = new SQLiteCommand(sql, this.m_dbConnection);
            this.addParameters(command, aParams);
            SQLiteDataReader reader = command.ExecuteReader();

            List<string> readerColumn = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            //bool s = reader.Read();

            while (reader.Read())
            {
                for (int i = 0; i < readerColumn.Count; i++)
                {
                    result[readerColumn[i]] = reader.GetValue(i);
                }
                break;
            }
            */

            List<Hashtable> res = this.Select(sql, aParams);
            if (res.Count > 0)
            {
                result = res[0];
            }
            return result;
        }

        /// <summary>
        /// Débute une transaction sur la bdd courrante
        /// </summary>
        public void Begin()
        {
            if (!this.InTransaction)
            {
                this.m_dbConnection = new SQLiteConnection(this.ConnectionString);
                this.m_dbConnection.Open();
            }
            else
            {
                this.End();
                this.Begin();
            }
            this.Query("BEGIN TRANSACTION");
        }

        /// <summary>
        /// Ferme la transaction sur la bdd courrante
        /// </summary>
        public void End()
        {
            this.Query("END TRANSACTION");
            if (this.InTransaction)
            {
                this.m_dbConnection.Dispose();
                this.m_dbConnection = null;
            }
        }

    }
}
