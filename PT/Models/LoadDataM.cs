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
                resultConvert.Add("№" + Convert.ToString(bus.Key + 1) + ": " + string.Join("-", bus.Select(x => x.Item2 + 1)));
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
                Bus.CounterBus = 0;
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
        }
        // Поиск всех путей
        private void AllPath(List<Edge> graph, int startPoint, int arrivalPoint, List<Tuple<int, int>> path, List<bool> visited) {
            if (startPoint == arrivalPoint) {
                AllPaths.Add(new List<Tuple<int, int>>(path));
                PathFound = true;
                path.Clear();
                return;
            }
            foreach (Edge edge in graph) {
                List<Tuple<int, int>> test = new List<Tuple<int, int>>(path);
                if (edge.StartV == startPoint && !visited[edge.IDEdge] && path.Select(x => x).Where(x => x.Item2 == edge.EndV).Count() == 0 && edge.EndV != StartPointProp) {
                    visited[edge.IDEdge] = true;
                    path.Add(new Tuple<int, int>(edge.IDBus, edge.EndV));
                    AllPath(graph, edge.EndV, arrivalPoint, new List<Tuple<int, int>>(path), new List<bool>(visited));
                    path.RemoveAt(path.Count - 1);
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
                    sum += AllBus[bus.Key].Price;
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
            List<Tuple<int, int>> resultPath = new List<Tuple<int, int>>();
            int minTime = int.MaxValue;
            DateTime midnightTime = new DateTime(2000, 1, 2, 0, 0, 0);

            foreach (List<Tuple<int, int>> path in AllPaths) {
                List<Bus> tempAllBus = Extensions.Clone(AllBus).ToList();
                DateTime currTime = timePass;
                int time = tempAllBus[path[0].Item1].WaitTime(currTime, startPoint); 
                currTime = currTime.AddMinutes(time);
                int lastBus = path[0].Item1;
                bool outTime = false;

                for (int i = 0; i < path.Count; i++) {
                    int timeWait = 0, timeRide = 0;
                    int indexNextPoint;
                    if (path[i].Item1 == lastBus) {
                        indexNextPoint = tempAllBus[lastBus].Waybill.FindIndex(x => x.Item1 == path[i].Item2);
                        timeRide = tempAllBus[lastBus].Waybill[indexNextPoint].Item2;
                    }
                    else {
                        lastBus = path[i].Item1;
                        indexNextPoint = tempAllBus[lastBus].Waybill.FindIndex(x => x.Item1 == path[i].Item2);
                        timeWait = tempAllBus[lastBus].WaitTime(currTime, path[i - 1].Item2);
                        timeRide = tempAllBus[lastBus].Waybill[indexNextPoint].Item2;
                    }
                    time += (timeWait + timeRide);
                    if (currTime.AddMinutes(timeWait + timeRide).CompareTo(midnightTime) >= 0) {
                        outTime = true;
                        break;
                    }

                    currTime = currTime.AddMinutes(timeWait + timeRide);
                }
                if (outTime) {
                    break;
                }
                else if (time <= minTime) {
                    resultPath = path;
                    minTime = time;
                }
            }
            if(resultPath.Count == 0) {
                MessageBox.Show("Маршрутов до 00:00 не найдено.");
            }
            return new Tuple<int, List<string>>(minTime, ConvertPath(resultPath));
        }
    }
}

static class Extensions {
    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }
}

public class Bus : ICloneable {
    #region vars
    static public int CounterBus = 0;
    public int IDBus { get; set; }
    public List<Tuple<int, int>> Waybill { get; set; }
    public DateTime StartWaybill { get; set; }
    public int Price { get; set; }
    public int NextStay { get; set; } = -1;
    public int TimeToNextStay { get; set; } = 0;
    public DateTime LastTimeState { get; set; } = new DateTime(1, 1, 1, 0, 0, 0);
    #endregion

    public Bus() {
        IDBus = CounterBus++;
    }

    public int WaitTime(DateTime currTime, int point) {
        UpdateState(currTime);

        int timeWait = TimeToNextStay;
        if (NextStay == point) {
            return timeWait;
        }
        else {
            int indexNextPoint = Waybill.FindIndex(x => x.Item1 == NextStay) + 1;
            int lenWay = Waybill.Count;
            while (NextStay != point) {
                timeWait += Waybill[indexNextPoint % lenWay].Item2;
                NextStay = Waybill[indexNextPoint % lenWay].Item1;
                indexNextPoint++;
            }
            LastTimeState = LastTimeState.AddMinutes(timeWait);
            TimeToNextStay = 0;
            return timeWait;
        }
    }

    private void UpdateState(DateTime currTime) {
        if(currTime.CompareTo(StartWaybill) <= 0) {
            NextStay = Waybill[0].Item1;
            TimeToNextStay = (int)(StartWaybill - currTime).TotalMinutes;
            LastTimeState = currTime;
            return;
        }

        DateTime busTime;
        int indexNextPoint;
        if (LastTimeState.Year != 1) {
            busTime = LastTimeState;
            busTime = busTime.AddMinutes(TimeToNextStay);
            indexNextPoint = Waybill.FindIndex(x => x.Item1 == NextStay);
        }
        else {
            busTime = StartWaybill;
            indexNextPoint = 0;
            NextStay = Waybill[0].Item1;
            TimeToNextStay = 0;
        } 

        int lenWay = Waybill.Count;

        while (busTime.CompareTo(currTime) < 0) {
            busTime = busTime.AddMinutes(Waybill[(indexNextPoint + 1) % lenWay].Item2);
            indexNextPoint++;
        }
        NextStay = Waybill[indexNextPoint % lenWay].Item1;
        TimeToNextStay = (int)(busTime - currTime).TotalMinutes;
        LastTimeState = currTime;
    }

    public object Clone() {
        return new Bus {
            IDBus = this.IDBus,
            LastTimeState = this.LastTimeState,
            Waybill = this.Waybill,
            TimeToNextStay = this.TimeToNextStay,
            NextStay = this.NextStay,
            Price = this.Price,
            StartWaybill = this.StartWaybill
        };
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



