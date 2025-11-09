using System.Diagnostics;

public class AutomatonVisualizer
{
    public void RenderSteps(Automaton automaton, string outputDir = "automaton_steps")
    {
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        for (int i = 0; i < automaton.StepHistory.Count; i++)
        {
            var step = automaton.StepHistory[i];
            var dotPath = Path.Combine(outputDir, $"step_{i:D3}.dot");
            var pngPath = Path.Combine(outputDir, $"step_{i:D3}.png");

            SaveToDot(step, automaton, dotPath);

            GeneratePngFromDot(dotPath, pngPath);

            Console.WriteLine($"Шаг {i}: {step.Comment}");
        }
        OpenOutputFolder(outputDir);
    }

    private void SaveToDot(StepSnapshot step, Automaton automaton, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("digraph G {");
            writer.WriteLine("  rankdir=LR;");
            writer.WriteLine("  node [shape = circle];");

            // Специальные стили для начального и конечного состояний
            writer.WriteLine($"  {automaton.Start.Name} [label=\"start\", shape=doublecircle, style=bold, color=blue];");
            writer.WriteLine($"  {automaton.Final.Name} [label=\"final\", shape=doublecircle, peripheries=2, style=bold, color=red];");

            // Обычные узлы
            foreach (var node in step.Nodes)
            {
                if (node.Id != automaton.Start.Id && node.Id != automaton.Final.Id)
                {
                    writer.WriteLine($"  {node.Name} [label=\"{node.Name}\", shape=circle];");
                }
            }

            // Переходы
            foreach (var node in step.Nodes)
            {
                foreach (var (to, expr) in node.Transitions)
                {
                    var label = string.IsNullOrEmpty(expr) ? "ε" : expr;
                    writer.WriteLine($"  {node.Name} -> {to.Name} [label=\"{label}\"];");
                }
            }

            writer.WriteLine("}");
        }
    }

    private void GeneratePngFromDot(string dotPath, string pngPath)
    {
        var dotExePath = @"C:\Program Files\Graphviz\bin\dot.exe";

        // Проверяем существование Graphviz
        if (!File.Exists(dotExePath))
        {
            // Попробуем найти в других возможных местах
            dotExePath = "dot.exe"; // Если добавлен в PATH
        }

        var start = new ProcessStartInfo
        {
            FileName = dotExePath,
            Arguments = $"-Tpng \"{dotPath}\" -o \"{pngPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        try
        {
            using (var process = Process.Start(start))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Warning: dot завершился с ошибкой. Проверьте установку Graphviz.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при генерации PNG: {ex.Message}");
            Console.WriteLine("Убедитесь, что Graphviz установлен и доступен в PATH");
        }
    }
    public void OpenOutputFolder(string outputDir = "automaton_steps")
    {
        if (!Directory.Exists(outputDir))
        {
            Console.WriteLine($"Папка {outputDir} не существует.");
            return;
        }

        try
        {
            Process.Start("explorer.exe", outputDir);
            Console.WriteLine($"Открыта папка с результатами: {Path.GetFullPath(outputDir)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось открыть папку: {ex.Message}");
        }
    }
}