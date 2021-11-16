using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Vertex = System.String;

namespace Project
{
    internal class Edge
    {
        public Edge(String Id, uint BW, uint PropDelay, String Source, String Destination)
        {
            this.Id = Id;
            this.BW = BW;
            this.PropDelay = PropDelay;
            this.Source = Source;
            this.Destination = Destination;
        }

        public uint BW { get; }
        public String Destination { get; }
        public String Id { get; }
        public uint PropDelay { get; }
        public String Source { get; }

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

    internal class Message
    {
        public Message(String name, String source, String destination, uint size, uint period, uint deadline)
        {
            this.Name = name;
            this.Source = source;
            this.Destination = destination;
            this.Size = size;
            this.Period = period;
            this.Deadline = deadline;
        }

        public uint Deadline { get; }
        public String Destination { get; }
        public String Name { get; }
        public uint Period { get; }
        public uint Size { get; }
        public String Source { get; }
    }

    internal class Program
    {
        // utility function for finding paths in graph from source to destination
        public static List<List<Edge>> Findpaths(Vertex src, Vertex dst, List<Edge> edges)
        {
            // create a queue which stores the paths
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

                // if last vertex is the desired destination then print the path
                if (String.Equals(last, dst))
                {
                    paths.Add(path);
                    List<Edge> pathEdges = new();
                    for (int i = 0; i < path.Count - 1; i++)    // create edge list of path
                    {
                        pathEdges.Add(Edge.GetEdgeFromVerticies(path[i], path[i + 1], edges));
                    }
                    pathsEdges.Add(pathEdges);
                }

                // traverse to all the nodes connected to current vertex and push new path to queue
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

        // utility function to check if current vertex is already present in path
        public static bool IsNotVisited(Vertex v, List<Vertex> path)
        {
            int size = path.Count;
            for (int i = 0; i < size; i++)
                if (String.Equals(path[i], v))
                    return false;
            return true;
        }

        public static string menuSize()
        {
            bool asking = true;

            while (asking)
            {
                Console.WriteLine("Welcome to our network queueing program!");
                Console.WriteLine("----------------------------------------");

                Console.WriteLine("The available sample sizes are: ");
                Console.WriteLine("1) small");
                Console.WriteLine("2) medium");
                Console.WriteLine("3) large");

                Console.WriteLine();

                Console.Write("Please Select the size of your Sample Use case: ");

                int response;
                {
                    string input;

                    input = Console.ReadLine();
                    while (!int.TryParse(input, out response))
                    {
                        Console.WriteLine("Bad input");
                        input = Console.ReadLine();
                    }
                }

                switch (response)
                {
                    case 1:
                        {
                            return "small";
                        }

                    case 2:
                        {
                            return "medium";
                        }
                    case 3:
                        {
                            return "large";
                        }
                    default:
                        {
                            Console.Clear();
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine("Sorry please select one of the options by entering: 1 - 3");
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine();

                            break;
                        }
                }
            }

            return "error";
        }

        public static string menuSolution()
        {
            bool asking = true;

            Console.WriteLine(); // Here for formatting

            while (asking)
            {
                Console.WriteLine("--------------------------------------------------------------------------------");

                Console.WriteLine("Available Solution Modes: ");
                Console.WriteLine("1) Simmulated Annealing");
                Console.WriteLine("2) Constraint Programming");

                Console.WriteLine();

                Console.Write("Please Select the Solution Mode you would like to use: ");

                int response;
                {
                    string input;

                    input = Console.ReadLine();
                    while (!int.TryParse(input, out response))
                    {
                        Console.WriteLine("Bad input");
                        input = Console.ReadLine();
                    }
                }

                switch (response)
                {
                    case 1:
                        {
                            return "Simmulated Annealing";
                        }

                    case 2:
                        {
                            return "Constraint Programming";
                        }
                    default:
                        {
                            Console.Clear();
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine("Sorry please select one of the options by entering: 1 - 2");
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine();

                            break;
                        }
                }
            }

            return "error";
        }

        public static string menuTopography(string size)
        {
            List<string> tcs = new List<string>();

            switch (size)
            {
                case "small":
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            string tc = "TC";

                            tc += (i + 1).ToString();

                            tcs.Add(tc);
                        }
                        break;
                    }

                case "medium":
                    {
                        for (int i = 3; i < 6; i++)
                        {
                            string tc = "TC";

                            tc += (i + 1).ToString();

                            tcs.Add(tc);
                        }
                        break;
                    }
                case "large":
                    {
                        for (int i = 6; i < 9; i++)
                        {
                            string tc = "TC";

                            tc += (i + 1).ToString();

                            tcs.Add(tc);
                        }
                        break;
                    }
            }

            bool asking = true;

            Console.WriteLine(); // Here for formatting

            while (asking)
            {
                Console.WriteLine("--------------------------------------------------------------------------------");

                Console.WriteLine("Available Topography Configurations based on sample size: ");
                Console.WriteLine("1) " + tcs[0]);
                Console.WriteLine("2) " + tcs[1]);
                Console.WriteLine("3) " + tcs[2]);

                Console.WriteLine();

                Console.Write("Please Select the Topography Configuration you would like to use: ");

                int response;
                {
                    string input;

                    input = Console.ReadLine();
                    while (!int.TryParse(input, out response))
                    {
                        Console.WriteLine("Bad input");
                        input = Console.ReadLine();
                    }
                }

                switch (response)
                {
                    case 1:
                        {
                            return tcs[0];
                        }

                    case 2:
                        {
                            return tcs[1];
                        }
                    case 3:
                        {
                            return tcs[2];
                        }
                    default:
                        {
                            Console.Clear();
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine("Sorry please select one of the options by entering: 1 - 3");
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine();

                            break;
                        }
                }
            }

            return "error";
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

        private static void Main()
        {
            string PATH = "..\\..\\..\\..\\..\\test_cases";

            int ITERATION_BUFFER = 0;

            // Sets up the program by asking if the user wants a small | medium | large sample size.
            string size = menuSize();
            switch (size)
            {
                case "small":
                    {
                        PATH += "\\Small";
                        ITERATION_BUFFER = 5555;
                        break;
                    }

                case "medium":
                    {
                        PATH += "\\Medium";
                        ITERATION_BUFFER = 1234;
                        break;
                    }
                case "large":
                    {
                        PATH += "\\Large";
                        ITERATION_BUFFER = 1486;
                        break;
                    }
                default:
                    {
                        Console.WriteLine("ERROR: response from menuSize() not recognized!");
                        PATH += "\\Medium";
                        ITERATION_BUFFER = 1234;
                        break;
                    }
            }

            // Sets up the program by asking what TC folder to use.
            string TC = menuTopography(size);
            if (TC == "error")
            {
                Console.WriteLine("ERROR: response from menuTopography() not recognized!");
            }
            else
            {
                PATH += "\\";
                PATH += TC;
                PATH += "\\Input\\";
            }

            Console.WriteLine();
            Console.WriteLine("Reading Inputs from: ");
            Console.WriteLine(PATH);

            // Reading from the XML
            List<Message> messages = ParseMessageXml(PATH + "Apps.xml");
            (List<Vertex> vertices, List<Edge> edges) = ParseArchiteure(PATH + "Config.xml");

            Dictionary<Message, List<List<Edge>>> message_routes = new Dictionary<Message, List<List<Edge>>>();
            foreach (Message message in messages)
            {
                message_routes.Add(message, Findpaths(message.Source, message.Destination, edges));       // create a list with all routes possible for every message
            }

            // Sets up the program by asking what solver method to use.
            string MODE = menuSolution();
            switch (MODE)
            {
                case "Simmulated Annealing":
                    {
                        SimulatedAnnealing simulatedAnnealing = new SimulatedAnnealing(messages, vertices, edges, message_routes);

                        simulatedAnnealing.Solve();

                        break;
                    }

                case "Constraint Programming":
                    {
                        ConstraintProgramming constraintProgramming = new ConstraintProgramming();

                        constraintProgramming.Solve();

                        break;
                    }
                default:
                    {
                        Console.WriteLine("ERROR: response from menuSolution() not recognized!");
                        PATH += "\\Medium";
                        ITERATION_BUFFER = 1234;
                        break;
                    }
            }
        }
    }
}