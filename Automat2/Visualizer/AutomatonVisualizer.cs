using System.Diagnostics;

public class AutomatonVisualizer
{
    private readonly string outputDir = "automaton_steps";
    public void RenderSteps(Automaton automaton, string name = null, bool deleteDirectory = true)
    {
        if (deleteDirectory && Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }
        Directory.CreateDirectory(outputDir);

        for (int i = 0; i < automaton.StepHistory.Count; i++)
        {
            var step = automaton.StepHistory[i];
            var dotPath = Path.Combine(outputDir, (name ?? $"step_{i:D3}") + ".dot");
            var pngPath = Path.Combine(outputDir, (name ?? $"step_{i:D3}") + ".png");

            SaveToDot(step, dotPath, i);

            GeneratePngFromDot(dotPath, pngPath);

            Console.WriteLine($"Шаг {i}: {step.Comment}");
        }

        OpenOutputFolder(outputDir);
    }

    private void SaveToDot(StepSnapshot step, string filePath, int stepNumber)
    {
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("digraph G {");
            writer.WriteLine("  rankdir=LR;");
            writer.WriteLine("  node [shape = circle];");

            writer.WriteLine($"  labelloc=\"t\";");
            writer.WriteLine($"  label=\"Шаг {stepNumber}: {step.Comment}\";");

            // Находим начальный и конечный узлы в snapshot по ИМЕНАМ из StepSnapshot
            var startNodeInSteps = step.Nodes.Where(n => n.IsStart);
            var finalNodeInSteps = step.Nodes.Where(n => n.IsFinal);

            if (startNodeInSteps != null && startNodeInSteps.Count() > 0)
            {
                foreach (var node in startNodeInSteps)
                {
                    writer.WriteLine($"  {node.Name} [label=\"{node.Name}\", shape=doublecircle, style=bold, color=blue];");
                }
            }

            if (finalNodeInSteps != null && finalNodeInSteps.Count() > 0)
            {
                foreach (var node in finalNodeInSteps)
                {
                    writer.WriteLine($"  {node.Name} [label=\"{node.Name}\", shape=doublecircle, peripheries=2, style=bold, color=red];");
                }
            }

            // Обычные узлы (которые не являются начальными или конечными)
            foreach (var node in step.Nodes)
            {
                if (!node.IsFinal && !node.IsStart)
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