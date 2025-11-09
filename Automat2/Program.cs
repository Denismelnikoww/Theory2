
namespace Automat2
{

    class Program
    {
        static void Main(string[] args)
        {
            var builder = new AutomatonBuilder();
            var automaton = builder.Build("a* + (a+b*)^ a + (ab)^");

            var visualizer = new AutomatonVisualizer();
            visualizer.RenderSteps(automaton);
        }
    }
}
