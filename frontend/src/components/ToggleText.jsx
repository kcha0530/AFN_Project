import { useState } from "react";

function ToggleText() {
  const [show, setShow] = useState(false);

  return (
    <div className="card">
      <h2>Toggle Text</h2>
      <p className="card-subtitle">Show or hide content with a button click.</p>
      <button onClick={() => setShow(s => !s)}>
        {show ? "Hide Text" : "Show Text"}
      </button>
      {show && (
        <div className="toggle-text-box">
          Hello! Now you can see me. This text is conditionally rendered using React state.
        </div>
      )}
    </div>
  );
}

export default ToggleText;
