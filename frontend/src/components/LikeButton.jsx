import { useState } from "react";

function LikeButton() {
  const [liked, setLiked] = useState(false);
  const [count, setCount] = useState(0);

  function handleLike() {
    setLiked(prev => {
      setCount(c => prev ? c - 1 : c + 1);
      return !prev;
    });
  }

  return (
    <div className="card">
      <h2>Like Button</h2>
      <p className="card-subtitle">Click to like or unlike. Count updates instantly.</p>
      <button className={`like-btn ${liked ? "liked" : ""}`} onClick={handleLike}>
        <span className="heart">{liked ? "❤️" : "🤍"}</span>
        {count} {count === 1 ? "like" : "likes"}
      </button>
    </div>
  );
}

export default LikeButton;
