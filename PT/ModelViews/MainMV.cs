using System;
using System.Windows.Input;
using PT.Models;

namespace PT.ModelViews {
    public class MainMV {

        public LoadDataM LoadDataMProp { get; private set; }
        public ICommand CommLoadFile { get; private set; }

        public MainMV() {
            LoadDataMProp = new LoadDataM();
            CommLoadFile = new ImplCommands(ReadFile);

        }

        private void ReadFile(object obj) {
            LoadDataMProp.ReadFile();
        }
    }
}
