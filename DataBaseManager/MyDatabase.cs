using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyUtils;

namespace DatabaseManager
{
    public partial class MyDatabase
    {

        protected string dbVersion = "3";
        
        private string _tableFiles = "files";
        private string _tableGroup = "groups";
        private string _tableGroupFiles = "groups_files";
        private string _tableExtension = "extensions";
        private string _tableFileStatus = "file_status";


        protected int DIR_ID
        {
            get
            {
                return this.GetDefaultDirectory();
            }
        }

        // G:\moviz\xxx\ / G:\moviz\film\
        public string DIR {
            get {
                return this.GetPathDirectory();
            }
        }


        public bool IsDefined
        {
            get
            {
                if (string.IsNullOrEmpty(this.DIR))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public string DbFile
        {
            get
            {
                if (this.InTransaction)
                {
                    return this.m_dbConnection.ConnectionString;
                }
                else
                {
                    Hashtable res = this.GetDirectory(this.DIR_ID);
                    if (res.Count > 0)
                    {
                        string db = res["db"].ToString();
                        return @"db\" + db + ".db";
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
            }
        }

        public string TableFileStatus
        {
            get
            {
                return this._tableFileStatus;
            }
            set
            {
                this._tableFileStatus = value;
            }
        }

        public bool InitDb()
        {
            // Si aucune repertoire n'est sélectionné
            if (!this.IsDefined) return false;
            
            // si la base n'existe pas => création des tables
            bool bInitDatabase = false;
            if (!File.Exists(this.DbFile) || new System.IO.FileInfo(this.DbFile).Length == 0)
            {
                bInitDatabase = true;
            }

            string sql;
            if (bInitDatabase)
            {
                try
                {
                    // Init des tables de la BDD
                    sql = @"CREATE TABLE IF NOT EXISTS
                                " + TableGroup + @" (
                                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    name varchar(255) NOT NULL,
                                    parent_id INTEGER NOT NULL DEFAULT 0,
                                    tag varchar(255),
                                    ordernode NOT NULL DEFAULT 0,
                                    description TEXT,
                                    unique (name, parent_id)
                                )";
                    this.Query(sql);

                    sql = "CREATE UNIQUE INDEX uk_" + TableGroup + "_tag ON " + TableGroup + " (name COLLATE nocase, parent_id)";
                    this.Query(sql);

                }
                catch { }

                try
                {
                    sql = @"CREATE TABLE IF NOT EXISTS
                            " + TableFiles + @" (
                                id INTEGER PRIMARY KEY AUTOINCREMENT,
                                name TEXT NOT NULL,
                                path TEXT UNIQUE NOT NULL,
                                size NUMERIC,
                                extension_id INTEGER NOT NULL REFERENCES " + TableExtension + @"(id) ON DELETE SET NULL ON UPDATE CASCADE,
                                date_create datetime,
                                date_modify datetime,
                                options TEXT
                            )";
                    this.Query(sql);

                }
                catch { }

                try
                {
                    sql = @"CREATE TABLE IF NOT EXISTS
                            " + TableGroupFiles + @" (
                                group_id INTEGER NOT NULL REFERENCES " + TableGroup + @"(id) ON DELETE CASCADE ON UPDATE CASCADE,
                                files_id INTEGER NOT NULL REFERENCES " + TableFiles + @"(id) ON DELETE CASCADE ON UPDATE CASCADE,
                                PRIMARY KEY (group_id, files_id)
                            )";
                    this.Query(sql);

                }
                catch { }

                try
                {
                    sql = @"CREATE TABLE IF NOT EXISTS
                            " + TableExtension + @" (
                                id INTEGER PRIMARY KEY AUTOINCREMENT,
                                name TEXT NOT NULL,
                                value TEXT NOT NULL
                            )";
                    this.Query(sql);

                }
                catch { }

                try
                {
                    /*
                     * Status:
                     *      1 : deleted
                     *      2 : merged
                     * */                    
                    sql = @"CREATE TABLE IF NOT EXISTS
                            " + TableFileStatus + @" (
                                files_id INTEGER NOT NULL REFERENCES " + TableFiles + @"(id) ON DELETE CASCADE ON UPDATE CASCADE,
                                status INTEGER NOT NULL,
                                parameters TEXT
                            )";
                    this.Query(sql);

                }
                catch { }

                this.InitData();
            }

            //this.Query("alter table files add column date_create datetime");
            //this.Query("alter table files add column date_modify datetime");

            // Purge group_files
            this.Query("DELETE FROM " + TableGroupFiles + " WHERE files_id NOT IN (SELECT id FROM " + TableFiles + ")");

            // Purge status files
            this.Query("DELETE FROM " + TableFileStatus + " WHERE files_id NOT IN (SELECT id FROM " + TableFiles + ")");

            return true;
        }

        /**
         * INitialise les données par défault des tags (favoris, extension, resolution, durée....)
         * */
        protected void InitData()
        {
            // Insert Tag node
            this.InitInsertGroup(this.DIR, 0, "root", 1, "");

            // Insert Favoris node
            this.InitInsertGroup("Favorites", 0, "favorites", 1, "");

            // Insert extension node
            this.InitInsertGroup("Extension", 0, "extension", 1, "");

            // Insert Video node
            int videoId = this.InitInsertGroup("Video", 0, "video", 0, "");

                // Insert Video Resolution node
                int groupVideoResolution = this.InitInsertGroup("Video_resolution", videoId, "video_resolution", 0, "");

                    // Insert Video Resolution items
                    this.InitInsertGroup("Video_resolution_Poor_resolution", groupVideoResolution, "video_resolution_poor", 1, "< 320 x 240");
                    this.InitInsertGroup("Video_resolution_Low_resolution", groupVideoResolution, "video_resolution_low", 1, "< 640 x 480");
                    this.InitInsertGroup("Video_resolution_Medium_resolution", groupVideoResolution, "video_resolution_medium", 1, "< 800 x 600");
                    this.InitInsertGroup("Video_resolution_High_resolution", groupVideoResolution, "video_resolution_high", 1, "> 800 x 600");
                    this.InitInsertGroup("Video_resolution_HD_resolution", groupVideoResolution, "video_resolution_hd", 1, "1280 * 720");
                    this.InitInsertGroup("Video_resolution_FHD_resolution", groupVideoResolution, "video_resolution_fhd", 1, "1920 * 1080");

                // Insert Video Length node
                int groupVideoLength = this.InitInsertGroup("Video_length", videoId, "video_length", 0, "");

                    // Insert Video Length items
                    this.InitInsertGroup("Video_length_VeryShort_length", groupVideoLength, "video_length_veryshort", 1, "< 2mn");
                    this.InitInsertGroup("Video_length_Short_length", groupVideoLength, "video_length_short", 1, "< 10mn");
                    this.InitInsertGroup("Video_length_Average_length", groupVideoLength, "video_length_average", 1, "< 45mn");
                    this.InitInsertGroup("Video_length_Long_length", groupVideoLength, "video_length_long", 1, "< 2h");
                    this.InitInsertGroup("Video_length_VeryLong_length", groupVideoLength, "video_length_verylong", 1, "> 2h");

        }


    }
}
