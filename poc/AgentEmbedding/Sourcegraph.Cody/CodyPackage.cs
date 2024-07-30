global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SourcegraphCody;
using StreamJsonRpc;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sourcegraph.Cody
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.CodyString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class CodyPackage : ToolkitPackage
    {
        DocumentsManager documentsManager;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();

            var runningDocumentTable = this.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
            var componentModel = this.GetService<SComponentModel, IComponentModel>();
            var editorAdapterFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            var shell = this.GetService<SVsUIShell, IVsUIShell>();

            //var manager = new DocumentsSyncManager(runningDocumentTable, editorAdapterFactoryService, shell);
            //manager.Initialize();

            documentsManager = new DocumentsManager(shell, new DocActions(), editorAdapterFactoryService);
            documentsManager.Initialize();
            

            //var componentModel = this.GetService<SComponentModel, IComponentModel>();
            //var editorAdapterFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService>();

            //runningDocumentTable.AdviseRunningDocTableEvents(new DocTableListener(editorAdapterFactoryService), out uint cookie);

            //runningDocumentTable.GetRunningDocumentsEnum(out IEnumRunningDocuments enumDocuments);
            //var array = new uint[100];
            //enumDocuments.Reset();
            //var list = new List<uint>();
            //bool more = false;
            //do
            //{
            //    more = enumDocuments.Next((uint)array.Length, array, out uint fethed) == 0;
            //    list.AddRange(array.Take((int)fethed));
            //}
            //while (more);



            //foreach (var item in list)
            //{
            //    runningDocumentTable.GetDocumentInfo(item, out uint flags, out uint readLocks, out uint editLocks, out string doc, out IVsHierarchy hier, out uint pit, out _);
            //}

            //var jsonRpc = StartAgentProcess();

            ////await jsonRpc.NotifyAsync("exit");

            //var vsVersionService = new VsVersionService();
            //var vsVersion = vsVersionService.GetDisplayVersion();
            //var editionName = vsVersionService.GetEditionName();

            //var clientInfo = new ClientInfo
            //{
            //    Name = "VisualStudio",
            //    Version = Vsix.Version,
            //    IdeVersion = vsVersion,
            //    WorkspaceRootUri = new Uri(Path.GetDirectoryName(VS.Solutions.GetCurrentSolution().FullPath)).AbsoluteUri,
            //    Capabilities = new ClientCapabilities
            //    {
            //        Edit = Capability.None,
            //        EditWorkspace = Capability.None,
            //        CodeLenses = Capability.None,
            //        ShowDocument = Capability.None,
            //        Ignore = Capability.Enabled,
            //        UntitledDocuments = Capability.Enabled,
            //    },
            //    ExtensionConfiguration = new ExtensionConfiguration
            //    {
            //        AnonymousUserID = "cdb239b6-6444-42fa-816e-0e32fdcf6d6b",
            //        ServerEndpoint = "https://sourcegraph.com/",
            //        Proxy = null,
            //        AccessToken = "<HIDDEN>",
            //        AutocompleteAdvancedProvider = null,
            //        Debug = false,
            //        VerboseDebug = false,
            //        Codebase = null,

            //    }
            //};

            //var result = await jsonRpc.InvokeAsync<ServerInfo>("initialize", clientInfo);
            //var result = await jsonRpc.InvokeAsync("graphql/getCurrentUserCodySubscription")
        }

        private JsonRpc StartAgentProcess()
        {
            var agentDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Agent");

            var process = new Process();
            process.StartInfo.FileName = Path.Combine(agentDir, "node-win-x64.exe");
            process.StartInfo.Arguments = "--inspect --enable-source-maps index.js api jsonrpc-stdio";
            process.StartInfo.WorkingDirectory = agentDir;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += Process_Exited;
            process.ErrorDataReceived += Process_ErrorDataReceived;

            var result = process.Start();


            var jsonMessageFormatter = new JsonMessageFormatter();
            jsonMessageFormatter.JsonSerializer.ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() };
            jsonMessageFormatter.JsonSerializer.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            var handler = new HeaderDelimitedMessageHandler(process.StandardInput.BaseStream, process.StandardOutput.BaseStream, jsonMessageFormatter);
            var jsonRpc = new JsonRpc(handler, new Target());
            jsonRpc.StartListening();


            //JsonRpc jsonRpc = JsonRpc.Attach(process.StandardInput.BaseStream, process.StandardOutput.BaseStream, new Target());

            //process.BeginErrorReadLine();
            //process.BeginOutputReadLine();
            return jsonRpc;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data, "Agent");
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await VS.MessageBox.ShowWarningAsync("Cody agent process Exited");
            });

            Debug.WriteLine("Process Exited", "Agent");
        }
    }

    

    public class DocumentsSyncManager
    {
        private IVsRunningDocumentTable runningDocumentTable;
        private readonly IVsEditorAdaptersFactoryService editorAdaptersFactoryService;
        private readonly IVsUIShell vsUIShell;
        private uint lastDocumentShow = 0;
        private uint pdwCookie;
        private HashSet<uint> documents = new HashSet<uint>();

        public DocumentsSyncManager(IVsRunningDocumentTable runningDocumentTable, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, IVsUIShell vsUIShell)
        {
            this.runningDocumentTable = runningDocumentTable;
            this.editorAdaptersFactoryService = editorAdaptersFactoryService;
            this.vsUIShell = vsUIShell;
        }

        private IEnumerable<IVsWindowFrame> GetOpenDocuments()
        {
            var results = new List<IVsWindowFrame>();

            vsUIShell.GetDocumentWindowEnum(out IEnumWindowFrames docEnum);
            var winFrameArray = new IVsWindowFrame[100];

            while(true)
            {
                docEnum.Next((uint)winFrameArray.Length, winFrameArray, out uint fetched);
                if (fetched == 0) break;

                results.AddRange(winFrameArray.Take((int)fetched));
            }

            return results;
        }

        private PropertyInfo rdtCookieProperty;

        private uint GetDocId(IVsWindowFrame windowFrame)
        {
            if(rdtCookieProperty == null)
            {
                rdtCookieProperty = windowFrame.GetType().GetProperty("RdtCookie");
                if (rdtCookieProperty == null)
                {
                    rdtCookieProperty = windowFrame.GetType().GetProperty("DocumentRdtCookie", BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }

            return (uint)rdtCookieProperty.GetValue(windowFrame);
        }

        public void Initialize()
        {
            var handlers = new RunningDocTableHandlers(OnAfterSave, OnShow, OnHide);
            runningDocumentTable.AdviseRunningDocTableEvents(handlers, out pdwCookie);

            runningDocumentTable.GetRunningDocumentsEnum(out IEnumRunningDocuments enumDocuments);
            var docArray = new uint[100];
            enumDocuments.Reset();
            var docList = new List<uint>();
            bool more = false;

            vsUIShell.GetDocumentWindowEnum(out IEnumWindowFrames ppenum);
            var winArray = new IVsWindowFrame[100];
            ppenum.Next((uint)winArray.Length, winArray, out uint wFetch);
            

            var zm = (uint)winArray[0].GetType().GetProperty("RdtCookie").GetValue(winArray[0]);
            //DocumentRdtCookie

            //Microsoft.VisualStudio.Platform.WindowManagement.WindowFrame

            //winArray[0].GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out object docData);
            //object objectForIUnknown = Marshal.GetObjectForIUnknown(ppunkDocData);
            //ComUtilities.IsSameComObject(docData, docData);

            do
            {
                more = enumDocuments.Next((uint)docArray.Length, docArray, out uint fetched) == 0;
                for(int i = 0; i < fetched; i++)
                {
                    if (!IsSolutionOrProjectDocument(docArray[i]))
                    {
                        documents.Add(docArray[i]);

                        runningDocumentTable.GetDocumentInfo(docArray[i], out _, out _, out _, out _, out _, out uint itemId, out IntPtr ppunkDocData);
                        object objectForIUnknown = Marshal.GetObjectForIUnknown(ppunkDocData);
                        if(objectForIUnknown is IVsTextLines)
                        {
                            var vsTextBuffer = (IVsTextBuffer)objectForIUnknown;
                            var buffer = editorAdaptersFactoryService.GetDocumentBuffer(vsTextBuffer);
                            
                            buffer.CurrentSnapshot.GetText();
                        }

                        //sent didopen

                        //Debug.WriteLine($"[[Cody]] didOpen() {GetDocumentFullPath(docArray[i])}");
                    }
                }
            }
            while (more);
        }

        private bool IsSolutionOrProjectDocument(uint docId)
        {
            runningDocumentTable.GetDocumentInfo(docId, out _, out _, out _, out _, out _, out uint itemId, out IntPtr ppunkDocData);
            
            return itemId == (uint)VSConstants.VSITEMID.Root;
        }

        private string GetDocumentFullPath(uint docId)
        {
            runningDocumentTable.GetDocumentInfo(docId, out _, out _, out _, out string documentFullPath, out _, out _, out _);
            return documentFullPath;
        }

        private void OnAfterSave(uint docId)
        {
            Debug.WriteLine($"[[Cody]] didSave() {GetDocumentFullPath(docId)}");
        }

        private IVsTextView activeTextView;
        private ITextBuffer activeTextBuffer;

        private void TrySetActiveDocument(uint docId, IVsWindowFrame pFrame)
        {

            var textView = VsShellUtilities.GetTextView(pFrame);
            if (textView != null)
            {
                if(activeTextBuffer != null)
                {
                    activeTextBuffer.ChangedLowPriority -= OnTextBufferChanged;
                    activeTextView = null;
                }

                var textBuffer = textView.ToDocumentView().TextBuffer;
                textBuffer.ChangedLowPriority += OnTextBufferChanged;
                
                activeTextBuffer = textBuffer;
                activeTextView = textView;
            }
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) => OnTextChange(activeTextView, (ITextBuffer)sender, e.Changes);

        private void OnShow(uint docId, IVsWindowFrame pFrame)
        {
            if (docId == lastDocumentShow) return;

            var docFullPath = GetDocumentFullPath(docId);
            if (documents.Contains(docId))
            {
                Debug.WriteLine($"[[Cody]] didFocus() {docFullPath}");
                //sent didFocus
            }
            else
            {
                documents.Add(docId);
                Debug.WriteLine($"[[Cody]] didOpen() {docFullPath}");
                //sent didOpen
            }

            TrySetActiveDocument(docId, pFrame);

            lastDocumentShow = docId;
        }

        private void OnHide(uint docId, IVsWindowFrame pFrame)
        {
            if (documents.Contains(docId))
            {
                documents.Remove(docId);
                Debug.WriteLine($"[[Cody]] didClose() {GetDocumentFullPath(docId)}");
                //sent didclose
            }
        }

        private void OnTextChange(IVsTextView textView, ITextBuffer textBuffer, INormalizedTextChangeCollection textChanges)
        {
            Debug.WriteLine($"[[Cody]] didChange() {textBuffer.GetFileName()}");
            //send didChange
        }
    }


    public class RunningDocTableHandlers : IVsRunningDocTableEvents2
    {
        private readonly Action<uint> onAfterSave;
        private readonly Action<uint, IVsWindowFrame> onShow;
        private readonly Action<uint, IVsWindowFrame> onHide;

        public RunningDocTableHandlers(Action<uint> onAfterSave, Action<uint, IVsWindowFrame> onShow, Action<uint, IVsWindowFrame> onHide)
        {
            if(onAfterSave == null) throw new ArgumentNullException(nameof(onAfterSave));
            if(onShow == null) throw new ArgumentNullException(nameof(onShow));
            if(onHide == null) throw new ArgumentNullException(nameof(onHide));

            this.onAfterSave = onAfterSave;
            this.onShow = onShow;
            this.onHide = onHide;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => VSConstants.S_OK;

        public int OnAfterSave(uint docCookie)
        {
            onAfterSave(docCookie);
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            onShow(docCookie, pFrame);
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            onHide(docCookie, pFrame);
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
            //throw new NotImplementedException();
        }
    }

    public class DocTableListener : IVsRunningDocTableEvents
    {
        private IVsEditorAdaptersFactoryService editorFactory;
        private IVsTextView textView;

        public DocTableListener(IVsEditorAdaptersFactoryService editorFactory)
        {
            this.editorFactory = editorFactory;
        }

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return 0;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return 0;
        }

        public int OnAfterSave(uint docCookie)
        {
            return 0;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return 0;
        }

        uint lastdocCookie = 0;

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            if (docCookie != lastdocCookie)
            {
                System.Diagnostics.Debug.WriteLine("Show " + pFrame.ToString() + " " + (fFirstShow == 1 ? "FIRST" : "NOTFIRST"), "[[Cody]]");
                lastdocCookie = docCookie;
                
                textView = VsShellUtilities.GetTextView(pFrame);
                textView.GetSelection(out int startLine, out int startCol, out int endLine, out int endCol);
                var tb = textView.ToDocumentView().TextBuffer;
                var fileName = tb.GetFileName();
                var tx = tb.CurrentSnapshot.GetText();
                tb.ChangedLowPriority += Tb_ChangedLowPriority;
                

                if(textView.GetBuffer(out IVsTextLines lines) == 0)
                {
                    var buffer = lines as IVsTextBuffer;
                    
                    var textBuffer = editorFactory.GetDataBuffer(buffer);
                    var text = textBuffer.CurrentSnapshot.GetText();
                }

            }
            return 0;
        }

        private void Tb_ChangedLowPriority(object sender, TextContentChangedEventArgs e) //sender text buffer
        {
            foreach (var change in e.Changes)
            {
                textView.GetLineAndColumn(change.NewPosition, out int line, out int column);
                System.Diagnostics.Debug.WriteLine($"change: {change.NewText} {change.NewPosition} {change.NewEnd}");
            }
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            System.Diagnostics.Debug.WriteLine("Hide " + pFrame.ToString(), "[[Cody]]");
            return 0;
        }

    }


    public class Target
    {
        [JsonRpcMethod("debug/message", UseSingleObjectParameterDeserialization = true)]
        public void Debug(DebugMessage msg)
        {
            System.Diagnostics.Debug.WriteLine(msg.Message, "Agent notify");
        }

        //[JsonRpcMethod("debug/message")]
        //public void Debug(string channel, string message)
        //{
        //    System.Diagnostics.Debug.WriteLine(message, "Agent notify");
        //}
    }

    public record DebugMessage(string Channel, string Message) { }


    public class ClientInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string? IdeVersion { get; set; }
        public string WorkspaceRootUri { get; set; }
        public ExtensionConfiguration? ExtensionConfiguration { get; set; }
        public ClientCapabilities? Capabilities { get; set; }

    }

    public class ExtensionConfiguration
    {
        public string ServerEndpoint { get; set; }
        public string? Proxy { get; set; }
        public string AccessToken { get; set; }

        public string? AnonymousUserID { get; set; }

        public string? AutocompleteAdvancedProvider { get; set; }

        public string? AutocompleteAdvancedModel { get; set; }

        public bool? Debug { get; set; }

        public bool? VerboseDebug { get; set; }

        public string? Codebase { get; set; }
    }

    public enum Capability
    {
        None,
        Enabled
    }

    public enum ChatCapability
    {
        None,
        Streaming
    }

    public enum ShowWindowMessageCapability
    {
        Notification,
        Request
    }

    public record ClientCapabilities
    {
        public string? Completions { get; set; }
        public ChatCapability? Chat { get; set; }
        public Capability? Git { get; set; }
        public Capability? ProgressBars { get; set; }
        public Capability? Edit { get; set; }
        public Capability? EditWorkspace { get; set; }
        public Capability? UntitledDocuments { get; set; }
        public Capability? ShowDocument { get; set; }
        public Capability? CodeLenses { get; set; }
        public ShowWindowMessageCapability? ShowWindowMessage { get; set; }
        public Capability? Ignore { get; set; }
        public Capability? CodeActions { get; set; }
        public string? WebviewMessages { get; set; }
    }

    public record ServerInfo(
        string Name,
        bool? Authenticated,
        bool? CodyEnabled,
        string? CodyVersion,
        AuthStatus? AuthStatus
        );

    public record AuthStatus(
    string Endpoint,
    bool IsDotCom,
    bool IsLoggedIn,
    bool ShowInvalidAccessTokenError,
    bool Authenticated,
    bool HasVerifiedEmail,
    bool RequiresVerifiedEmail,
    bool SiteHasCodyEnabled,
    string SiteVersion,
    bool UserCanUpgrade,
    string Username,
    string PrimaryEmail,
    string DisplayName,
    string AvatarURL,
    int CodyApiVersion,
    ConfigOverwrites ConfigOverwrites
);

    public record ConfigOverwrites(
        string ChatModel,
        int ChatModelMaxTokens,
        string FastChatModel,
        int FastChatModelMaxTokens,
        string CompletionModel,
        int CompletionModelMaxTokens,
        string Provider,
        bool SmartContextWindow
    );

    public record CurrentUserCodySubscription(
        string Status,
        string Plan,
        bool ApplyProRateLimits,
        DateTime CurrentPeriodStartAt,
        DateTime CurrentPeriodEndAt
    );
}