using System;
using System.Windows.Input;
using PT.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PT.ModelViews {
    public class MainMV : INotifyPropertyChanged{

        public LoadDataM LoadDataMProp { get; private set; }
        public ICommand CommLoadFile { get; private set; }
        public ICommand CommFindPaths { get; private set; }
        private string _startPoint = "1";
        public string StartPoint { get { return _startPoint; } set {_startPoint = value; NotifyPropertyChanged(); } }
        private string _endPoint = "4";
        public string EndPoint { get { return _endPoint; } set { _endPoint = value; NotifyPropertyChanged(); } }


        public MainMV() {
            LoadDataMProp = new LoadDataM(this);
            CommLoadFile = new ImplCommands(ReadFile);
            CommFindPaths = new ImplCommands(CheckFields, FindPath);

        }

        private bool CheckFields(object obj) {
            if(StartPoint == string.Empty || EndPoint == string.Empty) {
                return false;
            }
            else {
                return true;
            }
        }

        private void FindPath(object obj) {
            LoadDataMProp.MakeGraph();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void ReadFile(object obj) {
            LoadDataMProp.ReadFile();
        }
    }
}
