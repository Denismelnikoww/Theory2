using Automat2.Input;

namespace Automat2
{
    partial class Program
    {
        public class AutomatonDeterminizer
        {
            private ConsoleHelper _console;

            public AutomatonDeterminizer(ConsoleHelper consoleHelper)
            {
                _console = consoleHelper;
            }

            // Шаг 1: Построение ε-замыканий для всех состояний
            public Dictionary<string, HashSet<string>> BuildEpsilonClosures(AutomatonInput automaton)
            {
                var closures = new Dictionary<string, HashSet<string>>();

                foreach (var state in automaton.States)
                {
                    closures[state] = GetEpsilonClosure(state, automaton);
                }

                _console.WriteColoredLine("=== ε-ЗАМЫКАНИЯ ===", _console.HighlightColor);
                foreach (var closure in closures)
                {
                    _console.WriteColoredLine($"  E({closure.Key}) = {{{string.Join(", ", closure.Value)}}}", _console.TextColor);
                }

                return closures;
            }

            private HashSet<string> GetEpsilonClosure(string state, AutomatonInput automaton)
            {
                var closure = new HashSet<string> { state };
                var stack = new Stack<string>();
                stack.Push(state);

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    // Ищем все ε-переходы из текущего состояния
                    if (automaton.Transitions.TryGetValue((current, "e"), out var epsilonTransitions))
                    {
                        foreach (var nextState in epsilonTransitions)
                        {
                            if (!closure.Contains(nextState))
                            {
                                closure.Add(nextState);
                                stack.Push(nextState);
                            }
                        }
                    }
                }

                return closure;
            }


            // Шаг 2: Построение автомата без ε-переходов с переименованием вершин
            public AutomatonInput RemoveEpsilonTransitions(AutomatonInput automaton, Dictionary<string, HashSet<string>> closures)
            {
                // Убираем ε из входных сигналов
                var validInputs = automaton.Inputs.Where(s => s != "e").ToList();

                var newAutomaton = new AutomatonInput
                {
                    Inputs = validInputs, // Только валидные сигналы
                    Transitions = new Dictionary<(string, string), List<string>>()
                };

                // Переименовываем состояния в S0, S1, S2, ...
                var stateRenaming = new Dictionary<string, string>();
                for (int i = 0; i < automaton.States.Count; i++)
                {
                    stateRenaming[automaton.States[i]] = $"S{i}";
                    newAutomaton.States.Add($"S{i}");
                }

                // Инициализируем все переходы только для валидных сигналов
                foreach (var newState in newAutomaton.States)
                {
                    foreach (var input in validInputs)
                    {
                        newAutomaton.Transitions[(newState, input)] = new List<string>();
                    }
                }

                // Строим новые переходы через ε-замыкания
                foreach (var originalState in automaton.States)
                {
                    var newStateName = stateRenaming[originalState];

                    foreach (var input in validInputs)
                    {
                        var reachableStates = new HashSet<string>();

                        // Для каждого состояния в ε-замыкании
                        foreach (var closureState in closures[originalState])
                        {
                            // Ищем переходы по текущему символу
                            if (automaton.Transitions.TryGetValue((closureState, input), out var transitions))
                            {
                                foreach (var nextState in transitions)
                                {
                                    // Добавляем все состояния из ε-замыкания целевого состояния
                                    reachableStates.UnionWith(closures[nextState]);
                                }
                            }
                        }

                        // Преобразуем имена состояний в новые
                        var newReachableStates = reachableStates
                            .Select(state => stateRenaming[state])
                            .ToList();

                        newAutomaton.Transitions[(newStateName, input)] = newReachableStates;
                    }
                }

                _console.WriteColoredLine("=== АВТОМАТ БЕЗ ε-ПЕРЕХОДОВ ===", _console.HighlightColor);
                _console.WriteColoredLine("Переименование состояний:", _console.TextColor);
                foreach (var rename in stateRenaming)
                {
                    _console.WriteColoredLine($"  {rename.Key} -> {rename.Value}", _console.TextColor);
                }
                Console.WriteLine();

                DisplayTransitionTable(newAutomaton);

                return newAutomaton;
            }

            // Шаг 3: Детерминизация автомата
            public AutomatonInput Determinize(AutomatonInput automaton)
            {
                // Убираем ε из входных сигналов, так как ε-переходы уже устранены
                var validInputs = automaton.Inputs.Where(s => s != "e").ToList();

                var determinized = new AutomatonInput
                {
                    Inputs = validInputs, // Используем только валидные сигналы
                    Transitions = new Dictionary<(string, string), List<string>>()
                };

                // Начальное состояние - множество из исходного начального состояния
                var initialState = new HashSet<string> { automaton.States[0] };
                var stateSets = new Dictionary<string, HashSet<string>>();
                var stateNames = new Dictionary<HashSet<string>, string>();

                // Генерируем имена состояний в алфавитном порядке
                char currentChar = 'A';
                string GetStateName(HashSet<string> set)
                {
                    if (stateNames.ContainsKey(set))
                        return stateNames[set];

                    var name = currentChar.ToString();
                    stateNames[set] = name;
                    stateSets[name] = set;
                    determinized.States.Add(name);
                    currentChar++;
                    return name;
                }

                var queue = new Queue<HashSet<string>>();
                queue.Enqueue(initialState);
                GetStateName(initialState);

                while (queue.Count > 0)
                {
                    var currentSet = queue.Dequeue();
                    var currentStateName = stateNames[currentSet];

                    foreach (var input in validInputs)
                    {
                        var nextSet = new HashSet<string>();

                        // Для каждого состояния в текущем множестве
                        foreach (var state in currentSet)
                        {
                            if (automaton.Transitions.TryGetValue((state, input), out var transitions))
                            {
                                // Добавляем ВСЕ состояния, в которые можно перейти
                                foreach (var nextState in transitions)
                                {
                                    nextSet.Add(nextState);
                                }
                            }
                        }

                        if (nextSet.Count > 0)
                        {
                            var nextStateName = GetStateName(nextSet);
                            determinized.Transitions[(currentStateName, input)] = new List<string> { nextStateName };

                            // Если это новое множество, добавляем в очередь для обработки
                            if (!stateNames.ContainsKey(nextSet))
                            {
                                queue.Enqueue(nextSet);
                            }
                        }
                        else
                        {
                            // Нет переходов - пустое множество
                            determinized.Transitions[(currentStateName, input)] = new List<string>();
                        }
                    }
                }

                // Определяем начальное и конечные состояния
                determinized.States = determinized.States.OrderBy(s => s).ToList();


                _console.WriteColoredLine("=== ДЕТЕРМЕНИЗИРОВАННЫЙ АВТОМАТ ===", _console.HighlightColor);
                DisplayTransitionTable(determinized);
                DisplayStateSets(stateSets);


                return determinized;
            }
            private void DisplayTransitionTable(AutomatonInput automaton)
            {
                _console.WriteColoredLine("Таблица переходов:", _console.TextColor);

                // Заголовок
                Console.Write("Состояние".PadRight(12));
                foreach (var input in automaton.Inputs)
                {
                    Console.Write(input.PadRight(10));
                }
                Console.WriteLine();

                Console.WriteLine(new string('-', 12 + automaton.Inputs.Count * 10));

                // Строки
                foreach (var state in automaton.States)
                {
                    Console.Write(state.PadRight(12));
                    foreach (var input in automaton.Inputs)
                    {
                        var transitions = automaton.Transitions.TryGetValue((state, input), out var trans)
                            ? string.Join(",", trans)
                            : "-";
                        Console.Write(transitions.PadRight(10));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            private void DisplayStateSets(Dictionary<string, HashSet<string>> stateSets)
            {
                _console.WriteColoredLine("МНОЖЕСТВА СОСТОЯНИЙ:", _console.HighlightColor);
                foreach (var stateSet in stateSets)
                {
                    _console.WriteColoredLine($"  {stateSet.Key} = {{{string.Join(", ", stateSet.Value)}}}", _console.TextColor);
                }
            }

        }
    }
}
