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
        public List<Core> GetCores() {
            return cores;
        }
    }

    class Program
    {
        public static void randomAssign(Dictionary<Task, Core> map, List<MCP> mCPs, List<Task> tasks) {
            foreach (var task in tasks) {
                var random = new Random();
                int mCPNum = random.Next(0, mCPs.Count);
                int coreNum = random.Next(0, mCPs[mCPNum].GetCores().Count);
                map.Add(task, mCPs[mCPNum].GetCores()[coreNum]);
            }
        }

        public static void swap (Dictionary<Task, Core> map) {
            var random = new Random();
            int first = 0, second = 0;
            while (first == second || map.ElementAt(first).Value.Equals(map.ElementAt(second).Value)) {first = random.Next(0, map.Count); second = random.Next(0, map.Count);}
            //Console.WriteLine("Swapping " + first + " and " + second);
            var temp = map.ElementAt(first);
            map[map.ElementAt(first).Key] = map.ElementAt(second).Value;
            map[map.ElementAt(second).Key] = temp.Value;
        }
        public static bool DM_guarantee(Dictionary<Task, Core> map) {
            int R, i = 0;
            foreach (var mapping in map) {
                i++;
                int I = 0;
                do {
                    R = I + mapping.Key.getWCET();
                    if (R > mapping.Key.getDeadline()) return false;
                    for (int j = 0; j < i; j++) {
                        I+= (int)Math.Ceiling((decimal)R/mapping.Key.getPeriod())* map.ElementAt(j).Key.getWCET();
                    }
                } while (I + mapping.Key.getWCET() > R);
            }
            
            return true;
        }

        static void Main(string[] args)
        {
            /** Load data  **/
            var map = new Dictionary<Task, Core>();
            XmlDocument doc = new XmlDocument();
            
            doc.Load("Week_37-exercise\\test_cases\\small.xml");
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
            

            /** Are the tasks schelulable EDF D/P <= 1 EDF**/
            /** FP RTA **/

            /** Solve **/
            randomAssign(map, mcps, tasks);
            while (!DM_guarantee(map)) swap(map);

            foreach (var entry in map)
                Console.WriteLine("Task id " + entry.Key.getId() + " mcp id " + entry.Value.getMcp() + " core id " + entry.Value.getId());
        }
    }
}
