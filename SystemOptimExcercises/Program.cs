using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SystemOptimExcercises
{
    class MainClassss
    {
        public static void Main(string[] args)
        {
            // Might be better using the LINQ documentation can be seen at:
            // https://docs.microsoft.com/en-us/dotnet/standard/linq/linq-xml-overview

            // XML File reading 
            XmlDocument doc = new XmlDocument();
            doc.Load("../XML/small.xml");
            foreach (XmlNode node in doc.DocumentElement.ChildNodes) // DocumentElement is <Model>  children <Application> and <Plataform>
            {

                Console.WriteLine("<" + node.Name + ">");
                Console.WriteLine();

                if (node.Name == "Application" && node.HasChildNodes) // Will have children <Task>
                {
                    foreach (XmlNode task in node.ChildNodes)
                    {
                        
                        Console.WriteLine("\t" + "<" + task.LocalName + ">");
                        Console.WriteLine();


                        foreach (XmlAttribute attribute in task.Attributes) 
                        {
                            Console.Write("\t\t" + "<" + attribute.Name + ">");

                            switch (attribute.Name)
                            {
                                
                                case "Deadline":
                                    {
                                        Console.WriteLine("\t" + attribute.Value);
                                        break;
                                    }

                                case "Id":
                                    {
                                        Console.WriteLine("\t\t" + attribute.Value);
                                        break;
                                    }

                                case "Period":
                                    {
                                        Console.WriteLine("\t" + attribute.Value);
                                        break;
                                    }

                                case "WCET":
                                    {
                                        Console.WriteLine("\t\t" + attribute.Value);
                                        break;
                                    }
                            }
                        }
                    }

                } else if (node.Name == "Platform" && node.HasChildNodes) // Will have children <MCP>
                {
                    foreach (XmlNode mcp in node.ChildNodes) 
                    {
                        Console.Write("\t" + "<" + mcp.Name);
                        Console.WriteLine("\tID: " + mcp.Attributes.GetNamedItem("Id").Value + ">");
                        Console.WriteLine();


                        foreach (XmlNode core in mcp.ChildNodes) // Will have children <Core>
                        {
                            Console.Write("\t\t" + "<" + core.Name + ">");
                            Console.WriteLine();

                            foreach (XmlAttribute attribute in core.Attributes)
                            {
                                Console.Write("\t\t\t" + "<" + attribute.Name + ">");

                                switch (attribute.Name)
                                {
                                   
                                    case "Id":
                                        {
                                            Console.WriteLine("\t\t\t" + attribute.Value);
                                            break;
                                        }

                                    case "WCETFactor":
                                        {
                                            Console.WriteLine("\t\t" + attribute.Value);
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine(); // Here only for easier reading
            }


            Console.ReadLine(); // Here only to have time to read result
        }

    }
}
