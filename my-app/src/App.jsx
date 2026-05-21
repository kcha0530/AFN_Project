import Header from "./components/Header.jsx";
import Footer from "./components/Footer.jsx";
import Profile from "./components/Profile.jsx";
import Card from "./components/Card.jsx";

import Counter from "./components/Counter.jsx";
import ToggleText from "./components/ToggleText.jsx";
import ItemList from "./components/ItemList.jsx";
import LoginUI from "./components/LoginUI.jsx";
import LikeButton from "./components/LikeButton.jsx";

import GitHubProfile from "./components/GitHubProfile.jsx";
import RandomJoke from "./components/RandomJoke.jsx";
import BackendProducts from "./components/BackendProducts.jsx";

function App() {
  return (
    <div className="app">
      <section>
        <Header />
        <Profile />
        <Card />
      </section>

      <section>
        <h1></h1>
        <Counter />
        <ToggleText />
        <ItemList />
        <LoginUI />
        <LikeButton />
      </section>

      <section>
        <GitHubProfile />
        <RandomJoke />
        <BackendProducts />
      </section>

      <Footer />
    </div>
  );
}

export default App;

// function Header() {
//   return <h1>Welcome to My React App</h1>;
// }

// function Profile() {
//   return (
//     <div className="profile">
//       <h2>My Profile</h2>
//       <img src="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRVA_HrQLjkHiJ2Ag5RGuwbFeDKRLfldnDasw&s" alt="Profile" />
//       <p>Name: Krit Chaiyabud</p>
//       <p>Bio: This is my first React Profile.</p>
//     </div>
//   );
// }

// function Card() {
//   return (
//     <div className="card">
//       <h2>React Card</h2>
//       <p>
//         This is a simple card component with a title, description, and button.
//       </p>
//       <button>Read More</button>
//     </div>
//   );
// }

// function Footer() {
//   return <p>© 2026 All Rights Reserved</p>;
// }

// function App() {
//   return (
//     <div className="app">
//       <Header />
//       <Profile />
//       <Card />
//       <Footer />
//     </div>
//   );
// }

// export default App;
