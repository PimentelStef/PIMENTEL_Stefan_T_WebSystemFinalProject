const loginForm = document.getElementById("loginForm");
const message = document.getElementById("message");
const loginButton = document.getElementById("loginButton");

loginForm.addEventListener("submit", async function (event) {
    event.preventDefault();

    message.textContent = "";
    message.className = "mb-3 text-danger";
    loginButton.disabled = true;

    try {
        const response = await fetch(
            "http://localhost:5200/api/auth/login",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    email: document.getElementById("email").value,
                    password: document.getElementById("password").value
                })
            }
        );

        const data = await response.json();

        if (!response.ok) {
            message.textContent =
                data.message || "Login failed. Check your email and password.";
            return;
        }

        const roles = data.roles || [];

        if (!roles.includes("Student") && !roles.includes("Faculty")) {
            message.textContent = "Your account has no supported role.";
            return;
        }

        // Store the token for this browser tab/session.
        sessionStorage.setItem("jwtToken", data.token);
        sessionStorage.setItem("userRoles", JSON.stringify(roles));

        if (roles.includes("Faculty")) {
            window.location.href = "/Home/FacultyDashboard";
        } else {
            window.location.href = "/Home/StudentDashboard";
        }
    } catch (error) {
        message.textContent =
            "Cannot connect to the API. Make sure it is running.";
    } finally {
        loginButton.disabled = false;
    }
});