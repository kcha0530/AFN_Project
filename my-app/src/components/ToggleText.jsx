import { useState } from "react";

function ToggleText() {
  const [show, setShow] = useState(false);

  return (
    <div className="card">
      <h2>Toggle Text</h2>

      <button onClick={() => setShow(!show)}>
        {show ? "Hide Text" : "Show Text"}
      </button>

      {show && <p>Hello! Now you can see me.</p>}
    </div>
  );
}

export default ToggleText;