public class Automaton
{
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
        var copiedNodes = new List<Node>();
        var nodeMap = new Dictionary<int, Node>();

        foreach (var node in Nodes)
        {
            var copiedNode = new Node(node.Id);
            copiedNode.Name = node.Name;
            copiedNode.IsStart = node.IsStart;
            copiedNode.IsFinal = node.IsFinal;
            nodeMap[node.Id] = copiedNode;
            copiedNodes.Add(copiedNode);
        }

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

        var snapshot = new StepSnapshot(_stepCounter++, copiedNodes, comment);
        StepHistory.Add(snapshot);
    }

    public bool ProcessInput(string input)
    {
        var startNodes = Nodes.Where(n => n.IsStart).ToList();
        var finalNodes = Nodes.Where(n => n.IsFinal).ToList();

        if (startNodes.Count == 0)
            throw new InvalidOperationException("Начальное состояние не установлено");
        if (finalNodes.Count == 0)
            throw new InvalidOperationException("Финальное состояние не установлено");

        Console.WriteLine($"Обработка входной строки: '{input}'");
        AddStep($"Начало обработки строки: '{input}'");

        Node currentState = startNodes[0];
        int position = 0;

        foreach (char symbol in input)
        {
            Console.WriteLine($"Шаг {position + 1}: Текущее состояние '{currentState.Name}', символ '{symbol}'");

            bool transitionFound = false;

            foreach (var (nextState, transitionExpr) in currentState.Transitions)
            {
                if (MatchesTransition(symbol.ToString(), transitionExpr))
                {
                    Console.WriteLine($"  Переход в состояние '{nextState.Name}' по выражению '{transitionExpr}'");
                    currentState = nextState;
                    transitionFound = true;

                    AddStep($"Обработан символ '{symbol}': '{currentState.Name}' -> '{nextState.Name}'");
                    break;
                }
            }

            if (!transitionFound)
            {
                Console.WriteLine($"  НЕТ ПЕРЕХОДА для символа '{symbol}' из состояния '{currentState.Name}'");
                AddStep($"Ошибка: нет перехода для символа '{symbol}' из состояния '{currentState.Name}'");
                return false;
            }

            position++;
        }

        bool accepted = finalNodes.Contains(currentState);

        Console.WriteLine($"Результат: строка {(accepted ? "ПРИНЯТА" : "ОТВЕРГНУТА")}");
        Console.WriteLine($"Конечное состояние: '{currentState.Name}', Финальные состояния: {string.Join(", ", finalNodes.Select(n => n.Name))}");

        AddStep(accepted ? "Строка принята - достигнуто финальное состояние" : "Строка отвергнута - не достигнуто финальное состояние");

        return accepted;
    }

    public Dictionary<string, bool> ProcessMultipleInputs(string[] inputs)
    {
        var results = new Dictionary<string, bool>();

        foreach (string input in inputs)
        {
            try
            {
                bool result = ProcessInput(input);
                results[input] = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке строки '{input}': {ex.Message}");
                results[input] = false;
            }
        }

        return results;
    }

    private bool MatchesTransition(string input, string transitionExpr)
    {
        if (transitionExpr == "ε" || transitionExpr == "epsilon") 
            return true;

        if (transitionExpr == ".") 
            return true;

        if (transitionExpr.Length == 1) 
            return input == transitionExpr;

        if (transitionExpr.StartsWith("[") && transitionExpr.EndsWith("]") && transitionExpr.Contains("-"))
        {
            string range = transitionExpr.Substring(1, transitionExpr.Length - 2);
            string[] parts = range.Split('-');
            if (parts.Length == 2 && parts[0].Length == 1 && parts[1].Length == 1)
            {
                char start = parts[0][0];
                char end = parts[1][0];
                char inputChar = input[0];
                return inputChar >= start && inputChar <= end;
            }
        }

        if (transitionExpr.StartsWith("{") && transitionExpr.EndsWith("}"))
        {
            string set = transitionExpr.Substring(1, transitionExpr.Length - 2);
            return set.Contains(input);
        }

        return input == transitionExpr;
    }
}