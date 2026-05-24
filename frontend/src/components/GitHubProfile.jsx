import { useEffect, useState } from "react";

function GitHubProfile() {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("https://api.github.com/users/kcha0530")
      .then((res) => {
        if (!res.ok) {
          throw new Error("Error fetching data");
        }
        return res.json();
      })
      .then((data) => {
        setUser(data);
        setLoading(false);
      })
      .catch(() => {
        setError("Error fetching data");
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <div className="card">Loading...</div>;
  }

  if (error) {
    return <div className="card">{error}</div>;
  }

  return (
    <div className="card">
      <h2>GitHub Profile</h2>

      <img src={user.avatar_url} alt={user.name} width="120" />

      <h3>{user.name}</h3>
      <p>{user.bio}</p>
      <p>Followers: {user.followers}</p>
    </div>
  );
}

export default GitHubProfile;