# Chat Application with ASP.NET MVC & SignalR

## 📋 Description
This project is a real-time chat application built with **ASP.NET MVC**, **SignalR**, and **Entity Framework**. It provides functionalities such as **single chat**, **group chat**, **friend requests**, **user status management**, and **real-time messaging**. The app uses **UserManager** for secure access and allows users to send text, photo, and voice messages.

## 🚀 Features
- **User Authentication**: Secure user registration and login with UserManager.
- **Real-time Messaging**: Uses SignalR to enable real-time chat between users and in groups.
- **Friend Requests**: Users can send, accept, or reject friend requests.
- **Group Chats**: Ability to create groups, join/leave groups, and send messages to a group.
- **Message Attachments**: Supports sending text, image, and voice messages.
- **Search**: Allows users to search for friends, groups, and recent chats.

## 🛠️ Technologies Used
- **ASP.NET MVC**: For building the core web application with controllers and views.
- **SignalR**: For real-time messaging and push notifications.
- **Entity Framework**: For database operations (SQL Server or SQLite).
- **JWT Authentication**: For secure user login and API access.
- **UserManager**: ASP.NET Identity’s UserManager class for managing user-related operations (registration, login, password management).
- **HTML, CSS, JavaScript**: For frontend development.

## 📖 Usage

- **User Registration & Login**: Users can register and login with email/password using UserManager. The app uses JWT tokens to authenticate API requests.
- **Real-time Chat**: Use the SignalR-powered chat feature to send real-time messages. Messages will instantly appear for all connected users in the same chat.
- **Friend Management**: Users can send and accept/reject friend requests. Their friend status is updated instantly.
- **Group Chats**: Create new groups, invite friends, and chat within the group.
- **Attachments**: Send images or voice messages using the file upload feature.

## 📦 Dependencies
- **SignalR**: For real-time communication.
- **Entity Framework Core**: For ORM functionality and database management.
- **ASP.NET MVC**: For web application structure and routing.
- **UserManager**: For handling user-related operations like login, registration, password reset, etc.

