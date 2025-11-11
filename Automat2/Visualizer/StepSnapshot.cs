public class StepSnapshot
{
    public int StepNumber { get; set; }
    public List<Node> Nodes { get; set; }
    public string Comment { get; set; }
    public string StartStateName { get; set; }  // Добавляем имя начального состояния
    public string FinalStateName { get; set; }  // Добавляем имя конечного состояния

    public StepSnapshot(int stepNumber, List<Node> nodes, string comment, string startStateName, string finalStateName)
    {
        StepNumber = stepNumber;
        Nodes = nodes;
        Comment = comment;
        StartStateName = startStateName;
        FinalStateName = finalStateName;
    }
}