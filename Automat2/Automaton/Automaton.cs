public class Automaton
{
    public Node Start { get; set; }
    public Node Final { get; set; }
    public List<Node> Nodes { get; set; } = new List<Node>();
    public List<StepSnapshot> StepHistory { get; set; } = new List<StepSnapshot>();

    private int _nodeCounter = 0;
    private int _stepCounter = 0;

    public Node CreateNode()
    {
        var node = new Node(_nodeCounter++);
        Nodes.Add(node);
        return node;
    }

    public void AddStep(string comment)
    {
        // Создаем глубокую копию всех узлов и их переходов
        var copiedNodes = new List<Node>();
        var nodeMap = new Dictionary<int, Node>();

        // Сначала создаем копии всех узлов
        foreach (var node in Nodes)
        {
            var copiedNode = new Node(node.Id);
            copiedNode.Name = node.Name; // Сохраняем имя!
            nodeMap[node.Id] = copiedNode;
            copiedNodes.Add(copiedNode);
        }

        // Затем копируем переходы
        foreach (var node in Nodes)
        {
            var copiedNode = nodeMap[node.Id];
            foreach (var (to, expr) in node.Transitions)
            {
                if (nodeMap.TryGetValue(to.Id, out var copiedTo))
                {
                    copiedNode.AddTransition(copiedTo, expr);
                }
            }
        }

        // Передаем имена начального и конечного состояний
        var snapshot = new StepSnapshot(_stepCounter++, copiedNodes, comment, Start?.Name, Final?.Name);
        StepHistory.Add(snapshot);
    }
}