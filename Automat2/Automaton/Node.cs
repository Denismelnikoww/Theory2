public class Node
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<(Node To, string Expression)> Transitions { get; set; } = new List<(Node, string)>();

    public Node(int id)
    {
        Id = id;
        Name = $"q{id}";
    }

    public void AddTransition(Node to, string expression)
    {
        Transitions.Add((to, expression));
    }
}