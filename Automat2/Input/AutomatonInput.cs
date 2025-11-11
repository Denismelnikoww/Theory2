namespace Automat2.Input
{
    public class AutomatonInput
    {
        public List<string> States { get; set; } = new List<string>();
        public List<string> Inputs { get; set; } = new List<string>();
        public Dictionary<(string state, string input), List<string>> Transitions { get; set; } = new Dictionary<(string, string), List<string>>();
    }
}
