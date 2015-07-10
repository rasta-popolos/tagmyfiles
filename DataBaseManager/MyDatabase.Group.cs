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
        public string TableGroup {
            get
            {
                return this._tableGroup;
            }
            set
            {
                this._tableGroup = value;
            }
        }

        /// <summary>
        /// Reccup l'id d'un tag à partir de son identifiant text
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected int getGroupIdFromTag(string tag)
        {
            Hashtable aParams = new Hashtable();
            aParams["tag"] = tag;
            string sql = "SELECT id FROM " + TableGroup + " WHERE tag = @tag";
            List<Hashtable> res = this.Select(sql, aParams);
            if (res.Count > 0)
            {
                return res[0]["id"].ToString().ToInt();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Insertion des tags par défaut (lors de l'initialisation)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent_id"></param>
        /// <param name="tag"></param>
        /// <param name="ordernode"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        protected int InitInsertGroup(string name, int parent_id, string tag, int ordernode = 0, string description = "")
        {
            string sql = "INSERT INTO " + TableGroup + " (name, parent_id, tag, ordernode, description) VALUES (@name, @parent_id, @tag, @ordernode, @description)";
            Hashtable aParams = new Hashtable();
            aParams["name"] = name;
            aParams["parent_id"] = parent_id;
            aParams["tag"] = tag;
            aParams["ordernode"] = ordernode;
            aParams["description"] = description;
            return (int)this.QueryWithId(sql, aParams);
        }

        /// <summary>
        /// INsertion des tags normal (user tag)
        /// </summary>
        /// <param name="group"></param>
        /// <param name="prtId"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool addGroup(string group, int prtId = 0, string tag = "")
        {
            if (String.IsNullOrEmpty(group))
            {
                throw new System.Exception(Globalisation.GetString("Group_empty"));
            }
            if (prtId == 0)
            {
                // L'ajout des root se fait à l'initialisation de MyDatabase
                throw new System.Exception(Globalisation.GetString("ParentId_rempty"));
            }

            // Test si le group existe
            try
            {
                this.InitInsertGroup(group, prtId, tag, 1, "");
                return true;
            }
            catch(Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        /// <summary>
        /// Renommer un tag
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool renameGroup(int groupId, string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new System.Exception(Globalisation.GetString("Group_empty"));
            }

            Hashtable aParams = new Hashtable();
            aParams["value"] = name;

            try
            {
                string sql = "UPDATE " + TableGroup + " SET name = @value WHERE id = @groupId";
                aParams["groupId"] = groupId;
                this.Query(sql, aParams);
                return true;
            }
            catch// (Exception ex)
            {
                //throw new System.Exception(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Suppr un tag
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public bool deleteGroup(int groupId)
        {
            //if (MessageBox.Show(Globalisation.GetString("Prompt_delete_group"), Globalisation.GetString("Delete_group"), MessageBoxButtons.YesNo) == DialogResult.Yes)
            //{
                this.Begin();
                
                this.deleteReccurseGroup(groupId);

                this.End();

                return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        /// <summary>
        /// Suppr un tag et tous les tag enfants
        /// </summary>
        /// <param name="groupId"></param>
        protected void deleteReccurseGroup(int groupId)
        {
            Hashtable aParams = new Hashtable();
            aParams["groupId"] = groupId;
            
            string sql;
            List<Hashtable> res;

            sql = "SELECT id FROM " + TableGroup + " WHERE parent_id = @groupId";
            res = this.Select(sql, aParams);
            foreach (Hashtable row in res)
            {
                this.deleteReccurseGroup(int.Parse(row["id"].ToString()));
            }

            // Ne pas effacer le Root 0
            Hashtable prt = this.GetGroup(groupId);
            if (prt.Count > 0 && prt["parent_id"].ToString() != "0")
            {
                sql = "DELETE FROM " + TableGroup + " WHERE id = @groupId";
                this.Query(sql, aParams);
            }
        }

        /// <summary>
        /// Reccup les infos d'un tag
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public Hashtable GetGroup(int groupId) 
        {
            Hashtable aParams = new Hashtable();
            aParams["groupId"] = groupId;

            //test si le parenet est un default item => trie
            string sql = "SELECT * FROM " + TableGroup + " WHERE id = @groupId ";
            return this.SelectRow(sql, aParams);
        }

        /// <summary>
        /// Reccup la liste de tag fils
        /// </summary>
        /// <param name="prtId"></param>
        /// <returns></returns>
        public List<Hashtable> GetChildGroup(int prtId = 0)
        {
            Hashtable aParams = new Hashtable();
            aParams["prtId"] = prtId;

            string sql;

            //test si le parenet est un default item => trie
            Hashtable res = GetGroup(prtId);

            sql = @"
                SELECT
                    g.id,
                    g.name,
                    g.tag,
                    g.description,
                    count(gf.files_id) as numfile
                FROM 
                    " + TableGroup + @" g
                LEFT JOIN
                    " + TableGroupFiles + @" gf
                    ON g.id = gf.group_id
                WHERE
                    parent_id = @prtId 
                GROUP BY
                    g.id, g.name, g.tag, g.description
            ";
            if (res.Count > 0 && res["ordernode"].ToString().ToInt() == 1)
            {
                sql += " ORDER BY name";
            }
            else
            {
                sql += " ORDER BY id";
            }
            return this.Select(sql, aParams);
        }

        /// <summary>
        /// Supprime les group files sans ref file
        /// </summary>
        public void SyncGroupFiles()
        {
            string sql = "DELETE FROM " + TableGroupFiles + " WHERE files_id NOT IN (SELECT id FROM " + TableFiles + ")";
            this.Query(sql);
        }

        /// <summary>
        /// Reccup l'identifiant text de tous les tags
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllTags()
        {
            List<string> res = new List<string>();
            string sql = "SELECT DISTINCT tag FROM " + TableGroup + " WHERE tag IS NOT NULL AND tag <> ''";
            List<Hashtable> tags = this.Select(sql);
            foreach (Hashtable ligne in tags)
            {
                res.Add(ligne["tag"].ToString());
            }
            return res;
        }

        /// <summary>
        /// Délace un tag (dd)
        /// </summary>
        /// <param name="destinationId"></param>
        /// <param name="nodetomoveId"></param>
        /// <returns></returns>
        public bool moveGroup(int destinationId, int nodetomoveId)
        {
            if (!destinationId.Equals(nodetomoveId))
            {
                string sql = "UPDATE " + TableGroup + " SET parent_id = @destinationId WHERE id = @nodetomoveId";
                Hashtable aParams = new Hashtable();
                aParams["destinationId"] = destinationId;
                aParams["nodetomoveId"] = nodetomoveId;
                return this.Query(sql, aParams);
            }
            return false;
        }

    }
}
