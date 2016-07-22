using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyUtils;

/***
 * Class pour géré la base de données principale
 *      table de configuration comune
 *      table contenant la liste des répertoires disponibles
 * 
 * */
namespace DatabaseManager
{
    public partial class MyDatabase
    {
        protected string MainDataBase
        {
            get
            {
                return @"Data Source=main.db;Version=" + this.dbVersion + ";";
            }
        }

        /// <summary>
        /// Init les tables les les données par défaut de la bdd principale
        /// </summary>
        protected void InitMainDb()
        {
            string sql;

            sql = @"CREATE TABLE IF NOT EXISTS directory (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                path TEXT NOT NULL,
                db TEXT NOT NULL,
                description TEXT,
                active INTEGER NOT NULL default 0
            )";
            this.QueryMainDb(sql);

            // Configuration par défault
            sql = @"CREATE TABLE IF NOT EXISTS configuration (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT UNIQUE NOT NULL,
                value TEXT,
                defaultvalue TEXT NOT NULL
            )";
            this.QueryMainDb(sql);

            sql = "INSERT INTO configuration (name, value, defaultvalue) VALUES ('gridFile_ListSize', null, '750')";
            this.QueryMainDb(sql);

            // Type de fichier
            this.QueryMainDb("DROP TABLE IF EXISTS file_type");
            sql = @"CREATE TABLE IF NOT EXISTS file_type (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT UNIQUE NOT NULL,
                extension TEXT
            )";
            this.QueryMainDb(sql);


            // http://www.youneeditall.com/web-design-and-development/file-extensions.html
            // Init filt_type data
            sql = "INSERT INTO file_type (name, extension) VALUES ('Videos', '.usm,.sfd,.aep,.fcp,.rms,.psh,.scm,.sbk,.trp,.dir,.piv,.veg,.dzm,.3gp,.mswmm,.wlmp,.wp3,.webm,.dzp,.mpeg,.ntp,.amc,.bdmv,.dzt,.gfp,.ivr,.m21,.mk3d,.mproj,.mvp,.nuv,.rdb,.rmp,.rv,.screenflow,.swt,.vcpf,.viewlet,.wpl,.dnc,.meta,.wm,.bik,.mkv,.swf,.avi,.m4v,.srt,.m2p,.prproj,.mani,.rec,.msdvd,.tp,.vob,.scc,.bnp,.gvi,.vro,.wmv,.aepx,.bin,.fbr,.dcr,.nvc,.ts,.swi,.ifo,.mpg,.ism,.amx,.mp4,.infovid,.cpi,.mnv,.vsp,.playlist,.kmv,.vp6,.mts,.asf,.hdmov,.pds,.3gp2,.pac,.trec,.vc1,.vgz,.wmx,.wve,.bu,.spl,.mmv,.vp3,.vpj,.mp4,.mob,.mov,.bdm,.xvid,.dat,.flv,.dcr,.3g2,.r3d,.stx,.yuv,.890,.avchd,.dmx,.m1pg,.ogm,.roq,.ttxt,.f4f,.ivf,.k3g,.lsx,.lvix,.mvc,.par,.qt,.vcr,.w32,.f4v,.3mm,.dav,.ogv,.smv,.264,.camproj,.dvdmedia,.fcproject,.ismv,.otrkey,.sqz,.tix,.clpi,.f4p,.fli,.hdv,.rmd,.rsx,.thp,.inp,.m15,.mpeg4,.dvr,.video,.dxr,.lrv,.ced,.mvp,.wmd,.vid,.h264,.divx,.aetx,.vep,.db2,.m2t,.dv4,.mod,.rmvb,.sfera,.mxf,.3gpp,.pmf,.ajp,.rm,.camrec,.str,.dash,.ale,.avp,.bsf,.dmsm,.dream,.imovieproj,.moi,.3p2,.aaf,.aec,.arcut,.avb,.avv,.bdt3,.bmc,.cine,.cip,.cmmp,.cmmtpl,.cmrec,.cst,.d2v,.d3v,.dce,.dck,.dmsd,.dmss,.dpa,.evo,.eyetv,.fbz,.flc,.flh,.fpdx,.ftc,.gcs,.gifv,.gts,.hkm,.imoviemobile,.imovieproject,.ircp,.ismc,.izz,.izzy,.jss,.jts,.jtv,.kdenlive,.m21,.m2ts,.m2v,.mgv,.mj2,.mp21,.mpgindex,.mpls,.mpv,.mse,.mtv,.mvd,.mve,.mxv,.ncor,.nsv,.ogx,.photoshow,.plproj,.ppj,.pro,.prtl,.pxv,.qtl,.qtz,.rcd,.rum,.rvid,.rvl,.sdv,.sedprj,.seq,.sfvidcap,.siv,.smi,.smk,.stl,.svi,.tda3mt,.tivo,.tod,.tp0,.tpd,.tpr,.tsp,.tvlayer,.tvshow,.usf,.vbc,.vcv,.vdo,.vdr,.vfz,.vlab,.vtt,.wcp,.wmmp,.wvx,.xej,.xesc,.xfl,.xlmv,.xml,.y4m,.zm1,.zm2,.zm3,.lrec,.mp4v,.mpe,.mys,.aqt,.cvc,.gom,.mpv2,.orv,.rmv,.ssm,.zeg,.zmv,.dv,.box,.dpg,.tvs,.mpl,.arf,.rcproject,.smil,.60d,.moff,.tdt,.dvr-ms,.bmk,.edl,.snagproj,.amv,.dv-avi,.eye,.mjp,.mp21,.pgi,.avs,.int,.mp2v,.scn,.ismclip,.avs,.evo,.smi,.m4e,.mpg2,.vivo,.vf,.movie,.3gpp2,.psb,.irf,.asx,.axm,.cmproj,.dmsd3d,.dvx,.ezt,.ffm,.mqv,.mvy,.prel,.vp7,.xel,.aet,.anx,.avc,.avd,.awlive,.axv,.bdt2,.bs4,.bvr,.byu,.camv,.clk,.cmv,.cx3,.ddat,.dlx,.dmb,.dmsm3d,.exo,.fbr,.fcarch,.ffd,.flx,.g64,.gvp,.imovielibrary,.iva,.jmv,.ktn,.m1v,.m2a,.m4u,.mjpg,.mpsub,.mvex,.osp,.pns,.pro4dvd,.pro5dvd,.proqc,.pssd,.pva,.qtch,.qtindex,.qtm,.rp,.rts,.sbt,.sml,.theater,.tid,.tvrecording,.vem,.vfw,.vix,.vs4,.vse,.wot,.yog,.787,.mvb,.pjs,.ssf,.wtv,.mpl,.xmv,.dif,.vft,.vmlt,.anim,.grasp,.modd,.moov,.pvr,.vmlf,.am,.bix,.cel,.dsy,.gl,.ivs,.lsf,.m75,.mpeg1,.mpf,.msh,.nut,.pmv,.rmd,.rts,.scm,.sec,.tdx,.vdx,.viv,.mp2,')";
            this.QueryMainDb(sql);

            sql = "INSERT INTO file_type (name, extension) VALUES ('Audios', '.aif,.iff,.m3u,.m4a,.mid,.mp3,.mpa,.ra,.wav,.wma,')";
            this.QueryMainDb(sql);

            sql = "INSERT INTO file_type (name, extension) VALUES ('Documents', '.xls,.xlsx,.pdf,.doc,.docx,.log,.msg,.pages,.rtf,.wpd,.wps,.efx,.gbr,.key,.pps,.ppt,.pptx,.sdf,.tax2010,.vcf,')";
            this.QueryMainDb(sql);

            sql = "INSERT INTO file_type (name, extension) VALUES ('Text_document', '.xml,.txt,.csv,')";
            this.QueryMainDb(sql);

            sql = "INSERT INTO file_type (name, extension) VALUES ('Images', '.bmp,.gif,.jpg,.jpeg,.png,.psd,.pspimage,.thm,.tif,.yuv,')";
            this.QueryMainDb(sql);

            sql = "INSERT INTO file_type (name, extension) VALUES ('Compresse', '.zip,.rar,.7z,.gz,.iso,.zoo,.ace,.arc,.arj,.lzh,.tgz')";
            this.QueryMainDb(sql);

            sql = "INSERT INTO file_type (id, name, extension) VALUES (99, 'NA', '.')";
            this.QueryMainDb(sql);
        }

        /// <summary>
        /// Reccup la valeur d'une configuration
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetConfValue(string name)
        {
            string sql = "SELECT * FROM configuration WHERE name = @name";
            Hashtable aParams = new Hashtable();
            aParams["name"] = name;
            List<Hashtable> res = this.SelectMainDb(sql, aParams);
            if (res.Count > 0)
            {
                string val = res[0]["value"].ToString();
                if(String.IsNullOrEmpty(val)) 
                {
                    return res[0]["defaultvalue"].ToString(); 
                }
                else
                {
                    return val;
                }
            }
            else
            {
                return "";
            }

        }

        /// <summary>
        /// Modifie la valeur d'une configuration
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetConfValue(string name, string value)
        {
            string sql = "UPDATE configuration SET value = @value WHERE name = @name";
            Hashtable aParams = new Hashtable();
            aParams["value"] = value;
            aParams["name"] = name;
            return this.QueryMainDb(sql, aParams);
        }
        
        /// <summary>
        /// Ajoute une nouveau repertoire scané
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool InsertDirectoryMainDb(string dir)
        {
            Hashtable aParams = new Hashtable();

            string name = dir;
            string path = dir;
            string db = dir.Md5();

            aParams["name"] = dir;
            aParams["path"] = path;
            aParams["db"] = db;

            string sql = @"
                INSERT INTO directory (
                    name,
                    path,
                    db
                ) VALUES (
                    @name,
                    @path,
                    @db
                )
            ";
            return this.QueryMainDb(sql, aParams);
        }

        /// <summary>
        /// Modifie les information d'un repertoire
        /// </summary>
        /// <param name="dirId"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool SaveDirectoryMainDb(int dirId, string name, string path, string description)
        {
            string sql = @"
                UPDATE directory
                SET 
                    name = @name,
                    path = @path,
                    description = @description
                WHERE
                    id = @dirId
            ";
            Hashtable aParams = new Hashtable();
            aParams["name"] = name;
            aParams["path"] = path;
            aParams["description"] = description;
            aParams["dirId"] = dirId;

            return this.QueryMainDb(sql, aParams);
        }

        /// <summary>
        /// Supprime un repertoire (nesupprime pas la bdd correspondante)
        /// </summary>
        /// <param name="dirId"></param>
        /// <returns></returns>
        public bool DeleteDirectoryMainDb(int dirId)
        {
            string sql = "DELETE FROM directory WHERE id = @dirId";
            Hashtable aParams = new Hashtable();
            aParams["dirId"] = dirId;
            return this.QueryMainDb(sql, aParams);
        }

        /// <summary>
        /// Reccup les infos d'un type de fichier
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public Hashtable GetFileTypeMain(int fileId) {
            string sql = @"SELECT * FROM file_type WHERE id = " + fileId.ToString();
            List<Hashtable> res = this.SelectMainDb(sql);
            return res[0];
        }

        /// <summary>
        /// Reccup les infos d'un type de fichier
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public Hashtable GetAllFileTypeMain()
        {
            string sql = @"SELECT * FROM file_type";
            List<Hashtable> res = this.SelectMainDb(sql);
            Hashtable ret = new Hashtable();
            String ext = "";
            foreach (Hashtable tag in res)
            {
                ext += tag["extension"];
            }
            ret["extension"] = ext;
            return ret;
        }

        private int _defaultDirectory = -1;

        /// <summary>
        /// Reccup le repertoire actuel
        /// </summary>
        /// <returns></returns>
        public int GetDefaultDirectory()
        {
            if (this._defaultDirectory == -1)
            {
                string sql = @"SELECT id FROM directory WHERE active = 1";
                List<Hashtable> res = this.SelectMainDb(sql);
                if (res.Count > 0)
                {
                    this._defaultDirectory = res[0]["id"].ToString().ToInt();
                    return this._defaultDirectory;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return this._defaultDirectory;
            }
        }

        /// <summary>
        /// Change le repertoire courrant 
        /// </summary>
        /// <param name="dirId"></param>
        public void SetDefaultMainDirectory(int dirId=0)
        {
            if (dirId == 0) return;

            string sql;
            sql = @"UPDATE directory SET active = 0";
            this.QueryMainDb(sql);

            sql = @"UPDATE directory SET active = 1 WHERE id = @dirId";
            Hashtable aParams = new Hashtable();
            aParams["dirId"] = dirId;
            if (this.QueryMainDb(sql, aParams))
            {
                this._defaultDirectory = -1;
                this._pathDirectory = String.Empty;
            }
        }

        private string _pathDirectory = String.Empty;

        /// <summary>
        /// Reccup du path directory dans main.db
        /// </summary>
        /// <returns></returns>
        public string GetPathDirectory()
        {
            if (String.IsNullOrEmpty(this._pathDirectory))
            {
                Hashtable res = this.GetDirectory(this.DIR_ID);
                if (res.Count > 0)
                {
                    string path = res["path"].ToString();
                    if (!path.EndsWith("\\"))
                    {
                        path += "\\";
                    }
                    this._pathDirectory = path;
                    return path;
                }
                else
                {
                    return String.Empty;
                }
            }
            else
            {
                return this._pathDirectory;
            }
        }

        /// <summary>
        /// Reccup les infos d'un repertoire
        /// </summary>
        /// <param name="dirId"></param>
        /// <returns></returns>
        public Hashtable GetDirectory(int dirId)
        {
            Hashtable aParams = new Hashtable();
            aParams["dirId"] = dirId;

            string sql = @"SELECT * FROM directory WHERE id = @dirId";
            List<Hashtable> res = this.SelectMainDb(sql, aParams);
            if (res.Count > 0)
            {
                return res[0];
            }
            else
            {
                return new Hashtable();
            }
        }
        
        /// <summary>
        /// Reccup la liste des repertoire disponibles
        /// </summary>
        /// <returns></returns>
        public List<Hashtable> GetDirectories()
        {
            string sql = @"SELECT * FROM directory ORDER BY name";
            return this.SelectMainDb(sql);
        }

        /// <summary>
        /// Reccup la liste des type de fichier disponibles
        /// </summary>
        /// <returns></returns>
        public List<Hashtable> GetFileType()
        {
            string sql = @"SELECT * FROM file_type ORDER BY id";
            return this.SelectMainDb(sql);
        }


        /// <summary>
        /// Reccup les extension du type ddéfinie
        /// </summary>
        /// <returns></returns>
        public List<string> GetMainExtensions(string type)
        {
            List<string> reslist = new List<string>();

            string sql = "SELECT * FROM file_type WHERE name = @type";
            Hashtable aParams = new Hashtable();
            aParams["type"] = type;

            List<Hashtable> res = this.SelectMainDb(sql, aParams);
            if (res.Count > 0)
            {
                return res[0]["extension"].ToString().RegSplitNoEmpty(",");

            }
            return reslist;
        }

        /// <summary>
        /// Reccup les extension du type Vidéo
        /// </summary>
        /// <returns></returns>
        public List<string> GetVideosExtensions()
        {
            return this.GetMainExtensions("Videos");
            /*
            List<string> reslist = new List<string>();

            string sql = "SELECT * FROM file_type WHERE name = 'Videos'";
            List < Hashtable > res =  this.SelectMainDb(sql);
            if (res.Count > 0)
            {
                return res[0]["extension"].ToString().RegSplitNoEmpty(",");

            }
            return reslist;
             * */
        }

        /// <summary>
        /// Requete sur la bdd principale
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="aParams"></param>
        /// <returns></returns>
        private bool QueryMainDb(string sql, Hashtable aParams = null)
        {
            try
            {
                using (SQLiteConnection c = new SQLiteConnection(this.MainDataBase))
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
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Requete select sur la bdd principale
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="aParams"></param>
        /// <returns></returns>
        private List<Hashtable> SelectMainDb(string sql, Hashtable aParams = null)
        {
            List<Hashtable> result = new List<Hashtable>();
            using (SQLiteConnection c = new SQLiteConnection(this.MainDataBase))
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
                                row[readerColumn[i]] = reader.GetValue(i);
                            }
                            result.Add(row);
                        }
                    }
                }
            }
            return result;
        }

    }
}
