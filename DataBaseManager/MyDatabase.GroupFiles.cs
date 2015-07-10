using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager
{
    public partial class MyDatabase
    {
        public string TableGroupFiles
        {
            get
            {
                return this._tableGroupFiles;
            }
            set
            {
                this._tableGroupFiles = value;
            }
        }

        /// <summary>
        /// Ajoute un tag à un fichier
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool addGroupFiles(int groupId, List<int> fileId)
        {
            if (fileId.Count == 0) return false;

            string sql;
            Hashtable aParams;

            foreach (int file in fileId)
            {
                try
                {
                    sql = "INSERT INTO " + TableGroupFiles + " (group_id, files_id) VALUES (@groupId, @fileId)";
                    aParams = new Hashtable();
                    aParams["groupId"] = groupId;
                    aParams["fileId"] = file;
                    this.Query(sql, aParams);
                }
                catch
                {

                }
            }
            return true;
        }

        /// <summary>
        /// Reccup les Tag associé à un fichier
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public List<Hashtable> GetGroupFile(int fileId) 
        {
            string sql = @"
                SELECT
                    g.*,
                    gf.files_id
                FROM " + TableGroupFiles + @" gf
                JOIN
                    " + TableGroup + @" g
                    ON g.id = gf.group_id
                WHERE
                    gf.files_id = @fileId
                ORDER BY
                    g.name
            ";
            Hashtable aParams = new Hashtable();
            aParams["fileId"] = fileId;

            return this.Select(sql, aParams);

        }

        /// <summary>
        /// Suppr le tag d'un fichier
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool RemoveGroupFile(int groupId, int fileId)
        {
            try
            {
                string sql = "DELETE FROM " + TableGroupFiles + " WHERE group_id = @groupId AND files_id = @fileId";
                Hashtable aParams = new Hashtable();
                aParams["groupId"] = groupId;
                aParams["fileId"] = fileId;
                return this.Query(sql, aParams);
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Add FilesId tag to fid
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="filesId"></param>
        public void MergeGroupFile(int fid, int filesId)
        {
            List<int> FilesId = new List<int>();
            FilesId.Add(filesId);
            this.MergeGroupFile(fid, FilesId);
        }

        /// <summary>
        /// Add FilesId tag to fid
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="FilesId"></param>
        public void MergeGroupFile(int fid, List<int> FilesId) 
        {
            //sql = "INSERT INTO " + TableGroupFiles + " (group_id, files_id) VALUES (@groupId, @fileId)";
            string sql = @"
                SELECT DISTINCT
                    gf.group_id,
                    " + fid  + @" 
                FROM
                    " + TableGroupFiles + @" gf,
                    " + TableGroup + @" g
                WHERE
                    gf.group_id = g.id
                    AND (g.tag = '' OR g.Tag IS NULL)
                    AND gf.files_id IN (" + string.Join(",", FilesId) + @")
            ";
            this.Query("INSERT INTO " + TableGroupFiles + "  (group_id, files_id) " + sql);
        }

    }
}