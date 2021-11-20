using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Vertex = System.String;

namespace Project
{
    internal class SimulatedAnnealing
    {
        private const int MAX_QUE_COUNT = 3;

        private Cycle Cycle;
        private List<Edge> edges;
        private int ITERATION_BUFFER;
        private List<Message> messages;
        private List<Vertex> vertices;

        public SimulatedAnnealing(List<Message> messages, List<Vertex> vertices, List<Edge> edges, int ITERATION_BUFFER, Cycle cycle)
        {
            this.messages = messages;
            this.vertices = vertices;
            this.edges = edges;
            this.ITERATION_BUFFER = ITERATION_BUFFER;
            this.Cycle = cycle;
        }

        public void Solve()
        {
            PrintFindingSolution(false);

            // Transform the problem into a cycle based system instead of time as defined in Section
            // 4 of project description
            CycleDomainTransformation(edges);

            // Get initial Arrival Pattern as defined in Section 4 of project description
            List<Message> arrivalPattern = CalculateArrivalPattern();

            int count = 0;

            bool passedLink = false;
            bool passedDead = false;

            // Find an initial solution by iterating ques and assigning message routes in cycles
            while (!passedLink || !passedDead)
            {
                PrintFindingSolution(false);

                // For an initial assignation for message routes
                messages = EariliestDeadlineFirst(arrivalPattern);

                MovePathLowestBW();

                count++;

                passedLink = LinkCapactiyConstraint();
                passedDead = DeadlineConstraint();
            }

            PrintFindingSolution(true);

            Console.WriteLine(count);
            return;

            // Simulate a Cycle
            CyclicQ(messages, edges);

            return;
        }

        private uint ArrivalPattern(Message message, int offset)
        {
            ConsoleColor consoleColor = Console.ForegroundColor;

            Console.WriteLine();
            Console.WriteLine("Finding A of Message " + message.Name + ": ");

            if ((offset * Cycle.CycleLength % message.Period) == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;

                Console.WriteLine("Result of " + offset + " * " + Cycle.CycleLength + " % " + message.Period + " == 0 was TRUE!");
                Console.WriteLine("A = " + (message.Size - offset));

                Console.ForegroundColor = consoleColor;

                return (uint)(message.Size);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Result of " + offset + " * " + Cycle.CycleLength + " % " + message.Period + " == 0 was FASLE!");
                Console.WriteLine("A = 0");

                Console.ForegroundColor = consoleColor;

                return 0;
            }
        }

        private List<Message> CalculateArrivalPattern()
        {
            // Defined as A in the project description
            LinkedList<Message> arrival_pattern = new();

            /*
            * si.c for c * |c| % si.t = 0
            *  0   for c * |c| % si.t != 0
            */
            foreach (Message message in messages)
            {
                bool arrived = false;

                Console.WriteLine("Finding Arrival for " + message.Name);

                if ((Cycle.Cycles[Cycle.CycleIndex] * Cycle.CycleLength % message.Period) == 0)
                {
                    Console.WriteLine("Result of " + Cycle.Cycles[Cycle.CycleIndex] + " * " + Cycle.CycleLength + " % " + message.Period + " == 0 was true!");

                    Message previous = arrival_pattern.FirstOrDefault();

                    if (previous != null)
                    {
                        // The order of arrival depends on the message size (si.c)
                        foreach (Message m in arrival_pattern)
                        {
                            if (message.Size < m.Size && message.Size >= previous.Size)
                            {
                                Console.WriteLine("Arrival found! " + message.Size + " < " + m.Size + " && " + message.Size + " >= " + previous.Size);

                                arrived = true;

                                arrival_pattern.AddAfter(arrival_pattern.Find(previous), message);
                                break;
                            }

                            previous = m;
                        }

                        if (!arrived)
                        {
                            Console.WriteLine("Arrival Last");
                            arrival_pattern.AddLast(message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Arrival Empty");
                        arrival_pattern.AddLast(message);
                    }
                }
                else
                {
                    Console.WriteLine("Arrival First");
                    arrival_pattern.AddFirst(message);
                }

                Console.WriteLine();
            }

            return arrival_pattern.ToList();
        }

        private void CycleDomainTransformation(List<Edge> edges)
        {
            foreach (Edge edge in edges)
            {
                // The capacity from using a cycle system instead of a time system Defined as S in
                // the project description S = s * |c|
                edge.BW_Cylce_Transfer_Capacity = (uint)(edge.BW * Cycle.CycleLength);

                // The induced delay from using a cycle system instead of a time system Defined as D
                // in the project description D = d / |c|
                edge.WC_Cycle_Delay = (uint)(edge.PropDelay / Cycle.CycleLength);
            }
        }

        private void CyclicQ(List<Message> messages, List<Edge> edges)
        {
            Cycle.CycleIndex++;

            if (Cycle.Cycles.Count() >= Cycle.CycleIndex)
            {
            }
            else
            {
                ConsoleColor console = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: No more cycles allowed in this solution! Max Cycles = " + Cycle.Cycles.Count());
                Console.ForegroundColor = console;
            }
        }

        private bool DeadlineConstraint()
        {
            bool passed = false;

            Console.WriteLine();

            ConsoleColor consoleColor = Console.ForegroundColor;

            foreach (Message message in messages)
            {
                long e2e = 0;

                if (message.Path.Count <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("FAILED DeadlineConstraint! ( Message " + message.Name + " has no Path!)");
                    Console.ForegroundColor = consoleColor;
                    return false;
                }

                foreach (Edge edge in message.Path)
                {
                    int q_index = 0;

                    if (edge.Queue.Count > 0)
                    {
                        q_index = edge.Queue.IndexOf(message);
                    }

                    e2e += edge.PropDelay + q_index;
                }

                message.E2E = e2e;

                // The condition of the Deadline Constraint as defined in section 4.1
                if (e2e <= message.Deadline)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Message " + message.Name + " PASSED DeadlineConstraint! ( " + e2e + " <= " + message.Deadline + " )");
                    Console.ForegroundColor = consoleColor;
                    passed = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Message " + message.Name + " FAILED DeadlineConstraint! ( " + e2e + " <= " + message.Deadline + " )");
                    Console.ForegroundColor = consoleColor;
                    return false;
                }
            }

            return passed;
        }

        private List<Message> EariliestDeadlineFirst(List<Message> messages)
        {
            List<Message> unscheduled = new();

            int scheduled = 0;

            int lowestQCount = 0;
            uint highestBW = 0;

            List<Edge> pathToAssign;

            List<Edge> priorityEdges;

            // Check which message have not been Scheduled
            foreach (Message message in messages)
            {
                if (!message.Scheduled)
                {
                    unscheduled.Add(message);
                }
                else
                {
                    scheduled++;
                }
            }

            Console.WriteLine("Scheduled Messages: " + scheduled);
            Console.WriteLine("Unscheduled Messages: " + (messages.Count - scheduled));
            Console.WriteLine();

            // Sort into ascending order
            unscheduled.Sort((x, y) => x.Deadline.CompareTo(y.Deadline)); // Sort based on deadline
            edges.Sort((x, y) => x.PropDelay.CompareTo(y.PropDelay)); // Sort based on PropDelay

            foreach (Message message in unscheduled)
            {
                pathToAssign = new();
                priorityEdges = new();

                ConsoleColor consoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("*********************************************");
                Console.WriteLine("Edges that should take priority for message " + message.Name);
                Console.WriteLine("Edges with BW_Cons <= " + highestBW);
                Console.WriteLine("Edges with Queues <= " + lowestQCount);
                Console.WriteLine();

                // Get the lowest Queue number
                lowestQCount = edges.Min(e => e.Queue.Count);
                highestBW = edges.Max(e => e.BW_Consumption);

                // Check which edges should take priority to be assigned a new message
                foreach (Edge edge in edges)
                {
                    if (edge.Queue.Count <= lowestQCount
                        && (edge.BW_Consumption + message.Size) < edge.BW
                        && (edge.Queue.Count + 1 <= MAX_QUE_COUNT)
                        && (edge.BW_Consumption) <= highestBW)
                    {
                        edge.PrintEdge();
                        priorityEdges.Add(edge);
                        break; // An edge has been found an thus a path using this edge will take priority
                    }
                }

                foreach (List<Edge> path in message.PossiblePaths)
                {
                    bool available = false;

                    foreach (Edge edge in path)
                    {
                        foreach (Edge priority in priorityEdges)
                        {
                            if (edge == priority)
                            {
                                available = true;
                                break;
                            }
                        }

                        if (available) break; // No need to keep checking
                    }

                    if (available)
                    {
                        message.SetPath(path);

                        // Update the queues on each edge visited
                        foreach (Edge edge in message.Path)
                        {
                            edge.Queue.Add(message);
                            edge.CalculateBW(); // Update the BW_Consumption
                        }

                        scheduled++;
                        message.Scheduled = true;

                        Console.WriteLine();
                        message.PrintPath();

                        break; // Once the message has been scheduled a path then move on to the next message
                    }
                }

                Console.WriteLine("*********************************************");
                Console.WriteLine();
                Console.ForegroundColor = consoleColor;
            }

            foreach (Edge edge in edges)
            {
                edge.PrintEdgeDetails();
            }
            Console.WriteLine();

            Console.WriteLine("Scheduled Messages: " + scheduled);
            Console.WriteLine("Unscheduled Messages: " + (messages.Count - scheduled));
            Console.WriteLine();

            return messages;
        }

        private bool LinkCapactiyConstraint()
        {
            bool passed = false;

            // Calculate Latency Represented as Alpha in section 4.2
            foreach (Edge edge in edges)
            {
                int latency = (int)edge.WC_Cycle_Delay;

                if (edge.Queue.Count > 0)
                {
                    int q_Num = 0;

                    foreach (Message message in edge.Queue)
                    {
                        q_Num++;
                        latency += ((int)(edge.WC_Cycle_Delay + q_Num));
                    }
                }

                Console.WriteLine("Latency for Edge " + edge.Id + ": " + latency);

                edge.Latency = latency;
            }

            Console.WriteLine();

            // Calculate Consumed BW in each cycle
            foreach (Edge edge in edges)
            {
                long cBW = 0;

                foreach (Message message in edge.Queue)
                {
                    cBW += ArrivalPattern(message, Cycle.Cycles[Cycle.CycleIndex] - edge.Latency);
                }

                edge.BW_Consumption_Cycle = cBW;

                Console.WriteLine("ConsumedBW for Edge " + edge.Id + ": " + cBW);
            }

            ConsoleColor consoleColor = Console.ForegroundColor;
            Console.WriteLine();

            foreach (Edge edge in edges)
            {
                if (edge.BW_Consumption_Cycle <= edge.BW_Cylce_Transfer_Capacity)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("Edge " + edge.Id + " PASSED LinkeCapacityConstraint! ( " + edge.BW_Consumption_Cycle + " <= " + edge.BW_Cylce_Transfer_Capacity + " )");
                    Console.ForegroundColor = consoleColor;
                    passed = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Edge " + edge.Id + " FAILED LinkeCapacityConstraint! ( " + edge.BW_Consumption_Cycle + " <= " + edge.BW_Cylce_Transfer_Capacity + " )");
                    Console.ForegroundColor = consoleColor;
                    passed = false;
                }

                if (!passed)
                {
                    break;
                }
            }

            return passed;
        }

        private void MovePathLowestBW()
        {
            bool moved = false;

            uint highestBW = edges.Max(e => e.BW_Consumption);
            uint lowestBW = edges.Min(e => e.BW_Consumption);

            List<Edge> pathToAssign = new();

            Edge lowestBWEdge = edges[0];
            Edge highestBWEdge = edges[0];

            Console.WriteLine();

            ConsoleColor consoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("*********************************************");
            Console.WriteLine("Edges that should take priority for the Swap");
            Console.WriteLine("Edges with BW_Cons = " + lowestBW);
            Console.WriteLine("Edges with BW_Cons = " + highestBW);
            Console.WriteLine();

            // Check which edges should take priority to be assigned a new message
            foreach (Edge edge in edges)
            {
                if (edge.BW_Consumption <= lowestBW)
                {
                    edge.PrintEdgeDetails();
                    lowestBWEdge = edge;
                }
                else if (edge.BW_Consumption >= highestBW)
                {
                    edge.PrintEdgeDetails();
                    highestBWEdge = edge;
                }
            }

            foreach (Message message in messages)
            {
                // Checks that it changes the path of a message that doesn't already have the lowest
                // BW edge in its path
                if (!message.Path.Contains(lowestBWEdge))
                {
                    // Finds the path that contains the lowest BW edge and not the highest Bw edge
                    foreach (List<Edge> path in message.PossiblePaths)
                    {
                        if (path.Contains(lowestBWEdge)
                            && !path.Contains(highestBWEdge)
                            && path != message.Path)
                        {
                            Console.WriteLine("Message " + message.Name + " changed Path from: ");
                            message.PrintPath();

                            message.Path = path;

                            Console.WriteLine();
                            Console.WriteLine("To: ");
                            message.PrintPath();

                            moved = true;

                            break;
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = consoleColor;
        }

        private void PrintFindingSolution(bool found)
        {
            ConsoleColor consoleColor = Console.ForegroundColor;

            Console.Clear();

            if (found)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Initial solution Found!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Finding an initial solution");
            }

            Console.ForegroundColor = consoleColor;

            Console.WriteLine();
        }
    }
}