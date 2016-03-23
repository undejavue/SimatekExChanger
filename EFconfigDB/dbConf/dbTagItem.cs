using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ClassLibOPC;

namespace EFlocalDB
{
    public class dbTagItem : mTag
    {
        public virtual dbServerItem srvID {get; set; }

        public dbTagItem ()
        {

        }

        public dbTagItem(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

        public dbTagItem(mTag tag)
        {
            this.Name = tag.Name;
            this.NameInDb = tag.NameInDb;
            this.onChange = tag.onChange;
            this.Path = tag.Path;
            this.Quality = tag.Quality;
            this.Value = string.Empty;
        }

    }
}
