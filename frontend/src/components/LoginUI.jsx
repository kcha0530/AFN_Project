import { useState } from "react";

function LoginUI() {
  const [username, setUsername] = useState("");
  const [loggedInUser, setLoggedInUser] = useState("");

  function handleLogin() {
    setLoggedInUser(username);
  }

  return (
    <div className="card">
      <h2>Simple Login UI</h2>

      <input
        type="text"
        placeholder="Enter username"
        value={username}
        onChange={(e) => setUsername(e.target.value)}
      />

      <button onClick={handleLogin}>Login</button>

      {loggedInUser && <h3>Welcome, {loggedInUser}</h3>}
    </div>
  );
}

export default LoginUI;