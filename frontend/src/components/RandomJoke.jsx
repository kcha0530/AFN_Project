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
        if (!res.ok) {
          throw new Error("Error fetching joke");
        }
        return res.json();
      })
      .then((data) => {
        setJoke(data);
        setLoading(false);
      })
      .catch(() => {
        setError("Error fetching data");
        setLoading(false);
      });
  }

  return (
    <div className="card">
      <h2>Random Joke Generator</h2>

      <button onClick={getJoke}>Get Joke</button>

      {loading && <p>Loading...</p>}
      {error && <p>{error}</p>}

      {joke && (
        <div>
          <p><strong>Setup:</strong> {joke.setup}</p>
          <p><strong>Punchline:</strong> {joke.punchline}</p>
        </div>
      )}
    </div>
  );
}

export default RandomJoke;