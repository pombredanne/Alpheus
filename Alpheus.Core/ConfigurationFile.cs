﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

using Sprache;

namespace Alpheus
{
    public abstract class ConfigurationFile<S, K> : IConfiguration, IConfigurationStatistics, IConfigurationFactory<S, K> where S : IConfigurationNode where K : IConfigurationNode
    {
        #region Abstract methods and properties
        public abstract ConfigurationTree<S, K> ParseTree(string t);
        public abstract Parser<ConfigurationTree<S, K>> Parser { get; }
        #endregion

        #region Public properties
        public string FilePath { get; private set; }
        public FileInfo File
        {
            get
            {
                return new FileInfo(this.FilePath);
            }

        }
        public string FileContents { get; private set; }
        public IOException LastIOException { get; private set; }
        public ConfigurationTree<S, K> ConfigurationTree { get; private set; }
        public XDocument XmlConfiguration
        {
            get
            {
                if (this.ConfigurationTree != null && this.ConfigurationTree.Xml != null)
                {
                    return this.ConfigurationTree.Xml;
                }
                else return null;
            }
        }
        public ParseException LastParseException { get; private set; }
        public Exception LastException { get; private set; }
        public bool ParseSucceded { get; private set; } = false;
        public List<Tuple<string, bool, ConfigurationFile<S, K>>> IncludeFiles { get; protected set; }

        public string FullFilePath
        {
            get
            {
                return this.File.FullName;
            }
        }
        public List<Tuple<string, bool, IConfigurationStatistics>> IncludeFilesStatus
        {
            get
            {
                if (this.IncludeFiles == null) return null;
                else
                {
                    return this.IncludeFiles.Select(f => new Tuple<string, bool, IConfigurationStatistics>(f.Item1, f.Item2, f.Item3 as IConfigurationStatistics)).ToList();
                }
            }
        }

        public virtual int? TotalIncludeFiles
        { 
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    return this.IncludeFiles?.Count();
                }
            }
        }

        public virtual int? IncludeFilesParsed
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    return this.IncludeFiles?.Count(i => i.Item2);
                }
            }
        }

        public virtual int? TotalFileTopLevelNodes
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    IEnumerable<XElement> top =
                        from e in this.XmlConfiguration.Root.Elements()
                        where e.Attribute("File").Value == this.File.Name
                        select e;
                    return top.Count();
                }
            }
        }

        public virtual int? TotalTopLevelNodes
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    return this.XmlConfiguration.Root.Elements().Count();
                }
            }
        }

        public virtual int? FirstLineParsed
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    IEnumerable<int> lines =
                        from r in this.XmlConfiguration.Root.Descendants()
                        where r.Attribute("File").Value == this.File.Name && r.Attribute("Line") != null
                        select Int32.Parse(r.Attribute("Line").Value);

                    return lines.Count() == 0 ? 0 : lines.Min();
                }
            }
        }

        public virtual int? LastLineParsed
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    IEnumerable<int> lines =
                        from r in this.XmlConfiguration.Root.Descendants()
                        where r.Attribute("File").Value == this.File.Name && r.Attribute("Line") != null
                        select Int32.Parse(r.Attribute("Line").Value);

                    return lines.Count() == 0 ? 0 : lines.Max();
                }
            }
        }

        public virtual int? TotalFileComments
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    IEnumerable<XElement> comments =
                        from r in this.XmlConfiguration.Root.Descendants()
                        where r.Attribute("File").Value == this.File.Name && r.Name.LocalName.Contains("Comment")
                        select r;
                    return comments.Count();
                }
            }
        }

        public virtual int? TotalComments
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    IEnumerable<XElement> comments =
                        from r in this.XmlConfiguration.Root.Descendants()
                        where r.Name.LocalName.Contains("Comment")
                        select r;
                    return comments.Count();
                }
            }
        }

        public virtual int? TotalKeys
        {
            get
            {
                if (!this.ParseSucceded)
                {
                    return null;
                }
                else
                {
                    IEnumerable<XElement> k =
                        from r in this.XmlConfiguration.Root.Descendants()
                        where r.Attribute("File").Value == this.File.Name && (r.DescendantsAndSelf("Arg") != null || r.DescendantsAndSelf("Value") != null)
                        select r;
                    return k.Count();
                }
            }
        }
        #endregion 

        #region Constructors
        public ConfigurationFile()
        {

            this.FilePath = "none";
        }

        public ConfigurationFile(string file_path, bool read_file = true, bool parse_file = true) : base()
        {
            this.FilePath = file_path;
            if (read_file)
            {
                if (!this.ReadFile()) return;
            }
            if (read_file && parse_file)
            {
                this.ParseFile();
            }

        }
        #endregion

        #region Public methods
        public virtual bool ReadFile()
        {
            if (!this.File.Exists)
            {
                return false;
            }
            try
            {
                using (StreamReader s = new StreamReader(this.File.OpenRead()))
                {
                    this.FileContents = s.ReadToEnd();
                    return true;
                }
            }
            catch (Exception e)
            {
                if (e is IOException)
                {
                    this.LastIOException = e as IOException;
                    this.LastException = e;
                }
                else
                {
                    this.LastException = e;
                }
                return false;
            }
        }

        public virtual void ParseFile()
        {
            if (this.File == null || !this.File.Exists)
            {
                throw new InvalidOperationException(string.Format("The file {0} does not exist.", this.FilePath));
            }
            if (string.IsNullOrEmpty(this.FileContents))
            {
                throw new InvalidOperationException(string.Format("The file contents for {0} have not been read.", this.FilePath));
            }
            try
            {
                this.ConfigurationTree = this.ParseTree(this.FileContents);
                if (this.ConfigurationTree != null && this.ConfigurationTree.Xml != null)
                {
                    this.ParseSucceded = true;
                }
                else
                {
                    this.ParseSucceded = false;
                }
            }
            catch (ParseException pe)
            {
                this.LastParseException = pe;
                this.LastException = pe;
                this.ParseSucceded = false;
            }

        }

        public virtual Dictionary<string, Tuple<bool, List<string>, string>> XPathEvaluate(List<string> expressions)
        {
            if (this.ParseSucceded)
            {
                return this.ConfigurationTree.XPathEvaluate(expressions);
            }
            else
            {
                throw new InvalidOperationException("Parsing configuration failed.");
            }
        }

        public virtual bool XPathEvaluate(string e, out List<string> result, out string message)
        {
            if (this.ParseSucceded)
            {
                return this.ConfigurationTree.XPathEvaluate(e, out result, out message);
            }
            else
            {
                throw new InvalidOperationException("Parsing configuration failed.");
            }
        }

        public virtual bool XPathEvaluate(string e, out XElement result, out string message)
        {
            if (this.ParseSucceded)
            {
                return this.ConfigurationTree.XPathEvaluate(e, out result, out message);
            }
            else
            {
                throw new InvalidOperationException("Parsing configuration failed.");
            }
        }
        #endregion

        #region Static methods
        public static string UnescapeSlash(string txt)
        {
            if (string.IsNullOrEmpty(txt)) { return txt; }
            StringBuilder retval = new StringBuilder(txt.Length);
            for (int ix = 0; ix < txt.Length;)
            {
                int jx = txt.IndexOf('\\', ix);
                if (jx < 0 || jx == txt.Length - 1) jx = txt.Length;
                retval.Append(txt, ix, jx - ix);
                if (jx >= txt.Length) break;
                switch (txt[jx + 1])
                {
                  
                    //case 'n': retval.Append('\n'); break;  // Line feed
                    //case 'r': retval.Append('\r'); break;  // Carriage return
                    //case 't': retval.Append('\t'); break;  // Tab
                    case '\\': retval.Append(""); break; // Don't escape
                    default:                                 // Unrecognized, copy as-is
                        retval.Append('\\').Append(txt[jx + 1]); break;
                }
                ix = jx + 2;
            }
            return retval.ToString();
        }
        #endregion
    }
}
