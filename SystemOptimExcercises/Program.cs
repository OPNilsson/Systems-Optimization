using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace exercise
{
    public struct Task
    {
        int id, deadline, period, wcet;
        public Task(int id, int deadline, int period, int wcet)
        {
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

    public struct Core
    {
        int mcp;
        int id;
        float WCETFactor;

        public Core(int mcp, int id, float WCETFactor)
        {
            this.mcp = mcp;
            this.id = id;
            this.WCETFactor = WCETFactor;
        }

        public Core(Core core)
        {
            this.mcp = core.mcp;
            this.id = core.id;
            this.WCETFactor = core.WCETFactor;
        }

        public int getId()
        {
            return id;
        }

        public float getWCETFactor()
        {
            return WCETFactor;
        }

        public int getMcp() { return mcp; }
    }

    public struct MCP
    {
        int id;
        List<Core> cores;

        public MCP(int id, List<Core> cores)
        {
            this.id = id;
            this.cores = cores;
        }

        public int getId()
        {
            return id;
        }
        public List<Core> getCores()
        {
            return cores;
        }
    }

    class Program
    {
        public static void randomAssign(Dictionary<Core, List<Task>> map, List<MCP> mCPs, List<Task> tasks)
        {

            foreach(var mCp in mCPs)
            {
                foreach (var core in mCp.getCores())
                {
                    map.Add(core, new List<Task>());
                }
            }

            foreach (var task in tasks)
            {
                var random = new Random();
                int mCPNum = random.Next(0, mCPs.Count);
                int coreNum = random.Next(0, mCPs[mCPNum].getCores().Count);
               
                    map[mCPs[mCPNum].getCores()[coreNum]].Add(task);


            }
        }

        public static Dictionary<Core, List<Task>> generateSolution(Dictionary<Core, List<Task>> map, List<MCP> mcps)
        {
            var random = new Random();
            Dictionary<Core, List<Task>> mapP = new Dictionary<Core, List<Task>>();

            foreach (var entry in map)
            {
                mapP.Add(entry.Key, new List<Task>(entry.Value));
            }

            List<Task> tasks;
            do
            {
                tasks = mapP.ElementAt(random.Next(mapP.Count)).Value;
            } while (tasks.Count <= 0);
            Task task = tasks[random.Next(tasks.Count)];

            int mcpsNum = random.Next(0, mcps.Count);
            Core newCore;

            do
            {
                newCore = mcps[mcpsNum].getCores()[random.Next(mcps[mcpsNum].getCores().Count)];

                if(!mapP.ContainsKey(newCore))  mapP.Add(newCore, new List<Task>());

            } while (mapP[newCore].Contains(task));
            tasks.Remove(task);
            mapP[newCore].Add(task);

            return mapP;
        }
        public static bool DM_guarantee(Dictionary<Core, List<Task>> map)
        {
            int R, Ci;
            foreach (var mapping in map)
            {
                for (int i = 0; i < mapping.Value.Count; i++)
                {
                    int I = 0;
                    do
                    {
                        Ci = (int)(mapping.Value[i].getWCET() * mapping.Key.getWCETFactor());
                        R = I + Ci;
                        if (R > mapping.Value[i].getDeadline()) return false;
                        I = 0;
                        for (int j = 0; j < i; j++)
                        {
                            decimal rTemp = (decimal)R / (decimal)mapping.Value[j].getPeriod();
                            int Cj = (int)(mapping.Value[j].getWCET() * mapping.Key.getWCETFactor());
                            I += (int)Math.Ceiling(rTemp) * Cj;
                        }
                    } while (I + Ci > R);


                }
            }
            return true;
        }

        public static Dictionary<Core, List<Task>> compare_basic_criteria(Dictionary<Core, List<Task>> map, Dictionary<Core, List<Task>> mapP)
        {
            int t1 = 0;
            int R, Ci;
            bool addt = true;
            foreach (var mapping in map)
            {
                for (int i = 0; i < mapping.Value.Count; i++)
                {
                    int I = 0;
                    do
                    {
                        Ci = (int)(mapping.Value[i].getWCET() * mapping.Key.getWCETFactor());
                        R = I + Ci;
                        if (R > mapping.Value[i].getDeadline()) { addt = false; break; }
                        I = 0;
                        for (int j = 0; j < i; j++)
                        {
                            decimal rTemp = (decimal)R / (decimal)mapping.Value[j].getPeriod();
                            int Cj = (int)(mapping.Value[j].getWCET() * mapping.Key.getWCETFactor());
                            I += (int)Math.Ceiling(rTemp) * Cj;
                        }
                    } while (I + Ci > R);

                    if (addt == true) t1++;
                    addt = true;
                }
            }
            int t2 = 0;
            addt = true;
            foreach (var mapping in mapP)
            {
                for (int i = 0; i < mapping.Value.Count; i++)
                {
                    int I = 0;
                    do
                    {
                        Ci = (int)(mapping.Value[i].getWCET() * mapping.Key.getWCETFactor());
                        R = I + Ci;
                        if (R > mapping.Value[i].getDeadline()) { addt = false; break; }
                        I = 0;
                        for (int j = 0; j < i; j++)
                        {
                            decimal rTemp = (decimal)R / (decimal)mapping.Value[j].getPeriod();
                            int Cj = (int)(mapping.Value[j].getWCET() * mapping.Key.getWCETFactor());
                            I += (int)Math.Ceiling(rTemp) * Cj;
                        }
                    } while (I + Ci > R);

                    if (addt == true) t2++;
                    addt = true;
                }
            }

            return t1 >= t2 ? map : mapP;
        }

        static void Main(string[] args)
        {
            /** Load data  **/
            XmlDocument doc = new XmlDocument();

            doc.Load("../XML/medium.xml");
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
            foreach (XmlNode node in nodes)
            {
                var mcpNodes = node.SelectNodes(".//MCP");
                foreach (XmlNode coreNodes in mcpNodes)
                {
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

            do
            {
                iter++;
                var mapP = generateSolution(map, mcps);
                if (compare_basic_criteria(map, mapP) == mapP) map = mapP;

            } while (!DM_guarantee(map));

            //TODO hill climbing optimization

            /** Print solution **/
            Console.WriteLine("Run for {0} iterations", iter);
            foreach (var entry in map)
                foreach (var task in entry.Value)
                {
                    Console.WriteLine("Task id " + task.getId() + " mcp id " + entry.Key.getMcp() + " core id " + entry.Key.getId());
                }

            Console.ReadLine();
        }
    }
}
