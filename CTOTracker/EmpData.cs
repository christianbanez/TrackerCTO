using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTOTracker
{
    public class EmpData : INotifyPropertyChanged
    {
        private string _inforID;
        public string InforID
        {
            get { return _inforID; }
            set
            {
                if (_inforID != value)
                {
                    _inforID = value;
                    OnPropertyChanged(nameof(InforID));
                }
            }
        }

        private string _fname;
        public string Fname
        {
            get { return _fname; }
            set
            {
                if (_fname != value)
                {
                    _fname = value;
                    OnPropertyChanged(nameof(Fname));
                }
            }
        }

        private string _lname;
        public string Lname
        {
            get { return _lname; }
            set
            {
                if (_lname != value)
                {
                    _lname = value;
                    OnPropertyChanged(nameof(Lname));
                }
            }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        private string _contact;
        public string Contact
        {
            get { return _contact; }
            set
            {
                if (_contact != value)
                {
                    _contact = value;
                    OnPropertyChanged(nameof(Contact));
                }
            }
        }

        private string _role;
        public string Role
        {
            get { return _role; }
            set
            {
                if (_role != value)
                {
                    _role = value;
                    OnPropertyChanged(nameof(Role));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
