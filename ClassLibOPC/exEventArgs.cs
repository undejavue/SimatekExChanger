using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibOPC
{
    public class exEventArgs : EventArgs
    {
        private readonly string _message;
        public string message
        {
            get { return this._message; }
        }

        public exEventArgs(string text)
        {
            this._message = text;
        }


    }
}
