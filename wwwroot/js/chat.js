function updateUserProfile() {
    // Fetch user profile data from the backend
    fetch('/Account/GetUserProfile')
        .then(response => response.json())
        .then(data => {
            // Update the user name
            document.getElementById('userName').textContent = data.name;
            document.getElementById('userName').dataset.id = data.id;

            // Update the user image, fallback to default if no image is found
            const userImage = document.getElementById('userImage');
            userImage.src = "/images/" + data.profileImageUrl || '/images/default-user.jpg';
        })
        .catch(error => console.error('Error fetching user profile:', error));
}

// Call the function after the user logs in or the page loads
updateUserProfile();

// Function to load old (pending) friend requests on page load
async function loadPendingFriendRequests() {
    await axios.get('/GetPendingFriendRequests')
        .then(response => {
            const pendingRequests = response.data;
            console.log(pendingRequests);
            console.log();
            var x = pendingRequests.count;
            if (x) {
                document.getElementById("numberofPendingFriends").innerText = x;
            }
            pendingRequests.forEach(request => {
                displayFriendRequest(request); // Reuse your displayFriendRequest function
            });
        })
        .catch(error => {
            console.error("Error loading pending friend requests:", error);
        });
}

// Function to display a friend request in the UI (can be reused)
function displayFriendRequest(requestData) {
    const { id, senderId, senderFirstName, senderLastName, senderPhoto, requestDate } = requestData;

    // Create a new friend request item in the UI
    const friendRequestElement = document.createElement("div");
    friendRequestElement.classList.add("friend-request-item");

    // Set default image if PhotoName is not provided
    const senderImageSrc = senderPhoto ? `/images/${senderPhoto}` : '/images/default-user.jpg';

    friendRequestElement.innerHTML = `
        <div class="friend-request-profile">
            <img src="${senderImageSrc}" alt="${senderFirstName} ${senderLastName}" />
        </div>
        <div class="friend-request-info">
            <p>${senderFirstName} ${senderLastName}</p>
            <small>Request sent on ${new Date(requestDate).toLocaleString()}</small>
        </div>
        <div class="friend-request-actions">
            <button onclick="acceptFriendRequest('${id}')">Accept</button>
            <button onclick="rejectFriendRequest('${id}')">Reject</button>
        </div>
    `;

    // Append the friend request to the friend requests container
    document.getElementById("friendRequestsList").appendChild(friendRequestElement);
}

// Call this function when the page loads to fetch and display pending requests
document.addEventListener("DOMContentLoaded", function () {
    loadPendingFriendRequests();
});

// Function to load friend requests
async function loadPendingFriendRequestsSender() {
    await axios.get('/GetPendingFriendRequestsSender')  // Call the API
        .then(response => {
            const pendingRequests = response.data;  // API response

            // Clear existing list before adding new requests
            document.getElementById('friendRequestsListpending').innerHTML = '';

            if (pendingRequests.length === 0) {
                document.getElementById('friendRequestsListpending').innerHTML = "<p>No pending requests.</p>";
            } else {
                // Iterate through each pending request
                pendingRequests.forEach(request => {
                    displayFriendRequestSender(request);  // Call function to display each request
                });
            }
        })
        .catch(error => {
            console.error("Error loading pending friend requests:", error);
        });
}

// Function to display each friend request dynamically
function displayFriendRequestSender(requestData) {
    const { id, senderFirstName, senderLastName, senderPhoto, requestDate } = requestData;

    // Create a new friend request element
    const friendRequestElement = document.createElement('div');
    friendRequestElement.classList.add('friend-request-item');

    const senderImageSrc = senderPhoto ? `/images/${senderPhoto}` : '/images/default-user.jpg';

    // Build the inner HTML for the request
    friendRequestElement.innerHTML = `
        <div class="friend-request-profile">
            <img src="${senderImageSrc}" alt="${senderFirstName} ${senderLastName}">
        </div>
        <div class="friend-request-info">
            <p>${senderFirstName} ${senderLastName}</p>
            <small>Request sent on ${new Date(requestDate).toLocaleString()}</small>
        </div>
        <div class="friend-request-actions">
            <button onclick="rejectFriendRequest('${id}')">Cancel</button>
        </div>
    `;

    // Append the friend request to the friend requests container
    document.getElementById('friendRequestsListpending').appendChild(friendRequestElement);
}

// Call the function to load pending requests on page load
document.addEventListener('DOMContentLoaded', function () {
    loadPendingFriendRequestsSender();  // Load friend requests when the page loads
});


// Search New Users
document.getElementById("searchNewUserInput").addEventListener("input", async function () {
    const searchTerm = document.getElementById("searchNewUserInput").value;

    try {
        const response = await axios.post('/Search/SearchNewUsers',
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
        const response = await axios.post("/FriendRequest/SendFriendRequest",
             userId ,
            {
                headers: {
                    'Content-Type': 'application/json',
                }
            }
        );

        if (response.status == 200) {
            alert('Friend request sent successfully!');
            removenewFriendFromUI(userId);
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


// Function to load friends
async function loadFriends() {
    try {
        const response = await axios.get('/GetFriends');
        const friends = response.data;

        const friendsList = document.getElementById("friendsTab");
        friendsList.innerHTML = ""; // Clear previous content

        friends.forEach(friend => {
            const friendElement = document.createElement("div");
            friendElement.classList.add("friend-item");
            friendElement.dataset.chatId = friend.friendId;
            friendElement.dataset.chatName = friend.friendName;
            friendElement.dataset.chatImage = friend.photoUrl;

            friendElement.innerHTML = `
                <div class="friend-profile">
                    <img src="/images/${friend.photoUrl || 'default-user.jpg'}" alt="${friend.friendName}" />
                    <p>${friend.friendName}</p>
                </div>
            `;
            friendsList.appendChild(friendElement);
        });

        // Add click event listeners for starting chat
        friendsList.addEventListener('click', (event) => {
            if (event.target.closest('.friend-item')) {
                const chatId = event.target.closest('.friend-item').dataset.chatId;
                const name = event.target.closest('.friend-item').dataset.chatName;
                const image = event.target.closest('.friend-item').dataset.chatImage;
                document.getElementById("chat-container").dataset.receiverId = chatId;
                startChat(chatId,name,image);
            }
        });
        }
    catch (errpr) {
            console.error("Error loading friends:", error);
        }
}

// Function to load recent chats 
async function loadRecentChats() {
    try {
        const response = await axios.get('/GetRecentChats'); const recentChats = response.data;

        const recentChatsList = document.getElementById("recentTab");
        recentChatsList.innerHTML = ""; // Clear previous content
        recentChats.forEach(chat => {
            const chatElement = document.createElement("div");
            chatElement.classList.add("chat-item");
            chatElement.dataset.chatId = chat.conversationId;
            friendElement.dataset.chatName = chat.chatPartner;
            friendElement.dataset.chatImage = chat.photoUrl;

            chatElement.innerHTML = `
            <div class="chat-profile">
                <img src="/images/${chat.photoUrl || 'default-chat.jpg'}" alt="${chat.chatPartner}" />
                <p>${chat.chatPartner}</p>
                <small>Last message: ${chat.lastMessage}</small>
            </div>
        `;

            recentChatsList.appendChild(chatElement);
        });

        // Add click event listeners for starting chat
        recentChatsList.addEventListener('click', (event) => {
            if (event.target.closest('.chat-item')) {
                const chatId = event.target.closest('.chat-item').dataset.chatId;
                const name = event.target.closest('.chat-item').dataset.chatName;
                const image = event.target.closest('.chat-item').dataset.chatImage;
                startChat(chatId,name,image);
            }
        });
    } catch (error) {
        console.error("Error loading recent chats:", error);
    }
}


    // Function to load groups 
    async function loadGroups() {
        try {
            const response = await axios.get('/GetGroups'); const groups = response.data;

            const groupsList = document.getElementById("groupsTab");
            groupsList.innerHTML = ""; // Clear previous content

            groups.forEach(group => {
                const groupElement = document.createElement("div");
                groupElement.classList.add("group-item");
                groupElement.dataset.chatId = group.id;

                groupElement.innerHTML = `
            <div class="group-profile">
                <img src="${group.photoUrl || 'default-group.jpg'}" alt="${group.name}" />
                <p>${group.name}</p>
            </div>
        `;

                groupsList.appendChild(groupElement);
            });

            //// Add click event listeners for starting chat
            //groupsList.addEventListener('click', (event) => {
            //    if (event.target.closest('.group-item')) {
            //        const chatId = event.target.closest('.group-item').dataset.chatId;
            //        startChat(chatId);
            //    }
            //});
        } catch (error) {
            console.error("Error loading groups:", error);
        }
    }

// Function to start a chat
async function startChat(targetId,name,image) {
    // You can either navigate to a chat page or open a chat interface/modal here

    const senderId = document.getElementById('userName').dataset.id

    // Update the user name
    document.getElementById('reciverName').textContent = name;

    // Update the user image, fallback to default if no image is found
    const userImage = document.getElementById('reciverImage');
    userImage.src = "/images/" + image || '/images/default-user.jpg';

    console.log("Starting chat with:", targetId);
    try {
        // Make an API call to get the chat history
        const response = await axios.get(`/GetPrivateChat?reciverId=${targetId}`);
        const chatHistory = response.data;

        // Update the chat window with chat history
        const messagesList = document.getElementById("messagesList");
        messagesList.innerHTML = ""; // Clear previous messages

        chatHistory.forEach(message => {
            const messageElement = document.createElement("div");
            messageElement.classList.add("message", message.senderId == senderId ? "sent" :  "received" );
            messageElement.innerHTML = `<p>${message.content}</p>`; // Assuming message has a 'text' property
            messagesList.appendChild(messageElement);
        });

        console.log("Chat started with:", targetId);
    } catch (error) {
        console.error("Error starting chat:", error);
    }
}

document.addEventListener('DOMContentLoaded', function () {
    loadFriends(); 
    loadRecentChats();
    loadGroups();
    });

document.getElementById("toggle-friend-pending-requests").addEventListener("click", function () {
    const friendRequestsContainer = document.getElementById('friend-pending-requests-container');

        if (friendRequestsContainer.style.display === 'none') {
            friendRequestsContainer.style.display = 'block';
            document.getElementById("toggle-friend-pending-requests").textContent = 'Hide Friend Requests';
        } else {
            friendRequestsContainer.style.display = 'none';
            document.getElementById("toggle-friend-pending-requests").textContent = 'Show Friend Requests';
        }
});

document.getElementById("toggle-friend-requests").addEventListener("click", function () {
    const friendRequestsContainer = document.getElementById('friend-requests-container');
    const button = document.getElementById("toggle-friend-requests");
    const icon = document.getElementById("toggle-icon");

    if (friendRequestsContainer.style.display === 'none') {
        friendRequestsContainer.style.display = 'block';
        button.style.backgroundColor = '#0056b3'; // Change color to blue (for example)
    } else {
        friendRequestsContainer.style.display = 'none';
        button.style.backgroundColor = '#007bff';    // Reset color
    }
});

// Show the group creation modal when the "Create Group" button is clicked
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('createGroupButton').addEventListener('click', function () {
        fetchFriendsForGroup(); // Fetch friends dynamically when opening the modal
        document.getElementById('createGroupModal').style.display = 'block'; // Show modal
    });

    // Fetch friends and populate the modal with the member list
    function fetchFriendsForGroup() {
        const friendsListForGroup = document.getElementById('friendsListForGroup');
        const adminSelect = document.getElementById('adminSelect');

        // Clear previous list of friends and admin options
        friendsListForGroup.innerHTML = '';
        adminSelect.innerHTML = ''; // Reset admin options

        axios.get('/GetFriends')
            .then(response => {
                const friends = response.data;

                if (friends.length === 0) {
                    friendsListForGroup.innerHTML = '<p>No friends available to add to the group.</p>';
                    return;
                }

                // Populate the friends list
                friends.forEach(friend => {
                    // Create a div to hold friend info (image, checkbox, and label)
                    const friendItem = document.createElement('div');
                    friendItem.classList.add('friend-item'); // You can style this with CSS

                    // Create an image element for the friend's profile picture
                    const friendImage = document.createElement('img');
                    friendImage.src = `/images/${friend.photoUrl}`; // Assuming the image URL follows this structure
                    friendImage.alt = `${friend.friendName}'s profile picture`;
                    friendImage.classList.add('friend-image');
                    friendImage.style.width = '40px'; // Set image size
                    friendImage.style.height = '40px';
                    friendImage.style.borderRadius = '50%'; // Make it a circular image

                    // Create a checkbox for selecting the friend
                    const checkbox = document.createElement('input');
                    checkbox.type = 'checkbox';
                    checkbox.id = `friend_${friend.friendId}`;
                    checkbox.value = friend.friendId;
                    checkbox.classList.add('member-checkbox'); // For event listener

                    // Create a label for the friend's name
                    const label = document.createElement('label');
                    label.htmlFor = checkbox.id;
                    label.textContent = friend.friendName;

                    // Append the image, checkbox, and label to the friend item
                    friendItem.appendChild(friendImage);
                    friendItem.appendChild(checkbox);
                    friendItem.appendChild(label);

                    // Append the friend item to the friends list
                    friendsListForGroup.appendChild(friendItem);
                });

                // Add event listener to update admin select when members are chosen
                document.querySelectorAll('.member-checkbox').forEach(checkbox => {
                    checkbox.addEventListener('change', updateAdminSelect);
                });
            })
            .catch(error => {
                console.error('Error fetching friends:', error);
                friendsListForGroup.innerHTML = '<p>Error loading friends.</p>';
            });
    }

    // Update the admin select dropdown based on selected members
    function updateAdminSelect() {
        const adminSelect = document.getElementById('adminSelect');
        const selectedMembers = [];

        // Get all selected members
        document.querySelectorAll('.member-checkbox:checked').forEach(checkbox => {
            const friendId = checkbox.value;
            const friendName = checkbox.nextElementSibling.textContent; // Get the label text (friend's name)
            selectedMembers.push({ friendId, friendName });
        });

        // Clear the admin select dropdown
        adminSelect.innerHTML = '';

        // Populate the admin select dropdown with selected members
        selectedMembers.forEach(member => {
            const adminOption = document.createElement('option');
            adminOption.value = member.friendId;
            adminOption.textContent = member.friendName;
            adminSelect.appendChild(adminOption);
        });
    }

    // Close the modal when the "Cancel" button is clicked
    document.getElementById('cancelGroupButton').addEventListener('click', function () {
        document.getElementById('createGroupModal').style.display = 'none'; // Hide modal
        clearGroupForm(); // Clear form data
    });

    // Handle group creation
    document.getElementById('createGroupSubmitButton').addEventListener('click', function () {
        const groupName = document.getElementById('groupNameInput').value.trim();

        if (!groupName) {
            alert('Please enter a group name.');
            return;
        }

        // Get selected members
        const selectedMembers = [];
        document.querySelectorAll('#friendsListForGroup input[type="checkbox"]:checked').forEach(checkbox => {
            selectedMembers.push(checkbox.value);
        });

        if (selectedMembers.length === 0) {
            alert('Please select at least one member.');
            return;
        }

        // Get selected admins from selected members
        const selectedAdmins = Array.from(document.getElementById('adminSelect').selectedOptions).map(option => option.value);

        if (selectedAdmins.length === 0) {
            alert('Please select at least one admin.');
            return;
        }

        // Get the group image file
        const groupImage = document.getElementById('groupImageInput').files[0];

        if (!groupImage) {
            alert('Please upload a group image.');
            return;
        }

        // Get user ID (assumed to be stored in a hidden element with user info)
        const userId = document.getElementById('userName').dataset.id;

        // Create FormData object for sending image and other data
        const formData = new FormData();
        formData.append('groupName', groupName);
        selectedAdmins.push(userId);
        selectedMembers.push(userId);
        // Append each member individually to ensure they are sent as an array
        selectedMembers.forEach(member => formData.append("Members", member));

        // Append each admin individually
        selectedAdmins.forEach(admin => formData.append("Admins", admin));

        formData.append('creatorId', userId); // The logged-in user's ID
        formData.append('groupImage', groupImage); // Attach the group image file

        // Send the API request to create the group
        axios.post('/api/groups/create', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        })
            .then(response => {
                console.log('Group created successfully:', response.data);
                document.getElementById('createGroupModal').style.display = 'none'; // Hide modal after success
                clearGroupForm(); // Clear form data
            })
            .catch(error => {
                console.error('Error creating group:', error);
                alert('Failed to create the group. Please try again.');
            });
    });

    // Clear the group form after submission or when closing the modal
    function clearGroupForm() {
        document.getElementById('groupNameInput').value = '';
        document.getElementById('groupImageInput').value = '';
        document.getElementById('friendsListForGroup').innerHTML = '';
        document.getElementById('adminSelect').innerHTML = ''; // Reset admin options
    }
});




// SignalR connection setup
const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

// Start the connection
connection.start().then(function () {
    console.log("SignalR connection established");

    // Send message on button click
    document.getElementById("sendButton").addEventListener("click", function () {
        const messageContent = document.getElementById("messageInput").value;
        const receiverId = document.getElementById("chat-container").dataset.receiverId; // Assign receiver ID here

        // Call the SendMessage method on the backend
        connection.invoke("SendMessage", receiverId, messageContent, 0 /* messageType */, null)
            .catch(function (err) {
                return console.error(err.toString());
            });

        // Clear the input field
        document.getElementById("messageInput").value = "";

        const messagesList = document.getElementById("messagesList");

        // Check if messagesList exists
        if (!messagesList) {
            console.error("Element with ID 'messagesList' not found.");
            return;
        }

        // Create a new message element
        const messageDiv = document.createElement("div");

        messageDiv.classList.add('message', 'sent');
        messageDiv.innerHTML = `<p>${messageContent}</p>`;

        console.log("Appending message to messagesList:", messageDiv);
        messagesList.appendChild(messageDiv);

        // Auto-scroll to the bottom
        messagesList.scrollTop = messagesList.scrollHeight;
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

// When a new group is created
connection.on("GroupCreated", function (groupName, admin, members) {
    console.log(`Group '${groupName}' created with admin: ${admin}`);
    // Add group to the UI (e.g., in the Groups tab)
    const groupsTab = document.getElementById('groupsTab');
    const groupItem = document.createElement('div');
    groupItem.textContent = groupName;
    groupsTab.appendChild(groupItem);
});

connection.on("ReceiveMessage", function (message) {
    const messagesList = document.getElementById("messagesList");

    // Check if messagesList exists
    if (!messagesList) {
        console.error("Element with ID 'messagesList' not found.");
        return;
    }

    console.log("Received message:", message);

    // Get the current user's ID for comparison
    const currentUserId = document.getElementById('userName').dataset.id;
    console.log("Current User ID:", currentUserId);
    console.log("Message Sender ID:", message.senderId);

    if (!currentUserId) {
        console.error("Current user ID not found.");
        return;
    }

    // Create a new message element
    const messageDiv = document.createElement("div");
    const messageClass = message.senderId === currentUserId ? 'sent' : 'received';
    messageDiv.classList.add('message', messageClass);
    messageDiv.innerHTML = `<p>${message.content}</p>`;

    console.log("Appending message to messagesList:", messageDiv);
    messagesList.appendChild(messageDiv);

    // Auto-scroll to the bottom
    messagesList.scrollTop = messagesList.scrollHeight;
});


// Handle receiving friend request
connection.on("ReceiveFriendRequest", function (requestData) {
    console.log("Friend request received:", requestData); // Log request data for debugging

    // Destructure the request data (ensure these fields exist in requestData)
    const { friendRequestId, senderFirstName, senderLastName, senderImage, requestDate, senderId } = requestData;

    // Validate that the required fields are not undefined or null
    if (!senderFirstName || !senderLastName || !senderId) {
        console.error("Invalid friend request data:", requestData);
        return; // Stop if data is invalid
    }


    const num = parseInt(document.getElementById("numberofPendingFriends").innerText);

    num += 1;

    document.getElementById("numberofPendingFriends").innerText = num;

    // Create a new friend request item in the UI
    const friendRequestElement = document.createElement("div");
    friendRequestElement.classList.add("friend-request-item");

    // Set default image if SenderImage is not provided
    const senderImageSrc = '/images/' + senderImage || '/images/default-user.jpg';

    friendRequestElement.innerHTML = `
        <div class="friend-request-profile">
            <img src="${senderImageSrc}" alt="${senderFirstName} ${senderLastName}" />
        </div>
        <div class="friend-request-info">
            <p>${senderFirstName} ${senderLastName}</p>
            <small>Request sent on ${new Date(requestDate).toLocaleString()}</small>
        </div>
        <div class="friend-request-actions">
            <button onclick="acceptFriendRequest('${friendRequestId}')">Accept</button>
            <button onclick="rejectFriendRequest('${friendRequestId}')">Reject</button>
        </div>
    `;

    // Append the friend request to the friend requests container
    document.getElementById("friendRequestsList").appendChild(friendRequestElement);

    alert(`New friend request from ${senderFirstName} ${senderLastName}!`);
});

// Function to handle accepting the friend request
function acceptFriendRequest(requestId) {
    axios.post('/FriendRequest/AcceptFriendRequest', requestId,
        {
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => {
            alert("Friend request accepted:", response.data);
            // Optionally, remove the request from the UI or refresh the list
            removeFriendRequestFromUI(requestId);
        })
        .catch(error => {
            alert("Error accepting friend request:", error);
        });
}

// Function to handle rejecting the friend request
function rejectFriendRequest(requestId) {
    axios.post('/FriendRequest/DeclineFriendRequest', requestId,
        {
            headers: {
                'Content-Type': 'application/json',
            }
        })
        .then(response => {
            alert("Friend request declined:", response.data);
            // Optionally, remove the request from the UI or refresh the list
            removeFriendRequestFromUI(requestId);
        })
        .catch(error => {
            alert("Error declining friend request:", error);
        });
}

function removeFriendRequestFromUI(senderId) {
    // Remove the friend request element from the UI
    const requestElement = document.querySelector(`button[onclick="acceptFriendRequest('${senderId}')"]`).closest('.friend-request-item');
    if (requestElement) {
        requestElement.remove();
    }
}

function removenewFriendFromUI(userId) {
    // Remove the user from the search results
    const button = document.querySelector(`.send-request-btn[data-user-id='${userId}']`);
    if (button) {
        const userDiv = button.closest('.user-result');
        if (userDiv) {
            userDiv.remove(); // Remove the user element from the list
        }
    }
}