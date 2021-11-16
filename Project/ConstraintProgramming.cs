using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Vertex = System.String;

namespace Project
{
    internal class ConstraintProgramming
    {
        public static void printPath(List<Edge> path)
        {
            Console.Write(path[0].Source + ", " + path[0].Destination);
            for (int i = 1; i < path.Count; i++)
            {
                Console.Write(", " + path[i].Destination);
            }
            Console.WriteLine();
        }

        public void Solve()
        {
            //// Reading from the XML
            //List<Message> messages = ParseMessageXml(PATH + "Apps.xml");
            //(List<Vertex> vertices, List<Edge> edges) = ParseArchiteure(PATH + "Config.xml");

            //int[] mMessages = Enumerable.Range(0, messages.Count).ToArray();
            //int[] mEdges = Enumerable.Range(0, edges.Count).ToArray();

            //Dictionary<Edge, uint> curr_edge_bw = new Dictionary<Edge, uint>();
            //foreach (var e in mEdges)
            //{
            //    curr_edge_bw.Add(edges[e], 0);
            //}

            //Dictionary<Message, List<List<Edge>>> message_routes = new Dictionary<Message, List<List<Edge>>>();
            //foreach (var m in mMessages)
            //{
            //    message_routes.Add(messages[m], Findpaths(messages[m].Source, messages[m].Destination, edges));         // create a list with all routes possible for every message
            //}

            //Dictionary<Message, int> current_messages_routes = new Dictionary<Message, int>();
            //Random rand = new();
            //foreach (var m in messages)
            //{
            //    current_messages_routes.Add(m, rand.Next(0, message_routes[m].Count - 1));      // Assign a random route to the message
            //}

            //ulong iterations = 0;
            //bool exceeded = false;
            //while (++iterations < 10000000)
            //{
            //    foreach (var m in messages)
            //    {
            //        message_routes.TryGetValue(m, out List<List<Edge>> routes);     // Get all routes of a message
            //        current_messages_routes.TryGetValue(m, out int current_route);          // Get the index of the route assigned to the message
            //        List<Edge> route = routes.ElementAt(current_route);             // Get current route assigned to a message
            //        current_messages_routes[m] = rand.Next(0, message_routes[m].Count - 1);

            //        foreach (var e in route)
            //        {
            //        }
            //    }
            //    if (exceeded == false) break;
            //    else exceeded = false;
            //}

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

            // CpSolver solver = new(); solver.Solve(model); foreach (var path in messages_path)
            // Console.Write(path.Name() + " " + solver.Value(path) + ", "); Console.WriteLine();
            // foreach (var edgeBW in mEdgesBW) Console.Write(edgeBW.Name() + " " +
            // solver.Value(edgeBW) + ", ");
            // TODO: create variables in ortools

            // TODO: create domain

            // TODO: create constraints

            // Find the correct solver
        }
    }
}