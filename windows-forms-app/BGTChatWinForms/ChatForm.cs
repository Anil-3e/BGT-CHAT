using System.Drawing;
using System.Text;

namespace BGTChatWinForms;

public sealed class ChatForm : Form
{
    // These names match the component names required by the project brief.
    private readonly RichTextBox rtbMessages = new();
    private readonly TextBox txtMessage = new();
    private readonly Button btnSend = new();
    private readonly System.Windows.Forms.Timer timerRefresh = new();

    private readonly string username;
    private readonly string roomCode;
    private readonly ChatApiService chatApiService = new();
    private bool isRefreshing;

    public ChatForm(string username, string roomCode)
    {
        this.username = username;
        this.roomCode = roomCode;

        InitializeForm();
        BuildInterface();

        timerRefresh.Interval = 3000;
        timerRefresh.Tick += TimerRefresh_Tick;
        Load += ChatForm_Load;
        FormClosed += ChatForm_FormClosed;
    }

    private void InitializeForm()
    {
        Text = $"BGT Chat - {roomCode}";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(790, 620);
        MinimumSize = new Size(650, 520);
        BackColor = Color.FromArgb(238, 243, 248);
        Font = new Font("Segoe UI", 10F);
    }

    private void BuildInterface()
    {
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 82,
            BackColor = Color.White,
            Padding = new Padding(22, 14, 22, 10)
        };

        var roomLabel = new Label
        {
            AutoSize = true,
            Text = roomCode,
            ForeColor = Color.FromArgb(21, 37, 61),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            Location = new Point(21, 10)
        };

        var userLabel = new Label
        {
            AutoSize = true,
            Text = $"Signed in as {username}",
            ForeColor = Color.FromArgb(102, 117, 138),
            Font = new Font("Segoe UI", 9F),
            Location = new Point(24, 49)
        };

        var refreshLabel = new Label
        {
            AutoSize = true,
            Text = "Refreshes every 3 seconds",
            ForeColor = Color.FromArgb(102, 117, 138),
            Font = new Font("Segoe UI", 9F),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(594, 31)
        };

        headerPanel.Controls.AddRange(new Control[] { roomLabel, userLabel, refreshLabel });

        rtbMessages.Name = "rtbMessages";
        rtbMessages.Dock = DockStyle.Fill;
        rtbMessages.ReadOnly = true;
        rtbMessages.BorderStyle = BorderStyle.None;
        rtbMessages.BackColor = Color.White;
        rtbMessages.ForeColor = Color.FromArgb(36, 52, 77);
        rtbMessages.Font = new Font("Segoe UI", 10.5F);
        rtbMessages.Margin = new Padding(0);
        rtbMessages.DetectUrls = true;

        var composerPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 82,
            BackColor = Color.White,
            Padding = new Padding(18, 16, 18, 16)
        };

        btnSend.Name = "btnSend";
        btnSend.Text = "Send";
        btnSend.Dock = DockStyle.Right;
        btnSend.Width = 104;
        btnSend.FlatStyle = FlatStyle.Flat;
        btnSend.FlatAppearance.BorderSize = 0;
        btnSend.BackColor = Color.FromArgb(21, 94, 239);
        btnSend.ForeColor = Color.White;
        btnSend.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSend.Click += BtnSend_Click;

        txtMessage.Name = "txtMessage";
        txtMessage.Dock = DockStyle.Fill;
        txtMessage.PlaceholderText = "Write a message...";
        txtMessage.MaxLength = 1000;
        txtMessage.Margin = new Padding(0, 0, 12, 0);
        txtMessage.KeyDown += TxtMessage_KeyDown;

        var inputSpacing = new Panel
        {
            Dock = DockStyle.Right,
            Width = 12,
            BackColor = Color.White
        };

        composerPanel.Controls.Add(txtMessage);
        composerPanel.Controls.Add(inputSpacing);
        composerPanel.Controls.Add(btnSend);

        var messageBorder = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(1),
            BackColor = Color.FromArgb(217, 226, 238)
        };
        messageBorder.Controls.Add(rtbMessages);

        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 16, 18, 16),
            BackColor = Color.FromArgb(238, 243, 248)
        };
        contentPanel.Controls.Add(messageBorder);

        Controls.Add(contentPanel);
        Controls.Add(composerPanel);
        Controls.Add(headerPanel);
    }

    private async void ChatForm_Load(object? sender, EventArgs e)
    {
        try
        {
            await chatApiService.JoinRoomAsync(roomCode);
            await RefreshMessagesAsync(showErrors: true);
            timerRefresh.Start();
            txtMessage.Focus();
        }
        catch (Exception ex)
        {
            ShowError(ex);
            Close();
        }
    }

    private async void BtnSend_Click(object? sender, EventArgs e)
    {
        await SendCurrentMessageAsync();
    }

    private async void TxtMessage_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            await SendCurrentMessageAsync();
        }
    }

    private async Task SendCurrentMessageAsync()
    {
        string messageText = txtMessage.Text.Trim();

        if (string.IsNullOrWhiteSpace(messageText))
        {
            MessageBox.Show(
                "Please enter a message before sending.",
                "Empty message",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            txtMessage.Focus();
            return;
        }

        btnSend.Enabled = false;
        txtMessage.Enabled = false;

        try
        {
            await chatApiService.SendMessageAsync(roomCode, username, messageText);
            txtMessage.Clear();
            await RefreshMessagesAsync(showErrors: true);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
        finally
        {
            btnSend.Enabled = true;
            txtMessage.Enabled = true;
            txtMessage.Focus();
        }
    }

    private async void TimerRefresh_Tick(object? sender, EventArgs e)
    {
        await RefreshMessagesAsync(showErrors: false);
    }

    private async Task RefreshMessagesAsync(bool showErrors)
    {
        if (isRefreshing)
        {
            return;
        }

        isRefreshing = true;

        try
        {
            List<MessageModel> messages =
                await chatApiService.LoadMessagesAsync(roomCode);
            DisplayMessages(messages);
        }
        catch (Exception ex)
        {
            if (showErrors)
            {
                ShowError(ex);
            }
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private void DisplayMessages(List<MessageModel> messages)
    {
        var output = new StringBuilder();

        foreach (MessageModel message in messages)
        {
            string time = message.CreatedAt.ToLocalTime().ToString("HH:mm");
            output.AppendLine($"{message.Username}: {message.MessageText} ({time})");
            output.AppendLine();
        }

        rtbMessages.Text = output.Length == 0
            ? "No messages yet. Start the conversation!"
            : output.ToString().TrimEnd();

        rtbMessages.SelectionStart = rtbMessages.TextLength;
        rtbMessages.ScrollToCaret();
    }

    private void ShowError(Exception exception)
    {
        MessageBox.Show(
            exception.Message,
            "BGT Chat error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }

    private void ChatForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        timerRefresh.Stop();
        timerRefresh.Dispose();
        chatApiService.Dispose();
    }
}
