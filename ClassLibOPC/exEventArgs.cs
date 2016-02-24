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

        private readonly int _error;
        public int error
        {
            get { return this._error; }
        }

        public exEventArgs(string messageText)
        {
            this._message = messageText;
            this._error = 0;
        }

        public exEventArgs(int errorCode, string messageText)
        {
            this._message = messageText;
            this._error = errorCode;
        }


    }
}
