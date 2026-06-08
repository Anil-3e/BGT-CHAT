using System.Drawing;
using System.Diagnostics;

namespace BGTChatWinForms;

public sealed class LoginForm : Form
{
    // These names match the component names required by the project brief.
    private readonly TextBox txtUsername = new();
    private readonly TextBox txtRoomCode = new();
    private readonly Button btnJoin = new();
    private readonly Button btnOpenWeb = new();

    public LoginForm()
    {
        InitializeForm();
        BuildInterface();
    }

    private void InitializeForm()
    {
        Text = "BGT Chat - Join Room";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 470);
        MinimumSize = new Size(480, 440);
        BackColor = Color.FromArgb(238, 243, 248);
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
    }

    private void BuildInterface()
    {
        var card = new Panel
        {
            BackColor = Color.White,
            Location = new Point(55, 38),
            Size = new Size(410, 385),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var schoolLabel = new Label
        {
            AutoSize = true,
            Text = "BRITISH GYMNASIUM OF TECHNOLOGY",
            ForeColor = Color.FromArgb(21, 94, 239),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Location = new Point(36, 30)
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = "BGT Chat",
            ForeColor = Color.FromArgb(21, 37, 61),
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            Location = new Point(31, 52)
        };

        var helpLabel = new Label
        {
            AutoSize = false,
            Text = "Enter your display name and classroom room code.",
            ForeColor = Color.FromArgb(102, 117, 138),
            Location = new Point(36, 104),
            Size = new Size(335, 24)
        };

        var usernameLabel = CreateInputLabel("Username", 148);
        txtUsername.Name = "txtUsername";
        txtUsername.PlaceholderText = "For example, Alex";
        txtUsername.Location = new Point(36, 172);
        txtUsername.Size = new Size(338, 31);
        txtUsername.MaxLength = 40;

        var roomLabel = CreateInputLabel("Room code", 220);
        txtRoomCode.Name = "txtRoomCode";
        txtRoomCode.PlaceholderText = "For example, BGT2026";
        txtRoomCode.Location = new Point(36, 244);
        txtRoomCode.Size = new Size(338, 31);
        txtRoomCode.MaxLength = 30;
        txtRoomCode.CharacterCasing = CharacterCasing.Upper;

        btnJoin.Name = "btnJoin";
        btnJoin.Text = "Join Room";
        btnJoin.Location = new Point(36, 304);
        btnJoin.Size = new Size(338, 46);
        btnJoin.FlatStyle = FlatStyle.Flat;
        btnJoin.FlatAppearance.BorderSize = 0;
        btnJoin.BackColor = Color.FromArgb(21, 94, 239);
        btnJoin.ForeColor = Color.White;
        btnJoin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnJoin.Click += BtnJoin_Click;

        btnOpenWeb.Text = "Open Web App";
        btnOpenWeb.Location = new Point(36, 354);
        btnOpenWeb.Size = new Size(338, 25);
        btnOpenWeb.FlatStyle = FlatStyle.Flat;
        btnOpenWeb.FlatAppearance.BorderSize = 0;
        btnOpenWeb.BackColor = Color.White;
        btnOpenWeb.ForeColor = Color.FromArgb(21, 94, 239);
        btnOpenWeb.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnOpenWeb.Click += BtnOpenWeb_Click;

        AcceptButton = btnJoin;

        card.Controls.AddRange(new Control[]
        {
            schoolLabel,
            titleLabel,
            helpLabel,
            usernameLabel,
            txtUsername,
            roomLabel,
            txtRoomCode,
            btnJoin,
            btnOpenWeb
        });

        Controls.Add(card);
    }

    private static Label CreateInputLabel(string text, int top)
    {
        return new Label
        {
            AutoSize = true,
            Text = text,
            ForeColor = Color.FromArgb(21, 37, 61),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(36, top)
        };
    }

    private void BtnJoin_Click(object? sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string roomCode = txtRoomCode.Text.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(roomCode))
        {
            MessageBox.Show(
                "Please enter both a username and a room code.",
                "Missing information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        using var chatForm = new ChatForm(username, roomCode);
        Hide();
        chatForm.ShowDialog();
        Show();
        Activate();
    }

    private void BtnOpenWeb_Click(object? sender, EventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "http://127.0.0.1:5080",
            UseShellExecute = true
        });
    }
}
