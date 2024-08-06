namespace Cody.VisualStudio.Services
{
    public class WebviewsManager
    {
        private Dictionary<string, WebviewController> _controllers = new Dictionary<string, WebviewController>();

        public WebviewController GetController(string id)
        {
            return _controllers.TryGetValue(id, out var controller) ? controller : null;
        }

        public WebviewController AddController(string id)
        {
            var controller = new WebviewController(id);
            _controllers[id] = controller;
            return controller;
        }

        public bool RemoveController(string id)
        {
            return _controllers.Remove(id);
        }
    }
}