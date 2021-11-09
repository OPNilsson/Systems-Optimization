using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
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
        public static List<List<Vertex>> Findpaths(Vertex src, Vertex dst, List<Edge> edges)
        {
            // create a queue which stores
            // the paths
            Queue<List<Vertex>> q = new();

            // path vector to store the current path
            List<List<Vertex>> paths = new();
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
                    paths.Add(path);

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
            return paths;
        }

        static void Main()
        {
            List<Message> messages = ParseMessageXml("..\\..\\..\\..\\..\\test_cases\\Small\\TC1\\Input\\Apps.xml");
            (List<Vertex> vertices, List<Edge> edges) = ParseArchiteure("..\\..\\..\\..\\..\\test_cases\\Small\\TC1\\Input\\Config.xml");

            CpModel model = new();


            List<List<Vertex>> paths = Findpaths("ES1", "ES2", edges);

            foreach (List<Vertex> path in paths)
            {
                foreach (Vertex v in path)
                {
                    Console.Write(v + " ");
                }
                Console.WriteLine();
            }
            //IntVar[] tasks = new IntVar[messages.Count];

            // TODO: create variables in ortools



            // TODO: create domain

            // TODO: create constraints

            // Find the correct solver



        }
    }
}
