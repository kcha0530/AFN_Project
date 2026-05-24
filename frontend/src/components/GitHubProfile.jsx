import { useEffect, useState } from "react";

function GitHubProfile() {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("https://api.github.com/users/kcha0530")
      .then((res) => {
        if (!res.ok) throw new Error("Failed to fetch GitHub profile");
        return res.json();
      })
      .then((data) => { setUser(data); setLoading(false); })
      .catch(() => { setError("Unable to load GitHub profile."); setLoading(false); });
  }, []);

  return (
    <div className="card">
      <h2>GitHub Profile</h2>
      <p className="card-subtitle">Fetched live from the GitHub API.</p>

      {loading && <p style={{ color: "var(--slate-400)", fontSize: ".9rem" }}>Loading profile…</p>}
      {error   && <div className="alert alert-error">{error}</div>}

      {user && (
        <div className="gh-profile">
          <img className="gh-avatar" src={user.avatar_url} alt={user.login} />
          <p className="gh-name">{user.name || user.login}</p>
          {user.bio && <p className="gh-bio">{user.bio}</p>}
          <div className="gh-meta">
            <span className="gh-badge">👥 {user.followers} followers</span>
            <span className="gh-badge">📦 {user.public_repos} repos</span>
          </div>
        </div>
      )}
    </div>
  );
}

export default GitHubProfile;
