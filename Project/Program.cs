using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Vertex = System.String;

namespace Project
{
    class Message
    {
        public String Name { get; }
        public String Source { get; }
        public String Destination { get; }
        public uint Size { get; }
        public uint Period { get; }
        public uint Deadline { get; }
        public Message(String name, String source, String destination, uint size, uint period, uint deadline)
        {
            this.Name = name;
            this.Source = source;
            this.Destination = destination;
            this.Size = size;
            this.Period = period;
            this.Deadline = deadline;
        }

    }

    class Edge
    {
        public String Id { get; }
        public uint BW { get; }
        public uint PropDelay { get; }
        public String Source { get; }
        public String Destination { get; }

        public Edge(String Id, uint BW, uint PropDelay, String Source, String Destination)
        {
            this.Id = Id;
            this.BW = BW;
            this.PropDelay = PropDelay;
            this.Source = Source;
            this.Destination = Destination;
        }

        public static Edge GetEdgeFromVerticies(Vertex source, Vertex destination, List<Edge> edges)
        {
            foreach (Edge edge in edges)
            {
                if (String.Equals(edge.Source, source) && String.Equals(edge.Destination, destination) || String.Equals(edge.Source, destination) && String.Equals(edge.Destination, source))
                {
                    return edge;
                }
            }
            return null;
        }
    }
     

    class Program
    {
        public static List<Message> ParseMessageXml(String path)
        {
            List<Message> messages = new();
            messages.Clear();

            XmlDocument doc = new();

            doc.Load(path);

            XmlNode applicationNode = doc.SelectSingleNode("//Application");

            XmlNodeList messageNodes = applicationNode.SelectNodes(".//Message");

            foreach (XmlNode messageNode in messageNodes)
            {
                String name = messageNode.Attributes["Name"].Value;
                String source = messageNode.Attributes["Source"].Value;
                String destination = messageNode.Attributes["Destination"].Value;
                uint size = uint.Parse(messageNode.Attributes["Size"].Value);
                uint period = uint.Parse(messageNode.Attributes["Period"].Value);
                uint deadline = uint.Parse(messageNode.Attributes["Deadline"].Value);

                messages.Add(new Message(name, source, destination, size, period, deadline));
            }


            return messages;
        }

        public static (List<Vertex>, List<Edge>) ParseArchiteure(String path)
        {
            List<Vertex> vertices = new();
            vertices.Clear();

            List<Edge> edges = new();
            edges.Clear();

            XmlDocument doc = new();

            doc.Load(path);

            XmlNode architectureNode = doc.SelectSingleNode("//Architecture");

            XmlNodeList vertexNodes = architectureNode.SelectNodes(".//Vertex");

            foreach (XmlNode vertexNode in vertexNodes)
            {
                vertices.Add(vertexNode.Attributes["Name"].Value);
            }

            XmlNodeList edgeNodes = architectureNode.SelectNodes(".//Edge");

            foreach (XmlNode edgeNode in edgeNodes)
            {
                String id = edgeNode.Attributes["Id"].Value;
                uint BW = uint.Parse(edgeNode.Attributes["BW"].Value);
                uint propDelay = uint.Parse(edgeNode.Attributes["PropDelay"].Value);
                String source = edgeNode.Attributes["Source"].Value;
                String destination = edgeNode.Attributes["Destination"].Value;

                edges.Add(new Edge(id, BW, propDelay, source, destination));
            }

            return (vertices, edges);
        }

        public static void printPath(List<Edge> path)
        {
            Console.Write(path[0].Source + ", " + path[0].Destination);
            for (int i = 1; i < path.Count; i++)
            {
                Console.Write(", " + path[i].Destination);
            }
            Console.WriteLine();
        }

        // utility function to check if current
        // vertex is already present in path
        public static bool IsNotVisited(Vertex v, List<Vertex> path)
        {
            int size = path.Count;
            for (int i = 0; i < size; i++)
                if (String.Equals(path[i], v))
                    return false;
            return true;
        }

        // utility function for finding paths in graph
        // from source to destination
        public static List<List<Edge>> Findpaths(Vertex src, Vertex dst, List<Edge> edges)
        {
            // create a queue which stores
            // the paths
            Queue<List<Vertex>> q = new();

            // path vector to store the current path
            List<List<Vertex>> paths = new();
            List<List<Edge>> pathsEdges = new();
            List<Vertex> path = new();
            path.Add(src);
            q.Enqueue(path);
            while (q.Count != 0)
            {
                path = q.Dequeue();
                Vertex last = path[path.Count - 1];

                // if last vertex is the desired destination
                // then print the path
                if (String.Equals(last,dst))
                {
                    paths.Add(path);
                    List<Edge> pathEdges = new();
                    for (int i = 0; i < path.Count - 1; i++)    // create edge list of path
                    {
                        pathEdges.Add(Edge.GetEdgeFromVerticies(path[i], path[i + 1], edges));
                    }
                    pathsEdges.Add(pathEdges);
                }
                    

                // traverse to all the nodes connected to 
                // current vertex and push new path to queue
                foreach (Edge edge in edges)
                {
                    Vertex n = null;
                    if (String.Equals(edge.Source, last)) n = edge.Destination;
                    else if (String.Equals(edge.Destination, last)) n = edge.Source;
                    if (n != null && IsNotVisited(n, path))
                    {

                        List<Vertex> newpath = new(path);
                        newpath.Add(n);
                        q.Enqueue(newpath);
                    }
                }
            }
            return pathsEdges;
        }

        static void Main()
        {
            List<Message> messages = ParseMessageXml("..\\..\\..\\..\\..\\test_cases\\Small\\TC1\\Input\\Apps.xml");
            (List<Vertex> vertices, List<Edge> edges) = ParseArchiteure("..\\..\\..\\..\\..\\test_cases\\Small\\TC1\\Input\\Config.xml");


            int[] mMessages = Enumerable.Range(0, messages.Count).ToArray();
            int[] mEdges = Enumerable.Range(0, edges.Count).ToArray();

            Dictionary<Edge, uint> curr_edge_bw = new Dictionary<Edge, uint>();
            foreach (var e in mEdges)
            {
                curr_edge_bw.Add(edges[e], 0);
            }

            Dictionary<Message, List<List<Edge>>> message_routes = new Dictionary<Message, List<List<Edge>>>();
            foreach (var m in mMessages)
            {
                message_routes.Add(messages[m], Findpaths(messages[m].Source, messages[m].Destination, edges));
            }

            Dictionary<Message, int> current_messages_routes = new Dictionary<Message, int>();
            Random rand = new();
            foreach (var m in messages)
            {
                current_messages_routes.Add(m, rand.Next(0, message_routes[m].Count - 1));
            }

            ulong iterations = 0;
            bool exceeded = false;
            while(++iterations < 10000000)
            {
                foreach(var m in messages)
                {
                    message_routes.TryGetValue(m, out List<List<Edge>> routes);     // Get all routes of a message
                    current_messages_routes.TryGetValue(m, out int current_route);          // Get the index of the route assigned to the message   
                    List<Edge> route = routes.ElementAt(current_route);             // Get current route assigned to a message
                    foreach (var e in route)
                    {
                        curr_edge_bw[e] += m.Size;                  //Update the bandwidth
                        if (curr_edge_bw[e] > e.BW)
                        {
                            Console.WriteLine("edge " + e.Id + " exceeded max bandwidth MAX: " + e.BW + ", CURRENT: " + curr_edge_bw[e]);
                            current_messages_routes[messages[rand.Next(0,messages.Count - 1)]] = rand.Next(0, message_routes[messages[rand.Next(0, messages.Count - 1)]].Count - 1);
                            curr_edge_bw.Clear();
                            foreach (var e1 in edges)
                            {
                                curr_edge_bw.Add(e1, 0);
                            }
                            exceeded = true;
                            break;
                        }
                    }
                }
                if (exceeded == false) break;
                else exceeded = false;

            }

            


            /*

            IntVar[] mEdgesBW = new IntVar[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                mEdgesBW[i] = model.NewIntVar(0, edges[i].BW, edges[i].Id + " BW");
            }

            IntVar[] mEdge_MAX_BW = new IntVar[edges.Count];
            for (int i = 0; i < edges.Count; i++)
            {
                mEdge_MAX_BW[i] = model.NewConstant(edges[i].BW, edges[i].Id);
            }


            IntVar[] mMessage_BW = new IntVar[messages.Count];
            for (int i = 0; i < messages.Count; i++)
            {
                mMessage_BW[i] = model.NewConstant(messages[i].Size, messages[i].Name + " Size");
            }

            IntVar[] messages_path = new IntVar[messages.Count];
            for (int i = 0; i < messages.Count; i++)
            {
                List<List<Vertex>> paths = Findpaths(messages[i].Source, messages[i].Destination, edges);
                Console.Write("Message " + messages[i].Name + " ");
                Console.WriteLine();
                foreach (var path in paths)
                {
                    printPath(path);
                    Console.WriteLine();
                }
                messages_path[i] = model.NewIntVar(0, paths.Count, messages[i].Name);
                //model.AddElement(messages_path[i], mEdgesBW, mMessage_BW[i]);
                
            }
            */

            /*
                        for (int i = 0; i < edges.Count; i++)
                        {
                            model.Add(mEdgesBW[i] < mEdge_MAX_BW[i]);
                        }
            */

//            CpSolver solver = new();
//            solver.Solve(model);
//            foreach (var path in messages_path) Console.Write(path.Name() + " " + solver.Value(path) + ", ");
//            Console.WriteLine();
//            foreach (var edgeBW in mEdgesBW) Console.Write(edgeBW.Name() + " " + solver.Value(edgeBW) + ", ");
            // TODO: create variables in ortools
            


            // TODO: create domain

            // TODO: create constraints

            // Find the correct solver



        }
    }
}
