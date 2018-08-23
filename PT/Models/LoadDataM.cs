using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;


namespace PT.Models {
    public class LoadDataM {

        public ModelViews.MainMV MainMVP { get; set; }
        public List<Bus> AllBus { get; set; } = new List<Bus>();
        public List<List<Tuple<int, int>>> AllPaths { get; set; } = new List<List<Tuple<int, int>>>();

        public LoadDataM(ModelViews.MainMV mainMV) {
            MainMVP = mainMV;
        }

        public void ReadFile() {
            OpenFileDialog openFileOFD = new OpenFileDialog {
                FileName = "Document",
                DefaultExt = ".txt",
                Filter = "Текстовый файл | *.txt"
            };
            if (openFileOFD.ShowDialog() == true) {
                StreamReader reader = new StreamReader(openFileOFD.OpenFile());
                int sumBus = Convert.ToInt32(reader.ReadLine());
                
                while (AllBus.Count < sumBus) {
                    AllBus.Add(new Bus());
                }

                CommonInfo.SumPoint = Convert.ToInt32(reader.ReadLine());

                string[] timesStart = reader.ReadLine().Split(' ');
                for(int i = 0; i < timesStart.Length; i++) { 
                     AllBus[i].StartWaybill = DateTime.Parse(timesStart[i]);
                }

                var prices = from i in reader.ReadLine().Split(' ')
                             select Convert.ToInt32(i);
                prices = prices.ToList();
                for (int j = 0; j < prices.Count(); j++) {
                    AllBus[j].Price = prices.ElementAt(j);
                }

                foreach(Bus bus in AllBus) {
                    string[] line = reader.ReadLine().Split(' ');
                    int iter = Convert.ToInt32(line[0]);
                    Dictionary<int, int> tempWaybill = new Dictionary<int, int>();
                    for(int i = 1; i <= iter; i++) {
                        tempWaybill.Add(Convert.ToInt32(line[i]) - 1, Convert.ToInt32(line[i + iter]));
                    }
                    bus.Waybill = tempWaybill;
                }
            } 
        }

        public void MakeGraph() {
            IList<int>[,] graph = new IList<int>[CommonInfo.SumPoint, CommonInfo.SumPoint];
            for(int i = 0; i < CommonInfo.SumPoint; i++) {
                for (int j = 0; j < CommonInfo.SumPoint; j++) {
                    graph[i, j] = new List<int>();
                }
            }

            foreach(Bus bus in AllBus) {
                List<int> allPoint = bus.Waybill.Keys.ToList();
                graph[allPoint[allPoint.Count - 1], allPoint[0]].Add(bus.IDBus);
                for(int i = 0; i < allPoint.Count - 1; i++) {
                    graph[allPoint[i], allPoint[i + 1]].Add(bus.IDBus);
                }
            }

            AllPath(graph, Convert.ToInt32(MainMVP.StartPoint) - 1, Convert.ToInt32(MainMVP.EndPoint) - 1, new List<Tuple<int, int>>(), Enumerable.Repeat(false, CommonInfo.SumPoint).ToList());

        }

        private void AllPath(IList<int>[,] graph, int startPoint, int arrivalPoint, List<Tuple<int, int>> path, List<bool> visited) {
            visited[startPoint] = true;
            if (startPoint == arrivalPoint) {
                AllPaths.Add(path);
                path.Clear();
                visited[arrivalPoint] = false;
                return;
            }
            for (int nextPoint = 0; nextPoint < CommonInfo.SumPoint; nextPoint++) {
                if ( !visited[nextPoint] && graph[startPoint, nextPoint].Count != 0) {
                    foreach (int nextBus in graph[startPoint, nextPoint]) {
                        path.Add(new Tuple<int, int>(nextBus, nextPoint));
                        Console.WriteLine("{0}-{1}", startPoint, nextPoint);
                        AllPath(graph, nextPoint, arrivalPoint, path, visited);
                    }
                }
            }
        }
    }
}


    public class Bus {
            static public int CounterBus = 0;
            public int IDBus { get; set; }
            public Dictionary<int, int> Waybill { get; set; }
            public DateTime StartWaybill { get; set; }
            public int Price { get; set; }
            public string NextStay { get; set; }
            public int TimeToNextStay { get; set; } = 0;

            public Bus() {
                IDBus = ++CounterBus;
            }

            public string GetNextStay() {
                return new NotImplementedException().ToString();
            }
        }

    static class CommonInfo {
            static public DateTime Time { get; set; } = new DateTime();
            static public int SumPoint { get; set; } = 0;
            static public int SumBus { get; set; } = 0;
        }

    class Passenger {
            public DateTime StartTrip { get; set; }
            public string StartPoint { get; set; }
            public string FinishPoint { get; set; }
        }


