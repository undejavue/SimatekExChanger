using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibOracle
{

    public class oraEventArgs : EventArgs
    {
        private readonly string _message;
        public string message
        {
            get { return this._message; }
        }

        public oraEventArgs(string text)
        {
            this._message = text;
        }


    }
}
