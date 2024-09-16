function updateUserProfile() {
    // Fetch user profile data from the backend
    fetch('/Account/GetUserProfile')
        .then(response => response.json())
        .then(data => {
            // Update the user name
            document.getElementById('userName').textContent = data.name;

            // Update the user image, fallback to default if no image is found
            const userImage = document.getElementById('userImage');
            userImage.src = "/images/" + data.profileImageUrl || '/images/default-user.jpg';
        })
        .catch(error => console.error('Error fetching user profile:', error));
}

// Call the function after the user logs in or the page loads
updateUserProfile();

// Search New Users
document.getElementById("searchNewUserButton").addEventListener("click", async function () {
    const searchTerm = document.getElementById("searchNewUserInput").value;

    try {
        const response = await axios.post('/Chat/SearchNewUsers',
            searchTerm ,
            {
                headers: {
                    'Content-Type': 'application/json',
                }
            }
        );

        const users = response.data;

        console.log(users);

        //// Render the search results
        //const searchResults = document.getElementById('searchResults');
        //searchResults.innerHTML = "";  // Clear previous results

        //if (users.length === 0) {
        //    searchResults.innerHTML = "<p>No users found.</p>";
        //} else {
        //    users.forEach(user => {
        //        const userDiv = document.createElement('div');
        //        userDiv.classList.add('user-result');
        //        userDiv.innerHTML = `<strong>${user.firstName} ${user.lastName}</strong>`;
        //        searchResults.appendChild(userDiv);
        //    });
        //}

    } catch (error) {
        console.error("Error:", error);
        alert("An error occurred while searching for users.");
    }
});

// Tabs switching logic
function showTab(tabName) {
    document.getElementById("recentTab").style.display = tabName === 'recent' ? 'block' : 'none';
    document.getElementById("groupsTab").style.display = tabName === 'groups' ? 'block' : 'none';
    document.getElementById("friendsTab").style.display = tabName === 'friends' ? 'block' : 'none';
}

// SignalR connection setup
const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (message) {
    const messagesList = document.getElementById("messagesList");

    // Create a new message element
    const messageDiv = document.createElement("div");
    messageDiv.classList.add(message.senderId === '@User.Identity.Name' ? 'message sent' : 'message received');
    messageDiv.innerHTML = `<p>${message.content}</p>`;

    messagesList.appendChild(messageDiv);
});

// Start the connection
connection.start().then(function () {
    console.log("SignalR connection established");

    // Send message on button click
    document.getElementById("sendButton").addEventListener("click", function () {
        const messageContent = document.getElementById("messageInput").value;
        const receiverId = "some-receiver-id"; // Assign receiver ID here

        // Call the SendMessage method on the backend
        connection.invoke("SendMessage", receiverId, messageContent, 0 /* messageType */, null)
            .catch(function (err) {
                return console.error(err.toString());
            });

        // Clear the input field
        document.getElementById("messageInput").value = "";
    });

    // Search within Friends/Groups
    document.getElementById("searchFriendsButton").addEventListener("click", function () {
        const searchQuery = document.getElementById("searchFriendsInput").value;

        // Call the backend to search within friends or groups
        connection.invoke("SearchFriendsGroups", searchQuery)
            .then(function (results) {
                console.log("Search results:", results);
                // Handle displaying search results from friends and groups
            })
            .catch(function (err) {
                console.error("Error searching within friends/groups:", err);
            });
    });
}).catch(function (err) {
    console.error("Error establishing SignalR connection: " + err);
});