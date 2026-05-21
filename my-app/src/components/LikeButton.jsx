import { useState } from "react";

function LikeButton() {
  const [liked, setLiked] = useState(false);
  const [count, setCount] = useState(0);

  function handleLike() {
    if (liked) {
      setCount(count - 1);
    } else {
      setCount(count + 1);
    }

    setLiked(!liked);
  }

  return (
    <div className="card">
      <h2>Like Button</h2>

      <button onClick={handleLike} className="heart-btn">
        {liked ? "❤️" : "🤍"} {count}
      </button>
    </div>
  );
}

export default LikeButton;