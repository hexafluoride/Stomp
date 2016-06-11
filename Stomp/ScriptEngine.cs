using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;

using Stomp.Filters;
using Stomp.Filters.Contexts;

namespace Stomp
{
    public class ScriptEngine
    {
        public Dictionary<string, Type> Types = new Dictionary<string, Type>();

        public ScriptEngine()
        {
        }

        public FilterChain Parse(string chain)
        {
            int scope = 1;
            
            StringReader sr = new StringReader(chain);

            List<KeyValuePair<int, string>> lines = new List<KeyValuePair<int, string>>();

            while (true)
            {
                string line = sr.ReadLine();

                if (line == null)
                    break;
                
                if (line.Contains("}"))
                {
                    scope--;
                    if (line.Trim() == "}")
                        continue;
                }

                // Console.WriteLine("Scope {0}: {1}", scope, line.Replace("{", ":"));
                lines.Add(new KeyValuePair<int, string>(scope, line.Replace("{", "").Trim()));

                if (line.Contains("{"))
                {
                    scope++;
                }
            }

            // thanks to @le1ca for using his expertise of automata to help me write this really simple parser that i should have figured out myself

            int last_scope = 0;
            var root = new Node("filter-chain", null);
            var scopes = new Dictionary<int, Node>() 
            {
                {0, root}
            };

            foreach (var line in lines)
            {
                if (line.Key > last_scope + 1)
                    throw new Exception("Invalid script");

                var current_node = new Node(line.Value, scopes[line.Key - 1]);
                scopes[line.Key] = current_node;
                current_node.Parent.Children.Add(current_node);
                last_scope = line.Key;
            }

            return ParseFilterChain(scopes[0]);
        }

        public FilterChain ParseFilterChain(Node node)
        {
            FilterChain ret = new FilterChain();

            foreach (var child in node.Children)
            {
                ret.Append(ParseFilter(child));
            }

            return ret;
        }

        public IFilter ParseContext(Node node)
        {
            var type = Types[node.Content];
            var filter = Activator.CreateInstance(type);

            List<Node> filters = new List<Node>();

            foreach(var child in node.Children)
            {
                string line = child.Content;

                if (!line.Contains("="))
                {
                    filters.Add(child);
                    continue;
                }

                string key = line.Split('=')[0].Trim();
                string rest = string.Join("=", line.Split('=').Skip(1)).Trim();

                var prop = GetPropertyByAlias(type, key);

                if (prop == null)
                {
                    throw new Exception("Invalid property " + key);
                }

                if (prop.PropertyType.IsEnum)
                    prop.SetValue(filter, Enum.Parse(prop.PropertyType, rest, true));
                else
                    prop.SetValue(filter, Convert.ChangeType(rest, prop.PropertyType));
            }

            Node chain_node = new Node("filter-chain", null);
            chain_node.Children.AddRange(filters);

            GetPropertyByAlias(type, "inner-chain").SetValue(filter, ParseFilterChain(chain_node));

            return (IFilter)filter;
        }

        public IFilter ParseFilter(Node node)
        {
            if (node.Content.EndsWith("-context"))
                return ParseContext(node);

            var type = Types[node.Content];
            var filter = Activator.CreateInstance(type);

            foreach(var child in node.Children)
            {
                string line = child.Content;

                if (!line.Contains("="))
                    continue;

                string key = line.Split('=')[0].Trim();
                string rest = string.Join("=", line.Split('=').Skip(1)).Trim();

                var prop = GetPropertyByAlias(type, key);

                if (prop == null)
                {
                    Console.WriteLine("Couldn't find key: {0}", key);
                }

                if (prop.PropertyType.IsEnum)
                    prop.SetValue(filter, Enum.Parse(prop.PropertyType, rest, true));
                else
                    prop.SetValue(filter, Convert.ChangeType(rest, prop.PropertyType));
            }

            return (IFilter)filter;
        }

        private PropertyInfo GetPropertyByAlias(Type type, string alias)
        {
            var prop = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p =>
                {
                    var attribs = p.GetCustomAttributes(typeof(ScriptAliasAttribute), false);
                    if (attribs.Count() == 1)
                    {
                        if (((ScriptAliasAttribute)attribs.First()).Name == alias)
                        {
                            //Console.WriteLine("Alias match: {0}", alias);
                            return true;
                        }
                    }

                    return false;
                });

            return prop;
        }

        public void Register(IFilter filter)
        {
            Register(filter.ScriptName, filter.GetType());
        }

        public void Register(string name, Type type)
        {
            Types.Add(name, type);
        }
    }

    public class Node
    {
        public string Content { get; set; }
        public Node Parent { get; set; }
        public List<Node> Children { get; set; }

        public Node(string content, Node parent)
        {
            Content = content;
            Parent = parent;
            Children = new List<Node>();
        }

        public void Print(int depth = 0)
        {
            Console.WriteLine("{0}\"{1}\"", new string(' ', depth * 2), Content);
            Children.ForEach(n => n.Print(depth + 1));
        }
    }
}

