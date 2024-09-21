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
document.getElementById("searchNewUserInput").addEventListener("input", async function () {
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

        // Render the search results
        const searchResults = document.getElementById('searchResults');
        searchResults.innerHTML = "";  // Clear previous results
        searchResults.style.display = 'flex';

        if (users.length === 0) {
            searchResults.innerHTML = "<p>No users found.</p>";
        } else {
            users.forEach(user => {
                const userDiv = document.createElement('div');
                userDiv.classList.add('user-result');

                userDiv.innerHTML = `
                    <img src="/images/${user.photoName || 'default-user.jpg'}" alt="${user.firstName}">
                    <strong>${user.firstName} ${user.lastName}</strong>
                    <button class="send-request-btn" data-user-id="${user.id}">Send Request</button>
                `;

                // Append each user to the search results
                searchResults.appendChild(userDiv);
            });
        }

        // Add event listeners to the 'Send Request' buttons
        document.querySelectorAll('.send-request-btn').forEach(button => {
            button.addEventListener('click', function () {
                const userId = this.getAttribute('data-user-id');
                sendFriendRequest(userId);
            });
        });

    } catch (error) {
        console.error("Error:", error);
        const searchResults = document.getElementById('searchResults');
        searchResults.innerHTML = "";  // Clear previous results
        searchResults.style.display = 'none';
    }
});

async function sendFriendRequest(userId) {
    try
    {
        const response = await axios.post("/Chat/SendFriendRequest",
             userId ,
            {
                headers: {
                    'Content-Type': 'application/json',
                }
            }
        );

        if (response.data.success) {
            alert('Friend request sent successfully!');
        } else {
            alert('Failed to send friend request.');
        }
    }
    catch(error)
    {
        console.error("Error sending friend request:", error);
        alert("An error occurred while sending the friend request.");
    }
}

// Tabs switching logic
function showTab(tabName) {
    document.getElementById("recentTab").style.display = tabName === 'recent' ? 'block' : 'none';
    document.getElementById("groupsTab").style.display = tabName === 'groups' ? 'block' : 'none';
    document.getElementById("friendsTab").style.display = tabName === 'friends' ? 'block' : 'none';
}

document.getElementById("toggle-friend-requests").addEventListener("click", function () {
    console.log("sfs");
    const friendRequestsContainer = document.getElementById('friend-requests-container');

        if (friendRequestsContainer.style.display === 'none') {
            friendRequestsContainer.style.display = 'block';
            toggleButton.textContent = 'Hide Friend Requests';
        } else {
            friendRequestsContainer.style.display = 'none';
            toggleButton.textContent = 'Show Friend Requests';
        }
});


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