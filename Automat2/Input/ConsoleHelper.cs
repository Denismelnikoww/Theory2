
namespace Automat2
{
    public class ConsoleHelper
        {
            public string ConsoleTitle { get; set; } = "Лабораторная работа 1";
            public ConsoleColor TextColor { get; set; } = ConsoleColor.White;
            public ConsoleColor HighlightColor { get; set; } = ConsoleColor.Magenta;
            public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
            public ConsoleColor SuccessColor { get; set; } = ConsoleColor.Green;

            public ConsoleHelper()
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.Title = ConsoleTitle;
            }

            public void Menu(Dictionary<string, Action> menuItems, string text = "МЕНЮ")
            {
                var selectedIndex = 0;
                var menuActive = true;

                while (menuActive)
                {
                    Console.Clear();
                    WriteColoredLine($"=== {text} ===", HighlightColor);
                    Console.WriteLine();

                    for (int i = 0; i < menuItems.Count; i++)
                    {
                        if (i == selectedIndex)
                        {
                            WriteColoredLine($" ► {GetKeyByIndex(menuItems, i)}", HighlightColor);
                        }
                        else
                        {
                            Console.WriteLine($"   {GetKeyByIndex(menuItems, i)}");
                        }
                    }

                    Console.WriteLine("\n↑↓ для выбора, Enter - выполнить, Esc - выход");

                    var key = Console.ReadKey(true).Key;

                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            selectedIndex = (selectedIndex - 1 + menuItems.Count) % menuItems.Count;
                            break;
                        case ConsoleKey.DownArrow:
                            selectedIndex = (selectedIndex + 1) % menuItems.Count;
                            break;
                        case ConsoleKey.Enter:
                            Console.Clear();
                            GetActionByIndex(menuItems, selectedIndex)?.Invoke();
                            Pause();
                            break;
                        case ConsoleKey.Escape:
                            menuActive = false;
                            break;
                    }
                }
            }

            public void WriteColored(string text, ConsoleColor color = ConsoleColor.Magenta)
            {
                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ResetColor();
            }

            public void WriteColoredLine(string text, ConsoleColor color = ConsoleColor.Magenta)
            {
                WriteColored(text, color);
                Console.WriteLine();
            }

            public void Pause(string message = "Нажмите любую клавишу для продолжения...")
            {
                WriteColoredLine(message, HighlightColor);
                Console.ReadKey();
            }

            #region ReadWithValidate
            public int ReadInt(string text, int min = int.MinValue, int max = int.MaxValue)
            {
                while (true)
                {
                    WriteColored(text, TextColor);
                    if (int.TryParse(Console.ReadLine(), out int result) && result >= min && result <= max)
                    {
                        return result;
                    }
                    WriteColoredLine($"Ошибка! Введите число от {min} до {max}", ErrorColor);
                }
            }

            public double ReadDouble(string text, double min = double.MinValue, double max = double.MaxValue)
            {
                while (true)
                {
                    WriteColored(text, TextColor);
                    if (double.TryParse(Console.ReadLine(), out double result) && result >= min && result <= max)
                    {
                        return result;
                    }
                    WriteColoredLine($"Ошибка! Введите число от {min} до {max}", ErrorColor);
                }
            }

            public string ReadString(string text, bool allowEmpty = false)
            {
                while (true)
                {
                    WriteColored(text, TextColor);
                    string input = Console.ReadLine()?.Trim() ?? "";

                    if (!allowEmpty && string.IsNullOrEmpty(input))
                    {
                        WriteColoredLine("Ошибка! Поле не может быть пустым", ErrorColor);
                        continue;
                    }

                    return input;
                }
            }
            #endregion

            private string GetKeyByIndex(Dictionary<string, Action> dict, int index)
            {
                int i = 0;
                foreach (var key in dict.Keys)
                {
                    if (i++ == index) return key;
                }
                return "";
            }

            private Action GetActionByIndex(Dictionary<string, Action> dict, int index)
            {
                int i = 0;
                foreach (var value in dict.Values)
                {
                    if (i++ == index) return value;
                }
                return null;
            }
        }
    }
