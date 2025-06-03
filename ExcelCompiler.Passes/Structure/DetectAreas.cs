using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.Structure.Range;

namespace ExcelCompiler.Passes.Structure;

[CompilerPass]
public class DetectAreas
{
    public record Node(Location Location, List<Node> Neighbours);
    
    public List<Area> Detect(Workbook workbook)
    {
        List<Area> areas = new();

        foreach (Spreadsheet sh in workbook.Spreadsheets)
        {
            areas.AddRange(Detect(sh));
        }
        
        return areas;
    }

    private List<Area> Detect(Spreadsheet spreadsheet)
    {
        List<Area> areas = new();
        
        var graph = ConvertSpreadsheetToGraph(spreadsheet);
        var connectedComponents = FindConnectedComponents(graph);

        foreach (var component in connectedComponents)
        {
            // Find the right and left most position and create a range for the area
            var leftMost = component.Min(c => c.Location.Column);
            var rightMost = component.Max(c => c.Location.Column);
            var topMost = component.Min(c => c.Location.Row);
            var bottomMost = component.Max(c => c.Location.Row);
            
            // Create the range
            Location left = new Location()
            {
                Column = leftMost,
                Row = topMost,
                Spreadsheet = spreadsheet.Name,
            };

            Location right = new Location()
            {
                Column = rightMost,
                Row = bottomMost,
                Spreadsheet = spreadsheet.Name,
            };
            
            Range range = new Range(left, right);
            
            var area = new Area(range);
            areas.Add(area);
        }

        return areas;
    }

    private static List<HashSet<Node>> FindConnectedComponents(List<Node> graph)
    {
        List<HashSet<Node>> connectedComponents = new();
        HashSet<Node> remaining = new(graph);
        while (remaining.Count > 0)
        {
            // Get a random node
            Node randomNode = remaining.First();
            remaining.Remove(randomNode);
            
            // Find all connected components
            HashSet<Node> connectedComponent = new();
            connectedComponent.Add(randomNode);
            Queue<Node> queue = new(randomNode.Neighbours);
            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();

                if (!remaining.Remove(node)) continue;
                
                connectedComponent.Add(node);
                queue.EnqueueRange(node.Neighbours);
            }
            
            connectedComponents.Add(connectedComponent);
        }

        return connectedComponents;
    }

    private static List<Node> ConvertSpreadsheetToGraph(Spreadsheet spreadsheet)
    {
        // Convert the spreadsheet to a graph
        Dictionary<Location, Node> nodes = new();
        foreach (Cell cell in spreadsheet.Cells)
        {
            // Convert to node
            Node node = new Node(cell.Location, []);
            nodes.Add(cell.Location, node);
        }
        
        // Find neighbours
        foreach (var (neighbourLocation, node) in 
                 from node in nodes.Values 
                 from neighbourLocation in GetPossibleNeighbourLocations(node.Location) 
                 where HasNodeAt(nodes, neighbourLocation) 
                 select (neighbourLocation, node))
        {
            Node neighbour = nodes[neighbourLocation];
            node.Neighbours.Add(neighbour);
        }

        List<Node> graph = nodes.Values.ToList();
        return graph;
    }

    private static bool HasNodeAt(Dictionary<Location, Node> nodes, Location location)
    {
        return nodes.ContainsKey(location);
    }
    
    private static List<Location> GetPossibleNeighbourLocations(Location location)
    {
        return
        [
            location with { Row = location.Row - 1 },
            location with { Row = location.Row + 1 },
            location with { Column = location.Column - 1 },
            location with { Column = location.Column + 1 }
        ];
    }
}