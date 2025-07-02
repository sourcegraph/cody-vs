using System.Threading.Tasks;

namespace Cody.Core.Ide
{
    public class Notification
    {
        private readonly TaskCompletionSource<string> _selectedValueCompletionSource = new TaskCompletionSource<string>();

        public Task<string> SelectedValueAsync => _selectedValueCompletionSource.Task;

        public Notification(uint cookie)
        {
            Cookie = cookie;
        }

        public void SetValue(string s)
        {
            _selectedValueCompletionSource.SetResult(s);
        }

        public uint Cookie { private set; get; }
    }
}
