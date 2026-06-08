const SUPABASE_URL = "YOUR_SUPABASE_URL";
const SUPABASE_KEY = "YOUR_SUPABASE_ANON_PUBLIC_KEY";
const SETTINGS_STORAGE_KEY = "bgt-chat-supabase-settings";

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
const joinStatus = document.getElementById("joinStatus");
const chatStatus = document.getElementById("chatStatus");
const activeRoom = document.getElementById("activeRoom");
const activeUser = document.getElementById("activeUser");
const setupButton = document.getElementById("setupButton");
const setupDialog = document.getElementById("setupDialog");
const setupForm = document.getElementById("setupForm");
const setupUrlInput = document.getElementById("setupUrl");
const setupKeyInput = document.getElementById("setupKey");
const setupStatus = document.getElementById("setupStatus");
const cancelSetupButton = document.getElementById("cancelSetupButton");

let currentUsername = "";
let currentRoomCode = "";
let refreshTimer = null;
let loadedMessages = [];

// A room in the URL, such as ?room=BGT2026, is entered automatically.
const roomFromUrl = new URLSearchParams(window.location.search).get("room");
if (roomFromUrl) {
    roomCodeInput.value = roomFromUrl.trim().toUpperCase();
}

joinForm.addEventListener("submit", joinRoom);
messageForm.addEventListener("submit", sendMessage);
leaveButton.addEventListener("click", leaveRoom);
searchInput.addEventListener("input", renderMessages);
setupButton.addEventListener("click", openSetupDialog);
setupForm.addEventListener("submit", saveSupabaseSettings);
cancelSetupButton.addEventListener("click", () => setupDialog.close());
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

    if (!supabaseIsConfigured()) {
        showStatus(joinStatus, "Set up the Supabase connection first.");
        openSetupDialog();
        return;
    }

    joinButton.disabled = true;
    joinButton.textContent = "Joining...";

    try {
        const roomExists = await checkRoomExists(roomCode);
        if (!roomExists) {
            showStatus(joinStatus, `Room "${roomCode}" does not exist.`);
            return;
        }

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

async function checkRoomExists(roomCode) {
    const endpoint = `${apiBase()}/rooms?room_code=eq.${encodeURIComponent(roomCode)}&select=room_code&limit=1`;
    const response = await fetch(endpoint, {
        method: "GET",
        headers: requestHeaders()
    });

    if (!response.ok) {
        throw new Error(await getSupabaseError(response));
    }

    const rooms = await response.json();
    return rooms.length > 0;
}

async function loadMessages(silent = false) {
    if (!currentRoomCode) {
        return;
    }

    messagesContainer.setAttribute("aria-busy", "true");

    try {
        const endpoint =
            `${apiBase()}/messages` +
            `?room_code=eq.${encodeURIComponent(currentRoomCode)}` +
            "&select=id,room_code,username,message_text,created_at" +
            "&order=created_at.asc";

        const response = await fetch(endpoint, {
            method: "GET",
            headers: requestHeaders()
        });

        if (!response.ok) {
            throw new Error(await getSupabaseError(response));
        }

        loadedMessages = await response.json();
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
        const response = await fetch(`${apiBase()}/messages`, {
            method: "POST",
            headers: {
                ...requestHeaders(),
                "Content-Type": "application/json",
                "Prefer": "return=minimal"
            },
            body: JSON.stringify({
                room_code: currentRoomCode,
                username: currentUsername,
                message_text: messageText
            })
        });

        if (!response.ok) {
            throw new Error(await getSupabaseError(response));
        }

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

function renderMessages() {
    const keyword = searchInput.value.trim().toLowerCase();
    const filteredMessages = loadedMessages.filter((message) => {
        return message.username.toLowerCase().includes(keyword)
            || message.message_text.toLowerCase().includes(keyword);
    });

    const wasNearBottom =
        messagesContainer.scrollHeight - messagesContainer.scrollTop - messagesContainer.clientHeight < 80;

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

function requestHeaders() {
    const settings = getSupabaseSettings();
    return {
        "apikey": settings.key,
        "Authorization": `Bearer ${settings.key}`
    };
}

function apiBase() {
    return `${getSupabaseSettings().url.replace(/\/$/, "")}/rest/v1`;
}

function supabaseIsConfigured() {
    const settings = getSupabaseSettings();
    return settings.url.startsWith("https://")
        && !settings.url.includes("YOUR_SUPABASE")
        && settings.key.length > 20
        && !settings.key.includes("YOUR_SUPABASE");
}

function getSupabaseSettings() {
    try {
        const savedSettings = JSON.parse(localStorage.getItem(SETTINGS_STORAGE_KEY));
        if (savedSettings?.url && savedSettings?.key) {
            return savedSettings;
        }
    } catch {
        localStorage.removeItem(SETTINGS_STORAGE_KEY);
    }

    return {
        url: SUPABASE_URL,
        key: SUPABASE_KEY
    };
}

function openSetupDialog() {
    const settings = getSupabaseSettings();
    setupUrlInput.value = settings.url.includes("YOUR_SUPABASE") ? "" : settings.url;
    setupKeyInput.value = settings.key.includes("YOUR_SUPABASE") ? "" : settings.key;
    clearStatus(setupStatus);
    setupDialog.showModal();
    setupUrlInput.focus();
}

function saveSupabaseSettings(event) {
    event.preventDefault();

    const url = setupUrlInput.value.trim().replace(/\/$/, "");
    const key = setupKeyInput.value.trim();

    if (!url.startsWith("https://") || !url.includes(".supabase.co")) {
        showStatus(setupStatus, "Enter a valid Supabase Project URL.");
        return;
    }

    if (key.length < 20) {
        showStatus(setupStatus, "Enter the complete anon/public key.");
        return;
    }

    localStorage.setItem(SETTINGS_STORAGE_KEY, JSON.stringify({ url, key }));
    setupDialog.close();
    clearStatus(joinStatus);
    showStatus(joinStatus, "Supabase connection saved. You can join a room now.", true);
}

async function getSupabaseError(response) {
    try {
        const error = await response.json();
        return error.message || error.hint || `Supabase request failed (${response.status}).`;
    } catch {
        return `Supabase request failed (${response.status}).`;
    }
}

function readableError(error) {
    if (error instanceof TypeError) {
        return "Could not connect to Supabase. Check the URL, key, and internet connection.";
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
