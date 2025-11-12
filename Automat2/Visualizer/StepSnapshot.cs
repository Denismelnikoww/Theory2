public class StepSnapshot
{
    public int StepNumber { get; set; }
    public List<Node> Nodes { get; set; }
    public string Comment { get; set; }

    public StepSnapshot(int stepNumber, List<Node> nodes, string comment)
    {
        StepNumber = stepNumber;
        Nodes = nodes;
        Comment = comment;
    }
}