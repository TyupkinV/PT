using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Win32;

namespace PT.Models {
    public class LoadDataM {
        public void ReadFile() {
            OpenFileDialog openFileOFD = new OpenFileDialog {
                FileName = "Document",
                DefaultExt = ".txt",
                Filter = "Текстовый файл | *.txt"
            };
            if (openFileOFD.ShowDialog() == true) {
                StreamReader reader = new StreamReader(openFileOFD.OpenFile());
                int sumBus = Convert.ToInt32(reader.ReadLine());
                List<Bus> listBus = new List<Bus>();
                while (listBus.Count < sumBus) {
                    listBus.Add(new Bus());
                }

                CommonInfo.SumPoint = Convert.ToInt32(reader.ReadLine());

                string[] timesStart = reader.ReadLine().Split(' ');
                for(int i = 0; i < timesStart.Length; i++) { 
                     listBus[i].StartWaybill = DateTime.Parse(timesStart[i]);
                }

                var prices = from i in reader.ReadLine().Split(' ')
                             select Convert.ToInt32(i);
                prices = prices.ToList();
                for (int j = 0; j < prices.Count(); j++) {
                    listBus[j].Price = prices.ElementAt(j);
                }

                foreach(Bus bus in listBus) {
                    string[] line = reader.ReadLine().Split(' ');
                    int iter = Convert.ToInt32(line[0]);
                    Dictionary<string, int> tempWaybill = new Dictionary<string, int>();
                    for(int i = 1; i <= iter; i++) {
                        tempWaybill.Add(line[i], Convert.ToInt32(line[i + iter]));
                    }
                    bus.Waybill = tempWaybill;
                    int b = 0;
                }
            } 
        }

        class Bus {
            static public int CounterBus = 0;
            public int IDBus { get; set; }
            public Dictionary<string, int> Waybill { get; set; }
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
    }
}
