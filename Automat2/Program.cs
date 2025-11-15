
using Automat2.Input;

namespace Automat2
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var builder = new AutomatonBuilder();
            var automaton = builder.Build("a* + (a+b*)^ a + (ab)^");

            var visualizer = new AutomatonVisualizer();
            visualizer.RenderSteps(automaton);
        }

        //static void Main(string[] args)
        //{
        //    var consoleHelper = new ConsoleHelper();
        //    var matrixInput = new MatrixInputHelper(consoleHelper);
        //    var converter = new AutomatonConverter();
        //    var determinizer = new AutomatonDeterminizer(consoleHelper);
        //    var visualizer = new AutomatonVisualizer();
        //    var automatonInput = new AutomatonInput();
        //    var flagAutomaton = false;


        //    Automaton automaton = new();

        //    var menu = new Dictionary<string, Action>
        //    {
        //        ["Ввести автомат матрицей"] = () =>
        //        {
        //            automatonInput = matrixInput.InputAutomatonMatrix();
        //            matrixInput.DisplayAutomatonInfo(automatonInput);
        //            consoleHelper.Pause();

        //            automaton = converter.ConvertToAutomaton(automatonInput);
        //            converter.DisplayConversionInfo(automatonInput, automaton);
        //            consoleHelper.Pause();

        //            visualizer.RenderSteps(automaton);
        //            flagAutomaton = true;
        //        },
        //        ["Детерминизировать автомат"] = () =>
        //        {
        //            if (!flagAutomaton)
        //                return;

        //            var automatonWithoutEpsilon = determinizer.RemoveEpsilonTransitions(automatonInput, determinizer.BuildEpsilonClosures(automatonInput));
        //            visualizer.RenderSteps(
        //                converter.ConvertToAutomaton(automatonWithoutEpsilon),
        //                "automatonWithoutEpsilon",
        //                deleteDirectory: false);
        //            consoleHelper.Pause();

        //            var determinizedAutomaton = determinizer.Determinize(automatonWithoutEpsilon);
        //            visualizer.RenderSteps(
        //                converter.ConvertToAutomaton(determinizedAutomaton),
        //                "determinized",
        //                deleteDirectory: false);
        //            consoleHelper.Pause();
        //            automaton = converter.ConvertToAutomaton(determinizedAutomaton);
        //        },
        //        ["Мой вариант"] = () =>
        //            {
        //                automatonInput = CreateAutomatonInput();
        //                matrixInput.DisplayAutomatonInfo(automatonInput);
        //                consoleHelper.Pause();

        //                automaton = converter.ConvertToAutomaton(automatonInput);
        //                converter.DisplayConversionInfo(automatonInput, automaton);
        //                consoleHelper.Pause();

        //                visualizer.RenderSteps(automaton);
        //                flagAutomaton = true;
        //            },
        //        ["Подать сигнал"] = () =>
        //        {
        //            if (!flagAutomaton)
        //                return;

        //            automaton.ProcessInput(consoleHelper.ReadString("Введите входной сигнал \n>> "));
        //        }
        //    };

        //    consoleHelper.Menu(menu, "ГЛАВНОЕ МЕНЮ - ТЕОРИЯ АВТОМАТОВ");
        //}

        static AutomatonInput CreateAutomatonInput()
        {
            var automaton = new AutomatonInput
            {
                Inputs = new List<string> { "0", "1", "e" },
                States = new List<string> { "q0", "q1", "qf" },
                Transitions = new Dictionary<(string, string), List<string>>(),
                IndexesFinals = {2},
                IndexesStarts = {0}
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
