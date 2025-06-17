namespace Cody.VisualStudio.Completions
{
    public class Difference
    {
        public string RemovedText { get; }
        public string AddedText { get; }
        public int Position { get; }

        public Difference(string removedText, string addedText, int position)
        {
            RemovedText = removedText;
            AddedText = addedText;
            Position = position;
        }

        public override string ToString()
        {
            return $"At position {Position}: Removed '{RemovedText}', Added '{AddedText}'";
        }
    }


}
