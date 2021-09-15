using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace exercise
{
    public struct Task {
        int id, deadline, period, wcet;
        public Task(int id, int deadline, int period, int wcet) {
            this.id = id;
            this.deadline = deadline;
            this.period = period;
            this.wcet = wcet;
        }
        public int getId() { return id; }
        public int getDeadline() { return deadline; }
        public int getPeriod() { return period; }
        public int getWCET() { return wcet; }
    }

    public struct Core {
        int mcp;
        int id;
        float WCETFactor;

        public Core (int mcp, int id, float WCETFactor) {
            this.mcp = mcp;
            this.id = id;
            this.WCETFactor = WCETFactor;
        }

        public int getId() {
            return id;
        }

        public float getWCETFactor() {
            return WCETFactor;
        }

        public int getMcp() { return mcp; }
    }

    public struct MCP {
        int id;
        List<Core> cores;

        public MCP(int id, List<Core> cores) {
            this.id = id;
            this.cores = cores;
        }

        public int getId() {
            return id;
        }
        public List<Core> getCores() {
            return cores;
        }
    }

    class Program
    {
        public static void randomAssign(Dictionary<Core, List<Task>> map, List<MCP> mCPs, List<Task> tasks) {
            foreach (var task in tasks) {
                var random = new Random();
                int mCPNum = random.Next(0, mCPs.Count);
                int coreNum = random.Next(0, mCPs[mCPNum].getCores().Count);
                List<Task> mappedTasks;
                if (!map.TryGetValue(mCPs[mCPNum].getCores()[coreNum], out mappedTasks)) {
                    map.Add(mCPs[mCPNum].getCores()[coreNum], new List<Task>{task});
                }else {
                    mappedTasks.Add(task);
                }
            }
        }

        public static void swap (Dictionary<Core, List<Task>> map, List<MCP> mcps) {
            var random = new Random();
            List<Task> tasks;
            do {
                tasks = map.ElementAt(random.Next(map.Count)).Value;
            } while (tasks.Count <= 0);
            Task task = tasks[random.Next(tasks.Count)];

            int mcpsNum = random.Next(0, mcps.Count);
            Core newCore;

            //while (first == second || map.ElementAt(first).Value.Equals(map.ElementAt(second).Value)) {first = random.Next(0, map.Count); second = random.Next(0, map.Count);}
            do {
                newCore = mcps[mcpsNum].getCores()[random.Next( mcps[mcpsNum].getCores().Count)];
            } while (map[newCore].Contains(task));
            tasks.Remove(task);
            //Console.WriteLine("Changing task {0} from core {0}, to {0}" ,map.ElementAt(task).Key.getId(), map.ElementAt(task).Value.getId(), mcps[mcpsNum].getCores()[core].getId());
            map[newCore].Add(task);
        }
        public static bool DM_guarantee(Dictionary<Core, List<Task>> map) {
            int R, Ci;
            foreach (var mapping in map) {
                for (int i = 0; i < mapping.Value.Count; i++) {
                    int I = 0;
                    do {
                        Ci = (int)(mapping.Value[i].getWCET() * mapping.Key.getWCETFactor());
                        R = I + Ci;
                        if (R > mapping.Value[i].getDeadline()) return false;
                        I = 0;
                        for (int j = 0; j < i; j++) {
                            decimal rTemp = (decimal)R/(decimal)mapping.Value[j].getPeriod();
                            int Cj = (int)(mapping.Value[j].getWCET() * mapping.Key.getWCETFactor());
                            I+= (int)Math.Ceiling(rTemp) * Cj;
                        }
                    } while (I + Ci > R);

                    
                }
            }

                
                /*
                do {
                    R = I + (int)(mapping.Key.getWCET() * mapping.Value.getWCETFactor());
                    if (R > mapping.Key.getDeadline()) return false;
                    I = 0;
                    for (int j = 0; j < i; j++) {
                        if (!mapping.Value.Equals(map.ElementAt(j).Value)) continue;
                        decimal rTemp = (decimal)R/(decimal)mapping.Key.getPeriod();
                        I+= (int)Math.Ceiling(rTemp) * (int)(map.ElementAt(j).Key.getWCET() * mapping.Value.getWCETFactor());
                    }
                } while (I + mapping.Key.getWCET() > R);
            }
            */
            return true;
        }

        static void Main(string[] args)
        {
            /** Load data  **/
            XmlDocument doc = new XmlDocument();
            
            doc.Load("Week_37-exercise\\test_cases\\medium.xml");
            List<Task> tasks = new List<Task>();
            tasks.Clear();
            var nodes = doc.SelectNodes("//Application");
            foreach (XmlNode node in nodes)
            {
                var aNodes = node.SelectNodes(".//Task");
                foreach (XmlNode aNode in aNodes)
                    tasks.Add(new Task(int.Parse(aNode.Attributes["Id"].Value), int.Parse(aNode.Attributes["Deadline"].Value), int.Parse(aNode.Attributes["Period"].Value), int.Parse(aNode.Attributes["WCET"].Value)));
                
            }
            List<MCP> mcps = new List<MCP>();
            List<Core> cores = new List<Core>();
            mcps.Clear();
            cores.Clear();
            nodes = doc.SelectNodes("//Platform");
            foreach (XmlNode node in nodes) {
                var mcpNodes = node.SelectNodes(".//MCP");
                foreach (XmlNode coreNodes in mcpNodes) {
                        var coreNode = coreNodes.SelectNodes(".//Core");
                    foreach (XmlNode naNode in coreNode)
                        cores.Add(new Core(int.Parse(coreNodes.Attributes["Id"].Value), int.Parse(naNode.Attributes["Id"].Value), float.Parse(naNode.Attributes["WCETFactor"].Value)));
                    mcps.Add(new MCP(int.Parse(coreNodes.Attributes["Id"].Value), new List<Core>(cores))); 
                    cores.Clear();
                }
            }
            

            /** Solve **/
            var map = new Dictionary<Core, List<Task>>();
            randomAssign(map, mcps, tasks);
            int iter = 0;
            
            while (!DM_guarantee(map)) {
                iter++;
                swap(map, mcps);
            }

            //TODO hill climbing optimization

            /** Print solution **/
            foreach (var entry in map)
                foreach (var task in entry.Value) {
                                    Console.WriteLine("Task id " + task.getId() + " mcp id " + entry.Key.getMcp() + " core id " + entry.Key.getId());

                }
        }
    }
}
