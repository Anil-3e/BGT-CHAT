using System.Drawing;

namespace BGTChatWinForms;

public sealed class SetupForm : Form
{
    private readonly TextBox txtSupabaseUrl = new();
    private readonly TextBox txtSupabaseKey = new();
    private readonly Button btnSave = new();

    public SetupForm()
    {
        Text = "BGT Chat - Supabase Setup";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(620, 465);
        MinimumSize = new Size(580, 445);
        BackColor = Color.FromArgb(238, 243, 248);
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        BuildInterface();
        LoadCurrentSettings();
    }

    private void BuildInterface()
    {
        var titleLabel = new Label
        {
            AutoSize = true,
            Text = "Connect BGT Chat to Supabase",
            ForeColor = Color.FromArgb(21, 37, 61),
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            Location = new Point(36, 30)
        };

        var helpLabel = new Label
        {
            AutoSize = false,
            Text = "Enter the Project URL and anon/public key from Supabase. " +
                   "Do not use the private service_role key.",
            ForeColor = Color.FromArgb(102, 117, 138),
            Location = new Point(40, 78),
            Size = new Size(535, 48)
        };

        var urlLabel = CreateLabel("Supabase Project URL", 142);
        txtSupabaseUrl.Location = new Point(40, 168);
        txtSupabaseUrl.Size = new Size(535, 31);
        txtSupabaseUrl.PlaceholderText = "https://your-project.supabase.co";

        var keyLabel = CreateLabel("Supabase anon/public key", 220);
        txtSupabaseKey.Location = new Point(40, 246);
        txtSupabaseKey.Size = new Size(535, 92);
        txtSupabaseKey.Multiline = true;
        txtSupabaseKey.ScrollBars = ScrollBars.Vertical;
        txtSupabaseKey.PlaceholderText = "Paste the anon/public key here";

        btnSave.Text = "Save and Continue";
        btnSave.Location = new Point(375, 370);
        btnSave.Size = new Size(200, 45);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Color.FromArgb(21, 94, 239);
        btnSave.ForeColor = Color.White;
        btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSave.Click += BtnSave_Click;

        Controls.AddRange(new Control[]
        {
            titleLabel,
            helpLabel,
            urlLabel,
            txtSupabaseUrl,
            keyLabel,
            txtSupabaseKey,
            btnSave
        });

        AcceptButton = btnSave;
    }

    private static Label CreateLabel(string text, int top)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            ForeColor = Color.FromArgb(21, 37, 61),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(40, top)
        };
    }

    private void LoadCurrentSettings()
    {
        AppSettings settings = AppSettings.Load();
        txtSupabaseUrl.Text = settings.SupabaseUrl;
        txtSupabaseKey.Text = settings.SupabaseKey;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        var settings = new AppSettings
        {
            SupabaseUrl = txtSupabaseUrl.Text.Trim().TrimEnd('/'),
            SupabaseKey = txtSupabaseKey.Text.Trim()
        };

        if (!settings.IsConfigured)
        {
            MessageBox.Show(
                "Enter a valid Supabase Project URL and the complete anon/public key.",
                "Invalid Supabase settings",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        settings.Save();
        DialogResult = DialogResult.OK;
        Close();
    }
}
