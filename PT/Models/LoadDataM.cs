using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System.Windows;


namespace PT.Models {
    public class LoadDataM {

        public ModelViews.MainMV MainMVP { get; set; }
        public List<Bus> AllBus { get; set; } = new List<Bus>();
        public List<List<Tuple<int, int>>> AllPaths { get; set; } = new List<List<Tuple<int, int>>>();
        public int StartPointProp { get; set; }

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
            List<Edge> listAdj = new List<Edge>();
                foreach(Bus bus in AllBus) {
                    List<int> allPoint = bus.Waybill.Keys.ToList();
                    listAdj.Add(new Edge { StartV = allPoint[allPoint.Count - 1], EndV = allPoint[0], IDBus = bus.IDBus });
                    for(int i = 0; i < allPoint.Count - 1; i++) {
                        listAdj.Add(new Edge { StartV = allPoint[i], EndV = allPoint[i + 1], IDBus = bus.IDBus, Visited = false });
                    }
                }

            StartPointProp = Convert.ToInt32(MainMVP.StartPoint) - 1;
            AllPath(listAdj, StartPointProp, Convert.ToInt32(MainMVP.EndPoint) - 1, new List<Tuple<int, int>>());
            if(AllPaths.Count != 0) {
                BudgetPath();
            }
            else {
                MessageBox.Show("Маршрутов не найдено.");
            }
            
        }

        private void AllPath(List<Edge> graph, int startPoint, int arrivalPoint, List<Tuple<int, int>> path) {
            graph[startPoint].Visited = true;
            if (startPoint == arrivalPoint) {
                AllPaths.Add(new List<Tuple<int, int>>(path));
                Console.WriteLine("-");
                path.Clear();
                graph[startPoint].Visited = false;
                return;
            }
            foreach (Edge edge in graph) {
                if (edge.StartV == startPoint && !edge.Visited && edge.EndV != StartPointProp) {
                    path.Add(new Tuple<int, int>(edge.IDBus, edge.EndV));
                    Console.WriteLine("{0}-{1}", startPoint, edge.EndV);
                    AllPath(graph, edge.EndV, arrivalPoint, new List<Tuple<int, int>>(path));
                    path.RemoveAt(path.Count - 1);
                }
            }
        }
        private void BudgetPath() {
            int b = 0;
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

class Edge {
    public int StartV { get; set; }
    public int EndV { get; set; }
    public int IDBus { get; set; }
    public bool Visited { get; set; }
}



