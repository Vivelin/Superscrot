using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTP
{
    /// <summary>
    /// Holds information about Unix filesystem permissions.
    /// </summary>
    public struct UnixFilePermissions
    {
        /// <summary>The filetype (file, directory or other)</summary>
        public UnixFileTypes FileType;

        /// <summary>Permissions for the owner of the file.</summary>
        public Triplet User;

        /// <summary>Permissions for others in the group that owns the file.</summary>
        public Triplet Group;

        /// <summary>Permissions for everyone else.</summary>
        public Triplet Other;

        /// <summary>
        /// Creates a new Unix file permissions object parsed from the permission string.
        /// </summary>
        /// <param name="permissions">A string of length 10 that holds Unix filesystem permissions data.</param>
        public UnixFilePermissions(string permissions)
        {
            if (permissions != null && permissions.Length >= 10)
            {
                //Parse file type bit
                switch (permissions[0])
                {
                    case '-': FileType = UnixFileTypes.Regular; break;
                    case 'd': FileType = UnixFileTypes.Directory; break;
                    case 'l': FileType = UnixFileTypes.SymbolicLink; break;
                    case 'p': FileType = UnixFileTypes.NamedPipe; break;
                    case 's': FileType = UnixFileTypes.DomainSocket; break;
                    case 'b': FileType = UnixFileTypes.BlockSpecialFile; break;
                    case 'c': FileType = UnixFileTypes.CharacterSpecialFile; break;
                    case 'D': FileType = UnixFileTypes.Door; break;
                    default: FileType = UnixFileTypes.Unknown; break;
                }

                User = new Triplet(permissions[1], permissions[2], permissions[3]);
                Group = new Triplet(permissions[4], permissions[5], permissions[6]);
                Other = new Triplet(permissions[7], permissions[8], permissions[9]);
            }
            else
            {
                FileType = UnixFileTypes.Unknown;
                User = new Triplet();
                Group = new Triplet();
                Other = new Triplet();
            }
        }
    }

    /// <summary>
    /// Holds the part of Unix filesystem permission data that determines the
    /// permissions for either the owner, his group or others.
    /// </summary>
    public struct Triplet
    {
        public bool Read;      //r
        public bool Write;     //w
        public bool Execute;   //x or s or t (not S or T)

        public Triplet(char r, char w, char x)
        {
            Read = (r == 'r');
            Write = (w == 'w');
            Execute = (x == 'x' || x == 's' || x == 't');
        }
    }

    /// <summary>
    /// Unix file types bits (http://en.wikipedia.org/wiki/Unix_file_types)
    /// </summary>
    public enum UnixFileTypes
    {
        /// <summary>Regular files, denoted by a '-'.</summary>
        Regular,

        /// <summary>Directories, denoted by a 'd'.</summary>
        Directory,

        /// <summary>Symbolic links, denoted by an 'l'.</summary>
        SymbolicLink,

        /// <summary>Named pipe, denoted by a 'p'.</summary>
        NamedPipe,

        /// <summary>Unix domain socket, denoted by an 's'.</summary>
        DomainSocket,

        /// <summary>Block device file, denoted by a 'b'.</summary>
        BlockSpecialFile,

        /// <summary>Character device file, denoted by a 'c'.</summary>
        CharacterSpecialFile,

        /// <summary>Doors, denoted by a 'D'.</summary>
        Door,

        /// <summary>An unrecognized invalid file type, or empty.</summary>
        Unknown
    }
}
