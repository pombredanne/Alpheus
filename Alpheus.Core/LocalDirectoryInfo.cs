﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpheus.IO
{
    public class LocalDirectoryInfo : IDirectoryInfo
    {
        #region Constructors
        public LocalDirectoryInfo(string dir_path)
        {
            this.directory = new DirectoryInfo(dir_path);
            this.PathSeparator = new string(Path.DirectorySeparatorChar, 1);
        }

        public LocalDirectoryInfo(DirectoryInfo dir) : this(dir.FullName)
        {
            this.directory = dir;
        }
        #endregion

        #region Public properties
        public string PathSeparator { get; private set; } = new string(Path.DirectorySeparatorChar, 1);

        public IEnvironment Environment { get; protected set; }

        public string Name
        {
            get
            {
                return this.directory.Name;
            }
        }

        public string FullName
        {
            get
            {
                return this.directory.FullName;
            }
        }

        public IDirectoryInfo Parent
        {
            get
            {
                return new LocalDirectoryInfo(this.directory.Parent);
            }
        }

        public IDirectoryInfo Root
        {
            get
            {
                return new LocalDirectoryInfo(this.directory.Root);
            }
        }

        public bool Exists
        {
            get
            {
                return this.directory.Exists;
            }
        }
        #endregion

        #region Public methods
        public IDirectoryInfo[] GetDirectories()
        {
            DirectoryInfo[] dirs = this.directory.GetDirectories();
            return dirs != null ? dirs.Select(d => new LocalDirectoryInfo(d)).ToArray() : null;

        }

        public IDirectoryInfo[] GetDirectories(string searchPattern)
        {
            DirectoryInfo[] dirs = this.directory.GetDirectories(searchPattern);
            return dirs != null ? dirs.Select(d => new LocalDirectoryInfo(d)).ToArray() : null;
           
        }

        public IDirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            DirectoryInfo[] dirs = this.directory.GetDirectories(searchPattern, searchOption);
            return dirs != null ? dirs.Select(d => new LocalDirectoryInfo(d)).ToArray() : null;
        }

        public IFileInfo[] GetFiles()
        {
            FileInfo[] files = this.directory.GetFiles();
            return files != null ? files.Select(f => new LocalFileInfo(f)).ToArray() : null;
        }

        public IFileInfo[] GetFiles(string searchPattern)
        {
            FileInfo[] files = this.directory.GetFiles(searchPattern);
            return files != null ? files.Select(f => new LocalFileInfo(f)).ToArray() : null;
        }

        public IFileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            FileInfo[] files = this.directory.GetFiles(searchPattern, searchOption);
            return files != null ? files.Select(f => new LocalFileInfo(f)).ToArray() : null;
        }
        #endregion

        #region Private fields
        private DirectoryInfo directory;
        #endregion

    }
}
