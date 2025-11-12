using System.Xml.Linq;
using Automat2;
using Automat2.Input;

public class AutomatonConverter
{
    public Automaton ConvertToAutomaton(AutomatonInput input)
    {
        var automaton = new Automaton();

        // Создаем узлы для всех состояний
        var nodeMap = new Dictionary<string, Node>();
        for (int i = 0; i < input.States.Count; i++)
        {
            var state = input.States[i];
            var node = automaton.CreateNode();
            node.Name = state; // Переопределяем имя на то, что было введено
            nodeMap[state] = node;

            // Устанавливаем начальные и конечные состояния по индексам
            if (input.IndexesStarts.Contains(i))
            {
                node.IsStart = true;
            }
            if (input.IndexesFinals.Contains(i))
            {
                node.IsFinal = true; // Исправлено: было IsFalse, должно быть IsFinal
            }
        }

        // Создаем переходы
        foreach (var transition in input.Transitions)
        {
            var fromState = transition.Key.state;
            var inputSymbol = transition.Key.input;
            var toStates = transition.Value;

            foreach (var toState in toStates)
            {
                if (nodeMap.ContainsKey(fromState) && nodeMap.ContainsKey(toState))
                {
                    // Заменяем "e" на "ε" для красивого отображения
                    var displaySymbol = inputSymbol == "e" ? "ε" : inputSymbol;
                    nodeMap[fromState].AddTransition(nodeMap[toState], displaySymbol);
                }
            }
        }

        // Добавляем начальный шаг
        automaton.AddStep($"Автомат построен из матрицы. Состояний: {input.States.Count}, Сигналов: {input.Inputs.Count}");

        return automaton;
    }

    public void DisplayConversionInfo(AutomatonInput input, Automaton automaton)
    {
        Console.Clear();
        var console = new ConsoleHelper();

        console.WriteColoredLine("=== ПРЕОБРАЗОВАНИЕ МАТРИЦЫ В АВТОМАТ ===", console.HighlightColor);
        console.WriteColoredLine($"Исходные состояния: {string.Join(", ", input.States)}", console.TextColor);

        // Показываем все начальные состояния
        var startStates = automaton.Nodes.Where(n => n.IsStart).Select(n => n.Name);
        console.WriteColoredLine($"Начальные состояния: {string.Join(", ", startStates)}", console.SuccessColor);

        // Показываем все конечные состояния
        var finalStates = automaton.Nodes.Where(n => n.IsFinal).Select(n => n.Name);
        console.WriteColoredLine($"Конечные состояния: {string.Join(", ", finalStates)}", console.SuccessColor);

        console.WriteColoredLine("\nПОСТРОЕННЫЕ ПЕРЕХОДЫ:", console.HighlightColor);
        foreach (var node in automaton.Nodes)
        {
            foreach (var (to, expr) in node.Transitions)
            {
                console.WriteColoredLine($"  {node.Name} --{expr}--> {to.Name}", console.TextColor);
            }
        }

        console.WriteColoredLine($"\nВсего узлов: {automaton.Nodes.Count}", console.SuccessColor);
        console.WriteColoredLine($"Всего переходов: {automaton.Nodes.Sum(n => n.Transitions.Count)}", console.SuccessColor);
    }
}