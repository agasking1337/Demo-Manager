using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using MaterialSkin;
using MaterialSkin.Controls;
using IOZipArchive = System.IO.Compression.ZipArchive;
using IOCompressionLevel = System.IO.Compression.CompressionLevel;

namespace DemoManager
{
    public partial class MainForm : MaterialForm
    {
        private string csgoPath = string.Empty;
        private string demoPath = string.Empty;
        
        // Initialize all UI controls in the field declarations
        private MaterialListView demoList = new();
        private MaterialLabel statusLabel = new();
        private MaterialButton refreshButton = new();
        private MaterialButton renameButton = new();
        private MaterialButton deleteButton = new();
        private MaterialButton selectFolderButton = new();
        private MaterialButton compressButton = new();
        private MaterialButton shareButton = new();
        private TableLayoutPanel mainLayout = new();
        private FlowLayoutPanel buttonPanel = new();
        private MaterialLabel pathLabel = new();
        private MaterialSwitch themeSwitch = new();
        private readonly MaterialSkinManager materialSkinManager;

        public MainForm()
        {
            InitializeComponent();

            // Initialize MaterialSkinManager
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.BlueGrey800,
                Primary.BlueGrey900,
                Primary.BlueGrey500,
                Accent.LightBlue200,
                TextShade.WHITE
            );

            SetupUI();
            FindCS2Path();
            LoadDemos();
        }

        private void SetupUI()
        {
            this.Text = "CS2 Demo Manager";
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(600, 400);
            this.Padding = new Padding(3);
            this.FormStyle = FormStyles.ActionBar_48;
            this.Sizable = true;

            // Create main layout
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 1;
            mainLayout.RowCount = 4;
            mainLayout.Padding = new Padding(6);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Top bar
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Demo list
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Buttons
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));  // Status bar

            // Top panel with path and controls
            var topPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Height = 40,
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 60), // Path label
                    new ColumnStyle(SizeType.AutoSize),    // Select folder button
                    new ColumnStyle(SizeType.AutoSize)     // Theme switch
                }
            };

            pathLabel.Dock = DockStyle.Fill;
            pathLabel.AutoEllipsis = true;
            pathLabel.Margin = new Padding(0, 0, 10, 0);

            selectFolderButton.Text = "📁 Select Folder";
            selectFolderButton.Type = MaterialButton.MaterialButtonType.Contained;
            selectFolderButton.UseAccentColor = true;
            selectFolderButton.AutoSize = true;
            selectFolderButton.Margin = new Padding(0, 0, 10, 0);
            selectFolderButton.Click += SelectFolderButton_Click;

            themeSwitch.Text = "🌙 Dark Mode";
            themeSwitch.Checked = true;
            themeSwitch.AutoSize = true;
            themeSwitch.Margin = new Padding(0);
            themeSwitch.CheckedChanged += ThemeSwitch_CheckedChanged;

            topPanel.Controls.Add(pathLabel, 0, 0);
            topPanel.Controls.Add(selectFolderButton, 1, 0);
            topPanel.Controls.Add(themeSwitch, 2, 0);

            // Demo list
            demoList.Dock = DockStyle.Fill;
            demoList.FullRowSelect = true;
            demoList.MultiSelect = false;
            demoList.AllowDrop = true;
            demoList.View = View.Details;
            demoList.Columns.Add(new ColumnHeader { Text = "Type", Width = 50 });
            demoList.Columns.Add(new ColumnHeader { Text = "Name", Width = -2 });
            demoList.DragEnter += DemoList_DragEnter;
            demoList.DragDrop += DemoList_DragDrop;
            demoList.SelectedIndexChanged += DemoList_SelectedIndexChanged;

            // Button panel
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.LeftToRight;
            buttonPanel.WrapContents = false;
            buttonPanel.AutoSize = true;
            buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            buttonPanel.Anchor = AnchorStyles.None;

            // Buttons
            refreshButton.Text = "🔄 Refresh";
            refreshButton.Type = MaterialButton.MaterialButtonType.Contained;
            refreshButton.UseAccentColor = false;
            refreshButton.AutoSize = true;
            refreshButton.Margin = new Padding(0, 0, 10, 0);
            refreshButton.Click += (s, e) => LoadDemos();

            renameButton.Text = "✏️ Rename";
            renameButton.Type = MaterialButton.MaterialButtonType.Contained;
            renameButton.UseAccentColor = false;
            renameButton.AutoSize = true;
            renameButton.Enabled = false;
            renameButton.Margin = new Padding(0, 0, 10, 0);
            renameButton.Click += RenameButton_Click;

            compressButton.Text = "📦 Compress";
            compressButton.Type = MaterialButton.MaterialButtonType.Contained;
            compressButton.UseAccentColor = false;
            compressButton.AutoSize = true;
            compressButton.Enabled = false;
            compressButton.Margin = new Padding(0, 0, 10, 0);
            compressButton.Click += CompressButton_Click;

            shareButton.Text = "📋 Copy Demo";
            shareButton.Type = MaterialButton.MaterialButtonType.Contained;
            shareButton.UseAccentColor = false;
            shareButton.AutoSize = true;
            shareButton.Enabled = false;
            shareButton.Margin = new Padding(0, 0, 10, 0);
            shareButton.Click += ShareButton_Click;

            deleteButton.Text = "🗑️ Delete";
            deleteButton.Type = MaterialButton.MaterialButtonType.Contained;
            deleteButton.UseAccentColor = true;
            deleteButton.AutoSize = true;
            deleteButton.Enabled = false;
            deleteButton.Margin = new Padding(0);
            deleteButton.Click += DeleteButton_Click;

            // Status label
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.AutoSize = true;

            // Add buttons to panel
            buttonPanel.Controls.AddRange(new Control[] { 
                refreshButton, 
                renameButton, 
                compressButton,
                shareButton,
                deleteButton 
            });

            // Add all controls to main layout
            mainLayout.Controls.Add(topPanel, 0, 0);
            mainLayout.Controls.Add(demoList, 0, 1);
            mainLayout.Controls.Add(buttonPanel, 0, 2);
            mainLayout.Controls.Add(statusLabel, 0, 3);

            // Add main layout to form
            this.Controls.Add(mainLayout);
        }

        private void ThemeSwitch_CheckedChanged(object sender, EventArgs e)
        {
            materialSkinManager.Theme = themeSwitch.Checked ? 
                MaterialSkinManager.Themes.DARK : 
                MaterialSkinManager.Themes.LIGHT;
        }

        private void DemoList_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = demoList.SelectedItems.Count > 0;
            bool isDemoSelected = hasSelection && demoList.SelectedItems[0].Tag?.ToString() == ".dem";
            
            renameButton.Enabled = hasSelection;
            deleteButton.Enabled = hasSelection;
            compressButton.Enabled = isDemoSelected; // Only enable compress for .dem files
            shareButton.Enabled = hasSelection;
        }

        private void LoadDemos()
        {
            if (string.IsNullOrEmpty(demoPath) || !Directory.Exists(demoPath))
                return;

            demoList.Items.Clear();

            // Get both .dem and .zip files
            var files = Directory.GetFiles(demoPath)
                               .Where(f => f.EndsWith(".dem", StringComparison.OrdinalIgnoreCase) || 
                                         f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                               .OrderBy(f => Path.GetFileName(f));
            
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var extension = Path.GetExtension(file).ToLower();
                var icon = extension == ".dem" ? "📼" : "📦";
                
                var item = new ListViewItem(new[] { icon, fileName });
                item.Tag = extension; // Store the extension for later use
                demoList.Items.Add(item);
            }

            statusLabel.Text = $"Found {files.Count()} files";
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = demoList.SelectedItems.Count > 0;
            bool isDemoSelected = hasSelection && demoList.SelectedItems[0].Tag?.ToString() == ".dem";
            
            renameButton.Enabled = hasSelection;
            deleteButton.Enabled = hasSelection;
            compressButton.Enabled = isDemoSelected; // Only enable compress for .dem files
            shareButton.Enabled = hasSelection;
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select CS2 Demo Folder",
                UseDescriptionForTitle = true
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                demoPath = folderDialog.SelectedPath;
                pathLabel.Text = $"Demo Path: {demoPath}";
                LoadDemos();
            }
        }

        private void RenameButton_Click(object? sender, EventArgs e)
        {
            if (demoList.SelectedItems.Count == 0) return;

            var selectedItem = demoList.SelectedItems[0];
            string oldName = selectedItem.SubItems[1].Text; // Get name from second column
            string extension = selectedItem.Tag?.ToString() ?? ".dem";
            string oldPath = Path.Combine(demoPath, oldName);
            string fileType = extension == ".dem" ? "demo" : "compressed file";

            using var inputDialog = new MaterialForm
            {
                Text = $"Rename {fileType}",
                Width = 400,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent
            };

            var label = new MaterialLabel
            {
                Text = $"Enter new name for {oldName}:",
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            var textBox = new MaterialTextBox2
            {
                Text = Path.GetFileNameWithoutExtension(oldName),
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(10)
            };

            var okButton = new MaterialButton
            {
                Text = "Rename",
                DialogResult = DialogResult.OK,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            var cancelButton = new MaterialButton
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Type = MaterialButton.MaterialButtonType.Outlined
            };

            buttonPanel.Controls.AddRange(new Control[] { cancelButton, okButton });
            inputDialog.Controls.AddRange(new Control[] { buttonPanel, textBox, label });
            inputDialog.AcceptButton = okButton;
            inputDialog.CancelButton = cancelButton;

            if (inputDialog.ShowDialog(this) == DialogResult.OK)
            {
                string newName = textBox.Text.Trim();
                if (string.IsNullOrEmpty(newName))
                {
                    MaterialMessageBox.Show("Please enter a valid name", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Ensure the extension is preserved
                if (!newName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    newName += extension;
                }

                string newPath = Path.Combine(demoPath, newName);

                try
                {
                    if (File.Exists(newPath))
                    {
                        MaterialMessageBox.Show($"A {fileType} with this name already exists", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    File.Move(oldPath, newPath);
                    LoadDemos();
                    statusLabel.Text = $"{fileType} renamed successfully";
                }
                catch (Exception ex)
                {
                    MaterialMessageBox.Show($"Error renaming {fileType}: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = $"Error renaming {fileType}";
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (demoList.SelectedItems.Count == 0) return;

            string fileName = demoList.SelectedItems[0].SubItems[1].Text; // Get name from second column
            string filePath = Path.Combine(demoPath, fileName);
            string fileType = demoList.SelectedItems[0].Tag?.ToString() == ".dem" ? "demo" : "compressed file";

            var result = MessageBox.Show(
                $"Are you sure you want to delete this {fileType}?\n{fileName}",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    File.Delete(filePath);
                    LoadDemos();
                    statusLabel.Text = $"{fileType} deleted successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting {fileType}: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = $"Error deleting {fileType}";
                }
            }
        }

        private void DemoList_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Any(f => f.EndsWith(".dem", StringComparison.OrdinalIgnoreCase)))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private async void DemoList_DragDrop(object sender, DragEventArgs e)
        {
            if (string.IsNullOrEmpty(demoPath) || e.Data?.GetDataPresent(DataFormats.FileDrop) != true)
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                await ProcessDroppedFile(file);
            }

            LoadDemos();
        }

        private async Task ProcessDroppedFile(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLower();
                statusLabel.Text = $"Processing {Path.GetFileName(filePath)}...";

                switch (extension)
                {
                    case ".dem":
                        File.Copy(filePath, Path.Combine(demoPath, Path.GetFileName(filePath)), true);
                        break;

                    case ".zip":
                    case ".7z":
                        await Task.Run(() => ExtractArchive(filePath));
                        break;

                    default:
                        MessageBox.Show($"Unsupported file type: {extension}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                statusLabel.Text = "File processed successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error processing file";
            }
        }

        private void ExtractArchive(string archivePath)
        {
            using (var archive = ArchiveFactory.Open(archivePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory && entry.Key.EndsWith(".dem", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.WriteToDirectory(demoPath, new ExtractionOptions
                        {
                            ExtractFullPath = false,
                            Overwrite = true
                        });
                    }
                }
            }
        }

        private async void CompressButton_Click(object sender, EventArgs e)
        {
            if (demoList.SelectedItems.Count == 0 || 
                demoList.SelectedItems[0].Tag?.ToString() != ".dem") return;

            string demoName = demoList.SelectedItems[0].SubItems[1].Text;
            string demoFilePath = Path.Combine(this.demoPath, demoName);
            string zipName = Path.ChangeExtension(demoName, ".zip");
            string zipPath = Path.Combine(this.demoPath, zipName);

            try
            {
                statusLabel.Text = "Compressing demo with maximum compression...";
                compressButton.Enabled = false;
                long originalSize = new FileInfo(demoFilePath).Length;

                // Create a progress form
                using var progressForm = new MaterialForm
                {
                    Text = "Compressing Demo",
                    Width = 400,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false
                };

                var progressLabel = new MaterialLabel
                {
                    Text = "Compressing with maximum compression...",
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Padding = new Padding(10)
                };

                var progressBar = new MaterialProgressBar
                {
                    Dock = DockStyle.Top,
                    Style = ProgressBarStyle.Marquee,
                    MarqueeAnimationSpeed = 30,
                    Height = 5
                };

                progressForm.Controls.AddRange(new Control[] { progressBar, progressLabel });
                
                // Show progress form without blocking
                progressForm.Show(this);

                await Task.Run(() =>
                {
                    using var fileStream = new FileStream(zipPath, FileMode.Create);
                    using var zipArchive = new IOZipArchive(fileStream, System.IO.Compression.ZipArchiveMode.Create);
                    var entry = zipArchive.CreateEntry(demoName, IOCompressionLevel.SmallestSize);
                    
                    using var entryStream = entry.Open();
                    using var sourceStream = File.OpenRead(demoFilePath);
                    sourceStream.CopyTo(entryStream);
                });

                progressForm.Close();

                // Calculate compression ratio
                long compressedSize = new FileInfo(zipPath).Length;
                double compressionRatio = 100.0 * (1 - (double)compressedSize / originalSize);
                string compressionInfo = $"Original: {FormatFileSize(originalSize)}\n" +
                                       $"Compressed: {FormatFileSize(compressedSize)}\n" +
                                       $"Compression Ratio: {compressionRatio:F1}%";

                statusLabel.Text = "Demo compressed successfully";
                
                // Show compression results
                MaterialMessageBox.Show(
                    $"Demo compressed successfully!\n\n{compressionInfo}",
                    "Compression Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Refresh the demo list to show the new zip file
                LoadDemos();
            }
            catch (Exception ex)
            {
                MaterialMessageBox.Show($"Error compressing demo: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error compressing demo";
            }
            finally
            {
                compressButton.Enabled = true;
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:F2} {sizes[order]}";
        }

        private void ShareButton_Click(object sender, EventArgs e)
        {
            if (demoList.SelectedItems.Count == 0) return;

            try
            {
                string fileName = demoList.SelectedItems[0].SubItems[1].Text; // Get name from second column
                string fullPath = Path.Combine(demoPath, fileName);

                // Create a string array of file paths
                string[] files = new string[] { fullPath };

                // Create a StringCollection and add the files
                var filePaths = new System.Collections.Specialized.StringCollection();
                filePaths.AddRange(files);

                // Copy the files to the clipboard
                Clipboard.Clear();
                Clipboard.SetFileDropList(filePaths);
                
                statusLabel.Text = "File copied to clipboard";

                // Show a brief material snackbar
                MaterialSnackBar snackBar = new MaterialSnackBar("File copied to clipboard!", "OK", true);
                snackBar.Show(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error copying file";
            }
        }

        private void FindCS2Path()
        {
            string steamPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam", "steamapps", "common", "Counter-Strike Global Offensive", "game", "csgo");

            if (Directory.Exists(steamPath))
            {
                csgoPath = steamPath;
                demoPath = Path.Combine(csgoPath);
                pathLabel.Text = $"Demo Path: {demoPath}";
                statusLabel.Text = "CS2 installation found";
            }
            else
            {
                statusLabel.Text = "CS2 installation not found. Please select demo folder manually.";
                pathLabel.Text = "Demo Path: Not found";
            }
        }
    }
}
