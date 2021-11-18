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
        private List<Edge> edges;
        private Dictionary<Message, List<List<Edge>>> message_routes;
        private List<Message> messages;
        private List<Vertex> vertices;

        public SimulatedAnnealing(List<Message> messages, List<Vertex> vertices, List<Edge> edges, Dictionary<Message, List<List<Edge>>> message_routes)
        {
            this.messages = messages;
            this.vertices = vertices;
            this.edges = edges;
            this.message_routes = message_routes;
        }

        public void Solve()
        {
            Console.Clear();

            Console.WriteLine("Finding an initial solution");
            EariliestDeadlineFirst();
        }

        private void calculateMeanBandwith()
        {
            // MeanBandwidth = sum() / size of links
        }

        /// <summary>
        /// Makes sure that all the messages are meeting their deadline even when being affected by
        /// the edges prop delay and the verticies queue cycle delay.
        /// </summary>
        private void deadlineConstraint()
        {
            // End to end delay <= allMessages.deadline
        }

        private void EariliestDeadlineFirst()
        {
            // Sort into ascending order
            messages.Sort((x, y) => x.Deadline.CompareTo(y.Deadline)); // Sort based on deadline
            edges.Sort((x, y) => x.PropDelay.CompareTo(y.PropDelay)); // Sort based on PropDelay

            printMessageRoutes();

            foreach (Message message in messages)
            {
            }
        }

        private void printMessageRoutes()
        {
            int count_messges = 0;
            int count_path = 0;

            foreach (Message message in message_routes.Keys)
            {
                if (count_messges % 2 == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine("Showing paths for message: " + message.Name);
                Console.WriteLine("Total possible paths found: " + message_routes.Values.Count);
                Console.WriteLine("Message source to destination: " + message.Source.Name + " -> " + message.Destination.Name);
                Console.WriteLine("------------------------------------------");
                Console.WriteLine();

                foreach (List<List<Edge>> possiblePaths in message_routes.Values)
                {
                    foreach (List<Edge> path in possiblePaths)
                    {
                        Console.WriteLine("Path #" + count_path);

                        foreach (Edge edge in path)
                        {
                            Console.WriteLine(edge.Source.Name + " -> " + edge.Destination.Name + " Prop Delay=" + edge.PropDelay);
                        }

                        count_path++;

                        Console.WriteLine();
                    }
                }

                count_path = 0;
                count_messges++;
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}