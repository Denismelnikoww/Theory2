
using Automat2.Input;

namespace Automat2
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //var builder = new AutomatonBuilder();
        //var automaton = builder.Build("a* + (a+b*)^ a + (ab)^");

        //var visualizer = new AutomatonVisualizer();
        //visualizer.RenderSteps(automaton);
        //}

        static void Main(string[] args)
        {
            var consoleHelper = new ConsoleHelper();
            var matrixInput = new MatrixInputHelper(consoleHelper);
            var converter = new AutomatonConverter();

            Automaton automaton = new();

            var menu = new Dictionary<string, Action>
            {
                ["Ввести автомат матрицей"] = () =>
                {
                    var automatonInput = matrixInput.InputAutomatonMatrix();
                    matrixInput.DisplayAutomatonInfo(automatonInput);
                    consoleHelper.Pause();

                    automaton = converter.ConvertToAutomaton(automatonInput);
                    converter.DisplayConversionInfo(automatonInput, automaton);
                    consoleHelper.Pause();

                    var visualizer = new AutomatonVisualizer();
                    visualizer.RenderSteps(automaton);
                },
                ["Мой вариант"] = () =>
                {
                    var input = CreateAutomatonInput();
                    matrixInput.DisplayAutomatonInfo(input);
                    consoleHelper.Pause();

                    automaton = converter.ConvertToAutomaton(input);
                    converter.DisplayConversionInfo(input, automaton);
                    consoleHelper.Pause();

                    var visualizer = new AutomatonVisualizer();
                    visualizer.RenderSteps(automaton);
                }
            };

            consoleHelper.Menu(menu, "ГЛАВНОЕ МЕНЮ - ТЕОРИЯ АВТОМАТОВ");
        }

        static AutomatonInput CreateAutomatonInput()
        {
            var automaton = new AutomatonInput
            {
                Inputs = new List<string> { "0", "1", "e" },
                States = new List<string> { "q0", "q1", "qf" },
                Transitions = new Dictionary<(string, string), List<string>>()
            };

            foreach (var state in automaton.States)
            {
                foreach (var input in automaton.Inputs)
                {
                    automaton.Transitions[(state, input)] = new List<string>();
                }
            }

            automaton.Transitions[("q0", "0")] = new List<string> { "q1" };
            automaton.Transitions[("q0", "1")] = new List<string> { "qf" };
            automaton.Transitions[("q1", "e")] = new List<string> { "qf" };
            automaton.Transitions[("qf", "0")] = new List<string> { "qf" };
            automaton.Transitions[("qf", "1")] = new List<string> { "q1" };

            return automaton;
        }
    }
}
