import { useState } from "react";

function Counter() {
  const [count, setCount] = useState(0);

  return (
    <div className="card">
      <h2>Counter</h2>
      <p className="card-subtitle">Increment, decrement, or reset.</p>
      <div className="count-display">{count}</div>
      <div className="btn-group">
        <button className="btn-sm btn-green" onClick={() => setCount(c => c + 1)}>+ Increase</button>
        <button className="btn-sm btn-ghost" onClick={() => setCount(c => c - 1)}>− Decrease</button>
        <button className="btn-sm btn-danger" onClick={() => setCount(0)}>Reset</button>
      </div>
    </div>
  );
}

export default Counter;
