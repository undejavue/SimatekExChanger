using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibGlobal
{
    public class gErrorEntity : Entity
    {
        private bool _isError;
        public bool isError
        {
            get
            {
                return _isError;
            }
            set
           {
                _isError = value;
                OnPropertyChanged(new PropertyChangedEventArgs("isError"));
            }
        }

        private string _message;
        public string message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged(new PropertyChangedEventArgs("message"));
            }
        }

        private int _code;
        public int code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                SetError();
                OnPropertyChanged(new PropertyChangedEventArgs("code"));
            }
        }


        public gErrorEntity()
        {

        }

        public gErrorEntity(int ErrorCode, string ErrorMessage)
        {
            newError(ErrorCode, ErrorMessage);
        }


        /// <summary>
        /// New error entity item
        /// </summary>
        /// <param name="isError">True if error need to be indicated</param>
        /// <param name="ErrorCode">Error code</param>
        /// <param name="ErrorMessage">Error message</param>
        public void newError(int ErrorCode, string ErrorMessage)
        {
            code = ErrorCode;
            message = ErrorMessage;
        }

        private void SetError()
        {
            if (code == 0)
                isError = false;
            else
                isError = true;
        }


    }
}
