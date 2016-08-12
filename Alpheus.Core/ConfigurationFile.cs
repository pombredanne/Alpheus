﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sprache;

namespace Alpheus
{
    public abstract class ConfigurationFile<S, K> : IConfigurationFactory<S, K> where S: IConfigurationNode where K: IConfigurationNode
    {
        #region Abstract methods and properties
        public abstract ConfigurationTree<S, K> ParseTree(string d);
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
        public ParseException LastParseException { get; private set; }
        public Exception LastException { get; private set; }
        public bool ParseSucceded { get; private set; }
        #endregion

        #region Constructors
        public ConfigurationFile() 
        {
            
            this.FilePath = "none";
        }

        public ConfigurationFile(string file_path, bool read_file = true, bool parse_file = true) : base()
        {
            this.FilePath = file_path;
            if (read_file) this.ReadFile();
            if (read_file && parse_file)
            {
                try
                {
                    this.ConfigurationTree = this.Parser.Parse(this.FileContents);
                    if (this.ConfigurationTree != null)
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

      
        #endregion
    }
}