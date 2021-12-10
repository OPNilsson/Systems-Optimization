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
        public const int MAX_ITERATIONS = 100000;
        public const int MAX_QUE_COUNT = 3;
        public const double MIN_TEMPERATURE = 0.0001;
        public Cycle BestCycle;
        public List<Edge> BestEdges;
        public long BestMeanBW = 0;
        public long BestMeanE2E = 0;
        public List<Message> BestMessages;
        public DateTime endTime;
        public DateTime startTime;
        public bool TASK_SOLVED;
        private double coolingRate = 0.732;
        private Cycle Cycle;
        private bool debug = false;

        private List<Edge> edges;

        private int iteration = 0;
        private int ITERATION_BUFFER;
        private List<Message> messages;
        private double temperature = 1000.0; // As suggested in the lecture slides

        public SimulatedAnnealing(List<Message> messages, List<Edge> edges, Cycle cycle, int ITERATION_BUFFER)
        {
            this.messages = messages;
            this.edges = edges;
            this.ITERATION_BUFFER = ITERATION_BUFFER;
            this.Cycle = cycle;

            TASK_SOLVED = false;
        }

        public static double acceptingProbability(long MBW, long adjMBW, double temperature)
        {
            if (adjMBW > MBW)
                return 1.0;
            else
                return Math.Exp((MBW - adjMBW) / temperature);
        }

        public void ChangePath()
        {
            // Changes the path of a message
            //MovePathLowestBW();

            PickNewPath();

            // If not all messages reach deadline try and simulate another que cycle
            while (!DeadlineConstraint() || !LinkCapactiyConstraint() || !QueMax())
            {
                // Print a dot to show that the iteration is working
                //if (Cycle.CycleIndex % 100 == 0) Console.Write(" .");

                // Simulate a Cycle
                CyclicQ();

                if (Cycle.Cycles.Count() <= Cycle.CycleIndex) break;
            }
        }

        public void Solve()
        {
            startTime = DateTime.Now;

            // Save Originals to come up with a new solution during Algorithim execution
            List<Message> OriginalMessages = messages;
            List<Edge> OriginalEdges = edges;
            Cycle OriginalCycle = Cycle;

            PrintFindingSolution(false);

            // Transform the problem into a cycle based system instead of time as defined in Section
            // 4 of project description
            CycleDomainTransformation();

            // Get initial Arrival Pattern as defined in Section 4 of project description
            List<Message> arrivalPattern = CalculateArrivalPattern(messages, edges, Cycle);

            // Find an initial solution by iterating ques and assigning message routes in cycles
            while (!LinkCapactiyConstraint() || !DeadlineConstraint() || !AllMessagesScheduled())
            {
                PrintFindingSolution(false);

                // For an initial assignation for message routes
                messages = EariliestDeadlineFirst(arrivalPattern);

                // Simulate a Cycle
                CyclicQ();
            }

            PrintFindingSolution(true);

            Console.WriteLine("Cycles for solution: " + Cycle.CycleIndex);

            // Assign Best Solution
            BestMessages = messages;
            BestEdges = edges;
            BestCycle = Cycle;

            // Assign Best Objective Variable
            BestMeanBW = CalculateMeanBW();

            // Simmulated Annealing Variables
            long meanBW = BestMeanBW;
            int solutionsFound = 0;
            var random = new Random();

            // Start of Simmulated Annealing Algorithim
            while (iteration < MAX_ITERATIONS && temperature > MIN_TEMPERATURE)
            {
                // Adds a buffering effect to know if computing
                if (iteration % ITERATION_BUFFER == 0) { Console.Clear(); Console.WriteLine("Current iteration: " + iteration); }

                // Move a message with the highest BW to a route with the lowest BW
                ChangePath();

                // Uses the constraints for a solution as described in sections 4.0-4.2 of the
                // project description
                if (LinkCapactiyConstraint() && DeadlineConstraint() && QueMax())
                {
                    ++solutionsFound;

                    // Compute laxity of adjacent state.
                    long meanBW_Neighbor = CalculateMeanBW();

                    // Check if new solution is new best. A solution is described to be better in
                    // 4.3 of the project description if the meanBW is lower
                    if (meanBW_Neighbor < BestMeanBW)
                    {
                        // Assign Best Solution
                        BestMessages = messages;
                        BestEdges = edges;
                        BestCycle = Cycle;

                        BestMeanBW = meanBW_Neighbor;
                    }

                    // If neighbor solution is better, accept solution. Otherwise accept with
                    // varying probability.
                    double p = random.NextDouble();
                    if (acceptingProbability(meanBW, meanBW_Neighbor, temperature) > p)
                    {
                        meanBW = meanBW_Neighbor;
                    }

                    // End Iteration by cooling
                    temperature = temperature * coolingRate; // TODO: Make the cooling schedule not fixed?
                }
                else
                {
                    Cycle = OriginalCycle;
                }

                ++iteration;
            }

            BestMeanE2E = CalculateMeanE2E();

            endTime = DateTime.Now;

            Console.Clear();

            // Check why the simulated annealing ended
            if (temperature < MIN_TEMPERATURE)
            {
                Console.WriteLine("Temperature fell below accepted MIN! Ending Simulated Annealing!");
                Console.WriteLine("Temperature: " + temperature);
                Console.WriteLine("MIN TEMP:    " + MIN_TEMPERATURE);
                Console.WriteLine();
            }
            else if (iteration == MAX_ITERATIONS)
            {
                Console.WriteLine("Iteration Limit Hit! Ending Simulated Annealing!");
                Console.WriteLine("Iteration:       " + iteration);
                Console.WriteLine("Max Iterations:  " + MAX_ITERATIONS);
                Console.WriteLine();
            }

            // Give some statistics
            Console.WriteLine("Ran for " + (endTime - startTime).TotalMilliseconds + " ms");
            Console.WriteLine("Ran for {0} iterations", iteration);
            Console.WriteLine("Total Solutions found: " + solutionsFound);

            Console.WriteLine();

            // Sort the messages in order of name
            Message.GetIDMessages(BestMessages);
            BestMessages.Sort((x, y) => x.id.CompareTo(y.id));

            foreach (Message message in BestMessages)
            {
                message.PathToVertexPath();
                message.PrintVertexPath();
            }

            foreach (Edge edge in BestEdges)
            {
                while (edge.BW_Consumption > edge.BW)
                {
                    CyclicQ();
                }

                edge.PrintEdgeDetails();
            }

            Console.WriteLine();
            Console.WriteLine("Solution Cycles: " + BestCycle.CycleIndex);

            TASK_SOLVED = true;
        }

        private bool AllMessagesScheduled()
        {
            ConsoleColor consoleColor = Console.ForegroundColor;

            foreach (Message message in messages)
            {
                if (!message.Scheduled)
                {
                    if (debug) Console.ForegroundColor = ConsoleColor.DarkRed;
                    if (debug) Console.WriteLine("Message " + message.Name + " NOT Scheduled!");
                    if (debug) Console.ForegroundColor = consoleColor;

                    return false;
                }
            }

            if (debug) Console.ForegroundColor = ConsoleColor.DarkGreen;

            if (debug) Console.WriteLine("All Messages Scheduled!");

            if (debug) Console.ForegroundColor = consoleColor;

            return true;
        }

        private uint ArrivalPattern(Message message, int offset)
        {
            ConsoleColor consoleColor = Console.ForegroundColor;

            if (debug) Console.WriteLine();
            if (debug) Console.WriteLine("Finding A of Message " + message.Name + ": ");

            float x = ((Cycle.CycleIndex - offset) * Cycle.CycleLength) % message.Period;

            if (0 <= x && x < Cycle.CycleLength)
            {
                if (debug) Console.ForegroundColor = ConsoleColor.DarkGreen;

                if (debug) Console.WriteLine("((" + Cycle.CycleIndex + " - " + offset + ") " + " * " + Cycle.CycleLength + " ) % " + message.Period + " = " + x);
                if (debug) Console.WriteLine("0  <= " + x + " < " + Cycle.CycleLength + " TRUE!");
                if (debug) Console.WriteLine("A = " + message.Size);

                if (debug) Console.ForegroundColor = consoleColor;

                return message.Size;
            }
            else
            {
                if (debug) Console.ForegroundColor = ConsoleColor.DarkRed;

                if (debug) Console.WriteLine("((" + Cycle.CycleIndex + " - " + offset + ") " + " * " + Cycle.CycleLength + " ) % " + message.Period + " = " + x);
                if (debug) Console.WriteLine("0  <= " + x + " < " + Cycle.CycleLength + " FASLE!");
                if (debug) Console.WriteLine("A = 0");

                if (debug) Console.ForegroundColor = consoleColor;

                return 0;
            }
        }

        private List<Message> CalculateArrivalPattern(List<Message> messages, List<Edge> edges, Cycle cycle)
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

                if ((cycle.Cycles[cycle.CycleIndex] * cycle.CycleLength % message.Period) == 0)
                {
                    Console.WriteLine("Result of " + cycle.Cycles[cycle.CycleIndex] + " * " + cycle.CycleLength + " % " + message.Period + " == 0 was true!");

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

        private long CalculateMeanBW()
        {
            long totalBW = 0;
            List<long> consumedBW = new();

            foreach (Edge edge in edges)
            {
                long x = (edge.BW / edge.BW_Cylce_Transfer_Capacity);

                consumedBW.Add(x);
            }

            foreach (long x in consumedBW)
            {
                totalBW += x;
            }

            totalBW *= 3;

            return totalBW / edges.Count;
        }

        private long CalculateMeanE2E()
        {
            long totalE2E = 0;

            foreach (Message message in BestMessages)
            {
                totalE2E += message.E2E;
            }

            return totalE2E / BestMessages.Count;
        }

        private void CycleDomainTransformation()
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

        private void CyclicQ()
        {
            Cycle.CycleIndex++;
            iteration++;

            if (Cycle.Cycles.Count() >= Cycle.CycleIndex)
            {
                foreach (Edge edge in edges)
                {
                    if (edge.Queue.Count > 0)
                    {
                        edge.Queue.Remove(edge.Queue[0]);

                        edge.CalculateBW(); // Update the BW_Consumption

                        break;
                    }

                    if (edge.QueueBackwards.Count > 0)
                    {
                        edge.QueueBackwards.Remove(edge.QueueBackwards[0]);

                        edge.CalculateBW(); // Update the BW_Consumption

                        break;
                    }
                }
            }
            else
            {
                ConsoleColor console = Console.ForegroundColor;
                if (debug) Console.ForegroundColor = ConsoleColor.Red;
                if (debug) Console.WriteLine("Error: No more cycles allowed in this solution! Max Cycles = " + Cycle.Cycles.Count());
                if (debug) Console.ForegroundColor = console;
            }
        }

        private bool DeadlineConstraint()
        {
            bool passed = false;

            if (debug) Console.WriteLine();

            ConsoleColor consoleColor = Console.ForegroundColor;

            foreach (Message message in messages)
            {
                long e2e = 0;

                if (message.Path.Count <= 0)
                {
                    if (debug) Console.ForegroundColor = ConsoleColor.DarkRed;
                    if (debug) Console.WriteLine("FAILED DeadlineConstraint! ( Message " + message.Name + " has no Path!)");
                    if (debug) Console.ForegroundColor = consoleColor;
                    return false;
                }

                foreach (Edge edge in message.Path)
                {
                    int q_index = 0;

                    // Has to check which direction Queue to check
                    if (message.Backwards)
                    {
                        if (edge.QueueBackwards.Count > 0)
                        {
                            q_index = edge.QueueBackwards.IndexOf(message);
                        }
                    }
                    else
                    {
                        if (edge.Queue.Count > 0)
                        {
                            q_index = edge.Queue.IndexOf(message);
                        }
                    }

                    e2e += edge.PropDelay + q_index;
                }

                message.E2E = e2e;

                // The condition of the Deadline Constraint as defined in section 4.1
                if (e2e <= message.Deadline)
                {
                    if (debug) Console.ForegroundColor = ConsoleColor.DarkGreen;
                    if (debug) Console.WriteLine("Message " + message.Name + " PASSED DeadlineConstraint! ( " + e2e + " <= " + message.Deadline + " )");
                    if (debug) Console.ForegroundColor = consoleColor;
                    passed = true;
                }
                else
                {
                    if (debug) Console.ForegroundColor = ConsoleColor.DarkRed;
                    if (debug) Console.WriteLine("Message " + message.Name + " FAILED DeadlineConstraint! ( " + e2e + " <= " + message.Deadline + " )");
                    if (debug) Console.ForegroundColor = consoleColor;
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

            if (debug) Console.WriteLine("Scheduled Messages: " + scheduled);
            if (debug) Console.WriteLine("Unscheduled Messages: " + (messages.Count - scheduled));
            if (debug) Console.WriteLine();

            // Sort into ascending order
            unscheduled.Sort((x, y) => x.Deadline.CompareTo(y.Deadline)); // Sort based on deadline
            edges.Sort((x, y) => x.PropDelay.CompareTo(y.PropDelay)); // Sort based on PropDelay

            foreach (Message message in unscheduled)
            {
                pathToAssign = new();
                priorityEdges = new();

                ConsoleColor consoleColor = Console.ForegroundColor;
                if (debug) Console.ForegroundColor = ConsoleColor.Yellow;
                if (debug) Console.WriteLine("*********************************************");
                if (debug) Console.WriteLine("Edges that should take priority for message " + message.Name);
                if (debug) Console.WriteLine("Edges with BW_Cons <= " + highestBW);
                if (debug) Console.WriteLine("Edges with Queues <= " + lowestQCount);
                if (debug) Console.WriteLine();

                // Get the lowest Queue number for the direction of the message
                if (message.Backwards)
                {
                    lowestQCount = edges.Min(e => e.QueueBackwards.Count);
                }
                else
                {
                    lowestQCount = edges.Min(e => e.Queue.Count);
                }

                highestBW = edges.Max(e => e.BW_Consumption);

                // Check which edges should take priority to be assigned a new message
                foreach (Edge edge in edges)
                {
                    // Get the right parameters based on the direction of the message
                    if (message.Backwards)
                    {
                        if (edge.QueueBackwards.Count <= lowestQCount
                        && (edge.BW_Consumption + message.Size) < edge.BW
                        && (edge.QueueBackwards.Count + 1 <= MAX_QUE_COUNT)
                        && (edge.BW_Consumption) <= highestBW)
                        {
                            if (debug) edge.PrintEdge();
                            priorityEdges.Add(edge);
                        }
                    }
                    else
                    {
                        if (edge.Queue.Count <= lowestQCount
                        && (edge.BW_Consumption + message.Size) < edge.BW
                        && (edge.Queue.Count + 1 <= MAX_QUE_COUNT)
                        && (edge.BW_Consumption) <= highestBW)
                        {
                            if (debug) edge.PrintEdge();
                            priorityEdges.Add(edge);
                        }
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

                        scheduled++;
                        message.Scheduled = true;

                        if (debug) Console.WriteLine();
                        if (debug) message.PrintPath();

                        break; // Once the message has been scheduled a path then move on to the next message
                    }
                }

                if (debug) Console.WriteLine("*********************************************");
                if (debug) Console.WriteLine();
                if (debug) Console.ForegroundColor = consoleColor;
            }

            if (debug)
            {
                foreach (Edge edge in edges)
                {
                    edge.PrintEdgeDetails();
                }
            }

            if (debug) Console.WriteLine();
            if (debug) Console.WriteLine("Scheduled Messages: " + scheduled);
            if (debug) Console.WriteLine("Unscheduled Messages: " + (messages.Count - scheduled));
            if (debug) Console.WriteLine();
            if (debug)
            {
                foreach (Message m in messages)
                {
                    if (!m.Scheduled)
                    {
                        Console.WriteLine("Message " + m.Name + " unscheduled!");
                    }
                }
            }

            return messages;
        }

        private bool LinkCapactiyConstraint()
        {
            bool passed = false;

            // Calculate Latency Represented as Alpha in section 4.2
            foreach (Edge edge in edges)
            {
                int latency = (int)edge.WC_Cycle_Delay;

                if (edge.Queue.Count > 0 || edge.QueueBackwards.Count > 0)
                {
                    int q_Num = 0;

                    foreach (Message message in edge.Queue)
                    {
                        q_Num++;
                        latency += q_Num + (int)edge.PropDelay;
                    }

                    q_Num = 0;

                    foreach (Message message in edge.QueueBackwards)
                    {
                        q_Num++;
                        latency += q_Num + (int)edge.PropDelay;
                    }
                }

                if (debug) Console.WriteLine("Latency for Edge " + edge.Id + ": " + latency);

                edge.Latency = latency;
            }

            if (debug) Console.WriteLine();

            // Calculate Consumed BW in each cycle
            foreach (Edge edge in edges)
            {
                long cBW = 0;

                foreach (Message message in edge.Queue)
                {
                    cBW += ArrivalPattern(message, edge.Latency);
                }

                foreach (Message message in edge.QueueBackwards)
                {
                    cBW += ArrivalPattern(message, edge.Latency);
                }

                edge.BW_Consumption_Cycle = cBW;
                edge.LinkUtilization.Add(cBW);

                if (debug) Console.WriteLine("ConsumedBW for Edge " + edge.Id + ": " + cBW);
            }

            ConsoleColor consoleColor = Console.ForegroundColor;
            if (debug) Console.WriteLine();

            foreach (Edge edge in edges)
            {
                // BW_Cycle_Transfer is in MBytes while BW_Comsumption is in bits
                if (edge.BW_Consumption_Cycle <= edge.BW_Cylce_Transfer_Capacity * 125000)
                {
                    if (debug) Console.ForegroundColor = ConsoleColor.DarkGreen;
                    if (debug) Console.WriteLine("Edge " + edge.Id + " PASSED LinkeCapacityConstraint! ( " + edge.BW_Consumption_Cycle + " <= " + edge.BW_Cylce_Transfer_Capacity * 125000 + " )");
                    if (debug) Console.ForegroundColor = consoleColor;
                    passed = true;
                }
                else
                {
                    if (debug) Console.ForegroundColor = ConsoleColor.DarkRed;
                    if (debug) Console.WriteLine("Edge " + edge.Id + " FAILED LinkeCapacityConstraint! ( " + edge.BW_Consumption_Cycle + " <= " + edge.BW_Cylce_Transfer_Capacity * 125000 + " )");
                    if (debug) Console.ForegroundColor = consoleColor;
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

            Message temp = messages[0];

            Edge lowestBWEdge = edges[0];
            Edge highestBWEdge = edges[0];

            if (debug) Console.WriteLine();

            ConsoleColor consoleColor = Console.ForegroundColor;
            if (debug) Console.ForegroundColor = ConsoleColor.Blue;
            if (debug) Console.WriteLine("*********************************************");
            if (debug) Console.WriteLine("Edges that should take priority for the Swap");
            if (debug) Console.WriteLine("Edges with BW_Cons = " + lowestBW);
            if (debug) Console.WriteLine("Edges with BW_Cons = " + highestBW);
            if (debug) Console.WriteLine();

            // Check which edges should take priority to be assigned a new message
            foreach (Edge edge in edges)
            {
                if (edge.BW_Consumption <= lowestBW)
                {
                    if (debug) edge.PrintEdgeDetails();
                    lowestBWEdge = edge;
                }
                else if (edge.BW_Consumption >= highestBW)
                {
                    if (debug) edge.PrintEdgeDetails();
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
                            temp = message;

                            if (debug) Console.WriteLine("Message " + message.Name + " changed Path from: ");
                            if (debug) message.PrintPath();

                            pathToAssign = path;

                            if (debug) Console.WriteLine();
                            if (debug) Console.WriteLine("To: ");
                            if (debug) message.PrintPath();

                            moved = true;

                            break;
                        }

                        if (moved) break;
                    }
                }
            }

            temp.SetPath(pathToAssign);

            // Update the effect of the network
            foreach (Edge edge in temp.Path)
            {
                if (temp.Backwards)
                {
                    edge.QueueBackwards.Add(temp);
                }
                else
                {
                    edge.Queue.Add(temp);
                }

                edge.CalculateBW();
            }

            if (debug) Console.WriteLine();
            if (debug) Console.ForegroundColor = consoleColor;
        }

        private void PickNewPath()
        {
            var random = new Random();

            int index = random.Next(0, messages.Count);
            int index_path = random.Next(0, messages[index].PossiblePaths.Count);

            Message message = messages[index];

            if (debug) Console.WriteLine();

            ConsoleColor consoleColor = Console.ForegroundColor;
            if (debug) Console.ForegroundColor = ConsoleColor.Blue;
            if (debug) Console.WriteLine("*********************************************");
            if (debug) Console.WriteLine("Changing Path of Message " + message.Name + "!");
            if (debug) Console.WriteLine("Random Index of path = " + index_path);
            if (debug) Console.WriteLine();

            if (debug) message.PrintPath();

            foreach (Edge edge in edges)
            {
                edge.Queue.Remove(message);
                edge.QueueBackwards.Remove(message);
            }

            message.SetPath(message.PossiblePaths[index_path]);

            foreach (Edge edge in message.Path)
            {
                if (message.Backwards)
                {
                    edge.QueueBackwards.Add(message);
                }
                else
                {
                    edge.Queue.Add(message);
                }
            }

            if (debug) message.PrintPath();
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

        private bool QueMax()
        {
            bool passed = false;

            foreach (Message message in messages)
            {
                if (message.Backwards)
                {
                    foreach (Edge edge in message.Path)
                    {
                        if (edge.QueueBackwards.Count <= MAX_QUE_COUNT)
                        {
                            passed = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    foreach (Edge edge in message.Path)
                    {
                        if (edge.Queue.Count <= MAX_QUE_COUNT)
                        {
                            passed = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return passed;
        }
    }
}