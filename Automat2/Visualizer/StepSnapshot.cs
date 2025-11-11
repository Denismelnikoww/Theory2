public class StepSnapshot
{
    public int StepNumber { get; set; }
    public List<Node> Nodes { get; set; }
    public string Comment { get; set; }
    public Node Start { get; set; }
    public Node Final { get; set; }

    public StepSnapshot(int stepNumber, List<Node> nodes, string comment)
    {
        StepNumber = stepNumber;
        Nodes = nodes;
        Comment = comment.Replace("q0","Start").Replace("q1","Final");

        // Находим начальное и конечное состояния (q0 и последний созданный)
        Start = nodes.FirstOrDefault(n => n.Id == 0);
        Final = nodes.OrderByDescending(n => n.Id).FirstOrDefault();
    }
}