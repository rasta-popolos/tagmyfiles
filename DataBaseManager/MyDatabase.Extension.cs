using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyUtils;

namespace DatabaseManager
{
    public partial class MyDatabase
    {
        Hashtable fileExtension = new Hashtable();

        public string TableExtension
        {
            get
            {
                return this._tableExtension;
            }
            set
            {
                this._tableExtension = value;
            }
        }

        /// <summary>
        /// Ajoute une extension dans la table extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        protected bool addExtension(String extension) 
        {
            try
            {
                string sExt = extension.RegReplace(@"^\.", "").UcFirst();

                string sql = "INSERT INTO " + TableExtension + " (name, value) VALUES (@name, @value)";
                Hashtable aParams = new Hashtable();
                aParams["name"] = extension.ToLower();
                aParams["value"] = sExt;

                this.Query(sql, aParams);

                // Insert dans Group table
                int extId = this.getGroupIdFromTag("extension");
                if (extId > 0)
                {
                    this.addGroup(sExt, extId, aParams["name"].ToString());
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Return l'id de l'extension specifie
        /// Insert si n'existe pas 
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        protected int getExtensionId(string extension)
        {
            extension = extension.ToLower();

            if (this.fileExtension.ContainsKey(extension))
            {
                return this.fileExtension[extension].ToString().ToInt();
            }
            else
            {
                string sql = "SELECT * FROM " + TableExtension + " WHERE name = @extension";
                Hashtable aParams = new Hashtable();
                aParams["extension"] = extension;

                List<Hashtable> res = this.Select(sql, aParams);
                if (res.Count == 0)
                {
                    this.addExtension(extension);
                    return this.getExtensionId(extension);
                }
                else
                {
                    this.fileExtension[extension] = res[0]["id"].ToString().ToInt();
                    return this.fileExtension[extension].ToString().ToInt();
                }
            }
        }

        /// <summary>
        /// Reccup la liste des extensions disponibles sur le repertoire courrant
        /// </summary>
        /// <returns></returns>
        public List<Hashtable> getExtension()
        {
            string sql = "SELECT * FROM " + TableExtension + " ORDER BY name";
            return this.Select(sql);
        }

        /// <summary>
        /// Reccup la liste des nombre fichier par tag
        /// </summary>
        /// <returns></returns>
        public List<Hashtable> getTagCount()
        {
            string sql = @"
                SELECT
                    g.name,
                    sum(CASE
			            WHEN gf.files_id IS NULL THEN 0
			            ELSE 1
		            END) as ct
                FROM 
                    " + TableGroup + @" g
                LEFT JOIN
                    " + TableGroupFiles + @" gf
                        ON g.id = gf.group_id
                WHERE
                    g.tag = '' 
                    OR g.tag IS NULL
                GROUP BY
                    g.name
                ORDER BY 
                    ct DESC
            ";
            return this.Select(sql);
        }

        
    }
}
