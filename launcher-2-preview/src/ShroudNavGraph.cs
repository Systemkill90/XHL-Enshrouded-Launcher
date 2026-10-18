using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace XHLLauncher
{
    // A standalone road router. It does not touch game processes, network clients or saves.
    internal sealed class NavNodeData
    {
        public string Id, Name;
        public double X, Y;
    }
    internal sealed class NavLinkData
    {
        public string From, To, Proof, RoadName;
    }
    internal sealed class NavDocument
    {
        public string Format, CoordinateSystem, SourceNote;
        public List<NavNodeData> Nodes;
        public List<NavLinkData> Links;
    }
    internal sealed class NavStep
    {
        public string NodeId, Text;
        public double DistanceFromStart;
    }
    internal sealed class NavRoute
    {
        public readonly List<NavStep> Steps = new List<NavStep>();
        public readonly List<string> NodeIds = new List<string>();
        public double DistanceInMapUnits;
        public int CandidateSegments;
        public bool IsVerified { get { return CandidateSegments == 0; } }
    }

    internal sealed class ShroudNavGraph
    {
        sealed class Edge
        {
            public string Target;
            public bool Candidate;
            public double Distance;
        }
        sealed class OpenItem
        {
            public string Id;
            public double Score;
            public long Order;
        }
        sealed class OpenComparer : IComparer<OpenItem>
        {
            public int Compare(OpenItem a, OpenItem b)
            {
                int cmp = a.Score.CompareTo(b.Score);
                return cmp != 0 ? cmp : a.Order.CompareTo(b.Order);
            }
        }

        readonly Dictionary<string, NavNodeData> nodes = new Dictionary<string, NavNodeData>(StringComparer.Ordinal);
        readonly Dictionary<string, List<Edge>> adjacency = new Dictionary<string, List<Edge>>(StringComparer.Ordinal);
        public int NodeCount { get { return nodes.Count; } }
        public string SourceNote { get; private set; }
        const int MaxNodes = 30000, MaxLinks = 100000;

        static bool Finite(double n) { return !Double.IsNaN(n) && !Double.IsInfinity(n) && Math.Abs(n) <= 1000000; }
        static bool ValidId(string s)
        {
            return !String.IsNullOrWhiteSpace(s) && s.Length <= 48 &&
                Regex.IsMatch(s, @"^[A-Za-z0-9_.-]+$");
        }
        static double Distance(NavNodeData a, NavNodeData b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        static double Distance(double x, double y, NavNodeData b)
        {
            double dx = x - b.X, dy = y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static ShroudNavGraph Parse(string json)
        {
            if (String.IsNullOrWhiteSpace(json) || json.Length > 12000000)
                throw new ArgumentException("Road graph document is absent or too large.");
            NavDocument doc;
            try { doc = new JavaScriptSerializer { MaxJsonLength = 12000000 }.Deserialize<NavDocument>(json); }
            catch (Exception e) { throw new ArgumentException("Road graph JSON is invalid.", e); }
            if (doc == null || doc.Format != "shroudnav-roadgraph-v1" ||
                doc.CoordinateSystem != "enshrouded-world-xy-calibrated-v1" ||
                String.IsNullOrWhiteSpace(doc.SourceNote) || doc.SourceNote.Length > 1000)
                throw new ArgumentException("Graph format, coordinate-system declaration or source attribution is missing.");
            if (doc.Nodes == null || doc.Links == null ||
                doc.Nodes.Count < 1 || doc.Nodes.Count > MaxNodes || doc.Links.Count > MaxLinks)
                throw new ArgumentException("Graph exceeds safe node/link bounds or has no nodes.");

            var g = new ShroudNavGraph();
            g.SourceNote = doc.SourceNote;
            foreach (NavNodeData n in doc.Nodes)
            {
                if (n == null || !ValidId(n.Id) || !Finite(n.X) || !Finite(n.Y) ||
                    (n.Name ?? "").Length > 120 || g.nodes.ContainsKey(n.Id))
                    throw new ArgumentException("Invalid, duplicated or non-finite road node.");
                g.nodes.Add(n.Id, n);
                g.adjacency.Add(n.Id, new List<Edge>());
            }
            var edgeKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (NavLinkData link in doc.Links)
            {
                NavNodeData a, b;
                if (link == null || link.From == link.To ||
                    !g.nodes.TryGetValue(link.From ?? "", out a) ||
                    !g.nodes.TryGetValue(link.To ?? "", out b) ||
                    (link.Proof != "verified" && link.Proof != "candidate") ||
                    (link.RoadName ?? "").Length > 120)
                    throw new ArgumentException("Invalid road link, missing endpoint or unrecognized proof.");
                string key = String.CompareOrdinal(a.Id, b.Id) < 0 ? a.Id + "|" + b.Id : b.Id + "|" + a.Id;
                if (!edgeKeys.Add(key)) throw new ArgumentException("Duplicate link or conflicting road evidence.");
                double length = Distance(a, b);
                if (!Finite(length) || length < 0.001) throw new ArgumentException("Degenerate road segment.");
                bool candidate = link.Proof == "candidate";
                g.adjacency[a.Id].Add(new Edge { Target = b.Id, Candidate = candidate, Distance = length });
                g.adjacency[b.Id].Add(new Edge { Target = a.Id, Candidate = candidate, Distance = length });
            }
            return g;
        }

        // Caller supplies maximum *map-coordinate* snap distance, never a guessed world scale.
        public string NearestNode(double x, double y, double maxDistance)
        {
            if (!Finite(x) || !Finite(y) || !Finite(maxDistance) || maxDistance <= 0)
                throw new ArgumentException("Invalid coordinate or snap distance.");
            var best = nodes.Values.Select(n => new { n.Id, D = Distance(x, y, n) })
                .Where(n => n.D <= maxDistance)
                .OrderBy(n => n.D).ThenBy(n => n.Id, StringComparer.Ordinal).FirstOrDefault();
            return best == null ? null : best.Id;
        }

        public NavRoute FindRoute(string start, string destination, bool allowUnverified = false)
        {
            if (!nodes.ContainsKey(start ?? "") || !nodes.ContainsKey(destination ?? "")) return null;
            var open = new SortedSet<OpenItem>(new OpenComparer());
            var cheapest = new Dictionary<string, double>(StringComparer.Ordinal);
            var previous = new Dictionary<string, string>(StringComparer.Ordinal);
            var priorEdge = new Dictionary<string, Edge>(StringComparer.Ordinal);
            var closed = new HashSet<string>(StringComparer.Ordinal);
            long sequence = 0;
            cheapest[start] = 0;
            open.Add(new OpenItem { Id = start, Score = Distance(nodes[start], nodes[destination]), Order = sequence++ });
            while (open.Count > 0)
            {
                OpenItem item = open.Min; open.Remove(item);
                if (!closed.Add(item.Id)) continue;
                if (item.Id == destination) break;
                foreach (Edge edge in adjacency[item.Id])
                {
                    if (edge.Candidate && !allowUnverified) continue;
                    double proposed = cheapest[item.Id] + edge.Distance * (edge.Candidate ? 3.0 : 1.0);
                    double old;
                    if (closed.Contains(edge.Target) ||
                        (cheapest.TryGetValue(edge.Target, out old) && proposed >= old - 1e-8)) continue;
                    cheapest[edge.Target] = proposed; previous[edge.Target] = item.Id; priorEdge[edge.Target] = edge;
                    double estimate = proposed + Distance(nodes[edge.Target], nodes[destination]);
                    open.Add(new OpenItem { Id = edge.Target, Score = estimate, Order = sequence++ });
                }
            }
            if (!cheapest.ContainsKey(destination)) return null;
            var route = new NavRoute();
            string here = destination; route.NodeIds.Add(here);
            while (here != start)
            {
                string parent;
                if (!previous.TryGetValue(here, out parent)) return null;
                Edge edge = priorEdge[here];
                route.DistanceInMapUnits += edge.Distance;
                if (edge.Candidate) route.CandidateSegments++;
                here = parent;
                route.NodeIds.Add(here);
            }
            route.NodeIds.Reverse();
            double walked = 0;
            for (int i = 0; i < route.NodeIds.Count; i++)
            {
                string id = route.NodeIds[i]; NavNodeData n = nodes[id];
                string text;
                if (i == 0) text = "Start near " + (!String.IsNullOrEmpty(n.Name) ? n.Name : id);
                else if (i == route.NodeIds.Count - 1) text = "Arrive near " + (!String.IsNullOrEmpty(n.Name) ? n.Name : id);
                else
                {
                    NavNodeData prev = nodes[route.NodeIds[i - 1]];
                    NavNodeData next = nodes[route.NodeIds[i + 1]];
                    double cross = (n.X-prev.X)*(next.Y-n.Y)-(n.Y-prev.Y)*(next.X-n.X);
                    double dot = (n.X-prev.X)*(next.X-n.X)+(n.Y-prev.Y)*(next.Y-n.Y);
                    double degrees = Math.Atan2(cross,dot)*180/Math.PI;
                    text = degrees > 35 ? "Turn left" : degrees < -35 ? "Turn right" : "Continue";
                    if (!String.IsNullOrEmpty(n.Name)) text += " near " + n.Name;
                }
                if (i > 0) walked += Distance(nodes[route.NodeIds[i-1]], n);
                route.Steps.Add(new NavStep { NodeId = id, Text = text, DistanceFromStart = walked });
            }
            return route;
        }
    }
}
