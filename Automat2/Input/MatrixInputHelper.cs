namespace Automat2.Input
{
    public class MatrixInputHelper
    {
        private ConsoleHelper _console;
        private AutomatonInput _automaton;

        public MatrixInputHelper(ConsoleHelper consoleHelper)
        {
            _console = consoleHelper;
            _automaton = new AutomatonInput();
        }

        public AutomatonInput InputAutomatonMatrix()
        {
            _automaton = new AutomatonInput();

            InputInputSignals();
            InputStates();
            InputTransitions();
            InputInitialStates(); // Новый метод для начальных состояний
            InputFinalStates();   // Новый метод для конечных состояний

            return _automaton;
        }

        private void InputInputSignals()
        {
            _console.WriteColoredLine("=== ВВОД ВХОДНЫХ СИГНАЛОВ ===", _console.HighlightColor);
            _console.WriteColoredLine("Введите входные сигналы (символы) через пробел:", _console.TextColor);
            _console.WriteColoredLine("Для ε-переходов используйте символ 'ε' или 'e'", _console.TextColor);
            _console.WriteColored("Пример: ε a b\n> ", _console.TextColor);

            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                _automaton.Inputs = input.Split(' ')
                                       .Select(s => s.Trim())
                                       .Where(s => !string.IsNullOrEmpty(s))
                                       .Select(s => s.ToLower() == "e" ? "ε" : s) // Заменяем 'e' на 'ε'
                                       .ToList();
            }

            _console.WriteColoredLine($"Введено сигналов: {_automaton.Inputs.Count}", _console.SuccessColor);
        }

        private void InputStates()
        {
            _console.WriteColoredLine("=== ВВОД СОСТОЯНИЙ ===", _console.HighlightColor);
            int stateCount = _console.ReadInt("Введите количество состояний: ", 1, 20);

            _automaton.States.Clear();
            for (int i = 0; i < stateCount - 1; i++)
            {
                string stateName = $"q{i}";
                _automaton.States.Add(stateName);
            }

            string finalState = "qf";
            _automaton.States.Add(finalState);

            _console.WriteColoredLine($"Автоматически созданы состояния: {string.Join(", ", _automaton.States)}", _console.SuccessColor);
        }

        private void InputTransitions()
        {
            _console.WriteColoredLine("=== ВВОД ПЕРЕХОДОВ ===", _console.HighlightColor);

            // Инициализируем все переходы пустыми списками
            foreach (var state in _automaton.States)
            {
                foreach (var input in _automaton.Inputs)
                {
                    _automaton.Transitions[(state, input)] = new List<string>();
                }
            }

            int currentRow = 0;
            int currentCol = 0;
            bool editing = true;

            while (editing)
            {
                Console.Clear();
                DisplayTransitionTable(currentRow, currentCol);

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        currentRow = (currentRow - 1 + _automaton.States.Count) % _automaton.States.Count;
                        break;
                    case ConsoleKey.DownArrow:
                        currentRow = (currentRow + 1) % _automaton.States.Count;
                        break;
                    case ConsoleKey.LeftArrow:
                        currentCol = (currentCol - 1 + _automaton.Inputs.Count) % _automaton.Inputs.Count;
                        break;
                    case ConsoleKey.RightArrow:
                        currentCol = (currentCol + 1) % _automaton.Inputs.Count;
                        break;
                    case ConsoleKey.Enter:
                        EditCell(_automaton.States[currentRow], _automaton.Inputs[currentCol]);
                        break;
                    case ConsoleKey.Escape:
                        editing = false;
                        break;
                }
            }

            _console.WriteColoredLine("Ввод переходов завершен!", _console.SuccessColor);
        }

        private void InputInitialStates()
        {
            _console.WriteColoredLine("=== ВЫБОР НАЧАЛЬНЫХ СОСТОЯНИЙ ===", _console.HighlightColor);
            _console.WriteColoredLine("Выберите начальные состояния (может быть несколько):", _console.TextColor);

            SelectStates(_automaton.States, _automaton.IndexesStarts, "начальных");
        }

        private void InputFinalStates()
        {
            _console.WriteColoredLine("=== ВЫБОР КОНЕЧНЫХ СОСТОЯНИЙ ===", _console.HighlightColor);
            _console.WriteColoredLine("Выберите конечные состояния (может быть несколько):", _console.TextColor);

            SelectStates(_automaton.States, _automaton.IndexesFinals, "конечных");
        }

        private void SelectStates(List<string> states, List<int> selectedIndexes, string stateType)
        {
            int selectedIndex = 0;
            bool selecting = true;
            var selectedStates = new List<string>(selectedIndexes.Select(i => states[i]));

            while (selecting)
            {
                Console.Clear();
                _console.WriteColoredLine($"ВЫБОР {stateType.ToUpper()} СОСТОЯНИЙ", _console.HighlightColor);
                _console.WriteColoredLine("Используйте стрелки для навигации, Enter - выбрать/снять выделение, ✓ - завершить выбор",
                                        _console.TextColor);
                Console.WriteLine();

                // Отображаем все состояния
                for (int i = 0; i < states.Count; i++)
                {
                    var state = states[i];
                    bool isSelected = selectedStates.Contains(state);

                    if (i == selectedIndex)
                    {
                        var marker = isSelected ? "[✓]" : "[ ]";
                        _console.WriteColored($"{marker} {state}", _console.HighlightColor);
                    }
                    else
                    {
                        var marker = isSelected ? "[✓]" : "[ ]";
                        Console.Write($"{marker} {state}");
                    }
                    Console.WriteLine();
                }

                // Отображаем вариант "Завершить выбор"
                var finishIndex = states.Count;
                if (selectedIndex == finishIndex)
                {
                    _console.WriteColoredLine("[✓] Завершить выбор", _console.HighlightColor);
                }
                else
                {
                    Console.WriteLine("[ ] Завершить выбор");
                }

                Console.WriteLine();
                _console.WriteColoredLine($"Выбрано {stateType} состояний: {(selectedStates.Count == 0 ? "нет" : string.Join(", ", selectedStates))}",
                                        _console.SuccessColor);

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + states.Count + 1) % (states.Count + 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % (states.Count + 1);
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex < states.Count)
                        {
                            // Выбор/отмена выбора состояния
                            var stateToToggle = states[selectedIndex];
                            if (selectedStates.Contains(stateToToggle))
                            {
                                selectedStates.Remove(stateToToggle);
                            }
                            else
                            {
                                selectedStates.Add(stateToToggle);
                            }
                        }
                        else
                        {
                            // Завершение выбора
                            selecting = false;
                        }
                        break;
                    case ConsoleKey.Escape:
                        selecting = false;
                        break;
                }
            }

            // Сохраняем выбранные индексы
            selectedIndexes.Clear();
            foreach (var state in selectedStates)
            {
                selectedIndexes.Add(states.IndexOf(state));
            }

            _console.WriteColoredLine($"Сохранено {stateType} состояний: {(selectedStates.Count == 0 ? "нет" : string.Join(", ", selectedStates))}",
                                    _console.SuccessColor);
            _console.Pause("Нажмите любую клавишу чтобы продолжить...");
        }

        private void DisplayTransitionTable(int selectedRow, int selectedCol)
        {
            _console.WriteColoredLine("ТАБЛИЦА ПЕРЕХОДОВ (используйте стрелки для навигации, Enter - редактировать, Esc - завершить)",
                                    _console.HighlightColor);
            Console.WriteLine();

            // Заголовок с входными сигналами
            Console.Write("".PadRight(15));
            foreach (var input in _automaton.Inputs)
            {
                if (_automaton.Inputs.IndexOf(input) == selectedCol)
                {
                    _console.WriteColored(input.PadRight(10), _console.HighlightColor);
                }
                else
                {
                    Console.Write(input.PadRight(10));
                }
            }
            Console.WriteLine();

            Console.WriteLine(new string('-', 15 + _automaton.Inputs.Count * 10));

            // Строки с состояниями
            for (int i = 0; i < _automaton.States.Count; i++)
            {
                var state = _automaton.States[i];

                if (i == selectedRow)
                {
                    _console.WriteColored(state.PadRight(15), _console.HighlightColor);
                }
                else
                {
                    Console.Write(state.PadRight(15));
                }

                for (int j = 0; j < _automaton.Inputs.Count; j++)
                {
                    var input = _automaton.Inputs[j];
                    var transitions = _automaton.Transitions[(state, input)];
                    var cellText = transitions.Count == 0 ? "-" : string.Join(",", transitions);

                    if (i == selectedRow && j == selectedCol)
                    {
                        _console.WriteColored(cellText.PadRight(10), _console.HighlightColor);
                    }
                    else
                    {
                        Console.Write(cellText.PadRight(10));
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            _console.WriteColoredLine($"Выбрано: [{_automaton.States[selectedRow]}, {_automaton.Inputs[selectedCol]}]",
                                    _console.TextColor);
        }

        private void EditCell(string state, string input)
        {
            var currentTransitions = _automaton.Transitions[(state, input)];

            _console.WriteColoredLine($"Редактирование перехода: состояние '{state}' по сигналу '{input}'",
                                    _console.HighlightColor);
            _console.WriteColoredLine($"Текущие переходы: {(currentTransitions.Count == 0 ? "-" : string.Join(", ", currentTransitions))}",
                                    _console.TextColor);

            var selectedStates = new List<string>(currentTransitions);
            int selectedIndex = 0;
            bool selecting = true;

            while (selecting)
            {
                Console.Clear();
                _console.WriteColoredLine($"Выберите состояния для перехода (State: {state}, Input: {input})",
                                        _console.HighlightColor);
                _console.WriteColoredLine("Используйте стрелки для выбора, Enter - добавить/удалить, ✓ - завершить",
                                        _console.TextColor);
                Console.WriteLine();

                // Отображаем все доступные состояния
                for (int i = 0; i < _automaton.States.Count; i++)
                {
                    var availableState = _automaton.States[i];
                    bool isSelected = selectedStates.Contains(availableState);

                    if (i == selectedIndex)
                    {
                        var marker = isSelected ? "[✓]" : "[ ]";
                        _console.WriteColored($"{marker} {availableState}", _console.HighlightColor);
                    }
                    else
                    {
                        var marker = isSelected ? "[✓]" : "[ ]";
                        Console.Write($"{marker} {availableState}");
                    }
                    Console.WriteLine();
                }

                // Отображаем вариант "Завершить выбор"
                var finishIndex = _automaton.States.Count;
                if (selectedIndex == finishIndex)
                {
                    _console.WriteColoredLine("[✓] Завершить выбор", _console.HighlightColor);
                }
                else
                {
                    Console.WriteLine("[ ] Завершить выбор");
                }

                Console.WriteLine();
                _console.WriteColoredLine($"Выбрано: {(selectedStates.Count == 0 ? "-" : string.Join(", ", selectedStates))}",
                                        _console.SuccessColor);

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + _automaton.States.Count + 1) % (_automaton.States.Count + 1);
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % (_automaton.States.Count + 1);
                        break;
                    case ConsoleKey.Enter:
                        if (selectedIndex < _automaton.States.Count)
                        {
                            // Выбор/отмена выбора состояния
                            var stateToToggle = _automaton.States[selectedIndex];
                            if (selectedStates.Contains(stateToToggle))
                            {
                                selectedStates.Remove(stateToToggle);
                            }
                            else
                            {
                                selectedStates.Add(stateToToggle);
                            }
                        }
                        else
                        {
                            // Завершение выбора
                            selecting = false;
                        }
                        break;
                    case ConsoleKey.Escape:
                        selecting = false;
                        break;
                }
            }

            // Сохраняем выбранные переходы
            _automaton.Transitions[(state, input)] = selectedStates;

            _console.WriteColoredLine($"Сохранено: {state} --{input}--> {(selectedStates.Count == 0 ? "-" : string.Join(", ", selectedStates))}",
                                    _console.SuccessColor);
            _console.Pause("Нажмите любую клавишу чтобы вернуться к таблице...");
        }

        public void DisplayAutomatonInfo(AutomatonInput automaton)
        {
            Console.Clear();
            _console.WriteColoredLine("=== ИНФОРМАЦИЯ О ВВЕДЕННОМ АВТОМАТЕ ===", _console.HighlightColor);

            _console.WriteColoredLine($"Входные сигналы: {string.Join(", ", automaton.Inputs)}", _console.TextColor);
            _console.WriteColoredLine($"Состояния: {string.Join(", ", automaton.States)}", _console.TextColor);

            // Начальные состояния
            var initialStates = automaton.IndexesStarts.Select(i => automaton.States[i]).ToList();
            _console.WriteColoredLine($"Начальные состояния: {(initialStates.Count == 0 ? "нет" : string.Join(", ", initialStates))}",
                                    _console.TextColor);

            // Конечные состояния
            var finalStates = automaton.IndexesFinals.Select(i => automaton.States[i]).ToList();
            _console.WriteColoredLine($"Конечные состояния: {(finalStates.Count == 0 ? "нет" : string.Join(", ", finalStates))}",
                                    _console.TextColor);

            _console.WriteColoredLine("\nПЕРЕХОДЫ:", _console.HighlightColor);
            foreach (var state in automaton.States)
            {
                foreach (var input in automaton.Inputs)
                {
                    if (automaton.Transitions.TryGetValue((state, input), out var transitions) && transitions.Count > 0)
                    {
                        _console.WriteColoredLine($"  {state} --{input}--> {string.Join(", ", transitions)}",
                                                _console.TextColor);
                    }
                }
            }
        }
    }
}
