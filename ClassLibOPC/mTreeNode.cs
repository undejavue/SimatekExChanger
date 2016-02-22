using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClassLibOPC
{
    public class mTreeNode : Entity
    {

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Description"));
            }
        }


        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Path"));
            }
        }

        private ObservableCollection<mTreeNode> _nodes;
        public ObservableCollection<mTreeNode> Nodes
        {
            get { return _nodes; }
            set
            {
                _nodes = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Nodes"));
            }
        }
 


        public mTreeNode ()
        {
            _nodes = new ObservableCollection<mTreeNode>();
        }

    }
}
