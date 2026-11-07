using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace QuoteMaster.Launcher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        using var instance = new Mutex(true, "Local\\QuoteMaster.Launcher", out var created);
        if (!created)
        {
            MessageBox.Show("O QuoteMaster já está em execução.", "QuoteMaster", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        ApplicationConfiguration.Initialize();
        Application.Run(new LauncherForm(args.Contains("--demo", StringComparer.OrdinalIgnoreCase)));
    }
}

internal sealed class LauncherForm : Form
{
    private readonly Label _status = new() { AutoSize = true, Text = "Preparando o QuoteMaster…", Font = new Font("Segoe UI", 11) };
    private readonly Label _address = new() { AutoSize = true, ForeColor = Color.DarkSlateBlue };
    private readonly Button _open = new() { Text = "Abrir QuoteMaster", Enabled = false, Width = 150 };
    private readonly Button _stop = new() { Text = "Encerrar QuoteMaster", Width = 170 };
    private Process? _server;
    private string? _url;
    private string? _logPath;
    private readonly object _logLock = new();
    private StreamWriter? _log;
    private bool _closing;
    private readonly bool _demoMode;

    public LauncherForm(bool demoMode)
    {
        _demoMode = demoMode;
        Text = demoMode ? "QuoteMaster — Demonstração" : "QuoteMaster";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 185);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Controls.Add(new Label { Text = "QuoteMaster", AutoSize = true, Font = new Font("Segoe UI", 19, FontStyle.Bold), Location = new Point(24, 20) });
        _status.Location = new Point(27, 70);
        _address.Location = new Point(27, 99);
        _open.Location = new Point(27, 130);
        _stop.Location = new Point(190, 130);
        Controls.AddRange([_status, _address, _open, _stop]);
        _open.Click += (_, _) => OpenBrowser();
        _stop.Click += (_, _) => Close();
        FormClosing += (_, _) => StopServer();
        Shown += async (_, _) => await StartServerAsync();
    }

    private async Task StartServerAsync()
    {
        try
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuoteMaster", _demoMode ? "Demo" : "");
            var database = Path.Combine(appData, "database");
            var backups = Path.Combine(appData, "backups");
            var logos = Path.Combine(appData, "uploads", "logos");
            var logs = Path.Combine(appData, "logs");
            foreach (var path in new[] { database, backups, logos, logs }) Directory.CreateDirectory(path);
            _logPath = Path.Combine(logs, $"launcher-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            _log = new StreamWriter(new FileStream(_logPath, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8) { AutoFlush = true };
            var port = GetAvailablePort();
            _url = $"http://127.0.0.1:{port}";
            var serverPath = Path.Combine(AppContext.BaseDirectory, "QuoteMaster.Web.exe");
            if (!File.Exists(serverPath)) throw new FileNotFoundException("Os arquivos do aplicativo não foram encontrados. Reinstale o QuoteMaster.", serverPath);

            var start = new ProcessStartInfo(serverPath)
            {
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            start.ArgumentList.Add("--urls");
            start.ArgumentList.Add(_url);
            start.Environment["ASPNETCORE_URLS"] = _url;
            start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
            start.Environment["LocalLauncher__HttpOnly"] = "true";
            start.Environment["DemoData__Enabled"] = _demoMode ? "true" : "false";
            start.Environment["DemoData__SeedOnStartup"] = _demoMode ? "true" : "false";
            start.Environment["DemoData__ResetOnStartup"] = _demoMode ? "true" : "false";
            start.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={Path.Combine(database, "quotemaster.db")};Mode=ReadWriteCreate;Pooling=True;Default Timeout=30";
            start.Environment["Storage__BackupDirectory"] = backups;
            start.Environment["Storage__CompanyLogosDirectory"] = logos;
            _server = new Process { StartInfo = start, EnableRaisingEvents = true };
            _server.OutputDataReceived += (_, args) => WriteLog(args.Data);
            _server.ErrorDataReceived += (_, args) => WriteLog(args.Data);
            _server.Exited += (_, _) => OnServerExited();
            if (!_server.Start()) throw new InvalidOperationException("Não foi possível iniciar o servidor local.");
            _server.BeginOutputReadLine();
            _server.BeginErrorReadLine();

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var deadline = DateTime.UtcNow.AddSeconds(45);
            while (DateTime.UtcNow < deadline)
            {
                if (_server.HasExited) throw new InvalidOperationException("O servidor local encerrou durante a inicialização.");
                try
                {
                    using var response = await client.GetAsync(_url + "/health");
                    if (response.IsSuccessStatusCode)
                    {
                        _status.Text = "Aplicativo pronto. Deixe esta janela aberta enquanto usa o QuoteMaster.";
                        _address.Text = _url;
                        _open.Enabled = true;
                        OpenBrowser();
                        return;
                    }
                }
                catch (HttpRequestException) { }
                catch (TaskCanceledException) { }
                await Task.Delay(350);
            }
            throw new TimeoutException("O servidor local não ficou disponível dentro do tempo esperado.");
        }
        catch (Exception ex)
        {
            _status.Text = "Não foi possível iniciar o QuoteMaster.";
            WriteLog(ex.ToString());
            MessageBox.Show($"{ex.Message}\n\nO log está em: {_logPath ?? "%LOCALAPPDATA%\\QuoteMaster\\logs"}", "Falha ao iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            StopServer();
        }
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private void OpenBrowser()
    {
        if (_url is null) return;
        try { Process.Start(new ProcessStartInfo(_url) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show($"Abra este endereço no navegador: {_url}\n\n{ex.Message}", "QuoteMaster", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    }

    private void WriteLog(string? line)
    {
        if (line is null) return;
        lock (_logLock) { _log?.WriteLine($"[{DateTime.Now:O}] {line}"); }
    }

    private void OnServerExited()
    {
        if (_closing || IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke(() => { if (!_closing) { _status.Text = "O servidor local foi encerrado. Feche e abra o launcher novamente."; _open.Enabled = false; } }); }
        catch (InvalidOperationException) { }
    }

    private void StopServer()
    {
        _closing = true;
        try { if (_server is { HasExited: false }) _server.Kill(entireProcessTree: true); }
        catch (InvalidOperationException) { }
        catch (System.ComponentModel.Win32Exception ex) { WriteLog($"Falha ao encerrar processo: {ex}"); }
        _server?.Dispose();
        lock (_logLock) { _log?.Dispose(); _log = null; }
    }
}
