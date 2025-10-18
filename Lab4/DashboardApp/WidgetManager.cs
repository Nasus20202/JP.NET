using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Threading;
using Lab4.Contracts;

namespace Lab4.DashboardApp;

public class WidgetManager : IDisposable
{
    [ImportMany]
    public IEnumerable<IWidget>? Widgets { get; set; }

    public event EventHandler? WidgetsChanged;

    private static readonly string WidgetsPath = "Widgets";
    private static readonly string ShadowCopyPath = Path.Combine(
        Path.GetTempPath(),
        "WidgetsShadowCopy"
    );
    private FileSystemWatcher? _fileSystemWatcher;
    private CompositionHost? _compositionHost;
    private readonly IEventAggregator _eventAggregator;
    private readonly Dictionary<string, AssemblyLoadContext> _loadContexts = new();
    private readonly Dispatcher _dispatcher;

    public WidgetManager(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _dispatcher = Dispatcher.CurrentDispatcher;
        InitializeFileSystemWatcher();
    }

    private void InitializeFileSystemWatcher()
    {
        string widgetsFullPath = Path.GetFullPath(WidgetsPath);

        if (!Directory.Exists(widgetsFullPath))
        {
            Directory.CreateDirectory(widgetsFullPath);
        }

        _fileSystemWatcher = new FileSystemWatcher(widgetsFullPath)
        {
            Filter = "*.dll",
            NotifyFilter =
                NotifyFilters.Attributes
                | NotifyFilters.CreationTime
                | NotifyFilters.DirectoryName
                | NotifyFilters.FileName
                | NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.Security
                | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };

        _fileSystemWatcher.Created += OnWidgetFileChanged;
        _fileSystemWatcher.Deleted += OnWidgetFileChanged;
        _fileSystemWatcher.Changed += OnWidgetFileChanged;
        _fileSystemWatcher.Renamed += OnWidgetFileChanged;
    }

    private void OnWidgetFileChanged(object sender, FileSystemEventArgs e)
    {
        // Run LoadWidgets on a background thread to avoid blocking the FileSystemWatcher thread
        Task.Run(() => LoadWidgets());
    }

    /// <summary>
    /// Loads the widget assemblies from the specified directory. The DLL files are copied to a shadow copy directory to allow for unloading and reloading.
    /// </summary>
    public void LoadWidgets()
    {
        try
        {
            _compositionHost?.Dispose();

            // Unload previous contexts
            foreach (var context in _loadContexts.Values)
            {
                context.Unload();
            }
            _loadContexts.Clear();

            // Force garbage collection to release assemblies
            GC.Collect();
            GC.WaitForPendingFinalizers();

            CleanupShadowCopyDirectory();

            string widgetsFullPath = Path.GetFullPath(WidgetsPath);

            if (!Directory.Exists(widgetsFullPath))
            {
                // Marshal to UI thread for property assignment and event
                _dispatcher.Invoke(() =>
                {
                    Widgets = Array.Empty<IWidget>();
                    WidgetsChanged?.Invoke(this, EventArgs.Empty);
                });
                return;
            }

            var assemblies = new List<Assembly>();
            var dllFiles = Directory.GetFiles(widgetsFullPath, "*.dll");

            string shadowCopyDir = Path.Combine(ShadowCopyPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(shadowCopyDir);

            foreach (var dllFile in dllFiles)
            {
                try
                {
                    string fileName = Path.GetFileName(dllFile);
                    string shadowCopyFile = Path.Combine(shadowCopyDir, fileName);
                    File.Copy(dllFile, shadowCopyFile, true);

                    string depsFile = Path.ChangeExtension(dllFile, ".deps.json");
                    if (File.Exists(depsFile))
                    {
                        string shadowCopyDeps = Path.Combine(
                            shadowCopyDir,
                            Path.GetFileName(depsFile)
                        );
                        File.Copy(depsFile, shadowCopyDeps, true);
                    }

                    var context = new AssemblyLoadContext(fileName, true);
                    var assembly = context.LoadFromAssemblyPath(shadowCopyFile);
                    assemblies.Add(assembly);
                    _loadContexts[dllFile] = context;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to load assembly {dllFile}: {ex.Message}"
                    );
                }
            }

            // Create configuration (doesn't need UI thread)
            var configuration = new ContainerConfiguration().WithAssemblies(assemblies);
            configuration.WithExport<IEventAggregator>(_eventAggregator);

            _compositionHost = configuration.CreateContainer();

            _dispatcher.Invoke(() =>
            {
                try
                {
                    // This creates widget instances, which create UserControls in their constructors
                    // UserControls MUST be created on the STA UI thread
                    Widgets = _compositionHost.GetExports<IWidget>();

                    // Fire event on UI thread
                    WidgetsChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    string error = $"Error instantiating widgets: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine(error);
                    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Widgets = Array.Empty<IWidget>();
                    WidgetsChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }
        catch (Exception ex)
        {
            string error = $"Error loading widgets: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(error);

            _dispatcher.Invoke(() =>
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Widgets = Array.Empty<IWidget>();
                WidgetsChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    private void CleanupShadowCopyDirectory()
    {
        try
        {
            if (Directory.Exists(ShadowCopyPath))
            {
                foreach (var dir in Directory.GetDirectories(ShadowCopyPath))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch { }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Error cleaning shadow copy directory: {ex.Message}"
            );
        }
    }

    public void Dispose()
    {
        _fileSystemWatcher?.Dispose();
        _compositionHost?.Dispose();

        foreach (var context in _loadContexts.Values)
        {
            context.Unload();
        }
        _loadContexts.Clear();

        // Force garbage collection to release assemblies
        GC.Collect();
        GC.WaitForPendingFinalizers();

        CleanupShadowCopyDirectory();
    }
}
