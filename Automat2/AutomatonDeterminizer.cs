using Automat2.Input;

namespace Automat2
{

    public class AutomatonDeterminizer
    {
        private ConsoleHelper _console;

        public AutomatonDeterminizer(ConsoleHelper consoleHelper)
        {
            _console = consoleHelper;
        }

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

        public AutomatonInput RemoveEpsilonTransitions(AutomatonInput automaton, Dictionary<string, HashSet<string>> closures)
        {
            var validInputs = automaton.Inputs.Where(s => s != "e").ToList();

            var newAutomaton = new AutomatonInput
            {
                Inputs = validInputs, 
                Transitions = new Dictionary<(string, string), List<string>>(),
                IndexesStarts = new List<int>(),
                IndexesFinals = new List<int>()
            };

            var stateRenaming = new Dictionary<string, string>();
            for (int i = 0; i < automaton.States.Count; i++)
            {
                stateRenaming[automaton.States[i]] = $"S{i}";
                newAutomaton.States.Add($"S{i}");
            }

            for (int i = 0; i < newAutomaton.States.Count; i++)
            {
                var originalState = automaton.States[i];
                var newStateName = newAutomaton.States[i];

                if (automaton.IndexesStarts.Contains(i))
                {
                    newAutomaton.IndexesStarts.Add(i);
                }

                // Если в ε-замыкании состояния есть хотя бы одно конечное состояние, то новое состояние конечное
                var closure = closures[originalState];
                foreach (var stateInClosure in closure)
                {
                    var originalIndex = automaton.States.IndexOf(stateInClosure);
                    if (automaton.IndexesFinals.Contains(originalIndex))
                    {
                        newAutomaton.IndexesFinals.Add(i);
                        break; 
                    }
                }
            }

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

                    foreach (var closureState in closures[originalState])
                    {
                        if (automaton.Transitions.TryGetValue((closureState, input), out var transitions))
                        {
                            foreach (var nextState in transitions)
                            {
                                reachableStates.UnionWith(closures[nextState]);
                            }
                        }
                    }

                    var newReachableStates = reachableStates
                        .Select(state => stateRenaming[state])
                        .ToList();

                    newAutomaton.Transitions[(newStateName, input)] = newReachableStates;
                }
            }

            _console.WriteColoredLine("=== АВТОМАТ БЕЗ ε-ПЕРЕХОДОВ ===", _console.HighlightColor);
            Console.WriteLine();

            DisplayTransitionTable(newAutomaton);

            _console.WriteColoredLine("НАЧАЛЬНЫЕ СОСТОЯНИЯ:", _console.HighlightColor);
            foreach (var startIndex in newAutomaton.IndexesStarts)
            {
                _console.WriteColoredLine($"  {newAutomaton.States[startIndex]} (индекс {startIndex})", _console.SuccessColor);
            }

            _console.WriteColoredLine("КОНЕЧНЫЕ СОСТОЯНИЯ:", _console.HighlightColor);
            foreach (var finalIndex in newAutomaton.IndexesFinals)
            {
                _console.WriteColoredLine($"  {newAutomaton.States[finalIndex]} (индекс {finalIndex})", _console.SuccessColor);
            }

            return newAutomaton;
        }

        public AutomatonInput Determinize(AutomatonInput automaton)
        {
            var validInputs = automaton.Inputs.Where(s => s != "e").ToList();

            var determinized = new AutomatonInput
            {
                Inputs = validInputs,
                Transitions = new Dictionary<(string, string), List<string>>()
            };

            var initialState = new HashSet<string> { automaton.States[0] };

            var stateSets = new Dictionary<string, HashSet<string>>();
            var stateNames = new Dictionary<HashSet<string>, string>(HashSet<string>.CreateSetComparer());

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

                    foreach (var state in currentSet)
                    {
                        if (automaton.Transitions.TryGetValue((state, input), out var transitions))
                        {
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

                        if (!determinized.Transitions.TryGetValue((nextStateName, input), out var strings))
                        {
                            queue.Enqueue(nextSet);
                        }
                    }
                    else
                    {
                        determinized.Transitions[(currentStateName, input)] = new List<string>();
                    }
                }
            }

            determinized.IndexesStarts.Clear();
            determinized.IndexesFinals.Clear();

            for (int i = 0; i < determinized.States.Count; i++)
            {
                var stateName = determinized.States[i];
                var stateSet = stateSets[stateName];

                if (stateSet.Contains(automaton.States[0]))
                {
                    determinized.IndexesStarts.Add(i);
                }

                foreach (var finalIndex in automaton.IndexesFinals)
                {
                    if (finalIndex < automaton.States.Count && stateSet.Contains(automaton.States[finalIndex]))
                    {
                        determinized.IndexesFinals.Add(i);
                        break; 
                    }
                }
            }

            determinized.States = determinized.States.OrderBy(s => s).ToList();

            _console.WriteColoredLine("=== ДЕТЕРМЕНИЗИРОВАННЫЙ АВТОМАТ ===", _console.HighlightColor);
            DisplayTransitionTable(determinized);
            DisplayStateSets(stateSets);

            _console.WriteColoredLine("НАЧАЛЬНЫЕ СОСТОЯНИЯ:", _console.HighlightColor);
            foreach (var startIndex in determinized.IndexesStarts)
            {
                _console.WriteColoredLine($"  {determinized.States[startIndex]} (индекс {startIndex})", _console.SuccessColor);
            }

            _console.WriteColoredLine("КОНЕЧНЫЕ СОСТОЯНИЯ:", _console.HighlightColor);
            foreach (var finalIndex in determinized.IndexesFinals)
            {
                _console.WriteColoredLine($"  {determinized.States[finalIndex]} (индекс {finalIndex})", _console.SuccessColor);
            }

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
