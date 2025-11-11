using Automat2;
using Automat2.Input;

public class AutomatonConverter
{
    public Automaton ConvertToAutomaton(AutomatonInput input)
    {
        var automaton = new Automaton();

        // Создаем узлы для всех состояний
        var nodeMap = new Dictionary<string, Node>();
        foreach (var state in input.States)
        {
            var node = automaton.CreateNode();
            node.Name = state; // Переопределяем имя на то, что было введено
            nodeMap[state] = node;
        }

        // Устанавливаем начальное и конечное состояния по ИМЕНАМ
        if (input.States.Count > 0)
        {
            automaton.Start = nodeMap[input.States[0]]; // Первое состояние - стартовое
            automaton.Final = nodeMap[input.States[input.States.Count - 1]]; // Последнее - финальное
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
        console.WriteColoredLine($"Начальное состояние: {automaton.Start.Name}", console.SuccessColor);
        console.WriteColoredLine($"Финальное состояние: {automaton.Final.Name}", console.SuccessColor);

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

        // Отладочная информация
        console.WriteColoredLine($"\n[Отладка] ID начального: {automaton.Start.Id}, ID финального: {automaton.Final.Id}", console.TextColor);
        console.WriteColoredLine($"[Отладка] Все узлы: {string.Join(", ", automaton.Nodes.Select(n => $"{n.Name}(id:{n.Id})"))}", console.TextColor);
    }
}