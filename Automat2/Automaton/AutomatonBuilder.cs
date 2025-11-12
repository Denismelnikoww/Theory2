public class AutomatonBuilder
{
    private Automaton _automaton;

    public Automaton Build(string expression)
    {
        _automaton = new Automaton();
        var start = _automaton.CreateNode();
        var final = _automaton.CreateNode();
        start.IsStart = true;
        final.IsFinal = true;

        start.AddTransition(final, expression);

        _automaton.AddStep($"Исходное выражение: '{expression}' -> переход {start.Name} -> {final.Name}");

        ProcessAllTransitions();

        return _automaton;
    }

    private void ProcessAllTransitions()
    {
        bool changed;
        do
        {
            changed = false;

            var allTransitions = new List<(Node From, Node To, string Expr)>();

            foreach (var node in _automaton.Nodes)
            {
                foreach (var (to, expr) in node.Transitions)
                {
                    if (!IsSimpleSymbol(expr) && !string.IsNullOrEmpty(expr))
                    {
                        allTransitions.Add((node, to, expr));
                    }
                }
            }

            foreach (var (from, to, expr) in allTransitions)
            {
                from.Transitions.RemoveAll(t => t.To == to && t.Expression == expr);

                ProcessTransition(from, to, expr);
                changed = true;
                _automaton.AddStep($"Обработка перехода {from.Name} -> {to.Name} с выражением '{expr}'");

                break;
            }
        }
        while (changed);
    }

    private void ProcessTransition(Node from, Node to, string expr)
    {
        expr = expr.Trim();

        if (IsEnclosedInParens(expr))
        {
            var inner = expr.Substring(1, expr.Length - 2);
            from.AddTransition(to, inner);
            return;
        }

        var unionParts = SplitByTopLevelUnion(expr);
        if (unionParts.Count > 1)
        {
            HandleUnion(from, to, unionParts);
            return;
        }

        if (expr.EndsWith("^"))
        {
            var inner = expr.Substring(0, expr.Length - 1);
            HandleKleenePlus(from, to, inner);
            return;
        }

        if (expr.EndsWith("*"))
        {
            var inner = expr.Substring(0, expr.Length - 1);
            HandleKleeneStar(from, to, inner);
            return;
        }

        var concatParts = SplitByConcatenation(expr);
        if (concatParts.Count > 1)
        {
            HandleConcatenation(from, to, concatParts);
            return;
        }

        from.AddTransition(to, expr);
    }

    private List<string> SplitByTopLevelUnion(string expr)
    {
        var parts = new List<string>();
        int level = 0;
        int lastSplit = 0;

        for (int i = 0; i < expr.Length; i++)
        {
            char c = expr[i];

            if (c == '(')
            {
                level++;
            }
            else if (c == ')')
            {
                level--;
            }

            if (c == '+' && level == 0)
            {
                bool isOperator = true;

                if (i > 0)
                {
                    char left = expr[i - 1];
                    isOperator = (char.IsLetterOrDigit(left) || left == ')' || left == '^' || left == '*' || char.IsWhiteSpace(left));
                }

                if (i < expr.Length - 1)
                {
                    char right = expr[i + 1];
                    isOperator = isOperator && (char.IsLetterOrDigit(right) || right == '(' || char.IsWhiteSpace(right));
                }

                if (isOperator)
                {
                    var part = expr.Substring(lastSplit, i - lastSplit).Trim();
                    if (!string.IsNullOrEmpty(part))
                        parts.Add(part);
                    lastSplit = i + 1;
                }
            }
        }

        var lastPart = expr.Substring(lastSplit).Trim();
        if (!string.IsNullOrEmpty(lastPart))
            parts.Add(lastPart);

        return parts.Count > 1 ? parts : new List<string>();
    }

    private List<string> SplitByConcatenation(string expr)
    {
        var parts = new List<string>();
        int i = 0;
        expr = expr.Trim();

        while (i < expr.Length)
        {
            if (char.IsWhiteSpace(expr[i]))
            {
                i++;
                continue;
            }

            if (char.IsLetterOrDigit(expr[i]))
            {
                parts.Add(expr[i].ToString());
                i++;
            }
            else if (expr[i] == '(')
            {
                int level = 1;
                int start = i;
                i++;

                while (i < expr.Length && level > 0)
                {
                    if (expr[i] == '(') level++;
                    else if (expr[i] == ')') level--;
                    i++;
                }

                string subExpr = expr.Substring(start, i - start);
                parts.Add(subExpr);
            }
            else if (expr[i] == '^' || expr[i] == '*')
            {
                if (parts.Count > 0)
                {
                    parts[parts.Count - 1] += expr[i];
                }
                else
                {
                    parts.Add(expr[i].ToString());
                }
                i++;
            }
            else
            {
                i++;
            }
        }

        return parts.Count > 1 ? parts : new List<string>();
    }

    private bool IsEnclosedInParens(string s)
    {
        s = s.Trim();
        if (string.IsNullOrEmpty(s) || s.Length < 2 || s[0] != '(' || s[^1] != ')')
            return false;

        int level = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '(') level++;
            else if (s[i] == ')') level--;

            if (level == 0 && i < s.Length - 1)
                return false;
        }

        return level == 0;
    }

    private void HandleUnion(Node from, Node to, List<string> parts)
    {
        foreach (var part in parts)
        {
            from.AddTransition(to, part.Trim());
        }
    }

    private void HandleKleeneStar(Node from, Node to, string innerExpr)
    {
        var middle = _automaton.CreateNode();

        from.AddTransition(middle, "ε");        // ε переход в новое состояние
        middle.AddTransition(to, "ε");          // ε переход в конечное состояние
        middle.AddTransition(middle, innerExpr); // Цикл с внутренним выражением
    }

    private void HandleConcatenation(Node from, Node to, List<string> parts)
    {
        Node current = from;
        for (int i = 0; i < parts.Count - 1; i++)
        {
            var next = _automaton.CreateNode();
            current.AddTransition(next, parts[i]);
            current = next;
        }
        current.AddTransition(to, parts[parts.Count - 1]);
    }

    private void HandleKleenePlus(Node from, Node to, string inner)
    {
        var middle = _automaton.CreateNode();

        from.AddTransition(middle, inner);     
        middle.AddTransition(to, "ε");         
        middle.AddTransition(middle, inner);   
    }

    private bool IsSimpleSymbol(string s)
    {
        if (string.IsNullOrEmpty(s)) return true;
        return s.Length == 1 && char.IsLetterOrDigit(s[0]);
    }

}