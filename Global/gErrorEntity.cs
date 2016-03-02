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

        private string _code;
        public string code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                OnPropertyChanged(new PropertyChangedEventArgs("code"));
            }
        }

        public gErrorEntity()
        {

        }

        public gErrorEntity(bool isError, string ErrorCode, string ErrorMessage)
        {
            newError(isError, ErrorCode, ErrorMessage);
        }


        /// <summary>
        /// New error entity item
        /// </summary>
        /// <param name="isError">True if error need to be indicated</param>
        /// <param name="ErrorCode">Error code</param>
        /// <param name="ErrorMessage">Error message</param>
        public void newError(bool isError, string ErrorCode, string ErrorMessage)
        {
            this.isError = isError;
            code = ErrorCode;
            message = ErrorMessage;
        }


    }
}
