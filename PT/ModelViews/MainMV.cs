using System.Threading;
using System.Windows.Input;
using PT.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace PT.ModelViews {
    public class MainMV : INotifyPropertyChanged{

        public LoadDataM LoadDataMProp { get; private set; }
        public ICommand CommLoadFile { get; private set; }
        public ICommand CommFindPaths { get; private set; }
        private string _startPoint = "1";
        public string StartPoint { get { return _startPoint; } set {_startPoint = value; NotifyPropertyChanged(); } }
        private string _endPoint = "4";
        public string EndPoint { get { return _endPoint; } set { _endPoint = value; NotifyPropertyChanged(); } }
        private int _budgetPrice;
        public int BudgetPrice {
            get { return _budgetPrice; }
            set { _budgetPrice = value;  NotifyPropertyChanged(); }
        }
        private List<string> _budgetPath;
        public List<string> BudgetPath {
            get { return _budgetPath; }
            set { _budgetPath = value; NotifyPropertyChanged(); }
        }

        public MainMV() {
            LoadDataMProp = new LoadDataM(this);
            CommLoadFile = new ImplCommands(ReadFile);
            CommFindPaths = new ImplCommands(CheckFields, FindPaths);

        }

        private bool CheckFields(object obj) {
            if(StartPoint == string.Empty || EndPoint == string.Empty) {
                return false;
            }
            else {
                return true;
            }
        }

        private void FindPaths(object obj) {
            LoadDataMProp.MakeGraph();
            Thread threadLowBudgetPath = new Thread(FindBudgetPath);
            threadLowBudgetPath.Start();
            //Thread threadFastPath = new Thread(FindFastPath);
            //threadFastPath.Start();
        }

        private void FindFastPath() {
            throw new NotImplementedException();
        }

        private void FindBudgetPath() {
            Tuple<int, List<string>> result = LoadDataMProp.BudgetPath();
            BudgetPrice = result.Item1;
            BudgetPath = result.Item2; 
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
