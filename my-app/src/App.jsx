import Header from "./components/Header.jsx";
import Footer from "./components/Footer.jsx";
import LoginPage from "./components/LoginPage.jsx";
import BackendProducts from "./components/BackendProducts.jsx";
import GitHubProfile from "./components/GitHubProfile.jsx";
import RandomJoke from "./components/RandomJoke.jsx";

function App() {
  return (
    <div className="app">
      <Header />

      <section className="hero">
        <div>
          <p className="eyebrow">Secure Web App</p>
          <h1>React + ASP.NET Core with JWT Auth</h1>
          <p className="lead">
            Modern UI, clean structure, and secure API access using JWT, CORS, and rate limiting.
          </p>
        </div>
      </section>

      <section className="grid two-column" id="login">
        <LoginPage />
        <BackendProducts />
      </section>

      <section className="grid two-column">
        <GitHubProfile />
        <RandomJoke />
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
