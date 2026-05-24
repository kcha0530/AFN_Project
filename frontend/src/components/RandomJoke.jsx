import { useState } from "react";

function RandomJoke() {
  const [joke, setJoke] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  function getJoke() {
    setLoading(true);
    setError("");
    fetch("https://official-joke-api.appspot.com/random_joke")
      .then((res) => {
        if (!res.ok) throw new Error("Failed to fetch joke");
        return res.json();
      })
      .then((data) => { setJoke(data); setLoading(false); })
      .catch(() => { setError("Unable to load joke. Try again."); setLoading(false); });
  }

  return (
    <div className="card">
      <h2>Random Joke Generator</h2>
      <p className="card-subtitle">A random joke fetched from an external API.</p>
      <button onClick={getJoke} disabled={loading}>
        {loading ? <><span className="spinner" />Fetching…</> : "Get Joke 😄"}
      </button>

      {error && <div className="alert alert-error">{error}</div>}

      {joke && (
        <div className="joke-box">
          <p className="joke-setup">"{joke.setup}"</p>
          <p className="joke-punchline">👉 {joke.punchline}</p>
        </div>
      )}
    </div>
  );
}

export default RandomJoke;
