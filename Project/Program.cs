using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Project
{
    /// <summary>
    /// This class should be built in accordance to the project description pdf file.
    ///
    /// The denotations in the project description: Cycles = c Cycles.Count = |C| CycleLength = |c|
    /// HyperCycle = C
    /// </summary>
    internal class Cycle
    {
        public Cycle(List<Message> messages)
        {
            this.CycleIndex = 0;
            this.CycleLength = 0.012f; // 12us while messages period is in ms
            this.HyperCycle = CalculateHyperCycle(messages);

            // C / |c|
            Cycles = new int[(int)(HyperCycle / CycleLength)];

            // Initialize the Cycles
            for (int i = 0; i < Cycles.Length; i++)
            {
                Cycles[i] = (int)(i * HyperCycle);
            }
        }

        public int CycleIndex { get; set; }
        public float CycleLength { get; }
        public int[] Cycles { get; }
        public long HyperCycle { get; }

        public static long CalculateHyperCycle(List<Message> messages)
        {
            List<Message> copy = messages.ToList();

            long lcm_of_array_elements = 1;
            int divisor = 2;

            while (true)
            {
                int counter = 0;
                bool divisible = false;
                for (int i = 0; i < copy.Count; i++)
                {
                    // lcm_of_array_elements (n1, n2, ... 0) = 0.
                    if (copy[i].Period == 0)
                    {
                        return 0;
                    }

                    if (copy[i].Period == 1)
                    {
                        counter++;
                    }

                    // Divide element_array by devisor if complete division i.e. without remainder
                    // then replace number with quotient; used for find next factor
                    if (copy[i].Period % divisor == 0)
                    {
                        divisible = true;
                        copy[i].Period = (uint)(copy[i].Period / divisor);
                    }
                }

                // If divisor able to completely divide any number from array multiply with
                // lcm_of_array_elements and store into lcm_of_array_elements and continue to same
                // divisor for next factor finding. else increment divisor
                if (divisible)
                {
                    lcm_of_array_elements = lcm_of_array_elements * divisor;
                }
                else
                {
                    divisor++;
                }

                // Check if all element_array is 1 indicate we found all factors and terminate while loop.
                if (counter == copy.Count)
                {
                    return lcm_of_array_elements;
                }
            }
        }
    }

    internal class Edge
    {
        public Edge(String Id, uint BW, uint PropDelay, Vertex Source, Vertex Destination)
        {
            this.Id = Id;
            this.BW = BW;
            this.PropDelay = PropDelay;
            this.Source = Source;
            this.Destination = Destination;

            Queue = new();
            QueueBackwards = new();

            // The following are only used if simulating a cycle
            BW_Cylce_Transfer_Capacity = 0;  // The BW that a edge can transmit each cycle S in section 4
            WC_Cycle_Delay = 0; // The maximum amount of delays that can occur D in section 4
            Latency = 0; // Represented by Alpha in Section 4.2
            BW_Consumption_Cycle = 0; // The consumed BW each cycle in section 4.2
        }

        public uint BW { get; set; }
        public uint BW_Consumption { get; set; }
        public long BW_Consumption_Cycle { get; set; }
        public uint BW_Cylce_Transfer_Capacity { get; set; }
        public Vertex Destination { get; }
        public String Id { get; }
        public int Latency { get; set; }
        public uint PropDelay { get; set; }
        public List<Message> Queue { get; set; }
        public List<Message> QueueBackwards { get; set; }
        public Vertex Source { get; }
        public uint WC_Cycle_Delay { get; set; }

        public static Edge GetEdgeFromVerticies(Vertex source, Vertex destination, List<Edge> edges)
        {
            foreach (Edge edge in edges)
            {
                if (String.Equals(edge.Source.Name, source.Name) && String.Equals(edge.Destination.Name, destination.Name) || String.Equals(edge.Source.Name, destination.Name) && String.Equals(edge.Destination.Name, source.Name))
                {
                    return edge;
                }
            }
            return null;
        }

        public void CalculateBW()
        {
            uint consumption = 0;

            foreach (Message message in Queue)
            {
                consumption += message.Size;
            }

            foreach (Message message in QueueBackwards)
            {
                consumption += message.Size;
            }

            BW_Consumption = consumption;
        }

        public void PrintEdge()
        {
            Console.WriteLine(Source.Name + " <=> " + Destination.Name);
        }

        public void PrintEdgeDetails()
        {
            Console.WriteLine("Edge ID= " + Id + " | " + Source.Name + " <=> " + Destination.Name + " | " + " Latency= " + Latency + " QueCount= " + Queue.Count + " BQueCount= " + QueueBackwards.Count + " BW= " + BW + " BWC= " + BW_Consumption);
        }
    }

    internal class Message
    {
        public Message(String name, Vertex source, Vertex destination, uint size, uint period, uint deadline)
        {
            this.Name = name;
            this.Source = source;
            this.Destination = destination;
            this.Size = size;
            this.Period = period;
            this.Deadline = deadline;

            Path = new();

            PossiblePaths = new();
            PossibleVertexPaths = new();

            Scheduled = false;

            E2E = 0;

            QueNumbers = new();
        }

        public bool Backwards { get; set; }
        public uint Deadline { get; }
        public Vertex Destination { get; }
        public long E2E { get; set; }
        public int id { get; set; }
        public String Name { get; }
        public List<Edge> Path { get; set; }
        public List<Vertex> PathVertex { get; set; }
        public uint Period { get; set; }
        public List<List<Edge>> PossiblePaths { get; set; }
        public List<List<Vertex>> PossibleVertexPaths { get; set; }
        public List<int> QueNumbers { get; set; }
        public bool Scheduled { get; set; }

        public uint Size { get; }

        public Vertex Source { get; }

        public static List<Message> GetIDMessages(List<Message> messages)
        {
            foreach (Message message in messages)
            {
                string numericName = new String(message.Name.Where(Char.IsDigit).ToArray());

                message.id = Int32.Parse(numericName);
            }

            return messages;
        }

        public void PathToVertexPath()
        {
            List<Vertex> vertices = new();

            foreach (Edge edge in Path)
            {
                vertices.Add(edge.Source);
                vertices.Add(edge.Destination);
            }

            vertices = vertices.Distinct().ToList();

            bool found = false;

            int index;

            // Loop through all the possible Vertex Paths
            foreach (List<Vertex> route in PossibleVertexPaths)
            {
                index = 0;

                if (vertices.Count == route.Count)
                {
                    foreach (Vertex vertex in route)
                    {
                        if (vertex == vertices.ElementAt(index))
                        {
                            found = true;
                        }
                        else
                        {
                            found = false;
                        }

                        index++;
                    }
                }

                if (found)
                {
                    PathVertex = route.ToList();
                    break;
                }
            }
        }

        public void PrintPath()
        {
            Console.WriteLine("Showing path for message: " + Name);
            Console.WriteLine("Message source to destination: " + Source.Name + " -> " + Destination.Name);
            Console.WriteLine("Total Edges: " + Path.Count);
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();

            foreach (Edge edge in Path)
            {
                Console.WriteLine(edge.Source.Name + " <=> " + edge.Destination.Name);
            }

            Console.WriteLine();
        }

        public void PrintPossiblePaths()
        {
            int count_route = 0;

            foreach (List<Edge> route in PossiblePaths)
            {
                count_route++;
                Console.WriteLine("Route #" + count_route);
                Console.WriteLine("Total Edges: " + route.Count);
                Console.WriteLine("Visited Edges: ");

                foreach (Edge edge in route)
                {
                    Console.WriteLine(edge.Source.Name + " <=> " + edge.Destination.Name);
                }

                Console.WriteLine();
            }
        }

        public void PrintPossibleVertexPaths()
        {
            int count_route = 0;

            foreach (List<Vertex> route in PossibleVertexPaths)
            {
                int count_vertex = 0;

                count_route++;
                Console.WriteLine("Route #" + count_route);
                Console.WriteLine("Total Steps: " + route.Count);
                Console.WriteLine("Visited Vertecies: ");

                foreach (Vertex vertex in route)
                {
                    count_vertex++;

                    if (count_vertex < route.Count)
                    {
                        Console.Write(vertex.Name + " -> ");
                    }
                    else
                    {
                        Console.Write(vertex.Name);
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        public void PrintVertexPath()
        {
            int count_vertex = 0;

            Console.WriteLine("Showing path for message: " + Name);
            Console.WriteLine("Message source to destination: " + Source.Name + " -> " + Destination.Name);
            Console.WriteLine("Total Steps: " + PathVertex.Count);
            Console.WriteLine("------------------------------------------");
            Console.WriteLine();

            foreach (Vertex vertex in PathVertex)
            {
                count_vertex++;

                if (count_vertex < PathVertex.Count)
                {
                    Console.Write(vertex.Name + " -> ");
                }
                else
                {
                    Console.Write(vertex.Name);
                }
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        public bool ReachedFinalDestination()
        {
            if (PathVertex.Count > 0
                && PathVertex.First() == Source
                && PathVertex.Last() == Destination)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetPath(List<Edge> path)
        {
            int index = 0;

            int number = 0;

            // Remove message from current path Queues
            if (Path.Count > 0)
            {
                foreach (Edge edge in Path)
                {
                    if (Backwards)
                    {
                        edge.QueueBackwards.Remove(this);
                    }
                    else
                    {
                        edge.Queue.Remove(this);
                    }

                    edge.CalculateBW();
                }
            }

            Path = path;
            PathToVertexPath();

            // Get QueNumbers index ready
            QueNumbers.Clear();
            for (int i = 0; i < path.Count; i++)
            {
                QueNumbers.Add(-1);
            }

            // Update the queues on each edge visited
            foreach (Edge edge in Path)
            {
                if (Backwards)
                {
                    edge.QueueBackwards.Add(this);
                    number = edge.QueueBackwards.IndexOf(this);
                }
                else
                {
                    edge.Queue.Add(this);
                    number = edge.Queue.IndexOf(this);
                }

                if (number == 0)
                {
                    QueNumbers[index] = 1;
                }
                else
                {
                    if (number > 3)
                    {
                        while (number > 3)
                        {
                            number -= 3;
                        }

                        QueNumbers[index] = number;
                    }
                    else
                    {
                        QueNumbers[index] = number;
                    }
                }

                index++;

                edge.CalculateBW(); // Update the BW_Consumption
            }
        }

        public void VertexPathsToEdgePaths(List<Edge> edges)
        {
            List<List<Edge>> possiblePaths = new();
            List<Edge> path;

            foreach (List<Vertex> route in PossibleVertexPaths)
            {
                path = new();

                for (int i = 0; i < route.Count; i++)
                {
                    if (i < route.Count - 1)
                    {
                        path.Add(Edge.GetEdgeFromVerticies(route.ElementAt(i), route.ElementAt(i + 1), edges));
                    }
                }

                possiblePaths.Add(path);
            }

            PossiblePaths = possiblePaths.ToList();
        }
    }

    internal class Program
    {
        public static void AssignNeighbors(Vertex vertex, List<Edge> edges)
        {
            List<Vertex> neighbors = new();
            List<Edge> edgeNeighbors = new();

            foreach (Edge edge in edges)
            {
                if (edge.Destination == vertex || edge.Source == vertex)
                {
                    edgeNeighbors.Add(edge);
                }
            }

            //Console.WriteLine("Vector " + vertex.Name + " neighbors: ");
            foreach (Edge edge in edgeNeighbors)
            {
                if (edge.Source != vertex)
                {
                    //Console.WriteLine(edge.Source.Name);
                    neighbors.Add(edge.Source);
                }

                if (edge.Destination != vertex)
                {
                    //Console.WriteLine(edge.Destination.Name);
                    neighbors.Add(edge.Destination);
                }
            }

            vertex.Neighbors = neighbors.ToList();
        }

        public static void FindAllRoutes(List<Message> messages, List<Edge> edges, List<Vertex> vertices)
        {
            int count_messges = 0;

            List<Vertex> visited;

            foreach (Message message in messages)
            {
                visited = new();
                visited.Add(message.Source);

                if (count_messges % 2 == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                //Console.WriteLine();
                //Console.WriteLine("Finding Routes for Message: " + message.Name);
                //Console.WriteLine("Message source to destination: " + message.Source.Name + " -> " + message.Destination.Name);
                //Console.WriteLine("------------------------------------------");

                List<Vertex> pathList = new List<Vertex>();

                // add source to path[]
                pathList.Add(message.Source);

                // Call recursive utility
                FindPossiblePaths(message.Source, message.Destination, visited, pathList, message);

                //message.PrintPossibleVertexPaths();

                message.VertexPathsToEdgePaths(edges);

                //message.PrintPossiblePaths();

                count_messges++;

                //Console.WriteLine("------------------------------------------");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        // Does not work gives wrong paths
        public static List<List<Edge>> Findpaths(Message message, List<Edge> edges)
        {
            // create a queue which stores the paths
            Queue<List<Vertex>> q = new();

            // path vector to store the current path
            List<List<Vertex>> paths = new();
            List<List<Edge>> pathsEdges = new();
            List<Vertex> path = new();
            path.Add(message.Source);
            q.Enqueue(path);

            while (q.Count != 0)
            {
                path = q.Dequeue();
                Vertex last = path[path.Count - 1];

                // if last vertex is the desired destination then print the path
                if (String.Equals(last, message.Destination))
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

                    if (String.Equals(edge.Source.Name, last.Name)) n = edge.Destination;
                    else if (String.Equals(edge.Destination.Name, last.Name)) n = edge.Source;

                    if (n != null && IsNotVisited(n, path))
                    {
                        List<Vertex> newpath = path;
                        newpath.Add(n);
                        q.Enqueue(newpath);
                    }
                }
            }

            return pathsEdges;
        }

        /// <summary>
        /// This is a recursive function which will iterate through the origin vertex neighbors
        /// until it finds the destination vertex. The Message sent in will be assigned its
        /// PossibleVertexPaths list
        /// </summary>
        /// <param name="origin"> The origin vertex of the message </param>
        /// <param name="destination"> The destination vertex of the message </param>
        /// <param name="visited">
        /// A list of visited vertex IMPORTANT INCLUDE THE MESSAGE ORIGIN already in there
        /// </param>
        /// <param name="localPathList"> A list that is developed as the method itterates </param>
        /// <param name="message"> The message that will have its PossibleVertexPaths list updated </param>
        public static void FindPossiblePaths(Vertex origin, Vertex destination, List<Vertex> visited, List<Vertex> localPathList, Message message)
        {
            if (origin.Equals(destination))
            {
                List<Vertex> possiblePath = new();

                // Console.WriteLine("Path found for " + message.Name + ": ");
                foreach (Vertex v in localPathList)
                {
                    possiblePath.Add(v);
                }

                message.PossibleVertexPaths.Add(possiblePath);

                return;
            }

            // Mark the current node
            visited.Add(origin);

            // Recur for all the vertices adjacent to current vertex
            foreach (Vertex vertex in origin.Neighbors)
            {
                if (!visited.Contains(vertex))
                {
                    // store current node in path[]
                    localPathList.Add(vertex);

                    FindPossiblePaths(vertex, destination, visited, localPathList, message);

                    // remove current node in path[]
                    localPathList.Remove(vertex);
                }
            }

            // Mark the current node
            visited.Remove(origin);
        }

        public static void GetMessageDirection(List<Message> messages, List<Edge> edges)
        {
            foreach (Message message in messages)
            {
                foreach (Edge edge in edges)
                {
                    // Flows that are forward will have the the edge as their source
                    if (edge.Source == message.Source)
                    {
                        message.Backwards = false;
                    }
                    else if (edge.Destination == message.Source)
                    {
                        message.Backwards = true;
                    }
                }
            }
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

        public static void MenuPrintSASolution(String TC, SimulatedAnnealing solution)
        {
            bool asking = true;

            Console.WriteLine("---------------------------------------");

            while (asking)
            {
                Console.WriteLine();
                Console.WriteLine("The available options are: ");
                Console.WriteLine("1) YES");
                Console.WriteLine("2) NO");

                Console.WriteLine();

                Console.Write("Please Select if you would like to save the solution as an XML: ");

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
                            XDocument doc = new XDocument(new XElement("Report"));

                            // Add Solution Element
                            doc.Root.Add(new XElement("Solution",
                                new XAttribute("Runtime", (solution.endTime - solution.startTime).TotalSeconds),
                                new XAttribute("MeanE2E", solution.BestMeanE2E),
                                new XAttribute("MeanBW", solution.BestMeanBW)
                                ));

                            foreach (Message message in solution.BestMessages)
                            {
                                // Create Message Node
                                XElement messageNode = new XElement("Message",
                                    new XAttribute("Name", message.Name),
                                    new XAttribute("maxE2E", message.E2E)
                                    );

                                int index = 0;

                                foreach (Edge edge in message.Path)
                                {
                                    XElement linkNode;

                                    Vertex source = message.PathVertex[index];
                                    Vertex destination = message.PathVertex[index + 1];

                                    // Create Link Node for Backward Messages
                                    linkNode = new XElement("Link",
                                        new XAttribute("Source", source.Name),
                                        new XAttribute("Destination", destination.Name),
                                        new XAttribute("Qnumber", message.QueNumbers[index])
                                        );

                                    messageNode.Add(linkNode);

                                    index++;
                                }

                                doc.Root.Add(messageNode);
                            }

                            // Save the xml with name "Report_TC#.xml"
                            doc.Save(Directory.GetCurrentDirectory() + "//Report_" + TC + ".xml");

                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("Document saved to: ");
                            Console.WriteLine(Directory.GetCurrentDirectory() + "\\Report_" + TC + ".xml");

                            asking = false; // Break the asking loop
                            break;
                        }

                    case 2:
                        {
                            Console.WriteLine();
                            Console.Write("No file saved!");

                            asking = false; // Break the asking loop
                            break;
                        }
                    default:
                        {
                            Console.Clear();
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine("Sorry please select one of the options by entering: 1 | 2");
                            Console.WriteLine("**********************************************************");
                            Console.WriteLine();

                            break;
                        }
                }
            }

            Console.WriteLine();
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

                string input;

                input = Console.ReadLine();
                while (!int.TryParse(input, out response))
                {
                    Console.WriteLine("Bad input");
                    input = Console.ReadLine();
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
                vertices.Add(new Vertex(vertexNode.Attributes["Name"].Value));
            }

            XmlNodeList edgeNodes = architectureNode.SelectNodes(".//Edge");

            foreach (XmlNode edgeNode in edgeNodes)
            {
                String id = edgeNode.Attributes["Id"].Value;
                uint BW = uint.Parse(edgeNode.Attributes["BW"].Value);
                uint propDelay = uint.Parse(edgeNode.Attributes["PropDelay"].Value);

                Vertex source = null;
                Vertex destination = null;

                // Loop through the vertecies to find the source and destination objects
                foreach (Vertex vertex in vertices)
                {
                    if (vertex.Name == edgeNode.Attributes["Source"].Value)
                    {
                        source = vertex;
                    }

                    if (vertex.Name == edgeNode.Attributes["Destination"].Value)
                    {
                        destination = vertex;
                    }
                }

                if (source == null || destination == null)
                {
                    Console.WriteLine("Source or Destination EMPTY for Edge: " + id);
                }

                edges.Add(new Edge(id, BW, propDelay, source, destination));
            }

            return (vertices, edges);
        }

        public static List<Message> ParseMessageXml(String path, List<Vertex> vertices)
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

                uint size = uint.Parse(messageNode.Attributes["Size"].Value);
                uint period = uint.Parse(messageNode.Attributes["Period"].Value);
                uint deadline = uint.Parse(messageNode.Attributes["Deadline"].Value);

                Vertex source = null;
                Vertex destination = null;

                // Loop through the vertecies to find the source and destination objects
                foreach (Vertex vertex in vertices)
                {
                    if (vertex.Name == messageNode.Attributes["Source"].Value)
                    {
                        source = vertex;
                    }

                    if (vertex.Name == messageNode.Attributes["Destination"].Value)
                    {
                        destination = vertex;
                    }
                }

                if (source == null || destination == null)
                {
                    Console.WriteLine("Source or Destination EMPTY for Message: " + name);
                }

                messages.Add(new Message(name, source, destination, size, period, deadline));
            }

            return messages;
        }

        public static List<Vertex> TraverseToDestination(Vertex origin, Vertex destination, List<Edge> edges, List<Vertex> visited)
        {
            List<Edge> neighbors = new();

            List<Vertex> visitedTemp = new();

            if (origin == destination)
            {
                return visited;
            }

            Console.WriteLine("*********************");
            Console.WriteLine("Finding Path: " + origin.Name + " -> " + destination.Name);
            Console.WriteLine("Visited Vertecies: ");
            foreach (Vertex vertex in visited)
            {
                Console.Write(vertex.Name + " ");
            }
            Console.WriteLine();
            Console.WriteLine();

            foreach (Edge edge in edges)
            {
                if (edge.Destination == origin || edge.Source == origin)
                {
                    neighbors.Add(edge);
                }
            }

            Console.WriteLine("Vector " + origin.Name + " neighbors: ");
            foreach (Edge edge in neighbors)
            {
                Console.WriteLine(edge.Destination.Name);
                Console.WriteLine(edge.Source.Name);
            }

            // No Path has been found print Error!
            ConsoleColor consoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: TraverseToDestination could not find a path from " + origin.Name + " -> " + destination.Name);
            Console.ForegroundColor = consoleColor;

            return visited;
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
                        ITERATION_BUFFER = 1234;
                        break;
                    }

                case "medium":
                    {
                        PATH += "\\Medium";
                        ITERATION_BUFFER = 952;
                        break;
                    }
                case "large":
                    {
                        PATH += "\\Large";
                        ITERATION_BUFFER = 523;
                        break;
                    }
                default:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: response from menuSize() not recognized!");
                        Console.ForegroundColor = ConsoleColor.White;

                        PATH += "\\Medium";
                        ITERATION_BUFFER = 1234;
                        break;
                    }
            }

            // Sets up the program by asking what TC folder to use.
            string TC = menuTopography(size);
            if (TC == "error")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: response from menuTopography() not recognized!");
                Console.ForegroundColor = ConsoleColor.White;
                return;
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
            (List<Vertex> vertices, List<Edge> edges) = ParseArchiteure(PATH + "Config.xml");
            List<Message> messages = ParseMessageXml(PATH + "Apps.xml", vertices);

            Cycle cycle = new(messages);

            Console.WriteLine();
            Console.WriteLine("HyperCycle: " + cycle.HyperCycle);

            foreach (Vertex vertex in vertices)
            {
                AssignNeighbors(vertex, edges);
            }

            FindAllRoutes(messages, edges, vertices);

            GetMessageDirection(messages, edges);

            // Sets up the program by asking what solver method to use.
            string MODE = menuSolution();
            switch (MODE)
            {
                case "Simmulated Annealing":
                    {
                        SimulatedAnnealing simulatedAnnealing = new SimulatedAnnealing(messages, edges, cycle, ITERATION_BUFFER);

                        simulatedAnnealing.Solve();

                        MenuPrintSASolution(TC, simulatedAnnealing);

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
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("ERROR: response from menuSolution() not recognized!");
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }
            }

            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }
    }

    internal class Vertex
    {
        public Vertex(String name)
        {
            this.Name = name;
            Neighbors = new();
        }

        public String Name { get; set; }
        public List<Vertex> Neighbors { get; set; }
    }
}