using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LayoutConverter.Conversion.Options;
using LayoutConverter.Conversion.Pipeline;
using LayoutConverter.Core.Brlyt;

namespace LayoutConverter.Gui;

public sealed class MainForm : Form
{
    private readonly ListBox _inputList = new();
    private readonly TextBox _outputPathTextBox = new();
    private readonly TextBox _logTextBox = new();
    private readonly Button _runButton = new();
    private readonly Button _openOutputButton = new();
    private readonly CheckBox _bannerCheckBox = new();
    private readonly CheckBox _splitTagsCheckBox = new();
    private readonly CheckBox _omitSameKeyCheckBox = new();
    private readonly CheckBox _omitSameKeyAllCheckBox = new();
    private readonly CheckBox _bakeInfinityCheckBox = new();
    private readonly CheckBox _exportReferencedTexturesOnlyCheckBox = new();
    private readonly CheckBox _skipVersionCheckBox = new();
    private readonly CheckBox _xsdValidateCheckBox = new();
    private readonly CheckBox _suppressCvtrCharCheckBox = new();

    public MainForm()
    {
        Text = "LayoutConverter";
        MinimumSize = new Size(840, 620);
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(BuildLayout());
    }

    private Control BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12),
        };

        root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));

        root.Controls.Add(BuildInputGroup(), 0, 0);
        root.Controls.Add(BuildOutputGroup(), 0, 1);
        root.Controls.Add(BuildOptionsGroup(), 0, 2);
        root.Controls.Add(BuildLogGroup(), 0, 3);

        return root;
    }

    private Control BuildInputGroup()
    {
        var group = new GroupBox
        {
            Text = "Inputs",
            Dock = DockStyle.Fill,
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(8),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));

        _inputList.Dock = DockStyle.Fill;
        _inputList.HorizontalScrollbar = true;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
        };

        buttons.Controls.Add(CreateButton("Add Files", AddFiles));
        buttons.Controls.Add(CreateButton("Add Folder", AddFolder));
        buttons.Controls.Add(CreateButton("Remove", RemoveSelectedInput));
        buttons.Controls.Add(CreateButton("Clear", ClearInputs));

        layout.Controls.Add(_inputList, 0, 0);
        layout.Controls.Add(buttons, 1, 0);
        group.Controls.Add(layout);
        return group;
    }

    private Control BuildOutputGroup()
    {
        var group = new GroupBox
        {
            Text = "Output",
            Dock = DockStyle.Fill,
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(8),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

        _outputPathTextBox.Dock = DockStyle.Fill;

        _openOutputButton.Text = "Open";
        _openOutputButton.Dock = DockStyle.Fill;
        _openOutputButton.Click += (_, _) => OpenOutputDirectory();

        layout.Controls.Add(_outputPathTextBox, 0, 0);
        layout.Controls.Add(CreateButton("Browse", BrowseOutput), 1, 0);
        layout.Controls.Add(_openOutputButton, 2, 0);
        group.Controls.Add(layout);
        return group;
    }

    private Control BuildOptionsGroup()
    {
        var group = new GroupBox
        {
            Text = "Options",
            Dock = DockStyle.Fill,
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            Padding = new Padding(8),
        };

        for (int i = 0; i < 3; i++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        }

        AddOption(layout, _bannerCheckBox, "Banner profile", 0, 0);
        AddOption(layout, _splitTagsCheckBox, "Split by tags (-g)", 1, 0);
        AddOption(layout, _exportReferencedTexturesOnlyCheckBox, "Referenced textures only", 2, 0);
        AddOption(layout, _omitSameKeyCheckBox, "Omit same key", 0, 1);
        AddOption(layout, _omitSameKeyAllCheckBox, "Omit same key all", 1, 1);
        AddOption(layout, _bakeInfinityCheckBox, "Bake infinity", 2, 1);
        AddOption(layout, _skipVersionCheckBox, "Skip version check", 0, 2);
        AddOption(layout, _xsdValidateCheckBox, "XSD validate", 1, 2);
        AddOption(layout, _suppressCvtrCharCheckBox, "No cvtrchar conversion", 2, 2);

        _runButton.Text = "Convert";
        _runButton.Dock = DockStyle.Fill;
        _runButton.Click += async (_, _) => await RunConversionAsync();
        layout.Controls.Add(_runButton, 2, 3);

        group.Controls.Add(layout);
        return group;
    }

    private Control BuildLogGroup()
    {
        var group = new GroupBox
        {
            Text = "Log",
            Dock = DockStyle.Fill,
        };

        _logTextBox.Dock = DockStyle.Fill;
        _logTextBox.Multiline = true;
        _logTextBox.ReadOnly = true;
        _logTextBox.ScrollBars = ScrollBars.Both;
        _logTextBox.WordWrap = false;
        _logTextBox.Font = new Font(FontFamily.GenericMonospace, 9f);

        group.Controls.Add(_logTextBox);
        return group;
    }

    private static Button CreateButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Width = 112,
            Height = 30,
            Margin = new Padding(0, 0, 0, 8),
        };

        button.Click += (_, _) => action();
        return button;
    }

    private static void AddOption(TableLayoutPanel layout, CheckBox checkBox, string text, int column, int row)
    {
        checkBox.Text = text;
        checkBox.Dock = DockStyle.Fill;
        checkBox.AutoSize = true;
        layout.Controls.Add(checkBox, column, row);
    }

    private void AddFiles()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select layout or animation files",
            Filter = "Layout files|*.rlyt;*.rlan;*.rlpa;*.rlvi;*.rlvc;*.rlmc;*.rlts;*.rltp|All files|*.*",
            Multiselect = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            AddInputs(dialog.FileNames);
            SetDefaultOutputPath(dialog.FileNames[0]);
        }
    }

    private void AddFolder()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select a folder containing layout or animation files",
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            AddInputs([dialog.SelectedPath]);
            SetDefaultOutputPath(dialog.SelectedPath);
        }
    }

    private void AddInputs(IEnumerable<string> paths)
    {
        var existing = _inputList.Items.Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths.Select(Path.GetFullPath))
        {
            if (existing.Add(path))
            {
                _inputList.Items.Add(path);
            }
        }
    }

    private void SetDefaultOutputPath(string sourcePath)
    {
        if (!string.IsNullOrWhiteSpace(_outputPathTextBox.Text))
        {
            return;
        }

        var directory = Directory.Exists(sourcePath)
            ? sourcePath
            : Path.GetDirectoryName(Path.GetFullPath(sourcePath)) ?? Environment.CurrentDirectory;
        _outputPathTextBox.Text = Path.Combine(directory, "converted");
    }

    private void BrowseOutput()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select output folder",
            UseDescriptionForTitle = true,
        };

        if (!string.IsNullOrWhiteSpace(_outputPathTextBox.Text))
        {
            dialog.SelectedPath = _outputPathTextBox.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _outputPathTextBox.Text = dialog.SelectedPath;
        }
    }

    private void RemoveSelectedInput()
    {
        var selected = _inputList.SelectedItems.Cast<object>().ToArray();
        foreach (var item in selected)
        {
            _inputList.Items.Remove(item);
        }
    }

    private void ClearInputs()
    {
        _inputList.Items.Clear();
    }

    private async Task RunConversionAsync()
    {
        if (_inputList.Items.Count == 0)
        {
            MessageBox.Show(this, "Add at least one input file or folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(_outputPathTextBox.Text))
        {
            MessageBox.Show(this, "Select an output folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SetBusy(true);
        _logTextBox.Clear();

        try
        {
            var options = BuildOptions();
            using var writer = new StringWriter();
            var exitCode = await Task.Run(() => new ConversionPipeline().Run(options, writer));
            _logTextBox.Text = writer.ToString();
            AppendLog($"Exit code: {exitCode}");

            if (exitCode == ConversionExitCode.Success)
            {
                AppendLog("Done.");
            }
        }
        catch (Exception ex)
        {
            _logTextBox.Text = ex.ToString();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private ConverterOptions BuildOptions()
    {
        bool banner = _bannerCheckBox.Checked;
        return new ConverterOptions
        {
            InputPaths = _inputList.Items.Cast<string>().ToArray(),
            OutputPath = _outputPathTextBox.Text,
            ShowHelp = false,
            Execution = new ExecutionOptions(),
            XmlLoad = new XmlLoadOptions
            {
                SkipVersionCheck = _skipVersionCheckBox.Checked,
                EnableXsdValidation = _xsdValidateCheckBox.Checked,
                SuppressCvtrCharConversion = _suppressCvtrCharCheckBox.Checked,
            },
            Layout = new LayoutRouteOptions
            {
                Flavor = banner ? LayoutFlavor.Banner : LayoutFlavor.Default,
                ExportReferencedTexturesOnly = _exportReferencedTexturesOnlyCheckBox.Checked,
            },
            Animation = new AnimationRouteOptions
            {
                SplitOutputsByTag = _splitTagsCheckBox.Checked,
                IncludeTagInfo = true,
                OmitSameKeyAfterFirstTag = _omitSameKeyCheckBox.Checked,
                OmitSameKeyForAllTags = _omitSameKeyAllCheckBox.Checked,
                BakeInfinityAreaKey = _bakeInfinityCheckBox.Checked,
                ExportReferencedTexturesOnly = _exportReferencedTexturesOnlyCheckBox.Checked,
                UseBannerVersion = banner,
            },
        };
    }

    private void SetBusy(bool busy)
    {
        _runButton.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private void AppendLog(string text)
    {
        if (_logTextBox.TextLength > 0)
        {
            _logTextBox.AppendText(Environment.NewLine);
        }

        _logTextBox.AppendText(text);
    }

    private void OpenOutputDirectory()
    {
        if (string.IsNullOrWhiteSpace(_outputPathTextBox.Text))
        {
            return;
        }

        Directory.CreateDirectory(_outputPathTextBox.Text);
        using var _ = Process.Start(new ProcessStartInfo
        {
            FileName = _outputPathTextBox.Text,
            UseShellExecute = true,
        });
    }
}
