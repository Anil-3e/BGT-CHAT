const LOCAL_SERVER_URL = "http://127.0.0.1:5080";
const usesInstalledServer =
    window.location.protocol === "file:"
    || window.location.hostname.endsWith("github.io");
const API_BASE = usesInstalledServer ? `${LOCAL_SERVER_URL}/api` : "/api";

const joinPanel = document.getElementById("joinPanel");
const chatPanel = document.getElementById("chatPanel");
const joinForm = document.getElementById("joinForm");
const messageForm = document.getElementById("messageForm");
const usernameInput = document.getElementById("username");
const roomCodeInput = document.getElementById("roomCode");
const messageInput = document.getElementById("messageInput");
const searchInput = document.getElementById("searchMessages");
const messagesContainer = document.getElementById("messages");
const joinButton = document.getElementById("joinButton");
const sendButton = document.getElementById("sendButton");
const leaveButton = document.getElementById("leaveButton");
const openLocalButton = document.getElementById("openLocalButton");
const joinStatus = document.getElementById("joinStatus");
const chatStatus = document.getElementById("chatStatus");
const activeRoom = document.getElementById("activeRoom");
const activeUser = document.getElementById("activeUser");

let currentUsername = "";
let currentRoomCode = "";
let refreshTimer = null;
let loadedMessages = [];

const roomFromUrl = new URLSearchParams(window.location.search).get("room");
if (roomFromUrl) {
    roomCodeInput.value = roomFromUrl.trim().toUpperCase();
}

if (!usesInstalledServer) {
    openLocalButton.classList.add("hidden");
}

joinForm.addEventListener("submit", joinRoom);
messageForm.addEventListener("submit", sendMessage);
leaveButton.addEventListener("click", leaveRoom);
searchInput.addEventListener("input", renderMessages);
openLocalButton.addEventListener("click", () => {
    window.location.href = LOCAL_SERVER_URL;
});
roomCodeInput.addEventListener("input", () => {
    roomCodeInput.value = roomCodeInput.value.toUpperCase();
});

async function joinRoom(event) {
    event.preventDefault();
    clearStatus(joinStatus);

    const username = usernameInput.value.trim();
    const roomCode = roomCodeInput.value.trim().toUpperCase();

    if (!username || !roomCode) {
        showStatus(joinStatus, "Please enter both a username and a room code.");
        return;
    }

    joinButton.disabled = true;
    joinButton.textContent = "Joining...";

    try {
        await apiRequest("/rooms/join", {
            method: "POST",
            body: JSON.stringify({ room_code: roomCode })
        });

        currentUsername = username;
        currentRoomCode = roomCode;
        activeRoom.textContent = roomCode;
        activeUser.textContent = `Signed in as ${username}`;
        joinPanel.classList.add("hidden");
        chatPanel.classList.remove("hidden");
        updateRoomInUrl(roomCode);

        await loadMessages();
        refreshTimer = window.setInterval(() => loadMessages(true), 3000);
        messageInput.focus();
    } catch (error) {
        showStatus(joinStatus, readableError(error));
    } finally {
        joinButton.disabled = false;
        joinButton.textContent = "Join Room";
    }
}

async function loadMessages(silent = false) {
    if (!currentRoomCode) {
        return;
    }

    messagesContainer.setAttribute("aria-busy", "true");

    try {
        loadedMessages = await apiRequest(
            `/messages?roomCode=${encodeURIComponent(currentRoomCode)}`
        );
        renderMessages();
        clearStatus(chatStatus);
    } catch (error) {
        if (!silent) {
            showStatus(chatStatus, readableError(error));
        }
    } finally {
        messagesContainer.setAttribute("aria-busy", "false");
    }
}

async function sendMessage(event) {
    event.preventDefault();
    clearStatus(chatStatus);

    const messageText = messageInput.value.trim();
    if (!messageText) {
        showStatus(chatStatus, "Please enter a message before sending.");
        messageInput.focus();
        return;
    }

    sendButton.disabled = true;
    sendButton.textContent = "Sending...";

    try {
        await apiRequest("/messages", {
            method: "POST",
            body: JSON.stringify({
                room_code: currentRoomCode,
                username: currentUsername,
                message_text: messageText
            })
        });

        messageInput.value = "";
        await loadMessages();
        messageInput.focus();
    } catch (error) {
        showStatus(chatStatus, readableError(error));
    } finally {
        sendButton.disabled = false;
        sendButton.textContent = "Send";
    }
}

async function apiRequest(path, options = {}) {
    const response = await fetch(`${API_BASE}${path}`, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {})
        }
    });

    if (!response.ok) {
        throw new Error(await getApiError(response));
    }

    if (response.status === 204) {
        return null;
    }

    return response.json();
}

function renderMessages() {
    const keyword = searchInput.value.trim().toLowerCase();
    const filteredMessages = loadedMessages.filter((message) => {
        return message.username.toLowerCase().includes(keyword)
            || message.message_text.toLowerCase().includes(keyword);
    });

    const wasNearBottom =
        messagesContainer.scrollHeight
        - messagesContainer.scrollTop
        - messagesContainer.clientHeight < 80;

    messagesContainer.replaceChildren();

    if (filteredMessages.length === 0) {
        const emptyMessage = document.createElement("p");
        emptyMessage.className = "empty-state";
        emptyMessage.textContent = keyword
            ? "No messages match your search."
            : "No messages yet. Start the conversation!";
        messagesContainer.appendChild(emptyMessage);
        return;
    }

    for (const message of filteredMessages) {
        const messageElement = document.createElement("article");
        messageElement.className = "message";

        if (message.username === currentUsername) {
            messageElement.classList.add("current-user");
        }

        const heading = document.createElement("div");
        heading.className = "message-heading";

        const username = document.createElement("span");
        username.className = "message-username";
        username.textContent = message.username;

        const time = document.createElement("time");
        time.className = "message-time";
        time.dateTime = message.created_at;
        time.textContent = formatTime(message.created_at);

        const text = document.createElement("p");
        text.className = "message-text";
        text.textContent = message.message_text;

        heading.append(username, time);
        messageElement.append(heading, text);
        messagesContainer.appendChild(messageElement);
    }

    if (wasNearBottom) {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }
}

function leaveRoom() {
    window.clearInterval(refreshTimer);
    refreshTimer = null;
    currentUsername = "";
    currentRoomCode = "";
    loadedMessages = [];
    searchInput.value = "";
    messagesContainer.replaceChildren();
    chatPanel.classList.add("hidden");
    joinPanel.classList.remove("hidden");
    updateRoomInUrl(roomCodeInput.value.trim().toUpperCase());
    usernameInput.focus();
}

async function getApiError(response) {
    try {
        const error = await response.json();
        return error.message || `Chat server request failed (${response.status}).`;
    } catch {
        return `Chat server request failed (${response.status}).`;
    }
}

function readableError(error) {
    if (error instanceof TypeError) {
        return "The BGT Chat server is not running. Start the installed BGT Chat app, then try again.";
    }

    return error.message || "Something went wrong.";
}

function formatTime(timestamp) {
    return new Date(timestamp).toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit"
    });
}

function updateRoomInUrl(roomCode) {
    const url = new URL(window.location.href);
    if (roomCode) {
        url.searchParams.set("room", roomCode);
    } else {
        url.searchParams.delete("room");
    }
    window.history.replaceState({}, "", url);
}

function showStatus(element, text, success = false) {
    element.textContent = text;
    element.classList.toggle("success", success);
}

function clearStatus(element) {
    showStatus(element, "");
}

window.addEventListener("beforeunload", () => {
    window.clearInterval(refreshTimer);
});
