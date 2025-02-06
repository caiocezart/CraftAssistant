using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace ExileCore2TexturesHandler;
public class LoadingForm : Form
{
    private Label messageLabel;

    public LoadingForm()
    {
        this.Size = new Size(300, 100);
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.White;

        messageLabel = new Label
        {
            Text = "Extracting textures...\nPlease wait...",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill
        };

        this.Controls.Add(messageLabel);
    }

    public void UpdateMessage(string message)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => messageLabel.Text = message));
        }
        else
        {
            messageLabel.Text = message;
        }
    }
}

public partial class MainForm : Form
{
    private string contentPath = string.Empty;
    private string outputPath = string.Empty;
    private bool standaloneVersion = true;

    public MainForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Textures Extractor";
        Size = new Size(600, 200);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        // Archive Controls
        var lblArchive = new Label
        {
            Text = "Archive name:",
            Location = new Point(20, 20),
            AutoSize = true
        };

        var txtArchive = new TextBox
        {
            Location = new Point(20, 50),
            Width = 540,
            Text = "textures.zip"
        };

        // Game Folder Controls
        var lblGameFolder = new Label
        {
            Text = "Path to where your game is installed:",
            Location = new Point(20, 90),
            AutoSize = true
        };

        var txtGameFolder = new TextBox
        {
            Location = new Point(20, 120),
            Width = 430,
            ReadOnly = true
        };

        var btnBrowseGameFolder = new Button
        {
            Text = "Browse",
            Location = new Point(460, 120),
            Width = 100
        };

        // Output Folder Controls  
        var lblOutput = new Label
        {
            Text = "Output folder:",
            Location = new Point(20, 160),
            AutoSize = true
        };

        var txtOutput = new TextBox
        {
            Location = new Point(20, 190),
            Width = 430,
            ReadOnly = true
        };

        txtOutput.Text = AppDomain.CurrentDomain.BaseDirectory;
        outputPath = txtOutput.Text;

        var btnBrowseOutput = new Button
        {
            Text = "Browse",
            Location = new Point(460, 190),
            Width = 100
        };


        // Extract Button
        var btnExtract = new Button
        {
            Text = "Extract Textures",
            Location = new Point(20, 230),
            Width = 540,
            Height = 40,
            Enabled = false
        };

        // Adjust form size to show all controls
        this.Size = new Size(600, 320);

        // Events
        btnBrowseGameFolder.Click += (s, e) =>
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select Game Folder";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    contentPath = dialog.SelectedPath;
                    txtGameFolder.Text = contentPath;

                    if (File.Exists(Path.Combine(contentPath, "Content.ggpk")))
                    {
                        standaloneVersion = true;
                        btnExtract.Enabled = true;
                    }
                    else
                    {
                        standaloneVersion = false;
                        btnExtract.Enabled = false;
                        MessageBox.Show("Steam version detected (or wrong game installation path) - NOT SUPPORTED YET", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    UpdateExtractButton();
                }
            }
        };

        btnBrowseOutput.Click += (s, e) =>
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select output folder";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    outputPath = dialog.SelectedPath;
                    txtOutput.Text = outputPath;
                    UpdateExtractButton();
                }
            }
        };

        btnExtract.Click += async (s, e) =>
        {
            var loadingForm = new LoadingForm();

            try
            {
                string archiveName = txtArchive.Text.Trim();
                if (string.IsNullOrEmpty(archiveName))
                {
                    MessageBox.Show("Please enter an archive name", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var archiveZipPath = $"{outputPath}\\{archiveName}";

                btnExtract.Enabled = false;
                btnExtract.Text = "Extracting...";

                loadingForm.Show();

                await Task.Run(() =>
                {
                    loadingForm.UpdateMessage("Reading Game Content...");

                    if (standaloneVersion)
                    {
                        TexturesHandler.ExtractGGPKTexturesToArchive(
                            ggpkPath: contentPath,
                            nodePath: "Art/2DItems".ToLower().Trim(),
                            archiveZipPath: archiveZipPath
                        );
                    }
                });

                loadingForm.Close();
                MessageBox.Show("Extraction completed successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Application.Exit();

                // Open Explorer and select the file
                if (File.Exists(archiveZipPath))
                {
                    Process.Start("explorer.exe", $"/select,\"{archiveZipPath}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during extraction: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExtract.Enabled = true;
                btnExtract.Text = "Extract Textures";
            }
        };

        Controls.AddRange(new Control[]
        {
                lblArchive, txtArchive, lblGameFolder, txtGameFolder, btnBrowseGameFolder,
                lblOutput, txtOutput, btnBrowseOutput,
                btnExtract
        });

        void UpdateExtractButton()
        {
            btnExtract.Enabled = !string.IsNullOrEmpty(contentPath) &&
                               !string.IsNullOrEmpty(outputPath) && standaloneVersion;
        }
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
