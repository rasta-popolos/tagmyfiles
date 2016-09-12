using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyUtils;

namespace DatabaseManager
{
    partial class MyDatabase
    {
        public static string TAG_JOIN = "INTERSECT";
        
        public string TableFiles
        {
            get
            {
                return this._tableFiles;
            }
            set
            {
                this._tableFiles = value;
            }
        }

        /// <summary>
        /// Rescan le repertoire courrant
        ///     Ajoute les fichiers non tracké
        ///     Executé dans un task
        /// </summary>
        /// <returns></returns>
        public Task ReloadFile()
        {
            if (!Directory.Exists(this.DIR)) return null;

            DirectoryInfo dir = new DirectoryInfo(this.DIR);

            MyVideos.Ffmpeg.VIDEO_EXTENSIONS = this.GetVideosExtensions();

            Task task = Task.Factory.StartNew(() =>
            {
                this.Begin();
                this.addDirFile(dir, "*");
                this.End();
            }, TaskCreationOptions.LongRunning)
                .ContinueWith((prevTask) =>
                {
                    this.endAddFile();
                });

            return task;
        }

        /// <summary>
        /// Scan les fichiers vidéos et ajout les tag par défault
        /// </summary>
        /// <returns></returns>
        public Task ReloadVideoFile()
        {
            if (!Directory.Exists(this.DIR)) return null;

            DirectoryInfo dir = new DirectoryInfo(this.DIR);


            MyVideos.Ffmpeg.VIDEO_EXTENSIONS = this.GetVideosExtensions();

            Task task = Task.Factory.StartNew(() =>
            {
                this.Begin();
                this.ReloadSpecificFile();
                this.End();
            }, TaskCreationOptions.LongRunning)
                .ContinueWith((prevTask) =>
                {
                    this.endAddFile();
                });

            return task;
        }

        /// <summary>
        /// Met à jour les tag des fichiers spécifiques (Vidéos)
        /// </summary>
        public void ReloadSpecificFile(int? fileId = null)
        {
            List<string> ext = this.GetVideosExtensions();
            string fileType = string.Join(",", ext);
            string sql = @"
                SELECT DISTINCT
                    f.*,
                    ext.name as extension
                FROM 
                    " + TableFiles + @" f
                LEFT JOIN
                    " + TableExtension + @" ext
                    ON  f.extension_id = ext.id
                WHERE
                    CharIndex(ext.name, @fileType) > 0
            ";

            Hashtable aParams = new Hashtable();
            aParams["fileType"] = fileType;

            if (fileId != null)
            {
                sql += " AND f.id = @fileId";
                aParams["fileId"] = fileId;
            }

            List<Hashtable> res = this.SelectFiles(sql, aParams);
            string path;

            foreach (Hashtable file in res)
            {
                path = file["path"].ToString();
                if (File.Exists(path))
                {
                    this.addTagForVideoFiles(path, file["id"].ToString().ToInt());
                }
            }
        }

        /// <summary>
        /// Ajout les tag par défault d'un fichier vidéos
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileId"></param>
        public void addTagForVideoFiles(string path, int fileId)
        {
            Hashtable aSize = MyVideos.Ffmpeg.getVideoInformation(path);
            List<int> aFileId = new List<int>();
            aFileId.Add(fileId);

            if (aSize.Count > 0)
            {
                // Insert into tag Video
                this.addGroupFiles(this.getGroupIdFromTag("video"), aFileId);

                // Resolution
                if (aSize.ContainsKey("width") && aSize.ContainsKey("height"))
                {
                    if (aSize["width"].ToString().IsNumeric() && aSize["height"].ToString().IsNumeric())
                    {
                        long resolution = long.Parse(aSize["width"].ToString()) * long.Parse(aSize["height"].ToString());

                        // UHD resolution 2160
                        if (resolution >= (3840 * 2160))
                        {
                            this.addGroupFiles(this.getGroupIdFromTag("video_resolution_uhd"), aFileId);
                        }
                        // FHD resolution 1080
                        if (resolution == (1920 * 1080))
                        {
                            this.addGroupFiles(this.getGroupIdFromTag("video_resolution_fhd"), aFileId);
                        }
                        // HD resolution 720
                        if (resolution == (1280 * 720))
                        {
                            this.addGroupFiles(this.getGroupIdFromTag("video_resolution_hd"), aFileId);
                        }

                        // Poor_resolution
                        if (resolution < (320 * 240))
                        {
                            this.addGroupFiles(this.getGroupIdFromTag("video_resolution_poor"), aFileId);
                        }
                        //Low_resolution
                        else if (resolution < (640 * 480))
                        {
                            this.addGroupFiles(this.getGroupIdFromTag("video_resolution_low"), aFileId);
                        }
                        //Medium_resolution
                        else if (resolution < (800 * 600))
                        {
                            this.addGroupFiles(this.getGroupIdFromTag("video_resolution_medium"), aFileId);
                        }
                        //High_resolution
                        else if (resolution >= (800 * 600))
                        {
                            this.addGroupFiles(this.getGroupIdFromTag("video_resolution_high"), aFileId);
                        }
                    }
                }

                //Length
                if (aSize.ContainsKey("format_duration") && aSize["format_duration"].ToString().IsNumeric())
                {
                    float duration = float.Parse(aSize["format_duration"].ToString());
                    if (duration <= 120) // 2mn
                    {
                        this.addGroupFiles(this.getGroupIdFromTag("video_length_veryshort"), aFileId);
                    }
                    else if (duration <= 600) // 10mn
                    {
                        this.addGroupFiles(this.getGroupIdFromTag("video_length_short"), aFileId);
                    }
                    else if (duration <= 2700) // 45mn
                    {
                        this.addGroupFiles(this.getGroupIdFromTag("video_length_average"), aFileId);
                    }
                    else if (duration <= 7200) // 2h
                    {
                        this.addGroupFiles(this.getGroupIdFromTag("video_length_long"), aFileId);
                    }
                    else if (duration > 7200)   // > 2h
                    {
                        this.addGroupFiles(this.getGroupIdFromTag("video_length_verylong"), aFileId);
                    }
                }
            }
        }

        /// <summary>
        /// Scan un repertoire et ajout les fichier dans la BDD
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="searchPattern"></param>
        protected void addDirFile(DirectoryInfo dir, string searchPattern)
        {
            // list the files
            foreach (FileInfo f in dir.GetFiles(searchPattern))
            {
                this.addFile(f.FullName.ToString());
            }

            // list directories
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                this.addDirFile(d, searchPattern);                    
            }
        }

        /// <summary>
        /// Methode executer à la fin du task de scan
        /// </summary>
        public void endAddFile()
        {
            this.Query("END TRANSACTION");
            //MessageBox.Show(Globalisation.GetString("Load_end"));
        }

        /// <summary>
        /// Reccup les infos d'un fichier dans la bdd
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public Hashtable getFile(int fileId)
        {
            string sql = "SELECT * FROM " + TableFiles + " WHERE id = @id";
            Hashtable aParams = new Hashtable();
            aParams["id"] = fileId;

            List<Hashtable> res = this.SelectFiles(sql, aParams);
            if (res.Count > 0)
            {
                string file = res[0]["path"].ToString();
                if (!File.Exists(file))
                {
                    this.deleteFiles(fileId);
                    return null;
                }
                else
                {
                    return res[0];
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Requete select sur la table files (ajout le DIR)
        ///     Ajout du path complet dans le retour
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="aParams"></param>
        /// <returns></returns>
        protected List<Hashtable> SelectFiles(string sql, Hashtable aParams = null)
        {
            List<Hashtable> res = this.Select(sql, aParams);
            string maindir = this.DIR;
            
            DateTime fileCreate;
            DateTime fileModify;
            Hashtable param;
            if (!this.DIR.Trim().EndsWith("\\"))
            {
                maindir = maindir + "\\";
            }
            if (res.Count > 0)
            {
                this.Begin();
                foreach (Hashtable file in res)
                {
                    if (file.ContainsKey("path"))
                    {
                        file["path"] = maindir + "" + file["path"].ToString();
                    }
                    if (file.ContainsKey("date_create") && string.IsNullOrEmpty(file["date_create"].ToString()))
                    {
                        fileCreate = File.GetCreationTime(file["path"].ToString());
                        string sqlDt = "UPDATE " + TableFiles + " SET date_create = @dateCreate WHERE id = @fileId";
                        param = new Hashtable();
                        param["dateCreate"] = fileCreate.ToString("yyyy-MM-dd HH:mm:ss");
                        param["fileId"] = file["id"].ToString();
                        this.Query(sqlDt, param);
                    }
                    if (file.ContainsKey("date_modify") && string.IsNullOrEmpty(file["date_modify"].ToString()))
                    {
                        fileModify = File.GetLastWriteTime(file["path"].ToString());
                        string sqlDt = "UPDATE " + TableFiles + " SET date_modify = @dateModify WHERE id = @fileId";
                        param = new Hashtable();
                        param["dateModify"] = fileModify.ToString("yyyy-MM-dd HH:mm:ss");
                        param["fileId"] = file["id"].ToString();
                        this.Query(sqlDt, param);
                    }
                }
                this.End();

            }

            return res;
        }

        /// <summary>
        /// Ajoute un fichier dans la bdd
        ///     Ajoute le tag d'extension correspondant
        ///     Ajoute les tags vidéo correspondants
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public int addFile(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    string sql = "INSERT INTO " + TableFiles + " (name, path, size, extension_id, date_create, date_modify) VALUES (@name, @file, @size, @extension, @dateCreate, @dateModify)";

                    Hashtable aParams = new Hashtable();
                    FileInfo f = new FileInfo(file);

                    // Suppr du Path le repertoir courrant
                    string path = file.RegReplace("^" + Regex.Escape(this.DIR), "");

                    aParams["name"] = f.Name;
                    aParams["file"] = path;
                    aParams["size"] = f.Length;
                    aParams["extension"] = this.getExtensionId(f.Extension);
                    aParams["dateCreate"] = File.GetCreationTime(file).ToString("yyyy-MM-dd HH:mm:ss");
                    aParams["dateModify"] = File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm:ss");
                    
                    int? fId = this.QueryWithId(sql, aParams);
                    if (fId != null)
                    {
                        this.AddExtensionTag(file, (int) fId);
                        return (int) fId;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }                
            }
            catch
            {
                //throw new System.Exception(ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Ajoute le tag extension
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fId"></param>
        public void AddExtensionTag(string filePath, int fId)
        {
            FileInfo f = new FileInfo(filePath);
            // Insert to Extension Group
            int groupId = this.getGroupIdFromTag(f.Extension);
            List<int> fileId = new List<int>();

            if (fId > 0 && groupId > 0)
            {
                fileId.Add(fId.ToString().ToInt());
                this.addGroupFiles(groupId, fileId);
                //this.addTagForVideoFiles(path, fId.ToString().ToInt());
            }
        }

        /// <summary>
        /// Reccupe la liste des fichiers filtrer suivant le name ou les tags
        /// </summary>
        /// <param name="aFilters"></param>
        /// <returns></returns>
        public List<Hashtable> getFiles(Hashtable aFilters)
        {
            string sql;
            List<String> where;

            // Filters

            string filter = "";
            List<String> aTags = null;
            int fileType = 0;

            //if (aTags != null && aTags.Count == 0) return;
            if (aFilters != null)
            {
                if (aFilters.ContainsKey("filetype"))
                    fileType = aFilters["filetype"].ToString().ToInt();

                if (aFilters.ContainsKey("textfilter"))
                    filter = aFilters["textfilter"].ToString();

                if (aFilters.ContainsKey("tags"))
                    aTags = (List<String>)aFilters["tags"];
            }


            List<string> aSql = new List<string>();
            Hashtable aParams = new Hashtable();
            
            if (aTags == null || aTags.Count == 0)
            {
                aTags = new List<String>();
                aTags.Add("");
            }

            if (filter != "")
            {
                aParams["path"] = "%" + filter + "%";
            }

            if (fileType > 0)
            {
                    Hashtable f = this.GetFileTypeMain(fileType);
                    aParams["fileType"] = f["extension"].ToString() + ",";
            }

            foreach (string tag in aTags) 
            {
                sql = @"
                    SELECT DISTINCT
                        f.*,
                        ext.name as extension
                    FROM 
                        " + TableFiles + @" f
                    LEFT JOIN
                        " + TableExtension + @" ext
                        ON  f.extension_id = ext.id
                    LEFT JOIN
                        " + TableGroupFiles + @" tf
                        ON  f.id = tf.files_id
                    LEFT JOIN
                        " + TableFileStatus + @" fs
                        ON  f.id = fs.files_id
                ";

                where = new List<String>();
                if (filter != "")
                {
                    // recherche spécific
                    if (filter == "no tag" || filter == "no tags")
                    {
                        where.Add("f.id NOT IN (SELECT files_id FROM " + TableGroupFiles + ")");
                    }
                    else if (filter == "default tag only" || filter == "default tags only")
                    {

                        where.Add("f.id NOT IN (SELECT jf.id FROM " + TableFiles + " jf, " + TableGroupFiles + " jgf, " + TableGroup + " jg WHERE jf.id = jgf.files_id AND jgf.group_id = jg.id AND (jg.tag IS NULL OR jg.tag = ''))");
                    }
                    else if (filter == "duplicated files" || filter == "duplicated file")
                    {
                        where.Add(@"
                            f.size in (
                                select size from (select count(*), size from " + TableFiles + @" group by size  having count(*) > 1)
                            )
                        ");
                    }
                    else if (filter == "deleted files" || filter == "deleted file")
                    {

                    }
                    else if (filter == "merged files" || filter == "merged file")
                    {

                    }
                    else if (filter == "not merged files" || filter == "not merged file")
                    {

                    }
                    else
                    {
                        where.Add("path LIKE @path");
                    }
                }

                #region Status filter
                if (filter == "deleted files" || filter == "deleted file")
                {
                    where.Add("fs.status = 1");
                }
                else if (filter == "merged files" || filter == "merged file")
                {
                    where.Add("fs.status = 2");
                }
                else if (filter == "not merged files" || filter == "not merged file")
                {
                    where.Add("(fs.status <> 2 OR fs.status IS NULL OR fs.status = '')");
                    //where.Add("fs.status <> 1");
                }
                else
                {
                    where.Add("(fs.status <> 1 OR fs.status IS NULL OR fs.status = '')");
                }
                #endregion

                if (tag != "")
                {
                    where.Add("tf.group_id = " + tag);
                }
                if (fileType > 0)
                {
                    if (fileType == 99)
                    {
                        Hashtable f = this.GetAllFileTypeMain();
                        aParams["fileType"] = f["extension"].ToString() + ",";
                        where.Add("CharIndex(ext.name || ',', @fileType) <= 0");
                    }
                    else
                    {
                        where.Add("CharIndex(ext.name || ',', @fileType) > 0");
                    }
                }

                if (where.Count > 0)
                {
                    sql += " WHERE " + String.Join(" AND ", where);
                }
                aSql.Add(sql);
            }
            // INTERSECT OR UNION
            sql = @"
                SELECT DISTINCT * 
                FROM (
                    " + String.Join(" " + TAG_JOIN + " ", aSql) + @"

                ) as t
            ";

            if (aFilters != null && aFilters.ContainsKey("order") && aFilters["order"].ToString() != String.Empty)
            {
                sql += " ORDER BY " + aFilters["order"].ToString() + "";
            }
            else
            {
                //sql += " ORDER BY LOWER(name)";
                sql += " ORDER BY id DESC";
            }
            return this.SelectFiles(sql, aParams);
        }

        /// <summary>
        /// Supprime un fichier de la bdd
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool deleteFiles(int fileId)
        {
            if (!Directory.Exists(this.DIR)) return false;

            string sql = "DELETE FROM " + TableFiles + " WHERE id = (@fileId)";
            Hashtable aParams = new Hashtable();
            aParams["fileId"] = fileId;

            this.Query(sql, aParams);
            return true;
        }

        /// <summary>
        /// Update file status
        ///     1 : delete
        ///     2 : merge
        /// </summary>
        /// <param name="FilesId"></param>
        /// <param name="status"></param>
        public void UpdateStatusFile(int FilesId, int status, string parameters="")
        {
            List<int> f = new List<int>();
            f.Add(FilesId);
            this.UpdateStatusFile(f, status, parameters);
        }

        /// <summary>
        /// Update file status
        ///     1 : delete
        ///     2 : merge
        /// </summary>
        /// <param name="FilesId"></param>
        /// <param name="status"></param>
        public void UpdateStatusFile(List<int> FilesId, int status, string parameters = "")
        {
            //this.Begin();

            string sql;
            sql = "DELETE FROM " + TableFileStatus + " WHERE status = " + status + " AND files_id IN (" + string.Join(",", FilesId) + ")";
            this.Query(sql);

            Hashtable aParams;
            foreach (int f in FilesId)
            {
                aParams = new Hashtable();
                aParams["status"] = status;
                aParams["parameters"] = parameters;

                sql = "INSERT INTO " + TableFileStatus + " (files_id, status, parameters) VALUES (" + f + ", @status, @parameters)";
                this.Query(sql, aParams);
            }

            //this.End();
        }

        /// <summary>
        /// Get file status
        ///     1 : deleted
        ///     2 : merge
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public List<int> GetFileStatus(int fileId)
        {
            List<int> ret = new List<int>();

            string sql = "SELECT status FROM " + TableFileStatus + " WHERE files_id = @fileId";
            Hashtable aParams = new Hashtable();
            aParams["fileId"] = fileId;
            List<Hashtable> res = this.Select(sql, aParams);
            if (res.Count > 0)
            {
                ret.Add(res[0]["status"].ToString().ToInt());
            }
            return ret;
        }


        /// <summary>
        /// Reccup le nombre de fichier
        /// </summary>
        /// <returns></returns>
        public int getFileCount()
        {
            string sql = "SELECT count(*) as ct FROM " + TableFiles;
            List<Hashtable> res = this.Select(sql);

            if (res.Count > 0)
            {
                return res[0]["ct"].ToString().ToInt();
            }
            return 0;
        }

        /// <summary>
        /// Reccup le taille de fichier
        /// </summary>
        /// <returns></returns>
        public long getFileSize()
        {
            string sql = "SELECT sum(size) as ct FROM " + TableFiles;
            List<Hashtable> res = this.Select(sql);

            if (res.Count > 0)
            {
                return long.Parse(res[0]["ct"].ToString());
            }
            return 0;
        }

        /// <summary>
        /// Reccup xxx (taille, nombre..) des fichier par type
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, long>> GetFileXXXByType(string xxx)
        {
            List<KeyValuePair<string, long>> res = new List<KeyValuePair<string, long>>();

            string field = "";
            switch (xxx.ToLower())
            {
                case "size":
                    field = "sum(f.size)";
                    break;
                case "count":
                    field = "count(*)";
                    break;
                default:
                    return res;
            }

            List<Hashtable> listType = this.SelectMainDb("SELECT * FROM file_type");
            foreach (Hashtable tp in listType)
            {
                Hashtable f = this.GetFileTypeMain(tp["id"].ToString().ToInt());
                Hashtable aParams = new Hashtable();
                aParams["fileType"] = f["extension"].ToString() + ",";

                string sql = @"
                    SELECT
                        " + field + @" as xxx
                    FROM 
                        " + TableFiles + @" f
                    LEFT JOIN
                        " + TableExtension + @" ext
                        ON  f.extension_id = ext.id
                    WHERE
                        CharIndex(ext.name || ',', @fileType) > 0
                    ORDER BY
                        xxx DESC
                ";
                List<Hashtable> aSize = this.Select(sql, aParams);
                long size = 0;
                if (aSize.Count > 0 && long.TryParse(aSize[0]["xxx"].ToString(), out size) && size > 0)
                {
                    res.Add(new KeyValuePair<string, long>(tp["name"].ToString(), size));
                }
            }

            return res;
        }

        /// <summary>
        /// Reccup xxx (taille, nombre..) des fichier par extension
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, long>> GetFileXXXByExtension(string xxx)
        {
            List<KeyValuePair<string, long>> res = new List<KeyValuePair<string, long>>();

            string field = "";
            switch (xxx.ToLower())
            {
                case "size":
                    field = "sum(f.size)";
                    break;
                case "count":
                    field = "count(*)";
                    break;
                default:
                    return res;
            }

            string sql = @"
                SELECT
                    ext.value,
                    " + field + @" as xxx
                FROM 
                    " + TableFiles + @" f
                LEFT JOIN
                    " + TableExtension + @" ext
                    ON  f.extension_id = ext.id
                GROUP BY
                    ext.value 
                ORDER BY
                    xxx DESC
            ";
            List<Hashtable> aSize = this.Select(sql);
            foreach (Hashtable f in aSize)
            {
                res.Add(new KeyValuePair<string, long>(f["value"].ToString(), long.Parse(f["xxx"].ToString())));
            }

            return res;
        }

        /// <summary>
        /// Reccup nombre fichier entre deux tailles
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public int GetFileCountBetweenSize(long lower, long upper)
        {
            int res = 0;

            string sql = @"
                SELECT
                    count(*) as ct
                FROM 
                    " + TableFiles + @" f
                WHERE
                    size >= "+lower.ToString()+ @"
                    AND size < " + upper.ToString() + @"
            ";

            List<Hashtable> aSize = this.Select(sql);
            if (aSize.Count > 0)
            {
                res = aSize[0]["ct"].ToString().ToInt();
            }
            return res;
        }

        /// <summary>
        /// Reccup nombre de fichiers par [taille[
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, long>> GetFileXXXByInterval(string xxx)
        {
            List<KeyValuePair<string, long>> res = new List<KeyValuePair<string, long>>();

            string sql = @"SELECT size FROM " + TableFiles + " ORDER BY size ASC";
            List<Hashtable> aSize = this.Select(sql);

            List<long> aListeValues = new List<long>();

            foreach (Hashtable f in aSize)
            {
                aListeValues.Add(long.Parse(f["size"].ToString()));
            }


            List<long> aListeSeuils = MyUtils.CLassif.classSize();
            //List<long> aListeSeuils = MyUtils.CLassif.classQua(aListeValues, 20);
            //List<long> aListeSeuils = MyUtils.CLassif.classMoyenne(aListeValues, 20);
            //List<long> aListeSeuils = MyUtils.CLassif.classAmp(aListeValues, 20);
            
            // Traite un retour de classX
            for(int i=0;i < aListeSeuils.Count - 1; i++){
                long lower = aListeSeuils[i];
                long upper = aListeSeuils[i + 1];

                res.Add(new KeyValuePair<string, long>(upper.ToFileSize(), this.GetFileCountBetweenSize(lower, upper)));
            }
            return res;
        }

    }
}
