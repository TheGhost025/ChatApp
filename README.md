<!DOCTYPE html>
<html lang="en">
<head>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f4f4f9;
        }
        h1, h2, h3 {
            color: #333;
        }
        h1 {
            text-align: center;
        }
        .badge {
            background-color: #4CAF50;
            color: white;
            padding: 5px 10px;
            border-radius: 3px;
        }
        .section {
            margin-bottom: 20px;
        }
        ul {
            margin-top: 10px;
        }
        .tech-list, .feature-list {
            list-style-type: square;
            margin-left: 20px;
        }
    </style>
</head>
<body>
    <h1>Chat Application with ASP.NET MVC & SignalR</h1>

    <div class="section">
        <h2>📋 Description</h2>
        <p>This project is a real-time chat application built with <strong>ASP.NET MVC</strong>, <strong>SignalR</strong>, and <strong>Entity Framework</strong>. It provides functionalities such as <strong>single chat</strong>, <strong>group chat</strong>, <strong>friend requests</strong>, <strong>user status management</strong>, and <strong>real-time messaging</strong>. The app uses <strong>UserManager</strong> for secure access and allows users to send text, photo, and voice messages.</p>
    </div>

    <div class="section">
        <h2>🚀 Features</h2>
        <ul class="feature-list">
            <li><strong>User Authentication</strong>: Secure user registration and login with UserManager.</li>
            <li><strong>Real-time Messaging</strong>: Uses SignalR to enable real-time chat between users and in groups.</li>
            <li><strong>Friend Requests</strong>: Users can send, accept, or reject friend requests.</li>
            <li><strong>Group Chats</strong>: Ability to create groups, join/leave groups, and send messages to a group.</li>
            <li><strong>Message Attachments</strong>: Supports sending text, image, and voice messages.</li>
            <li><strong>Search</strong>: Allows users to search for friends, groups, and recent chats.</li>
        </ul>
    </div>

    <div class="section">
        <h2>🛠️ Technologies Used</h2>
        <ul class="tech-list">
            <li><strong>ASP.NET MVC</strong>: For building the core web application with controllers and views.</li>
            <li><strong>SignalR</strong>: For real-time messaging and push notifications.</li>
            <li><strong>Entity Framework</strong>: For database operations (SQL Server or SQLite).</li>
            <li><strong>JWT Authentication</strong>: For secure user login and API access.</li>
            <li><strong>UserManager</strong>: ASP.NET Identity’s UserManager class for managing user-related operations (registration, login, password management).</li>
            <li><strong>HTML, CSS, JavaScript</strong>: For frontend development.</li>
        </ul>
    </div>

    <div class="section">
        <h2>📖 Usage</h2>
        <p><strong>User Registration & Login:</strong> Users can register and login with email/password using UserManager. The app uses JWT tokens to authenticate API requests.</p>
        <p><strong>Real-time Chat:</strong> Use the SignalR-powered chat feature to send real-time messages. Messages will instantly appear for all connected users in the same chat.</p>
        <p><strong>Friend Management:</strong> Users can send and accept/reject friend requests. Their friend status is updated instantly.</p>
        <p><strong>Group Chats:</strong> Create new groups, invite friends, and chat within the group.</p>
        <p><strong>Attachments:</strong> Send images or voice messages using the file upload feature.</p>
    </div>

    <div class="section">
        <h2>📦 Dependencies</h2>
        <ul class="tech-list">
            <li><strong>SignalR</strong>: For real-time communication.</li>
            <li><strong>Entity Framework Core</strong>: For ORM functionality and database management.</li>
            <li><strong>ASP.NET MVC</strong>: For web application structure and routing.</li>
            <li><strong>UserManager</strong>: For handling user-related operations like login, registration, password reset, etc.</li>
        </ul>
    </div>
</body>
</html>
