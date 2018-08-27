using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;
using System.Windows;

namespace PT.Models {
    public class LoadDataM {
        #region complete
        public ModelViews.MainMV MainMVP { get; set; }
        public List<Bus> AllBus { get; set; }
        public List<List<Tuple<int, int>>> AllPaths { get; set; } = new List<List<Tuple<int, int>>>();
        public int StartPointProp { get; set; }
        public bool PathFound { get; set; }


        public LoadDataM(ModelViews.MainMV mainMV) {
            MainMVP = mainMV;
        }
        // Преобразование маршрута для вывода
        private List<string> ConvertPath(List<Tuple<int, int>> path) {
            List<Tuple<int, int>> temp = new List<Tuple<int, int>>(path);
            temp.Insert(0, new Tuple<int, int>(temp[0].Item1, StartPointProp));
            var sharedPath = temp.GroupBy(x => x.Item1);
            List<string> resultConvert = new List<string>();
            foreach (var bus in sharedPath) {
                resultConvert.Add("№" + Convert.ToString(bus.Key) + ": " + string.Join("-", bus.Select(x => x.Item2 + 1)));
            }
            return resultConvert;
        }
        // Чтение файла
        public void ReadFile() {
            OpenFileDialog openFileOFD = new OpenFileDialog {
                FileName = "Document",
                DefaultExt = ".txt",
                Filter = "Текстовый файл | *.txt"
            };
            if (openFileOFD.ShowDialog() == true) {
                StreamReader reader = new StreamReader(openFileOFD.OpenFile());
                int sumBus = Convert.ToInt32(reader.ReadLine());
                AllBus = new List<Bus>();
                while (AllBus.Count < sumBus) {
                    AllBus.Add(new Bus());
                }

                CommonInfo.SumPoint = Convert.ToInt32(reader.ReadLine());

                string[] timesStart = reader.ReadLine().Split(' ');
                for(int i = 0; i < timesStart.Length; i++) { 
                     AllBus[i].StartWaybill = DateTime.ParseExact("2000-01-01 " + timesStart[i], "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    
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
                    List<Tuple<int, int>> tempWaybill = new List<Tuple<int, int>> {
                        new Tuple<int, int>(Convert.ToInt32(line[1]) - 1, Convert.ToInt32(line[iter + iter]))
                    };
                    for(int i = 2; i <= iter; i++) {
                        tempWaybill.Add(new Tuple<int, int>(Convert.ToInt32(line[i]) - 1, Convert.ToInt32(line[i + iter - 1])));
                    }
                    bus.Waybill = tempWaybill;
                }
            } 
        }
        // Преобразование данных файла в графовую модель.
        public void MakeGraph() {
            List<Edge> listAdj = new List<Edge>();
            PathFound = false;
            int counter = 0;
                foreach(Bus bus in AllBus) {
                    List<int> allPoint = bus.Waybill.Select(x => x.Item1).ToList();
                    listAdj.Add(new Edge { StartV = allPoint[allPoint.Count - 1], EndV = allPoint[0], IDBus = bus.IDBus, IDEdge = counter });
                    counter++;
                    for(int i = 0; i < allPoint.Count - 1; i++) {
                        listAdj.Add(new Edge { StartV = allPoint[i], EndV = allPoint[i + 1], IDBus = bus.IDBus, IDEdge = counter});
                        counter++;
                    }
                }

            StartPointProp = Convert.ToInt32(MainMVP.StartPoint) - 1;
            AllPath(listAdj, StartPointProp, Convert.ToInt32(MainMVP.EndPoint) - 1, new List<Tuple<int, int>>(), Enumerable.Repeat(false, listAdj.Count).ToList());
            if(AllPaths.Count != 0) {
                BudgetPath();
            }
            else {
                MessageBox.Show("Маршрутов не найдено.");
            }
            
        }
        // Поиск всех путей
        private void AllPath(List<Edge> graph, int startPoint, int arrivalPoint, List<Tuple<int, int>> path, List<bool> visited) {
            if (startPoint == arrivalPoint) {
                AllPaths.Add(new List<Tuple<int, int>>(path));
                PathFound = true;
                //Console.WriteLine("Result path: {0}", string.Join("-", path));
                path.Clear();
                return;
            }
            foreach (Edge edge in graph) {
                //Console.WriteLine("All: {0}-{1}-[{2}]", edge.StartV, edge.EndV, edge.IDBus);
                List<Tuple<int, int>> test = new List<Tuple<int, int>>(path);
                if (edge.StartV == startPoint && !visited[edge.IDEdge] && path.Select(x => x).Where(x => x.Item2 == edge.EndV).Count() == 0 && edge.EndV != StartPointProp) {
                    visited[edge.IDEdge] = true;
                    path.Add(new Tuple<int, int>(edge.IDBus, edge.EndV));
                    //Console.WriteLine("Select: {0}-{1}-[{2}]", startPoint, edge.EndV, edge.IDBus);
                    AllPath(graph, edge.EndV, arrivalPoint, new List<Tuple<int, int>>(path), new List<bool>(visited));
                    path.RemoveAt(path.Count - 1);
                    //Console.WriteLine("Return to: {0}", string.Join("-", path));
                }
            }
        }
  
        // Поиск бюджетного пути
        public Tuple<int, List<string>> BudgetPath() {
            int poorPath = int.MaxValue;
            List<Tuple<int, int>> resultPath = new List<Tuple<int, int>>(); 
            foreach(List<Tuple<int, int>> path in AllPaths) {
                int sum = 0;
                var allBusPath = path.Select(x => x.Item1).GroupBy(x => x);
                foreach (var bus in allBusPath) {
                    sum += AllBus[bus.Key - 1].Price;
                }
                if (sum < poorPath) {
                    poorPath = sum;
                    resultPath = path;
                }
            }
            return new Tuple<int, List<string>>(poorPath, ConvertPath(resultPath));
        }

        #endregion
        // Поиск быстрейшего пути
        public Tuple<int, List<string>> FastPath(DateTime timePass, int startPoint) {
            List<Tuple<int, int>> resultPath = null;
            int minTime = int.MaxValue;


            foreach (List<Tuple<int, int>> path in AllPaths) {
                DateTime timeStart = timePass;
                int time = 0;
                int LastBus = path[0].Item1 - 1;
                int timeWait = AllBus[LastBus].WaitTime(timeStart, startPoint);
                time += timeWait;
                timeStart = timeStart.AddMinutes(timeWait);
                for (int i = 0; i < path.Count; i++) {
                    if (timeStart.AddMinutes(1).CompareTo(new DateTime(2000,1,2,0,0,0)) == 0) {
                        break;
                    }
                    if(path[i].Item1 - 1 == LastBus) {
                        int temp1 = AllBus[LastBus].Waybill.Find(x => x.Item1 == path[i].Item2).Item2;
                        timeStart = timeStart.AddMinutes(temp1);
                        time += temp1;
                    }
                    else {
                        LastBus = path[i].Item1 - 1;
                        timeWait = AllBus[LastBus].WaitTime(timeStart, path[i - 1].Item2);
                        time += timeWait;
                        int temp = AllBus[LastBus].Waybill.Find(x => x.Item1 == path[i].Item2).Item2;
                        time += temp;
                        timeStart = timeStart.AddMinutes(timeWait + temp);

                    }

                }
                if (time < minTime) {
                    resultPath = path;
                    minTime = time;
                }
            }

            if(minTime == int.MaxValue) {
                MessageBox.Show("Машрутов до 00:00 не найдено.");
                return new Tuple<int, List<string>>(minTime = -1, new List<string>());
            }
            return new Tuple<int, List<string>>(minTime, ConvertPath(resultPath));
        }
    }
}

public class Bus {
    #region var
    static public int CounterBus = 0;
    public int IDBus { get; set; }
    public List<Tuple<int, int>> Waybill { get; set; }
    public DateTime StartWaybill { get; set; }
    public int Price { get; set; }
    public int NextStay { get; set; } = -1;
    public int TimeToNextStay { get; set; } = -1;
    public DateTime LastTimeState { get; set; } = new DateTime(2000, 1, 1, 0, 0, 0);
    #endregion

    public Bus() {
        IDBus = ++CounterBus;
    }

    public int WaitTime(DateTime currTime, int point) {
        UpdateState(currTime);
        int timeWait;
        if(NextStay == -1) {
            timeWait = (int)(StartWaybill - currTime).TotalMinutes;
            if(Waybill[0].Item1 == point) {
                return timeWait;
            }
            else {
                int lenWay = Waybill.Count;
                NextStay = Waybill[0].Item1;
                for (int i = 0; i % lenWay < lenWay; i++) {
                    if (NextStay != point) {
                        timeWait += Waybill[i % lenWay].Item2;
                        NextStay = Waybill[i + 1 % lenWay].Item1;
                    }
                    else {
                        break;
                    }
                }
                return timeWait;
            }
        }

        if (NextStay == point) {
            return TimeToNextStay;
        }
        else {
            timeWait = TimeToNextStay;
            int t = Waybill.FindIndex(x => x.Item1 == point);
            int lenWay = Waybill.Count;
            for (int i = t; i % lenWay < lenWay; i++) {
                if (NextStay != point) {
                    timeWait += Waybill[i % lenWay].Item2;
                    NextStay = Waybill[i % lenWay].Item1;
                }
                else {
                    int b = 0;
                    break;
                }
            }
            return timeWait;
        }
    }

    private void UpdateState(DateTime currTime) {
        if (currTime.CompareTo(StartWaybill) > 0) {
            DateTime busTime = StartWaybill;
            int count;
            if(NextStay == -1) {
                count = 0;
            }
            else {
                count = Waybill.FindIndex(x => x.Item1 == NextStay);
            }
            List<int> allPoint = Waybill.Select(x => x.Item1).ToList();
            int lenWay = allPoint.Count;
            while (true) {
                DateTime temp = busTime.AddMinutes(Waybill[(count + 1) % lenWay].Item2);
                if (temp.CompareTo(currTime) > 0) {
                    TimeToNextStay = (temp - currTime).Minutes;
                    NextStay = allPoint[count % lenWay];
                    break;
                }
                else if(temp.CompareTo(currTime) == 0) {
                    TimeToNextStay = 0;
                    NextStay = allPoint[(count - 1) % lenWay];
                    busTime = temp;
                    break;
                }
                else {
                    busTime = temp;
                    count++;
                }
            }
            return;
        }
        else {
            return;
        }
    }
}

static class CommonInfo {
    static public DateTime Time { get; set; } = new DateTime();
    static public int SumPoint { get; set; } = 0;
    static public int SumBus { get; set; } = 0;
}

class Edge {
    public int StartV { get; set; }
    public int EndV { get; set; }
    public int IDBus { get; set; }
    public int IDEdge { get; set; }
}



